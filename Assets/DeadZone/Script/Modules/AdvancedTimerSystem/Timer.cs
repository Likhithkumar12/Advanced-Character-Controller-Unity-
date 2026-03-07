using System;
using UnityEngine;

namespace ImprovedTimers
{
    public abstract class Timer:IDisposable
    { 
        
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private  set; }
        protected float initialTime;
        
        public float Progress =>Mathf.Clamp(CurrentTime /initialTime, 0f, 1f);
        
        public Action OnTimerStarted = delegate { };
        public Action OnTimerStopped = delegate { };

        protected Timer(float value)
        {
            initialTime = value;
        }

        public void Start()
        {
            CurrentTime=initialTime;
            if(!IsRunning)
            {
                IsRunning=true;
                TimerManager.RegisterTimer(this);
                OnTimerStarted.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                TimerManager.UnregisterTimer(this);
                OnTimerStopped.Invoke();
            }
        }
        
        public abstract void Tick();
        public abstract bool IsFinished {get;  }
        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;
        
        public virtual void Reset()=> CurrentTime=initialTime;

        public virtual void Reset(float newTime)
        {
            CurrentTime=newTime;
            Reset();
        }

        private bool disposed;
        ~Timer() {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposed) return;
            if (disposing)
            {
                TimerManager.UnregisterTimer(this);
            }

            disposed = true;

        }
    }
}