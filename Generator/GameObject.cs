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

            // Animation attributes
            string componentSpriteFileName = "Ninja",
            string spriteFile = null,
            Dictionary<string, Component> components = null,
            bool castsShadow = true,

            // Actions
            bool isWalking = false,
            bool isSwinging = false,
            bool isShooting = false,
            bool isHurting = false,

            // Resources
            int baseHealth = 100,
            int baseStamina = 0,
            int baseElectricity = 0,

            // Primary Attributes
            int baseStrength = 0,
            int baseSpeed = 0,
            int baseSense = 0,
            int baseStyle = 0,

            // ...Other Attributes
            string id = null,
            string name = null,
            int level = 1,
            int experience = 0,
            float direction = (float)Math.PI,
            Vector3? brightness = null,

            // Abilities
            List<Ability> abilities = null,

            // Interaction
            Conversation conversation = null,
            Loaded<Action<GameObject>> ai = null,
            Loaded<Action<GameObject, GameObject>> collisionEffect = null,
            bool temporary = false,
            Loaded<Action<GameObject, GameObject>> activationEffect = null,

            // Equipment
            Weapon weapon = null,
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
            CastsShadow = castsShadow;

            // Actions
            IsWalking = isWalking;
            IsSwinging = isSwinging;
            IsShooting = isShooting;
            IsHurting = isHurting;

            // Resources
            Health = new Resource("Health", baseHealth);
            Stamina = new Resource("Stamina", baseStamina, 10);
            Electricity = new Resource("Electricity", baseElectricity);

            // Primary Attributes
            Strength = new Attribute(baseStrength);
            Speed = new Attribute(baseSpeed);
            Sense = new Attribute(baseSense);
            Style = new Attribute(baseSense);

            // ...Other Attributes
            ID = id ?? Guid.NewGuid().ToString();
            Name = name;
            Level = level;
            Experience = experience;
            Direction = direction;
            Brightness = brightness ?? Vector3.Zero;

            // Equipment
            EquippedWeapon = weapon ?? new Weapon("Fists", new Loaded<Texture2D>("Sprites/white_dot"));
            EquippedArmor = armor ?? new Armor("[No Armor]", new Loaded<Texture2D>("Sprites/white_dot"));
            EquippedGenerator = generator ?? new GeneratorObj("[No Generator]", new Loaded<Texture2D>("Sprites/white_dot"));
            EquippedAccessory = accessory ?? new Accessory("[No Accessory]", new Loaded<Texture2D>("Sprites/white_dot"));

            // Abilities
            Abilities = abilities != null ? (List<Ability>) Globals.Copy(abilities) : new List<Ability>();

            // Interaction
            Conversation = conversation;
            if (Conversation != null) Conversation.SourceObject = this;
            ActivationEffect = activationEffect;
            AI = ai; // Run on each Update - argument is this
            CollisionEffect = collisionEffect; // Run when attempting to move into another object - arguments are this, other
            Temporary = temporary; // If true, destroy this object as soon as it's no longer being updated
            GameObjectManager.AddNewObject(ID, this);

            // Grid logic
            Size = size ?? Vector3.One;
            Position = position;

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
                    _Sprite = Globals.Content.Load<Texture2D>(SpriteFile);
                }
                return _Sprite;
            }
            set { throw new NotSupportedException("Set the _Sprite instead."); }
        }

        // Toggleables
        private bool _isWalking;
        [JsonIgnore]
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
        override public Vector3 Position
        {
            get => _Position + AnimationOffset;
            set
            {
                // If we can move there then move there
                var targetAtPosition = GetTargetAtPosition(value);
                if (targetAtPosition == null)
                {
                    base.Position = value;
                    Area = new RectangleF(Position.X, Position.Y, Size.X, Size.Y);
                }

                // If not then we collide with the object and it collides with us
                else
                {
                    CollisionEffect?.Value(this, targetAtPosition);
                    targetAtPosition.CollisionEffect?.Value(targetAtPosition, this);
                }
            }
        }
        public RectangleF Area { get; set; }

        // Resources
        public Resource Health;
        public Resource Stamina;
        public Resource Electricity;

        // Primary Attributes
        public Attribute Strength;
        public Attribute Sense;
        public Attribute Speed;
        public Attribute Style;  // Right???

        // Brightness
        public Vector3 RelativeLightPosition = Vector3.Zero;
        public Vector3 Brightness;

        // Growth
        public int Level;
        public int Experience;

        // Abilities
        private List<Ability> _abilities = new List<Ability>();
        [JsonIgnore]
        public List<Ability> Abilities
        {
            get => _abilities;
            set 
            {
                _abilities = value;
                foreach (var ability in _abilities) ability.SourceObject = this;

                if (Abilities.Count >= 1) Ability1 = Abilities[0];
                if (Abilities.Count >= 2) Ability2 = Abilities[1];
                if (Abilities.Count >= 3) Ability3 = Abilities[2];
                if (Abilities.Count >= 4) Ability4 = Abilities[3];
            }
        }
        [JsonIgnore]
        public Ability Ability1;
        [JsonIgnore]
        public Ability Ability2;
        [JsonIgnore]
        public Ability Ability3;
        [JsonIgnore]
        public Ability Ability4;

        // Interaction
        public Conversation Conversation;
        public Loaded<Action<GameObject, GameObject>> ActivationEffect;
        public Loaded<Action<GameObject>> AI;
        public Loaded<Action<GameObject, GameObject>> CollisionEffect;
        public bool Temporary;
        public bool IsVisible()
        {
            return GameControl.camera.VisibleArea.IntersectsWith(Area);
        }
        public bool IsUpdating()
        {
            return GameControl.camera.UpdatingArea.IntersectsWith(Area);
        }

        // Equipment
        private Weapon _equippedWeapon = new Weapon("Fists", new Loaded<Texture2D>("Sprites/white_dot"));
        public Weapon EquippedWeapon
        {
            get => _equippedWeapon;
            set { Equip(value); }
        }

        private Armor _equippedArmor = new Armor("[No Armor]", new Loaded<Texture2D>("Sprites/white_dot"));
        public Armor EquippedArmor
        {
            get => _equippedArmor;
            set { Equip(value); }
        }

        private GeneratorObj _equippedGenerator = new GeneratorObj("[No Generator]", new Loaded<Texture2D>("Sprites/white_dot"));
        public GeneratorObj EquippedGenerator
        {
            get => _equippedGenerator;
            set { Equip(value); }
        }

        private Accessory _equippedAccessory = new Accessory("[No Accessory]", new Loaded<Texture2D>("Sprites/white_dot"));
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
            Equipment equippedEquipment = null;
            switch (equipmentToEquip.Slot)
            {
                case "Weapon":
                    equippedEquipment = _equippedWeapon;
                    break;
                case "Armor":
                    equippedEquipment = _equippedArmor;
                    break;
                case "Generator":
                    equippedEquipment = _equippedGenerator;
                    break;
                case "Accessory":
                    equippedEquipment = _equippedAccessory;
                    break;
                default:
                    break;
            }

            // Resources
            Health.Max += equipmentToEquip.Health - equippedEquipment.Health;
            Stamina.Max += equipmentToEquip.Stamina - equippedEquipment.Stamina;
            Electricity.Max += equipmentToEquip.Capacity - equippedEquipment.Capacity;

            // Attributes
            Strength.Modifier += equipmentToEquip.Strength - equippedEquipment.Strength;
            Sense.Modifier += equipmentToEquip.Sense - equippedEquipment.Sense;
            Speed.Modifier += equipmentToEquip.Speed - equippedEquipment.Speed;
            Style.Modifier += equipmentToEquip.Style - equippedEquipment.Style;

            equippedEquipment = equipmentToEquip;
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
                    rotationPoint: new Vector3(.16f, 0, .256f),
                    yOffset: -.05f)
                },
                {"Face", new Component(
                    id: "Face",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .57f, 1),
                    relativeSize: .14f,
                    rotationPoint: new Vector3(.08f, 0, .066f),
                    yOffset: -.05f,
                    castsShadow: false)
                },
                {"Body", new Component(
                    id: "Body",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .505f, .47f),
                    relativeSize: .16f,
                    rotationPoint: new Vector3(.08f, 0, .08f))
                },
                {"Arm/Left", new Component(
                    id: "Arm/Left",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.08f, .504f, .515f),
                    relativeSize: .08f,
                    relativeRotation: new Vector3(0, .4f, 0),
                    rotationPoint: new Vector3(.04f, 0, .023f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0)
                                },
                                duration: 3
                                )
                            )
                        }
                    })
                },
                {"Arm/Right", new Component(
                    id: "Arm/Righ",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.92f, .504f, .515f),
                    relativeSize: .08f,
                    relativeRotation: new Vector3(0, -.4f, 0),
                    rotationPoint: new Vector3(.04f, 0, .023f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0)
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
                    rotationPoint: new Vector3(.04f, 0, .032f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0)
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
                    rotationPoint: new Vector3(.04f, 0, .032f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0)
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
                    rotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .15f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    new Vector3(-.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(.7f, 0, 0)
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
                    rotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .15f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                baseRotations: new List<Vector3>
                                {
                                    new Vector3(.7f, 0, 0),
                                    Vector3.Zero,
                                    new Vector3(-.7f, 0, 0)
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
            // Perform whatever actions the object wants to perform
            AI?.Value(this);

            // Update resources
            Health.Update();
            Stamina.Update();
            Electricity.Update();

            // Update animation
            foreach (var component in Components) component.Value.Update();

            // Use abilities
            foreach (var ability in Abilities) ability.Update();
        }

        public void Remove()
        {
            GameObjectManager.RemoveObject(ID);
            GameObjectManager.Updating.Remove(ID);
            GameObjectManager.Visible.Remove(ID);
        }

        // Plays death animation and despawns
        // TODO: Drop equipment + inventory
        // TODO: Play death animation
        public void Die()
        {
            Remove();
            Globals.Log(this + " has passed away. RIP.");
        }

        // Deal damage to a target
        public void DealDamage(GameObject target, int damage)
        {
            Globals.Log(this + " attacks for " + damage + " damage.");
            target.TakeDamage(damage);
        }

        // Take damage
        public void TakeDamage(int damage)
        {
            if (damage > 0)
            {
                Globals.Log(this + " takes " + damage + " damage. "
                        + Health.Current + " -> " + (Health.Current - damage));

                IsHurting = true;
                // TODO: We can't hardcode "Face"
                // Components["Face"].SpriteFile = componentSpriteFileName + "/Face04";
                Health.Current -= damage;

                if (Health.Current <= 0)
                {
                    Die();
                }
            }
        }

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
            foreach (var gameObject in GameObjectManager.ObjectFromID.Values)
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
        public GameObject GetTargetAtPosition(Vector3 position)
        {
            // See if we would overlap with any other objects
            var targetArea = new RectangleF(position.X, position.Y, Size.X, Size.Y);
            foreach (var gameObject in GameObjectManager.ObjectFromID.Values)
            {
                if (gameObject != this)
                {
                    if (targetArea.IntersectsWith(gameObject.Area))
                    {
                        return gameObject;
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

        // Attempts to move the object in a direction (radians).
        public void MoveInDirection(
                float radians = 0,
                float speed = 1  // Relative to Speed.CurrentValue; 1 == running, .5 == walking, etc
            )
        {
            // Update sprite visuals
            IsWalking = true;
            Direction = radians;

            // Get distance
            var distance = speed * (float) Math.Sqrt(Speed.CurrentValue) * Timing.GameSpeed / Globals.RefreshRate;

            // Convert from radian direction to X/Y offsets
            var offsets = MathTools.OffsetFromRadians(radians);
            var newPosition = new Vector3(
                _Position.X + distance * offsets.X,
                _Position.Y + distance * offsets.Y,
                _Position.Z);

            // See if you can move to the location
            Position = newPosition;
        }

        // Submit message to the screen with icon
        public void Say(string message)
        {
            // TODO: Floating text bubbles
        }

        // Return name, useful for debugging.
        public override string ToString()
        {
            return ID ?? "Unnamed GameObject";
        }
    }
}