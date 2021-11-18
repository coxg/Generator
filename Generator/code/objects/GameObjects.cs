using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Generator.code.objects
{
    public static class GameObjects
    {
        public static Dictionary<string, GameObject> ObjectMap = new Dictionary<string, GameObject>();
        
        static GameObjects()
        {
            FieldInfo[] fields = typeof(GameObjects).GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(GameObject))
                {
                    GameObject gameObject = (GameObject) field.GetValue(null);
                    gameObject.ID = field.Name;
                    ObjectMap[field.Name] = gameObject;
                }
            }
        }
        
        private static bool saidName;
        
        public static GameObject Niels = new GameObject(
            new Vector3(50, 50, 0),
            baseMana: 100,
            baseStrength: 1,
            baseSpeed: 100,
            baseSense: 90,
            baseStyle: 1,
            name: "Niels",
            abilities: new List<Ability>
            {
                Abilities.Thwack,
                Abilities.Blink,
                Abilities.Shoot,
                Abilities.PlaceObject
            });

        public static GameObject Farrah = new GameObject(
            new Vector3(50, 55, 0),
            baseMana: 100,
            baseStrength: 10,
            baseSpeed: 20,
            baseSense: 90,
            baseStyle: 100,
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
                                    "Niels: Yo.",
                                    "Farrah: Yo."
                                }),
                            new Conversation.Choices.Node(
                                text: new List<string>()
                                {
                                    "Niels: Ya!",
                                    "Farrah: Nah!"
                                }),
                            new Conversation.Choices.Node(
                                text: new List<string>()
                                {
                                    "Niels: Hello my friend! This is a very long option - why am I suddenly so talkative? Did you know I was supposed to be the silent protagonist? Funny in hindsight, right???",
                                    "Farrah: Right???? And I have such a long, non-sassy response! I love giving extremely verbose responses in order to test out text alignment!",
                                    "Niels: And I love responding multiple times!",
                                    "Niels: And saying multiple things in a row!",
                                    "Farrah: Me too!"
                                }),
                            new Conversation.Choices.Node(
                                text: new List<string>()
                                {
                                    "Niels: Who are you?",
                                    "Farrah: _sigh..._ Farrah."
                                },
                                rewards: new Rewards(experience: 100),
                                effects: () => { saidName = true; },
                                conditional: () => { return saidName == false; }),
                            new Conversation.Choices.Node(
                                text: new List<string>()
                                {
                                    "Niels: Who are you again?",
                                    "Farrah: I *JUST* told you."
                                },
                                rewards: new Rewards(experience: 1),
                                effects: () => { saidName = false; },
                                conditional: () => { return saidName; }),
                            new Conversation.Choices.Node(
                                text: "Niels: Let me OUT of here!!",
                                exitsConversation: true),
                        })
                }));

        public static GameObject OldMan = new GameObject(
            new Vector3(55, 55, 0),
            baseStrength: 10,
            baseSpeed: 10,
            baseSense: 10,
            activationEffect: new Cached<Action<GameObject, GameObject>>("CreateBigBoy"),
            conversation: new Conversation(
                choicesList: new List<Conversation.Choices>
                {
                    new Conversation.Choices(
                        index: 0,
                        nodes: new List<Conversation.Choices.Node>
                        {
                            new Conversation.Choices.Node(
                                text: new List<string>
                                {
                                    "Niels: Toggle creativeMode.",
                                    "creativeMode is now {wasCreativeMode}."
                                },
                                effects: () => { Globals.CreativeMode = !Globals.CreativeMode; }),
                            new Conversation.Choices.Node(
                                text: new List<string>
                                {
                                    "Niels: I'm done changing settings.",
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
                            new Conversation.Choices.Node("Niels: Nevermind.", exitsConversation: true)
                        }),
                    new Conversation.Choices(
                        index: 2,
                        nodes: new List<Conversation.Choices.Node>
                        {
                            new Conversation.Choices.Node("{manual_0}", effects: () => { Saving.Load("manual", 0); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_1}", effects: () => { Saving.Load("manual", 1); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_2}", effects: () => { Saving.Load("manual", 2); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_3}", effects: () => { Saving.Load("manual", 3); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_4}", effects: () => { Saving.Load("manual", 4); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("Niels: Go back", goToChoicesIndex: 1)
                        }),
                    new Conversation.Choices(
                        index: 3,
                        nodes: new List<Conversation.Choices.Node>
                        {
                            new Conversation.Choices.Node("{auto_0}", effects: () => { Saving.Load("auto", 0); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{auto_1}", effects: () => { Saving.Load("auto", 1); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{auto_2}", effects: () => { Saving.Load("auto", 2); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{auto_3}", effects: () => { Saving.Load("auto", 3); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{auto_4}", effects: () => { Saving.Load("auto", 4); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("Niels: Go back", goToChoicesIndex: 1)
                        }),
                    new Conversation.Choices(
                        index: 4,
                        nodes: new List<Conversation.Choices.Node>
                        {
                            new Conversation.Choices.Node("{quick_0}", effects: () => { Saving.Load("quick", 0); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{quick_1}", effects: () => { Saving.Load("quick", 1); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{quick_2}", effects: () => { Saving.Load("quick", 2); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{quick_3}", effects: () => { Saving.Load("quick", 3); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{quick_4}", effects: () => { Saving.Load("quick", 4); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("Niels: Go back", goToChoicesIndex: 1)
                        }),
                    new Conversation.Choices(
                        index: 5,
                        nodes: new List<Conversation.Choices.Node>
                        {
                            new Conversation.Choices.Node("{manual_0}", effects: () => { Saving.Save("manual", 0); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_1}", effects: () => { Saving.Save("manual", 1); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_2}", effects: () => { Saving.Save("manual", 2); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_3}", effects: () => { Saving.Save("manual", 3); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("{manual_4}", effects: () => { Saving.Save("manual", 4); },
                                exitsConversation: true),
                            new Conversation.Choices.Node("Niels: Nevermind.", exitsConversation: true)
                        })
                }));

        public static GameObject Building = new GameObject(
            new Vector3(57, 59, 0),
            new Vector3(6, 6, 9),
            baseStrength: 10,
            baseSpeed: 10,
            baseSense: 10,
            baseDefense: 100,
            activationEffect: new Cached<Action<GameObject, GameObject>>("SetZoneBuildings"),
            components: new Dictionary<string, Component>
            {
                {"body", new Component(Globals.SpriteSheet.GetCopy("NinjaHead"), new Vector3(6, 6, 9))}
            });

        public static GameObject BadGuy = new GameObject(
            new Vector3(50, 62, 0),
            baseHealth: 100,
            baseMana: 100,
            baseStrength: 1,
            baseSpeed: 10,
            baseSense: 90,
            baseStyle: 100,
            name: "Bad Guy",
            activationEffect: new Cached<Action<GameObject, GameObject>>("EnterCombat"));

        public static GameObject Building2 = new GameObject(
            new Vector3(65, 65, 0),
            new Vector3(6, 6, 9),
            baseStrength: 10,
            baseSpeed: 10,
            baseSense: 10,
            baseDefense: 100,
            activationEffect: new Cached<Action<GameObject, GameObject>>("SetZoneTestingZone"),
            components: new Dictionary<string, Component>
            {
                {"body", new Component(Globals.SpriteSheet.GetCopy("NinjaHead"), new Vector3(6, 6, 9))}
            });
    }
}