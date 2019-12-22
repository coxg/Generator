using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Generator
{
    public class GameObjectManager : Manager<GameObject>
    {
        public GameObjectManager(List<GameObject> objects)
        {
            foreach (var gameObject in objects)
            {
                Objects[gameObject.ID] = gameObject;
            }
        }

        [JsonConstructor]
        public GameObjectManager(Dictionary<string, GameObject> objects)
        {
            Objects = objects;
        }

        public void Save()
        {
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/gameObjects.json"))
            {
                Globals.Serializer.Serialize(file, Objects);
            }
        }
    }
}