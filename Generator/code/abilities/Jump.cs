using System;
using System.Collections.Generic;
using Generator.code.objects;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Jump : Ability
    {
        public Jump() : base("Jump") { }

        public override void Start()
        {
            if (SourceObject.Position.Z == 0)
            {
                SourceObject.Say("Wahoo!");
                SourceObject.Velocity.Z += 20;
            }
        }
    }
}
