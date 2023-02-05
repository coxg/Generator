using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Generator
{
    public static class CombatManager
    {
        public static bool InCombat => Enemies.Count > 0;
        public static HashSet<GameObject> Enemies = new HashSet<GameObject>();
        private static GameObject preBattleLeader;

        public static void Update()
        {
            if (!Enemies.Any())
            {
                EndCombat();
                return;
            }
            
            // TODO
        }

        public static void StartCombat(HashSet<GameObject> enemies)
        {
            Enemies = enemies;
            preBattleLeader = Globals.Player;
            GameControl.CurrentScreen = GameControl.GameScreen.CombatPlayEvents;

            // TODO: Initial combat queueing
        }

        private static void EndCombat()
        {
            // TODO: Award gold, experience, etc
            Globals.Party.Value.LeaderID = preBattleLeader.ID;
        }
    }
}