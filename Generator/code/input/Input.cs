using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Generator.code.objects;

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

        public static GamePadCapabilities Capabilities;
        public static GamePadState ControllerState;
        public static KeyboardState KeyboardState;
        public static MouseState MouseState;
        public static Vector3 CursorPosition;
        public static InputMode Mode;
        private static bool leftClickIsPressed;

        public static void Update()
        {
            if (GameControl.CurrentScreen == GameControl.GameScreen.CombatPlayEvents)
            {
                // If I allow for any input during combat animations (pausing etc) this will need to change
                CombatManager.Update();
                return;
            }

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
                if (!leftClickIsPressed)
                {
                    Globals.Log("Clicking on (" + Math.Round(CursorPosition.X) + ", " + Math.Round(CursorPosition.Y) + ")");
                }
                leftClickIsPressed = true;
            }
            else
            {
                leftClickIsPressed = false;
            }
            
            switch (GameControl.CurrentScreen)
            {
                case GameControl.GameScreen.WalkingAround:
                    ProcessNonCombatInput();
                    break;
                case GameControl.GameScreen.Conversation:
                    ProcessConversationInput();
                    break;
                case GameControl.GameScreen.CombatOptionSelector:
                    Selectors.CombatScreenSelector.Update();
                    break;
                case GameControl.GameScreen.AbilitySelector:
                    Selectors.AbilitySelector.Update();
                    break;
                case GameControl.GameScreen.AbilityTargeter:
                    Targeters.AbilityTargeter.Update();
                    break;
                case GameControl.GameScreen.ItemSelector:
                    Selectors.ItemSelector.Update();
                    break;
                case GameControl.GameScreen.ItemTargeter:
                    Targeters.ItemTargeter.Update();
                    break;
                case GameControl.GameScreen.CombatLookAround:
                    Targeters.LookAroundTargeter.Update();
                    break;
                default:
                    throw new Exception("uh oh!");
            }
        }

        private static void ProcessNonCombatInput()
        {
            // The "Activate" button
            if (KeyBindings.A.IsBeingPressed)
            {
                var target = Globals.Player.GetTarget();
                Globals.Player?.Activate(target);
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
                Globals.CurrentConversation = GameObjects.OldMan.Conversation;
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
                Globals.CurrentConversation = GameObjects.OldMan.Conversation;
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
                // Selectors.PlayerSelector.Update();
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

        private static Vector2 GetLeftStickInput()
        {
            if (Mode == InputMode.Controller)
            {
                // TODO
            }

            return new Vector2(
                Convert.ToSingle(KeyBindings.Right.IsBeingPressed) - Convert.ToSingle(KeyBindings.Left.IsBeingPressed), 
                Convert.ToSingle(KeyBindings.Up.IsBeingPressed) - Convert.ToSingle(KeyBindings.Down.IsBeingPressed));
        }

        private static void ProcessMovementInput(GameObject gameObject)
        {
            if (KeyBindings.Right.IsBeingPressed || KeyBindings.Left.IsBeingPressed || 
                KeyBindings.Up.IsBeingPressed || KeyBindings.Down.IsBeingPressed)
            {
                var movementInput = GetLeftStickInput();
                gameObject.MoveInDirection(
                    (float)MathTools.Angle(Vector3.Zero, new Vector3(movementInput.X, movementInput.Y, 0)));
                Globals.Log(gameObject + " moves to " + gameObject.Position);
            }
        }
    }
}