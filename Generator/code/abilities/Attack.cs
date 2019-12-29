using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Attack : Ability
    {
        public Attack() : base(
            "Attack",
            cooldown: .2f,
            keepCasting: true) { }

        public override void Start()
        {
            SourceObject.IsSwinging = true;

            // Figure out which one you hit
            var target = SourceObject.GetTargetInRange(SourceObject.EquippedWeapon.Range);

            // Deal damage
            if (target != null)
            {
                Globals.Log(SourceObject + " attacks, hitting " + target + ".");
                SourceObject.DealDamage(target, SourceObject.EquippedWeapon.Damage + SourceObject.Strength.CurrentValue);
            }
            else
            {
                Globals.Log(SourceObject + " attacks and misses.");
            }
        }

        public override Dictionary<string, float> GetPriorityValues(
            IEnumerable<GameObject> allies, IEnumerable<GameObject> enemies, List<GameObject> projectiles)
        {
            var damage = 0f;
            var target = SourceObject.GetTargetInRange(SourceObject.EquippedWeapon.Range);
            if (target != null)
            {
                damage = target.DamageToTake(SourceObject.EquippedWeapon.Damage + SourceObject.Strength.CurrentValue) / target.Health.Max;
            }


            return new Dictionary<string, float>
            {
                { "Damage",     damage },
                { "Healing",    0f },
                { "Ailments",   0f },
                { "Slows",      0f },
                { "Distance",   0f }
            };
        }
    }
}
