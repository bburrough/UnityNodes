using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;


[Serializable]
public class MathFunctionNodeSaveData
{
    public int operation;
    public float firstTermValue;
    public bool firstTermValueIsSet;
    public float secondTermValue;
    public bool secondTermValueIsSet;
}

public class MathFunctionNode : Node
{

    public Dropdown operationDropdown;

    public SinkSocket firstTermInputSocket;
    public SinkSocket secondTermInputSocket;

    public InputField firstTermInputField;
    public InputField secondTermInputField;

    public SourceSocket outputSocket;

    private static float defaultTermValue = 0.0f;

    private bool firstTermValueIsSet = false;
    private bool secondTermValueIsSet = false;
    private float firstTermValue = defaultTermValue;
    private float secondTermValue = defaultTermValue;



    public void Awake()
    {
        // TODO: If we use an abstraction layere to resolve sink/source sockets, instead of lists,
        //   then we can better accommodate node-level changes. For example, if a socket is removed,
        //   the abstraction layer can recognize a request for the removed socket and throw up a
        //   notice to the user explaining such.

        sinks = new List<SinkSocket>();
        sinks.Add(firstTermInputSocket);
        sinks.Add(secondTermInputSocket);

        sources = new List<SourceSocket>();
        sources.Add(outputSocket);
    }


    // generic type
    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        float inputted_value;

        if (writer == firstTermInputSocket)
            firstTermValueIsSet = false;
        else if (writer == secondTermInputSocket)
            secondTermValueIsSet = false;
        else
            throw new System.ArgumentException();

        if (x is int)
            inputted_value = (float)(int)x;
        else if (x is float)
            inputted_value = (float)x;
        else if (x is string)
            inputted_value = float.Parse((string)x);
        else
            throw new System.ArgumentException(this.GetType().Name + " was provided a value of type " + x.GetType().FullName + " but doesn't know what to do with it.");

        if (writer == firstTermInputSocket)
        {
            firstTermValue = inputted_value;
            firstTermInputField.text = inputted_value.ToString("R");
            firstTermValueIsSet = true;
        }
        else
        {
            secondTermValue = inputted_value;
            secondTermInputField.text = inputted_value.ToString("R");
            secondTermValueIsSet = true;
        }

        // TODO: only call output if the result actually changed.

        if (firstTermValueIsSet && (secondTermValueIsSet || !secondTermInputField.IsActive()))
            outputSocket.SetValue(this, (object)Calculate());
}


    public override bool RecursionCheck(SourceSocket source)
    {
        return outputSocket.RecursionCheck(source);
    }


    public override void ResetInput(SinkSocket sink)
    {
        if (sink == firstTermInputSocket)
        {
            firstTermValue = defaultTermValue;
            firstTermInputField.text = "";
            firstTermValueIsSet = false;
        }
        else if (sink == secondTermInputSocket)
        {
            secondTermValue = defaultTermValue;
            secondTermInputField.text = "";
            secondTermValueIsSet = false;
        }
        else
            throw new System.ArgumentException();

        outputSocket.ResetInput();
    }


    public float Calculate()
    {
        switch (operationDropdown.value)
        {   // These values are defined by the arithmetic prefab
            case 0:
                return Mathf.Sin(firstTermValue);

            case 1:
                return Mathf.Cos(firstTermValue);

            case 2:
                return Mathf.Tan(firstTermValue);

            case 3:
                return Mathf.Asin(firstTermValue);

            case 4:
                return Mathf.Acos(firstTermValue);

            case 5:
                return Mathf.Atan(firstTermValue);

            case 6:
                return firstTermValue * Mathf.Deg2Rad;

            case 7:
                return firstTermValue * Mathf.Rad2Deg;

            case 8:
                return Mathf.Sign(firstTermValue);

            case 9:
                return Mathf.Round(firstTermValue);

            case 10:
                return Mathf.Pow(firstTermValue, secondTermValue);

            case 11:
                return Mathf.Min(firstTermValue, secondTermValue);

            case 12:
                return Mathf.Max(firstTermValue, secondTermValue);

            case 13:
                return Mathf.Atan2(firstTermValue, secondTermValue);

            case 14:
                return firstTermValue + secondTermValue;

            case 15:
                return firstTermValue - secondTermValue;

            case 16:
                return firstTermValue * secondTermValue;

            case 17:
                return firstTermValue / secondTermValue;

            case 18:
                return firstTermValue % secondTermValue;

            default:
                throw new System.ArgumentException();
        }
    }


    public override void OnConnect(SourceSocket source, WireSpline ws)
    {
        if (firstTermValueIsSet && (secondTermValueIsSet || !secondTermInputField.IsActive()))
            source.SetValue(this, (object)Calculate(), ws);
        //else
        //    source.ResetInput();
    }


    public override void OnConnect(SinkSocket sink)
    {
        if (sink == firstTermInputSocket)
            firstTermInputField.interactable = false;
        else if (sink == secondTermInputSocket)
            secondTermInputField.interactable = false;
        else
            throw new System.ArgumentException();
    }


    public override void OnDisconnect(SinkSocket sink)
    {
        if (sink == firstTermInputSocket)
            firstTermInputField.interactable = true;
        else if (sink == secondTermInputSocket)
            secondTermInputField.interactable = true;
        else
            throw new System.ArgumentException();
    }


    public void onOperationChanged()
    {
        uiController.SetDirty();

        switch (operationDropdown.value)
        {
            // Funtions which take a single argument
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                EnableSecondTermField(false);
                break;

            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
                EnableSecondTermField(true);
                break;

            default:
                throw new System.ArgumentException();
        }

        if (firstTermValueIsSet && (secondTermValueIsSet || !secondTermInputField.IsActive()))
            outputSocket.SetValue(this, (object)Calculate());
        else
            outputSocket.ResetInput();
    }


    private void EnableSecondTermField(bool enable)
    {
        secondTermInputField.gameObject.SetActive(enable);
        secondTermInputSocket.gameObject.SetActive(enable);

        if (!enable)      
            secondTermInputSocket.OnDestroy(); // This doesn't actually destroy the sink socket. It tells the sink socket to destroy any wires that might be attached to it.
    }


    public void onFirstTermFieldChanged()
    {
        if (!firstTermInputField.interactable)
            return;

        uiController.SetDirty();

        try // to catch parse errors
        {
            firstTermValue = defaultTermValue;
            firstTermValueIsSet = false;

            if (firstTermInputField.text.Length > 0)
            {
                firstTermValue = float.Parse(firstTermInputField.text);
                firstTermValueIsSet = true;

                if (firstTermValueIsSet && (secondTermValueIsSet || !secondTermInputField.IsActive()))
                    outputSocket.SetValue(this, (object)Calculate());
            }
            else
                outputSocket.ResetInput();
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


    public void onSecondTermFieldChanged()
    {
        if (!secondTermInputField.interactable)
            return;

        uiController.SetDirty();

        try // to catch parse errors
        {
            secondTermValue = defaultTermValue;
            secondTermValueIsSet = false;

            if (secondTermInputField.text.Length > 0)
            {
                secondTermValue = float.Parse(secondTermInputField.text);
                secondTermValueIsSet = true;

                if (firstTermValueIsSet && (secondTermValueIsSet || !secondTermInputField.IsActive()))
                    outputSocket.SetValue(this, (object)Calculate());
            }
            else
                outputSocket.ResetInput();
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


    public override System.Object GetSaveData()
    {
        MathFunctionNodeSaveData mfnsd = new MathFunctionNodeSaveData();

        mfnsd.operation = operationDropdown.value;
        mfnsd.firstTermValue = firstTermValue;
        mfnsd.firstTermValueIsSet = firstTermValueIsSet;
        mfnsd.secondTermValue = secondTermValue;
        mfnsd.secondTermValueIsSet = secondTermValueIsSet;

        return mfnsd;
    }


    public override void SetSaveData(object saveData)
    {
        MathFunctionNodeSaveData mfnsd = (MathFunctionNodeSaveData)saveData;

        operationDropdown.value = mfnsd.operation;
        firstTermValueIsSet = mfnsd.firstTermValueIsSet;
        if (firstTermValueIsSet)
        {
            firstTermValue = mfnsd.firstTermValue;
            firstTermInputField.text = firstTermValue.ToString("R");
        }
        else
        {
            firstTermValue = defaultTermValue;
            firstTermInputField.text = "";
        }

        secondTermValueIsSet = mfnsd.secondTermValueIsSet;
        if (secondTermValueIsSet)
        {
            secondTermValue = mfnsd.secondTermValue;
            secondTermInputField.text = secondTermValue.ToString("R");
        }
        else
        {
            secondTermValue = defaultTermValue;
            secondTermInputField.text = "";
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
