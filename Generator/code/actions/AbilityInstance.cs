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
            GameObject initialTargetObject,
            Vector3 targetOffset)
        {
            Name = name;
            _ability = Abilities.get(Name);

            SourceObject = sourceObject;
            InitialTargetObject = initialTargetObject;
            TargetOffset = targetOffset;
        }

        public String Name;
        
        public GameObject SourceObject;
        public GameObject InitialTargetObject;
        
        public Vector3 InitialTargetPos;
        public Vector3 TargetOffset;
        
        public float RemainingCastTime;
        public float RemainingRecharge;

        [JsonIgnore] 
        private Ability _ability;
        public Ability Ability
        {
            get => _ability;
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
            Globals.Log(SourceObject + " casts " + this);
            Ability.Animation?.Stop();
            SourceObject.AbilityCooldowns[Name] = Ability.Cooldown;
            RemainingRecharge = Ability.Recharge;

            var targetPosition = SourceObject.Position += TargetOffset;
            if (Ability.Collision)
            {
                targetPosition = SourceObject.GetFirstTargetInRange(TargetOffset.Length())?.Position ?? targetPosition;
            }
            
            var targetPositions = MathTools.GetCoordinatesInCircle(
                (int)targetPosition.X, (int)targetPosition.Y, Ability.Radius);

            var targetObjects = new List<GameObject>();
            foreach (var eachTargetPosition in targetPositions)
            {
                Ability.LocationEffect?.Invoke(SourceObject, eachTargetPosition);
                targetObjects.Union(Globals.GameObjectManager.Get(eachTargetPosition.X, eachTargetPosition.Y));
            }

            foreach (var targetObject in targetObjects)
            {
                Ability.ObjectEffect?.Invoke(SourceObject, targetObject);
                SourceObject.DealDamage(targetObject, Ability.Damage, Ability.Type);
                targetObject.Heal(Ability.Healing);
                targetObject.Ailments.UnionWith(Ability.Ailments);  // TODO: Ailments need their own getter/setter logic
            }
        }
    }
}