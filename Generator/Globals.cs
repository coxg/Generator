using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    // C# doesn't have globals. I do!
    public static class Globals
    {
        // The player
        public static GameObject Player { get; set; }

        // For making characters despawm
        public static List<string> DeathList = new List<string>();

        // Regular old variables
        public static Vector2 Resolution { get; set; }
        public static ContentManager Content { get; set; }
        public static bool Logging { get; set; }
        public static int Clock { get; set; }
        public static int RefreshRate { get; set; }
        public static string Directory { get; set; }

        // For displaying talking stuff
        public static Queue<string> DisplayTextQueue { get; set; }
        public static Queue<GameObject> TalkingObjectQueue { get; set; }

        // Loading assets
        public static Texture2D WhiteDot { get; set; }
        public static SpriteFont Font { get; set; }

        // Data storage
        public static Dictionary<string, GameObject> ObjectDict { get; set; }
        public static Dictionary<string, Weapon> WeaponsDict { get; set; }
        public static Dictionary<string, OffHand> OffHandDict { get; set; }
        public static Dictionary<string, Helmet> HelmetDict { get; set; }
        public static Dictionary<string, Armor> ArmorDict { get; set; }
        public static Dictionary<string, Generation> GeneratorDict { get; set; }
        public static Dictionary<string, Accessory> AccessoryDict { get; set; }

        [MethodImpl(
            MethodImplOptions.NoInlining)]
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

        // Population
        public static void PopulateGlobals()
        {
            // Regular old variables
            Resolution = new Vector2(1500, 900);
            Logging = true;
            Clock = 0;
            RefreshRate = 30;
            Directory = "/Generator/Generator/";

            // For displaying talking stuff
            DisplayTextQueue = new Queue<string>();
            TalkingObjectQueue = new Queue<GameObject>();

            // Data storage
            ObjectDict = new Dictionary<string, GameObject>();
            WeaponsDict = new Dictionary<string, Weapon>();
            OffHandDict = new Dictionary<string, OffHand>();
            HelmetDict = new Dictionary<string, Helmet>();
            ArmorDict = new Dictionary<string, Armor>();
            GeneratorDict = new Dictionary<string, Generation>();
            AccessoryDict = new Dictionary<string, Accessory>();
        }
    }
}