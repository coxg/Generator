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
        // The player's party
        public static Party Party = new Party();
        public static int PlayerPartyNumber = 0;
        public static GameObject Player
        {
            get { return Party.Members[PlayerPartyNumber]; }
            set { PlayerPartyNumber = Party.Members.IndexOf(value); }
        }
        private static Conversation currentConversation = null;
        public static Conversation CurrentConversation
        {
            get { return currentConversation; }
            set
            {
                if (currentConversation != null)
                {
                    currentConversation.Reset();
                }
                currentConversation = value;
            }
        }

        // Regular old variables
        public static Vector2 Resolution = new Vector2(1600, 900);
        public static ContentManager Content;
        public static bool Logging = true;
        public static int RefreshRate = 30; // TODO: Change to 60, modify other calculations to reflects this
        public static string Directory = "/Generator/Generator/";

        // Configure the world building mode
        public static bool CreativeMode = false;
        public static int CreativeObjectIndex = 0;

        // Loading assets
        public static Texture2D WhiteDot;
        public static SpriteFont Font;
        public static Texture2D LightTexture;

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
    }
}