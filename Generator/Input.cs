using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Generator
{
    public static class Input
    {
        public static void GetInput(GameObject player)
        {

            // Get direction from input
            // TODO: Once I'm using a controller this can be a lot easier
            // Once I have that then I'll need to incorperate up-left/up-right whatever
            // Maybe even free movement, then lock to a grid when you stop? That feels bad man
            float? MoveDirection = null;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                MoveDirection = 0f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                MoveDirection = (float)Math.PI;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                MoveDirection = 1.5f * (float)Math.PI;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                MoveDirection = .5f * (float)Math.PI;
            }

            // Convert input direction into movement direction
            if (MoveDirection != null)
            {
                Globals.Log("Movement direction: " + MoveDirection.ToString());

                MoveDirection -= (float)Globals.MapRotation;
                MoveDirection = Globals.Mod((float)MoveDirection, 2f * (float)Math.PI);
                if (1.75f * (float)Math.PI < MoveDirection || MoveDirection <= 0.25f * (float)Math.PI)
                {
                    player.Move("Up");
                }
                else if (.25f * (float)Math.PI < MoveDirection && MoveDirection <= .75f * (float)Math.PI)
                {
                    player.Move("Right");
                }
                else if (.75f * (float)Math.PI < MoveDirection && MoveDirection <= 1.25f * (float)Math.PI)
                {
                    player.Move("Down");
                }
                else if (1.25f * (float)Math.PI < MoveDirection && MoveDirection <= 1.75f * (float)Math.PI)
                {
                    player.Move("Left");
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
