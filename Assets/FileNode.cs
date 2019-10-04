using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Threading;
using System.Text;


[Serializable]
public class FileNodeSaveData
{
    public String filename;
    public bool filenameIsSet;
}


/*
    Implements a generic file loading node.  The standard
    File Node prefab provides the UI controls necessary
    to choose a file on the file system, load it, then
    pass it on.

    As currently implemented, this class doesn't actually
    load the file.
*/
public class FileNode : Node
{
    protected bool filenameIsSet = false;
    protected string filename;

    //public InputField stringField;
    public Text filenameLabel;
    public Button openButton;
    public Button closeButton;
    public Dropdown recentsMenu;

    public SourceSocket stringOutput;

    private IntPtr chooseFileContext;
    private bool chooseFileContextIsSet = false;

    public string[] type_names; // These are defined in a prefab.
    public string[] type_extensions;

    private Natives.ChooseFileSuccessCallback successCallback;
    private Natives.ChooseFileCancelledCallback cancelledCallback;


    void Awake()
    {
        sources = new List<SourceSocket>();
        sources.Add(stringOutput);

        // necessary for use of the Natives plugin.
        Loom.Current.GetComponent<Loom>();
        successCallback = this.ChooseFileSuccess;
        cancelledCallback = this.ChooseFileCancelled;
    }


    /*
    public override int GetIntegerValue(SourceSocket reader, Node destination)
    {
        Debug.LogError("FileNode was asked for an integer value.");
        return 0;
    }


    public override float GetDecimalValue(SourceSocket reader, Node destination)
    {
        Debug.LogError("FileNode was asked for a float value.");
        return 0.0f;
    }


    public override string GetStringValue(SourceSocket reader, Node destination)
    {
        return "Hello from a file node.";
    }


    // generic type
    public override object GetValue(SourceSocket reader, Node destination)
    {
        return GetStringValue(reader, destination);
    }
    */


    public override void SetValue(int x, SinkSocket writer, Node originator)
    {
        // has no sinks ockets
        throw new NotImplementedException("FileNode has no sink sockets.");
    }


    public override void SetValue(float x, SinkSocket writer, Node originator)
    {
        // has no sinks ockets
        throw new NotImplementedException("FileNode has no sink sockets.");
    }


    public override void SetValue(string x, SinkSocket writer, Node originator)
    {
        // has no sinks ockets
        throw new NotImplementedException("FileNode has no sink sockets.");
    }


    // generic type
    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        // has no sinks ockets
        throw new NotImplementedException("FileNode has no sink sockets.");
    }


    public override bool RecursionCheck(SourceSocket source)
    {
        // has no source sockets
        throw new NotImplementedException("FileNode has no source sockets.");
    }


    protected virtual void Reset()
    {
        if (chooseFileContextIsSet)
            throw new System.Exception("Should never Reset while the chooseFileContextIsSet.");

        openButton.interactable = true;
        closeButton.interactable = false;
        filenameLabel.text = "";
        filename = "";
        filenameIsSet = false;
        stringOutput.ResetInput();
    }


    public override void ResetInput(SinkSocket sink)
    {
        // has no sinks ockets
        throw new NotImplementedException("FileNode has no sink sockets.");
    }


    public void openButtonPressed()
    {
        if (chooseFileContextIsSet) // prevents re-entrance (i.e. if the user hit the button twice in a row)
            return;

        chooseFileContext = Natives.CreateChooseFileContext();
        chooseFileContextIsSet = true;
        for(int i = 0; i < type_names.Length; i++)
        {
            Natives.AddChooseFileType(chooseFileContext, type_names[i], type_extensions[i]);
        }
        Natives.ChooseFile(chooseFileContext, successCallback, cancelledCallback);
    }


    public void closeButtonPressed()
    {
        if(filenameIsSet)
            uiController.SetDirty();

        Reset();
    }


    public void HandleFilename(string filenameArg)
    {
        openButton.interactable = false;
        closeButton.interactable = true;
        filenameLabel.text = System.IO.Path.GetFileName(filenameArg);
        filename = filenameArg;
        filenameIsSet = true;

        uiController.SetDirty();
    }


    [MonoPInvokeCallback(typeof(Natives.ChooseFileSuccessCallback))]
    public virtual void ChooseFileSuccess(string filenameArg)
    {
        Loom.QueueOnMainThread(() =>
        {
            HandleFilename(filenameArg);
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


    void OnDestroy()
    {
        if (chooseFileContextIsSet)
        {
            Natives.DestroyChooseFileContext(chooseFileContext);
            chooseFileContextIsSet = false;
        }
    }


    public override System.Object GetSaveData()
    {
        FileNodeSaveData fnsd = new FileNodeSaveData();
        fnsd.filenameIsSet = filenameIsSet;
        fnsd.filename = filename;
        return fnsd;
    }


    public override void SetSaveData(System.Object saveData)
    {
        FileNodeSaveData fnsd = (FileNodeSaveData)saveData;
        filenameIsSet = fnsd.filenameIsSet;
        if (filenameIsSet)
        {
            HandleFilename(fnsd.filename);
        }
        else
        {
            Reset();
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
