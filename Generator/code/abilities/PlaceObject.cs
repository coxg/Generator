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

        public override void Start()
        {
            var randomBaseTile = Globals.Zone.TileManager.TileSheet.GetRandomBaseTileId(Globals.CreativeObjectIndex);
            Vector3 targetPosition;
            if (SourceObject == Globals.Player && !Input.ControllerState.IsConnected)
            {
                targetPosition = Input.CursorPosition;
            }
            else
            {
                targetPosition = SourceObject.GetTargetCoordinates();
            }
            Globals.Zone.TileManager.Set((int)targetPosition.X, (int)targetPosition.Y, randomBaseTile);
        }
    }
}
