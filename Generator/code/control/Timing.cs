using System;
using System.Collections.Generic;

namespace Generator
{
    public static class Timing
    {
        public static int MaxClock = int.MaxValue;
        public static float GameClock;  // TODO: This should be in seconds
        public static Dictionary<int, List<Action>> Events = new Dictionary<int, List<Action>>();

        // The speed at which the game time moves relative to IRL time
        public static float GameSpeed = 1;

        // Seconds passed since the last update
        public static float SecondsPassed => GameSpeed / Globals.RefreshRate;

        // Keeping track of FPS
        public static int NumDraws = 0;
        public static DateTime[] FrameTimes = new DateTime[30];
        public static float FPS
        {
            get {
                DateTime priorTime = FrameTimes[(int)MathTools.Mod(NumDraws + 1, FrameTimes.Length)];
                DateTime currentTime = FrameTimes[(int)MathTools.Mod(NumDraws, FrameTimes.Length)];
                return FrameTimes.Length / (float)(currentTime - priorTime).TotalSeconds;
            }
        }

        public static void UpdateGameSpeed()
        {
            if (Globals.CurrentConversation != null)
            {
                GameSpeed = 0;
            }
            else if (Globals.Party.Value.InCombat)
            {
                // TODO
                GameSpeed = 1;
            }
            else
            {
                GameSpeed = 1;
            }
        }

        public static void Update()
        {
            // Recompute the in-game time
            UpdateGameSpeed();
            var newClock = MathTools.Mod(GameClock + GameSpeed, MaxClock);

            // Launch any events scheduled to happen between the last time and the new time
            // Bugs will happen when clock resets... at 482 days of game time
            for (int i = (int)GameClock; i < (int)newClock; i++)
            {
                if (Events.ContainsKey(i))
                {
                    foreach (Action action in Events[i])
                    {
                        action();
                    }
                    Events.Remove(i);
                }
            }

            // Update the clock itself
            GameClock = newClock;
        }

        public static void AddEvent(float seconds, Action action)
        // Schedules an event to happen after a delay
        {
            int scheduledTime = (int)MathTools.Mod(GameClock + Globals.RefreshRate * seconds, MaxClock);
            if (Events.ContainsKey(scheduledTime))
            {
                Events[scheduledTime].Add(action);
            }
            else
            {
                Events[scheduledTime] = new List<Action> { action };
            }
        }
    }
}