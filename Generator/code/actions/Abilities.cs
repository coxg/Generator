using System;
using System.Collections.Generic;

namespace Generator
{
    public static class Abilities
    {
        private static Dictionary<String, Ability> AbilityMap = new Dictionary<String, Ability>();

        public static Ability get(String Name)
        {
            return AbilityMap[Name];
        }

        public static void set(Ability ability)
        {
            AbilityMap[ability.Name] = ability;
        }
        
        public static Ability Move = new Ability(
            "Move to the specified area",
            Type.Untyped,
            range: 1,
            castTime: .25f);

        public static Ability Thwack = new Ability(
            "Give 'em a thwack!",
            Type.Physical,
            damage: 1,
            castTime: 1);

        public static Ability Blink = new Ability(
            "Poof on out of there!",
            Type.Untyped,
            range: 5,
            recharge: 1,
            cooldown: 5,
            locationEffect: (source, targetPos) =>
            {
                // TODO: What if there's already something there?
                source.Position = targetPos;
            });
        
        public static Ability PlaceObject = new Ability(
            "DEV USE ONLY",
            Type.Untyped,
            range: 10,
            cooldown: .05f,
            locationEffect: (source, targetPos) =>
            {
                var x = (int) targetPos.X;
                var y = (int) targetPos.Y;
                var selectedTile = Globals.TileManager.TileSheet.Tiles[Globals.CreativeObjectIndex];
                var randomBaseTile = selectedTile.GetRandomBaseId();
                Globals.TileManager.Set(x, y, randomBaseTile);
                Globals.Log("Placing " + selectedTile.Name + " at " + x + ", " + y);
            });

        public static Ability Shoot = new Ability(
            "Pew pew!",
            Type.Physical,
            damage: 1,
            range: 10,
            castTime: 1);
    }
}