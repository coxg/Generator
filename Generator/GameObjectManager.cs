using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

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

        public static void Save(string saveFile)
        {
            using (StreamWriter file = File.CreateText(Globals.SaveDirectory + saveFile + "/gameObjects.json"))
            {
                Globals.Serializer.Serialize(file, ObjectFromID);
            }
        }

        public static void Load(string saveFile)
        {
            using (StreamReader file = File.OpenText(Globals.SaveDirectory + saveFile + "/gameObjects.json"))
            {
                ObjectFromID = (Dictionary<string, GameObject>)Globals.Serializer.Deserialize(
                    file, typeof(Dictionary<string, GameObject>));
            }
            foreach(var gameObject in ObjectFromID.Values)
            {
                // We're ignoring various source objects to avoid circular references, so add it back in when loading
                gameObject.Conversation.SourceObject = gameObject;
                foreach (var choices in gameObject.Conversation.ChoicesList)
                {
                    choices.SourceConversation = gameObject.Conversation;
                    foreach (var node in choices.Nodes)
                    {
                        node.SourceChoices = choices;
                    }
                }
                foreach (var component in gameObject.Components.Values)
                {
                    component.SourceObject = gameObject;
                    foreach (var animation in component.Animations.Values)
                    {
                        animation.SourceObject = gameObject;
                        animation.AnimatedElement = component;
                        animation.StartFrames.SourceAnimation = animation;
                        animation.UpdateFrames.SourceAnimation = animation;
                        animation.StopFrames.SourceAnimation = animation;
                    }
                }
                foreach (var ability in gameObject.Abilities)
                {
                    ability.SourceObject = gameObject;
                    var animation = ability.Animation;
                    animation.SourceObject = gameObject;
                    animation.AnimatedElement = gameObject;
                    animation.StartFrames.SourceAnimation = animation;
                    animation.UpdateFrames.SourceAnimation = animation;
                    animation.StopFrames.SourceAnimation = animation;
                }
            }
        }

        static void WalkToPlayer(GameObject gameObject)
            // TODO: Move these to their own AI file
            // TODO: Why isn't this just Angle(gameObject, Player)? My angle logic seems fundamentally flawed if that's not the calculation
        {
            gameObject.Direction = -(float)MathTools.Angle(Globals.Player.Position, gameObject.Position) + MathHelper.PiOver2;
            gameObject.MoveInDirection(gameObject.Direction);
        }

        static void WalkNearPlayer(GameObject gameObject)
        {
            if (MathTools.Distance(Globals.Player.Position, gameObject.Position) > 5)
            {
                gameObject.Direction = -(float)MathTools.Angle(Globals.Player.Position, gameObject.Position) + MathHelper.PiOver2;
                gameObject.MoveInDirection(gameObject.Direction);
            }
            else
            {
                gameObject.IsWalking = false;
            }
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
                    sprite: Globals.WhiteDot,
                    damage: 10),
                brightness: Vector3.One,
                abilities: new List<string>() {
                    "Sprint", "Shoot", "Place Object", "Attack" });
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
                ai: WalkNearPlayer,
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

            var terrain1 = new GameObject(
                new Vector3(55, 56, 0),
                id: "angry terrain", 
                brightness: new Vector3(.5f, .1f, .5f),
                baseStrength: 10, 
                baseSpeed: 10, 
                baseSense: 10,
                ai: WalkNearPlayer,
                componentSpriteFileName: "Old",
                conversation: new Conversation(
                    choicesList: new List<Conversation.Choices>()
                    {
                        new Conversation.Choices(
                            index: 1,
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
                            })
                    }));
            Globals.Party.Members.Add(terrain1);
            terrain1.ActivationEffect = delegate
            {
                if (!ObjectFromID.ContainsKey("big terrain"))
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
                    var terrain3 = ObjectFromID["big terrain"];
                }
            };

            new GameObject(
                new Vector3(57, 59, 0), 
                new Vector3(6, 9, 9), 
                id: "building", 
                baseStrength: 10, 
                baseSpeed: 10, 
                baseSense: 10,
                castsShadow: false,
                activationEffect: delegate { Globals.Zone = "Buildings"; },
                components: new Dictionary<string, Component>()
                {
                    {"body", new Component(
                        id: "building",
                        relativePosition: new Vector3(.5f, .5f, .5f),
                        relativeSize: .2f,
                        rotationPoint: new Vector3(.5f, .5f, .5f),
                        spriteFile: "Sprites/building")
                    }
                });
        }
    }
}