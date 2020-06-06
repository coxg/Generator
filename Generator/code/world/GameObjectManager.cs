using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Generator
{
    public class GameObjectManager
    {
        public Dictionary<string, GameObject> Objects = new Dictionary<string, GameObject>();
        [JsonIgnore]
        public code.world.CollisionMap CollisionMap;
        public HashSet<string> Enemies = new HashSet<string>();

        public List<GameObject> EnemyObjects()
        {
            var enemyObjects = new List<GameObject>();
            foreach (string enemy in Enemies)
            {
                enemyObjects.Add(Objects[enemy]);
            }
            return enemyObjects;
        }
        
        public GameObjectManager(List<GameObject> objects)
        {
            foreach (var gameObject in objects)
            {
                Objects[gameObject.ID] = gameObject;
            }
            CollisionMap = new code.world.CollisionMap(Globals.Zone.Width, Globals.Zone.Height, Objects.Values);
        }

        [JsonConstructor]
        public GameObjectManager(Dictionary<string, GameObject> objects, HashSet<string> enemies)
        {
            Objects = objects;
            CollisionMap = new code.world.CollisionMap(Globals.Zone.Width, Globals.Zone.Height, Objects.Values);
            Enemies = enemies;
        }
        
        public void Remove(GameObject gameObject)
        {
            Objects.Remove(gameObject.ID);
            Enemies.Remove(gameObject.ID);
            if (gameObject.Collision) CollisionMap.Remove(gameObject);
        }

        public void Kill(GameObject gameObject)
            // TODO: Don't remove - set to dead, play death animation, and leave on the ground
        {
            Globals.Party.Value.MemberIDs.Remove(gameObject.ID);
            Remove(gameObject);
            Globals.Log(gameObject + " has passed away. RIP.");
        }

        public static void Load(string fileName)
        {
            using (StreamReader file = File.OpenText(fileName))
            {
                Globals.GameObjectManager = (GameObjectManager)Globals.Serializer.Deserialize(file, typeof(GameObjectManager));

                // We're ignoring various source objects to avoid circular references, so add it back in when loading
                foreach (var gameObject in Globals.GameObjectManager.Objects.Values)
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
                    var abilities = new List<Ability>();
                    foreach (var ability in gameObject.Abilities)
                    {
                        var newAbility = Ability.GetTyped(ability);
                        newAbility.SourceObject = gameObject;
                        var animation = newAbility.Animation;
                        if (animation != null)
                        {
                            animation.AnimatedElement = gameObject;
                            animation.SetSource(gameObject);
                        }
                        abilities.Add(newAbility);
                    }
                    gameObject.Abilities = abilities;
                }
            }
        } 
    }
}