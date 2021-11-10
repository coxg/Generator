using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Generator
{
    public static class Input
    {
        public enum InputMode
        {
            MouseKeyboard,
            Controller
        }

        public static float ControllerTolerance = .05f;
        public static Targeter EverythingTargeter = new Targeter();
        
        public static GamePadCapabilities Capabilities;
        public static GamePadState ControllerState;
        public static KeyboardState KeyboardState;
        public static MouseState MouseState;
        public static Vector3 CursorPosition;
        public static InputMode Mode;

        public static void Update()
        {
            ControllerState = GamePad.GetState(PlayerIndex.One);
            Capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();
            CursorPosition = MathTools.PositionFromPixels(new Vector2(MouseState.X, MouseState.Y));

            foreach (KeyBinding keyBinding in KeyBindings.KeyMap.Values)
            {
                keyBinding.Update();
            }
            if (MouseState.LeftButton == ButtonState.Pressed)
            {
                Mode = InputMode.MouseKeyboard;
            }

            if (Globals.CurrentConversation != null)
            {
                ProcessConversationInput();
            }
            
            else if (CombatManager.InCombat)
            {
                ProcessCombatInput();
            }

            else
            {
                ProcessNonCombatInput();
            }
        }

        private static void ProcessCombatInput()
        {
            switch (CombatManager.SelectedScreen)
            {
                case CombatManager.CombatScreen.SelectionScreen:
                    Selectors.CombatScreenSelector.Update();
                    break;
                case CombatManager.CombatScreen.AbilityScreen:
                    Selectors.AbilitySelector.Update();
                    break;
                case CombatManager.CombatScreen.ItemScreen:
                    Selectors.ItemSelector.Update();
                    break;
                case CombatManager.CombatScreen.MovementScreen:
                    // TODO: This
                    break;
                case CombatManager.CombatScreen.TargetingScreen:
                    // TODO: This
                    break;
                default:
                    throw new Exception("Uh oh! You shouldn't be here!");
            }
        }

        private static void ProcessNonCombatInput()
        {
            // The "Activate" button
            if (KeyBindings.A.IsBeingPressed)
            {
                var target = Globals.Player.GetClosest(Globals.Player.GetTargets());
                if (target != null)
                {
                    Globals.Player.Activate(target);
                }
            }

            ProcessMovementInput(Globals.Player);

            // Save the game
            if (KeyBindings.Start.IsBeingReleased && KeyBindings.Start.PressedDuration <= .5f)
            {
                Saving.Quicksave();
            }
            else if (KeyBindings.Start.IsPressed && KeyBindings.Start.PressedDuration >= .5f)
            {
                // TODO: Replace this with a menu
                Globals.CurrentConversation = Globals.GameObjectManager.Get("old man").Conversation;
                Globals.CurrentConversation.CurrentChoicesIndex = 5;
            }

            // Load the game
            if (KeyBindings.Select.IsBeingReleased && KeyBindings.Select.PressedDuration <= .5f)
            {
                Saving.Quickload();
            }
            else if (KeyBindings.Select.IsPressed && KeyBindings.Select.PressedDuration >= .5f)
            {
                // TODO: Replace this with a menu
                Globals.CurrentConversation = Globals.GameObjectManager.Get("old man").Conversation;
                Globals.CurrentConversation.CurrentChoicesIndex = 1;
            }

            // Creative mode controls
            if (Globals.CreativeMode)
            {
                Selectors.CreativeTileSelector.Update();
            }

            // If not in creative mode then use these key bindings to switch characters
            else
            {
                Selectors.PlayerSelector.Update();
            }

            // Camera controls
            Selectors.ZoomSelector.Update();
        }

        private static void ProcessConversationInput()
        // TODO: Can this be refactored to use selectors?
        {
            // Advance the conversation based on what's currently selected
            if (KeyBindings.A.IsBeingPressed)
            {
                Globals.CurrentConversation.Advance();
            }

            // Change selection if we're choosing between options
            if (KeyBindings.Down.IsBeingPressed)
            {
                Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex = MathTools.Mod(
                    Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex + 1,
                    Globals.CurrentConversation.CurrentChoices.Nodes.Count);
            }
            if (KeyBindings.Up.IsBeingPressed)
            {
                Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex = MathTools.Mod(
                    Globals.CurrentConversation.CurrentChoices.CurrentNodeIndex - 1,
                    Globals.CurrentConversation.CurrentChoices.Nodes.Count);
            }

            // Select the option to back us out of the conversation
            if (KeyBindings.B.IsBeingPressed)
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

        private static void ProcessMovementInput(GameObject player)
        {
            // Convert from actual movement input to direction offsets
            var moveVerticalOffset = 0.0;
            var moveHorizontalOffset = 0.0;

            player.MovementDirection = null;
            if (Mode == InputMode.Controller)
            {
                moveHorizontalOffset = ControllerState.ThumbSticks.Left.X;
                moveVerticalOffset = ControllerState.ThumbSticks.Left.Y;
                player.MovementSpeedMultiplier = (float)Math.Min(1, Math.Sqrt(
                    Math.Pow(ControllerState.ThumbSticks.Left.X, 2)
                    + Math.Pow(ControllerState.ThumbSticks.Left.Y, 2)));

                // We're not using the mouse for input so null this out
                player.MovementTarget = null;
            }

            else if (MouseState.LeftButton == ButtonState.Pressed)
            {
                player.MovementTarget = CursorPosition - new Vector3(player.Size.X / 2, player.Size.Y / 2, 0);
            }
            
            else
            {
                if (KeyboardState.IsKeyDown(Keys.W)) moveVerticalOffset += 1;
                if (KeyboardState.IsKeyDown(Keys.S)) moveVerticalOffset -= 1;
                if (KeyboardState.IsKeyDown(Keys.A)) moveHorizontalOffset -= 1;
                if (KeyboardState.IsKeyDown(Keys.D)) moveHorizontalOffset += 1;
                player.MovementTarget = null;
                player.MovementSpeedMultiplier = 1;
            }

            if (Math.Abs(moveHorizontalOffset) > ControllerTolerance 
                || Math.Abs(moveVerticalOffset) > ControllerTolerance)
            {
                // Convert from offsets to radians
                var radianDirection = (float)MathTools.Angle(
                    Vector3.Zero, new Vector3((float)moveHorizontalOffset, (float)moveVerticalOffset, 0));

                // Apply offset from map rotation
                radianDirection -= GameControl.camera.Rotation;
                radianDirection = MathTools.Mod(radianDirection, MathHelper.TwoPi);

                // Move in that direction
                player.MovementDirection = radianDirection;
            }
        }
    }
}