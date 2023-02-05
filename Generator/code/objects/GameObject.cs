using System;
using System.Collections;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using Generator.code.objects;
using Microsoft.Xna.Framework;

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
            Sprite sprite = null,
            Dictionary<string, Component> components = null,
            bool isWalking = false,

            // Resources
            int baseHealth = 3,
            int baseMana = 0,

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

            HashSet<Ailment> ailments = null,

            // Abilities
            List<Ability> abilities = null,

            // Interaction
            Conversation conversation = null,
            Cached<Action<GameObject>> ai = null,
            Cached<Action<GameObject, GameObject>> collisionEffect = null,
            bool temporary = false,
            Cached<Action<GameObject, GameObject>> activationEffect = null,

            // Equipment
            Armor armor = null,
            Accessory accessory = null
        )
        {
            // Animation attributes
            Components = components ?? GenerateDefaultComponentDict();
            LinkComponents();
            Sprite = sprite;
            IsWalking = isWalking;

            // Resources
            Health = new Resource("Health", baseHealth);
            Mana = new Resource("Mana", baseMana, 10);

            // Primary Attributes
            Strength = new Attribute(baseStrength);
            Speed = new Attribute(baseSpeed);
            Smarts = new Attribute(baseSense);
            Style = new Attribute(baseStyle);
            Defense = new Attribute(baseDefense);

            // ...Other Attributes
            ID = id ?? Guid.NewGuid().ToString();
            Name = name;

            Ailments = ailments ?? new HashSet<Ailment>();

            // Equipment
            EquippedArmor = armor ?? new Armor("[No Armor]", null);
            EquippedAccessory = accessory ?? new Accessory("[No Accessory]", null);

            // Abilities
            Abilities = abilities ?? new List<Ability>();
            foreach (var ability in Abilities)
            {
                AbilityCooldowns[ability.Name] = 0;
            }

            // Interaction
            Conversation = conversation;
            if (Conversation != null) Conversation.SourceObject = this;
            ActivationEffect = activationEffect;
            AI = ai ?? new Cached<Action<GameObject>>("DefaultAI"); // Run on each Update - argument is this
            Temporary = temporary; // If true, destroy this object as soon as it's no longer being updated

            // Location
            Size = size ?? Vector3.One;
            base.Position = position;
            Direction = direction;

            Globals.Log(ID + " has spawned.");
        }

        public Dictionary<string, Component> Components;

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

        // Location
        override public float Direction { get; set; }

        override public Vector3 Position
        {
            get => _Position + AnimationOffset;
            set
            {
                // If it's outside the zone and we're temporary then kill yourself
                if (value.X < 0 || value.Y < 0 || value.X > Globals.Zone.Width || value.Y > Globals.Zone.Height ||
                    value.Z < 0)
                {
                    if (Temporary)
                    {
                        Globals.GameObjectManager.Kill(this);
                    }
                    else
                    {
                        Globals.Warn(this + " can't move to " + value);
                    }

                    return;
                }

                // If we can move there then move there
                var targetPosition = new RectangleF(value.X, value.Y, Size.X, Size.Y);
                var targetAtPosition = GetClosest(GetTargets(targetPosition));
                if (targetAtPosition == null)
                {
                    Globals.GameObjectManager?.RemoveFromMap(this);
                    base.Position = value;
                    Globals.GameObjectManager?.AddToMap(this);
                }
            }
        }

        // Resources
        public Resource Health;
        public Resource Mana;

        // Primary attributes
        public Attribute Strength;
        public Attribute Smarts;
        public Attribute Speed;
        public Attribute Style;

        // Secondary attributes
        public Attribute Defense;
        public Attribute MagicDefense;

        public HashSet<Ailment> Ailments;

        // Abilities
        // TODO: Function to add new ability, should not be able to add it directly
        // This would make a copy and set the sourceObject
        public List<Ability> Abilities = new List<Ability>();
        public Dictionary<String, float> AbilityCooldowns = new Dictionary<String, float>();

        // Interaction
        public Conversation Conversation;
        public Cached<Action<GameObject, GameObject>> ActivationEffect;
        public Cached<Action<GameObject>> AI;
        public bool Temporary;

        private Armor _equippedArmor = new Armor("[No Armor]", null);

        public Armor EquippedArmor
        {
            get => _equippedArmor;
            set { Equip(value); }
        }

        private Accessory _equippedAccessory = new Accessory("[No Accessory]", null);

        public Accessory EquippedAccessory
        {
            get => _equippedAccessory;
            set { Equip(value); }
        }

        // When activating, say each thing in the ActivationText and perform the ActivationFunction
        public void Activate(GameObject other)
        {
            Globals.Log(this + " activates " + other);
            other.ActivationEffect?.Value(other, this);
            other.Conversation?.Start();
        }

        // Establish a two-way link between the components and this GameObject
        private void LinkComponents()
        {
            foreach (var component in Components)
            {
                component.Value.ID = component.Key; // Just for debugging purposes
                component.Value.SourceObject = this;
                foreach (var animation in component.Value.Animations)
                {
                    animation.Value.Name = animation.Key;
                    animation.Value.AnimatedElement = component.Value;
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
                    _equippedArmor = (Armor) equipmentToEquip;
                    break;
                case "Accessory":
                    equippedEquipment = _equippedAccessory;
                    _equippedAccessory = (Accessory) equipmentToEquip;
                    break;
                default:
                    throw new ArgumentException("Unknown equipment slot: " + equipmentToEquip.Slot);
            }

            // Resources
            Health.Max += equipmentToEquip.Health - equippedEquipment.Health;
            Mana.Max += equipmentToEquip.Capacity - equippedEquipment.Capacity;

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
                {
                    "Head", new Component(
                        Globals.SpriteSheet.GetCopy("NinjaHead"),
                        relativePosition: new Vector3(.5f, .5f, 1.2f),
                        size: new Vector3(1.5f),
                        rotationPoint: new Vector3(.16f, 0, .256f),
                        yOffset: -.05f)
                },
                {
                    "Face", new Component(
                        Globals.SpriteSheet.GetCopy("NormalEyes"),
                        relativePosition: new Vector3(.5f, .52f, .96f),
                        size: new Vector3(1.5f),
                        rotationPoint: new Vector3(.2f, 0, .15f),
                        yOffset: -.1f)
                },
                {
                    "Body", new Component(
                        Globals.SpriteSheet.GetCopy("NinjaBody"),
                        relativePosition: new Vector3(.5f, .5f, .47f),
                        size: new Vector3(.75f),
                        rotationPoint: new Vector3(.08f, 0, .08f))
                },
                {
                    "Arm/Left", new Component(
                        Globals.SpriteSheet.GetCopy("NinjaArm"),
                        relativePosition: new Vector3(.3f, .5f, .45f),
                        size: new Vector3(.375f, .375f, .75f),
                        relativeRotation: new Vector3(0, .4f, 0),
                        rotationPoint: new Vector3(.5f, 0, .8f),
                        yOffset: .001f,
                        animations: new Dictionary<string, Animation>()
                        {
                            {
                                "Walk", new Animation(
                                    updateFrames: new Frames(
                                        baseRotations: new List<Vector3>
                                        {
                                            Vector3.Zero,
                                            new Vector3(.7f, 0, 0),
                                            Vector3.Zero,
                                            new Vector3(-.7f, 0, 0),
                                            Vector3.Zero
                                        },
                                        baseOffsets: new List<Vector3>
                                        {
                                            Vector3.Zero,
                                            new Vector3(.05f, 0, 0),
                                            Vector3.Zero,
                                            new Vector3(-.05f, 0, 0),
                                            Vector3.Zero
                                        },
                                        duration: .4f
                                    )
                                )
                            }
                        })
                },
                {
                    "Arm/Right", new Component(
                        Globals.SpriteSheet.GetCopy("NinjaArm"),
                        relativePosition: new Vector3(.7f, .5f, .45f),
                        size: new Vector3(.375f, .375f, .75f),
                        relativeRotation: new Vector3(0, -.4f, 0),
                        rotationPoint: new Vector3(.5f, 0, .8f),
                        yOffset: .001f,
                        animations: new Dictionary<string, Animation>()
                        {
                            {
                                "Walk", new Animation(
                                    updateFrames: new Frames(
                                        baseRotations: new List<Vector3>
                                        {
                                            Vector3.Zero,
                                            new Vector3(-.7f, 0, 0),
                                            Vector3.Zero,
                                            new Vector3(.7f, 0, 0),
                                            Vector3.Zero
                                        },
                                        baseOffsets: new List<Vector3>
                                        {
                                            Vector3.Zero,
                                            new Vector3(-.05f, 0, 0),
                                            Vector3.Zero,
                                            new Vector3(.05f, 0, 0),
                                            Vector3.Zero
                                        },
                                        duration: .4f
                                    )
                                )
                            }
                        })
                },
                {
                    "Leg/Left", new Component(
                        Globals.SpriteSheet.GetCopy("NinjaLeg"),
                        relativePosition: new Vector3(.375f, .5f, .1f),
                        size: new Vector3(.375f),
                        rotationPoint: new Vector3(.5f, 0, .9f),
                        yOffset: .15f,
                        animations: new Dictionary<string, Animation>()
                        {
                            {
                                "Walk", new Animation(
                                    updateFrames: new Frames(
                                        baseRotations: new List<Vector3>
                                        {
                                            Vector3.Zero,
                                            new Vector3(-.7f, 0, 0),
                                            Vector3.Zero,
                                            new Vector3(.7f, 0, 0),
                                            Vector3.Zero
                                        },
                                        duration: .4f
                                    )
                                )
                            }
                        })
                },
                {
                    "Leg/Right", new Component(
                        Globals.SpriteSheet.GetCopy("NinjaLeg"),
                        relativePosition: new Vector3(.625f, .5f, .1f),
                        size: new Vector3(.375f),
                        rotationPoint: new Vector3(.5f, 0, .9f),
                        yOffset: .15f,
                        animations: new Dictionary<string, Animation>()
                        {
                            {
                                "Walk", new Animation(
                                    updateFrames: new Frames(
                                        baseRotations: new List<Vector3>
                                        {
                                            Vector3.Zero,
                                            new Vector3(.7f, 0, 0),
                                            Vector3.Zero,
                                            new Vector3(-.7f, 0, 0),
                                            Vector3.Zero
                                        },
                                        duration: .4f
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
            if (this != Globals.Player)
            {
                AI?.Value(this);
            }

            ApplyMovement();

            Health.Update();
            Mana.Update();

            UpdateAbilities();

            foreach (var ailment in Ailments) ailment.Update();
            foreach (var component in Components) component.Value.Update();
        }

        public void Cast(AbilityInstance abilityInstance)
        {
            // TODO
            Globals.Log(this + " casts " + abilityInstance.Name + " at " 
                        + abilityInstance.Target.X + ", " + abilityInstance.Target.Y);
            GameControl.CurrentScreen = GameControl.GameScreen.CombatPlayEvents;
        }

        private void UpdateAbilities()
        {
            // TODO
        }

        private void ApplyMovement()
        {
            // TODO
            _isWalking = false;
        }

        // Deal damage to a target
        public void DealDamage(GameObject target, int damage, Type abilityType)
        {
            // TODO: Take damage type into account
            Globals.Log(this + " attacks for " + damage + " " + abilityType + " damage.");
            target.TakeDamage(damage, abilityType);
        }

        public bool IsVisible()
        {
            return Area.IntersectsWith(GameControl.camera.VisibleArea);
        }

        public void Heal(int healing)
        {
            if (healing > 0)
            {
                Globals.Log(this + " heals for " + healing);
                Health.Current += healing;
            }
        }

        public int DamageToTake(int damage, Type abilityType)
        {
            // TODO: Take type into account
            return damage;
        }

        // null isPhysical means "true damage"
        public void TakeDamage(int damage, Type abilityType)
        {
            damage = DamageToTake(damage, abilityType);

            if (damage <= 0) return;

            Health.Current -= damage;
            Globals.Log(this + " takes " + damage + " damage, and is left with " + Health.Current + " HP");
            if (Health.Current <= 0)
            {
                Globals.GameObjectManager.Kill(this);
            }
        }

        // Gets the coordinates at the range specified
        public Vector3 GetTargetCoordinates(float range = 1, float? direction = null)
        {
            var offsets = MathTools.OffsetFromRadians(direction ?? Direction) * range;
            var center = Center;
            return new Vector3(center.X + offsets.X, center.Y + offsets.Y, center.Z);
        }

        // Gets whichever object is exactly [distance] away in the current direction
        public GameObject GetTarget(float range = 1, float? direction = null)
        {
            // See if we would overlap with any other objects
            var targetPosition = GetTargetCoordinates(range, direction);
            return Globals.GameObjectManager.Get((int)targetPosition.X, (int)targetPosition.Y);
        }

        // See if we can move to a location
        public HashSet<GameObject> GetTargets(RectangleF targetArea)
        {
            var targetObjects = Globals.GameObjectManager.Get(targetArea);
            targetObjects.Remove(this);
            return targetObjects;
        }

        // Gets whichever object is [distance] away or closer in the current direction
        public HashSet<GameObject> GetTargetsInRange(int range = 1)
        {
            HashSet<GameObject> returnObjects = new HashSet<GameObject>();
            for (var i = 1; i <= range; i++)
            {
                returnObjects.Add(GetTarget(i));
            }

            return returnObjects;
        }
        
        public GameObject GetFirstTargetInRange(float range = 1)
        {
            for (var i = 1; i <= range; i++)
            {
                var targets = GetTargetsInRange(i);
                if (targets.Count > 0)
                {
                    return targets.OrderBy(target => Vector3.Distance(Center, target.Center)).First();
                }
            }

            return null;
        }

        // Attempts to move the object in a direction (radians).
        public void MoveInDirection(float radians = 0)
        {
            // Update sprite visuals
            IsWalking = false; // TODO
            Direction = radians;

            // Get distance
            var offsets = MathTools.OffsetFromRadians(radians);
            Position = new Vector3(Position.X + offsets.X, Position.Y + offsets.Y, 0);
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

        public GameObject GetClosest(IEnumerable<GameObject> gameObjects)
        {
            // TODO: If this hits performance issues (especially in the case where the list of gameObjects is large) 
            // then I can try branching outward from the gameObject, and if any are in the immediate area then return 
            // the nearest out of those.
            GameObject nearestObject = null;
            float nearestDistance = 10000;
            var center = Center;
            foreach (var gameObject in gameObjects)
            {
                var distance = Vector3.Distance(center, gameObject.Center);
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
