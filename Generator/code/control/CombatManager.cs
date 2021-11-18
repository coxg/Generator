using System.Collections.Generic;
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

            var readyMember = GetReadyPartyMember();
            if (readyMember != null)
            {
                Globals.Log(readyMember + "'s turn");
                GameControl.CurrentScreen = GameControl.GameScreen.CombatOptionSelector;
                Globals.Party.Value.LeaderID = readyMember.ID;
                return;
            }

            var readyEnemy = GetReadyEnemy();
            if (readyEnemy != null)
            {
                Globals.Log(readyEnemy + "'s turn");
                // TODO: Perform some combat AI
            }
        }

        private static GameObject GetReadyPartyMember()
        {
            return Globals.Party.Value.GetMembers().FirstOrDefault(partyMember => partyMember.IsReady);
        }

        private static GameObject GetReadyEnemy()
        {
            return Enemies.FirstOrDefault(enemy => enemy.IsReady);
        }

        public static void StartCombat(HashSet<GameObject> enemies)
        {
            Enemies = enemies;
            preBattleLeader = Globals.Player;
            GameControl.CurrentScreen = GameControl.GameScreen.CombatPlayEvents;

            // TODO: Move everyone to nearest square

            // TODO: Initial combat queueing
        }

        private static void EndCombat()
        {
            // TODO: Award gold, experience, etc
            Globals.Party.Value.LeaderID = preBattleLeader.ID;
        }
    }
}