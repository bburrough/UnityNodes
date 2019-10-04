using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if true
// Array-based implementation
public class TimeSeriesNode : Node
{

    public MeshFilter meshFilter;

    public SinkSocket valueSocket;

    private float penPosition = 0.0f;

    const int array_size = 2048;

    private Vector3[] _vertices = new Vector3[array_size];
    private int[] _indices = new int[array_size];
    private int _localVertexCount = 0;
    private int _seekPosition = 0;


    private void ResetArrays()
    {
        for (int i = 0; i < array_size; i++)
        {
            _vertices[i] = Vector3.zero;
            _indices[i] = i;
        }
        _localVertexCount = 0;
        _seekPosition = 0;
    }


    public void Awake()
    {
        if (sinks != null)
            throw new System.ArgumentException();

        sinks = new List<SinkSocket>();
        sinks.Add(valueSocket);
        
        // no sources
    }


    // Use this for initialization
    void Start()
    {
        ResetArrays();
        meshFilter.mesh.vertices = _vertices;
        meshFilter.mesh.SetIndices(_indices, MeshTopology.Lines, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (_localVertexCount > 0)
        {
            // shift all positions left by the amount of time that has elapsed since the last frame.
            Mesh mesh = meshFilter.mesh;
            float distanceTravelled = UnityEngine.Time.deltaTime;
            Vector3 offset_vector = new Vector3(-distanceTravelled, 0.0f, 0.0f);

            int begin = ((_seekPosition - _localVertexCount) + array_size) % array_size;
            for (int i = 0; i < _localVertexCount - 1; i++) // minus one because we are intentionally not shifting the right-most vertex.
            {
                int current_index = (begin + i) % array_size;
                _vertices[current_index] += offset_vector;
            }

            mesh.vertices = _vertices;
        }
    }


    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        if (x is float)
        {
            penPosition = (float)x;

            if (penPosition > 10.0f || penPosition < -10.0f)
                Debug.LogWarning("here");


            Vector3 pen_position_vector = new Vector3(0.0f, penPosition, 0.0f);
            if (_localVertexCount == 0)
                _vertices[_seekPosition] = pen_position_vector;
            else
                _vertices[_seekPosition] = _vertices[(_seekPosition - 1 + array_size) % array_size];

            _localVertexCount++;

            _vertices[_seekPosition + 1] = pen_position_vector;
            _localVertexCount++;

            meshFilter.mesh.vertices = _vertices;

            _seekPosition = (_seekPosition + 2) % array_size;
            if (_localVertexCount > array_size)
                _localVertexCount = array_size;
        }
        else
            throw new System.ArgumentException(this.GetType().Name + " was provided a value of type " + x.GetType().FullName + " but doesn't know what to do with it.");
    }


    public override bool RecursionCheck(SourceSocket source)
    {
        return false;
    }


    public override void ResetInput(SinkSocket sink)
    {
        ResetArrays();
        meshFilter.mesh.vertices = _vertices;
    }


    public override System.Object GetSaveData()
    {
        return null;
    }


    public override void SetSaveData(object saveData)
    {
    }
}

#else
// List-based implementation
public class TimeSeriesNode : Node {

    public MeshFilter meshFilter;

    SinkSocket valueSocket;

    private float penPosition = 0.0f;

    private List<Vector3> vertex_list = new List<Vector3>();
    private List<int> index_list = new List<int>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        // shift all positions left by the amount of time that has elapsed since the last frame.
        Mesh mesh = meshFilter.mesh;

        float distanceTravelled = UnityEngine.Time.deltaTime;

        Vector3 offset_vector = new Vector3(-distanceTravelled, 0.0f, 0.0f);

        for(int i = 0; i < vertex_list.Count - 1; i++) // minus one because we are intentionally not shifting the right most vertex.
            vertex_list[i] += offset_vector;

        mesh.vertices = vertex_list.ToArray();
	}


    public override void SetValue(object x, SinkSocket writer, Node originator)
    {
        if(x is float)
        {
            penPosition = (float)x;

            Vector3 pen_position_vector = new Vector3(0.0f, penPosition, 0.0f);
            if (vertex_list.Count == 0)
                vertex_list.Add(pen_position_vector);
            else
                vertex_list.Add(vertex_list[vertex_list.Count - 1]);

            vertex_list.Add(pen_position_vector);

            index_list.Add(vertex_list.Count - 2);
            index_list.Add(vertex_list.Count - 1);

            Mesh mesh = meshFilter.mesh;
            mesh.vertices = vertex_list.ToArray();
            mesh.SetIndices(index_list.ToArray(), MeshTopology.Lines, 0);
        }
        else
            throw new System.ArgumentException(this.GetType().Name + " was provided a value of type " + x.GetType().FullName + " but doesn't know what to do with it.");
    }


    public override bool RecursionCheck(SourceSocket source)
    {
        return false;
    }


    public override void ResetInput(SinkSocket sink)
    {
        meshFilter.mesh.Clear();
        vertex_list.Clear();
        index_list.Clear();
    }
}
#endif

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
