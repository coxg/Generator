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

            // Movement logic
            Vector3? movementPosition = null,
            float? movementDirection = null,
            float baseMovementSpeed = 20,
            float? movementSpeedMultiplier = null,
            Vector3? movementVelocity = null,

            // Physics
            float mass = 100,
            Vector3? velocity = null,
            Dictionary<String, PhysicsEffect> physicsEffects = null,

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
            Level = level;
            Experience = experience;
            BaseMovementSpeed = baseMovementSpeed;
            MovementSpeedMultiplier = movementSpeedMultiplier;

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
            CollisionEffect =
                collisionEffect; // Run when attempting to move into another object - arguments are this, other
            Temporary = temporary; // If true, destroy this object as soon as it's no longer being updated

            // Physics logic
            Size = size ?? Vector3.One;
            base.Position = position;
            Direction = direction;
            MovementTarget = movementPosition;
            MovementDirection = movementDirection;
            MovementVelocity = movementVelocity ?? Vector3.Zero;
            Mass = mass;
            Velocity = velocity ?? Vector3.Zero;
            PhysicsEffects = physicsEffects ?? new Dictionary<string, PhysicsEffect>();

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
        public Vector3? MovementTarget;
        public Dictionary<String, PhysicsEffect> PhysicsEffects;
        public float? MovementDirection;
        public Vector3 Velocity;

        public float BaseMovementSpeed;
        public float? MovementSpeedMultiplier; // 1 = running, .5 = walking, etc
        public Vector3 MovementVelocity;
        public float Mass;

        override public Vector3 Position
        {
            get => _Position + AnimationOffset;
            set
            {
                if (value.Z < 0)
                {
                    value.Z = 0;
                    Velocity.Z = 0;
                }

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
                        Globals.Warn((Name ?? ID) + " can't move to " + value);
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

                // If not then we collide with the object and it collides with us
                // TODO: In the current implementation this will trigger twice per update if both are moving
                else
                {
                    CollisionEffect?.Value(this, targetAtPosition);
                    targetAtPosition.CollisionEffect?.Value(targetAtPosition, this);
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

        // Growth
        // TODO: these should probably have getters and setters and logic and whatnot
        public int Level;
        public int Experience;

        // Abilities
        // TODO: Function to add new ability, should not be able to add it directly
        // This would make a copy and set the sourceObject
        public List<Ability> Abilities = new List<Ability>();
        public Dictionary<String, float> AbilityCooldowns = new Dictionary<String, float>();
        public AbilityInstance CastingAbility;
        public AbilityInstance RechargingAbility;
        private Queue<AbilityInstance> queuedAbilities = new Queue<AbilityInstance>();
        public bool IsReady => !queuedAbilities.Any();

        // Interaction
        public Conversation Conversation;
        public Cached<Action<GameObject, GameObject>> ActivationEffect;
        public Cached<Action<GameObject>> AI;
        public Cached<Action<GameObject, GameObject>> CollisionEffect;
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
                        sprite: Globals.SpriteSheet.GetCopy("NinjaHead"),
                        relativePosition: new Vector3(.5f, .5f, 1.2f),
                        size: new Vector3(1.5f),
                        rotationPoint: new Vector3(.16f, 0, .256f),
                        yOffset: -.05f)
                },
                {
                    "Face", new Component(
                        sprite: Globals.SpriteSheet.GetCopy("NormalEyes"),
                        relativePosition: new Vector3(.5f, .52f, .96f),
                        size: new Vector3(1.5f),
                        rotationPoint: new Vector3(.2f, 0, .15f),
                        yOffset: -.1f)
                },
                {
                    "Body", new Component(
                        sprite: Globals.SpriteSheet.GetCopy("NinjaBody"),
                        relativePosition: new Vector3(.5f, .5f, .47f),
                        size: new Vector3(.75f),
                        rotationPoint: new Vector3(.08f, 0, .08f))
                },
                {
                    "Arm/Left", new Component(
                        sprite: Globals.SpriteSheet.GetCopy("NinjaArm"),
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
                        sprite: Globals.SpriteSheet.GetCopy("NinjaArm"),
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
                        sprite: Globals.SpriteSheet.GetCopy("NinjaLeg"),
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
                        sprite: Globals.SpriteSheet.GetCopy("NinjaLeg"),
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
            ApplyPhysics();

            Health.Update();
            Mana.Update();

            UpdateAbilities();

            foreach (var ailment in Ailments) ailment.Update();
            foreach (var component in Components) component.Value.Update();
        }

        public void Cast(AbilityInstance abilityInstance)
        {
            Globals.Log(this + " is preparing to cast " + abilityInstance.Name + " at " 
                        + abilityInstance.Target.X + ", " + abilityInstance.Target.Y);
            queuedAbilities.Enqueue(abilityInstance);
            GameControl.CurrentScreen = GameControl.GameScreen.CombatPlayEvents;
        }

        private void UpdateAbilities()
        {
            foreach (var abilityName in new List<string>(AbilityCooldowns.Keys))
            {
                AbilityCooldowns[abilityName] = Math.Min(0, AbilityCooldowns[abilityName] - Timing.SecondsPassed);
            }

            if (RechargingAbility != null)
            {
                // TODO: Play recharging animation
                RechargingAbility.RemainingRecharge = Math.Min(0, RechargingAbility.RemainingRecharge - Timing.SecondsPassed);
                if (RechargingAbility.RemainingRecharge == 0)
                {
                    RechargingAbility = null;
                }
            }
            
            if (CastingAbility != null)
            {
                CastingAbility.Ability.Animation?.Update();
                CastingAbility.RemainingCastTime = Math.Min(0, CastingAbility.RemainingCastTime - Timing.SecondsPassed);
                if (CastingAbility.RemainingCastTime == 0)
                {
                    CastingAbility.FinishCasting();
                    if (CastingAbility.RemainingRecharge != 0)
                    {
                        RechargingAbility = CastingAbility;
                    }
                    CastingAbility = null;
                }

                if (CastingAbility == null && RechargingAbility == null)
                {
                    CastingAbility = queuedAbilities.Dequeue();
                    CastingAbility.StartCasting();
                }
            }
        }

        private void ApplyMovement()
        {
            // Try to slow down when close to target
            if (MovementTarget != null)
            {
                var distanceToMove = Vector3.Distance(MovementTarget.Value, new Vector3(Position.X, Position.Y, 0));
                if (distanceToMove < GetMovementDistance() * Timing.SecondsPassed)
                {
                    IsWalking = true;
                    MovementTarget = null;
                    MovementDirection = null;
                    return;
                }
                if (Position.Z == 0)
                {
                    MovementSpeedMultiplier = Math.Min(distanceToMove / 3, 1);
                }
            }

            // In the air
            if (Position.Z > 0)
            {
                return;
            }
            
            // Click to target place to move
            if (MovementTarget != null)
            {
                MoveInDirection((float) MathTools.Angle(Position, MovementTarget.Value));
            }

            // Controller
            else if (MovementDirection != null)
            {
                MovementSpeedMultiplier = MovementSpeedMultiplier ?? 1;
                MoveInDirection(MovementDirection.Value);
            }
            
            // No input
            else
            {
                MovementVelocity = Vector3.Zero;
                MovementSpeedMultiplier = null;
                IsWalking = false;
            }
        }

        private void ApplyPhysics()
        {
            var forces = Vector3.Zero;
            foreach (var physicsEffects in PhysicsEffects.Values)
            {
                forces += physicsEffects.Force;
                physicsEffects.Update(); // This order to let duration 0 still apply force
            }
            forces.Z -= Globals.Zone.Gravity * Mass / 10;  // 10 because gravity and squares are in meters
            forces += Globals.Zone.Wind;
            
            Velocity += forces * Timing.SecondsPassed;
            if (Position.Z == 0)
            {
                var friction = GetFriction() * 300 * Timing.SecondsPassed;  // why 300?
                Velocity.X = Velocity.X > 0 ? Math.Max(Velocity.X - friction, 0) : Math.Min(Velocity.X + friction, 0);
                Velocity.Y = Velocity.Y > 0 ? Math.Max(Velocity.Y - friction, 0) : Math.Min(Velocity.Y + friction, 0);
                if (MovementVelocity != Vector3.Zero)
                {
                    MovementVelocity += Globals.Zone.Wind;
                }
            }
            
            Position += (Velocity + MovementVelocity) * Timing.SecondsPassed;
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

            if (damage > 0)
            {
                Globals.Log(this + " takes " + damage + " damage. "
                            + Health.Current + " -> " + (Health.Current - damage));

                // TODO: Taking damage animation
                Health.Current -= damage;

                if (Health.Current <= 0)
                {
                    Globals.GameObjectManager.Kill(this);
                }
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
        public HashSet<GameObject> GetTargets(float range = 1, float? direction = null)
        {
            // See if we would overlap with any other objects
            var targetPosition = GetTargetCoordinates(range, direction);
            var targetObjects = Globals.GameObjectManager.Get(targetPosition.X, targetPosition.Y);
            return targetObjects.Where(x => x != this).ToHashSet();
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
                returnObjects.UnionWith(GetTargets(i));
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
                    return targets.OrderBy(target => (Center - target.Center).Length()).First();
                }
            }

            return null;
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
            var friction = GetFriction();
            MovementVelocity.X += (distance * offsets.X - MovementVelocity.X) * friction;
            MovementVelocity.Y += (distance * offsets.Y - MovementVelocity.Y) * friction;
        }

        private float GetMovementDistance()
        {
            return (MovementSpeedMultiplier ?? 0) * BaseMovementSpeed;  // per second
        }

    private float GetFriction()
        {
            return Globals.TileManager.Get((int) Center.X, (int) Center.Y)?.Friction ?? 0;
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
