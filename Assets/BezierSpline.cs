using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shapes
{
	/// <summary>
	/// A data structure to store length information about BezierSegments. Used by BezierSpline.GenerateSegmentLengthCache().
	/// </summary>
	public struct BezierSegmentLengthInfo
	{
		public float start;
		public float end;
		public float length;
		public int index;
	}

	/// <summary>
	/// A spline object.
	/// </summary>
	public class BezierSpline:System.Object
	{
		private List<BezierPoint> points = new List<BezierPoint>();
		private List<BezierSegment> segments = new List<BezierSegment>();

		private Matrix4x4 _matrix = Matrix4x4.identity;
		private Matrix4x4 _inverseMatrix = Matrix4x4.identity;
		/// <summary>
		/// The matrix to transform samples by this spline.
		/// </summary>
		public Matrix4x4 matrix
		{
			get
			{
				return _matrix;
			}
			set
			{
				matrixIsIdentity = value == Matrix4x4.identity;
				_matrix = value;
				_inverseMatrix = value.inverse;
			}
		}


		/// <summary>
		/// The inverse matrix.
		/// </summary>
		public Matrix4x4 inverseMatrix
		{
			get
			{
				return _inverseMatrix;
			}
		}


		private bool _matrixIsIdentity = true;
		/// <summary>
		/// Is the matrix an identity matrix? Used for optimization.
		/// </summary>
		public bool matrixIsIdentity
		{
			get
			{
				return _matrixIsIdentity;
			}
			private set
			{
				_matrixIsIdentity = value;
			}
		}

		private bool _isClosed = false;
		/// <summary>
		/// If set to true there will be a span between the last and first points. Default is false.
		/// </summary>
		public bool isClosed
		{
			get
			{
				return _isClosed;
			}
			set
			{
				if (_isClosed == value) return;

				if (_isClosed)
				{
					segments.RemoveAt(segments.Count-1);
				}
				else
				{
					segments.Add(new BezierSegment(this, segments.Count));
				}

				_isClosed = value;
			}
		}


		public int pointCount
		{
			get
			{
				return points.Count;
			}
		}


		public int segmentCount
		{
			get
			{
				return segments.Count;
			}
		}


		/// <summary>
		/// Create a new spline. The spline will be initialized with 2 points and 1 segment.
		/// A spline must always have at least 2 points and 1 segment. (2 segments if its a closed spline).
		/// </summary>
		public BezierSpline()
		{
			//Add 2 default points and 1 segment.
			
            BezierPoint p = new BezierPoint(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
			points.Add(p);
			p = new BezierPoint(new Vector3(1, 0, 0), new Vector3(1, 0, 0));
			points.Add(p);
			BezierSegment newSegment = new BezierSegment(this, 0);
			segments.Add(newSegment);            
		}


        public BezierSpline(BezierPoint left, BezierPoint right)
        {
            points.Add(left);
            points.Add(right);
            segments.Add(new BezierSegment(this, 0));
        }

		/// <summary>
		/// Add a point to the spline with default position and tangent.
		/// </summary>
		/// <returns>The new point.</returns>
		public BezierPoint AddPoint()
		{
			return AddPoint(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
		}

		/// <summary>
		/// Add a point to the spline.
		/// </summary>
		/// <returns>The new point.</returns>
		public BezierPoint AddPoint(Vector3 position, Vector3 outTangent)
		{
			BezierPoint p = new BezierPoint(position, outTangent);
			points.Add(p);

            if (points.Count < 2)
                return p;

			BezierSegment newSegment = new BezierSegment(this, segments.Count);

			if (isClosed)
			{
				//If its a closed spline then make sure the last closing segment stays at the end.
				segments.Insert(segmentCount-1, newSegment);
			}
			else
			{
				segments.Add(newSegment);
			}
			return p;
		}

		
		/// <summary>
		/// Insert a point into the spline with default position and tangent.
		/// </summary>
		/// <param name="index"></param>
		/// <returns>The new point.</returns>
		public BezierPoint InsertPoint(int index)
		{
			return InsertPoint(index, new Vector3(0, 0, 0), new Vector3(1, 0, 0));
		}


		/// <summary>
		/// Insert a point into the spline.
		/// </summary>
		/// <returns>The new point.</returns>
		public BezierPoint InsertPoint(int index, Vector3 position, Vector3 outTangent)
		{
			if (index == pointCount)
			{
				return AddPoint(position, outTangent);
			}

			BezierPoint p = new BezierPoint(position, outTangent);
			points.Insert(index, p);

            if (points.Count < 2)
                return p;

			BezierSegment newSegment = new BezierSegment(this, segments.Count);
			segments.Insert(index, newSegment);

			for (int i = index+1; i < segments.Count; i++)
			{
				segments[i].index++;
			}

			return p;
		}


		/// <summary>
		/// Remove a point from this spline.
		/// </summary>
		public void RemovePointAt(int index)
		{
			if (index < 0 || index >= pointCount)
			{
				throw new System.IndexOutOfRangeException();
			}

			if (pointCount <= 2)
			{
				throw new System.Exception("Cannot remove point because a spline must contain at least 2 points and 1 segment.");
			}

			points.RemoveAt(index);

			if (!isClosed && index == segments.Count)
			{
				segments.RemoveAt(index-1);
			}
			else
			{
				segments.RemoveAt(index);
			}

			for (int i = index; i < segments.Count; i++)
			{
				segments[i].index--;
			}
		}


		/// <summary>
		/// Get a point from this spline.
		/// </summary>
		public BezierPoint GetPoint(int index)
		{
			return points[index];
		}


		/// <summary>
		/// Get a segment from this spline.
		/// </summary>
		public BezierSegment GetSegment(int index)
		{
			return segments[index];
		}


		/// <summary>
		/// Generate a list of segment length infos. Used by ConstantPositionToSamplePosition().
		/// </summary>
		/// <returns></returns>
		public List<BezierSegmentLengthInfo> GenerateSegmentLengthCache()
		{
			List<BezierSegmentLengthInfo> infos = new List<BezierSegmentLengthInfo>(segmentCount);

			float totalLength = 0;
			Vector3 lastEndPos = GetSegment(0).localPosition0;

			for (int i = 0; i < segmentCount; i++)
			{
				BezierSegment segment = GetSegment(i);
				BezierSegmentLengthInfo info = new BezierSegmentLengthInfo();

				Vector3 pos = segment.localPosition1;
				float dist = Vector3.Distance(lastEndPos, pos);
				
				info.start = totalLength;
				totalLength += dist;
				info.end = totalLength;
				info.length = dist;
				info.index = i;

				infos.Add(info);

				lastEndPos = pos;
			}

			for (int i = 0; i < infos.Count; i++)
			{
				BezierSegmentLengthInfo info = infos[i];
				
				info.start /= totalLength;
				info.end /= totalLength;
				info.length /= totalLength;

				infos[i] = info;
			}

			return infos;
		}


		private int BinarySearch<T>(IList<T> list, System.Func<T, int> comparer)
		{
			int min = 0;
			int max = list.Count-1;

			while (min <= max)
			{
				int mid = (min + max) / 2;
				int comparison = comparer(list[mid]);
				if (comparison == 0)
				{
					return mid;
				}
				if (comparison < 0)
				{
					min = mid+1;
				}
				else
				{
					max = mid-1;
				}
			}
			return ~min;
		}


		/// <summary>
		/// Create a BezierPoint representing the sample information at a normalized position on the spline.
		/// </summary>
		public BezierPoint SamplePoint(float position)
		{
			position = Mathf.Repeat(position, 1);

			int segmentIndex = Mathf.Min(Mathf.FloorToInt(position*segmentCount), pointCount-1);
			BezierSegment segment = GetSegment(segmentIndex);
			float segmentPosition = Mathf.Clamp01(position*segmentCount-segmentIndex);

			Vector3 pos = TransformPoint(segment.GetLocalPosition(segmentPosition));
			Vector3 tangent = TransformDirection(segment.GetLocalTangent(segmentPosition));

			BezierPoint point = new BezierPoint(pos, tangent);

			if (segment.point0.hasNormal || segment.point1.hasNormal)
			{
				point.normal = TransformDirection(segment.GetLocalNormal(segmentPosition));
			}

			return point;
		}


		/// <summary>
		/// Get the position at a normalized position on the spline.
		/// </summary>
		public Vector3 SamplePosition(float position)
		{
			position = Mathf.Repeat(position, 1);
			
			int segmentIndex = Mathf.Min(Mathf.FloorToInt(position*segmentCount), pointCount-1);
			BezierSegment segment = GetSegment(segmentIndex);
			float segmentPosition = Mathf.Clamp01(position*segmentCount-segmentIndex);

			Vector3 point = TransformPoint(segment.GetLocalPosition(segmentPosition));

			return point;
		}


		/// <summary>
		/// Get the tangent at a normalized position on the spline.
		/// </summary>
		public Vector3 SampleTangent(float position)
		{
			position = Mathf.Repeat(position, 1);

			int segmentIndex = Mathf.Min(Mathf.FloorToInt(position*segmentCount), pointCount-1);
			BezierSegment segment = GetSegment(segmentIndex);
			float segmentPosition = Mathf.Clamp01(position*segmentCount-segmentIndex);

			Vector3 normal = TransformDirection(segment.GetLocalTangent(segmentPosition));

			return normal;
		}


		/// <summary>
		/// Get the normal at a normalized position on the spline.
		/// </summary>
		public Vector3 SampleNormal(float position)
		{
			position = Mathf.Repeat(position, 1);

			int segmentIndex = Mathf.Min(Mathf.FloorToInt(position*segmentCount), pointCount-1);
			BezierSegment segment = GetSegment(segmentIndex);
			float segmentPosition = Mathf.Clamp01(position*segmentCount-segmentIndex);

			Vector3 normal = TransformDirection(segment.GetLocalNormal(segmentPosition));

			return normal;
		}


		/// <summary>
		/// Get the total length of the spline.
		/// </summary>
		public float GetLength()
		{
			float length = 0;
			for (int i = 0; i < segmentCount; i++)
			{
				BezierSegment segment = GetSegment(i);
				length += Vector3.Distance(TransformPoint(segment.localPosition0), TransformPoint(segment.localPosition1));
			}
			return length;
		}


		private Vector3 TransformPoint(Vector3 point)
		{
			if (matrixIsIdentity)
			{
				return point;
			}
			return matrix.MultiplyPoint(point);
		}


		private Vector3 TransformDirection(Vector3 direction)
		{
			if (matrixIsIdentity)
			{
				return direction;
			}
			return matrix.MultiplyVector(direction);
		}


		private Vector3 InverseTransformPoint(Vector3 point)
		{
			if (matrixIsIdentity)
			{
				return point;
			}
			return inverseMatrix.MultiplyPoint(point);
		}


		private Vector3 InverseTransformDirection(Vector3 direction)
		{
			if (matrixIsIdentity)
			{
				return direction;
			}
			return inverseMatrix.MultiplyVector(direction);
		}


		/// <summary>
		/// Get the closest position along the spline to a target point.
		/// </summary>
		public float GetClosestPoint(Vector3 point, int iterations)
		{
			point = InverseTransformPoint(point);
			
			float bestDistance = Mathf.Infinity;
			float bestPoint = 0;
			int bestSegment = -1;

			for (int i = 0; i < segmentCount; i++)
			{
				BezierSegment segment = GetSegment(i);
				float dist;
				float p = segment.GetClosestPoint(point, iterations, out dist);
				if (dist < bestDistance)
				{
					bestDistance = dist;
					bestPoint = p;
					bestSegment = i;
				}
			}

			float lengthFactor = 1.0f/segmentCount;

			float splinePoint = ((float)bestSegment/segmentCount)+bestPoint*lengthFactor;

			return splinePoint;
		}


		/// <summary>
		/// Convert a "constant velocity" position along this spline to a position that can be used to sample at that position.
		/// Useful for sampling uniformly along the spline.
		/// </summary>
		public float ConstantPositionToSamplePosition(float position, List<BezierSegmentLengthInfo> lengthCache)
		{
			position = Mathf.Repeat(position, 1);

			int index = BinarySearch<BezierSegmentLengthInfo>(lengthCache, delegate(BezierSegmentLengthInfo v)
			{
				int i = 0;
				if (position >= v.end)
				{
					i = -1;
				}
				else if (position < v.start)
				{
					i = 1;
				}
				return i;
			});

			float stepSize = 1.0f/lengthCache.Count;

			BezierSegmentLengthInfo info = lengthCache[index];

			float segmentPosition = (position-info.start)/info.length;

			float output = Mathf.Lerp(stepSize*index, stepSize*index+stepSize, segmentPosition);
			
			return output;
		}


		public void DrawGizmos()
		{
			Color originalColor = Gizmos.color;

			for (int i = 0; i < segments.Count; i++)
			{
				BezierSegment segment = segments[i];

				Vector3 position0 = TransformPoint(segment.localPosition0);
				Vector3 position1 = TransformPoint(segment.localPosition1);
				float dist = Vector3.Distance(position0, position1);

				Vector3 previousPoint = position0;
				for (int j = 1; j <= 20; j++)
				{
					float nPoint = (float)j/20;
					Color color = Color.white;
					if (j%2 == 1)
					{
						color = Color.black;
					}
					Vector3 point = TransformPoint(segment.GetLocalPosition(nPoint));
					
					Gizmos.color = color;
					Gizmos.DrawLine(previousPoint, point);

					if (j%5 == 0)
					{
						Vector3 normal = TransformDirection(segment.GetLocalNormal(nPoint));
						Gizmos.color = new Color(1, 0.7f, 0.3f, 1);
						Gizmos.DrawLine(point, point+normal*dist*0.1f);
					}

					previousPoint = point;
				}

				Gizmos.color = new Color(1, 0, 0, 0.75f);
				Gizmos.DrawLine(position0, segment.localTangetPos0);
				Gizmos.DrawLine(position1, segment.localTangetPos1);
			}

			Gizmos.color = originalColor;
		}
    }
}

/*
------------------------------------------------------------------------------
This software is available under 2 licenses -- choose whichever you prefer.
------------------------------------------------------------------------------
ALTERNATIVE A - MIT License
Copyright (c) 2003-2019 Bobby G. Burrough
Permission is hereby granted, free of charge, to any person obtaining a copy of 
this software and associated documentation files (the "Software"), to deal in 
the Software without restriction, including without limitation the rights to 
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE.
------------------------------------------------------------------------------
ALTERNATIVE B - Public Domain (www.unlicense.org)
This is free and unencumbered software released into the public domain.
Anyone is free to copy, modify, publish, use, compile, sell, or distribute this 
software, either in source code form or as a compiled binary, for any purpose, 
commercial or non-commercial, and by any means.
In jurisdictions that recognize copyright laws, the author or authors of this 
software dedicate any and all copyright interest in the software to the public 
domain. We make this dedication for the benefit of the public at large and to 
the detriment of our heirs and successors. We intend this dedication to be an 
overt act of relinquishment in perpetuity of all present and future rights to 
this software under copyright law.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
------------------------------------------------------------------------------
*/
