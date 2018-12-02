﻿using System;
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
            var speed = 0.0;
            
            // Use controller if available
            var capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            bool ButtonOrKeyDown(Buttons button, Keys key)
            {
                return capabilities.IsConnected ? state.IsButtonDown(button) : Keyboard.GetState().IsKeyDown(key);
            }

            if (capabilities.IsConnected)
            {
                moveHorizontalOffset = state.ThumbSticks.Left.X;
                moveVerticalOffset = state.ThumbSticks.Left.Y;
                speed = Math.Sqrt(
                    player.Speed.CurrentValue 
                    * Math.Sqrt(Math.Pow(state.ThumbSticks.Left.X, 2) 
                                + Math.Pow(state.ThumbSticks.Left.Y, 2)));
            }
            
            // If not, just use the keyboard
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Up)) moveVerticalOffset += 1;
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) moveVerticalOffset -= 1;
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) moveHorizontalOffset -= 1;
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) moveHorizontalOffset += 1;
                speed = (float)Math.Sqrt(player.Speed.CurrentValue);
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
                player.MoveInDirection(radianDirection, (float)speed);
            }

            // Abilities
            if (player.Ability1 != null) player.Ability1.IsPressed = ButtonOrKeyDown(Buttons.LeftTrigger, Keys.D1);
            if (player.Ability2 != null) player.Ability2.IsPressed = ButtonOrKeyDown(Buttons.RightTrigger, Keys.D2);
            if (player.Ability3 != null) player.Ability3.IsPressed = ButtonOrKeyDown(Buttons.LeftShoulder, Keys.D3);
            if (player.Ability4 != null) player.Ability4.IsPressed = ButtonOrKeyDown(Buttons.RightShoulder, Keys.D4);

            // Map rotation
            if (Keyboard.GetState().IsKeyDown(Keys.Q)) GameControl.camera.Rotation = .1f;
            if (Keyboard.GetState().IsKeyDown(Keys.E)) GameControl.camera.Rotation = -.1f;

            // Zoom in/out
            if (ButtonOrKeyDown(Buttons.DPadUp, Keys.OemPlus))
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

            if (ButtonOrKeyDown(Buttons.DPadDown, Keys.OemMinus))
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
            if (ButtonOrKeyDown(Buttons.A, Keys.F))
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