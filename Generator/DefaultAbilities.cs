using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class DefaultAbilities
    {
        // TODO: Abilities shouldn't be tied to a GameObject on initialization
        // TODO: Once they're separate we can convert all of this to a dictionary

        public static List<Ability> GenerateDefaultAbilities(GameObject gameObject)
        {
            List<Ability> result = new List<Ability>
            {
                CreateDefaultSprintAbility(gameObject),
                CreateDefaultShootAbility(gameObject),
                Globals.CreativeMode ? CreatePlacementAbility(gameObject) : CreateDefaultAlwaysSprintAbility(gameObject),
                CreateDefaultAttackAbility(gameObject),
            };

            return result;
        }

        public static Ability CreateDefaultSprintAbility(GameObject gameObject)
        {
            return new Ability(
                "Sprint",
                staminaCost: 0,
                isChanneled: true,
                requiresWalking: true,
                animation: new Animation(
                    updateFrames: new Frames(
                        offsets: new List<Vector3>
                        {
                            new Vector3(0, 0, .2f)
                        },
                        duration: .5f)),
                start: delegate
                {
                    gameObject.Speed.CurrentValue *= 16;
                    gameObject.IsWalking = true;
                },
                stop: delegate
                {
                    gameObject.Speed.CurrentValue /= 16;
                    gameObject.IsWalking = false;
                });
        }

        public static Ability CreateDefaultAttackAbility(GameObject gameObject)
        {
            return new Ability(
                "Attack",
                staminaCost: gameObject.EquippedWeapon.Weight + 10,
                start: delegate
                {
                    gameObject.IsSwinging = true;

                    // Figure out which one you hit
                    var target = gameObject.GetTargetInRange(gameObject.EquippedWeapon.Range);

                    // Deal damage
                    if (target != null)
                    {
                        Globals.Log(gameObject + " attacks, hitting " + target + ".");
                        gameObject.DealDamage(target, gameObject.EquippedWeapon.Damage + gameObject.Strength.CurrentValue);
                    }
                    else
                    {
                        Globals.Log(gameObject + " attacks and misses.");
                    }
                }
            );
        }

        public static Ability CreateDefaultShootAbility(GameObject gameObject)
        {
            void BulletAI(GameObject bullet)
            {
                bullet.MoveInDirection(bullet.Direction);
            }

            void BulletCollision(GameObject bullet, GameObject other)
            {
                bullet.DealDamage(other, (int)System.Math.Sqrt(bullet.Speed.CurrentValue));
                bullet.Die();
            }

            return new Ability(
                "Shoot",
                staminaCost: gameObject.EquippedWeapon.Weight,
                cooldown: .1f,
                keepCasting: true,
                start: delegate
                {
                    gameObject.IsShooting = true;
                    var name = System.Guid.NewGuid().ToString();
                    var position = gameObject.GetTargetCoordinates(1);
                    position.Z += gameObject.Size.Z / 2;
                    Globals.GameObjects.AddNewObject(
                        name,
                        new GameObject(
                            name: name,
                            health: 1,
                            position: position,
                            size: new Vector3(.05f, .05f, .05f),
                            direction: gameObject.Direction,
                            speed: 100,
                            ai: BulletAI,
                            collisionEffect: BulletCollision,
                            brightness: new Vector3(.5f, .1f, .5f),
                            castsShadow: false,
                            temporary: true,
                            components: new Dictionary<string, Component>()
                            {
                                {"body", new Component(
                                    spriteFile: "Ninja/Hand",
                                    relativePosition: new Vector3(.5f, .5f, .5f),
                                    relativeSize: 1,
                                    rotationPoint: new Vector3(.5f, .5f, .5f))
                                }
                            }
                        )
                    );
                }
            );
        }

        public static Ability CreatePlacementAbility(GameObject gameObject)
        {
            return new Ability(
                "Place Object",
                start: delegate
                {
                    var baseTileName = Globals.Tiles.NameFromIndex[Globals.Tiles.BaseTileIndexes[Globals.CreativeObjectIndex]];
                    var randomBaseTile = Globals.Tiles.GetRandomBaseIndex(Globals.Tiles.ObjectFromName[baseTileName].BaseTileName);
                    var targetCoordinates = gameObject.GetTargetCoordinates(1);
                    Globals.Tiles.Set((int)targetCoordinates.X, (int)targetCoordinates.Y, Globals.Tiles.NameFromIndex[randomBaseTile]);
                },
                animation: new Animation(
                    startFrames: new Frames(
                        rotations: new List<Vector3>
                        {
                            new Vector3(0, 0, 1)
                        },
                        duration: .5f))
            );
        }

        public static Ability CreateDefaultAlwaysSprintAbility(GameObject gameObject)
        {
            return new Ability(
                "Always Sprint",
                staminaCost: 1,
                isToggleable: true,
                requiresWalking: true,
                start: delegate
                {
                    gameObject.Speed.CurrentValue *= 4;
                    gameObject.IsWalking = true;
                },
                stop: delegate
                {
                    gameObject.Speed.CurrentValue /= 4;
                    gameObject.IsWalking = false;
                },
                animation: new Animation(
                    startFrames: new Frames(
                        offsets: new List<Vector3>
                        {
                            new Vector3(0, 0, 1)
                        },
                        duration: 1),
                    updateFrames: new Frames(
                        offsets: new List<Vector3>
                        {
                            new Vector3(-.2f, 0, 0),
                            new Vector3(.2f, 0, 0)
                        },
                        duration: .5f),
                    stopFrames: new Frames(
                        offsets: new List<Vector3>
                        {
                            new Vector3(0, 0, 1)
                        },
                        duration: 1)));
        }
    }
}
