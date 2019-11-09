using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class GameObjectManager : Manager<GameObject>
    {
        public static Dictionary<string, GameObject> Updating = new Dictionary<string, GameObject>();
        public static Dictionary<string, GameObject> Visible = new Dictionary<string, GameObject>();

        public delegate void Delegate(GameObject gameObject);

        new public static void Update()
            // TODO: This should use the same logic as the Manager
        {
            List<GameObject> objectsToRemove = new List<GameObject>();
            foreach (var gameObject in ObjectFromName.Values)
            {
                // See if the object should be visible
                var IsVisible = Visible.ContainsKey(gameObject.Name);
                var ShouldBeVisible = gameObject.IsVisible();
                if (!IsVisible && ShouldBeVisible)
                {
                    Visible[gameObject.Name] = gameObject;
                }
                else if (IsVisible && !ShouldBeVisible)
                {
                    Visible.Remove(gameObject.Name);
                }

                // See if the object should be updating
                var IsUpdating = Updating.ContainsKey(gameObject.Name);
                var ShouldBeUpdating = gameObject.IsUpdating();
                if (!IsUpdating && ShouldBeUpdating)
                {
                    Updating[gameObject.Name] = gameObject;
                }
                else if (IsUpdating && !ShouldBeUpdating)
                {
                    Updating.Remove(gameObject.Name);
                }

                // If it's not updating then it should be deleted
                if (!ShouldBeUpdating && gameObject.Temporary)
                {
                    objectsToRemove.Add(gameObject);
                }
            }

            // Delete outside of loop as not to modify the collection
            foreach (var gameObject in objectsToRemove)
            {
                gameObject.Remove();
            }
        }

        static void WalkToPlayer(GameObject gameObject)
            // TODO: Move these to their own AI file
            // TODO: Why isn't this just Angle(gameObject, Player)? My angle logic seems fundamentally flawed if that's not the calculation
        {
            gameObject.Direction = -(float)MathTools.Angle(Globals.Player.Position, gameObject.Position) + MathHelper.PiOver2;
            gameObject.MoveInDirection(gameObject.Direction);
        }

        static void WalkAwayFromPlayer(GameObject gameObject)
        {
            gameObject.Direction = -(float)MathTools.Angle(gameObject.Position, Globals.Player.Position) + MathHelper.PiOver2;
            gameObject.MoveInDirection(gameObject.Direction);
        }

        public static void Initialize()
        {
            // Set the name
            Name = "GameObjects";

            var niels = new GameObject(
                new Vector3(50, 50, 0),
                stamina: 100,
                strength: 10,
                speed: 100,
                sense: 90,
                style: 100,
                name: "niels",
                componentSpriteFileName: "Ninja",
                weapon: new Weapon(
                    name: "Sword",
                    sprite: Globals.WhiteDot,
                    damage: 10),
                brightness: Vector3.One);
            Globals.Party.Members.Add(niels);

            var notSaidName = new Requirements(
                new Dictionary<string, int>()
                {
                    { "notSaidName", 1 }
                });
            var saidName = new Requirements(
                new Dictionary<string, int>()
                {
                    { "saidName", 1 }
                });
            var farrah = new GameObject(
                new Vector3(50, 55, 0),
                stamina: 100,
                strength: 10,
                speed: 100,
                sense: 90,
                style: 100,
                name: "farrah",
                componentSpriteFileName: "Girl",
                weapon: new Weapon(
                    name: "Sword",
                    sprite: Globals.WhiteDot,
                    damage: 10),
                brightness: Vector3.One,
                conversation: new Conversation(
                    choicesList: new List<Conversation.Choices>()
                    {
                        new Conversation.Choices(
                            index: 0,
                            nodes: new List<Conversation.Choices.Node>()
                            {
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Yo.",
                                        "farrah: Yo."
                                    }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Ya!",
                                        "farrah: Nah!"
                                    }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Hello my friend! This is a very long option - why am I suddenly so talkative? Did you know I was supposed to be the silent protagonist? Funny in hindsight, right???",
                                        "farrah: k."
                                    }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Who are you?.",
                                        "farrah: _sigh..._ Farrah."
                                    },
                                    rewards: new Rewards(experience: 100),
                                    effects: () => { notSaidName.Progress["notSaidName"] = 1 - notSaidName.Progress["notSaidName"]; },
                                    requirements: notSaidName),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Who are you again?",
                                        "farrah: I *JUST* told you."
                                    },
                                    rewards: new Rewards(experience: 1),
                                    effects: () => { saidName.Progress["saidName"] = 1 - saidName.Progress["saidName"]; },
                                    requirements: saidName),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Let me tinker with the settings.",
                                        "farrah: Alright."
                                    },
                                    goToChoicesIndex: 1),
                                new Conversation.Choices.Node(
                                    text: "niels: Let me OUT of here!!",
                                    exitsConversation: true),
                            }),
                        new Conversation.Choices(
                            index: 1,
                            nodes: new List<Conversation.Choices.Node>()
                            {
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Don't fight me.",
                                        "farrah: Okay."
                                    },
                                    effects: () => { Globals.Party.InCombat = false; }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Fight me.",
                                        "farrah: Okay."
                                    },
                                    effects: () => { Globals.Party.InCombat = true; }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Are we fighting?",
                                        "farrah: " + Globals.Party.InCombat.ToString()
                                    },
                                    effects: () => { Globals.Party.InCombat = true; }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: I want to ask your name again.",
                                        "farrah: ...great."
                                    },
                                    goToChoicesIndex: 0),
                            })
                    }));
            Globals.Party.Members.Add(farrah);

            var terrain1 = new GameObject(
                new Vector3(55, 56, 0),
                spriteFile: "Sprites/angry_boy", 
                name: "angry terrain", 
                brightness: new Vector3(.5f, .1f, .5f),
                strength: 10, 
                speed: 10, 
                sense: 10,
                ai: WalkToPlayer,
                componentSpriteFileName: "Old",
                conversation: new Conversation(
                    new List<string> {
                        "Check it out, I do something weird!",
                        "Did you see how weird that was?!"
                    } )
                );
            terrain1.ActivationEffect = delegate
            {
                if (!ObjectFromName.ContainsKey("big terrain"))
                {
                    terrain1.Conversation = new Conversation("There's already a boy!");

                    new GameObject(
                        new Vector3(60, 60, 0), 
                        new Vector3(5, 1, 5), 
                        name: "big terrain", 
                        strength: 10, 
                        speed: 10, 
                        sense: 10,
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
                    var terrain3 = ObjectFromName["big terrain"];
                }
            };

            new GameObject(
                new Vector3(57, 59, 0), 
                new Vector3(2, 1, 2), 
                name: "medium terrain", 
                strength: 10, 
                speed: 10, 
                sense: 10);

            // Populate the Acres
            PopulateAcres();
        }
    }
}