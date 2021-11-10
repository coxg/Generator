using System;
using System.Collections.Generic;
using System.IO;
using Generator.code.objects;
using Microsoft.Xna.Framework;

namespace Generator
{
    public class Zone
    {
        public string Name;
        public int Width;
        public int Height;
        public float Gravity;
        public Vector3 Wind;

        public Zone(string name, int width, int height, float gravity = 9.8f, Vector3? wind = null)
        {
            Name = name;
            Width = width;
            Height = height;
            Gravity = gravity;
            Wind = wind ?? Vector3.Zero;
        }

        public static void Enter(string name)
        // Move to a new zone, taking the party objects with you
        {
            // Remove party from zone before serializing
            var partyMembers = new List<GameObject>();
            foreach (var memberId in (List<String>) Globals.Copy(Globals.Party.Value.MemberIDs))
            {
                var partyMemeber = Globals.GameObjectManager.Get(memberId);
                partyMembers.Add(partyMemeber);
                Globals.GameObjectManager.Remove(partyMemeber);
            }
            
            // Serialize
            Saving.CurrentSaveDirectory = Saving.BaseSaveDirectory + "tmp";
            Globals.Log("Saving to " + Saving.CurrentSaveDirectory);
            Saving.SaveAreaToDisk();

            // Set the new zone
            Saving.LoadAreaFromDisk(name);

            // Add the party to the new zone
            foreach (var partyMember in partyMembers)
            {
                Globals.GameObjectManager.Set(partyMember);
            }
        }

        public static void Initialize(string name)
        // TODO: Something better than this!
        {
            switch (name)
            {
                case "testingZone":
                    Globals.Zone = new Zone("testingZone", 500, 500);
                    bool saidName = false;
                    Globals.GameObjectManager = new GameObjectManager(new List<GameObject>
                    {
                        // niels
                        new GameObject(
                            new Vector3(50, 50, 0),
                            baseMana: 100,
                            baseStrength: 1,
                            baseSpeed: 100,
                            baseSense: 90,
                            baseStyle: 1,
                            id: "niels",
                            name: "Niels",
                            abilities: new List<Ability> {
                                Abilities.Move,
                                Abilities.Blink,
                                Abilities.Shoot,
                                Abilities.Thwack,
                                Abilities.PlaceObject
                            }),

                        // farrah
                        new GameObject(
                            new Vector3(50, 55, 0),
                            baseMana: 100,
                            baseStrength: 10,
                            baseSpeed: 20,
                            baseSense: 90,
                            baseStyle: 100,
                            id: "farrah",
                            name: "Farrah",
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
                                                conditional: () => { return saidName; }),
                                            new Conversation.Choices.Node(
                                                text: "niels: Let me OUT of here!!",
                                                exitsConversation: true),
                                        })
                                })),

                        // old man
                        new GameObject(
                            new Vector3(55, 55, 0),
                            id: "old man",
                            baseStrength: 10,
                            baseSpeed: 10,
                            baseSense: 10,
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
                            new Vector3(6, 6, 9),
                            id: "building",
                            baseStrength: 10,
                            baseSpeed: 10,
                            baseSense: 10,
                            baseDefense: 100,
                            activationEffect: new Cached<Action<GameObject, GameObject>>("SetZoneBuildings"),
                            components: new Dictionary<string, Component>
                            {
                                { "body", new Component(Globals.SpriteSheet.GetCopy("NinjaHead"), new Vector3(6, 6, 9)) }
                            }),

                        new GameObject(
                            new Vector3(50, 62, 0),
                            baseHealth: 100,
                            baseMana: 100,
                            baseStrength: 1,
                            baseSpeed: 10,
                            baseSense: 90,
                            baseStyle: 100,
                            id: "bad_guy",
                            name: "Bad Guy")
                    });
                    Globals.TileManager = new TileManager(Globals.DefaultTileSheet, 1);
                    break;

                case "buildings":
                    Globals.Zone = new Zone("buildings", 100, 100);
                    Globals.GameObjectManager = new GameObjectManager(new List<GameObject>
                    {
                        // building
                        new GameObject(
                            new Vector3(65, 65, 0),
                            new Vector3(6, 6, 9),
                            id: "testBuilding",
                            baseStrength: 10,
                            baseSpeed: 10,
                            baseSense: 10,
                            baseDefense: 100,
                            activationEffect: new Cached<Action<GameObject, GameObject>>("SetZoneTestingZone"),
                            components: new Dictionary<string, Component>
                            {
                                { "body", new Component(Globals.SpriteSheet.GetCopy("NinjaHead"), new Vector3(6, 6, 9)) }
                            })
                    });
                    Globals.TileManager = new TileManager(Globals.DefaultTileSheet);
                    break;

                default:
                    throw new KeyNotFoundException(name);
            }
        }
    }
}
