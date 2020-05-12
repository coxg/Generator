using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Jump : Ability
    {
        public Jump() : base(
            "Jump",
            animation: new Animation(
                startFrames: new Frames(
                    baseOffsets: new List<Vector3>
                    {
                        Vector3.Zero,
                        new Vector3(0, 0, 10f),
                        Vector3.Zero
                    },
                    duration: 30f))) { }

        public override void Start()
        {
            SourceObject.Say("Wahoo!");
        }
    }
}
