using System;
using System.Collections.Generic;
using Generator.code.objects;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Dash : Ability
    {
        public Dash() : base("Dash") { }

        public override void Start()
        {
            var targetPosition = SourceObject.GetTargetCoordinates();
            var velocityOffset = targetPosition - SourceObject.Center;
            velocityOffset.Z = 0;
            velocityOffset.Normalize();
            velocityOffset *= 100;
            SourceObject.Velocity += velocityOffset;
            SourceObject.MovementTarget = null;
        }
    }
}
