using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator.code.abilities
{
    public class Shoot : Ability
    {
        public Shoot() : base(
            "Shoot",
            electricityCost: 0,
            cooldown: .02f,  // TODO: Can't fire faster than this
            keepCasting: true) { }

        public override void Start()
        {
            SourceObject.IsShooting = true;
            var position = SourceObject.GetTargetCoordinates(1);
            position.Z += SourceObject.Size.Z / 2;
            var bullet = new GameObject(
                baseHealth: 1,
                position: position,
                size: new Vector3(.05f, .05f, .05f),
                direction: SourceObject.Direction,
                baseSpeed: 1000,
                ai: new Cached<Action<GameObject>>("BulletAI"),
                collisionEffect: new Cached<Action<GameObject, GameObject>>("BulletCollision"),
                brightness: new Vector3(.5f, .1f, .5f),
                temporary: true,
                components: new Dictionary<string, Component>()
                {
                    {"body", new Component(
                        id: "Hand",
                        spriteFile: "Ninja",
                        relativePosition: new Vector3(.5f, .5f, .5f),
                        relativeSize: 1,
                        baseRotationPoint: new Vector3(.5f, .5f, .5f))
                    }
                }
            );
            Globals.GameObjectManager.ObjectList[bullet.ID] = bullet;
        }

        public override Dictionary<string, float> GetPriorityValues(
            IEnumerable<GameObject> allies, IEnumerable<GameObject> enemies, List<GameObject> projectiles)
        {
            var targetEnemy = SourceObject.GetNearest(enemies);
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
