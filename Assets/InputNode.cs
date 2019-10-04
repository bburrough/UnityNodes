using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;


[Serializable]
public class InputNodeSaveData : System.Object
{
    public float decimalValue;
    public bool decimalValueIsSet;
}


public class InputNode : Node {

    //public InputField integerField;
    public InputField decimalField;

    private static float defaultDecimalValue = 0.0f;

    private float decimalValue = defaultDecimalValue;

    private bool decimalValueIsSet = false;

    //public SourceSocket integerSocket;
    public SourceSocket decimalSocket;


    public void Awake()
    {
        sources = new List<SourceSocket>();
        sources.Add(decimalSocket);

        // no sinks
    }


    /*
    public void onIntegerFieldChanged()
    {
        integerUpdate();
    }


    public void integerUpdate(WireSpline ws = null)
    {
        try // to catch parse errors
        {
            int result = 0;

            if (integerField.text.Length > 0)
            {
                result = int.Parse(integerField.text);

                integerSocket.SetValue(this, result, ws);
            }
            else
                integerSocket.ResetInput();
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
    */


    public void onDecimalFieldChanged()
    {
        uiController.SetDirty();

        decimalUpdate();
    }


    public void decimalUpdate(WireSpline ws = null)
    {
        try // to catch parse errors
        {
            decimalValue = defaultDecimalValue;
            decimalValueIsSet = false;

            if (decimalField.text.Length > 0)
            {
                decimalValue = float.Parse(decimalField.text);
                decimalValueIsSet = true;

                decimalSocket.SetValue(this, decimalValue, ws);
            }
            else
                decimalSocket.ResetInput();
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

    public override void SetValue(int x, SinkSocket writer, Node originator)
    {
        throw new NotImplementedException();
    }


    public override void SetValue(float x, SinkSocket writer, Node originator)
    {
        throw new NotImplementedException();
    }


    public override void SetValue(string x, SinkSocket writer, Node originator)
    {
        throw new NotImplementedException();
    }


    // generic type
    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        throw new NotImplementedException();
    }


    public override bool RecursionCheck(SourceSocket source)
    {
        // has no sink sockets
        return false;
    }


    public override void ResetInput(SinkSocket sink)
    {
        // has no sink sockets
        throw new NotImplementedException();
    }


    public override void OnConnect(SourceSocket source, WireSpline ws)
    {
        /* if (source == integerSocket)
            onIntegerFieldChanged();
        else */
        if (source == decimalSocket)
            onDecimalFieldChanged();
        else
            throw new System.ArgumentException("OnConnect was called on input node for an unrecognized SourceSokcet.");
    }


    public override System.Object GetSaveData()
    {
        InputNodeSaveData insd = new InputNodeSaveData();
        insd.decimalValue = decimalValue;
        insd.decimalValueIsSet = decimalValueIsSet;
        return insd;
    }


    public override void SetSaveData(System.Object saveData)
    {
        InputNodeSaveData insd = (InputNodeSaveData)saveData;

        decimalValueIsSet = insd.decimalValueIsSet;
        if (decimalValueIsSet)
        {
            decimalValue = insd.decimalValue;
            decimalField.text = decimalValue.ToString("R");
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
