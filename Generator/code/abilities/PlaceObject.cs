using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class PlaceObject : Ability
    {
        public PlaceObject() : base(
            "PlaceObject",
            cooldown: .1f,
            keepCasting: true) { }

        public override void Start()
        {
            /*var baseTileName = Globals.Zone.Tiles.BaseTileNames[Globals.CreativeObjectIndex];
            var randomBaseTile = Globals.Zone.Tiles.GetRandomBaseName(baseTileName);
            var targetCoordinates = SourceObject.GetTargetCoordinates(1);
            Globals.Zone.Tiles.IDs[(int)targetCoordinates.X, (int)targetCoordinates.Y] = randomBaseTile;*/
        }
    }
}
