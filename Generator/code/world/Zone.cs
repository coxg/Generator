using System;
using System.Collections.Generic;
using Generator.code.objects;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class Zone
    {
        public string Name;
        public int Width;
        public int Height;
        public float Gravity;
        public Vector3 Wind;

        public Zone(string name, int width, int height, float gravity = 9.8f, Vector3? wind = null)
        {
            Name = name;
            Width = width;
            Height = height;
            Gravity = gravity;
            Wind = wind ?? Vector3.Zero;
        }

        public static void Enter(string name)
        // Move to a new zone, taking the party objects with you
        {
            // Remove party from zone before serializing
            var partyMembers = new List<GameObject>();
            foreach (var memberId in (List<String>) Globals.Copy(Globals.Party.Value.MemberIDs))
            {
                var partyMemeber = Globals.GameObjectManager.Get(memberId);
                partyMembers.Add(partyMemeber);
                Globals.GameObjectManager.Remove(partyMemeber);
            }
            
            // Serialize
            Saving.CurrentSaveDirectory = Saving.BaseSaveDirectory + "tmp";
            Globals.Log("Saving to " + Saving.CurrentSaveDirectory);
            Saving.SaveAreaToDisk();

            // Set the new zone
            Saving.LoadAreaFromDisk(name);

            // Add the party to the new zone
            foreach (var partyMember in partyMembers)
            {
                Globals.GameObjectManager.Set(partyMember);
            }
        }

        public static void Initialize(string name)
        // TODO: Something better than this!
        {
            switch (name)
            {
                case "testingZone":
                    Globals.Zone = new Zone("testingZone", 500, 500);
                    Globals.GameObjectManager = new GameObjectManager(new List<GameObject>
                    {
                        GameObjects.Niels,
                        GameObjects.Farrah,
                        GameObjects.OldMan,
                        GameObjects.Building,
                        GameObjects.BadGuy
                    });
                    Globals.TileManager = new TileManager(Globals.DefaultTileSheet, 1);
                    break;

                case "buildings":
                    Globals.Zone = new Zone("buildings", 100, 100);
                    Globals.GameObjectManager = new GameObjectManager(new List<GameObject>
                    {
                        GameObjects.Building2
                    });
                    Globals.TileManager = new TileManager(Globals.DefaultTileSheet);
                    break;

                default:
                    throw new KeyNotFoundException(name);
            }
        }
    }
}
