using System;
using System.Linq;
using System.Collections.Generic;

namespace Generator.code.actions
{
    public class Strategy
    {
        public Strategy(Dictionary<string, float> priorityWeights)
        {
            PriorityWeights = priorityWeights;
        }

        public Dictionary<string, float> PriorityWeights;

        public float GetAbilityPriority(
            Ability ability, IEnumerable<GameObject> allies, IEnumerable<GameObject> enemies, List<GameObject> projectiles)
        {
            // Don't prioritize abilities you can't use
            // If out of range of an ability then it's up to the player to reflect that in the distance priority
            if (!ability.CanUse())
            {
                return 0;
            }

            // Get weighted sum of priorities
            var priorityValues = ability.GetPriorityValues(allies, enemies, projectiles);
            var weightedPriorities = new Dictionary<string, float>();
            foreach (string priorityName in priorityValues.Keys)
            {
                weightedPriorities[priorityName] = priorityValues[priorityName] * PriorityWeights[priorityName];
            }
            var abilityPriority = weightedPriorities.Values.Sum();

            // Weight by efficiency - right now this means "at what threshold do I start doing the calculation?"
            // At some point we could get more advanced:
            // - Change it to be 0 --> go all out, 1 --> do straight calculation, .5 --> maintain (cost ~= regen)
            // - Or should it be something like .5 --> "Willing to spend some extra electricity for some more output"?
            if (ability.SourceObject.Electricity.Current > 0
                && ability.SourceObject.Electricity.Current / ability.SourceObject.Electricity.Max <= PriorityWeights["Efficiency"])
            {
                var cost = Math.Max(ability.ElectricityCost, 1);
                abilityPriority /= PriorityWeights["Efficiency"] * cost;
            }

            return abilityPriority;
        }

        public void Follow(
            GameObject user, IEnumerable<GameObject> allies, IEnumerable<GameObject> enemies, List<GameObject> projectiles)
        {
            float highestPriority = 0;
            Ability highestPriorityAbility = null;
            foreach (Ability ability in user.Abilities)
            {
                ability.IsTryingToUse = false;
                var abilityPriority = GetAbilityPriority(ability, allies, enemies, projectiles);
                if (abilityPriority > highestPriority)
                {
                    highestPriority = abilityPriority;
                    highestPriorityAbility = ability;
                }
            }

            if (highestPriorityAbility != null)
            {
                highestPriorityAbility.IsTryingToUse = true;
            }
        }

        public static Dictionary<string, Strategy> Strategies = new Dictionary<string, Strategy>
        {
            { "Whatevs",
               new Strategy(new Dictionary<string, float>
               {
                   { "Damage",     .5f },
                   { "Healing",    .5f },
                   { "Ailments",   .5f },
                   { "Slows",      .5f },
                   { "Efficiency", .5f },
                   { "Distance",   .5f }
               })
            },
            { "I don't have time for this",
               new Strategy(new Dictionary<string, float>
               {
                   { "Damage",     1f },
                   { "Healing",    .25f },
                   { "Ailments",   0f },
                   { "Slows",      0f },
                   { "Efficiency", 0f },
                   { "Distance",   0f }
               })
            },
            { "Stayin' Alive",
               new Strategy(new Dictionary<string, float>
               {
                   { "Damage",     .25f },
                   { "Healing",    1f },
                   { "Ailments",   .25f },
                   { "Slows",      .5f },
                   { "Efficiency", .5f },
                   { "Distance",   1f }
               })
            },
            { "Conserve your resources!!",
               new Strategy(new Dictionary<string, float>
               {
                   { "Damage",     .5f },
                   { "Healing",    .5f },
                   { "Ailments",   .5f },
                   { "Slows",      .5f },
                   { "Efficiency", 1f },
                   { "Distance",   .5f }
               })
            },
            { "Debaser",
               new Strategy(new Dictionary<string, float>
               {
                   { "Damage",     .5f },
                   { "Healing",    .5f },
                   { "Ailments",   1f },
                   { "Slows",      .75f },
                   { "Efficiency", .5f },
                   { "Distance",   .5f }
               })
            },
            { "Keep 'em in my sights",
               new Strategy(new Dictionary<string, float>
               {
                   { "Damage",     .5f },
                   { "Healing",    .5f },
                   { "Ailments",   .5f },
                   { "Slows",      1f },
                   { "Efficiency", .5f },
                   { "Distance",   .5f }
               })
            }
        };
    }
}
