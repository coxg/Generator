using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Generator.code.world;

namespace Generator
{
    public class GameObjectManager
    {
        public Dictionary<string, GameObject> ObjectDict = new Dictionary<string, GameObject>();
        [JsonIgnore]
        private ObjectMap ObjectMap;
        public HashSet<string> EnemyIds = new HashSet<string>();

        public GameObject Get(string objectId)
        {
            GameObject gameObject;
            ObjectDict.TryGetValue(objectId, out gameObject);
            return gameObject;
        }

        public void Set(GameObject gameObject)
        {
            ObjectDict[gameObject.ID] = gameObject;
        }

        public IEnumerable<GameObject> Get(float x, float y)
        {
            return ObjectMap.Get(x, y);
        }
        
        public HashSet<GameObject> Get(RectangleF area)
        {
            // Looping over all tiles in a large area is expensive, so in this case just loop over all objects and see
            // if they're in the area
            if (area.Width * area.Height > ObjectDict.Count)
            {
                return ObjectDict.Values.Where(x => x.Area.IntersectsWith(area)).ToHashSet();
            }
            return ObjectMap.Get(area);
        }
        
        public HashSet<GameObject> GetVisible()
        {
            return ObjectDict.Values.Where(gameObject => gameObject.IsVisible()).ToHashSet();
        }

        public List<GameObject> GetEnemyObjects()
        {
            var enemyObjects = new List<GameObject>();
            foreach (string enemy in EnemyIds)
            {
                enemyObjects.Add(ObjectDict[enemy]);
            }
            return enemyObjects;
        }
        
        public GameObjectManager(List<GameObject> objects)
        {
            foreach (var gameObject in objects)
            {
                ObjectDict[gameObject.ID] = gameObject;
            }
            ObjectMap = new ObjectMap(Globals.Zone.Width, Globals.Zone.Height, ObjectDict.Values);
        }

        [JsonConstructor]
        public GameObjectManager(Dictionary<string, GameObject> objectDict, HashSet<string> enemyIds)
        {
            ObjectDict = objectDict;
            ObjectMap = new ObjectMap(Globals.Zone.Width, Globals.Zone.Height, ObjectDict.Values);
            EnemyIds = enemyIds;
        }
        
        public void Remove(GameObject gameObject)
        {
            ObjectDict.Remove(gameObject.ID);
            EnemyIds.Remove(gameObject.ID);
            ObjectMap.Remove(gameObject);
        }
        
        // TODO: Remove these, make GameObjects request to move
        public void RemoveFromMap(GameObject gameObject)
        {
            ObjectMap.Remove(gameObject);
        }
        
        public void AddToMap(GameObject gameObject)
        {
            ObjectMap.Add(gameObject);
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
                foreach (var gameObject in Globals.GameObjectManager.ObjectDict.Values)
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
                    foreach (var lightComponent in gameObject.LightComponents.Values)
                    {
                        lightComponent.SourceObject = gameObject;
                        foreach (var animation in lightComponent.Animations.Values)
                        {
                            animation.AnimatedElement = lightComponent;
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

        public void Update()
        {
            foreach (var gameObject in ObjectDict.Values.ToList())
                gameObject.Update();
        }
    }
}