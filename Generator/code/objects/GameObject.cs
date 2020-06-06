using System;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Generator
{
    /*
     * Represents every object you will see in the game.
     * Used as a basis for terrain, playable characters, and enemies.
     */
    public class GameObject : GameElement
    {
        // Constructor
        public GameObject(

            // Grid logic
            Vector3 position,
            Vector3? size = null,

            // Movement logic
            Vector3? movementPosition = null,
            float? movementDirection = null,
            float? movementSpeed = null,
            float? directionOverride = null,

            // Animation attributes
            string componentSpriteFileName = "Ninja",
            string spriteFile = null,
            Dictionary<string, Component> components = null,

            // Actions
            bool isWalking = false,
            bool isSwinging = false,
            bool isShooting = false,
            bool isHurting = false,

            // Resources
            int baseHealth = 3,
            int baseElectricity = 0,

            // Primary Attributes
            int baseStrength = 0,
            int baseSpeed = 0,
            int baseSense = 0,
            int baseStyle = 0,
            int baseDefense = 0,

            // ...Other Attributes
            string id = null,
            string name = null,
            int level = 1,
            int experience = 0,
            float direction = (float)Math.PI,
            Vector3? brightness = null,

            List<code.objects.Ailment> ailments = null,

            // Abilities
            List<Ability> abilities = null,
            Dictionary<string, code.actions.Strategy> strategies = null,
            string strategyName = null,

            // Interaction
            Conversation conversation = null,
            Cached<Action<GameObject>> ai = null,
            Cached<Action<GameObject, GameObject>> collisionEffect = null,
            bool temporary = false,
            Cached<Action<GameObject, GameObject>> activationEffect = null,

            // Equipment
            Armor armor = null,
            GeneratorObj generator = null,
            Accessory accessory = null
        )
        {
            // Animation attributes
            ComponentSpriteFileName = componentSpriteFileName;
            Components = components ?? GenerateDefaultComponentDict();
            LinkComponents();
            SpriteFile = spriteFile;

            // Actions
            IsWalking = isWalking;
            IsSwinging = isSwinging;
            IsShooting = isShooting;
            IsHurting = isHurting;

            // Resources
            Health = new Resource("Health", baseHealth);
            Electricity = new Resource("Electricity", baseElectricity, regeneration: 10);

            // Primary Attributes
            Strength = new Attribute(baseStrength);
            Speed = new Attribute(baseSpeed);
            Smarts = new Attribute(baseSense);
            Style = new Attribute(baseStyle);
            Defense = new Attribute(baseDefense);

            // ...Other Attributes
            ID = id ?? Guid.NewGuid().ToString();
            Name = name;
            Level = level;
            Experience = experience;
            Brightness = brightness ?? Vector3.Zero;
            MovementSpeed = movementSpeed;

            Ailments = ailments ?? new List<code.objects.Ailment>();

            // Equipment
            EquippedArmor = armor ?? new Armor("[No Armor]", new Cached<Texture2D>("Sprites/white_dot"));
            EquippedGenerator = generator ?? new GeneratorObj("[No Generator]", new Cached<Texture2D>("Sprites/white_dot"));
            EquippedAccessory = accessory ?? new Accessory("[No Accessory]", new Cached<Texture2D>("Sprites/white_dot"));

            // Abilities
            Abilities = abilities ?? new List<Ability>();
            foreach (var ability in Abilities) ability.SourceObject = this;
            Strategies = strategies ?? (Dictionary<string, code.actions.Strategy>)Globals.Copy(code.actions.Strategy.Strategies);
            StrategyName = strategyName ?? "Whatevs";

            // Interaction
            Conversation = conversation;
            if (Conversation != null) Conversation.SourceObject = this;
            ActivationEffect = activationEffect;
            AI = ai ?? new Cached<Action<GameObject>>("DefaultAI"); // Run on each Update - argument is this
            CollisionEffect = collisionEffect; // Run when attempting to move into another object - arguments are this, other
            Temporary = temporary; // If true, destroy this object as soon as it's no longer being updated

            // Grid logic
            Size = size ?? Vector3.One;
            base.Position = position;
            Direction = direction;
            MovementTarget = movementPosition;
            MovementDirection = movementDirection;
            DirectionOverride = directionOverride;
            Area = new RectangleF(Position.X, Position.Y, Size.X, Size.Y);

            Globals.Log(ID + " has spawned.");
        }

        // name of component sprite file for this game object
        private string ComponentSpriteFileName;

        // Sprites
        public Dictionary<string, Component> Components;
        private string SpriteFile;
        [JsonIgnore]
        private Texture2D _Sprite;
        [JsonIgnore]
        public override Texture2D Sprite {
            get
            {
                if (_Sprite == null && SpriteFile != null)
                {
                    _Sprite = Globals.ContentManager.Load<Texture2D>(SpriteFile);
                }
                return _Sprite;
            }
            set { throw new NotSupportedException("Set the _Sprite instead."); }
        }

        // Toggleables
        private bool _isWalking;
        public bool IsWalking
        {
            get => _isWalking;
            set
            {
                if (value & !IsWalking)
                {
                    foreach (var component in Components)
                    {
                        if (component.Value.Animations.ContainsKey("Walk"))
                        {
                            component.Value.Animations["Walk"].Start();
                        }
                    }
                }
                else if (!value & IsWalking)
                {
                    foreach (var component in Components)
                    {
                        if (component.Value.Animations.ContainsKey("Walk"))
                        {
                            component.Value.Animations["Walk"].Stop();
                        }
                    }
                }
                _isWalking = value;
            }
        }
        public bool IsSwinging;
        public bool IsShooting;
        public bool IsHurting;

        // Location
        override public float Direction { get; set; }
        public Vector3? MovementTarget;
        public float? MovementDirection;
        public float? DirectionOverride;  // This is a hack for ability targeting
        public float? MovementSpeed;  // 1 = running, .5 = walking, etc
        override public Vector3 Position
        {
            get => _Position + AnimationOffset;
            set
            {
                // If it's outside the zone and we're temporary then kill yourself
                if (value.X < 0 || value.Y < 0 || value.X > Globals.Zone.Width || value.Y > Globals.Zone.Height || value.Z < 0)
                {
                    if (Temporary)
                    {
                        Globals.GameObjectManager.Kill(this);
                    }
                    else
                    {
                        Globals.Warn(Name ?? ID + " can't move to " + value);
                    }
                    return;
                }

                // If we can move there then move there
                var targetPosition = new RectangleF(value.X, value.Y, Size.X, Size.Y);
                var targetAtPosition = GetTargetInArea(targetPosition);
                if (targetAtPosition == null)
                {
                    Globals.GameObjectManager?.ObjectMap.Remove(this);
                    base.Position = value;
                    Area = targetPosition;
                    Globals.GameObjectManager?.ObjectMap.Add(this);
                }

                // If not then we collide with the object and it collides with us
                // TODO: In the current implementation this will trigger twice per update if both are moving
                else
                {
                    CollisionEffect?.Value(this, targetAtPosition);
                    targetAtPosition.CollisionEffect?.Value(targetAtPosition, this);
                }
            }
        }
        [JsonIgnore]
        public RectangleF Area { get; set; }

        // Resources
        public Resource Health;
        public Resource Electricity;

        // Primary attributes
        public Attribute Strength;
        public Attribute Smarts;
        public Attribute Speed;
        public Attribute Style;  // Right???

        // Secondary attributes
        public Attribute Defense;

        public List<code.objects.Ailment> Ailments;

        // Brightness
        public Vector3 RelativeLightPosition = Vector3.Zero;
        public Vector3 Brightness;

        // Growth
        public int Level;
        public int Experience;

        // Abilities
        // TODO: Function to add new ability, should not be able to add it directly
        // This would make a copy and set the sourceObject
        public List<Ability> Abilities = new List<Ability>();
        public Dictionary<string, code.actions.Strategy> Strategies;
        public string StrategyName;

        // Interaction
        public Conversation Conversation;
        public Cached<Action<GameObject, GameObject>> ActivationEffect;
        public Cached<Action<GameObject>> AI;
        public Cached<Action<GameObject, GameObject>> CollisionEffect;
        public bool Temporary;

        private Armor _equippedArmor = new Armor("[No Armor]", new Cached<Texture2D>("Sprites/white_dot"));
        public Armor EquippedArmor
        {
            get => _equippedArmor;
            set { Equip(value); }
        }

        private GeneratorObj _equippedGenerator = new GeneratorObj("[No Generator]", new Cached<Texture2D>("Sprites/white_dot"));
        public GeneratorObj EquippedGenerator
        {
            get => _equippedGenerator;
            set { Equip(value); }
        }

        private Accessory _equippedAccessory = new Accessory("[No Accessory]", new Cached<Texture2D>("Sprites/white_dot"));
        public Accessory EquippedAccessory
        {
            get => _equippedAccessory;
            set { Equip(value); }
        }

        // When activating, say each thing in the ActivationText and perform the ActivationFunction
        public void Activate(GameObject other)
        {
            ActivationEffect?.Value(this, other);
            Conversation?.Start();
        }

        // Establish a two-way link between the components and this GameObject
        private void LinkComponents()
        {
            foreach (var component in Components)
            {
                component.Value.ID = component.Key;
                component.Value.SourceObject = this;
                foreach (var animation in component.Value.Animations)
                {
                    animation.Value.Name = animation.Key;
                    animation.Value.AnimatedElement = component.Value;
                    animation.Value.SourceObject = this;
                }
            }
        }

        // Equip some equipment!
        public void Equip(Equipment equipmentToEquip)
        {
            Equipment equippedEquipment;
            switch (equipmentToEquip.Slot)
            {
                case "Armor":
                    equippedEquipment = _equippedArmor;
                    _equippedArmor = (Armor)equipmentToEquip;
                    break;
                case "Generator":
                    equippedEquipment = _equippedGenerator;
                    _equippedGenerator = (GeneratorObj)equipmentToEquip;
                    break;
                case "Accessory":
                    equippedEquipment = _equippedAccessory;
                    _equippedAccessory = (Accessory)equipmentToEquip;
                    break;
                default:
                    throw new ArgumentException("Unknown equipment slot: " + equipmentToEquip.Slot);
            }

            // Resources
            Health.Max += equipmentToEquip.Health - equippedEquipment.Health;
            Electricity.Max += equipmentToEquip.Capacity - equippedEquipment.Capacity;

            // Attributes
            Strength.Modifier += equipmentToEquip.Strength - equippedEquipment.Strength;
            Smarts.Modifier += equipmentToEquip.Sense - equippedEquipment.Sense;
            Speed.Modifier += equipmentToEquip.Speed - equippedEquipment.Speed;
            Style.Modifier += equipmentToEquip.Style - equippedEquipment.Style;
            Defense.Modifier += equipmentToEquip.Defense - equippedEquipment.Defense;
        }

        // Create the default ComponentDictionary
        private Dictionary<string, Component> GenerateDefaultComponentDict()
        {
            Dictionary<string, Component> result = new Dictionary<string, Component>()
            {
                {"Head", new Component(
                    id: "Head",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .506f, 1.26f),
                    relativeSize: .32f,
                    baseRotationPoint: new Vector3(.16f, 0, .256f),
                    yOffset: -.05f)
                },
                {"Face", new Component(
                    id: "Face",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .57f, 1),
                    relativeSize: .14f,
                    baseRotationPoint: new Vector3(.08f, 0, .066f),
                    yOffset: -.05f)
                },
                {"Body", new Component(
                    id: "Body",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .505f, .47f),
                    relativeSize: .16f,
                    baseRotationPoint: new Vector3(.08f, 0, .08f))
                },
                {"Arm/Left", new Component(
                    id: "Arm/Left",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.08f, .504f, .515f),
                    relativeSize: .08f,
                    relativeRotation: new Vector3(0, .4f, 0),
                    baseRotationPoint: new Vector3(.04f, 0, .023f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero
                                },
                                duration: 3
                                )
                            )
                        }
                    })
                },
                {"Arm/Right", new Component(
                    id: "Arm/Right",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.92f, .504f, .515f),
                    relativeSize: .08f,
                    relativeRotation: new Vector3(0, -.4f, 0),
                    baseRotationPoint: new Vector3(.04f, 0, .023f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero
                                },
                                duration: 3
                                )
                            )
                        }
                    })
                },
                {"Hand/Left", new Component(
                    id: "Hand/Left",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(-.25f, .5045f, .35f),
                    relativeSize: .08f,
                    baseRotationPoint: new Vector3(.04f, 0, .032f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero
                                },
                                duration: 3
                                )
                            )
                        }
                    })
                },
                {"Hand/Right", new Component(
                    id: "Hand/Right",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(1.25f, .5045f, .35f),
                    relativeSize: .08f,
                    baseRotationPoint: new Vector3(.04f, 0, .032f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero
                                },
                                duration: 3
                                )
                            )
                        }
                    })
                },
                {"Leg/Left", new Component(
                    id: "Leg/Left",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.23f, .504f, .14f),
                    relativeSize: .08f,
                    baseRotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .15f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero
                                },
                                duration: 3
                                )
                            )
                        }
                    })
                },
                {"Leg/Right", new Component(
                    id: "Leg/Right",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.77f, .504f, .14f),
                    relativeSize: .08f,
                    baseRotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .15f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero
                                },
                                duration: 3
                                )
                            )
                        }
                    })
                }
            };

            return result;
        }

        // What to do on each frame
        public void Update()
        {
            // Do whatever the object wants to do
            AI?.Value(this);
            if (MovementTarget != null || MovementDirection != null)
            {
                if (MovementTarget != null)
                {
                    // TODO: Pathfinding
                    var distanceToMove = Vector3.Distance(MovementTarget.Value, Position);
                    MovementSpeed = Math.Min(distanceToMove / 3, 1);
                    if (distanceToMove < .001f || distanceToMove <= GetMovementDistance())
                    {
                        IsWalking = true;
                        Position = MovementTarget.Value;
                        MovementTarget = null;
                        MovementDirection = null;
                    }
                    else
                    {
                        MoveInDirection((float)MathTools.Angle(Position, MovementTarget.Value));
                    }
                }
                else
                {
                    MovementSpeed = MovementSpeed ?? 1;
                    MoveInDirection(MovementDirection.Value);
                }
            }
            else
            {
                MovementSpeed = null;
                IsWalking = false;
            }
            if (DirectionOverride != null)
            {
                Direction = (float)DirectionOverride;
                DirectionOverride = null;  // This is a hack; will need to set this every update
            }

            // Update resources
            Health.Update();
            Electricity.Update();

            // Update ailments
            foreach (var ailment in Ailments) { ailment.Update(); }

            // Update animation
            foreach (var component in Components) component.Value.Update();

            // Use abilities
            foreach (var ability in Abilities) ability.Update();
        }

        // Deal damage to a target
        public void DealDamage(GameObject target, int damage)
        {
            Globals.Log(this + " attacks for " + damage + " damage.");
            target.TakeDamage(damage);
        }

        public int DamageToTake(int damage)
        {
            return damage - Defense.CurrentValue;
        }

        // Take damage
        public void TakeDamage(int damage)
        {
            damage = DamageToTake(damage);

            if (damage > 0)
            {
                Globals.Log(this + " takes " + damage + " damage. "
                        + Health.Current + " -> " + (Health.Current - damage));

                // Add this to the set of enemies
                if (!Globals.Party.Value.MemberIDs.Contains(ID))
                {
                    Globals.GameObjectManager.Enemies.Add(ID);
                }

                IsHurting = true;
                // TODO: We can't hardcode "Face"
                // Components["Face"].SpriteFile = componentSpriteFileName + "/Face04";
                Health.Current -= damage;

                if (Health.Current <= 0)
                {
                    Globals.GameObjectManager.Kill(this);
                }
            }
        }

        // TODO: Replace all of these with new collision logic

        // Gets the coordinates at the range specified
        public Vector3 GetTargetCoordinates(float range = 1, float? direction = null)
        {
            var offsets = MathTools.OffsetFromRadians(direction ?? Direction) * range;
            var center = Center;
            var target = new Vector3(center.X + offsets.X, center.Y + offsets.Y, center.Z);
            return target;
        }

        // Gets whichever object is exactly [distance] away in the current direction
        public GameObject GetTargetAtRange(float range = 1, float? direction = null)
        {
            // See if we would overlap with any other objects
            var target = GetTargetCoordinates(range, direction);
            foreach (var gameObject in Globals.GameObjectManager.ObjectList.Values)
            {
                if (gameObject != this)
                {
                    if (gameObject.Area.Contains(target.X, target.Y))
                    {
                        return gameObject;
                    }
                }
            }

            return null;
        }

        // See if we can move to a location
        public GameObject GetTargetInArea(RectangleF targetArea)
        {
            // See if we would overlap with any other objects
            if (Globals.Zone != null)
            {
                for (int x = (int)Math.Floor(targetArea.Left); x <= (int)Math.Ceiling(targetArea.Right); x++)
                {
                    for (int y = (int)Math.Floor(targetArea.Top); y <= (int)Math.Ceiling(targetArea.Bottom); y++)
                    {
                        foreach (var gameObject in Globals.GameObjectManager.ObjectMap.Get(x, y))
                        {
                            if (gameObject != this)
                            {
                                if (targetArea.IntersectsWith(gameObject.Area))
                                {
                                    return gameObject;
                                }
                            }
                        }
                    }
                }
            }

            // If not then we're clear to move
            return null;
        }

        // Gets whichever object is [distance] away or closer in the current direction
        public GameObject GetTargetInRange(int range = 1)
        {
            GameObject returnObject = null;
            var targetRange = 1;

            // Loop from 1 to [range], seeing if anything is in the way
            while (returnObject == null && targetRange <= range)
            {
                returnObject = GetTargetAtRange(targetRange);
                targetRange++;
            }

            return returnObject;
        }

        public float GetMovementDistance()
        {
            return (MovementSpeed ?? 0) * 2 * (float)Math.Sqrt(Speed.CurrentValue) * Timing.GameSpeed / Globals.RefreshRate;
        }

        // Attempts to move the object in a direction (radians).
        private void MoveInDirection(float radians = 0)
        {
            // Update sprite visuals
            IsWalking = true;
            Direction = radians;

            // Get distance
            var distance = GetMovementDistance();
            var offsets = MathTools.OffsetFromRadians(radians);
            var newPosition = new Vector3(
                _Position.X + distance * offsets.X,
                _Position.Y + distance * offsets.Y,
                _Position.Z);

            // See if you can move to the location
            Position = newPosition;
        }

        // Submit message to the screen with icon
        public string IsSaying;
        public void Say(string message)
        {
            IsSaying = message;
            Timing.AddEvent(
                .1f * message.Length + 1,
                () => { if (IsSaying == message) IsSaying = null; });
        }

        // Return name, useful for debugging.
        public override string ToString()
        {
            return ID ?? "Unnamed GameObject";
        }

        public GameObject GetNearest(IEnumerable<GameObject> gameObjects)
        {
            // TODO: If this hits performance issues (especially in the case where the list of gameObjects is large) 
            // then I can try branching outward from the gameObject, and if any are in the immediate area then return 
            // the nearest out of those.
            GameObject nearestObject = null;
            float nearestDistance = 10000;
            foreach (var gameObject in gameObjects)
            {
                var distance = Vector3.Distance(Position, gameObject.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestObject = gameObject;
                }
            }
            return nearestObject;
        }
    }
}
