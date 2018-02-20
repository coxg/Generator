using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Generator
{
    public class Camera
    {

        // Fields 
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }

        // Rotation stuff
        private float rotation;
        public float Rotation {
            get
            {
                return rotation;
            }

            set
            {
                Position = Vector3.Transform(
                    Position - Target,
                    Matrix.CreateFromAxisAngle(new Vector3(0, 0, 1), value)
                ) + Target;
                rotation = value;
            }
        }

        public Matrix View
        // Get the look at vector
        {
            get
            {
                return Matrix.CreateLookAt(Position, Target, Up);
            }
        }

        public Matrix Projection
        // Field of view stuff
        {
            get
            {
                float fieldOfView = MathHelper.PiOver4;
                float nearClipPlane = 1;
                float farClipPlane = 200;
                float aspectRatio = GameControl.graphics.GraphicsDevice.Viewport.Width 
                    / (float)GameControl.graphics.GraphicsDevice.Viewport.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }

        public Camera(Vector3 position)
        // Constructor
        {
            Position = position;
            Target = new Vector3(position.X, 0, 0);
            Up = Vector3.UnitZ;
            Rotation = 0f;
        }
    }
}