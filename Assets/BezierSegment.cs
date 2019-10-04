using UnityEngine;
using System.Collections;

namespace Shapes
{
	/// <summary>
	/// A spline segment representing the shape between 2 BezierPoints.
	/// </summary>
	public class BezierSegment:System.Object
	{
		/// <summary>
		/// If true this segment will be sampled linearly between its 2 points.
		/// </summary>
		public bool linear = false;

		/// <summary>
		/// Allows you to specify custom tangent distances with customLocalTangentDistance0 and customLocalTangentDistance1.
		/// </summary>
		public bool useCustomDistance = false;

		/// <summary>
		/// The spline that owns this segment.
		/// </summary>
		public BezierSpline spline = null;

		/// <summary>
		/// The index of this segment in its spline.
		/// </summary>
		public int index = -1;
		

		/// <summary>
		/// The first BezierPoint of this segment.
		/// </summary>
		public BezierPoint point0
		{
			get
			{
				return spline.GetPoint(index);
			}
		}


		/// <summary>
		/// The second BezierPoint of this segment.
		/// </summary>
		public BezierPoint point1
		{
			get
			{
				return spline.GetPoint((index+1)%spline.pointCount);
			}
		}


		/// <summary>
		/// The normal of point0.
		/// </summary>
		public Vector3 localNormal0
		{
			get
			{
				return spline.GetPoint(index).normal;
			}
			set
			{
				spline.GetPoint(index).normal = value;
			}
		}


		/// <summary>
		/// The normal of point1.
		/// </summary>
		public Vector3 localNormal1
		{
			get
			{
				return spline.GetPoint((index+1)%spline.pointCount).normal;
			}
			set
			{
				spline.GetPoint((index+1)%spline.pointCount).normal = value;
			}
		}


		/// <summary>
		/// The position of point0.
		/// </summary>
		public Vector3 localPosition0
		{
			get
			{
				return spline.GetPoint(index).position;
			}
			set
			{
				spline.GetPoint(index).position = value;
			}
		}


		/// <summary>
		/// The position of point1.
		/// </summary>
		public Vector3 localPosition1
		{
			get
			{
				return spline.GetPoint((index+1)%spline.pointCount).position;
			}
			set
			{
				spline.GetPoint((index+1)%spline.pointCount).position = value;
			}
		}


		/// <summary>
		/// the in tangent of this segment. (faces forwards towards point1).
		/// </summary>
		public Vector3 localTangent0
		{
			get
			{
				if (linear)
				{
					return (localPosition1-localPosition0).normalized;
				}
				return spline.GetPoint(index).outTangent;
			}
			set
			{
				spline.GetPoint(index).outTangent = value;
			}
		}


		/// <summary>
		/// the out tangent of this segment. (faces backwards towards point0).
		/// </summary>
		public Vector3 localTangent1
		{
			get
			{
				if (linear)
				{
					return (localPosition0-localPosition1).normalized;
				}
				return spline.GetPoint((index+1)%spline.pointCount).inTangent;
			}
			set
			{
				spline.GetPoint((index+1)%spline.pointCount).inTangent = value;
			}
		}


		public float customLocalTangentDistance0 = 1.0f/3.0f;
		public float customLocalTangentDistance1 = 1.0f/3.0f;


		/// <summary>
		/// The normalized distance of localTangent0 along this segment.
		/// If useCustomDistance is set this will return customLocalTangentDistance0
		/// otherwise it will return 1/3.
		/// </summary>
		public float localTangentDistance0
		{
			get
			{
				if (useCustomDistance && !linear)
				{
					return customLocalTangentDistance0;
				}
				return 1.0f/3.0f;
			}
		}


		/// <summary>
		/// The normalized distance of localTangent1 along this segment.
		/// If useCustomDistance is set this will return customLocalTangentDistance1
		/// otherwise it will return 1/3.
		/// </summary>
		public float localTangentDistance1
		{
			get
			{
				if (useCustomDistance && !linear)
				{
					return customLocalTangentDistance1;
				}
				return 1.0f/3.0f;
			}
		}


		/// <summary>
		/// The position of localTangent0. Useful for drawing a tangent handle.
		/// </summary>
		public Vector3 localTangetPos0
		{
			get
			{
				float pointDist = Vector3.Distance(localPosition0, localPosition1);
				return localPosition0+localTangent0*localTangentDistance0*pointDist;
			}
			set
			{
				localTangent0 = (value-localPosition0).normalized;
			}
		}


		/// <summary>
		/// The position of localTangent1. Useful for drawing a tangent handle.
		/// </summary>
		public Vector3 localTangetPos1
		{
			get
			{
				float pointDist = Vector3.Distance(localPosition0, localPosition1);
				return localPosition1+localTangent1*localTangentDistance1*pointDist;
			}
			set
			{
				localTangent1 = (value-localPosition1).normalized;
			}
		}

		/// <summary>
		/// Create a new bezier segment.
		/// </summary>
		/// <param name="spline">The spline that owns this segment.</param>
		/// <param name="index">The intex of this segment in the spline.</param>
		public BezierSegment(BezierSpline spline, int index)
		{
			this.spline = spline;
			this.index = index;
		}

		/// <summary>
		/// Make both tangetns linear
		/// </summary>
		public void SetLinearTangents()
		{
			SetLinearTangents0();
			SetLinearTangents1();
		}

		/// <summary>
		/// Make localTangent0 linear
		/// </summary>
		public void SetLinearTangents0()
		{
			localTangent0 = (localPosition1-localPosition0).normalized;
		}


		/// <summary>
		/// Make localTangent1 linear
		/// </summary>
		public void SetLinearTangents1()
		{
			localTangent1 = (localPosition0-localPosition1).normalized;
		}


		/// <summary>
		/// Get the tangent at a position along this segment. t should be 0-1.
		/// </summary>
		public Vector3 GetLocalTangent(float t)
		{
			//Theres probaby a much better way of doing this...
			Vector3 pPos = GetLocalPosition(t+0.01f);
			Vector3 pNeg = GetLocalPosition(t-0.01f);

			Vector3 forward = (pPos-pNeg).normalized;

			return forward;
		}


		/// <summary>
		/// Get the normal at a position along this segment. t should be 0-1.
		/// </summary>
		public Vector3 GetLocalNormal(float t)
		{
			Vector3 up = Vector3.Slerp(localNormal0, localNormal1, t);
			return up;
		}


		/// <summary>
		/// Get the position at a position along this segment. t should be 0-1.
		/// </summary>
		public Vector3 GetLocalPosition(float t)
		{
			Vector3 localPoint0 = this.localPosition0;
			Vector3 localPoint1 = this.localPosition1;
			Vector3 localNormal0 = this.localTangent0;
			Vector3 localNormal1 = this.localTangent1;

			Vector3 a, b, c, p = new Vector3(0, 0, 0);

			float pointDist = Vector3.Distance(localPoint0, localPoint1);
			float p0ActualDist = localTangentDistance0*pointDist;
			float p1ActualDist = localTangentDistance1*pointDist;

			Vector3 n0 = localNormal0;
			Vector3 n1 = localNormal1;
			if (linear)
			{
				n0 = (localPoint1-localPoint0).normalized;
				n1 = (localPoint0-localPoint1).normalized;
			}

			Vector3 p0 = localPoint0;
			Vector3 p1 = localPoint0+n0*p0ActualDist;
			Vector3 p2 = localPoint1+n1*p1ActualDist;
			Vector3 p3 = localPoint1;

			c.x = 3.0f * (p1.x - p0.x);
			c.y = 3.0f * (p1.y - p0.y);
			c.z = 3.0f * (p1.z - p0.z);
			b.x = 3.0f * (p2.x - p1.x) - c.x;
			b.y = 3.0f * (p2.y - p1.y) - c.y;
			b.z = 3.0f * (p2.z - p1.z) - c.z;
			a.x = p3.x - p0.x - c.x - b.x;
			a.y = p3.y - p0.y - c.y - b.y;
			a.z = p3.z - p0.z - c.z - b.z;

			p.x = a.x * t * t * t + b.x * t * t + c.x * t + p0.x;
			p.y = a.y * t * t * t + b.y * t * t + c.y * t + p0.y;
			p.z = a.z * t * t * t + b.z * t * t + c.z * t + p0.z;

			return p;
		}

		/// <summary>
		/// Get the closest position the curve point to this segment.
		/// </summary>
		/// <param name="point">The target point.</param>
		/// <param name="initialIterations">How many iterations.</param>
		/// <param name="distance">The resulting distance from the target point to the closest point.</param>
		/// <returns>The position that can be sampled to get the closest position.</returns>
		public float GetClosestPoint(Vector3 point, int initialIterations, out float distance)
		{
			float pointPos = 0.0f;
			float bestDistance = Mathf.Infinity;
			float bestPoint = pointPos;
			
			for (int i = 0; i < initialIterations; i++)
			{
				float ip = (float)i/(initialIterations-1);
				Vector3 p = GetLocalPosition(ip);
				float dist = Vector3.Distance(point, p);
				
				if (dist < bestDistance)
				{
					bestDistance = dist;
					bestPoint = ip;
				}
			}

			pointPos = bestPoint;

			distance = bestDistance;

			return pointPos;
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
