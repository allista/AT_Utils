//   ComputationBalancer.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AT_Utils
{
    [KSPAddon(KSPAddon.Startup.FlightEditorAndKSC, false)]
    public class ComputationBalancer : MonoBehaviour
    {
        public class Task
        {
            public IEnumerator iter;
            public bool canceled;
            public bool finished;
            public bool error;
            public static implicit operator bool(Task t) => t.finished;
        }

        static ComputationBalancer Instance;
        static bool level_loaded;

        DateTime next_ts = DateTime.MinValue;
        LowPassFilterF fps = new LowPassFilterF();
        LowPassFilterF base_fps = new LowPassFilterF();
        double duration;

        public static float FPS => Instance.fps;
        public static float FPS_AVG => Instance.base_fps;

        List<Task> tasks = new List<Task>();
        int current = 0;

        void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelLoaded);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoad);
        }

        void Start()
        {
            fps.Tau = 1;
            base_fps.Tau = 2;
            tasks = new List<Task>();
            current = 0;
        }

        void OnDestroy() 
        { 
            Instance = null;
            GameEvents.onLevelWasLoadedGUIReady.Remove(onLevelLoaded);
            GameEvents.onGameSceneLoadRequested.Remove(onGameSceneLoad);
        }

        void onLevelLoaded(GameScenes scene) 
        { 
            fps.Reset();
            base_fps.Reset();
            level_loaded = true; 
        }

        void onGameSceneLoad(GameScenes scene) => level_loaded = false;

        public static Task AddTask(IEnumerator task)
        {
            var t = new Task { iter = task };
            Instance.tasks.Add(t);
            return t;
        }

        void Update()
        {
            if(!level_loaded || Time.timeSinceLevelLoad < 1) return;
            var now = DateTime.Now;
            if(fps.Value.Equals(0))
            {
                fps.Set(1 / Time.unscaledDeltaTime);
                base_fps.Set(fps);
            }
            else
            {
                fps.Update(1 / Time.unscaledDeltaTime);
                base_fps.Update(fps);
            }
            //this.Log("tasks {}, unscaled dT {}, fps {}, fps avg {}", 
                     //tasks.Count, Time.unscaledDeltaTime, fps.Value, base_fps.Value);//debug
            if(tasks.Count > 0)
            {
                var dt = Time.unscaledDeltaTime / 100;
                duration = fps >= base_fps ?
                    Utils.ClampH(duration + dt, Time.unscaledDeltaTime / 2) :
                         Utils.ClampL(duration - dt, Time.unscaledDeltaTime / 10);
                //var i = 0;
                var next = now.AddSeconds(duration);
                while(now < next && tasks.Count > 0)
                {
                    var task = tasks[current];
                    if(task.canceled)
                        tasks.RemoveAt(current);
                    else
                    {
                        try
                        {
                            if(!task.iter.MoveNext())
                            {
                                task.finished = true;
                                tasks.RemoveAt(current);
                            }
                            else
                            {
                                current += 1;
                                if(current >= tasks.Count)
                                    current = 0;
                            }
                        }
                        catch(Exception e)
                        {
                            task.finished = true;
                            task.error = true;
                            tasks.RemoveAt(current);
                            Debug.Log(e);
                        }                        
                    }
                    now = DateTime.Now;
                    //i++;
                }
            }
        }
    }
}
