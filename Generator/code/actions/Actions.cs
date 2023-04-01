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
                self =>
                {
                    // Party members follow you around
                    if (Globals.Party.Value.MemberIDs.Contains(self.ID))
                    {
                        if (MathTools.Distance(Globals.Player.Position, self.Position) > 5)
                        {
                            self.MovementTarget = Globals.Player.Position;
                        }
                        else
                        {
                            self.MovementTarget = null;
                        }
                    }

                    // If they're fighting you
                    else if (CombatManager.Enemies.Contains(self))
                    {
                        // TODO
                    }
                }
            },
            {
                "DoNothing",
                self => { }
            },
            {
                "WalkToPlayer",
                self =>
                {
                    self.MovementTarget = Globals.Player.Position;
                }
            },
            {
                "WalkAwayFromPlayer",
                self =>
                {
                    self.MovementDirection = (float)MathTools.Angle(self.Position, Globals.Player.Position) + MathHelper.Pi;
                }
            },
            {
                "WalkInStraightLine",
                self => { self.MovementDirection = self.Direction; }
            }
        };

        // First GameObject is the one using the action, the second GameObject is what they're targeting
        // Used for GameObject.ActivationEffect and GameObject.CollisionEffect
        public static Dictionary<string, Action<GameObject, GameObject>> TargetedActions
            = new Dictionary<string, Action<GameObject, GameObject>>
        {
            {
                "CreateBigBoy",
                (self, other) =>
                {
                    var _ = new GameObject(
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
                (self, other) => { Zone.Enter("buildings"); }
            },
            {
                "SetZoneTestingZone",
                (self, other) => { Zone.Enter("testingZone"); }
            },
            {
                "BulletCollision",
                (self, other) =>
                {
                    self.DealDamage(other, 1, Type.Physical);
                    Globals.GameObjectManager.Kill(self);
                }
            },
            {
                "EnterCombat",
                (self, other) => { CombatManager.StartCombat(new HashSet<GameObject> { self } ); }
            },
        };
    }
}
