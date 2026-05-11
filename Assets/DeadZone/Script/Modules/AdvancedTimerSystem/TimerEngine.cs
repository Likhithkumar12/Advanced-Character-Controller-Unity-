using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UntiyUtils.LowLevel;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ImprovedTimers
{
    internal static class TimerBootStraper
    {
        private static PlayerLoopSystem timerSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem currentPlayerloop = PlayerLoop.GetCurrentPlayerLoop();
            if (!InsertTimerManager<Update>(ref currentPlayerloop, 0))
            {
                Debug.LogWarning("Timer System not Initialized, failed to insert into PlayerLoop");
                return;
            }
            PlayerLoop.SetPlayerLoop(currentPlayerloop);
            PlayerLoopUtils.PrintPlayerLoop(currentPlayerloop);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerLoopSystem currentPlayerloop = PlayerLoop.GetCurrentPlayerLoop();
                RemoveTimerManager<Update>(ref currentPlayerloop);
                PlayerLoop.SetPlayerLoop(currentPlayerloop);
                TimerManager.Clear();
            }
        }
#endif

        static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopUtils.RemoveSystem<T>(ref loop, timerSystem);
        }

        static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            timerSystem = new PlayerLoopSystem()
            {
                type = typeof(TimerManager),
                updateDelegate = TimerManager.UpdateTimers,
                subSystemList = null
            };
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }
    }
}