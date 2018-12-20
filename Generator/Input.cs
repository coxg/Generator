using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Generator
{
    public static class Input
    {
        public static bool ActivateButtonWasDown = false;
        public static bool SaveButtonWasDown = false;
        public static bool CreativeScrollLeftButton = false;
        public static bool CreativeScrollRightButton = false;

        public static void GetInput(GameObject player)
        {
            // Convert from actual movement input to direction offsets
            var moveVerticalOffset = 0.0;
            var moveHorizontalOffset = 0.0;
            var speed = 0.0;

            // Use controller to calculate movement/direction if available and being used
            var capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            float directionHorizontalOffset = 0;
            float directionVerticalOffset = 0;
            if (capabilities.IsConnected & !(
                state.ThumbSticks.Right.X == 0 & state.ThumbSticks.Right.Y == 0 
                & state.ThumbSticks.Left.X == 0 & state.ThumbSticks.Left.Y == 0))
            {
                directionHorizontalOffset = state.ThumbSticks.Right.X;
                directionVerticalOffset = state.ThumbSticks.Right.Y;
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

            // Move in the direction specified
            if (moveHorizontalOffset != 0 || moveVerticalOffset != 0)
            {
                // Convert from offsets to radians
                var radianDirection = (float) Math.Atan2(moveHorizontalOffset, moveVerticalOffset);

                // Apply offset from map rotation
                radianDirection -= GameControl.camera.Rotation;
                radianDirection = MathTools.Mod(radianDirection, 2f * (float) Math.PI);

                // Move in that direction
                player.MoveInDirection(radianDirection, (float)speed);
            }
            else
            {
                player.IsWalking = false;
            }

            // Convert from direction offsets to radian direction
            if (directionHorizontalOffset != 0 || directionVerticalOffset != 0)
            {
                // Convert from offsets to radians
                var radianDirection = (float)Math.Atan2(directionHorizontalOffset, directionVerticalOffset);

                // Apply offset from map rotation
                radianDirection -= GameControl.camera.Rotation;
                radianDirection = MathTools.Mod(radianDirection, 2f * (float)Math.PI);

                // Look in that direction
                player.Direction = radianDirection;
            }

            bool ButtonOrKeyDown(Buttons button, Keys key)
            {
                return capabilities.IsConnected & state.IsButtonDown(button) | Keyboard.GetState().IsKeyDown(key);
            }

            // Abilities
            if (player.Ability1 != null) player.Ability1.IsPressed = ButtonOrKeyDown(Buttons.LeftTrigger, Keys.D1);
            if (player.Ability2 != null) player.Ability2.IsPressed = ButtonOrKeyDown(Buttons.RightTrigger, Keys.D2);
            if (player.Ability3 != null) player.Ability3.IsPressed = ButtonOrKeyDown(Buttons.LeftShoulder, Keys.D3);
            if (player.Ability4 != null) player.Ability4.IsPressed = ButtonOrKeyDown(Buttons.RightShoulder, Keys.D4);

            // The "Activate" button
            if (ButtonOrKeyDown(Buttons.A, Keys.F))
            {
                // To make sure you're not holding it down
                if (!ActivateButtonWasDown)
                {
                    ActivateButtonWasDown = true;

                    // If you're trying to progress a message
                    if (Globals.DisplayTextQueue.Count != 0)
                    {
                        // Removes text box if player looks away from active object
                        if (player.GetTarget() != Globals.TalkingObjectQueue.Peek())
                        {
                            Globals.DisplayTextQueue.Clear();
                            Globals.TalkingObjectQueue.Clear();
                        }
                        else
                        {
                            Globals.DisplayTextQueue.Dequeue();
                            Globals.TalkingObjectQueue.Dequeue();
                        }
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
                ActivateButtonWasDown = false;
            }

            // Save the game
            if (ButtonOrKeyDown(Buttons.Start, Keys.F12))
            {
                // To make sure you're not holding it down
                if (!SaveButtonWasDown)
                {
                    Globals.Log("Saving game");
                    SaveButtonWasDown = true;

                    // Write the grids to disk
                    foreach (Acre acre in Globals.Tiles.Acres) acre.Write();
                }
            }
            else
            {
                SaveButtonWasDown = false;
            }

            // Creative mode controls
            if (Globals.CreativeMode)
            {
                // Scroll left
                if (ButtonOrKeyDown(Buttons.DPadLeft, Keys.OemMinus))
                {
                    if (!CreativeScrollLeftButton)
                    {
                        CreativeScrollLeftButton = true;
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex - 1, Globals.Tiles.BaseObjectIndexes.Count);
                    }
                }
                else
                {
                    CreativeScrollLeftButton = false;
                }

                // Scroll right
                if (ButtonOrKeyDown(Buttons.DPadRight, Keys.OemPlus))
                {
                    if (!CreativeScrollRightButton)
                    {
                        CreativeScrollRightButton = true;
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex + 1, Globals.Tiles.BaseObjectIndexes.Count);
                    }
                }
                else
                {
                    CreativeScrollRightButton = false;
                }
            }
        }
    }
}