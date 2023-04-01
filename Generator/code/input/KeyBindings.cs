using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Input;

namespace Generator
{
    public static class KeyBindings
    {
        public static Dictionary<string, KeyBinding> KeyMap = new Dictionary<string, KeyBinding>();

        static KeyBindings()
        {
            FieldInfo[] fields = typeof(KeyBindings).GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(KeyBinding))
                {
                    KeyMap[field.Name] = (KeyBinding) field.GetValue(null);
                }
            }
        }
            
        public static KeyBinding A = new KeyBinding(Buttons.A, Keys.Q);
        public static KeyBinding B = new KeyBinding(Buttons.B, Keys.E);
        public static KeyBinding X = new KeyBinding(Buttons.X, Keys.E);
        public static KeyBinding Y = new KeyBinding(Buttons.Y, Keys.R);
        public static KeyBinding Up = new KeyBinding(Buttons.DPadUp, Keys.Up);
        public static KeyBinding Down = new KeyBinding(Buttons.DPadDown, Keys.Down);
        public static KeyBinding Left = new KeyBinding(Buttons.DPadLeft, Keys.Left);
        public static KeyBinding Right = new KeyBinding(Buttons.DPadRight, Keys.Right);
        public static KeyBinding L = new KeyBinding(Buttons.LeftTrigger, Keys.D1);
        public static KeyBinding R = new KeyBinding(Buttons.RightTrigger, Keys.D2);
        public static KeyBinding LB = new KeyBinding(Buttons.LeftShoulder, Keys.D3);
        public static KeyBinding RB = new KeyBinding(Buttons.RightShoulder, Keys.D4);
        public static KeyBinding Start = new KeyBinding(Buttons.Start, Keys.Z);
        public static KeyBinding Select = new KeyBinding(Buttons.Back, Keys.X);
    }
}