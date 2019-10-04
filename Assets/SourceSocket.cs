using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public enum SourceSinkType
{
    Integer,
    FloatingPoint,
    Text,
    Generic
}

public class SourceSocket : MonoBehaviour {

    public List<WireSpline> wires;

    public GameObject wirePrefab;
    GameObject currentWireSplineGo;
    WireSpline currentWireSpline;

    public SourceSinkType type = SourceSinkType.Integer;

    public Node node;

    void Awake()
    {
        wires = new List<WireSpline>();
    }


	void Start () {
	
	}
	

    public void UpdateWires()
    {
        for (int i = 0; i < wires.Count; i++)
        {
            wires[i].connectorLeftPosition = transform.position;
        }
    }


    public void CreateWireFirstHalf()
    {
        currentWireSplineGo = Instantiate(wirePrefab) as GameObject;
        currentWireSpline = currentWireSplineGo.GetComponent<WireSpline>();
        currentWireSplineGo.transform.SetParent(node.transform.parent);
        currentWireSpline.connectorLeftPosition = transform.position;
        currentWireSpline.connectorRightPosition = transform.position + new Vector3(0.0f, 0.0f, -8.0f);
    }

    public void CreateWireSecondHalf(SinkSocket sink)
    {
        currentWireSpline.left = this;
        if (sink.ConnectWire(currentWireSpline, type))
        {
            wires.Add(currentWireSpline);

            sink.OnConnect();
            node.OnConnect(this, currentWireSpline);
            node.uiController.SetDirty();

            currentWireSplineGo = null;
            currentWireSpline = null;
        }
    }

    public void CreateNewWireTo(SinkSocket sink)
    {
        CreateWireFirstHalf();
        CreateWireSecondHalf(sink);
    }


    public void OnDrag()
    {
        Vector3 mouseWorldPosition;
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 newPosition = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, -8.0f);
        currentWireSpline.connectorRightPosition = newPosition;
    }


    public void BeginDrag()
    {
        CreateWireFirstHalf();
    }


    public void EndDrag(BaseEventData bed)
    {
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll((PointerEventData)bed, hits);
        for(int i = 0; i < hits.Count; i++)
        {
            /*
                if (hits[i].gameObject == gameObject) // don't allow a node to attach to itself
                continue;

                // Actually, let's allow this.  I need a general solution for recursive connections.
            */

            SinkSocket sink = hits[i].gameObject.GetComponent<SinkSocket>();
            if (sink)
            {
                CreateWireSecondHalf(sink);
                break;
            }
        }

        if(currentWireSplineGo)
            DestroyImmediate(currentWireSplineGo);

        currentWireSplineGo = null;
        currentWireSpline = null;
    }


    public void SetValue(Node writer, int x)
    {
        for(int i = 0; i < wires.Count; i++)
        {
            wires[i].right.SetValue(x, writer);
        }
    }


    public void SetValue(Node writer, float x)
    {
        for (int i = 0; i < wires.Count; i++)
        {
            wires[i].right.SetValue(x, writer);
        }
    }

    public void SetValue(Node writer, string x)
    {
        for (int i = 0; i < wires.Count; i++)
        {
            wires[i].right.SetValue(x, writer);
        }
    }


    public void SetValue(Node writer, object x, WireSpline ws = null)
    {
        if (wires.Count < 1 && ws == null)
            return;
        bool wrote = false;
        for (int i = 0; i < wires.Count; i++)
        {
            if (ws == null || ws == wires[i]) // write to the wire if either a wire wasn't specified, or if the wire is the one specified.
            {
                wires[i].right.SetValue(x, writer);
                wrote = true;
            }
        }
        if (!wrote)
            throw new System.ArgumentException("Source Socket doesn't contain the specified wire.");
    }


    /*
    public int GetIntegerValue(Node destination)
    {
        return node.GetIntegerValue(this, destination);
    }


    public float GetDecimalValue(Node destination)
    {
        return node.GetDecimalValue(this, destination);
    }


    public string GetStringValue(Node destination)
    {
        return node.GetStringValue(this, destination);
    }


    public object GetValue(Node destination)
    {
        return node.GetValue(this, destination);
    }
    */


    public void Disconnect(WireSpline wire)
    {
        wires.Remove(wire);
    }

    public bool RecursionCheck(SourceSocket source)
    {
        if (source == this)
            return true;

        for(int i = 0; i < wires.Count; i++)
        {
            if (wires[i].right.RecursionCheck(source))
                return true;
        }
        return false;
    }

    public void OnDestroy()
    {
        List<WireSpline> destructionList = new List<WireSpline>(wires);        
        for(int i = 0; i < destructionList.Count; i++)
        {
            destructionList[i].Disconnect();
            DestroyImmediate(destructionList[i]);
        }
    }

    public void ResetInput()
    {
        for (int i = 0; i < wires.Count; i++)
        {
            wires[i].right.ResetInput();
        }
    }

    public bool isConnected
    {
        get
        {
            return wires.Count > 0;
        }
        private set
        { }
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
