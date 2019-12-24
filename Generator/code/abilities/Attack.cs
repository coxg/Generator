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
    }
}
