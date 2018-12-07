using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class GameObject
        /*
        Represents every object you will see in the game.
        Used as a basis for terrain, playable characters, and enemies.
        */
    {
        // Equipment
        private Accessory _equippedAccessory;
        private Armor _equippedArmor;
        private Generation _equippedGenerator;
        private OffHand _equippedOffHand;
        private Weapon _equippedWeapon;

        // Constructor
        public GameObject(
            
            // Sprite attributes
            string spriteFile = "Ninja",
            string avatarFile = null,
            
            // Actions
            bool isWalking = false,
            bool isSwinging = false,
            bool isShooting = false,
            bool isHurting = false,

            // Grid logic
            float width = 1,
            float length = 1,
            float height = 1,
            float x = 0,
            float y = 0,
            float z = 0,

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

            // Abilities
            List<Ability> abilities = null,

            // Interaction
            int partyNumber = -1,

            // Equipment
            Weapon weapon = null,
            OffHand offHand = null,
            Armor armor = null,
            Generation generator = null,
            Accessory accessory = null
        )
        {
            // Sprites
            ComponentDictionary = new Dictionary<string, Component>()
            {
                {"Head", new Component(
                    spriteFile: spriteFile + "/Head",
                    directional: true,
                    relativePosition: new Vector3(.5f, .506f, .68f),
                    relativeSize: .96f,
                    yOffset: -.05f,
                    sourceObject: this)
                },
                {"Face", new Component(
                    spriteFile: spriteFile + "/Face01",
                    directional: true,
                    relativePosition: new Vector3(.5f, .57f, .555f),
                    relativeSize: .384f,
                    yOffset: -.05f,
                    sourceObject: this)
                },
                {"Body", new Component(
                    spriteFile: spriteFile + "/Body",
                    directional: true,
                    relativePosition: new Vector3(.5f, .505f, .26f),
                    relativeSize: .48f,
                    sourceObject: this)
                },
                {"Left Arm", new Component(
                    spriteFile: spriteFile + "/RightArm",
                    directional: true,
                    relativePosition: new Vector3(-.1f, .504f, .30f),
                    relativeSize: .24f,
                    sourceObject: this)
                },
                {"Right Arm", new Component(
                    spriteFile: spriteFile + "/LeftArm",
                    directional: true,
                    relativePosition: new Vector3(1.1f, .504f, .30f),
                    relativeSize: .24f,
                    sourceObject: this)
                },
                {"Left Hand", new Component(
                    spriteFile: spriteFile + "/Hand",
                    relativePosition: new Vector3(-.2f, .5045f, .21f),
                    relativeSize: .24f,
                    sourceObject: this)
                },
                {"Right Hand", new Component(
                    spriteFile: spriteFile + "/Hand",
                    relativePosition: new Vector3(1.2f, .5045f, .21f),
                    relativeSize: .24f,
                    sourceObject: this)
                },
                {"Left Leg", new Component(
                    spriteFile: spriteFile + "/Leg",
                    relativePosition: new Vector3(.23f, .504f, .07f),
                    relativeSize: .24f,
                    yOffset: .1f,
                    sourceObject: this)
                },
                {"Right Leg", new Component(
                    spriteFile: spriteFile + "/Leg",
                    relativePosition: new Vector3(.77f, .504f, .07f),
                    relativeSize: .24f,
                    yOffset: .1f,
                    sourceObject: this)
                }
            };
            Avatar = avatarFile == null ? null : Globals.Content.Load<Texture2D>(avatarFile);
            
            // Actions
            IsWalking = isWalking;
            IsSwinging = isSwinging;
            IsShooting = isShooting;
            IsHurting = isHurting;

            // Grid logic
            Size = new Vector3(width, length, height);
            Position = new Vector3(x, y, z);
            Spawn();

            // Resources
            Health = new Resource("Health", health);
            Stamina = new Resource("Stamina", stamina, 10);
            Electricity = new Resource("Electricity", electricity);

            // Primary Attributes
            Strength = new Attribute(strength);
            Speed = new Attribute(speed);
            Perception = new Attribute(perception);
            Weight = new Attribute(weight); // Roughly in pounds

            // ...Other Attributes
            Name = name;
            Level = level;
            Experience = experience;
            Direction = direction;

            // Equipment
            _equippedWeapon = weapon ?? new Weapon();
            _equippedOffHand = offHand ?? new OffHand();
            _equippedArmor = armor ?? new Armor();
            _equippedGenerator = generator ?? new Generation();
            _equippedAccessory = accessory ?? new Accessory();
            EquippedWeapon = _equippedWeapon;
            EquippedOffHand = _equippedOffHand;
            EquippedArmor = _equippedArmor;
            EquippedGenerator = _equippedGenerator;
            EquippedAccessory = _equippedAccessory;

            // Abilities
            if (abilities == null)
            {
                abilities = new List<Ability>
                {
                    // TODO: We should create a dictionary for these in Globals
                    new Ability(
                        "Sprint",
                        staminaCost: 1,
                        isChanneled: true,
                        animation: new Animation(
                            updateFrames: new Frames(
                                new List<Vector3>
                                {
                                    new Vector3(0, 0, .2f)
                                },
                                .5f)),
                        start: delegate { 
                            Speed.CurrentValue *= 4;
                            IsWalking = true; 
                        },
                        stop: delegate
                        {
                            Speed.CurrentValue /= 4;
                            IsWalking = false; 
                        }),
                    new Ability(
                        "Attack",
                        staminaCost: EquippedWeapon.Weight + 10,
                        start: delegate
                        {
                            IsSwinging = true;
                            
                            // Figure out which one you hit
                            var target = GetTargetInRange(EquippedWeapon.Range);

                            // Deal damage
                            if (target != null)
                            {
                                Globals.Log(this + " attacks, hitting " + target + ".");
                                DealDamage(target, EquippedWeapon.Damage + Strength.CurrentValue);
                            }
                            else
                            {
                                Globals.Log(this + " attacks and misses.");
                            }
                        }
                    ),
                    new Ability(
                        "Shoot",
                        staminaCost: EquippedWeapon.Weight + 10,
                        start: delegate
                        {
                            IsShooting = true;
                            
                            // Figure out which one you hit
                            var target = GetTargetInRange(EquippedWeapon.Range + 20);

                            // Deal damage
                            if (target != null)
                            {
                                Globals.Log(this + " shoots, hitting " + target + ".");
                                DealDamage(target, EquippedWeapon.Damage + Strength.CurrentValue);
                            }
                            else
                            {
                                Globals.Log(this + " shoots and misses.");
                            }
                        }
                    ),
                    new Ability(
                        "Always Sprint",
                        staminaCost: 1,
                        isToggleable: true,
                        start: delegate { 
                            Speed.CurrentValue *= 4;
                            IsWalking = true; 
                        },
                        stop: delegate
                        {
                            Speed.CurrentValue /= 4;
                            IsWalking = false; 
                        },
                        animation: new Animation(
                            startFrames: new Frames(
                                new List<Vector3>
                                {
                                    new Vector3(0, 0, 1)
                                },
                                1),
                            updateFrames: new Frames(
                                new List<Vector3>
                                {
                                    new Vector3(-.2f, 0, 0),
                                    new Vector3(.2f, 0, 0)
                                },
                                .5f),
                            stopFrames: new Frames(
                                new List<Vector3>
                                {
                                    new Vector3(0, 0, 1)
                                },
                                1.0f)))
                };
                foreach (var ability in abilities) ability.SourceObject = this;
                Abilities = abilities;
                if (Abilities.Count >= 1) Ability1 = Abilities[0];
                if (Abilities.Count >= 2) Ability2 = Abilities[1];
                if (Abilities.Count >= 3) Ability3 = Abilities[2];
                if (Abilities.Count >= 4) Ability4 = Abilities[3];
            }

            // Interaction
            PartyNumber = partyNumber;
            Activate = delegate // What happens when you try to talk to it
            {
                if (Name != null) Say("This is just a " + Name + ".");
            };

            // Make yourself accessible
            Globals.ObjectDict[Name] = this;
        }

        // Sprites
        public Dictionary<string, Component> ComponentDictionary { get; set; }
        public Texture2D Avatar { get; set; }
        
        // Actions
        public bool IsWalking { get; set; }
        public bool IsSwinging { get; set; }
        public bool IsShooting { get; set; }
        public bool IsHurting { get; set; }

        // Location
        public Vector3 Size { get; set; }
        private Vector3 _position { get; set; }
        public Vector3 Position
        {
            get => _position;
            set
            {
                // Null out all previous locations
                Despawn();

                // Assing new attributes for object
                _position = new Vector3(
                    Globals.Mod(value.X, Globals.Grid.GetLength(0)),
                    Globals.Mod(value.Y, Globals.Grid.GetLength(1)),
                    value.Z);

                // Assign all new locations
                Spawn();
            }
        }

        // Resources
        public Resource Health { get; set; }
        public Resource Stamina { get; set; }
        public Resource Electricity { get; set; }

        // Primary Attributes
        public Attribute Strength { get; set; }
        public Attribute Perception { get; set; }
        public Attribute Speed { get; set; }
        public Attribute Weight { get; set; } // Roughly in pounds

        // ...Other Attributes
        public string Name { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public float Direction { get; set; }

        // Abilities
        public List<Ability> Abilities { get; set; }
        public Ability Ability1 { get; set; }
        public Ability Ability2 { get; set; }
        public Ability Ability3 { get; set; }
        public Ability Ability4 { get; set; }

        // Interaction
        public int PartyNumber { get; set; }
        public Action Activate { get; set; }

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

        public void Update()
            // What to do on each frame
        {
            // Update resources
            Health.Update();
            Stamina.Update();
            Electricity.Update();

            // Update animation
            foreach (var component in ComponentDictionary) component.Value.Update();
            IsWalking = false;

            // Use abilities
            foreach (var ability in Abilities) ability.Update();
        }

        public override string ToString()
            // Return name, useful for debugging.
        {
            return Name ?? "Unnamed GameObject";
        }

        public void Spawn()
            // Populates grid with self. This DOES NOT add sprite.
        {
            for (var eachX = (int) Math.Floor(Position.X);
                eachX <= Math.Ceiling(Position.X + Size.X - 1);
                eachX++)
            for (var eachY = (int) Math.Floor(Position.Y);
                eachY <= Math.Ceiling(Position.Y + Size.Y - 1);
                eachY++)
                Globals.Grid.SetObject(eachX, eachY, this);
        }

        public void Despawn()
            // Removes self from grid. This DOES NOT remove sprite.
        {
            for (var eachX = (int) Math.Floor(Position.X);
                eachX <= Math.Ceiling(Position.X + Size.X - 1);
                eachX++)
            for (var eachY = (int) Math.Floor(Position.Y);
                eachY <= Math.Ceiling(Position.Y + Size.Y - 1);
                eachY++)
                Globals.Grid.SetObject(eachX, eachY, null);
        }

        public void Die()
            // Plays death animation and despawns
        {
            // TODO: Play death animation
            Globals.DeathList.Add(Name);

            // Remove self from grid
            Despawn();

            // TODO: Drop equipment + inventory

            Globals.Log(this + " has passed away. RIP.");
        }

        public void DealDamage(GameObject target, int damage)
            // Deal damage to a target
        {
            Globals.Log(this + " attacks for " + damage + " damage.");
            target.TakeDamage(damage);
        }

        public void TakeDamage(int damage)
            // Take damage
        {
            Globals.Log(this + " takes " + damage + " damage. "
                        + Health.Current + " -> " + (Health.Current - damage));

            // Live
            if (Health.Current > damage)
            {
                IsHurting = true;
                Health.Current -= damage;
            }

            // Die
            else
            {
                Health.Current = 0;
                Die();
            }
        }

        public GameObject GetTarget(float range = 1)
            // Gets whichever object is [distance] away in the current direction
        {
            var offsets = Globals.OffsetFromRadians(Direction);
            var targettedObject = Globals.Grid.GetObject(
                (int) Math.Round(Position.X + (range + Size.X / 2) * offsets.X),
                (int) Math.Round(Position.Y + (range + Size.Y / 2) * offsets.Y));
            return targettedObject;
        }

        public GameObject GetTargetInRange(int range = 1)
            // Gets whichever object is [distance] away or closer in the current direction
        {
            GameObject returnObject = null;
            var targetRange = 1;

            // Loop from 1 to [range], seeing if anything is in the way
            while (returnObject == null && targetRange <= range)
            {
                returnObject = GetTarget(targetRange);
                targetRange++;
            }

            return returnObject;
        }

        public bool CanMoveTo(Vector3 position)
            // Sees if the GameObject can move to the specified location unimpeded.
        {
            // Loop through each x coordinate you're trying to move to
            for (
                    var moveToX = (int) Math.Floor(position.X);
                    moveToX < (int) Math.Ceiling(position.X + Size.X);
                    moveToX++)
                
                // Loop through each y coordinate you're trying to move to
                for (
                        var moveToY = (int) Math.Floor(position.Y);
                        moveToY < (int) Math.Ceiling(position.Y + Size.Y);
                        moveToY++)
                    
                    // If location is not empty or self
                    if (Globals.Grid.GetObject(moveToX, moveToY) != null
                        && Globals.Grid.GetObject(moveToX, moveToY) != this)
                    {
                        Globals.Log(
                            "[" + moveToX + ", " + moveToY + "]" +
                            " is not empty or self: " + Globals.Grid.GetObject(moveToX, moveToY));
                        return false;
                    }

            // If none of the above return false then it's passable
            return true;
        }

        public void MoveInDirection(
                float radians = 0,
                float? speed = null
            )
            // Attempts to move the object in a direction (radians).
        {
            // Update sprite visuals
            IsWalking = true;
            Direction = radians;

            // Get distance
            if (speed == null) speed = (float) Math.Sqrt(Speed.CurrentValue);
            var distance = (float) speed / Globals.RefreshRate;

            // Convert from radian direction to X/Y offsets
            var offsets = Globals.OffsetFromRadians(radians);
            var newPosition = new Vector3(
                Position.X + distance * offsets.X,
                Position.Y + distance * offsets.Y,
                Position.Z);

            // See if you can move to the location
            if (CanMoveTo(newPosition)) Position = newPosition;
        }

        public void Say(string message)
            // Submit message to the screen with icon
        {
            Globals.DisplayTextQueue.Enqueue(message);
            Globals.TalkingObjectQueue.Enqueue(this);
        }
    }
}