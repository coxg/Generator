﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Generator
{
    public static class Input
    {
        // TODO: Refactor, each button should have this ability
        public static GamePadCapabilities Capabilities;
        public static GamePadState State;

        public class KeyBinding
        {
            public Buttons Button;
            public Keys Key;
            public bool IsPressed;
            public bool WasPressed;
            public bool IsBeingPressed;
            public bool IsBeingReleased;

            public KeyBinding(Buttons button, Keys key)
            {
                Button = button;
                Key = key;
            }

            public void Update()
            {
                WasPressed = IsPressed;
                IsPressed = Capabilities.IsConnected & State.IsButtonDown(Button) 
                    | Keyboard.GetState().IsKeyDown(Key);
                IsBeingPressed = IsPressed & !WasPressed;
                IsBeingReleased = WasPressed & !IsPressed;
            }
        }

        public static Dictionary<string, KeyBinding> KeyBindings = new Dictionary<string, KeyBinding>()
        {
            { "a",      new KeyBinding(Buttons.A,               Keys.Q) },
            { "b",      new KeyBinding(Buttons.B,               Keys.E) },
            { "x",      new KeyBinding(Buttons.X,               Keys.R) },
            { "y",      new KeyBinding(Buttons.Y,               Keys.F) },
            { "up",     new KeyBinding(Buttons.DPadUp,          Keys.Up) },
            { "down",   new KeyBinding(Buttons.DPadDown,        Keys.Down) },
            { "left",   new KeyBinding(Buttons.DPadLeft,        Keys.Left) },
            { "right",  new KeyBinding(Buttons.DPadRight,       Keys.Right) },
            { "l",      new KeyBinding(Buttons.LeftTrigger,     Keys.D1) },
            { "r",      new KeyBinding(Buttons.RightTrigger,    Keys.D2) },
            { "lb",     new KeyBinding(Buttons.LeftShoulder,    Keys.D3) },
            { "rb",     new KeyBinding(Buttons.RightShoulder,   Keys.D4) },
            { "start",  new KeyBinding(Buttons.Start,           Keys.F1) },
            { "select", new KeyBinding(Buttons.Back,            Keys.F2) },
        };

        public static void GetInput(GameObject player)
        {
            State = GamePad.GetState(PlayerIndex.One);
            Capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            foreach (KeyBinding keyBinding in KeyBindings.Values)
            {
                keyBinding.Update();
            }

            // Determine what mode we're in
            if (Globals.CurrentConversation != null)
            {
                // Advance the conversation based on what's currently selected
                if (KeyBindings["a"].IsBeingPressed)
                {
                    Globals.CurrentConversation.Advance();
                }

                // Change selection if we're choosing between options
                if (KeyBindings["down"].IsBeingPressed)
                {
                    Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex = (int)MathTools.Mod(
                        Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex + 1, 
                        Globals.CurrentConversation.CurrentChoices.Nodes.Count);
                }
                if (KeyBindings["up"].IsBeingPressed)
                {
                    Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex = (int)MathTools.Mod(
                        Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex - 1,
                        Globals.CurrentConversation.CurrentChoices.Nodes.Count);
                }

                // Select the option to back us out of the conversation
                if (KeyBindings["b"].IsBeingPressed)
                {
                    // Prioritize leaving the conversation over returning to the main options, so do it after
                    for (int i = 0; i < Globals.CurrentConversation.CurrentChoices.Nodes.Count; i++)
                    {
                        var node = Globals.CurrentConversation.CurrentChoices.Nodes[i];
                        if (node.GoToChoicesIndex == Globals.CurrentConversation.StartingChoicesIndex)
                        {
                            Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex = i;
                            break;
                        }
                    }
                    for (int i = 0; i < Globals.CurrentConversation.CurrentChoices.Nodes.Count; i++)
                    {
                        var node = Globals.CurrentConversation.CurrentChoices.Nodes[i];
                        if (node.ExitsConversation)
                        {
                            Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex = i;
                            break;
                        }
                    }
                }
            }

            // We're walking around, clicking stuff, etc
            else
            {
                // The "Activate" button
                if (KeyBindings["a"].IsBeingPressed)
                {
                    if (player.GetTargetAtRange() != null) player.GetTargetAtRange().Activate();
                }

                // Convert from actual movement input to direction offsets
                var moveVerticalOffset = 0.0;
                var moveHorizontalOffset = 0.0;
                var speed = 0f;  // Separate from gameSpeed; playerSpeed should be 1 when in combat

                // Use controller to calculate movement/direction if available and being used
                float directionHorizontalOffset = 0;
                float directionVerticalOffset = 0;
                if (Capabilities.IsConnected & !(
                    State.ThumbSticks.Right.X == 0 & State.ThumbSticks.Right.Y == 0
                    & State.ThumbSticks.Left.X == 0 & State.ThumbSticks.Left.Y == 0))
                {
                    directionHorizontalOffset = State.ThumbSticks.Right.X;
                    directionVerticalOffset = State.ThumbSticks.Right.Y;
                    moveHorizontalOffset = State.ThumbSticks.Left.X;
                    moveVerticalOffset = State.ThumbSticks.Left.Y;
                    speed = (float)Math.Min(1, Math.Sqrt(
                        Math.Pow(State.ThumbSticks.Left.X, 2)
                        + Math.Pow(State.ThumbSticks.Left.Y, 2)));
                }

                // If not, just use the keyboard
                else
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.W)) moveVerticalOffset += 1;
                    if (Keyboard.GetState().IsKeyDown(Keys.S)) moveVerticalOffset -= 1;
                    if (Keyboard.GetState().IsKeyDown(Keys.A)) moveHorizontalOffset -= 1;
                    if (Keyboard.GetState().IsKeyDown(Keys.D)) moveHorizontalOffset += 1;
                    speed = 1;
                }

                // Move in the direction specified
                if (moveHorizontalOffset != 0 || moveVerticalOffset != 0)
                {
                    // Convert from offsets to radians
                    var radianDirection = (float)Math.Atan2(moveHorizontalOffset, moveVerticalOffset);

                    // Apply offset from map rotation
                    radianDirection -= GameControl.camera.Rotation;
                    radianDirection = MathTools.Mod(radianDirection, 2f * (float)Math.PI);

                    // Move in that direction
                    player.MoveInDirection(radianDirection, speed);
                    Timing.PlayerMovementMagnitude = speed;
                }
                else
                {
                    player.IsWalking = false;
                    Timing.PlayerMovementMagnitude = 0;
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

                // Abilities
                if (player.Ability1 != null) player.Ability1.IsPressed = KeyBindings["l"].IsPressed;
                if (player.Ability2 != null) player.Ability2.IsPressed = KeyBindings["r"].IsPressed;
                if (player.Ability3 != null) player.Ability3.IsPressed = KeyBindings["lb"].IsPressed;
                if (player.Ability4 != null) player.Ability4.IsPressed = KeyBindings["rb"].IsPressed;

                // Save the game
                if (KeyBindings["start"].IsBeingPressed)
                {
                    Globals.Log("Saving game");

                    // Write the grids to disk
                    foreach (Acre acre in TileManager.Acres) acre.Save();

                    System.IO.Directory.CreateDirectory(Globals.SaveDirectory + "save1");
                    GameObjectManager.Save("save1");
                }

                // Load the game
                if (KeyBindings["up"].IsBeingPressed)
                {
                    Globals.Log("Loading game");
                    GameObjectManager.Load("save1");
                }

                // Creative mode controls
                if (Globals.CreativeMode)
                {
                    // Scroll left
                    if (KeyBindings["left"].IsBeingPressed)
                    {
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex - 1, TileManager.BaseTileIndexes.Count);
                    }

                    // Scroll right
                    if (KeyBindings["right"].IsBeingPressed)
                    {
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex + 1, TileManager.BaseTileIndexes.Count);
                    }
                }

                // If not in creative mode then use these key bindings to switch characters
                else
                {
                    // Scroll left
                    if (KeyBindings["left"].IsBeingPressed)
                    {
                        Globals.PlayerPartyNumber = (int)MathTools.Mod(
                            Globals.PlayerPartyNumber - 1, Globals.Party.Members.Count);
                    }

                    // Scroll right
                    if (KeyBindings["right"].IsBeingPressed)
                    {
                        Globals.PlayerPartyNumber = (int)MathTools.Mod(
                            Globals.PlayerPartyNumber + 1, Globals.Party.Members.Count);
                    }
                }
            }
        }
    }
}