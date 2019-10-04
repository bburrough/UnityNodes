using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class OutputNode : Node {

    public SinkSocket valueSinkSocket;

    public UnityEngine.UI.Text outputText;


    public void Awake()
    {
        sinks = new List<SinkSocket>();
        sinks.Add(valueSinkSocket);
    }

    /*
    // strict type, int
    public override int GetIntegerValue(SourceSocket reader, Node destination)
    {
        throw new NotImplementedException();
    }


    // strict type, float
    public override float GetDecimalValue(SourceSocket reader, Node destination)
    {
        throw new NotImplementedException();
    }


    // strict type, string
    public override string GetStringValue(SourceSocket reader, Node destination)
    {
        throw new NotImplementedException();
    }


    // generic type
    public override object GetValue(SourceSocket reader, Node destination)
    {
        throw new NotImplementedException();
    }
    */


    // generic type
    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        if (x is float)
        {
            float y = (float)x;
            outputText.text = y.ToString("R");
        }
        else
            outputText.text = x.ToString();
    }


    // strict type, int
    public override void SetValue(int x, SinkSocket writer, Node originator)
    {
        outputText.text = x.ToString();
    }


    // strict type, float
    public override void SetValue(float x, SinkSocket writer, Node originator)
    {
        outputText.text = x.ToString("R");
    }


    // strict type, string
    public override void SetValue(string x, SinkSocket writer, Node originator)
    {
        outputText.text = x;
    }


    public override bool RecursionCheck(SourceSocket source)
    {
        // OutputNode can't recurse because it doesn't have any outputs
        return false;
    }


    public override void ResetInput(SinkSocket sink)
    {
        outputText.text = "undef";
    }

    public override object GetSaveData()
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
