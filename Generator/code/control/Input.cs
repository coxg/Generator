using System;
using System.IO;
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
        public static Vector3 MoveToPosition;
        public static bool MovingToPosition;

        public class KeyBinding
        {
            public Buttons Button;
            public Keys Key;
            public bool IsPressed;
            public bool WasPressed;
            public bool IsBeingPressed;
            public bool IsBeingReleased;
            public float PressedDuration;
            public float NotPressedDuration;

            public KeyBinding(Buttons button, Keys key)
            {
                Button = button;
                Key = key;
            }

            public void Update()
            {
                // Update the states
                WasPressed = IsPressed;
                IsPressed = Capabilities.IsConnected & State.IsButtonDown(Button) 
                    | Keyboard.GetState().IsKeyDown(Key);
                IsBeingPressed = IsPressed & !WasPressed;
                IsBeingReleased = WasPressed & !IsPressed;

                // Update the timing
                if (IsBeingPressed) PressedDuration = 0;
                if (IsBeingReleased) NotPressedDuration = 0;
                if (IsPressed)
                {
                    PressedDuration += 1f / Globals.RefreshRate;
                }
                else
                {
                    NotPressedDuration += 1f / Globals.RefreshRate;
                }
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
            { "start",  new KeyBinding(Buttons.Start,           Keys.Z) },
            { "select", new KeyBinding(Buttons.Back,            Keys.X) },
        };

        public static void Save()
        {
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/input.json"))
            {
                Globals.Serializer.Serialize(file, KeyBindings);
            }
        }

        public static void Load()
        {
            using (StreamReader file = File.OpenText(Saving.CurrentSaveDirectory + "/input.json"))
            {
                KeyBindings = (Dictionary<string, KeyBinding>)
                    Globals.Serializer.Deserialize(file, typeof(Dictionary<string, KeyBinding>));
            }
        }

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
                    if (player.GetTargetAtRange() != null) player.GetTargetAtRange().Activate(player);
                }

                // Abilities
                bool anyAbilitiesBeingUsed = false;
                if (player.Abilities.Count > 0)
                {
                    player.Abilities[0].IsTryingToUse = KeyBindings["l"].IsPressed;
                    anyAbilitiesBeingUsed = true;
                }
                if (player.Abilities.Count > 1)
                {
                    player.Abilities[1].IsTryingToUse = KeyBindings["r"].IsPressed;
                    anyAbilitiesBeingUsed = true;
                }
                if (player.Abilities.Count > 2)
                {
                    player.Abilities[2].IsTryingToUse = KeyBindings["lb"].IsPressed;
                    anyAbilitiesBeingUsed = true;
                }
                if (player.Abilities.Count > 3)
                {
                    player.Abilities[3].IsTryingToUse = KeyBindings["rb"].IsPressed;
                    anyAbilitiesBeingUsed = true;
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

                    // We're not using the mouse for input so null this out
                    MovingToPosition = false;
                }

                // If not, use the mouse/keyboard
                else
                {
                    // If we're trying to use the keyboard for movement
                    var keyboardState = Keyboard.GetState();
                    if (keyboardState.IsKeyDown(Keys.W) | keyboardState.IsKeyDown(Keys.A)
                        | keyboardState.IsKeyDown(Keys.S) | keyboardState.IsKeyDown(Keys.D))
                    {
                        if (keyboardState.IsKeyDown(Keys.W)) moveVerticalOffset += 1;
                        if (keyboardState.IsKeyDown(Keys.S)) moveVerticalOffset -= 1;
                        if (keyboardState.IsKeyDown(Keys.A)) moveHorizontalOffset -= 1;
                        if (keyboardState.IsKeyDown(Keys.D)) moveHorizontalOffset += 1;
                        MovingToPosition = false;
                        speed = 1;
                    }

                    // If the mouse is pressed then start moving to its position
                    var mouseState = Mouse.GetState();
                    var cursorPosition = MathTools.PositionFromPixels(new Vector2(mouseState.X, mouseState.Y));
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        MovingToPosition = true;
                        MoveToPosition = cursorPosition;
                    }

                    // If we're already where we're trying to go then stop moving
                    var playerCenter = player.Center;
                    if (Math.Abs(MoveToPosition.X - playerCenter.X) < .5f
                        && Math.Abs(MoveToPosition.Y - playerCenter.Y) < .5f)
                    {
                        MovingToPosition = false;
                    }

                    // Move in the direction of the last input location
                    if (MovingToPosition)
                    {
                        moveHorizontalOffset = MoveToPosition.X - playerCenter.X;
                        moveVerticalOffset = MoveToPosition.Y - playerCenter.Y;
                        speed = 1;
                    }

                    // If using any abilities then also look in that direction
                    if (anyAbilitiesBeingUsed)
                    {
                        directionHorizontalOffset = cursorPosition.X - playerCenter.X;
                        directionVerticalOffset = cursorPosition.Y - playerCenter.Y;
                    }
                }

                // Move in the direction specified
                if (moveHorizontalOffset != 0 || moveVerticalOffset != 0)
                {
                    // Convert from offsets to radians

                    var radianDirection = (float)MathTools.Angle(
                        Vector3.Zero, new Vector3((float)moveHorizontalOffset, (float)moveVerticalOffset, 0));

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
                    var radianDirection = (float)MathTools.Angle(
                        Vector3.Zero, new Vector3(directionHorizontalOffset, directionVerticalOffset, 0));

                    // Apply offset from map rotation
                    radianDirection -= GameControl.camera.Rotation;
                    radianDirection = MathTools.Mod(radianDirection, MathHelper.TwoPi);

                    // Look in that direction
                    player.Direction = radianDirection;
                }

                // Save the game
                if (KeyBindings["start"].IsBeingReleased && KeyBindings["start"].PressedDuration <= .5f)
                {
                    Saving.Quicksave();
                }
                // TODO: Once menus are a thing we won't need to check if the CurrentConversation is null
                else if (Globals.CurrentConversation == null && KeyBindings["start"].IsPressed && KeyBindings["start"].PressedDuration >= .5f)
                {
                    Globals.CurrentConversation = Globals.Zone.GameObjects.Objects["old man"].Conversation;
                    Globals.CurrentConversation.CurrentChoicesIndex = 5;
                }

                // Load the game
                if (KeyBindings["select"].IsBeingReleased && KeyBindings["select"].PressedDuration <= .5f)
                {
                    Saving.Quickload();
                }
                // TODO: Once menus are a thing we won't need to check if the CurrentConversation is null
                else if (Globals.CurrentConversation == null && KeyBindings["select"].IsPressed && KeyBindings["select"].PressedDuration >= .5f)
                {
                    Globals.CurrentConversation = Globals.Zone.GameObjects.Objects["old man"].Conversation;
                    Globals.CurrentConversation.CurrentChoicesIndex = 1;
                }


                // Creative mode controls
                if (Globals.CreativeMode)
                {
                    // Scroll left
                    if (KeyBindings["left"].IsBeingPressed)
                    {
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex - 1, Globals.Zone.Tiles.TileInfo.Count);
                    }

                    // Scroll right
                    if (KeyBindings["right"].IsBeingPressed)
                    {
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex + 1, Globals.Zone.Tiles.TileInfo.Count);
                    }
                }

                // If not in creative mode then use these key bindings to switch characters
                else
                {
                    // Scroll left
                    if (KeyBindings["left"].IsBeingPressed)
                    {
                        Globals.PlayerPartyNumber.Value = (int)MathTools.Mod(
                            Globals.PlayerPartyNumber.Value - 1, Globals.Party.Value.MemberIDs.Count);
                    }

                    // Scroll right
                    if (KeyBindings["right"].IsBeingPressed)
                    {
                        Globals.PlayerPartyNumber.Value = (int)MathTools.Mod(
                            Globals.PlayerPartyNumber.Value + 1, Globals.Party.Value.MemberIDs.Count);
                    }
                }

                // Camera controls
                if (KeyBindings["down"].IsBeingPressed)
                {
                    GameControl.camera.Height += 10;
                }
                if (KeyBindings["up"].IsBeingPressed)
                {
                    GameControl.camera.Height -= 10;
                    GameControl.camera.Height = Math.Max(GameControl.camera.Height, 5);
                }
            }
        }
    }
}