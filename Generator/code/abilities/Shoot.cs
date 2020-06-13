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
            cooldown: .1f,  // TODO: Can't fire faster than this or the bullets hit each other
            keepCasting: true) { }

        public override void Start()
        {
            SourceObject.IsShooting = true;
            var position = SourceObject.GetTargetCoordinates(3); // TODO: Can't shoot closer than this
            position.Z += SourceObject.Size.Z / 2;
            var bullet = new GameObject(
                baseHealth: 1,
                position: position,
                size: new Vector3(.5f, .5f, .5f),
                direction: SourceObject.Direction,
                baseSpeed: 1000,
                ai: new Cached<Action<GameObject>>("BulletAI"),
                collisionEffect: new Cached<Action<GameObject, GameObject>>("BulletCollision"),
                temporary: true,
                components: new Dictionary<string, Component>
                {
                    {"body", new Component(Globals.SpriteSheet.GetCopy("MetalBall"), new Vector3(.5f, .5f, .5f))}
                },
                lightComponents: new Dictionary<string, LightComponent>
                {
                    {"light", new LightComponent(Vector3.One, Color.Red)}
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
