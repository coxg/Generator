using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class Targeter
    {
        private Vector3 Target = Globals.Player.Position;
        private BoundAction ActivationAction;
        private BoundAction CancelAction;
        private Func<Vector3, List<Vector3>> GetTargetsMethod;

        public Targeter(BoundAction activationAction, BoundAction cancelAction, 
            Func<Vector3, List<Vector3>> getTargetsMethod = null)
        {
            ActivationAction = activationAction;
            CancelAction = cancelAction;
            GetTargetsMethod = getTargetsMethod;
        }

        public void Update()
        {
            switch (Input.Mode)
            {
                case Input.InputMode.Controller:
                    ProcessControllerInput();
                    break;
                case Input.InputMode.MouseKeyboard:
                    ProcessMouseKeyboardInput();
                    break;
                default:
                    throw new Exception("Uh oh!");
            }
            ActivationAction?.Update();
            CancelAction?.Update();
        }

        public void Reset()
        {
            Target = Globals.Player.Position;
        }

        public Vector3 GetTarget()
        {
            return new Vector3(
                (float)Math.Floor(Target.X), (float)Math.Ceiling(Target.Y), Target.Z);
        }

        public List<Vector3> GetTargets()
        {
            var target = GetTarget();
            return GetTargetsMethod != null ? GetTargetsMethod.Invoke(target) : new List<Vector3> { target };
        }

        private void ProcessControllerInput()
        {
            var isHorizontalPressed = Math.Abs(Input.ControllerState.ThumbSticks.Left.X) > Input.ControllerTolerance;
            if (isHorizontalPressed)
            {
                Target.X += Input.ControllerState.ThumbSticks.Left.X / 20;
            }

            var isVerticalPressed = Math.Abs(Input.ControllerState.ThumbSticks.Left.Y) > Input.ControllerTolerance;
            if (isVerticalPressed)
            {
                Target.Y += Input.ControllerState.ThumbSticks.Left.Y / 20;
            }

            if (!isHorizontalPressed && !isVerticalPressed)
            {
                Target.X = (float)Math.Round(Target.X);
                Target.Y = (float)Math.Round(Target.Y);
            }
        }

        private void ProcessMouseKeyboardInput()
        {
            Target = Input.CursorPosition;
        }
    }
}