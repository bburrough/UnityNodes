using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class SinkSocket : MonoBehaviour {

    private WireSpline wireSpline;
    public Node node;
    public SourceSinkType type = SourceSinkType.Integer;

    private WireSpline draggingWireSpline;

    // Use this for initialization
    void Start () {
	
	}


    public void UpdateWires()
    {
        if(wireSpline)
            wireSpline.connectorRightPosition = transform.position;
    }


    public void BeginDrag()
    {
        //Debug.Log("begin drag");

        draggingWireSpline = wireSpline;
        wireSpline.Disconnect();

    }


    public void OnDrag()
    {
        if (draggingWireSpline == null)
            return;

        Vector3 mouseWorldPosition;
        mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 newPosition = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, -8.0f);
        draggingWireSpline.connectorRightPosition = newPosition;
    }


    public void EndDrag(BaseEventData bed)
    {
        if (draggingWireSpline == null)
            return;

        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll((PointerEventData)bed, hits);
        for (int i = 0; i < hits.Count; i++)
        {
            SinkSocket sink = hits[i].gameObject.GetComponent<SinkSocket>();
            if (sink)
            {
                if (sink.node == draggingWireSpline.left.node)
                {
                    continue;
                }

                if (sink.ConnectWire(draggingWireSpline, type))
                {
                    draggingWireSpline.left.wires.Add(draggingWireSpline); // TODO: Wires should be private.
                    sink.OnConnect();
                    draggingWireSpline.left.node.OnConnect(draggingWireSpline.left, draggingWireSpline);

                    //currentWireSplineGo = null;
                    draggingWireSpline = null;
                    node.uiController.SetDirty();
                    return;
                }
            }
        }
        DestroyImmediate(draggingWireSpline.gameObject);
        node.uiController.SetDirty();
    }


    public void SetValue(int x, Node originator)
    {
        node.SetValue(x, this, originator);
    }


    public void SetValue(float x, Node originator)
    {
        node.SetValue(x, this, originator);
    }


    public void SetValue(string x, Node originator)
    {
        node.SetValue(x, this, originator);
    }


    public void SetValue(object x, Node originator)
    {
        node.SetValue(x, this, originator);
    }


    /*
    public int GetIntegerValue(Node destination)
    {
        if (ws)
            return ws.left.GetIntegerValue(destination);
        else
            return 0;
    }


    public float GetDecimalValue(Node destination)
    {
        return ws.left.GetDecimalValue(destination);
    }


    public string GetStringValue(Node destination)
    {
        return ws.left.GetStringValue(destination);
    }


    public object GetValue(Node destination)
    {
        if (ws == null)
            return null;
        return ws.left.GetValue(destination);
    }
    */


    private bool RecursionTest()
    {
       // List<WireSpline> inspectedWires = new List<WireSpline>();

        return false;
            

    }

    public bool ConnectWire(WireSpline wire, SourceSinkType sourceType)
    {
        if (type == sourceType || type == SourceSinkType.Generic)
        {            
            if (RecursionCheck(wire.left)) // check whether any connectoins lead back to our source
                return false;

            if (wireSpline && wireSpline != wire)  // If there's a wire already connected, let's disconnect it.
            {
                WireSpline wsGoingAway = wireSpline; // this is necessary because Disconnect will set ws to null, but we subsequently need to destroy the wire.
                wsGoingAway.Disconnect();

#if false
                // snap the wire back (like letting go of an extended tape measure)
                WireSpline oldWire = ws;
                System.Action<Vector3> tweenAction = (Vector3 val) => { oldWire.connectorRightPosition = val; };
                LeanTween.value(oldWire.gameObject, tweenAction, oldWire.connectorRightPosition, oldWire.connectorLeftPosition, 0.125f).setEase(LeanTweenType.easeInQuad).setOnComplete(() => { DestroyImmediate(oldWire); });
#else
                DestroyImmediate(wsGoingAway.gameObject);
#endif                
            }
            wireSpline = wire; // This is necessary to update the position of the wire when the sink moves.
            wire.connectorRightPosition = transform.position;
            wire.right = this; // This is necessary to signal the sink when the wire value changes.

            return true;
        }
        return false;
    }


    public bool RecursionCheck(SourceSocket source)
    {
        return node.RecursionCheck(source);
    }


    public void ResetInput()
    {
        node.ResetInput(this);
    }

     
    public void Disconnect()
    {
        node.ResetInput(this);
        OnDisconnect();
        wireSpline = null;
    }

    public void OnDestroy()
    {   
        if (wireSpline)
        {
            WireSpline temp = wireSpline; // because ws gets set to null in Disconnect()            
            temp.Disconnect();
            Destroy(temp);
        }
    }

    public virtual void OnConnect()
    {
        node.OnConnect(this);
    }

    public virtual void OnDisconnect()
    {
        node.OnDisconnect(this);
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
