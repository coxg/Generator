using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Generator
{
    public class GameObjectManager
    {
        public Dictionary<string, GameObject> Objects = new Dictionary<string, GameObject>();
        public static int ComponentCount;
        public static List<VertexPositionColorTexture> Vertices = new List<VertexPositionColorTexture>();
        
        public GameObjectManager(List<GameObject> objects)
        {
            ComponentCount = 0;
            foreach (var gameObject in objects)
            {
                Objects[gameObject.ID] = gameObject;
                ComponentCount += gameObject.Components.Count;
            }
        }

        [JsonConstructor]
        public GameObjectManager(Dictionary<string, GameObject> objects, int componentCount)
        {
            Objects = objects;
            ComponentCount = 0;
            foreach (var gameObject in objects)
            {
                ComponentCount += gameObject.Value.Components.Count;
            }
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