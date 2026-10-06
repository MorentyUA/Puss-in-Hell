using UnityEngine;

namespace PussInHell.Core
{
    public static class CutsceneState
    {
        private static int menuBlockedUntilFrame = -1;

        public static bool IsPlaying { get; set; }

        public static bool IsMenuBlocked => Time.frameCount <= menuBlockedUntilFrame;

        public static void BlockMenuForOneFrame()
        {
            menuBlockedUntilFrame = Time.frameCount + 1;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            IsPlaying = false;
            menuBlockedUntilFrame = -1;
        }
    }
}
