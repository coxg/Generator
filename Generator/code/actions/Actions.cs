using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public static class Actions
    {
        // The GameObject is the one calling it
        // Used for GameObject.AI, which is called on each update
        public static Dictionary<string, Action<GameObject>> SelfActions = new Dictionary<string, Action<GameObject>>
        {
            {
                "DefaultAI",
                (GameObject self) =>
                {
                    // The player is controlled directly
                    if (Globals.Player.ID == self.ID)
                    {
                        return;
                    }

                    // Party members
                    else if (Globals.Party.Value.MemberIDs.Contains(self.ID))
                    {
                        // During combat they fight your enemies
                        if (Globals.Party.Value.InCombat)
                        {

                        }

                        // Out of combat they follow you around
                        else
                        {
                            if (MathTools.Distance(Globals.Player.Position, self.Position) > 5)
                            {
                                self.MoveInDirection((float)MathTools.Angle(self.Position, Globals.Player.Position));
                            }
                            else
                            {
                                // TODO: Why do I need to set this manually?
                                self.IsWalking = false;
                            }
                        }
                    }

                    // NPCs
                    else
                    {
                        // If they're fighting you
                        if (Globals.Zone.Enemies.Contains(self.ID))
                        {

                        }

                        // If they're not fighting you
                        else
                        {
                            return;
                        }
                    }
                }
            },
            {
                "DoNothing",
                (GameObject self) => { }
            },
            {
                "WalkToPlayer",
                (GameObject self) =>
                {
                    self.MoveInDirection((float)MathTools.Angle(self.Position, Globals.Player.Position));
                }
            },
            {
                "WalkAwayFromPlayer",
                (GameObject self) =>
                {
                    self.MoveInDirection((float)MathTools.Angle(self.Position, Globals.Player.Position) + MathHelper.Pi);
                }
            },
            {
                "WalkInStraightLine",
                (GameObject self) => { self.MoveInDirection(self.Direction); }
            }
        };

        // First GameObject is the one using the action, the second GameObject is what they're targeting
        // Used for GameObject.ActivationEffect and GameObject.CollisionEffect
        public static Dictionary<string, Action<GameObject, GameObject>> TargetedActions
            = new Dictionary<string, Action<GameObject, GameObject>>
        {
            {
                "CreateBigBoy",
                (GameObject self, GameObject other) =>
                {
                    new GameObject(
                        new Vector3(60, 60, 0),
                        new Vector3(5, 1, 5),
                        id: "big terrain",
                        baseStrength: 10,
                        baseSpeed: 10,
                        baseSense: 10,
                        conversation: new Conversation(
                            new List<Conversation.Choices>()
                            {
                                new Conversation.Choices(
                                    new Conversation.Choices.Node(
                                        new List<string> {
                                            "I don't do anything weird.",
                                            "...I'm just really fat."
                                        },
                                        goToChoicesIndex: 1,
                                        exitsConversation: true),
                                    index: 0),
                                new Conversation.Choices(
                                    new Conversation.Choices.Node(
                                        new List<string> {
                                            "Well, other than saying something different the second time you talk to me.",
                                            "That's pretty cool I guess.",
                                            "If you're into that kind of thing."
                                        },
                                        goToChoicesIndex: 0,
                                        exitsConversation: true),
                                    index: 1)
                            }));
                }
            },
            {
                "SetZoneBuildings",
                (GameObject self, GameObject other) => {
                    Globals.Zone = Zone.Load("buildings");
                }
            },
            {
                "SetZoneTestingZone",
                (GameObject self, GameObject other) => {
                    Globals.Zone = Zone.Load("testingZone");
                }
            },
            {
                "BulletCollision",
                (GameObject self, GameObject other) =>
                {
                    self.DealDamage(other, (int)Math.Sqrt(self.Speed.CurrentValue));
                    self.Die();
                }
            }
        };
    }
}
