using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Generator
{
    public class AbilityInstance
    {
        public AbilityInstance(
            String name,
            GameObject sourceObject,
            Vector3 target)
        {
            Name = name;
            _ability = Abilities.get(Name);

            SourceObject = sourceObject;
            Target = target;
        }

        public String Name;
        public GameObject SourceObject;
        public Vector3 Target;
        
        public float RemainingCastTime;
        public float RemainingRecharge;

        [JsonIgnore] 
        private Ability _ability;
        public Ability Ability
        {
            get => _ability;
        }
        
        public override string ToString()
        {
            return Name;
        }

        public void StartCasting()
        {
            Globals.Log(SourceObject + " starts casting " + this);
            SourceObject.TakeDamage(Ability.HealthCost, Type.Untyped);
            SourceObject.Mana.Current -= Ability.ManaCost;
            Ability.Animation?.Start();
            RemainingCastTime = Ability.CastTime;
        }

        public void FinishCasting()
        {
            Globals.Log(SourceObject + " casts " + this + " at " + Target.X + ", " + Target.Y);
            Ability.Animation?.Stop();
            SourceObject.AbilityCooldowns[Name] = Ability.Cooldown;
            RemainingRecharge = Ability.Recharge;

            var targetPosition = Target;
            if (Ability.Collision)
            {
                var targetOffset = Target - SourceObject.Position;
                targetPosition = SourceObject.GetFirstTargetInRange(targetOffset.Length())?.Position ?? targetPosition;
            }
            
            var targetPositions = MathTools.GetCoordinatesInCircle(targetPosition, Ability.Radius);

            var targetObjects = new HashSet<GameObject>();
            foreach (var eachTargetPosition in targetPositions)
            {
                Ability.LocationEffect?.Invoke(SourceObject, eachTargetPosition);
                targetObjects.UnionWith(Globals.GameObjectManager.Get(eachTargetPosition.X, eachTargetPosition.Y));
            }

            if (!targetObjects.Any())
            {
                Globals.Log(SourceObject + " misses everything!");
            }

            foreach (var targetObject in targetObjects)
            {
                Globals.Log(SourceObject + " hits " + targetObject + "!");
                Ability.ObjectEffect?.Invoke(SourceObject, targetObject);
                SourceObject.DealDamage(targetObject, Ability.Damage, Ability.Type);
                targetObject.Heal(Ability.Healing);
                targetObject.Ailments.UnionWith(Ability.Ailments);  // TODO: Ailments need their own getter/setter logic
            }
        }
    }
}