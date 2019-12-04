using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Generator
{
    // C# doesn't have globals. I do!
    public static class Globals
    {
        // The player's party
        public static SavedParty Party = new SavedParty("party", new Party());
        public static SavedInt PlayerPartyNumber = new SavedInt("playerPartyNumber", 0);
        public static GameObject Player
        {
            get { return Party.Value.Members[PlayerPartyNumber.Value]; }
            set { PlayerPartyNumber.Value = Party.Value.Members.IndexOf(value); }
        }

        // The current conversation - there can be only one
        private static Conversation currentConversation;
        public static Conversation CurrentConversation
        {
            get { return currentConversation; }
            set
            {
                if (value != null)
                {
                    value.Reset();
                }
                currentConversation = value;
            }
        }

        // Regular old variables
        public static Vector2 Resolution = new Vector2(1600, 900);
        public static ContentManager Content;
        public static bool Logging = true;
        public static int RefreshRate = 60;
        public static string Directory = "/Generator/Generator/";
        // TODO: Remove formatting before release - this roughly doubles the size of the save files
        public static JsonSerializer Serializer = new JsonSerializer { Formatting = Formatting.Indented };

        // World management
        public static string zone = "Overworld";
        public static string Zone
        {
            get { return zone; }
            set
            {
                if (value != zone)
                {
                    zone = value;
                    GameObjectManager.Initialize();
                }
            }
        }

        // Configure the world building mode
        public static bool CreativeMode;
        public static int CreativeObjectIndex;

        // Loading assets
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public static Texture2D LightTexture;
        public static Texture2D WhiteDot;
        public static SpriteFont Font;

        // Data storage
        public static Dictionary<string, Weapon> WeaponsDict = new Dictionary<string, Weapon>();
        public static Dictionary<string, Armor> ArmorDict = new Dictionary<string, Armor>();
        public static Dictionary<string, GeneratorObj> GeneratorDict = new Dictionary<string, GeneratorObj>();
        public static Dictionary<string, Accessory> AccessoryDict = new Dictionary<string, Accessory>();

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Log(object text = null)
        // Logs to console with debugging information
        {
            if (Logging)
            {
                var CallingFrame = new StackTrace(1, true).GetFrame(0);
                Console.WriteLine(
                    CallingFrame.GetFileName().Split('\\').Last() + " line "
                    + CallingFrame.GetFileLineNumber() + ", in "
                    + CallingFrame.GetMethod().ToString().Split(" ".ToCharArray())[1].Split("(".ToCharArray()).First()
                    + ": " + text);
            }
        }

        public static object Copy(object copyObj)
            // C# doesn't have a native copy method, so just serialize and deserialize
        {
            using (StreamWriter file = File.CreateText(Directory + "tmp.json"))
            {
                Serializer.Serialize(file, copyObj);
            }
            using (StreamReader file = File.OpenText(Directory + "tmp.json"))
            {
                copyObj = Serializer.Deserialize(file, copyObj.GetType());
            }
            return copyObj;
        }
    }
}