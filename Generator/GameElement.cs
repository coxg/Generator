using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class GameElement
        // GameObjects and their components
    {
        public Vector3 Position { get; set; }
        public Vector3 Size { get; set; }
        public Texture2D Sprite { get; set; }
        public float Direction { get; set; }
        public List<Ability> Abilities { get; set; }

        public bool CanMoveTo(Vector3 position)
        // Sees if the GameObject can move to the specified location unimpeded.
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
                    if (Globals.Grid.GetObject(moveToX, moveToY) != null
                        && Globals.Grid.GetObject(moveToX, moveToY) != this)
                    {
                        Globals.Log(
                            "[" + moveToX + ", " + moveToY + "]" +
                            " is not empty or self: " + Globals.Grid.GetObject(moveToX, moveToY));
                        return false;
                    }

            // If none of the above return false then it's passable
            return true;
        }
    }
}