using UnityEngine;
using UnityEngine.SceneManagement;
using PussInHell.Player;

namespace PussInHell.Core
{
    public static class PlayerControlLock
    {
        private static int lockCount;

        public static bool IsLocked => lockCount > 0;

        public static void Lock()
        {
            lockCount++;
            if (lockCount == 1)
                Apply(false);
        }

        public static void Unlock()
        {
            if (lockCount == 0) return;

            lockCount--;
            if (lockCount == 0)
                Apply(true);
        }

        public static void ForceUnlock()
        {
            lockCount = 0;
            Apply(true);
        }

        private static void Apply(bool enabled)
        {
            foreach (var motor in Object.FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None))
            {
                if (!enabled)
                    motor.Stop();

                motor.enabled = enabled;

                var view = motor.GetComponentInChildren<PlayerAnimationView>(true);
                if (view != null && !enabled)
                    view.ResetLocomotion();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            lockCount = 0;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                lockCount = 0;
        }
    }
}
