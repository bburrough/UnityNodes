using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Dragger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private bool leftHandDrag = false;

    public void OnInitializePotentialDrag(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        ped.useDragThreshold = false;
    }


    private Vector3 mousePositionOffset;
    public void BeginDrag(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        if (ped.button == PointerEventData.InputButton.Left)
        {
            leftHandDrag = true;

            Vector3 currentWidgetScreenPoint = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 currentMouseScreenPoint = Input.mousePosition;
            mousePositionOffset = currentWidgetScreenPoint - currentMouseScreenPoint;

            {
                // The fact that this is here suggests the drag code perhaps belongs inside Node.
                Node node = GetComponent<Node>();
                if (node)
                {
                    node.uiController.SetDirty();

                    node.consumePointerUp = true;

                    foreach (Node candidateNode in node.scratchPad.nodeParent.GetComponentsInChildren<Node>())
                    {
                        if (candidateNode == node)
                            continue;

                        if (candidateNode.isSelected)
                            candidateNode.transform.SetParent(node.transform, true);
                    }
                }
            }
        }
    }


    public void OnDrag()
    {
        if (leftHandDrag)
        {
            Vector3 updatedPosition = Input.mousePosition;
            updatedPosition.x += mousePositionOffset.x;
            updatedPosition.y += mousePositionOffset.y;

            
            float rescueZ = transform.position.z;
            // To reiterate...in Looking Glass mode, screen position is expressed as a viewport point, otherwise screen point.
            Vector3 newPosition;
            newPosition = Camera.main.ScreenToWorldPoint(updatedPosition);
            newPosition.z = rescueZ;
            transform.position = newPosition;

            foreach (SinkSocket ss in transform.GetComponentsInChildren<SinkSocket>())
                ss.UpdateWires();
            foreach (SourceSocket ss in transform.GetComponentsInChildren<SourceSocket>())
                ss.UpdateWires();
        }
    }


    public void EndDrag(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        if (ped.button == PointerEventData.InputButton.Left)
        {
            leftHandDrag = false;

            {
                // The fact that this is here suggests the drag code perhaps belongs inside Node.
                Node node = GetComponent<Node>();
                if (node)
                {
                    foreach (Node candidateNode in node.GetComponentsInChildren<Node>())
                    {
                        candidateNode.transform.SetParent(node.scratchPad.nodeParent.transform, true);
                    }
                }
            }
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
