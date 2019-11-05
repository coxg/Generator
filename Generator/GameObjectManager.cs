using System.Collections.Generic;
using Microsoft.Xna.Framework;

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

            Globals.Player = new GameObject(
                new Vector3(50, 50, 0),
                stamina: 100,
                strength: 10,
                speed: 100,
                sense: 90,
                style: 100,
                name: "Niels",
                partyNumber: 0,
                componentSpriteFileName: "Girl",
                weapon: new Weapon(
                    name: "Sword",
                    sprite: Globals.WhiteDot,
                    damage: 10),
                brightness: Vector3.One);

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
                activationTextList: new List<List<string>> {
                    new List<string> { "Check it out, I do something weird!", "Did you see how weird that was?!" } });
            terrain1.ActivationEffect = delegate
            {
                if (!ObjectFromName.ContainsKey("big terrain"))
                {
                    terrain1.ActivationText = new List<List<string>> { new List<string> { "There's already a boy!" } };

                    new GameObject(
                        new Vector3(60, 60, 0), 
                        new Vector3(5, 1, 5), 
                        name: "big terrain", 
                        strength: 10, 
                        speed: 10, 
                        sense: 10,
                        activationTextList: new List<List<string>> {
                            new List<string> { "I don't do anything weird.", "...I'm just really fat." },
                            new List<string> { "Well, other than saying something different the second time you talk to me. " +
                                               "\nThat's pretty cool I guess. \nIf you're into that kind of thing." }
                        });
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