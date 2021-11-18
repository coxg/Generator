using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Generator
{
    public static class MathTools
    {
        // Return a random number
        private static Random random = new Random();
        public static int RandInt(int value)
        {
            return random.Next(value);
        }

        public static int Mod(int number, int modulo) 
            // Because % is remainder, not mod
        {
            var remainder = number % modulo;
            return remainder < 0 ? remainder + modulo : remainder;
        }

        public static float Mod(float number, float modulo)
            // Because % is remainder, not mod
        {
            var remainder = number % modulo;
            return remainder < 0 ? remainder + modulo : remainder;
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
                end = (int)second;
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
            var floatRange = Array.ConvertAll(range, rangeVal => (float)rangeVal);
            return floatRange;
        }

        public static Vector2 OffsetFromRadians(float radians)
        // Converts from radians to an offset
        {
            return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        public static string StringFromRadians(float radians)
        // Converts from radians to a string representing the direction
        {
            var cardinalDirection = "Right";
            if (radians >= MathHelper.PiOver4 & radians <= 3 * MathHelper.PiOver4)
            {
                cardinalDirection = "Back";
            }
            else if (radians > 3 * MathHelper.PiOver4 & radians < 5 * MathHelper.PiOver4)
            {
                cardinalDirection = "Left";
            }
            else if (radians >= 5 * MathHelper.PiOver4 & radians <= 7 * MathHelper.PiOver4)
            {
                cardinalDirection = "Front";
            }

            return cardinalDirection;
        }

        public static Vector3 PointRotatedAroundPoint(
                        Vector3 RotatedPoint, Vector3 AroundPoint, Vector3 RotationAxis)
        // Rotates a point around another point one avis at a time
        {
            // Translate point
            RotatedPoint -= AroundPoint;

            // Rotate point x-axis
            if (RotationAxis.X != 0)
            {
                var sin = (float)Math.Sin(RotationAxis.X);
                var cos = (float)Math.Cos(RotationAxis.X);
                RotatedPoint = new Vector3(
                    RotatedPoint.X,
                    RotatedPoint.Y * cos - RotatedPoint.Z * sin,
                    RotatedPoint.Y * sin + RotatedPoint.Z * cos);
            }

            // Rotate point y-axis
            if (RotationAxis.Y != 0)
            {
                var sin = (float)Math.Sin(RotationAxis.Y);
                var cos = (float)Math.Cos(RotationAxis.Y);
                RotatedPoint = new Vector3(
                    RotatedPoint.X * cos + RotatedPoint.Z * sin,
                    RotatedPoint.Y,
                    -RotatedPoint.X * sin + RotatedPoint.Z * cos);
            }

            // Rotate point z-axis
            if (RotationAxis.Z != 0)
            {
                var sin = (float)Math.Sin(RotationAxis.Z);
                var cos = (float)Math.Cos(RotationAxis.Z);
                RotatedPoint = new Vector3(
                    RotatedPoint.X * cos - RotatedPoint.Y * sin,
                    RotatedPoint.X * sin + RotatedPoint.Y * cos,
                    RotatedPoint.Z);
            }

            // Translate point back
            RotatedPoint += AroundPoint;
            return RotatedPoint;
        }

        // Gets the angle in radians between two points
        public static double Angle(Vector3 Point1, Vector3 Point2)
        {
            var deltaX = Point2.X - Point1.X;
            var deltaY = Point2.Y - Point1.Y;
            return Mod((float)Math.Atan2(deltaY, deltaX), MathHelper.TwoPi);
        }

        // Gets the location where a line between two points intersects the Z plane
        public static Vector3 IntersectionLocation(Vector3 Point1, Vector3 Point2)
        {
            var Direction = Point2 - Point1;
            var Distance = Point1.Z / Direction.Z;
            return Point1 + Direction * Distance;
        }

        // The distance between two points
        public static float Distance(Vector3 Point1, Vector3 Point2)
        {
            return (Point1 - Point2).Length();
        }

        public static Vector3 PositionFromPixels(Vector2 pixels)
        {
            var position = Vector3.Zero;
            position.X = (
                pixels.X / Globals.Resolution.X
                * (GameControl.camera.VisibleArea.Right - GameControl.camera.VisibleArea.Left))
                + GameControl.camera.VisibleArea.Left;
            position.Y = (
                pixels.Y / Globals.Resolution.Y
                * (GameControl.camera.VisibleArea.Top - GameControl.camera.VisibleArea.Bottom))
                + GameControl.camera.VisibleArea.Bottom;
            return position;
        }

        public static Vector2 PixelsFromPosition(Vector3 position)
        {
            var pixels = Vector2.Zero;
            pixels.X =
                Globals.Resolution.X
                * (position.X - GameControl.camera.VisibleArea.Left)
                / (GameControl.camera.VisibleArea.Right - GameControl.camera.VisibleArea.Left);
            pixels.Y =
                Globals.Resolution.Y
                * (position.Y + position.Z - GameControl.camera.VisibleArea.Bottom)
                / (GameControl.camera.VisibleArea.Top - GameControl.camera.VisibleArea.Bottom);
            return pixels;
        }

        public static List<Vector3> GetCoordinatesInCircle(Vector3 target, int radius)
        {
            var x = (int) Math.Round(target.X);
            var y = (int) Math.Round(target.Y);
            var roundedTarget = new Vector3(x, y, 0);
            
            if (radius == 0)
            {
                return new List<Vector3>{ roundedTarget };
            }

            var coordinates = new List<Vector3>();
            for (var curX = x - radius ; curX <= x + radius; curX++)
            {
                for (var curY = y - radius ; curY <= y + radius; curY++)
                {
                    var coordinate = new Vector3(curX, curY, 0);
                    if (Vector3.Distance(coordinate, roundedTarget) < radius + .5f)
                    {
                        coordinates.Add(coordinate);
                    }
                }
            }

            return coordinates.OrderBy(coordinate => Vector3.Distance(coordinate, target)).ToList();
        }
    }
}