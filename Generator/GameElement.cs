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

        public bool CanMoveTo(Vector3 position)
        // Sees if the GameElement can move to the specified location unimpeded.
        {
            // Loop through each x coordinate you're trying to move to
            for (
                    var moveToX = (int)Math.Floor(position.X);
                    moveToX < (int)Math.Ceiling(position.X + Size.X);
                    moveToX++)

                // Loop through each y coordinate you're trying to move to
                for (
                        var moveToY = (int)Math.Floor(position.Y);
                        moveToY < (int)Math.Ceiling(position.Y + Size.Y);
                        moveToY++)

                    // If location is not empty or self
                    if (GridLogic.Grid.GetObject(moveToX, moveToY) != null
                        && GridLogic.Grid.GetObject(moveToX, moveToY) != this)
                    {
                        Globals.Log(
                            "[" + moveToX + ", " + moveToY + "]" +
                            " is not empty or self: " + GridLogic.Grid.GetObject(moveToX, moveToY));
                        return false;
                    }

            // If none of the above return false then it's passable
            return true;
        }
    }
}