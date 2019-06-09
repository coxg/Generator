using System;
using System.Collections.Generic;

namespace Generator
{
    public static class Timer
    {
        public static int MaxClock = 1000000000;
        public static int Clock = 0;
        public static Dictionary<int, List<Action>> Events = new Dictionary<int, List<Action>>();

        public static void Update()
        // Launch any events scheduled to happen
        {
            Clock = (int)MathTools.Mod(Clock + 1, MaxClock);
            if (Events.ContainsKey(Clock))
            {
                foreach (Action action in Events[Clock])
                {
                    action();
                }
                Events.Remove(Clock);
            }
        }

        public static void AddEvent(float seconds, Action action)
        // Schedules an event to happen after a delay
        {
            int scheduledTime = (int)MathTools.Mod(Clock + Globals.RefreshRate * seconds, MaxClock);
            if (Events.ContainsKey(scheduledTime))
            {
                Events[scheduledTime].Add(action);
            }
            else
            {
                Events[scheduledTime] = new List<Action>() { action };
            }
        }
    }
}