using System;
using System.Collections.Generic;

namespace Generator
{
    public class Selector<T>
    {
        public List<T> Options;

        private int StartIndex;
        private int Index;
        private KeyBinding Incrementor;
        private KeyBinding Decrementor;
        private BoundAction ActivationAction;
        private BoundAction CancelAction;
        private Action OnUpdate;

        public Selector(List<T> options, KeyBinding decrementor, KeyBinding incrementor, Action onUpdate = null, 
            BoundAction activationAction = null, BoundAction cancelAction = null, int startIndex = 0)
        {
            Options = options;
            
            Incrementor = incrementor;
            Decrementor = decrementor;
            OnUpdate = onUpdate;

            ActivationAction = activationAction;
            CancelAction = cancelAction;
            
            StartIndex = Index = startIndex;
        }

        public void Update()
        {
            if (Incrementor.IsBeingPressed || Decrementor.IsBeingPressed)
            {
                if (Incrementor.IsBeingPressed)
                {
                    Index = MathTools.Mod(Index + 1, Options.Count);
                    Globals.Log(typeof(T).Name + " selector now selecting " + GetSelection());
                }
                if (Decrementor.IsBeingPressed)
                {
                    Index = MathTools.Mod(Index - 1, Options.Count);
                    Globals.Log(typeof(T).Name + " selector now selecting " + GetSelection());
                }
                OnUpdate?.Invoke();
            }

            ActivationAction?.Update();
            CancelAction?.Update();
        }

        public T GetSelection()
        {
            return Options[Index];
        }

        public void Reset()
        {
            Index = StartIndex;
        }
    }
}