using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

[Serializable]
public class ArithmeticNodeSaveData : System.Object
{
    public int operation;
    public float firstTermValue;
    public bool firstTermValueIsSet;
    public float secondTermValue;
    public bool secondTermValueIsSet;
}


public class ArithmeticNode : Node
{
    /*
        For serialization, store the dropdown state

        ---

        spitballing. The possible data are:

        - operation
        - firstTermValue
        - secondTermValue

        How to do we deal with storage
        of wire splines?

        Option 1:

        Store list of all wiresplines with
        indexes referring to a list of nodes.
        Also store a second index representing
        the index number of the socket on each
        node.

        Option 2:

        Store a list of sockets, and each wirespline
        refers to an index in that list.  If we do
        this, we also need to associate the socket
        to the node.
    
        
    */

    public Dropdown operationDropdown;

    public SinkSocket firstTermInputSocket;
    public SinkSocket secondTermInputSocket;

    public InputField firstTermInputField;
    public InputField secondTermInputField;

    public SourceSocket outputSocket;

    private static float defaultTermValue = 0.0f;

    private float firstTermValue = defaultTermValue;
    private float secondTermValue = defaultTermValue;
    private bool firstTermValueIsSet = false;
    private bool secondTermValueIsSet = false;


    public void Awake()
    {
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
        {
            firstTermValue = defaultTermValue;
            firstTermValueIsSet = false;
        }
        else if(writer == secondTermInputSocket)
        {
            secondTermValue = defaultTermValue;
            secondTermValueIsSet = false;
        }
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

        if (firstTermValueIsSet && secondTermValueIsSet)
            outputSocket.SetValue(this, (object)Calculate());
        else
            outputSocket.ResetInput();
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
        switch(operationDropdown.value)
        {   // These values are defined by the arithmetic prefab
            case 0:
                // add
                return firstTermValue + secondTermValue;

            case 1:
                // subtract
                return firstTermValue - secondTermValue;

            case 2:
                // multiply
                return firstTermValue * secondTermValue;

            case 3:
                //divide
                return firstTermValue / secondTermValue;

            case 4:
                //modulo
                return firstTermValue % secondTermValue;

            default:
                throw new System.ArgumentException();
        }
    }


    public override void OnConnect(SourceSocket source, WireSpline ws)
    {
        if(firstTermValueIsSet && secondTermValueIsSet)
            source.SetValue(this, (object)Calculate(), ws);
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

        if (firstTermValueIsSet && secondTermValueIsSet)
            outputSocket.SetValue(this, (object)Calculate());
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

                if(firstTermValueIsSet && secondTermValueIsSet)
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

                if (firstTermValueIsSet && secondTermValueIsSet)
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
        ArithmeticNodeSaveData ansd = new ArithmeticNodeSaveData();
        ansd.operation = operationDropdown.value;
        ansd.firstTermValue = firstTermValue;
        ansd.firstTermValueIsSet = firstTermValueIsSet;
        ansd.secondTermValue = secondTermValue;
        ansd.secondTermValueIsSet = secondTermValueIsSet;
        return ansd;
    }


    public override void SetSaveData(System.Object saveData)
    {
        ArithmeticNodeSaveData ansd = (ArithmeticNodeSaveData)saveData;
        operationDropdown.value = ansd.operation;
        firstTermValueIsSet = ansd.firstTermValueIsSet;
        if (firstTermValueIsSet)
        {
            firstTermValue = ansd.firstTermValue;
            firstTermInputField.text = firstTermValue.ToString("R");
        }
        else
        {
            firstTermValue = defaultTermValue;
            firstTermInputField.text = "";
        }


        secondTermValueIsSet = ansd.secondTermValueIsSet;
        if (secondTermValueIsSet)
        {
            secondTermValue = ansd.secondTermValue;
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
