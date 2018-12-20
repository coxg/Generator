﻿using System.Collections.Generic;

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
                name: "Niels", partyNumber: 0, weapon: new Weapon(
                    name: "Sword",
                    type: "Cut",
                    damage: 10)));

            AddNewObject("angry terrain", new GameObject(
                spriteFile: "Sprites/angry_boy", x: 55, y: 56, name: "angry terrain"));
            var terrain1 = NameToObject["angry terrain"];
            terrain1.Activate = delegate
            {
                if (Get(60, 60) == null)
                {
                    terrain1.Say("Check it out, I do something weird!");
                    terrain1.Say("Did you see how weird that was?!");
                    NameToObject["big terrain"].AddToGrid();
                }
                else
                {
                    terrain1.Say("There's already a boy!");
                }
            };

            AddNewObject("medium terrain", new GameObject(
                width: 2, height: 2, x: 55, y: 59, name: "medium terrain"));

            AddNewObject("big terrain", new GameObject(width: 5, length: 1, height: 5, x: 60, y: 60, name: "big terrain"));
            var terrain3 = NameToObject["big terrain"];
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