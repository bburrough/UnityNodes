using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Shapes;

public class ScratchPad : MonoBehaviour, IPointerDownHandler {


    public ContextMenuDropdown contextMenu;

    public GameObject inputPrefab;
    public GameObject inverterPrefab;
    public GameObject outputPrefab;
    public GameObject meshLoaderNodePrefab;
    public GameObject gcodeLoaderNodePrefab;
    public GameObject meshDisplayPrefab;
    public GameObject meshSubtractPrefab;
    public GameObject positionPrefab;
    public GameObject position2Prefab;
    public GameObject setPositionPrefab;
    public GameObject waveSlicerPrefab;
    public GameObject planeSlicerPrefab;
    public GameObject randomPrefab;
    public GameObject setRotationPrefab;
    public GameObject arithmeticPrefab;
    public GameObject polygonSetContractorPrefab;
    public GameObject clockPrefab;
    public GameObject timeSeriesPrefab;
    public GameObject mathFunctionPrefab;
    public GameObject audioGeneratorPrefab;
    public GameObject fermatRouterPrefab;
    public GameObject filePrefab;
    public GameObject triangulatePrefab;

    public Canvas parentCanvas;
    public ContextMenuDropdown nodeContextMenu;
    public GameObject nodeParent;
    public UserInterfaceController uiController;

    private GameObject selectionTubeGO;



    // Use this for initialization
    void Start() {
    }


    // Update is called once per frame
    void Update() {

    }


    public void PointerUp(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        if (ped.button == PointerEventData.InputButton.Right)
        {
            //contextMenu.gameObject.active = true;
            Vector3 pos;
            pos = Camera.main.ScreenToWorldPoint(ped.pressPosition);
            pos.z = -10.0f;
            contextMenu.transform.position = pos;
            //CanvasRenderer cr;
            //contextMenu.
            contextMenu.enabled = true;
            contextMenu.Show();
        }
    }


    //private Square selectionSquare = null;
    //private GameObject selectionSquareGO = null;

    private float selectionTubeRadius = 1f;
    private Color selectionTubeColor = Color.blue;
    public GameObject tubePrefab;
    private GameObject trgo = null;
    private TubeRenderer tr = null;
    private float selectionTubeZ = 2f;
    public void BeginDrag(BaseEventData bed)
    {
        PointerEventData ped = (PointerEventData)bed;
        if (ped.button != PointerEventData.InputButton.Left)
            return;

        trgo = GameObject.Instantiate(tubePrefab) as GameObject;
        tr = trgo.GetComponent<TubeRenderer>();
        trgo.transform.SetParent(transform, false);
        trgo.transform.position = Vector3.zero;
        trgo.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

        pressPos = Camera.main.ScreenToWorldPoint(ped.pressPosition);
        pressPos.z = selectionTubeZ;

        Vector3 pos;
        pos = Camera.main.ScreenToWorldPoint(ped.position);
        pos.z = selectionTubeZ;

        SelectionBoxPositionUpdate(pressPos, pos);
    }


    private void SelectionBoxPositionUpdate(Vector3 upperLeftCorner, Vector3 loweRightCorner)
    {
        //Vector3 startStopPosition = new Vector3(upperLeftCorner.x, selectionTubeZ, (upperLeftCorner.y + loweRightCorner.y) / 2f); // We start and stop the tube half way through the left most side of the box rather than starting at the corner to make sure the start/stop is seamless.
        tr.Reset();
        // starting point
        //tr.AppendPoint(new TubeVertex(startStopPosition, selectionTubeRadius, selectionTubeColor));
        // box proper
        tr.AppendPoint(new TubeVertex(new Vector3(upperLeftCorner.x, selectionTubeZ, upperLeftCorner.y), selectionTubeRadius, selectionTubeColor));
        tr.AppendPoint(new TubeVertex(new Vector3(loweRightCorner.x, selectionTubeZ, upperLeftCorner.y), selectionTubeRadius, selectionTubeColor));
        tr.AppendPoint(new TubeVertex(new Vector3(loweRightCorner.x, selectionTubeZ, loweRightCorner.y), selectionTubeRadius, selectionTubeColor));
        tr.AppendPoint(new TubeVertex(new Vector3(upperLeftCorner.x, selectionTubeZ, loweRightCorner.y), selectionTubeRadius, selectionTubeColor));
        tr.AppendPoint(new TubeVertex(new Vector3(upperLeftCorner.x, selectionTubeZ, upperLeftCorner.y), selectionTubeRadius, selectionTubeColor));
        // stopping point
        //tr.AppendPoint(new TubeVertex(startStopPosition, selectionTubeRadius, selectionTubeColor));
        tr.Rebuild();
    }

    private Vector3 pressPos;
    public void OnDrag(BaseEventData bed)
    {
        if (!trgo)
            return;

        PointerEventData ped = (PointerEventData)bed;
        Vector3 pos;
        pos = Camera.main.ScreenToWorldPoint(ped.position);
        pos.z = selectionTubeZ;

        SelectionBoxPositionUpdate(pressPos, pos);

        UpdateSelectionGroup(pressPos, pos);
    }


    private bool TestOverlap(RectTransform rt, Vector3 upperLeftCorner, Vector3 lowerRightCorner)
    {
        // sorted positions
        Vector2 upperLeft = new Vector2(Math.Min(upperLeftCorner.x, lowerRightCorner.x), Math.Min(upperLeftCorner.y, lowerRightCorner.y));
        Vector2 lowerRight = new Vector2(Math.Max(upperLeftCorner.x, lowerRightCorner.x), Math.Max(upperLeftCorner.y, lowerRightCorner.y));

        Vector2 rtUpperLeft = new Vector2(rt.anchoredPosition.x - rt.sizeDelta.x, rt.anchoredPosition.y - rt.sizeDelta.y);
        Vector2 rtLowerRight = new Vector2(rt.anchoredPosition.x + rt.sizeDelta.x, rt.anchoredPosition.y + rt.sizeDelta.y);

        bool horizontalComponentsOverlap = rt.anchoredPosition.x > upperLeft.x && rt.anchoredPosition.x < lowerRight.x;
        bool verticalComponentsOverlap = rt.anchoredPosition.y > upperLeft.y && rt.anchoredPosition.y < lowerRight.y;

        return horizontalComponentsOverlap && verticalComponentsOverlap;
    }



    private void UpdateSelectionGroup(Vector3 upperLeftCorner, Vector3 lowerRightCorner)
    {
        if (!nodeParent)
            return;

        //List<Node> selectedNodes = new List<Node>();
        foreach (Node childNode in nodeParent.GetComponentsInChildren<Node>())
        {
            RectTransform childRT = childNode.GetComponent<RectTransform>();
            if (TestOverlap(childRT, upperLeftCorner, lowerRightCorner))
            {
                //selectedNodes.Add(childNode);
                childNode.Select();
            }
            else
            {
                if(!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    childNode.Deselect();
            }
        }

    }


    public void DeselectAllNodes()
    {
        if (!nodeParent)
            return;

        foreach (Node childNode in nodeParent.GetComponentsInChildren<Node>())
        {
            childNode.Deselect();
        }
    }


    public void DeselectAllExcept(Node dontDeselect)
    {
        if (!nodeParent)
            return;

        foreach (Node childNode in nodeParent.GetComponentsInChildren<Node>())
        {
            if (childNode == dontDeselect)
                continue;

            childNode.Deselect();
        }
    }


    public void EndDrag(BaseEventData bed)
    {
        if (!trgo)
            return;

        DestroyImmediate(trgo);
        trgo = null;

        //DeselectAllNodes();
    }


    public void OnContextMenuValueChagned(Int32 value)
    {
        //Vector3 node_position = contextMenu.transform.position;
        //RectTransform nodeRT = node.GetComponent<RectTransform>();
        //node_position.x += nodeRT.rect.width / 2;
        //node_position.y -= nodeRT.rect.height / 2;

        AddNode((NodeType)value, contextMenu.transform.position.x, contextMenu.transform.position.y);

        contextMenu.value = 0;
        contextMenu.Toss();
    }


    public Node AddNode(NodeType nodeType, float xPosition, float yPosition)
    {
        GameObject go;
        GameObject prefab;
        switch (nodeType)
        {
            case NodeType.Input:
                prefab = inputPrefab;
                break;
            case NodeType.Inverter:
                prefab = inverterPrefab;
                break;
            case NodeType.Output:
                prefab = outputPrefab;
                break;
            case NodeType.MeshLoader:
                prefab = meshLoaderNodePrefab;
                break;
            case NodeType.GcodeLoader:
                prefab = gcodeLoaderNodePrefab;
                break;
            case NodeType.MeshDisplay:
                prefab = meshDisplayPrefab;
                break;
            case NodeType.MeshSubtract:
                prefab = meshSubtractPrefab;
                break;
            case NodeType.Position:
                prefab = positionPrefab;
                break;
            case NodeType.Position2:
                prefab = position2Prefab;
                break;
            case NodeType.SetPosition:
                prefab = setPositionPrefab;
                break;
            case NodeType.WaveSlicer:
                prefab = waveSlicerPrefab;
                break;
            case NodeType.PlaneSlicer:
                prefab = planeSlicerPrefab;
                break;
            case NodeType.Random:
                prefab = randomPrefab;
                break;
            case NodeType.SetRotation:
                prefab = setRotationPrefab;
                break;
            case NodeType.Arithmetic:
                prefab = arithmeticPrefab;
                break;
            case NodeType.PolygonSetContractor:
                prefab = polygonSetContractorPrefab;
                break;
            case NodeType.Clock:
                prefab = clockPrefab;
                break;
            case NodeType.TimeSeriesGraph:
                prefab = timeSeriesPrefab;
                break;
            case NodeType.MathFunction:
                prefab = mathFunctionPrefab;
                break;
            case NodeType.AudioGenerator:
                prefab = audioGeneratorPrefab;
                break;
            case NodeType.FermatRouter:
                prefab = fermatRouterPrefab;
                break;
            case NodeType.File:
                prefab = filePrefab;
                break;
            case NodeType.Triangulate:
                prefab = triangulatePrefab;
                break;
            default:
                contextMenu.value = 0;
                return null;
        }
        go = Instantiate(prefab) as GameObject;
        Node node = go.GetComponent<Node>();
        node.parentCanvas = parentCanvas;
        node.scratchPad = this;
        node.contextMenu = nodeContextMenu;
        node.nodeType = nodeType;

        if (nodeParent == null)
        {
            nodeParent = new GameObject("Node Parent");
            nodeParent.transform.SetParent(parentCanvas.transform);
        }

        Vector3 node_position;
        node_position.x = xPosition;
        node_position.y = yPosition;
        node_position.z = -2.0f;

        go.transform.position = node_position;
        go.transform.SetParent(nodeParent.transform);
        node.uiController = uiController;
        uiController.SetDirty();
        return node;
    }


    public void ClearNodes()
    {
        DestroyImmediate(nodeParent);
        nodeParent = null;
    }


    void IPointerDownHandler.OnPointerDown(PointerEventData ped)
    {
        //Debug.Log("pointer down");
        if (ped.button == PointerEventData.InputButton.Left)
        {
            if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                DeselectAllNodes();   
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
