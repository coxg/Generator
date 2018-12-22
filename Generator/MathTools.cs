using System;
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
            return new Vector2((float)Math.Sin(radians), (float)Math.Cos(radians));
        }

        public static string StringFromRadians(float radians)
        // Converts from radians to a string representing the direction
        {
            var cardinalDirection = "Back";
            if (radians > MathHelper.PiOver4 & radians < 3 * MathHelper.PiOver4)
            {
                cardinalDirection = "LView";
            }
            else if (radians >= 3 * MathHelper.PiOver4 & radians <= 5 * MathHelper.PiOver4)
            {
                cardinalDirection = "Front";
            }
            else if (radians > 5 * MathHelper.PiOver4 & radians < 7 * MathHelper.PiOver4)
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
    }
}