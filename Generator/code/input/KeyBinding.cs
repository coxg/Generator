using Microsoft.Xna.Framework.Input;

namespace Generator
{
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
            WasPressed = IsPressed;
            IsPressed = false;
            if (Input.Capabilities.IsConnected && Input.ControllerState.IsButtonDown(Button))
            {
                IsPressed = true;
                Input.Mode = Input.InputMode.Controller;
            }
            else if (Input.KeyboardState.IsKeyDown(Key))
            {
                IsPressed = true;
                Input.Mode = Input.InputMode.MouseKeyboard;
            }

            IsBeingPressed = IsPressed & !WasPressed;
            IsBeingReleased = WasPressed & !IsPressed;

            if (IsPressed)
            {
                PressedDuration += 1f / Globals.RefreshRate;
                NotPressedDuration = 0;
            }
            else
            {
                NotPressedDuration += 1f / Globals.RefreshRate;
                PressedDuration = 0;
            }
        }
    }
}