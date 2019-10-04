using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


[System.Serializable]
class NodeBuffer
{
    public List<NodeType> nodeTypes;
    public List<float> nodeXPosition;
    public List<float> nodeYPosition;
    public List<System.Object> nodeSaveData;

    public List<WireSplineSaveData> wireSplineAssociations;

    [System.Serializable]
    public struct WireSplineSaveData
    {
        public int sourceNodeIndex;
        public int sourceSocketIndex;
        public int sinkNodeIndex;
        public int sinkSocketIndex;
    }
}


[System.Serializable]
class SaveData
{
    public float cameraX;
    public float cameraY;
    public float cameraZ;
    public float cameraSize;

    public NodeBuffer nodeBuffer;
}

/*
    TODO:
    - support for dirty/clean status.
        - "File has changed, would you like to save it? Warning."
        //- Enable/disable save, save as buttons.
    - Open recents.
    - Close support. (Clear the scratchpad.)
    - Clear the scratchpad before attempting to load.
*/
public class UserInterfaceController : MonoBehaviour {
    
    public ScratchPad scratchPad;
    public CameraFitter cameraFitter;
    public UnityEngine.UI.Text filenameLabel;
    public UnityEngine.UI.Button saveButton;
    public UnityEngine.UI.Button saveAsButton;
    public UnityEngine.UI.Button closeButton;

    private string _savedFilename;
    private string savedFilename
    {
        get
        {
            return _savedFilename;
        }
        set
        {
            _savedFilename = value;
            filenameLabel.text = filenameLabel.text = System.IO.Path.GetFileName(savedFilename);
        }
    }

    private bool isFileLoaded = false;
    public bool isDirty = false;

    private IntPtr chooseFileContext;
    private bool chooseFileContextIsSet = false;

    private Natives.ChooseFileSuccessCallback successCallback;
    private Natives.ChooseFileCancelledCallback cancelledCallback;
    private Natives.ChooseFileSuccessCallback saveAsSuccessCallback;

    public string[] type_names; // These are defined in a prefab.
    public string[] type_extensions;


    // Use this for initialization
    void Start () {
        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;    
	}


    public void Awake()
    {
        // necessary for use of the Natives plugin.
        Loom.Current.GetComponent<Loom>();
        successCallback = this.ChooseFileSuccess;
        cancelledCallback = this.ChooseFileCancelled;
        saveAsSuccessCallback = this.ChooseFileSaveAsSuccess;
    }

	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                CopySelectedNodes();
            }
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                PasteNodeBuffer();
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                Debug.Log("Ctrl-X");
            else
                Debug.Log("X");
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeleteSelectedNodes();
        }
    }


    private void DeleteSelectedNodes()
    {
        foreach (Node node in scratchPad.nodeParent.GetComponentsInChildren<Node>())
        {
            if (node.isSelected)
            {
                DestroyImmediate(node.gameObject);
                SetDirty();
            }
        }
    }


    private NodeBuffer copyBuffer;
    private int pasteCount = 0;
    private void CopySelectedNodes()
    {
        List<Node> selectedNodes = new List<Node>();
        foreach (Node node in scratchPad.nodeParent.GetComponentsInChildren<Node>())
        {
            if (node.isSelected)
            {
                selectedNodes.Add(node);
            }
        }
        copyBuffer = CreateNodeBuffer(selectedNodes.ToArray()); // Could create another CreateNodeBuffer() to get rid of the ToArray() call.
        pasteCount = 0;
    }


    private float pasteOffset = 30f;
    private void PasteNodeBuffer()
    {
        if (copyBuffer == null)
            return;
        scratchPad.DeselectAllNodes();
        pasteCount++; // TODO: This can go crazy, running off the scratch pad.
        LoadNodes(copyBuffer, pasteOffset * pasteCount, pasteOffset * pasteCount, true);
    }


    public void SetDirty(bool dirty = true)
    {
        if (dirty != isDirty)
        {
            isDirty = dirty;
            if (isDirty)
            {
                if(saveButton) // teardown may cause the saveButton to be set to null while SetDirty() is still being called due to wires being disconnected
                    saveButton.interactable = true;
                if(saveAsButton)
                    saveAsButton.interactable = true;

                if (isFileLoaded && filenameLabel)
                    filenameLabel.text += "*";
            }
            else
            {
                saveButton.interactable = false;
                saveAsButton.interactable = false;
            }
        }
    }


    private string TestFilename()
    {
        return Application.persistentDataPath + "/hello_world.dat";
    }


    [MonoPInvokeCallback(typeof(Natives.ChooseFileSuccessCallback))]
    public virtual void ChooseFileSuccess(string filenameArg)
    {
        Loom.QueueOnMainThread(() =>
        {
            Load(filenameArg);
        });
        Natives.DestroyChooseFileContext(chooseFileContext);
        chooseFileContextIsSet = false;
    }


    [MonoPInvokeCallback(typeof(Natives.ChooseFileSuccessCallback))]
    public virtual void ChooseFileSaveAsSuccess(string filenameArg)
    {
        Loom.QueueOnMainThread(() =>
        {
            Save(filenameArg);
        });
        Natives.DestroyChooseFileContext(chooseFileContext);
        chooseFileContextIsSet = false;
    }


    [MonoPInvokeCallback(typeof(Natives.ChooseFileCancelledCallback))]
    public void ChooseFileCancelled()
    {
        Natives.DestroyChooseFileContext(chooseFileContext);
        chooseFileContextIsSet = false;
    }


    private NodeBuffer CreateNodeBuffer(Node[] nodes)
    {
        NodeBuffer nodeBuffer = new NodeBuffer();
        nodeBuffer.nodeTypes = new List<NodeType>(nodes.Length);
        nodeBuffer.nodeXPosition = new List<float>(nodes.Length);
        nodeBuffer.nodeYPosition = new List<float>(nodes.Length);
        nodeBuffer.nodeSaveData = new List<System.Object>(nodes.Length);
        for (int i = 0; i < nodes.Length; i++)
        {
            nodeBuffer.nodeTypes.Add(nodes[i].nodeType);
            nodeBuffer.nodeXPosition.Add(nodes[i].transform.position.x);
            nodeBuffer.nodeYPosition.Add(nodes[i].transform.position.y);
            nodeBuffer.nodeSaveData.Add(nodes[i].GetSaveData());
        }

        for (int i = 0; i < nodes.Length; i++)
        {
            Node currentNode = nodes[i];
            if (currentNode.sources == null)
                continue;

            for (int j = 0; j < currentNode.sources.Count; j++)
            {
                SourceSocket currentSourceSocket = currentNode.sources[j];

                for (int k = 0; k < currentSourceSocket.wires.Count; k++)
                {
                    WireSpline currentWireSpline = currentSourceSocket.wires[k];

                    NodeBuffer.WireSplineSaveData wssd = new NodeBuffer.WireSplineSaveData();
                    wssd.sourceNodeIndex = i;
                    wssd.sourceSocketIndex = j;
                    //wssd.sinkNodeIndex = nodes.IndexOf(currentWireSpline.right.node);
                    wssd.sinkNodeIndex = System.Array.IndexOf(nodes, currentWireSpline.right.node);
                    wssd.sinkSocketIndex = currentWireSpline.right.node.sinks.IndexOf(currentWireSpline.right);

                    //if (wssd.sinkSocketIndex == -1 || wssd.sinkNodeIndex == -1)
                    //{
                    //    Debug.Log("wssd: " + wssd.sinkSocketIndex + " sinks: " + currentWireSpline.right.node.sinks + " count: " + currentWireSpline.right.node.sinks.Count);
                    //    Debug.Log("right: " + currentWireSpline.right.GetHashCode());
                    //    for (int l = 0; l < currentWireSpline.right.node.sinks.Count; l++)
                    //    {
                    //        Debug.Log("l: " + l + " " + currentWireSpline.right.node.sinks[l].GetHashCode());
                    //    }
                    //}

                    if (nodeBuffer.wireSplineAssociations == null)
                        nodeBuffer.wireSplineAssociations = new List<NodeBuffer.WireSplineSaveData>();

                    nodeBuffer.wireSplineAssociations.Add(wssd);
                }
            }
        }

        return nodeBuffer;
    }


    private void Save(string filename)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filename, FileMode.OpenOrCreate);

        SaveData sd = new SaveData();
        sd.cameraX = cameraFitter.transform.position.x;
        sd.cameraY = cameraFitter.transform.position.y;
        sd.cameraZ = cameraFitter.transform.position.z;
        sd.cameraSize = cameraFitter.targetOrthograhicSize;

        Node[] nodes = scratchPad.parentCanvas.GetComponentsInChildren<Node>(); // TODO: This is kind of whacky. Perhaps there should be a primary node owner?                                                                             

        sd.nodeBuffer = CreateNodeBuffer(nodes);

        bf.Serialize(file, sd);
        file.Close();

        SetSavedFilename(filename);
        SetDirty(false);
    }


    private void LoadNodes(NodeBuffer nodeBuffer)
    {
        LoadNodes(nodeBuffer, 0f, 0f, false);
    }

    private void LoadNodes(NodeBuffer nodeBuffer, float xOffset, float yOffset, bool selected)
    {        
        List<Node> nodes = new List<Node>();

        // load all nodes
        for (int i = 0; i < nodeBuffer.nodeTypes.Count; i++)
        {
            Node node = scratchPad.AddNode(nodeBuffer.nodeTypes[i],
                nodeBuffer.nodeXPosition[i] + xOffset,
                nodeBuffer.nodeYPosition[i] + yOffset);

            node.SetSaveData(nodeBuffer.nodeSaveData[i]);

            nodes.Add(node);
        }

        //Node[] nodes = scratchPad.parentCanvas.GetComponentsInChildren<Node>();

        // now that all nodes are loaded, wire them up
        if (nodeBuffer.wireSplineAssociations != null)
        {
            for (int i = 0; i < nodeBuffer.wireSplineAssociations.Count; i++)
            {
                if (nodeBuffer.wireSplineAssociations[i].sinkNodeIndex == -1 || nodeBuffer.wireSplineAssociations[i].sourceNodeIndex == -1)
                    continue;

                Node sourceNode = nodes[nodeBuffer.wireSplineAssociations[i].sourceNodeIndex]; // TODO: nodes should be private.
                SourceSocket sourceSocket = sourceNode.sources[nodeBuffer.wireSplineAssociations[i].sourceSocketIndex];
                Node sinkNode = nodes[nodeBuffer.wireSplineAssociations[i].sinkNodeIndex];
                SinkSocket sinkSocket = sinkNode.sinks[nodeBuffer.wireSplineAssociations[i].sinkSocketIndex];
                sourceSocket.CreateNewWireTo(sinkSocket);
            }
        }

        if (selected)
        {
            scratchPad.DeselectAllNodes();
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Select();
        }
    }


    private void Load(string filename)
    {
        Close();

        //string filename = TestFilename();
        if (File.Exists(filename))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filename, FileMode.Open);
            SaveData sd = (SaveData)bf.Deserialize(file);
            file.Close();

            cameraFitter.transform.position = new Vector3(sd.cameraX, sd.cameraY, -670f);
            cameraFitter.targetOrthograhicSize = sd.cameraSize;

            LoadNodes(sd.nodeBuffer);

            SetSavedFilename(filename);
            SetDirty(false);
        }
    }


    public void onOpenButtonPressed()
    {
        if (chooseFileContextIsSet) // prevents re-entrance (i.e. if the user hit the button twice in a row)
            return;

        chooseFileContext = Natives.CreateChooseFileContext();
        chooseFileContextIsSet = true;
        for (int i = 0; i < type_names.Length; i++)
        {
            Natives.AddChooseFileType(chooseFileContext, type_names[i], type_extensions[i]);
        }
        Natives.ChooseFile(chooseFileContext, successCallback, cancelledCallback);
    }


    public void onSaveButtonPressed()
    {
        /*
            If a filename is already established (i.e.
            a file has already been opened, overwrite it.
        */
        if (isFileLoaded)
            Save(savedFilename);
        else
            onSaveAsButtonPressed();
    }


    public void onSaveAsButtonPressed()
    {
        if (chooseFileContextIsSet) // prevents re-entrance (i.e. if the user hit the button twice in a row)
            return;

        chooseFileContext = Natives.CreateChooseFileContext();
        chooseFileContextIsSet = true;
        Natives.SetChooseFileModeSaveAs(chooseFileContext);
        Natives.ChooseFile(chooseFileContext, saveAsSuccessCallback, cancelledCallback);
    }


    private void SetSavedFilename(string filename)
    {
        isFileLoaded = true;
        savedFilename = filename;
        closeButton.interactable = true;
    }


    private void UnsetSavedFilename()
    {
        isFileLoaded = false;
        savedFilename = "";
        closeButton.interactable = false;
        SetDirty(false);
    }


    private void Close()
    {
        scratchPad.ClearNodes();
        UnsetSavedFilename();
    }


    public void onCloseButtonPressed()
    {
        Close();
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
