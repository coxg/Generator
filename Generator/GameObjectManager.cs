using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class GameObjectManager : Manager<GameObject>
    {
        public HashSet<string> ActiveGameObjects = new HashSet<string>();

        public GameObjectManager()
        {
            // Set the name
            Name = "GameObjects";

            // Create the objects - default comes first
            AddNewObject("", null);

            AddNewObject("Niels", new GameObject(
                x: 50f, y: 50f, stamina: 100, strength: 10, speed: 10, perception: 10, 
                name: "Niels", partyNumber: 0, 
                brightness: new Vector3(.5f, .25f, .5f),
                weapon: new Weapon(
                    name: "Sword",
                    type: "Cut",
                    damage: 10)));

            AddNewObject("angry terrain", new GameObject(
                spriteFile: "Sprites/angry_boy", x: 55, y: 56, name: "angry terrain", 
                brightness: Vector3.One));
            var terrain1 = ObjectFromName["angry terrain"];
            terrain1.Activate = delegate
            {
                if (Get(60, 60) == null)
                {
                    terrain1.Say("Check it out, I do something weird!");
                    terrain1.Say("Did you see how weird that was?!");
                    ObjectFromName["big terrain"].AddToGrid();
                }
                else
                {
                    terrain1.Say("There's already a boy!");
                }
            };

            AddNewObject("medium terrain", new GameObject(
                width: 2, height: 2, x: 55, y: 59, name: "medium terrain"));

            AddNewObject("big terrain", new GameObject(
                width: 5, length: 1, height: 5, x: 60, y: 60, name: "big terrain"));
            var terrain3 = ObjectFromName["big terrain"];
            terrain3.Activate = delegate
            {
                terrain3.Say("I don't do anything weird.");
                terrain3.Say("...I'm just really fat.");
            };

            // Populate the Acres
            PopulateAcres();
        }
    }
}