using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public abstract class GameElement
        // GameObjects and their components
    {
        public string ID;
        public string Name;
        public Vector3 AnimationOffset;
        public Vector3 RotationOffset;
        public Vector3 RotationPoint;
        public bool CastsShadow;
        public Vector3 Size;
        public abstract Texture2D Sprite { get; set; }
        public abstract float Direction { get; set; }

        protected Vector3 _Center;
        public Vector3 Center { get => _Center; }
        protected Vector3 _Position;
        public virtual Vector3 Position
        {
            get => _Position;
            set
            {
                _Position = value;
                _Center = Position + Size / 2;
            }
        }

        // Checks if it can see the specified position or if we're blocked by any gameObjects
        public bool CanSee(Vector3 position)
        {
            var viewAngle = MathTools.Angle(Center, position);
            var viewDistance = Vector3.Distance(Center, position);

            // Check each active gameObject
            foreach (var gameObject in Globals.Zone.GameObjects.Objects.Values)
            {
                // Make sure it's not this object
                if (gameObject != this)
                {

                    // Make sure the object is between the two points
                    if (Vector3.Distance(Center, gameObject.Center) + gameObject.Size.Length() / 4 < viewDistance)
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