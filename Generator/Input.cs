using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Generator
{
    public static class Input
    {
        public static void GetInput(GameObject player)
        {

            // Move the player
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                player.Move("Up");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                player.Move("Down");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                player.Move("Left");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                player.Move("Right");
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
