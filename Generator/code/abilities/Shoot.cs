using System;
using System.Collections.Generic;
using Generator.code.objects;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Shoot : Ability
    {
        public Shoot() : base(
            "Shoot",
            electricityCost: 0,
            cooldown: .01f,
            keepCasting: true) { }

        public override void Start()
        {
            var position = SourceObject.GetTargetCoordinates(1.5f);
            position.Z += SourceObject.Size.Z / 2;
            var bullet = new GameObject(
                baseHealth: 1,
                position: position,
                size: new Vector3(.01f),
                direction: SourceObject.Direction,
                baseSpeed: 1000,
                ai: new Cached<Action<GameObject>>("BulletAI"),
                collisionEffect: new Cached<Action<GameObject, GameObject>>("BulletCollision"),
                temporary: true,
                components: new Dictionary<string, Component>{},
                lightComponents: new Dictionary<string, LightComponent>
                {
                    {"light", new LightComponent(new Vector3(2), Color.LightPink, 3)}
                });
            Globals.GameObjectManager.Set(bullet);
        }

        public override Dictionary<string, float> GetPriorityValues(
            IEnumerable<GameObject> allies, IEnumerable<GameObject> enemies, List<GameObject> projectiles)
        {
            var targetEnemy = SourceObject.GetClosest(enemies);
            if (targetEnemy != null)
            {
                Target = targetEnemy.Center;
            }
            return new Dictionary<string, float>
            {
                { "Damage",     .1f },
                { "Healing",    0f },
                { "Ailments",   0f },
                { "Slows",      0f },
                { "Distance",   0f }
            };
        }
    }
}
