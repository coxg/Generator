using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class GameObjectManager : Manager<GameObject>
    {

        void LogHealth(GameObject gameObject)
        {
            Globals.Log(gameObject.Health);
        }

        void WalkToPlayer(GameObject gameObject)
            // TODO: Move these to their own AI file
            // TODO: Why isn't this just Angle(gameObject, Player)? My angle logic seems fundamentally flawed if that's not the calculation
        {
            gameObject.Direction = -(float)MathTools.Angle(Globals.Player.Position, gameObject.Position) + MathHelper.PiOver2;
            gameObject.MoveInDirection(gameObject.Direction);
        }

        void WalkAwayFromPlayer(GameObject gameObject)
        {
            gameObject.Direction = -(float)MathTools.Angle(gameObject.Position, Globals.Player.Position) + MathHelper.PiOver2;
            gameObject.MoveInDirection(gameObject.Direction);
        }

        public GameObjectManager()
        {
            // Set the name
            Name = "GameObjects";

            AddNewObject("Niels", new GameObject(
                new Vector3(50, 50, 0), stamina: 100, strength: 10, speed: 10, perception: 10,
                name: "Niels", partyNumber: 0,
                weapon: new Weapon(
                    name: "Sword",
                    type: "Cut",
                    damage: 10),
                brightness: Vector3.One));

            AddNewObject("angry terrain", new GameObject(
                new Vector3(55, 56, 0),
                spriteFile: "Sprites/angry_boy", name: "angry terrain", 
                brightness: new Vector3(.5f, .1f, .5f),
                strength: 10, speed: 10, perception: 10,
                ai: WalkToPlayer));
            var terrain1 = ObjectFromName["angry terrain"];
            terrain1.Activate = delegate
            {
                if (Get(60, 60) == null)
                {
                    terrain1.Say("Check it out, I do something weird!");
                    terrain1.Say("Did you see how weird that was?!");

                    AddNewObject("big terrain", new GameObject(
                    new Vector3(60, 60, 0), new Vector3(5, 1, 5), name: "big terrain", strength: 10, speed: 10, perception: 10));
                    var terrain3 = ObjectFromName["big terrain"];
                    terrain3.Activate = delegate
                    {
                        terrain3.Say("I don't do anything weird.");
                        terrain3.Say("...I'm just really fat.");
                    };
                }
                else
                {
                    terrain1.Say("There's already a boy!");
                }
            };

            AddNewObject("medium terrain", new GameObject(
                new Vector3(57, 59, 0), new Vector3(2, 1, 2), name: "medium terrain", strength: 10, speed: 10, perception: 10));

            // Populate the Acres
            PopulateAcres();
        }
    }
}