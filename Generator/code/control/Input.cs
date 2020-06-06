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
        public static GamePadState ControllerState;
        public static MouseState MouseState;
        public static Vector3 CursorPosition;

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
                IsPressed = Capabilities.IsConnected & ControllerState.IsButtonDown(Button) 
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

        public static void ProcessInput(GameObject player)
        {
            ControllerState = GamePad.GetState(PlayerIndex.One);
            MouseState = Mouse.GetState();
            CursorPosition = MathTools.PositionFromPixels(new Vector2(MouseState.X, MouseState.Y));
            Capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            foreach (KeyBinding keyBinding in KeyBindings.Values)
            {
                keyBinding.Update();
            }

            // Determine what mode we're in
            if (Globals.CurrentConversation != null)
            {
                ProcessConversationInput();
            }

            // We're walking around, clicking stuff, etc
            else
            {
                // The "Activate" button
                if (KeyBindings["a"].IsBeingPressed)
                {
                    var target = player.GetClosest(player.GetTargets());
                    target?.Activate(player);
                }

                bool anyAbilitiesBeingUsed = ProcessAbilityInput(player);

                ProcessMovementInput(player, anyAbilitiesBeingUsed);

                // Save the game
                if (KeyBindings["start"].IsBeingReleased && KeyBindings["start"].PressedDuration <= .5f)
                {
                    Saving.Quicksave();
                }
                // TODO: Once menus are a thing we won't need to check if the CurrentConversation is null
                else if (Globals.CurrentConversation == null && KeyBindings["start"].IsPressed && KeyBindings["start"].PressedDuration >= .5f)
                {
                    Globals.CurrentConversation = Globals.GameObjectManager.Get("old man").Conversation;
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
                    Globals.CurrentConversation = Globals.GameObjectManager.Get("old man").Conversation;
                    Globals.CurrentConversation.CurrentChoicesIndex = 1;
                }

                // Creative mode controls
                if (Globals.CreativeMode)
                {
                    // Scroll left
                    if (KeyBindings["left"].IsBeingPressed)
                    {
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex - 1, Globals.TileManager.TileSheet.Tiles.Count);
                    }

                    // Scroll right
                    if (KeyBindings["right"].IsBeingPressed)
                    {
                        Globals.CreativeObjectIndex = (int)MathTools.Mod(
                            Globals.CreativeObjectIndex + 1, Globals.TileManager.TileSheet.Tiles.Count);
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
                    GameControl.camera.Height *= 2;
                    Globals.Log(GameControl.camera.Height);
                }
                if (KeyBindings["up"].IsBeingPressed)
                {
                    GameControl.camera.Height /= 2;
                    GameControl.camera.Height = Math.Max(GameControl.camera.Height, 5);
                    Globals.Log(GameControl.camera.Height);
                }
            }
        }

        private static void ProcessConversationInput()
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

        private static bool ProcessAbilityInput(GameObject player)
        {
            // Abilities
            bool anyAbilitiesBeingUsed = false;
            if (player.Abilities.Count > 0)
            {
                player.Abilities[0].IsTryingToUse = KeyBindings["l"].IsPressed;
                if (player.Abilities[0].IsTryingToUse)
                {
                    anyAbilitiesBeingUsed = true;
                }
            }
            if (player.Abilities.Count > 1)
            {
                player.Abilities[1].IsTryingToUse = KeyBindings["r"].IsPressed;
                if (player.Abilities[1].IsTryingToUse)
                {
                    anyAbilitiesBeingUsed = true;
                }
            }
            if (player.Abilities.Count > 2)
            {
                player.Abilities[2].IsTryingToUse = KeyBindings["lb"].IsPressed;
                if (player.Abilities[2].IsTryingToUse)
                {
                    anyAbilitiesBeingUsed = true;
                }
            }
            if (player.Abilities.Count > 3)
            {
                player.Abilities[3].IsTryingToUse = KeyBindings["rb"].IsPressed;
                if (player.Abilities[3].IsTryingToUse)
                {
                    anyAbilitiesBeingUsed = true;
                }
            }

            return anyAbilitiesBeingUsed;
        }

        private static void ProcessMovementInput(GameObject player, bool anyAbilitiesBeingUsed)
        {
            // Convert from actual movement input to direction offsets
            var moveVerticalOffset = 0.0;
            var moveHorizontalOffset = 0.0;

            // Use controller to calculate movement/direction if available and being used
            float directionHorizontalOffset = 0;
            float directionVerticalOffset = 0;
            player.MovementDirection = null;
            if (Capabilities.IsConnected & !(
                ControllerState.ThumbSticks.Right.X == 0 & ControllerState.ThumbSticks.Right.Y == 0
                & ControllerState.ThumbSticks.Left.X == 0 & ControllerState.ThumbSticks.Left.Y == 0))
            {

                directionHorizontalOffset = ControllerState.ThumbSticks.Right.X;
                directionVerticalOffset = ControllerState.ThumbSticks.Right.Y;
                moveHorizontalOffset = ControllerState.ThumbSticks.Left.X;
                moveVerticalOffset = ControllerState.ThumbSticks.Left.Y;
                player.MovementSpeed = (float)Math.Min(1, Math.Sqrt(
                    Math.Pow(ControllerState.ThumbSticks.Left.X, 2)
                    + Math.Pow(ControllerState.ThumbSticks.Left.Y, 2)));

                // We're not using the mouse for input so null this out
                player.MovementTarget = null;
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
                    player.MovementTarget = null;
                    player.MovementSpeed = 1;
                }

                // If the mouse is pressed then start moving to its position
                if (MouseState.LeftButton == ButtonState.Pressed)
                {
                    player.MovementTarget = CursorPosition - new Vector3(player.Size.X / 2, player.Size.Y / 2, 0);
                }

                // If using any abilities then also look in that direction
                if (anyAbilitiesBeingUsed)
                {
                    var playerCenter = player.Center;
                    directionHorizontalOffset = CursorPosition.X - playerCenter.X;
                    directionVerticalOffset = CursorPosition.Y - playerCenter.Y;
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
                player.MovementDirection = radianDirection;
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
                player.DirectionOverride = radianDirection;
            }
            else
            {
                player.DirectionOverride = null;
            }
        }
    }
}