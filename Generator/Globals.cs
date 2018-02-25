using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Generator
{

    // C# doesn't have globals. I do!
    public static class Globals
    {

        // Regular old variables
        public static Vector2 Resolution { get; set; }
        public static int SquareSize { get; set; }
        public static ContentManager Content { get; set; }
        public static bool Logging { get; set; }
        public static Vector2 MapOffset { get; set; }
        public static int Clock { get; set; }
        public static int GridAlpha { get; set; }
        public static bool Multithreaded { get; set; }
        public static int RefreshRate { get; set; }

        // For better controls
        public static bool ActivateButtonWasDown { get; set; }

        // For displaying talking stuff
        public static Queue<string> DisplayTextQueue { get; set; }
        public static Queue<GameObject> TalkingObjectQueue { get; set; }

        // Loading assets
        public static Texture2D WhiteDot { get; set; }
        public static SpriteFont Font { get; set; }
        public static Texture2D Checker { get; set; }

        // Map rotation values
        private static double currentSin;
        public static double CurrentSin { get { return currentSin; } }
        private static double currentCos;
        public static double CurrentCos { get { return currentCos; } }
        private static double mapRotation;
        public static double MapRotation {
            get { return mapRotation; }
            set
            {
                currentSin = Math.Sin(value);
                currentCos = Math.Cos(value);
                mapRotation = value;
            }
        }

        // Grid logic
        private static GameObject[,] _grid { get; set; }
        public static class Grid
        {
            // "Getter"
            public static GameObject GetObject(int XVal, int YVal)
            {
                return _grid[
                    (int)Mod(XVal, _grid.GetLength(0)), 
                    (int)Mod(YVal, _grid.GetLength(1))];
            }

            // "Setter"
            public static void SetObject(int XVal, int YVal, GameObject gameObject)
            {
                _grid[
                    (int)Mod(XVal, _grid.GetLength(0)), 
                    (int)Mod(YVal, _grid.GetLength(1))] = gameObject;
            }

            // GetLength
            public static int GetLength(int Dimension)
            {
                return _grid.GetLength(Dimension);
            }
        }

        public static float Mod(float Number, float Modulo)
        // Because % is remainder, not mod
        {
            float Remainder = Number % Modulo;
            return Remainder < 0 ? Remainder + Modulo : Remainder;
        }

        public static Vector2 OffsetFromRadians(float radians)
        // Converts from radians to an offset
        {
            return new Vector2((float)Math.Sin(radians), (float)Math.Cos(radians));
        }

        [System.Runtime.CompilerServices.MethodImpl(
         System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void Log(string text = "")
        // Logs to console with debugging information
        {
            if (Logging)
            {
                var CallingFrame = new System.Diagnostics.StackTrace(1, true).GetFrame(0);
                Console.WriteLine(
                    CallingFrame.GetFileName().Split('\\').Last() + " line " 
                    + CallingFrame.GetFileLineNumber().ToString() + ", in " 
                    + CallingFrame.GetMethod().ToString().Split(" ".ToCharArray())
                      [1].Split("(".ToCharArray()).First() + ": " 
                    + text);
            }
        }

        // Data storage
        public static Dictionary<string, GameObject> ObjectDict { get; set; }
        public static Dictionary<string, Weapon> WeaponsDict { get; set; }
        public static Dictionary<string, OffHand> OffHandDict { get; set; }
        public static Dictionary<string, Armor> ArmorDict { get; set; }
        public static Dictionary<string, Generation> GeneratorDict { get; set; }
        public static Dictionary<string, Accessory> AccessoryDict { get; set; }

        // Population
        public static void Populate()
        {
            // Regular old variables
            Resolution = new Vector2(1800, 1000);
            Logging = true;
            Clock = 0;
            GridAlpha = 50;
            _grid = new GameObject[100, 100];
            Multithreaded = false;
            RefreshRate = 30;

            // For better control
            ActivateButtonWasDown = false;

            // For displaying talking stuff
            DisplayTextQueue = new Queue<string>();
            TalkingObjectQueue = new Queue<GameObject>();

            // Data storage
            ObjectDict = new Dictionary<string, GameObject>();
            WeaponsDict = new Dictionary<string, Weapon>();
            OffHandDict = new Dictionary<string, OffHand>();
            ArmorDict = new Dictionary<string, Armor>();
            GeneratorDict = new Dictionary<string, Generation>();
            AccessoryDict = new Dictionary<string, Accessory>();
        }
    }
}
