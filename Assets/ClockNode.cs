using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;


[Serializable]
public class ClockNodeSaveData : System.Object
{
    public float clockFrequency;
    public bool clockFrequencyIsSet;
}


public class ClockNode : Node
{
    public SinkSocket clockFrequencySinkSocket;
    public InputField clockFrequencyField;
    public SourceSocket clockValueSourceSocket;

    private static float defaultClockFrequency = 1.0f;
    private float clockFrequency = defaultClockFrequency;
    private bool clockFrequencyIsSet = false;

    public void Awake()
    {
        Debug.Log("Clock::Awake()");
        if (sinks != null)
            throw new System.ArgumentException();

        sinks = new List<SinkSocket>();
        sinks.Add(clockFrequencySinkSocket);

        sources = new List<SourceSocket>();
        sources.Add(clockValueSourceSocket);
    }


    void Update()
    {
        if(clockValueSourceSocket != null)
            clockValueSourceSocket.SetValue(this, (object)(UnityEngine.Time.time % (1.0f / clockFrequency)));
    }


    public void onDecimalFieldChanged()
    {
        uiController.SetDirty();

        decimalUpdate();
    }


    public void decimalUpdate(WireSpline ws = null)
    {
        try // to catch parse errors
        {
            clockFrequency = defaultClockFrequency;
            clockFrequencyIsSet = false;

            if (clockFrequencyField.text.Length > 0)
            {
                clockFrequency = float.Parse(clockFrequencyField.text);
                clockFrequencyIsSet = true;
            }
        }
        catch (System.ArgumentNullException)
        {
            // swallow it
        }
        catch (System.FormatException e)
        {
            // TODO: must display error in the node
            throw e;
        }
        catch (System.OverflowException e)
        {
            // TODO: must display error in the node
            throw e;
        }
    }


    public override bool RecursionCheck(SourceSocket source)
    {
        // has no sink sockets
        return false;
    }


    public override void OnConnect(SinkSocket sink)
    {
        if (sink == clockFrequencySinkSocket)
            clockFrequencyField.interactable = false;
        else
            throw new System.ArgumentException();
    }


    public override void OnDisconnect(SinkSocket sink)
    {
        if (sink == clockFrequencySinkSocket)
            clockFrequencyField.interactable = true;
        else
            throw new System.ArgumentException();
    }


    public override void ResetInput(SinkSocket sink)
    {
        if (sink == clockFrequencySinkSocket)
        {
            clockFrequency = defaultClockFrequency;
            clockFrequencyField.text = defaultClockFrequency.ToString("R");
        }
        else
            throw new System.ArgumentException();
    }

    
    // generic type
    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        float inputted_value;

        if (writer != clockFrequencySinkSocket)
            throw new System.ArgumentException();

        if (x is int)
        {
            inputted_value = (float)(int)x;
        }
        else if (x is float)
        {
            inputted_value = (float)x;
        }
        else if (x is string)
        {
            inputted_value = float.Parse((string)x);
        }
        else
            throw new System.ArgumentException(this.GetType().Name + " was provided a value of type " + x.GetType().FullName + " but doesn't know what to do with it.");

        if (writer == clockFrequencySinkSocket)
        {
            clockFrequency = inputted_value;
            clockFrequencyField.text = inputted_value.ToString("R");
        }
    }


    public override System.Object GetSaveData()
    {
        ClockNodeSaveData cnsd = new ClockNodeSaveData();
        cnsd.clockFrequency = clockFrequency;
        cnsd.clockFrequencyIsSet = clockFrequencyIsSet;
        return cnsd;
    }


    public override void SetSaveData(System.Object saveData)
    {
        ClockNodeSaveData cnsd = (ClockNodeSaveData)saveData;

        clockFrequencyIsSet = cnsd.clockFrequencyIsSet;
        if (clockFrequencyIsSet)
        {
            clockFrequency = cnsd.clockFrequency;
            clockFrequencyField.text = clockFrequency.ToString("R");
        }
        else
        {
            clockFrequency = defaultClockFrequency;
            clockFrequencyField.text = "";
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
