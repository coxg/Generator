using System;
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

        // Checks if the game element has line of sight to the specified position
        public bool CanSee(Vector3 position)
        {
            // Create a beam from the object to the position
            var m = (position.Y - Center.Y) / (position.X - Center.X);
            var b = Center.Y - m * Center.X;

            // Check each active gameObject
            foreach (var gameObjectName in Globals.GameObjects.ActiveGameObjects)
            {

                // Make sure it's not this object
                if (gameObjectName != Name)
                {
                    var gameObject = Globals.GameObjects.ObjectFromName[gameObjectName];

                    // Make sure the x value is valid
                    if ((position.X < gameObject.Center.X && gameObject.Center.X < Center.X) 
                        || (Center.X < gameObject.Center.X && gameObject.Center.X < position.X))
                    {
                        // Make sure the y value is valid
                        if ((position.Y < gameObject.Center.Y && gameObject.Center.Y < Center.Y)
                            || (Center.Y < gameObject.Center.Y && gameObject.Center.Y < position.Y))
                        {
                            // If any active gameObjects intercept the beam then return false
                            var y = m * gameObject.Center.X + b;
                            if (y > gameObject.Position.Y && y < gameObject.Position.Y + gameObject.Size.Y)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            // If we didn't collide with anything then we have line of sight
            return true;
        }
    }
}