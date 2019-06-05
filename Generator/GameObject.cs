﻿using System;
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
        private string componentSpriteFileName;

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
        private Vector3 _Position;
        override public Vector3 Position
        {
            get => _Position + AnimationOffset;
            set
            {
                // If we can move there then move there
                var targetAtPosition = GetTargetAtPosition(value);
                if (targetAtPosition == null)
                {
                    _Position = value;
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
                this._abilities = value;
                foreach (var ability in this._abilities) ability.SourceObject = this;

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
        public Action Activate;
        public Action<GameObject> AI;
        public Action<GameObject, GameObject> CollisionEffect;

        // Equipment
        private Weapon _equippedWeapon =  new Weapon();
        public Weapon EquippedWeapon
        {
            get => _equippedWeapon;
            set
            {
                // Resources
                Health.Max += value.Health - _equippedWeapon.Health;
                Stamina.Max += value.Stamina - _equippedWeapon.Stamina;
                Electricity.Max += value.Capacity - _equippedWeapon.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - _equippedWeapon.Strength;
                Perception.CurrentValue += value.Perception - _equippedWeapon.Perception;
                Speed.CurrentValue += value.Speed - _equippedWeapon.Speed;
                _equippedWeapon = value;
            }
        }

        private OffHand _equippedOffHand = new OffHand();
        public OffHand EquippedOffHand
        {
            get => _equippedOffHand;
            set
            {
                // Resources
                Health.Max += value.Health - _equippedOffHand.Health;
                Stamina.Max += value.Stamina - _equippedOffHand.Stamina;
                Electricity.Max += value.Capacity - _equippedOffHand.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - _equippedOffHand.Strength;
                Speed.CurrentValue += value.Speed - _equippedOffHand.Speed;
                Perception.CurrentValue += value.Perception - _equippedOffHand.Perception;
                _equippedOffHand = value;
            }
        }

        private Armor _equippedArmor = new Armor();
        public Armor EquippedArmor
        {
            get => _equippedArmor;
            set
            {
                // Resources
                Health.Max += value.Health - _equippedArmor.Health;
                Stamina.Max += value.Stamina - _equippedArmor.Stamina;
                Electricity.Max += value.Capacity - _equippedArmor.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - _equippedArmor.Strength;
                Speed.CurrentValue += value.Speed - _equippedArmor.Speed;
                Perception.CurrentValue += value.Perception - _equippedArmor.Perception;
                _equippedArmor = value;
            }
        }

        private Generation _equippedGenerator = new Generation();
        public Generation EquippedGenerator
        {
            get => _equippedGenerator;
            set
            {
                // Resources
                Health.Max += value.Health - _equippedGenerator.Health;
                Stamina.Max += value.Stamina - _equippedGenerator.Stamina;
                Electricity.Max += value.Capacity - _equippedGenerator.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - _equippedGenerator.Strength;
                Speed.CurrentValue += value.Speed - _equippedGenerator.Speed;
                Perception.CurrentValue += value.Perception - _equippedGenerator.Perception;
                _equippedGenerator = value;
            }
        }

        private Accessory _equippedAccessory = new Accessory();
        public Accessory EquippedAccessory
        {
            get => _equippedAccessory;
            set
            {
                // Resources
                Health.Max += value.Health - _equippedAccessory.Health;
                Stamina.Max += value.Stamina - _equippedAccessory.Stamina;
                Electricity.Max += value.Capacity - _equippedAccessory.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - _equippedAccessory.Strength;
                Speed.CurrentValue += value.Speed - _equippedAccessory.Speed;
                Perception.CurrentValue += value.Perception - _equippedAccessory.Perception;
                _equippedAccessory = value;
            }
        }

        // Constructor
        public GameObject(

            // Grid logic
            Vector3 position,
            Vector3? size = null,

            // Sprite attributes
            string componentSpriteFileName = "Ninja",
            string spriteFile = null,
            Dictionary<string, Component> components = null,

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

            // Equipment
            Weapon weapon = null,
            OffHand offHand = null,
            Armor armor = null,
            Generation generator = null,
            Accessory accessory = null
        )
        {

            // Sprites
            this.componentSpriteFileName = componentSpriteFileName;
            this.Components = components ?? GenerateDefaultComponentDict();
            LinkComponents();
            this.Sprite = spriteFile == null ? null : Globals.Content.Load<Texture2D>(spriteFile);

            // Actions
            this.IsWalking = isWalking;
            this.IsSwinging = isSwinging;
            this.IsShooting = isShooting;
            this.IsHurting = isHurting;

            // Resources
            this.Health = new Resource("Health", health);
            this.Stamina = new Resource("Stamina", stamina, 10);
            this.Electricity = new Resource("Electricity", electricity);

            // Primary Attributes
            this.Strength = new Attribute(strength);
            this.Speed = new Attribute(speed);
            this.Perception = new Attribute(perception);
            this.Weight = new Attribute(weight); // TODO: Use this in knockback calculation

            // ...Other Attributes
            this.Name = name;
            this.Level = level;
            this.Experience = experience;
            this.Direction = direction;
            this.Brightness = brightness ?? Vector3.Zero;

            // Equipment
            this.EquippedWeapon = weapon ?? new Weapon();
            this.EquippedOffHand = offHand ?? new OffHand();
            this.EquippedArmor = armor ?? new Armor();
            this.EquippedGenerator = generator ?? new Generation();
            this.EquippedAccessory = accessory ?? new Accessory();

            // Abilities
            this.Abilities = abilities ?? DefaultAbilities.GenerateDefaultAbilities(this);

            // Interaction
            this.PartyNumber = partyNumber;
            this.Activate = delegate // What happens when you try to talk to it
            {
                if (this.Name != null) Say("This is just a " + this.Name + ".");
            };
            this.AI = ai; // Run on each Update - argument is this
            this.CollisionEffect = collisionEffect; // Run when attempting to move into another object - arguments are this, other

            // Grid logic
            this.Size = size ?? Vector3.One;
            this._Position = position;
            Area = new RectangleF(Position.X, Position.Y, Size.X, Size.Y);
            Globals.Log(Name + " has spawned.");
        }

        // Establish a two-way link between the components and this GameObject
        private void LinkComponents()
        {
            foreach (var component in this.Components)
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

        // Create the default ComponentDictionary
        private Dictionary<string, Component> GenerateDefaultComponentDict()
        {
            Dictionary<string, Component> result = new Dictionary<string, Component>()
            {
                {"Head", new Component(
                    spriteFile: this.componentSpriteFileName + "/Head",
                    directional: true,
                    relativePosition: new Vector3(.5f, .506f, 1.36f),
                    relativeSize: .32f,
                    rotationPoint: new Vector3(.16f, 0, .256f),
                    yOffset: -.05f)
                },
                {"Face", new Component(
                    spriteFile: this.componentSpriteFileName + "/Face01",
                    directional: true,
                    relativePosition: new Vector3(.5f, .57f, 1.11f),
                    relativeSize: .128f,
                    rotationPoint: new Vector3(.08f, 0, .066f),
                    yOffset: -.05f)
                },
                {"Body", new Component(
                    spriteFile: this.componentSpriteFileName + "/Body",
                    directional: true,
                    relativePosition: new Vector3(.5f, .505f, .52f),
                    relativeSize: .16f,
                    rotationPoint: new Vector3(.08f, 0, .08f))
                },
                {"Left Arm", new Component(
                    spriteFile: this.componentSpriteFileName + "/RightArm",
                    directional: true,
                    relativePosition: new Vector3(-.1f, .504f, .6f),
                    relativeSize: .08f,
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
                    spriteFile: this.componentSpriteFileName + "/LeftArm",
                    directional: true,
                    relativePosition: new Vector3(1.1f, .504f, .6f),
                    relativeSize: .08f,
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
                    spriteFile: this.componentSpriteFileName + "/Hand",
                    relativePosition: new Vector3(-.2f, .5045f, .42f),
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
                    spriteFile: this.componentSpriteFileName + "/Hand",
                    relativePosition: new Vector3(1.2f, .5045f, .42f),
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
                    spriteFile: this.componentSpriteFileName + "/Leg",
                    relativePosition: new Vector3(.23f, .504f, .14f),
                    relativeSize: .08f,
                    rotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .1f,
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
                    spriteFile: this.componentSpriteFileName + "/Leg",
                    relativePosition: new Vector3(.77f, .504f, .14f),
                    relativeSize: .08f,
                    rotationPoint: new Vector3(.04f, 0, .018f),
                    yOffset: .1f,
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

        // Plays death animation and despawns
        public void Die()
        {
            Globals.GameObjects.RemoveObject(Name);

            // TODO: Drop equipment + inventory
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

                this.IsHurting = true;
                // TODO: We can't hardcode "Face"
                // this.Components["Face"].SpriteFile = this.componentSpriteFileName + "/Face04";
                this.Health.Current -= damage;

                if (this.Health.Current <= 0)
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

        // Gets whether this object is visible
        public bool IsVisible()
        {
            return GameControl.camera.VisibleArea.IntersectsWith(Area);
        }

        // Gets whether this object is within the logically updating range
        public bool IsUpdating()
        {
            return GameControl.camera.UpdatingArea.IntersectsWith(Area);
        }

        // Gets whichever object is exactly [distance] away in the current direction
        public GameObject GetTargetAtRange(float range = 1, float? direction = null)
        {
            // See if we would overlap with any other objects
            var target = GetTargetCoordinates(range, direction);
            foreach (var gameObject in Globals.GameObjects.ObjectFromName.Values)
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
            foreach (var gameObject in Globals.GameObjects.ObjectFromName.Values)
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
                float? speed = null
            )
        {
            // Update sprite visuals
            IsWalking = true;
            Direction = radians;

            // Get distance
            if (speed == null) speed = (float) Math.Sqrt(Speed.CurrentValue);
            var distance = (float) speed / Globals.RefreshRate;

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