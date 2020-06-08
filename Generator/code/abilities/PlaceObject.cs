using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class PlaceObject : Ability
    {
        public PlaceObject() : base(
            "PlaceObject",
            cooldown: 0,
            keepCasting: true) { }

        private int? lastTargetX;
        private int? lastTargetY;
        
        public override void Start()
        {
            // Lets us use the cursor for targetting
            Vector3 targetPosition;
            if (SourceObject == Globals.Player && !Input.ControllerState.IsConnected)
            {
                targetPosition = Input.CursorPosition;
            }
            else
            {
                targetPosition = SourceObject.GetTargetCoordinates();
            }

            // Prevents us from setting the same tile 60 times per second
            var targetX = (int) targetPosition.X;
            var targetY = (int) targetPosition.Y;
            if (lastTargetX != targetX || lastTargetY != targetY)
            {
                var randomBaseTile = Globals.TileManager.TileSheet.Tiles[Globals.CreativeObjectIndex].GetRandomBaseId();
                Globals.TileManager.Set(targetX, targetY, randomBaseTile);
            }
            lastTargetX = targetX;
            lastTargetY = targetY;
        }
    }
}
