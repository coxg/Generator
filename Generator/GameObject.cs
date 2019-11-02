using System;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    /*
     * Represents every object you will see in the game.
     * Used as a basis for terrain, playable characters, and enemies.
     */
    public class GameObject : GameElement
    {
        // name of component sprite file for this game object
        private string ComponentSpriteFileName;

        // Sprites
        public Dictionary<string, Component> Components;
        public override Texture2D Sprite { get; set; }

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
                    CollisionEffect?.Invoke(this, targetAtPosition);
                    targetAtPosition.CollisionEffect?.Invoke(targetAtPosition, this);
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
        public Attribute Perception;
        public Attribute Speed;
        public Attribute Weight; // Roughly in pounds

        // Brightness
        public Vector3 RelativeLightPosition = Vector3.Zero;
        public Vector3 Brightness;

        // Growth
        public int Level;
        public int Experience;

        // Abilities
        private List<Ability> _abilities;
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
        public Ability Ability1;
        public Ability Ability2;
        public Ability Ability3;
        public Ability Ability4;

        // Interaction
        public int PartyNumber;
        public List<List<string>> ActivationText;
        public Action ActivationEffect;
        private int ActivationIndex;
        public Action<GameObject> AI;
        public Action<GameObject, GameObject> CollisionEffect;
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
        private Weapon _equippedWeapon = new Weapon("Fists", Globals.WhiteDot);
        public Weapon EquippedWeapon
        {
            get => _equippedWeapon;
            set { Equip(value); }
        }

        private Armor _equippedArmor = new Armor("[No Armor]", Globals.WhiteDot);
        public Armor EquippedArmor
        {
            get => _equippedArmor;
            set { Equip(value); }
        }

        private GeneratorObj _equippedGenerator = new GeneratorObj("[No Generator]", Globals.WhiteDot);
        public GeneratorObj EquippedGenerator
        {
            get => _equippedGenerator;
            set { Equip(value); }
        }

        private Accessory _equippedAccessory = new Accessory("[No Accessory]", Globals.WhiteDot);
        public Accessory EquippedAccessory
        {
            get => _equippedAccessory;
            set { Equip(value); }
        }

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
            int health = 100,
            int stamina = 0,
            int electricity = 0,

            // Primary Attributes
            int strength = 0,
            int speed = 0,
            int perception = 0,
            int weight = 150,

            // ...Other Attributes
            string name = null,
            int level = 1,
            int experience = 0,
            float direction = (float)Math.PI,
            Vector3? brightness = null,

            // Abilities
            List<Ability> abilities = null,

            // Interaction
            int partyNumber = -1,
            Action<GameObject> ai = null,
            Action<GameObject, GameObject> collisionEffect = null,
            bool temporary = false,
            Action activationEffect = null,
            string activationText = null,
            List<List<string>> activationTextList = null,

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
            Sprite = spriteFile == null ? null : Globals.Content.Load<Texture2D>(spriteFile);
            CastsShadow = castsShadow;

            // Actions
            IsWalking = isWalking;
            IsSwinging = isSwinging;
            IsShooting = isShooting;
            IsHurting = isHurting;

            // Resources
            Health = new Resource("Health", health);
            Stamina = new Resource("Stamina", stamina, 10);
            Electricity = new Resource("Electricity", electricity);

            // Primary Attributes
            Strength = new Attribute(strength);
            Speed = new Attribute(speed);
            Perception = new Attribute(perception);
            Weight = new Attribute(weight); // TODO: Use this in knockback calculation

            // ...Other Attributes
            Name = name ?? Guid.NewGuid().ToString();
            Level = level;
            Experience = experience;
            Direction = direction;
            Brightness = brightness ?? Vector3.Zero;

            // Equipment
            EquippedWeapon = weapon ?? new Weapon("Fists", Globals.WhiteDot);
            EquippedArmor = armor ?? new Armor("[No Armor]", Globals.WhiteDot);
            EquippedGenerator = generator ?? new GeneratorObj("[No Generator]", Globals.WhiteDot);
            EquippedAccessory = accessory ?? new Accessory("[No Accessory]", Globals.WhiteDot);

            // Abilities
            Abilities = abilities ?? DefaultAbilities.GenerateDefaultAbilities(this);

            // Interaction
            PartyNumber = partyNumber;
            ActivationEffect = activationEffect;
            AI = ai; // Run on each Update - argument is this
            CollisionEffect = collisionEffect; // Run when attempting to move into another object - arguments are this, other
            Temporary = temporary; // If true, destroy this object as soon as it's no longer being updated
            GameObjectManager.AddNewObject(Name, this);

            // Can either give a string or a list of lists, depending on how complicated the text is
            if (activationTextList != null)
            {
                ActivationText = activationTextList;
            }
            else if (activationText != null)
            {
                ActivationText = new List<List<string>> { new List<string> { activationText } };
            }
            else if (Name != null)
            {
                ActivationText = new List<List<string>> { new List<string> { "This is just a " + Name + "." } };
            }

            // Grid logic
            Size = size ?? Vector3.One;
            Position = position;
            
            Globals.Log(Name + " has spawned.");
        }

        // When activating, say each thing in the ActivationText and perform the ActivationFunction
        public void Activate()
        {
            foreach (var text in ActivationText[ActivationIndex])
            {
                Say(text);
            }
            ActivationEffect?.Invoke();
            ActivationIndex = (int)MathTools.Mod(ActivationIndex + 1, ActivationText.Count);
        }

        // Establish a two-way link between the components and this GameObject
        private void LinkComponents()
        {
            foreach (var component in Components)
            {
                component.Value.Name = component.Key;
                component.Value.SourceObject = this;
                foreach (var animation in component.Value.Animations)
                {
                    animation.Value.Name = animation.Key;
                    animation.Value.SourceElement = component.Value;
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
            Strength.CurrentValue += equipmentToEquip.Strength - equippedEquipment.Strength;
            Perception.CurrentValue += equipmentToEquip.Perception - equippedEquipment.Perception;
            Speed.CurrentValue += equipmentToEquip.Speed - equippedEquipment.Speed;

            equippedEquipment = equipmentToEquip;
        }

        // Create the default ComponentDictionary
        private Dictionary<string, Component> GenerateDefaultComponentDict()
        {
            Dictionary<string, Component> result = new Dictionary<string, Component>()
            {
                {"Head", new Component(
                    name: "Head",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .506f, 1.26f),
                    relativeSize: .32f,
                    rotationPoint: new Vector3(.16f, 0, .256f),
                    yOffset: -.05f)
                },
                {"Face", new Component(
                    name: "Face",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .57f, 1),
                    relativeSize: .14f,
                    rotationPoint: new Vector3(.08f, 0, .066f),
                    yOffset: -.05f,
                    castsShadow: false)
                },
                {"Body", new Component(
                    name: "Body",
                    spriteFile: ComponentSpriteFileName,
                    directional: true,
                    relativePosition: new Vector3(.5f, .505f, .47f),
                    relativeSize: .16f,
                    rotationPoint: new Vector3(.08f, 0, .08f))
                },
                {"Left Arm", new Component(
                    name: "Arm",
                    side: "Left",
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
                                rotations: new List<Vector3>
                                {
                                    new Vector3(.7f, 0, 0),
                                    new Vector3(-.7f, 0, 0)
                                },
                                duration: 1
                                )
                            )
                        }
                    })
                },
                {"Right Arm", new Component(
                    name: "Arm",
                    side: "Right",
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
                                rotations: new List<Vector3>
                                {
                                    new Vector3(-.7f, 0, 0),
                                    new Vector3(.7f, 0, 0)
                                },
                                duration: 1
                                )
                            )
                        }
                    })
                },
                {"Left Hand", new Component(
                    name: "Hand",
                    side: "Left",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(-.25f, .5045f, .35f),
                    relativeSize: .08f,
                    rotationPoint: new Vector3(.04f, 0, .032f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                rotations: new List<Vector3>
                                {
                                    new Vector3(.7f, 0, 0),
                                    new Vector3(-.7f, 0, 0)
                                },
                                duration: 1
                                )
                            )
                        }
                    })
                },
                {"Right Hand", new Component(
                    name: "Hand",
                    side: "Right",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(1.25f, .5045f, .35f),
                    relativeSize: .08f,
                    rotationPoint: new Vector3(.04f, 0, .032f),
                    yOffset: .001f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                rotations: new List<Vector3>
                                {
                                    new Vector3(-.7f, 0, 0),
                                    new Vector3(.7f, 0, 0)
                                },
                                duration: 1
                                )
                            )
                        }
                    })
                },
                {"Left Leg", new Component(
                    name: "Leg",
                    side: "Left",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.23f, .504f, .14f),
                    relativeSize: .08f,
                    rotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .15f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                rotations: new List<Vector3>
                                {
                                    new Vector3(-.7f, 0, 0),
                                    new Vector3(.7f, 0, 0)
                                },
                                duration: 1
                                )
                            )
                        }
                    })
                },
                {"Right Leg", new Component(
                    name: "Leg",
                    side: "Right",
                    spriteFile: ComponentSpriteFileName,
                    relativePosition: new Vector3(.77f, .504f, .14f),
                    relativeSize: .08f,
                    rotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .15f,
                    animations: new Dictionary<string, Animation>()
                    {
                        {"Walk", new Animation(
                            updateFrames: new Frames(
                                rotations: new List<Vector3>
                                {
                                    new Vector3(.7f, 0, 0),
                                    new Vector3(-.7f, 0, 0)
                                },
                                duration: 1
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
            AI?.Invoke(this);

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
            GameObjectManager.RemoveObject(Name);
            GameObjectManager.Updating.Remove(Name);
            GameObjectManager.Visible.Remove(Name);
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
            foreach (var gameObject in GameObjectManager.ObjectFromName.Values)
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
            foreach (var gameObject in GameObjectManager.ObjectFromName.Values)
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
            var distance = speed * (float) Math.Sqrt(Speed.CurrentValue) / Globals.RefreshRate;

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
            Globals.DisplayTextQueue.Enqueue(message);
            Globals.TalkingObjectQueue.Enqueue(this);
        }

        // Return name, useful for debugging.
        public override string ToString()
        {
            return Name ?? "Unnamed GameObject";
        }
    }
}