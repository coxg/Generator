using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class Defaults
    {
        public Defaults()
        {
        }

        public static List<Ability> generateDefaultAbilities(GameObject gameObject)
        {
            List<Ability> result = new List<Ability>
                { createDefaultSprintAbility(gameObject),
                  createDefaultAttackAbility(gameObject),
                  createDefaultShootAbility(gameObject),
                  createDefaultAlwaysSprintAbility(gameObject)
                };

            return result;
        }

        public static Ability createDefaultSprintAbility(GameObject gameObject)
        {
            return new Ability(
                        "Sprint",
                        staminaCost: 1,
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
                            gameObject.Speed.CurrentValue *= 4;
                            gameObject.IsWalking = true;
                        },
                        stop: delegate
                        {
                            gameObject.Speed.CurrentValue /= 4;
                            gameObject.IsWalking = false;
                        });
        }

        public static Ability createDefaultAttackAbility(GameObject gameObject)
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

        public static Ability createDefaultShootAbility(GameObject gameObject)
        {
            return new Ability(
                        "Shoot",
                        staminaCost: gameObject.EquippedWeapon.Weight + 10,
                        start: delegate
                        {
                            gameObject.IsShooting = true;

                            // Figure out which one you hit
                            var target = gameObject.GetTargetInRange(gameObject.EquippedWeapon.Range + 20);

                            // Deal damage
                            if (target != null)
                            {
                                Globals.Log(gameObject + " shoots, hitting " + target + ".");
                                gameObject.DealDamage(target, gameObject.EquippedWeapon.Damage + gameObject.Strength.CurrentValue);
                            }
                            else
                            {
                                Globals.Log(gameObject + " shoots and misses.");
                            }
                        }
                    );
        }

        public static Ability createDefaultAlwaysSprintAbility(GameObject gameObject)
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
