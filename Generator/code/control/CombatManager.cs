using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public static class CombatManager
    {
        public static bool InCombat => Enemies.Count > 0;
        public static HashSet<GameObject> Enemies = new HashSet<GameObject>();
        
        public enum CombatScreen
        {
            SelectionScreen,
            AbilitySelectionScreen,
            AbilityTargetingScreen,
            ItemSelectionScreen,
            ItemTargetingScreen,
            MovementScreen,
            LookAroundScreen
        }

        public static CombatScreen SelectedScreen = SelectedScreen;
        
        public static GameObject GetReadyPartyMember()
        {
            foreach (var partyMember in Globals.Party.Value.GetMembers())
            {
                if (partyMember.IsReady)
                {
                    return partyMember;
                }
            }

            return null;
        }

        public static GameObject GetReadyEnemy()
        {
            foreach (var enemy in Enemies)
            {
                if (enemy.IsReady)
                {
                    return enemy;
                }
            }

            return null;
        }

        public static void StartCombat(HashSet<GameObject> enemies)
        {
            Enemies = enemies;
            
            // TODO: How do we want to do the initial queueing?
            foreach (var enemy in Enemies)
            {
                // enemy.Cast();
            }
        }
    }
}