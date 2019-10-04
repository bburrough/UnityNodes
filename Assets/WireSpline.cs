using UnityEngine;
using System.Collections;
using Shapes;

public class WireSpline : MonoBehaviour, ISerializationCallbackReceiver {

    private Vector3 _connectorLeftPosition;
    public Vector3 connectorLeftPosition
    {
        get
        {
            return _connectorLeftPosition;
        }
        set
        {
            _connectorLeftPosition = value;
            leftPoint.position = value;
            DrawTube();
        }
    }

    private Vector3 _connectorRightPosition;
    public Vector3 connectorRightPosition
    {
        get
        {
            return _connectorRightPosition;
        }
        set
        {
            _connectorRightPosition = value;
            rightPoint.position = value;
            DrawTube();
        }
    }


    public SourceSocket left;
    public SinkSocket right;

    private BezierPoint leftPoint;
    private BezierPoint rightPoint;

    private BezierSpline spline;


    void Awake()
    {
        CreateSplines();
    }


    private void CreateSplines()
    {
        leftPoint = new BezierPoint(connectorLeftPosition, Vector3.right);
        rightPoint = new BezierPoint(connectorRightPosition, Vector3.right);
        spline = new BezierSpline(leftPoint, rightPoint);
    }
     

    public void OnAfterDeserialize()
    {
        CreateSplines();
    }


    public void OnBeforeSerialize()
    {
    }


    void Start ()
    {
        DrawTube();
    }


    void OnDrawGizmos()
    {        
        //leftPoint.position = connectorLeft.position;
        //rightPoint.position = connectorRight.position;
        if(spline != null) 
            spline.DrawGizmos();
    }
 

    public GameObject prefab;
    private GameObject trgo = null;
    private TubeRenderer tr = null;
    public void DrawTube()
    {
        if (trgo == null)
        {
            trgo = Instantiate(prefab) as GameObject;
            trgo.transform.SetParent(transform, false);
            tr = trgo.GetComponent<TubeRenderer>();
        }

        tr.Reset();

        for (int i = 0; i < spline.segmentCount; i++)
        {
            BezierSegment segment = spline.GetSegment(i);

            Vector3 position0 = segment.localPosition0;

            position0 = new Vector3(position0.x, -position0.z, position0.y);
            //Vector3 position1 = segment.localPosition1;
            //float dist = Vector3.Distance(position0, position1);

            Vector3 previousPoint = position0;
            TubeVertex tv = new TubeVertex(previousPoint, 1.5f, Color.white);
            tr.AppendPoint(tv);

            for (int j = 1; j <= 40; j++)
            {
                float nPoint = (float)j / 40;
                Vector3 point = segment.GetLocalPosition(nPoint);

                Vector3 altPoint = new Vector3(point.x, -point.z, point.y);

                tv = new TubeVertex(altPoint, 1.5f, Color.white);


                tr.AppendPoint(tv);

                previousPoint = altPoint;
            }
        }
        tr.Rebuild();
    }


    public void OnDestroy()
    {
        DestroyImmediate(trgo);
    }


    public void Disconnect()
    {
        left.Disconnect(this);
        right.Disconnect();
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
