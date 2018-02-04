using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Generator
{
    public static class Input
    {
        public static void GetInput(GameObject player)
        {

            // Convert from actual movement input to direction offsets
            int MoveVerticalOffset = 0;
            int MoveHorizontalOffset = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                MoveVerticalOffset += 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                MoveVerticalOffset -= 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                MoveHorizontalOffset -= 1;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                MoveHorizontalOffset += 1;
            }

            // Convert from direction offsets to radian direction
            if (MoveHorizontalOffset != 0 || MoveVerticalOffset != 0)
            {
                // Convert from offsets to radians
                float RadianDirection = (float)Math.Atan2(MoveHorizontalOffset, MoveVerticalOffset);

                // Apply offset from map rotation
                RadianDirection -= (float)Globals.MapRotation;
                RadianDirection = Globals.Mod((float)RadianDirection, 2f * (float)Math.PI);

                // Convert from radian direction to cardinal direction
                player.Move(RadianDirection);
            }

            // Attack
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                player.Attack();
            }

            // Rotate the map
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Globals.MapRotation += .1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                Globals.MapRotation -= .1f;
            }

            // Move the screen
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Globals.MapOffset = new Vector2(
                    (float)(Globals.MapOffset.X - Globals.CurrentSin),
                    (float)(Globals.MapOffset.Y + Globals.CurrentCos));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Globals.MapOffset = new Vector2(
                    (float)(Globals.MapOffset.X + Globals.CurrentSin),
                    (float)(Globals.MapOffset.Y - Globals.CurrentCos));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Globals.MapOffset = new Vector2(
                    (float)(Globals.MapOffset.X - Globals.CurrentCos),
                    (float)(Globals.MapOffset.Y - Globals.CurrentSin));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Globals.MapOffset = new Vector2(
                    (float)(Globals.MapOffset.X + Globals.CurrentCos),
                    (float)(Globals.MapOffset.Y + Globals.CurrentSin));
            }

            // Zoom in/out
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) && Globals.SquareSize < 256)
            {
                Globals.SquareSize *= 2;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && Globals.SquareSize > 16)
            {
                Globals.SquareSize /= 2;
            }
        }
    }
}
