using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

/*
    Node data flow

    When a user clicks-and-drags on a source socket, the source
    socket instantiates a wire (which renders the tube), and
    processes the drag events to show the wire moving around.

    When the user drops the wire, that same code does a  raycast
    into the scene at the mouse position.  If that raycast hits
    a sink socket, the source socket attempts to connect
    the wire to the sink.  If the connection succeeds, the node
    is notified of the connection via an OnConnect event, and
    if it has data it will then pass data along via SetValue().

    To rephrase all of that, all data flows left to right.
    You might notice that there are no GetValue() calls in the
    nodes, sinks, or sources.  That is so, to enforce the left-
    to-right convention.  As explained above, when a wire
    is connected between them, the originating node (left-hand-
    side) is notified via the OnConnect method.  It then has
    the prerogative to send data across the wire via SetValue().
    Although, it is not required to do so (e.g. it might not
    have all requisite data.

    When a Node is instantiate it (obviously) hasn't received
    any data.  It's internal data is in a null state.  Only
    after all prerequisite data has arrived, will its internal
    state be valid.  If is internal state is invalidated --
    for example, when a wire that was providing essential
    data is disconnected -- it will notify down-stream
    vodes via ResetInput().



    top-down, left-right ordering

    When writing code for nodes, let's, by convention,
    write in the following order:

    source before sink
    connect before disconnect
*/


public enum NodeType
{
    Input = 1,
    Inverter,
    Output,
    MeshLoader,
    GcodeLoader,
    MeshDisplay,
    MeshSubtract,
    Position,
    Position2,
    SetPosition,
    WaveSlicer,
    PlaneSlicer,
    Random,
    SetRotation,
    Arithmetic,
    PolygonSetContractor,
    Clock,
    TimeSeriesGraph,
    MathFunction,
    AudioGenerator,
    FermatRouter,
    File,
    Triangulate
};


public abstract class Node : MonoBehaviour, IPointerDownHandler {

    public Canvas parentCanvas;
    public ContextMenuDropdown contextMenu;
    public Material nodeBackgroundMaterial;
    public Material socketBackgroundMaterial;
    public NodeType nodeType;
    public UserInterfaceController uiController;
    public ScratchPad scratchPad;

    [NonSerialized]
    public List<SinkSocket> sinks;
    [NonSerialized]
    public List<SourceSocket> sources;

    /*
    // strict types
    public abstract int GetIntegerValue(SourceSocket reader, Node destination);
    public abstract float GetDecimalValue(SourceSocket reader, Node destination);
    public abstract string GetStringValue(SourceSocket reader, Node destination);
    // generic type
    public abstract object GetValue(SourceSocket reader, Node destination);
    */
    // strict types
    public virtual void SetValue(int x, SinkSocket writer, Node originator)
    {
        throw new System.NotImplementedException();
    }

    public virtual void SetValue(float x, SinkSocket writer, Node originator)
    {
        throw new System.NotImplementedException();
    }

    public virtual void SetValue(string x, SinkSocket writer, Node originator)
    {
        throw new System.NotImplementedException();
    }

    // generic type
    public virtual void SetValue(object x, SinkSocket writer, Node originator)
    {
        throw new System.NotImplementedException();
    }

    public abstract bool RecursionCheck(SourceSocket source);

    public abstract void ResetInput(SinkSocket sink);


    public virtual void OnPointerUp(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        if (ped.button == PointerEventData.InputButton.Right)
        {
            /*          
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData("Halcyon"));
            options.Add(new Dropdown.OptionData("Days"));
            options.Add(new Dropdown.OptionData("Klingon"));
            options.Add(new Dropdown.OptionData("Empire"));

            nodeContextMenu.AddOptions(options);
            nodeContextMenu.onValueChanged.AddListener((int x) => { Debug.Log("new value " + x); });
            */

            Vector3 pos;
            pos = Camera.main.ScreenToWorldPoint(ped.position);
            pos.z = -20f;
            contextMenu.transform.position = pos;
            contextMenu.enabled = true;
            contextMenu.onValueChanged.RemoveAllListeners();
            contextMenu.onValueChanged.AddListener(delegate (int x)
            {
                switch (x)
                {
                    case 1:
                        DestroyImmediate(gameObject);
                        uiController.SetDirty();
                        break;
                }
                contextMenu.value = 0;
                contextMenu.Toss();
            });
            contextMenu.Show();
        }

        if (ped.button == PointerEventData.InputButton.Left || ped.button == PointerEventData.InputButton.Right)
        {
            if (!consumePointerUp)
                Deselect();
            consumePointerUp = false;
        }
    }


    //private Vector3 focusOffset = new Vector3(0f, 0f, -15f);
    private float focusOffset = -15f;
    [SerializeField]
    private bool offsetsAreSet = false;
    private float transitionTime = 0.125f;    
    private void CacheOffsets()
    {
        if (!offsetsAreSet)
        {
            _upZ = transform.position.z + focusOffset;
            _downZ = transform.position.z;
            offsetsAreSet = true;
        }
    }


    [SerializeField]
    private float _upZ;
    private float upZ
    {
        get
        {
            CacheOffsets();
            return _upZ;
        }
    }


    [SerializeField]
    private float _downZ;
    private float downZ
    {
        get
        {
            CacheOffsets();
            return _downZ;
        }
    }


    void IPointerDownHandler.OnPointerDown(PointerEventData ped)
    {
        OnPointerDownStub(ped);
    }


    public bool consumePointerUp = false;
    private void OnPointerDownStub(PointerEventData ped)
    {
        if (ped.button == PointerEventData.InputButton.Left || ped.button == PointerEventData.InputButton.Right)
        {            
            Select();
            consumePointerUp = true;

            if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                scratchPad.DeselectAllExcept(this);
            }
        }
    }


    public virtual void OnPointerDown(BaseEventData bed)
    {
        /*
            TODO: This sucks. I had to add it because for some inexplicable reason, the unity event system
            is refusing to use IPointerDownHandler for MeshPreviewNode.  It works for every other node, just
            not that one.
        */
        OnPointerDownStub((PointerEventData)bed);
    }

    private void SocketWireUpdater()
    {
        foreach (SinkSocket ss in transform.GetComponentsInChildren<SinkSocket>())
            ss.UpdateWires();
        foreach (SourceSocket ss in transform.GetComponentsInChildren<SourceSocket>())
            ss.UpdateWires();
    }

    //private System.Action socketPositionUpdateAction = SocketPositionUpdater;

    public bool isSelected = false;
    public void Select()
    {
        if (LeanTween.isTweening(gameObject) && isSelected)
            return;

        LeanTween.cancel(gameObject);
        LeanTween.moveZ(gameObject, upZ, transitionTime).setOnUpdate((float x)=> { SocketWireUpdater(); }).setEase(LeanTweenType.easeInOutQuad);
        isSelected = true;
    }


    public void Deselect()
    {
        if (LeanTween.isTweening(gameObject) && !isSelected)
            return;

        LeanTween.cancel(gameObject);
        LeanTween.moveZ(gameObject, downZ, transitionTime).setOnUpdate((float x) => { SocketWireUpdater(); }).setEase(LeanTweenType.easeInOutQuad);
        isSelected = false;
    }


    public virtual void OnConnect(SourceSocket source, WireSpline ws)
    {
    }


    public virtual void OnConnect(SinkSocket sink)
    {
    }


    public virtual void OnDisconnect(SourceSocket source)
    {
    }


    public virtual void OnDisconnect(SinkSocket sink)
    {
    }

    public abstract System.Object GetSaveData();
    public abstract void SetSaveData(System.Object saveData);

    //public virtual System.Object GetSaveData()
    //{
    //    return null;
    //}

    //public virtual void SetSaveData(System.Object saveData)
    //{
    //}
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
