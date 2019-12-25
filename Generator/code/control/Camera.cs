using Microsoft.Xna.Framework;
using System.Drawing;

namespace Generator
{
    public class Camera
    {
        // Rotation stuff
        private float rotation;

        public Camera()
        // Constructor
        {
            Up = Vector3.UnitZ;
            Rotation = 0f;
        }

        // Fields 
        public Vector3 Position;
        public Vector3 Target;
        public Vector3 Up;
        public RectangleF VisibleArea;
        public RectangleF UpdatingArea;
        public float Height = 25;

        public float Rotation
        {
            get => rotation;

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
            => Matrix.CreateLookAt(Position, Target, Up);

        public Matrix Projection
            // Field of view stuff
        {
            get
            {
                var fieldOfView = MathHelper.PiOver4;
                float nearClipPlane = 1;
                float farClipPlane = 200;
                var aspectRatio = GameControl.graphics.GraphicsDevice.Viewport.Width
                                  / (float) GameControl.graphics.GraphicsDevice.Viewport.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }

        public void Update()
        {
            Position = new Vector3(
                Globals.Player.Center.X,
                Globals.Player.Center.Y - .00001f,
                Globals.Player.Center.Z + Height);
            Target = new Vector3(
                Globals.Player.Center.X,
                Globals.Player.Center.Y,
                Globals.Player.Center.Z);

            var screenWidth = Globals.Resolution.X * (Height * 1.5f) / 1600;
            var screenHeight = Globals.Resolution.Y * (Height * 1.5f) / 1600;
            VisibleArea = new RectangleF(
                Position.X - screenWidth / 2, 
                Position.Y - screenHeight / 2,
                screenWidth,
                screenHeight);
            UpdatingArea = new RectangleF(
                Position.X - 3 * screenWidth / 2,
                Position.Y - 3 * screenHeight / 2,
                3 * screenWidth,
                3 * screenHeight);
        }
    }
}