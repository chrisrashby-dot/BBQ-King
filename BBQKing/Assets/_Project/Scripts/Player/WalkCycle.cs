using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>Pure maths for stepping through walk animation frames, tested by EditMode tests.</summary>
    public static class WalkCycle
    {
        /// <summary>
        /// Which frame to show after walking for <paramref name="walkSeconds"/>, looping round.
        /// Returns 0 (the standing frame) if there are no frames or the rate is not positive.
        /// </summary>
        public static int FrameAt(float walkSeconds, float framesPerSecond, int frameCount)
        {
            if (frameCount <= 0 || framesPerSecond <= 0f || walkSeconds <= 0f) return 0;
            int frame = Mathf.FloorToInt(walkSeconds * framesPerSecond);
            return frame % frameCount;
        }
    }
}
