using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shapes
{
	/// <summary>
	/// A point or knot within a spline.
	/// </summary>
	public class BezierPoint:System.Object
	{
		/// <summary>
		/// The position of this point.
		/// </summary>
		public Vector3 position;

		/// <summary>
		/// If this is true inTangent will return -outTangent.
		/// Default is true, unless using the constructor that takes an inTangent parameter.
		/// </summary>
		public bool useSingleTangent = true;

		/// <summary>
		/// The normal of this point. Useful for defining a spline with an explicit rotation.
		/// </summary>
		public Vector3 normal;


		public bool hasNormal
		{
			get
			{
				return normal != Vector3.zero;
			}
		}


		private Vector3 _inTangent;
		/// <summary>
		/// The in tangent to this point. If useSignleTangent is true then it will return -outTangent.
		/// </summary>
		public Vector3 inTangent
		{
			get
			{
				if (useSingleTangent)
				{
					return -outTangent;
				}
				return _inTangent;
			}
			set
			{
				if (useSingleTangent)
				{
					_outTangent = -value;
				}
				else
				{
					_inTangent = value;
				}
			}
		}


		private Vector3 _outTangent;
		/// <summary>
		/// The out tangent of this point.
		/// </summary>
		public Vector3 outTangent
		{
			get
			{
				return _outTangent;
			}
			set
			{
				_outTangent = value;
			}
		}


		/// <summary>
		/// Create a new BezierPoint with a position and out tangent.
		/// </summary>
		public BezierPoint(Vector3 position, Vector3 outTangent)
		{
			this.position = position;
			this.outTangent = outTangent;
		}


		/// <summary>
		/// Create a new BezierPoint with a position, out tangent, and in tangent. useSingleTangent will be set to false.
		/// </summary>
		public BezierPoint(Vector3 position, Vector3 inTangent, Vector3 outTangent)
		{
			this.position = position;
			this.inTangent = inTangent;
			this.outTangent = outTangent;
			useSingleTangent = false;
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
