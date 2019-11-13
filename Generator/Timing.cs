﻿using System;
using System.Collections.Generic;

namespace Generator
{
    public static class Timing
    {
        public static int MaxClock = 1000000000;
        public static float GameClock = 0;
        public static Dictionary<int, List<Action>> Events = new Dictionary<int, List<Action>>();

        // The speed at which the game time moves relative to IRL time
        public static float PlayerMovementMagnitude;  // 1 == running, 0 == still
        public static float? GameSpeedOverride = null;
        private static float gameSpeed = 1;
        public static float GameSpeed
        {
            set { GameSpeedOverride = value; }
            get { return gameSpeed; }
        }

        // Keeping track of FPS
        public static int NumDraws = 0;
        public static DateTime[] FrameTimes = new DateTime[30];
        public static bool ShowFPS = true;
        public static float FPS
        {
            get {
                DateTime priorTime = FrameTimes[(int)MathTools.Mod(NumDraws + 1, FrameTimes.Length)];
                if (priorTime == null) return 0;
                DateTime currentTime = FrameTimes[(int)MathTools.Mod(NumDraws, FrameTimes.Length)];
                return FrameTimes.Length / (float)(currentTime - priorTime).TotalSeconds;
            }
        }

        public static void UpdateGameSpeed()
        // Update the game speed based on any overrides, whether or not we're in combat, and the player's Sense
        {
            if (Globals.CurrentConversation != null)
            {
                gameSpeed = 0;
            }
            else if (GameSpeedOverride != null)
            {
                gameSpeed = (float)GameSpeedOverride;
            }
            else if (Globals.Party.InCombat)
            {
                gameSpeed = 1 - ((1 - PlayerMovementMagnitude) * (float)Math.Sqrt(Globals.Player.Sense.CurrentValue / 100f));
            }
            else
            {
                gameSpeed = 1;
            }
        }

        public static void Update()
        {
            // Recompute the in-game time
            UpdateGameSpeed();
            var newClock = MathTools.Mod(GameClock + GameSpeed, MaxClock);

            // Launch any events scheduled to happen between the last time and the new time
            // TODO: Handle the case when exceeding MaxClock... if we expect people to exceed 482 days of game time
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
                Events[scheduledTime] = new List<Action>() { action };
            }
        }
    }
}