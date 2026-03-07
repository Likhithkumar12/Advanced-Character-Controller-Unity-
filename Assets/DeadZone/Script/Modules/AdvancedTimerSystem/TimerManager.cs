using System.Collections.Generic;

namespace ImprovedTimers
{
    public static class TimerManager
    {
        static readonly List<Timer> timers = new List<Timer>();
        public static void RegisterTimer(Timer timer)=> timers.Add(timer);
        public static void UnregisterTimer(Timer timer)=> timers.Remove(timer);

        public static void UpdateTimers()
        {
            for (int i = timers.Count - 1; i >= 0; i--)
            {
                timers[i].Tick();
            }
            
        }
        public static void Clear()=> timers.Clear();
        
    }
}