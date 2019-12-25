using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Sprint : Ability
    {
        public Sprint() : base(
            "Sprint",
            isChanneled: true,
            requiresWalking: true,
            animation: new Animation(
                updateFrames: new Frames(
                    baseOffsets: new List<Vector3>
                    {
                        Vector3.Zero,
                        new Vector3(0, 0, .1f),
                        Vector3.Zero
                    },
                    duration: 3f))) { }

        public override void Start()
        {
            SourceObject.Speed.Multiplier *= 3;
            SourceObject.IsWalking = true;
            SourceObject.Say("I'm sprinting!!!");
        }

        public override void Stop()
        {
            SourceObject.Speed.Multiplier /= 3;
            SourceObject.IsWalking = false;
        }
    }
}
