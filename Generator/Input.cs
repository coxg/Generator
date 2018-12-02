using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Generator
{
    public static class Input
    {
        public static void GetInput(GameObject player)
        {
            // Convert from actual movement input to direction offsets
            var moveVerticalOffset = 0.0;
            var moveHorizontalOffset = 0.0;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) moveVerticalOffset += 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) moveVerticalOffset -= 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) moveHorizontalOffset -= 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) moveHorizontalOffset += 1;
            
            // Check the device for Player One
            var capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            
            // If there a controller attached, handle it
            Globals.Log(PlayerIndex.One);
            Globals.Log(capabilities);
            Globals.Log(capabilities.IsConnected);
            if (capabilities.IsConnected)
            {
                // Get the current state of Controller1
                GamePadState state = GamePad.GetState(PlayerIndex.One);
                Globals.Log(state);
                Globals.Log(capabilities.HasLeftXThumbStick);

                // You can check explicitly if a gamepad has support for a certain feature
                if (capabilities.HasLeftXThumbStick)
                {
                    moveHorizontalOffset = state.ThumbSticks.Left.X;
                    moveVerticalOffset = state.ThumbSticks.Left.Y;
                }
            }

            // Convert from direction offsets to radian direction
            if (moveHorizontalOffset != 0 || moveVerticalOffset != 0)
            {
                // Convert from offsets to radians
                var radianDirection = (float) Math.Atan2(moveHorizontalOffset, moveVerticalOffset);

                // Apply offset from map rotation
                radianDirection -= GameControl.camera.Rotation;
                radianDirection = Globals.Mod(radianDirection, 2f * (float) Math.PI);

                // Convert from radian direction to cardinal direction
                var speed = (float) Math.Sqrt(player.Speed.CurrentValue);
                player.MoveInDirection(radianDirection, speed);
            }

            // Abilities
            if (player.Ability1 != null) player.Ability1.IsPressed = Keyboard.GetState().IsKeyDown(Keys.D1);
            if (player.Ability2 != null) player.Ability2.IsPressed = Keyboard.GetState().IsKeyDown(Keys.D2);
            if (player.Ability3 != null) player.Ability3.IsPressed = Keyboard.GetState().IsKeyDown(Keys.D3);
            if (player.Ability4 != null) player.Ability4.IsPressed = Keyboard.GetState().IsKeyDown(Keys.D4);

            // Map rotation
            if (Keyboard.GetState().IsKeyDown(Keys.Q)) GameControl.camera.Rotation = .1f;
            if (Keyboard.GetState().IsKeyDown(Keys.E)) GameControl.camera.Rotation = -.1f;

            // Zoom in/out
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                GameControl.camera.Position = new Vector3(
                    GameControl.camera.Position.X,
                    GameControl.camera.Position.Y,
                    GameControl.camera.Position.Z - 1);
                GameControl.camera.Target = new Vector3(
                    GameControl.camera.Target.X,
                    GameControl.camera.Target.Y,
                    GameControl.camera.Target.Z - 1);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                GameControl.camera.Position = new Vector3(
                    GameControl.camera.Position.X,
                    GameControl.camera.Position.Y,
                    GameControl.camera.Position.Z + 1);
                GameControl.camera.Target = new Vector3(
                    GameControl.camera.Target.X,
                    GameControl.camera.Target.Y,
                    GameControl.camera.Target.Z + 1);
            }

            // The "Activate" button
            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                // To make sure you're not holding it down
                if (!Globals.ActivateButtonWasDown)
                {
                    Globals.ActivateButtonWasDown = true;

                    // If you're trying to progress a message
                    if (Globals.DisplayTextQueue.Count != 0)
                    {
                        Globals.DisplayTextQueue.Dequeue();
                        Globals.TalkingObjectQueue.Dequeue();
                    }

                    // If you're trying to activate the object in front of you
                    else
                    {
                        if (player.GetTarget() != null) player.GetTarget().Activate();
                    }
                }
            }
            else
            {
                Globals.ActivateButtonWasDown = false;
            }
        }
    }
}