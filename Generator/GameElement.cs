using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public abstract class GameElement
        // GameObjects and their components
    {
        public string Name { get; set; }
        public Vector3 AnimationOffset { get; set; }
        public Vector3 RotationOffset { get; set; }
        public Vector3 RotationPoint { get; set; }
        public Vector3 Size { get; set; }
        public Texture2D Sprite { get; set; }
        public abstract Vector3 Position { get; set; }
        public abstract float Direction { get; set; }

        public Vector3 Center { get { return Position + Size / 2; } }

        // Checks if it can see the specified position or if we're blocked by any gameObjects
        public bool CanSee(Vector3 position)
        {
            var viewAngle = MathTools.Angle(Center, position);
            var viewDistance = MathTools.Distance(Center, position);

            // Check each active gameObject
            foreach (var gameObjectName in Globals.GameObjects.ActiveGameObjects)
            {
                // Make sure it's not this object
                if (gameObjectName != Name)
                {
                    var gameObject = Globals.GameObjects.ObjectFromName[gameObjectName];

                    // Make sure the object is between the two points
                    if (MathTools.Distance(Center, gameObject.Center) + gameObject.Size.Length() / 4 < viewDistance)
                    {
                        // Get the angles for each corner of the gameObject
                        var objectAngles = new double[4];
                        objectAngles[0] = MathTools.Angle(Center, gameObject.Position);
                        objectAngles[1] = MathTools.Angle(Center, new Vector3(
                            gameObject.Position.X + gameObject.Size.X,
                            gameObject.Position.Y,
                            gameObject.Position.Z));
                        objectAngles[2] = MathTools.Angle(Center, new Vector3(
                            gameObject.Position.X,
                            gameObject.Position.Y + gameObject.Size.Y,
                            gameObject.Position.Z));
                        objectAngles[3] = MathTools.Angle(Center, new Vector3(
                            gameObject.Position.X + gameObject.Size.X,
                            gameObject.Position.Y + gameObject.Size.Y,
                            gameObject.Position.Z));

                        // Compare the viewing angle with the upper and lower bounds of the object
                        var minObjectAngle = objectAngles.Min();
                        var maxObjectAngle = objectAngles.Max();
                        if (maxObjectAngle - minObjectAngle > MathHelper.Pi)
                        {
                            if (maxObjectAngle < viewAngle || viewAngle < minObjectAngle)
                            {
                                return false;
                            }
                        }
                        else if (minObjectAngle < viewAngle && viewAngle < maxObjectAngle)
                        {
                            return false;
                        }
                    }
                }
            }

            // If we didn't collide with anything then we have line of sight
            return true;
        }
    }
}