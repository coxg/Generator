using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace Generator
{

    // C# doesn't have globals. I do!
    public static class Globals
    {

        // Regular old variables
        public static Vector2 Resolution { get; set; }
        public static int SquareSize { get; set; }
        public static GameObject[,] Grid { get; set; }
        public static ContentManager Content { get; set; }
        public static bool Logging { get; set; }
        public static Vector2 MapOffset { get; set; }
        public static int Clock { get; set; }
        public static int GridAlpha { get; set; }

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

        // Map rotation logic
        public static Vector2 GetRotatedCoordinates(Vector2 coordinates)
        {
            int XOffsetInPixels = (int)(Globals.MapOffset.X * (double)Globals.SquareSize);
            int YOffsetInPixels = (int)(Globals.MapOffset.Y * (double)Globals.SquareSize);
            Vector2 MapCenter = new Vector2(
                Globals.Resolution.X / 2 + XOffsetInPixels, 
                Globals.Resolution.Y / 2 - YOffsetInPixels);

            // Rotate around the center of the screen
            coordinates.X -= MapCenter.X;
            coordinates.Y -= MapCenter.Y;
            double newXCoordinate = coordinates.X * Globals.CurrentCos - coordinates.Y * Globals.CurrentSin;
            double newYCoordinate = coordinates.X * Globals.CurrentSin + coordinates.Y * Globals.CurrentCos;
            coordinates.X = (int)newXCoordinate + MapCenter.X;
            coordinates.Y = (int)newYCoordinate + MapCenter.Y;

            // Apply map translation
            coordinates.X -= XOffsetInPixels;
            coordinates.Y += YOffsetInPixels;

            return coordinates;

        }


        // Logging
        [System.Runtime.CompilerServices.MethodImpl(
         System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void Log(string text = "")
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
            // Variables
            Resolution = new Vector2(1280, 720);
            SquareSize = 64;
            Logging = true;
            MapOffset = new Vector2(0, 0);
            Clock = 0;
            MapRotation = 0.0;
            GridAlpha = 50;

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
