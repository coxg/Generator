using Microsoft.Xna.Framework;
using System.Drawing;
using System;

namespace Generator
{
    public class Camera
    {
        // Rotation stuff
        private float rotation;

        public Camera()
        // Constructor
        {
            Rotation = 0f;
        }

        // Fields 
        public Vector3 Position;
        public Vector3 Target;
        public RectangleF VisibleArea;
        public float Height = 25;

        public float Rotation
        {
            get => rotation;

            set
            {
                Position = Vector3.Transform(
                               Position - Target,
                               Matrix.CreateFromAxisAngle(Vector3.Backward, value)
                           ) + Target;
                rotation = value;
            }
        }

        public Matrix View
            // Get the look at vector
            => Matrix.CreateLookAt(Position, Target, Vector3.Backward);

        public static Matrix Projection
            // Field of view stuff
        {
            get
            {
                var fieldOfView = MathHelper.PiOver4;
                float nearClipPlane = 1;
                float farClipPlane = float.MaxValue;
                var aspectRatio = GameControl.graphics.GraphicsDevice.Viewport.Width
                                  / (float) GameControl.graphics.GraphicsDevice.Viewport.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }

        public void Update()
        {
            var center = Globals.Player.Center;
            Position = new Vector3(
                center.X,
                center.Y - .00001f,
                center.Z + Height);
            Target = center;

            var screenWidth =
                Globals.Resolution.X
                * ((center.Z + Height) * 1.5f)
                / Math.Max(Globals.Resolution.X, Globals.Resolution.Y);
            var screenHeight =
                Globals.Resolution.Y
                * ((center.Z + Height) * 1.5f)
                / Math.Max(Globals.Resolution.X, Globals.Resolution.Y);
            VisibleArea = new RectangleF(
                Position.X - screenWidth / 2, 
                Position.Y - screenHeight / 2,
                screenWidth,
                screenHeight);
        }
    }
}