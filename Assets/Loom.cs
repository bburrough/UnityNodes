using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class Loom:MonoBehaviour
{
	public static int maxThreads = 8;
	static int numThreads;
	
	private static Loom _current;
	public static Loom Current
	{
		get
		{
			Initialize();
			return _current;
		}
	}
	
	void Awake()
	{
		_current = this;
		initialized = true;
	}
	
	static bool initialized;
	
	static void Initialize()
	{
		if (!initialized)
		{
			if(!Application.isPlaying)
				return;
			initialized = true;
			var g = new GameObject("Loom");
			_current = g.AddComponent<Loom>();
		}
	}
	
	private List<Action> _actions = new List<Action>();
	/*
    public struct DelayedQueueItem
	{
		public float time;
		public Action action;
	}
	private List<DelayedQueueItem> _delayed = new  List<DelayedQueueItem>();	
	List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
    */
	
	/*
    public static void QueueOnMainThread(Action action)
	{
		if (Current)
		{
			QueueOnMainThread(action, 0f);
		}
	}
    */

    public static void QueueOnMainThread(Action action)
    {
        lock (Current._actions)
        {
            Current._actions.Add(action);
        }
    }

    /*
    public static void QueueOnMainThread(Action action, float time)
	{
		if(time != 0)
		{
			lock(Current._delayed)
			{
				Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action});
			}
		}
		else
		{
			lock (Current._actions)
			{
				Current._actions.Add(action);
			}
		}
	}
    */


    public static Thread RunAsync(Action a)
	{
		Initialize();
		while(numThreads >= maxThreads)
		{
			Thread.Sleep(1);
		}
		Interlocked.Increment(ref numThreads);
		ThreadPool.QueueUserWorkItem(RunAction, a);
		return null;
	}
	

	private static void RunAction(object action)
	{
		try
		{
			((Action)action)();
		}
		catch
		{
		}
		finally
		{
			Interlocked.Decrement(ref numThreads);
		}		
	}
	
	
	void OnDisable()
	{
		if (_current == this)
		{
			
			_current = null;
		}
	}
	
	
	void Update()
	{
        List<Action> _currentActions = null;
        lock (_actions)
		{
            UnityEngine.Profiling.Profiler.BeginSample("Loom::Update_ActionsCopy");
            if (_actions.Count <= 0)
            {
                UnityEngine.Profiling.Profiler.EndSample();
                return;
            }
            _currentActions = new List<Action>(_actions);
			_actions.Clear();
            UnityEngine.Profiling.Profiler.EndSample();
        }
        UnityEngine.Profiling.Profiler.BeginSample("Loom::Update_ActionsExec");
        for (int i = 0; i < _currentActions.Count; i++)
		{
			_currentActions[i]();
		}
        UnityEngine.Profiling.Profiler.EndSample();
    }
}