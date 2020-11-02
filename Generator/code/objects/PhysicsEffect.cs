using System;
using Microsoft.Xna.Framework;

namespace Generator.code.objects
{
    public class PhysicsEffect
    {
        public Vector3 Force;
        public float? RemainingDuration;  // null is permanent

        public PhysicsEffect(Vector3 force, float? duration = null)
        {
            Force = force;
            RemainingDuration = duration;
        }

        public void Update()
        {
            if (RemainingDuration != null)
            {
                RemainingDuration = Math.Max(0, RemainingDuration.Value - Timing.SecondsPassed);
                if (RemainingDuration == 0)
                {
                    Force = Vector3.Zero;
                }
            }
        }
    }
}