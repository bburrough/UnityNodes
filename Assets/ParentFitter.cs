﻿using UnityEngine;
using System.Collections;

public class ParentFitter : MonoBehaviour {


    RectTransform rt;
    Renderer r;

    void Awake()
    {
        rt = transform.parent.GetComponent<RectTransform>();
        if (rt == null)
            r = transform.parent.GetComponent<Renderer>();
    }


	// Use this for initialization
	void Start () {
        UpdateScale();
    }
	

	void Update ()
    {
        if (rt != null && rt.hasChanged)
        {
            UpdateScale();
            rt.hasChanged = false;
        }
        else if (r != null && r.transform.hasChanged)
        {
            UpdateScale();
            r.transform.hasChanged = false;
        }
        //else if(transform.parent.transform.hasChanged)
        //    UpdateScale();
    }

    private void UpdateScale()
    {
        if (rt != null)
        {
            Vector3 new_position;
            new_position.x = rt.rect.center.x;
            new_position.y = rt.rect.center.y;
            new_position.z = 0.25f;

            transform.localPosition = new_position;

            Vector3 new_scale;
            new_scale.x = rt.rect.width;
            new_scale.y = rt.rect.height;
            new_scale.z = 1.0f;
            transform.localScale = new_scale;
        }
        else if (r != null)
        {
            Vector3 new_pos;
            new_pos.x = 0.0f;
            new_pos.y = 0.0f;
            new_pos.z = 0.25f;

            transform.localPosition = new_pos;

            Vector3 new_scale = r.bounds.size;
            new_scale.z = 1.0f;
            
            transform.localScale = new_scale;
        }

        //Renderer r = transform.parent.GetComponent<Renderer>();
        //if (r != null)
        //{
        //    Debug.Log("here");
        //    Vector3 new_scale = r.bounds.size;
        //    new_scale.z = 1.0f;
        //    transform.localScale = new_scale;
        //}
        //else
        //{
        //    Debug.Log("there");
        //}
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
