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
        // For making characters despawm
        public static List<string> DeathList = new List<string>();

        // Regular old variables
        public static Vector2 Resolution { get; set; }
        public static int SquareSize { get; set; }
        public static ContentManager Content { get; set; }
        public static bool Logging { get; set; }
        public static Vector2 MapOffset { get; set; }
        public static int Clock { get; set; }
        public static int GridAlpha { get; set; }
        public static int RefreshRate { get; set; }
        public static string Directory { get; set; }

        // For better controls
        public static bool ActivateButtonWasDown { get; set; }

        // For displaying talking stuff
        public static Queue<string> DisplayTextQueue { get; set; }
        public static Queue<GameObject> TalkingObjectQueue { get; set; }

        // Loading assets
        public static Texture2D WhiteDot { get; set; }
        public static SpriteFont Font { get; set; }
        public static Texture2D Checker { get; set; }

        // Grid logic
        private static GameObject[,] _grid { get; set; }

        // Data storage
        public static Dictionary<string, GameObject> ObjectDict { get; set; }
        public static Dictionary<string, Weapon> WeaponsDict { get; set; }
        public static Dictionary<string, OffHand> OffHandDict { get; set; }
        public static Dictionary<string, Helmet> HelmetDict { get; set; }
        public static Dictionary<string, Armor> ArmorDict { get; set; }
        public static Dictionary<string, Generation> GeneratorDict { get; set; }
        public static Dictionary<string, Accessory> AccessoryDict { get; set; }

        public static float Mod(float Number, float Modulo)
            // Because % is remainder, not mod
        {
            var Remainder = Number % Modulo;
            return Remainder < 0 ? Remainder + Modulo : Remainder;
        }

        public static int[] Range(int first, int? second = null)
            // Because C# doesn't have a Range function. Seriously, C#?
        {
            // Get start and end values
            int start;
            int end;
            if (second == null)
            {
                start = 0;
                end = first;
            }
            else
            {
                start = first;
                end = (int) second;
            }

            // Get the range
            var enumerableRange = Enumerable.Range(start, end - start);
            var range = enumerableRange.ToArray();
            return range;
        }

        public static float[] FloatRange(int first, int? second = null)
            // Like range, but returns float. Because I won't remember how to do this.
        {
            var range = Range(first, second);
            var floatRange = Array.ConvertAll(range, rangeVal => (float) rangeVal);
            return floatRange;
        }

        public static Vector2 OffsetFromRadians(float radians)
            // Converts from radians to an offset
        {
            return new Vector2((float) Math.Sin(radians), (float) Math.Cos(radians));
        }

        public static string StringFromRadians(float radians)
        // Converts from radians to a string representing the direction
        {
            var cardinalDirection = "Back";
            if (radians >= .25 * Math.PI & radians < .75 * Math.PI)
            {
                cardinalDirection = "LView";
            }
            else if (radians >= .75 * Math.PI & radians < 1.25 * Math.PI)
            {
                cardinalDirection = "Front";
            }
            else if (radians >= 1.25 * Math.PI & radians < 1.75 * Math.PI)
            {
                cardinalDirection = "RView";
            }

            return cardinalDirection;
        }

        public static Vector3 PointRotatedAroundPoint(
                        Vector3 RotatedPoint, Vector3 AroundPoint, Vector3 RotationAxis)
        // Rotates a point around another point
        {
            // Translate point
            RotatedPoint -= AroundPoint;

            // Precompute sin/cos
            var sin = new Vector3(
                (float)Math.Sin(RotationAxis.X),
                (float)Math.Sin(RotationAxis.Y),
                (float)Math.Sin(RotationAxis.Z));
            var cos = new Vector3(
                (float)Math.Cos(RotationAxis.X),
                (float)Math.Cos(RotationAxis.Y),
                (float)Math.Cos(RotationAxis.Z));

            // Rotate point around each axis
            RotatedPoint = new Vector3(
                RotatedPoint.X,
                RotatedPoint.Y * cos.X - RotatedPoint.Z * sin.X,
                RotatedPoint.Y * sin.X + RotatedPoint.Z * cos.X);
            RotatedPoint = new Vector3(
                RotatedPoint.X * cos.Y + RotatedPoint.Z * sin.Y,
                RotatedPoint.Y,
                -RotatedPoint.X * sin.Y + RotatedPoint.Z * cos.Y);
            RotatedPoint = new Vector3(
                RotatedPoint.X * cos.Z - RotatedPoint.Y * sin.Z,
                RotatedPoint.X * sin.Z + RotatedPoint.Y * cos.Z,
                RotatedPoint.Z);

            // Translate point back
            RotatedPoint += AroundPoint;
            return RotatedPoint;
        }

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
        public static void Populate()
        {
            // Regular old variables
            Resolution = new Vector2(1500, 900);
            Logging = true;
            Clock = 0;
            GridAlpha = 50;
            _grid = new GameObject[100, 100];
            RefreshRate = 30;
            Directory = "/Generator/Generator/";

            // For better control
            ActivateButtonWasDown = false;

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

        public static class Grid
        {
            // "Getter"
            public static GameObject GetObject(int XVal, int YVal)
            {
                return _grid[
                    (int) Mod(XVal, _grid.GetLength(0)),
                    (int) Mod(YVal, _grid.GetLength(1))];
            }

            // "Setter"
            public static void SetObject(int XVal, int YVal, GameObject gameObject)
            {
                _grid[
                    (int) Mod(XVal, _grid.GetLength(0)),
                    (int) Mod(YVal, _grid.GetLength(1))] = gameObject;
            }

            // GetLength
            public static int GetLength(int Dimension)
            {
                return _grid.GetLength(Dimension);
            }
        }
    }
}