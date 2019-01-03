using Microsoft.Xna.Framework;

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
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }

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
                Globals.Player.Center.Y - 10,
                Globals.Player.Center.Z + 10);
            Target = new Vector3(
                Globals.Player.Center.X,
                Globals.Player.Center.Y - 1,
                Globals.Player.Center.Z);
        }

        public Vector2 ViewMinCoordinates()
        {
            return new Vector2(Position.X - 17, Position.Y + 2);
        }

        public Vector2 ViewMaxCoordinates()
        {
            return new Vector2(Position.X + 17, Position.Y + 2 * Position.Z + 2);
        }
    }
}