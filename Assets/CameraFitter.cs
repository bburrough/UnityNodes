using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraFitter : MonoBehaviour {

    private float _targetOrthograhicSize;
    public float targetOrthograhicSize
    {
        get
        {
            return _targetOrthograhicSize;
        }
        set
        {
            _targetOrthograhicSize = value;
            holderObject.transform.position = new Vector3(_targetOrthograhicSize, 0.0f, 0.0f);
        }
    }

    private GameObject holderObject;

    public int orthographicSizeMin = 40;
    public int orthographicSizeMax = 2400;

    public CanvasScaler canvasScaler;
    private float initialOrthographicSize;

    private bool middleMouseButtonIsPressed = false;
    private Vector3 initialMousePosition;
    private Vector3 initialCameraPosition;
    void Awake()
    {
        Camera myCamera = GetComponent<Camera>();
        initialOrthographicSize = Screen.height / 2.0f;
        myCamera.orthographicSize = initialOrthographicSize;
        _targetOrthograhicSize = initialOrthographicSize;
        holderObject = new GameObject();
        holderObject.name = "Camera Fitter Placeholder";
        holderObject.transform.position = new Vector3(_targetOrthograhicSize, 0.0f, 0.0f);
    }


    void Update () {
        bool scrollEventOccurred = false;

        // Camera zoom functionality
        const float decreaseProportion = 0.85f;
        const float increaseProportion = 1.15f;

        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            scrollEventOccurred = true;
            //targetOrthograhicSize -= 20.0f;
            //canvasScaler.dynamicPixelsPerUnit *= increaseProportion;
            _targetOrthograhicSize *= decreaseProportion;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0) // back
        {
            scrollEventOccurred = true;
            //targetOrthograhicSize += 20.0f;
            //canvasScaler.dynamicPixelsPerUnit *= decreaseProportion;
            _targetOrthograhicSize *= increaseProportion;
        }
        if(scrollEventOccurred)
        {
            _targetOrthograhicSize = Mathf.Clamp(_targetOrthograhicSize, orthographicSizeMin, orthographicSizeMax);
            LeanTween.cancel(holderObject);
            LeanTween.moveX(holderObject, _targetOrthograhicSize, 0.2f).setEase(LeanTweenType.easeOutCubic);
        }
        Camera.main.orthographicSize = holderObject.transform.position.x;

        // Camera pan functionality
        if (Input.GetMouseButtonDown(2))
        {
            middleMouseButtonIsPressed = true;

            initialMousePosition = Input.mousePosition; // screen space dimensions
            initialCameraPosition = transform.position; // world space dimensions
        }
        if (Input.GetMouseButtonUp(2))
        {
            middleMouseButtonIsPressed = false;
        }
        if (middleMouseButtonIsPressed)
        {
            Vector3 mouseDelta = initialMousePosition - Input.mousePosition; // screen space dimensions
            Vector3 newCameraPosition;
            newCameraPosition = initialCameraPosition + mouseDelta * (Camera.main.orthographicSize / initialOrthographicSize);
            newCameraPosition.z = initialCameraPosition.z;
            transform.position = newCameraPosition;
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
