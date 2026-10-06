using UnityEngine;

namespace PussInHell.Core
{
    public static class PlayerLocator
    {
        public const string Tag = "Player";

        private static Transform cached;

        public static Transform Player
        {
            get
            {
                if (cached == null)
                {
                    var motor = Object.FindFirstObjectByType<Player.PlayerMotor>();
                    if (motor != null) cached = motor.transform;
                    else
                    {
                        var go = GameObject.FindGameObjectWithTag(Tag);
                        cached = go != null ? go.transform : null;
                    }
                }
                return cached;
            }
        }

        public static bool TryGetDistance(Vector3 from, out float distance)
        {
            var player = Player;
            if (player == null)
            {
                distance = float.PositiveInfinity;
                return false;
            }
            distance = Vector3.Distance(from, player.position);
            return true;
        }
    }
}
