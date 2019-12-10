using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Zone
    {
        public GameObjectManager GameObjects;
        public TileManager Tiles;
        public string Name;
        public int Width;
        public int Height;

        public Zone(string name, int width, int height, GameObjectManager gameObjects, TileManager tiles)
        {
            Name = name;
            Width = width;
            Height = height;
            GameObjects = gameObjects;
            Tiles = tiles;
        }

        public void Save()
        {
            Directory.CreateDirectory(Saving.CurrentSaveDirectory + "/Zones/");
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/Zones/" + Name + ".json"))
            {
                Globals.Serializer.Serialize(file, this);
            }
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/Zones/gameObjects.json"))
            {
                Globals.Serializer.Serialize(file, GameObjects);
            }
            using (StreamWriter file = File.CreateText(Saving.CurrentSaveDirectory + "/Zones/tiles.json"))
            {
                Globals.Serializer.Serialize(file, Tiles);
            }
        }

        public static Zone Load(string name)
        {
            var fileName = Saving.CurrentSaveDirectory + "/Zones/" + name + ".json";
            if (File.Exists(fileName))
            {
                using (StreamReader file = File.OpenText(fileName))
                {
                    var returnedZone = (Zone)Globals.Serializer.Deserialize(file, typeof(Zone));

                    // We're ignoring various source objects to avoid circular references, so add it back in when loading
                    foreach (var gameObject in returnedZone.GameObjects.Objects.Values)
                    {
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
                    return returnedZone;
                }
            }
            else
            {
                return Initialize(name);
            }
        }

        public static Zone Initialize(string name)
        {
            switch (name)
            {
                case "testingZone":
                    bool saidName = false;
                    var gameObjects = new GameObjectManager(new List<GameObject>
                    {
                        // niels
                        new GameObject(
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
                                sprite: new Cached<Texture2D>("Sprites/white_dot"),
                                damage: 10),
                            brightness: Vector3.Zero,
                            abilities: new List<Ability>() {
                                Ability.Abilities["Sprint"],
                                Ability.Abilities["Shoot"],
                                //Ability.Abilities["Place Object"],
                                Ability.Abilities["Attack"] }),

                        // farrah
                        new GameObject(
                            new Vector3(50, 55, 0),
                            baseStamina: 100,
                            baseStrength: 10,
                            baseSpeed: 20,
                            baseSense: 90,
                            baseStyle: 100,
                            id: "farrah",
                            name: "Farrah",
                            ai: new Cached<System.Action<GameObject>>("WalkNearPlayer"),
                            componentSpriteFileName: "Girl",
                            weapon: new Weapon(
                                name: "Sword",
                                sprite: new Cached<Texture2D>("Sprites/white_dot"),
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
                                })),

                        // old man
                        new GameObject(
                            new Vector3(55, 56, 0),
                            id: "old man",
                            brightness: Vector3.Zero,
                            baseStrength: 10,
                            baseSpeed: 10,
                            baseSense: 10,
                            ai: new Cached<Action<GameObject>>("WalkNearPlayer"),
                            componentSpriteFileName: "Old",
                            activationEffect: new Cached<Action<GameObject, GameObject>>("CreateBigBoy"),
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
                                                effects: () => { Globals.Party.Value.InCombat = !Globals.Party.Value.InCombat; }),
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
                                })),

                        // building
                        new GameObject(
                            new Vector3(57, 59, 0),
                            new Vector3(6, 9, 9),
                            id: "building",
                            baseStrength: 10,
                            baseSpeed: 10,
                            baseSense: 10,
                            castsShadow: false,
                            brightness: new Vector3(2, 2, 2),
                            activationEffect: new Cached<Action<GameObject, GameObject>>("SetZoneBuildings"),
                            components: new Dictionary<string, Component>()
                            {
                                {"body", new Component(
                                    id: "building",
                                    relativePosition: new Vector3(.5f, .5f, .5f),
                                    relativeSize: .2f,
                                    baseRotationPoint: new Vector3(.5f, .5f, .5f),
                                    spriteFile: "Sprites/building")
                                }
                            })
                    });

                    var tiles = new TileManager(
                        new List<string> { "Grass", "Clay" },
                        new string[100, 100]
                    );

                    return new Zone("testingZone", 100, 100, gameObjects, tiles);

                default:
                    throw new KeyNotFoundException(name);
            }
        }
    }
}
