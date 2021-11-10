using System;

namespace Generator
{
    public class BoundAction
    {
        private KeyBinding KeyBinding;
        private Action Action;

        public BoundAction(KeyBinding keyBinding, Action action)
        {
            KeyBinding = keyBinding;
            Action = action;
        }

        public void Update()
        {
            if (KeyBinding.IsBeingPressed)
            {
                Action.Invoke();
            }
        }
    }
}