using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class GameObjectManager : Manager<GameObject>
    {
        public static Dictionary<string, GameObject> Updating = new Dictionary<string, GameObject>();
        public static Dictionary<string, GameObject> Visible = new Dictionary<string, GameObject>();

        new public static void Update()
            // TODO: This should use the same logic as the Manager
        {
            List<GameObject> objectsToRemove = new List<GameObject>();
            foreach (var gameObject in ObjectFromID.Values)
            {
                // See if the object should be visible
                var IsVisible = Visible.ContainsKey(gameObject.ID);
                var ShouldBeVisible = gameObject.IsVisible();
                if (!IsVisible && ShouldBeVisible)
                {
                    Visible[gameObject.ID] = gameObject;
                }
                else if (IsVisible && !ShouldBeVisible)
                {
                    Visible.Remove(gameObject.ID);
                }

                // See if the object should be updating
                var IsUpdating = Updating.ContainsKey(gameObject.ID);
                var ShouldBeUpdating = gameObject.IsUpdating();
                if (!IsUpdating && ShouldBeUpdating)
                {
                    Updating[gameObject.ID] = gameObject;
                }
                else if (IsUpdating && !ShouldBeUpdating)
                {
                    Updating.Remove(gameObject.ID);
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

        public static void Clear()
            // Clear all objects from memory, resetting it
        {
            ObjectFromID = new Dictionary<string, GameObject>();
            IDFromIndex = new Dictionary<int, string>();
            IndexFromID = new Dictionary<string, int>();
            Updating = new Dictionary<string, GameObject>();
            Visible = new Dictionary<string, GameObject>();
        }

        public static void Save(string saveFile)
        {
            using (StreamWriter file = File.CreateText(saveFile + "/gameObjects.json"))
            {
                Globals.Serializer.Serialize(file, ObjectFromID);
            }
        }

        public static void Load(string saveFile)
        {
            using (StreamReader file = File.OpenText(saveFile + "/gameObjects.json"))
            {
                // GameObjects add themselves to the manager, so just clear it and let them do their thing
                Clear();
                Globals.Serializer.Deserialize(file, typeof(Dictionary<string, GameObject>));
            }
            foreach(var gameObject in ObjectFromID.Values)
            {
                // We're ignoring various source objects to avoid circular references, so add it back in when loading
                if (gameObject.Conversation != null)
                {
                    gameObject.Conversation.SourceObject = gameObject;
                    foreach (var choices in gameObject.Conversation.ChoicesList)
                    {
                        choices.SourceConversation = gameObject.Conversation;
                        foreach (var node in choices.Nodes)
                        {
                            node.SourceChoices = choices;
                        }
                    }
                }
                foreach (var component in gameObject.Components.Values)
                {
                    component.SourceObject = gameObject;
                    foreach (var animation in component.Animations.Values)
                    {
                        animation.AnimatedElement = component;
                        animation.SetSource(gameObject);
                    }
                }
                foreach (var ability in gameObject.Abilities)
                {
                    ability.SourceObject = gameObject;
                    var animation = ability.Animation;
                    if (animation != null)
                    {
                        animation.AnimatedElement = gameObject;
                        animation.SetSource(gameObject);
                    }
                }
            }
            Globals.Party.Members.Add(ObjectFromID["niels"]);
            Globals.Player = ObjectFromID["niels"];
            GameControl.lightingRenderTargets = new Dictionary<GameObject, RenderTarget2D>();
        }

        public static void Initialize()
        {
            // Set the name
            Name = "GameObjects";

            var niels = new GameObject(
                new Vector3(50, 50, 0),
                baseStamina: 100,
                baseStrength: 10,
                baseSpeed: 100,
                baseSense: 90,
                baseStyle: 100,
                id: "niels",
                name: "Niels",
                componentSpriteFileName: "Ninja",
                weapon: new Weapon(
                    name: "Sword",
                    sprite: new Loaded<Texture2D>("Sprites/white_dot"),
                    damage: 10),
                brightness: Vector3.Zero,
                abilities: new List<Ability>() {
                    Ability.Abilities["Sprint"],
                    Ability.Abilities["Shoot"],
                    Ability.Abilities["Place Object"],
                    Ability.Abilities["Attack"] });
            Globals.Party.Members.Add(niels);


            bool saidName = false;
            var farrah = new GameObject(
                new Vector3(50, 55, 0),
                baseStamina: 100,
                baseStrength: 10,
                baseSpeed: 20,
                baseSense: 90,
                baseStyle: 100,
                id: "farrah",
                name: "Farrah",
                ai: new Loaded<System.Action<GameObject>>("WalkNearPlayer"),
                componentSpriteFileName: "Girl",
                weapon: new Weapon(
                    name: "Sword",
                    sprite: new Loaded<Texture2D>("Sprites/white_dot"),
                    damage: 10),
                brightness: Vector3.Zero,
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
                                        "farrah: Right???? And I have such a long, non-sassy response! I love giving extremely verbose responses in order to test out text alignment!",
                                        "niels: And I love responding multiple times!",
                                        "niels: And saying multiple things in a row!",
                                        "farrah: Me too!"
                                    }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Who are you?",
                                        "farrah: _sigh..._ Farrah."
                                    },
                                    rewards: new Rewards(experience: 100),
                                    effects: () => { saidName = true; },
                                    conditional: () => { return saidName == false; }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Who are you again?",
                                        "farrah: I *JUST* told you."
                                    },
                                    rewards: new Rewards(experience: 1),
                                    effects: () => { saidName = false; },
                                    conditional: () => { return saidName == true; }),
                                new Conversation.Choices.Node(
                                    text: "niels: Let me OUT of here!!",
                                    exitsConversation: true),
                            })
                    }));
            Globals.Party.Members.Add(farrah);

            var oldMan = new GameObject(
                new Vector3(55, 56, 0),
                id: "old man", 
                brightness: Vector3.Zero,
                baseStrength: 10, 
                baseSpeed: 10, 
                baseSense: 10,
                ai: new Loaded<System.Action<GameObject>>("WalkNearPlayer"),
                componentSpriteFileName: "Old",
                activationEffect: new Loaded<System.Action<GameObject, GameObject>>("CreateBigBoy"),
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
                                        "niels: Toggle inCombat.",
                                        "inCombat is now {wasInCombat}."
                                    },
                                    effects: () => { Globals.Party.InCombat = !Globals.Party.InCombat; }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Toggle creativeMode.",
                                        "creativeMode is now {wasCreativeMode}."
                                    },
                                    effects: () => { Globals.CreativeMode = !Globals.CreativeMode; }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: Load save1.",
                                        "Okay."
                                    },
                                    effects: () => { Load("save1"); }),
                                new Conversation.Choices.Node(
                                    text: new List<string>()
                                    {
                                        "niels: I'm done changing settings.",
                                        "Roger."
                                    },
                                    exitsConversation: true),
                            }),

                        // This is triggered when you press load
                        new Conversation.Choices(
                            index: 1,
                            nodes: new List<Conversation.Choices.Node>
                            {
                                new Conversation.Choices.Node("Manual Saves", goToChoicesIndex: 2),
                                new Conversation.Choices.Node("Autosaves", goToChoicesIndex: 3),
                                new Conversation.Choices.Node("Quicksaves", goToChoicesIndex: 4),
                                new Conversation.Choices.Node("niels: Nevermind.", exitsConversation: true)
                            }),
                        new Conversation.Choices(
                            index: 2,
                            nodes: new List<Conversation.Choices.Node>
                            {
                                new Conversation.Choices.Node("{manual_0}", effects: () => { Saving.Load("manual", 0); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_1}", effects: () => { Saving.Load("manual", 1); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_2}", effects: () => { Saving.Load("manual", 2); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_3}", effects: () => { Saving.Load("manual", 3); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_4}", effects: () => { Saving.Load("manual", 4); }, exitsConversation: true),
                                new Conversation.Choices.Node("niels: Go back", goToChoicesIndex: 1)
                            }),
                        new Conversation.Choices(
                            index: 3,
                            nodes: new List<Conversation.Choices.Node>
                            {
                                new Conversation.Choices.Node("{auto_0}", effects: () => { Saving.Load("auto", 0); }, exitsConversation: true),
                                new Conversation.Choices.Node("{auto_1}", effects: () => { Saving.Load("auto", 1); }, exitsConversation: true),
                                new Conversation.Choices.Node("{auto_2}", effects: () => { Saving.Load("auto", 2); }, exitsConversation: true),
                                new Conversation.Choices.Node("{auto_3}", effects: () => { Saving.Load("auto", 3); }, exitsConversation: true),
                                new Conversation.Choices.Node("{auto_4}", effects: () => { Saving.Load("auto", 4); }, exitsConversation: true),
                                new Conversation.Choices.Node("niels: Go back", goToChoicesIndex: 1)
                            }),
                        new Conversation.Choices(
                            index: 4,
                            nodes: new List<Conversation.Choices.Node>
                            {
                                new Conversation.Choices.Node("{quick_0}", effects: () => { Saving.Load("quick", 0); }, exitsConversation: true),
                                new Conversation.Choices.Node("{quick_1}", effects: () => { Saving.Load("quick", 1); }, exitsConversation: true),
                                new Conversation.Choices.Node("{quick_2}", effects: () => { Saving.Load("quick", 2); }, exitsConversation: true),
                                new Conversation.Choices.Node("{quick_3}", effects: () => { Saving.Load("quick", 3); }, exitsConversation: true),
                                new Conversation.Choices.Node("{quick_4}", effects: () => { Saving.Load("quick", 4); }, exitsConversation: true),
                                new Conversation.Choices.Node("niels: Go back", goToChoicesIndex: 1)
                            }),
                        new Conversation.Choices(
                            index: 5,
                            nodes: new List<Conversation.Choices.Node>
                            {
                                new Conversation.Choices.Node("{manual_0}", effects: () => { Saving.Save("manual", 0); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_1}", effects: () => { Saving.Save("manual", 1); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_2}", effects: () => { Saving.Save("manual", 2); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_3}", effects: () => { Saving.Save("manual", 3); }, exitsConversation: true),
                                new Conversation.Choices.Node("{manual_4}", effects: () => { Saving.Save("manual", 4); }, exitsConversation: true),
                                new Conversation.Choices.Node("niels: Nevermind.", exitsConversation: true)
                            })
                    }));
            Globals.Party.Members.Add(oldMan);

            new GameObject(
                new Vector3(57, 59, 0), 
                new Vector3(6, 9, 9), 
                id: "building", 
                baseStrength: 10, 
                baseSpeed: 10, 
                baseSense: 10,
                castsShadow: false,
                brightness: new Vector3(2, 2, 2),
                activationEffect: new Loaded<System.Action<GameObject, GameObject>>("SetZoneBuildings"),
                components: new Dictionary<string, Component>()
                {
                    {"body", new Component(
                        id: "building",
                        relativePosition: new Vector3(.5f, .5f, .5f),
                        relativeSize: .2f,
                        baseRotationPoint: new Vector3(.5f, .5f, .5f),
                        spriteFile: "Sprites/building")
                    }
                });
        }
    }
}