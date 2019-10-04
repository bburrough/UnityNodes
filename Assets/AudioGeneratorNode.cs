using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class AudioGeneratorNodeSaveData : System.Object
{
    public float frequency;
    public bool frequencyIsSet;
    public float amplitude;
    public bool amplitudeIsSet;

}


public class AudioGeneratorNode : Node {

    public int position = 0;
    public int samplerate = 44100;

    // TODO: This needs isSet.
    private static float defaultFrequency = 440f;
    private static float defaultAmplitude = 1.0f;

    private float frequency = defaultFrequency;
    private float amplitude = defaultAmplitude;
    private bool frequencyIsSet = false;
    private bool amplitudeIsSet = false;

    public InputField amplitudeField;
    public InputField frequencyField;

    public SinkSocket amplitudeSinkSocket;
    public SinkSocket frequencySinkSocket;


    void Start()
    {
        AudioClip myClip = AudioClip.Create("MySinusoid", samplerate * 2, 1, samplerate, true, OnAudioRead, OnAudioSetPosition);
        AudioSource aud = GetComponent<AudioSource>();
        aud.clip = myClip;
        aud.Play();
    }


    public void Awake()
    {
        sinks = new List<SinkSocket>();
        sinks.Add(amplitudeSinkSocket);
        sinks.Add(frequencySinkSocket);

        // no sources


    }


    void OnAudioRead(float[] data)
    {
        int count = 0;
        while (count < data.Length)
        {
            data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate) * amplitude;
            position++;
            count++;
        }
    }


    void OnAudioSetPosition(int newPosition)
    {
        position = newPosition;
    }

    public override bool RecursionCheck(SourceSocket source)
    {
        return false;
    }


    public override void ResetInput(SinkSocket sink)
    {

    }

    public void onAmplitudeFieldChanged()
    {
        uiController.SetDirty();

        amplitude = float.Parse(amplitudeField.text);
    }


    public void onFrequencyFieldChanged()
    {
        uiController.SetDirty();

        frequency = float.Parse(frequencyField.text);
    }


    public override System.Object GetSaveData()
    {
        AudioGeneratorNodeSaveData agnsd = new AudioGeneratorNodeSaveData();
        agnsd.amplitude = amplitude;
        agnsd.amplitudeIsSet = amplitudeIsSet;
        agnsd.frequency = frequency;
        agnsd.frequencyIsSet = frequencyIsSet;
        return agnsd;
    }


    public override void SetSaveData(System.Object saveData)
    {
        AudioGeneratorNodeSaveData agnsd = (AudioGeneratorNodeSaveData)saveData;

        frequencyIsSet = agnsd.frequencyIsSet;
        if (frequencyIsSet)
        {
            frequency = agnsd.frequency;
            frequencyField.text = frequency.ToString("R");
        }
        else
        {
            frequency = defaultFrequency;
            frequencyField.text = "";
        }

        amplitudeIsSet = agnsd.amplitudeIsSet;
        if (amplitudeIsSet)
        {
            amplitude = agnsd.amplitude;
            amplitudeField.text = amplitude.ToString("R");
        }
        else
        {
            amplitude = defaultAmplitude;
            amplitudeField.text = "";
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
