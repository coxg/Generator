using System;
using System.Collections.Generic;
using Generator.code.objects;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Blink : Ability
    {
        public Blink() : base("Blink") { }

        public override void Start()
        {
            var targetPosition = SourceObject.GetTargetCoordinates(10);
            SourceObject.Position = targetPosition;
            SourceObject.MovementTarget = null;
        }
    }
}
