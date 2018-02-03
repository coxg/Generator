using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Generator
{
    public static class Input
    {
        public static void GetInput(GameObject player)
        {

            // Convert from actual movement input to intended movement input
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

            // Convert from intended movement input to cardinal direction
            if (MoveHorizontalOffset != 0 || MoveVerticalOffset != 0)
            {
                // Convert from intended movement input to intended movement direction
                float MoveDirection = 0f;
                if (MoveVerticalOffset == 1 && MoveHorizontalOffset == 0) // Move up
                {
                    MoveDirection = 0f;
                }
                else if (MoveVerticalOffset == 1 && MoveHorizontalOffset == 1) // Move up-right
                {
                    MoveDirection = .25f * (float)Math.PI;
                }
                else if (MoveVerticalOffset == 0 && MoveHorizontalOffset == 1) // Move right
                {
                    MoveDirection = .5f * (float)Math.PI;
                }
                else if (MoveVerticalOffset == -1 && MoveHorizontalOffset == 1) // Move down-right
                {
                    MoveDirection = .75f * (float)Math.PI;
                }
                else if (MoveVerticalOffset == -1 && MoveHorizontalOffset == 0) // Move down
                {
                    MoveDirection = (float)Math.PI;
                }
                else if (MoveVerticalOffset == -1 && MoveHorizontalOffset == -1) // Move down-left
                {
                    MoveDirection = 1.25f * (float)Math.PI;
                }
                else if (MoveVerticalOffset == 0 && MoveHorizontalOffset == -1) // Move left
                {
                    MoveDirection = 1.5f * (float)Math.PI;
                }
                else if (MoveVerticalOffset == 1 && MoveHorizontalOffset == -1) // Move up-left
                {
                    MoveDirection = 1.75f * (float)Math.PI;
                }

                // Convert from intended movement direction to cardinal direction
                MoveDirection -= (float)Globals.MapRotation;
                MoveDirection = Globals.Mod((float)MoveDirection, 2f * (float)Math.PI);
                if (1.875f * (float)Math.PI < MoveDirection || MoveDirection <= 0.125f * (float)Math.PI)
                {
                    player.Move("North");
                }
                else if (.125f * (float)Math.PI < MoveDirection && MoveDirection <= .375f * (float)Math.PI)
                {
                    player.Move("Northeast");
                }
                else if (.375f * (float)Math.PI < MoveDirection && MoveDirection <= .625f * (float)Math.PI)
                {
                    player.Move("East");
                }
                else if (.625f * (float)Math.PI < MoveDirection && MoveDirection <= .875f * (float)Math.PI)
                {
                    player.Move("Southeast");
                }
                else if (.875f * (float)Math.PI < MoveDirection && MoveDirection <= 1.125f * (float)Math.PI)
                {
                    player.Move("South");
                }
                else if (1.125f * (float)Math.PI < MoveDirection && MoveDirection <= 1.375f * (float)Math.PI)
                {
                    player.Move("Southwest");
                }
                else if (1.375f * (float)Math.PI < MoveDirection && MoveDirection <= 1.625f * (float)Math.PI)
                {
                    player.Move("West");
                }
                else if (1.625f * (float)Math.PI < MoveDirection && MoveDirection <= 1.875f * (float)Math.PI)
                {
                    player.Move("Northwest");
                }
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
