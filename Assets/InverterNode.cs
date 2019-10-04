using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InverterNode : Node {

    //public SinkSocket integerInput;
    public SinkSocket decimalInput;
    //public SinkSocket stringInput;

    public SourceSocket outputSocket;

    float value;
    bool isSet = false;


    public void Awake()
    {
        sinks = new List<SinkSocket>();
        sinks.Add(decimalInput);

        sources = new List<SourceSocket>();
        sources.Add(outputSocket);
    }


    /*
    public override int GetIntegerValue(SourceSocket reader, Node destination)
    {
        if(destination == this)
        {
            // recursion encountered
            return 0;
        }
        else
        {
            return -integerInput.GetIntegerValue(destination);
        }
    }


    public override float GetDecimalValue(SourceSocket reader, Node destination)
    {
        if (destination == this)
        {
            // recursion encountered
            return float.NaN;
        }
        else
        {
            return -decimalInput.GetDecimalValue(destination);
        }
    }


    public override string GetStringValue(SourceSocket reader, Node destination)
    {
        if (destination == this)
        {
            // recursion encountered
            return "";
        }
        else
        {
            return stringInput.GetStringValue(destination);
        }
    }


    // generic type
    public override object GetValue(SourceSocket reader, Node destination)
    {
        return GetIntegerValue(reader, destination);
    }
    */


    public override void SetValue(int x, SinkSocket writer, Node originator)
    {
        if (this == originator) // TODO: I don't think originator is necessary considering that we have RecursionCheck().
            return;

        value = (float)-x;
        isSet = true;
        outputSocket.SetValue(originator, (object)value);
    }


    public override void SetValue(float x, SinkSocket writer, Node originator)
    {
        if (this == originator)
            return;

        value = -x;
        isSet = true;
        outputSocket.SetValue(originator, (object)value);
    }


    public override void SetValue(string x, SinkSocket writer, Node originator)
    {
        if (this == originator)
            return;

        value = -float.Parse(x);
        isSet = true; // TODO: Parse can fail.
        outputSocket.SetValue(originator, (object)x);
    }


    // generic type
    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        if (x is int)
            SetValue((int)x, writer, originator);
        else if (x is float)
            SetValue((float)x, writer, originator);
        else if (x is string)
            SetValue((string)x, writer, originator);
        else
            throw new System.ArgumentException(this.GetType().Name + " was provided a value of type " + x.GetType().FullName + " but doesn't know what to do with it.");
    }


    public override bool RecursionCheck(SourceSocket source)
    {
        return outputSocket.RecursionCheck(source);
    }


    public override void ResetInput(SinkSocket sink)
    {
        value = float.NaN;
        isSet = false;
        outputSocket.ResetInput();
    }


    public override void OnConnect(SourceSocket source, WireSpline ws)
    {
        if(isSet)
            source.SetValue(this, value, ws);
    }


    public override void OnConnect(SinkSocket sink)
    {
        // nothing to do
    }


    public override void OnDisconnect(SourceSocket source)
    {
        // nothing to do
    }


    public override void OnDisconnect(SinkSocket sink)
    {
        // nothing to do
    }

    public override System.Object GetSaveData()
    {
        return null;
    }

    public override void SetSaveData(object saveData)
    {        
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
