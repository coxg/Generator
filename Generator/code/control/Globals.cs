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
        public static SavedParty Party = new SavedParty("party", new Party(new List<string> { "Niels", "Farrah", "OldMan"}));
        public static GameObject Player
        {
            get => Party.Value.GetLeader();
        }

        // The current conversation - there can be only one
        private static Conversation currentConversation;
        public static Conversation CurrentConversation
        {
            get => currentConversation;
            set
            {
                value?.Reset();
                currentConversation = value;
                GameControl.CurrentScreen = value == null ? 
                    GameControl.GameScreen.WalkingAround : GameControl.GameScreen.Conversation;
            }
        }

        // Regular old variables
        public static Vector2 Resolution = new Vector2(1600, 900);
        public static ContentManager ContentManager;
        // TODO: Things break if I try to change this mid-game
        public static int RefreshRate = 60;
        // TODO: This will definitely break when I move to production
        public static string ProjectDirectory = Path.GetFullPath(@"../../../../");
        public static JsonSerializer Serializer;
        public const bool IsRelease = false;

        // World management
        public static SavedString ZoneName = new SavedString("zoneName", "testingZone");
        public static GameObjectManager GameObjectManager;
        public static TileManager TileManager;
        private static Zone _zone;
        public static Zone Zone
        {
            get => _zone;
            set
            {
                // Set the new zone
                _zone = value;
                ZoneName.Value = value.Name;
            }
        }

        // Configure the world building mode
        public static bool CreativeMode = false;

        // Loading assets
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public static SpriteSheet CommonSpriteSheet;
        public static Texture2D WhiteDot;
        public static SpriteFont Font;
        public static TileSheet DefaultTileSheet;
        public static SpriteSheet DefaultSpriteSheet;
        public static SpriteSheet SpriteSheet;

        public static List<string> RecentLogs = new List<string>();

        private static string getLogPrefix()
        {
            var callingFrame = new StackTrace(1, true).GetFrame(2);
            var time = DateTime.Now.ToString(@"h\:mm\:ss.fff");
            var file = callingFrame.GetFileName().Split(Path.DirectorySeparatorChar).Last().Split('.').First();
            var line = callingFrame.GetFileLineNumber();
            var method = callingFrame.GetMethod().ToString().Split(" ".ToCharArray())[1].Split("(".ToCharArray()).First();
            return $"{time} {file}#{method}:{line}: ";
        }

        private static void writeLogLine(object text)
        {
            var logLine = getLogPrefix() + text;
            Console.WriteLine(logLine);
            RecentLogs.Add(logLine);
            if (RecentLogs.Count > 10)
            {
                RecentLogs.RemoveAt(0);
            }
        }

        public static void Log(object text = null)
        // Logs to console/screen with debugging information
        {
            if (IsRelease) return;
            writeLogLine(text);
        }

        public static void Warn(object text = null)
        {
            if (IsRelease) return;
            writeLogLine("[WARNING] " + text);
        }

        public static object Copy(object copyObj) 
            // C# doesn't have a native copy method, so just serialize and deserialize
            // TODO: Is it possible to do this in memory instead of being IO bound?
        {
            using (StreamWriter file = File.CreateText(ProjectDirectory + "tmp.json"))
            {
                Serializer.Serialize(file, copyObj);
            }
            using (StreamReader file = File.OpenText(ProjectDirectory + "tmp.json"))
            {
                copyObj = Serializer.Deserialize(file, copyObj.GetType());
            }
            return copyObj;
        }

        public static void CopyTo<T>(object copyObj, out T copyToObj)
        // C# doesn't have a native copy method, so just serialize and deserialize
        {
            using (StreamWriter file = File.CreateText(ProjectDirectory + "tmp.json"))
            {
                Serializer.Serialize(file, copyObj);
            }
            using (StreamReader file = File.OpenText(ProjectDirectory + "tmp.json"))
            {
                copyToObj = (T)Serializer.Deserialize(file, typeof(T));
            }
        }
    }
}