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
            var moveVerticalOffset = 0;
            var moveHorizontalOffset = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) moveVerticalOffset += 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) moveVerticalOffset -= 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) moveHorizontalOffset -= 1;
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) moveHorizontalOffset += 1;

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

            // Pan the camera
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                GameControl.camera.Position = new Vector3(
                    GameControl.camera.Position.X,
                    GameControl.camera.Position.Y + 1,
                    GameControl.camera.Position.Z);
                GameControl.camera.Target = new Vector3(
                    GameControl.camera.Target.X,
                    GameControl.camera.Target.Y + 1,
                    GameControl.camera.Target.Z);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                GameControl.camera.Position = new Vector3(
                    GameControl.camera.Position.X,
                    GameControl.camera.Position.Y - 1,
                    GameControl.camera.Position.Z);
                GameControl.camera.Target = new Vector3(
                    GameControl.camera.Target.X,
                    GameControl.camera.Target.Y - 1,
                    GameControl.camera.Target.Z);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                GameControl.camera.Position = new Vector3(
                    GameControl.camera.Position.X - 1,
                    GameControl.camera.Position.Y,
                    GameControl.camera.Position.Z);
                GameControl.camera.Target = new Vector3(
                    GameControl.camera.Target.X - 1,
                    GameControl.camera.Target.Y,
                    GameControl.camera.Target.Z);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                GameControl.camera.Position = new Vector3(
                    GameControl.camera.Position.X + 1,
                    GameControl.camera.Position.Y,
                    GameControl.camera.Position.Z);
                GameControl.camera.Target = new Vector3(
                    GameControl.camera.Target.X + 1,
                    GameControl.camera.Target.Y,
                    GameControl.camera.Target.Z);
            }

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