﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Generator
{
    public class GameObject
    /*
    Represents every object you will see in the game.
    Used as a basis for terrain, playable characters, and enemies.
    */
    {

        // Sprites
        // TODO: Make sure this can be an AnimatedSprite
        public Texture2D StandingSprite { get; set; }
        public Texture2D MovingSprite { get; set; }
        public Texture2D AimingSprite { get; set; }
        public Texture2D DeadSprite { get; set; }
        public Texture2D Avatar { get; set; }

        // Location
        public Vector3 Dimensions { get; set; }
        public Vector3 Position { get; set; }

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
        public bool InParty { get; set; }
        public Action Activate { get; set; }

        // Weapon
        private Weapon equippedWeapon;
        public Weapon EquippedWeapon
        {
            get
            {
                return equippedWeapon;
            }
            set
            {
                // Resources
                Health.Max += value.Health - equippedWeapon.Health;
                Stamina.Max += value.Stamina - equippedWeapon.Stamina;
                Electricity.Max += value.Capacity - equippedWeapon.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - equippedWeapon.Strength;
                Perception.CurrentValue += value.Perception - equippedWeapon.Perception;
                Speed.CurrentValue += value.Speed - equippedWeapon.Speed;
                equippedWeapon = value;
            }
        }

        // OffHand
        private OffHand equippedOffHand;
        public OffHand EquippedOffHand
        {
            get
            {
                return equippedOffHand;
            }
            set
            {
                // Resources
                Health.Max += value.Health - equippedOffHand.Health;
                Stamina.Max += value.Stamina - equippedOffHand.Stamina;
                Electricity.Max += value.Capacity - equippedOffHand.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - equippedOffHand.Strength;
                Speed.CurrentValue += value.Speed - equippedOffHand.Speed;
                Perception.CurrentValue += value.Perception - equippedOffHand.Perception;
                equippedOffHand = value;
            }
        }

        // Armor
        private Armor equippedArmor;
        public Armor EquippedArmor
        {
            get
            {
                return equippedArmor;
            }
            set
            {
                // Resources
                Health.Max += value.Health - equippedArmor.Health;
                Stamina.Max += value.Stamina - equippedArmor.Stamina;
                Electricity.Max += value.Capacity - equippedArmor.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - equippedArmor.Strength;
                Speed.CurrentValue += value.Speed - equippedArmor.Speed;
                Perception.CurrentValue += value.Perception - equippedArmor.Perception;
                equippedArmor = value;
            }
        }

        // Generation
        private Generation equippedGenerator;
        public Generation EquippedGenerator
        {
            get
            {
                return equippedGenerator;
            }
            set
            {
                // Resources
                Health.Max += value.Health - equippedGenerator.Health;
                Stamina.Max += value.Stamina - equippedGenerator.Stamina;
                Electricity.Max += value.Capacity - equippedGenerator.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - equippedGenerator.Strength;
                Speed.CurrentValue += value.Speed - equippedGenerator.Speed;
                Perception.CurrentValue += value.Perception - equippedGenerator.Perception;
                equippedGenerator = value;
            }
        }

        // Accessory
        private Accessory equippedAccessory;
        public Accessory EquippedAccessory
        {
            get
            {
                return equippedAccessory;
            }
            set
            {
                // Resources
                Health.Max += value.Health - equippedAccessory.Health;
                Stamina.Max += value.Stamina - equippedAccessory.Stamina;
                Electricity.Max += value.Capacity - equippedAccessory.Capacity;

                // Attributes
                Strength.CurrentValue += value.Strength - equippedAccessory.Strength;
                Speed.CurrentValue += value.Speed - equippedAccessory.Speed;
                Perception.CurrentValue += value.Perception - equippedAccessory.Perception;
                equippedAccessory = value;
            }
        }

        // Constructor
        public GameObject(

            // Sprites
            string standingSpriteFile = "Sprites/face",
            string avatarFile = null,

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
            int intellect = 0,
            int speed = 0,
            int perception = 0,
            int weight = 150, // Roughly in pounds

            // ...Other Attributes
            string name = null,
            int level = 1,
            int experience = 0,
            float direction = 0f,

            // Abilities
            List<Ability> abilities = null,

            // Interaction
            bool inParty = false,

            // Equipment
            Weapon weapon = null,
            OffHand offHand = null,
            Armor armor = null,
            Generation generator = null,
            Accessory accessory = null

            )
        {
            // Sprites
            StandingSprite = Globals.Content.Load<Texture2D>(standingSpriteFile);
            if (avatarFile != null)
            {
                Avatar = Globals.Content.Load<Texture2D>(avatarFile);
            }
            else
            {
                Avatar = null;
            }

            // Grid logic
            Dimensions = new Vector3(width, length, height);
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
            equippedWeapon = weapon ?? new Weapon();
            equippedOffHand = offHand ?? new OffHand();
            equippedArmor = armor ?? new Armor();
            equippedGenerator = generator ?? new Generation();
            equippedAccessory = accessory ?? new Accessory();
            EquippedWeapon = equippedWeapon;
            EquippedOffHand = equippedOffHand;
            EquippedArmor = equippedArmor;
            EquippedGenerator = equippedGenerator;
            EquippedAccessory = equippedAccessory;

            // Abilities
            if (abilities == null)
            {
                abilities = new List<Ability>
                {
                    new Ability(
                        name: "Sprint",
                        sourceObject: this,
                        staminaCost: 1,
                        isChanneled: true,
                        start: delegate ()
                        {
                            Speed.CurrentValue *= 4;
                        },
                        stop: delegate ()
                        {
                            Speed.CurrentValue /= 4;
                        }),
                    new Ability(
                        name: "Attack",
                        sourceObject: this,
                        staminaCost: EquippedWeapon.Weight + 10,
                        animation: delegate ()
                        {
                            // TODO: This
                        },
                        start: delegate ()
                        {

                            // Figure out which one you hit
                            GameObject target = GetTarget(EquippedWeapon.Range);

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
                        name: "Always Sprint",
                        sourceObject: this,
                        staminaCost: 1,
                        isToggleable: true,
                        start: delegate ()
                        {
                            Speed.CurrentValue *= 4;
                        },
                        stop: delegate ()
                        {
                            Speed.CurrentValue /= 4;
                        }),
                };
                Abilities = abilities;
                if (Abilities.Count >= 1)
                {
                    Ability1 = Abilities[0];
                }
                if (Abilities.Count >= 2)
                {
                    Ability2 = Abilities[1];
                }
                if (Abilities.Count >= 3)
                {
                    Ability3 = Abilities[2];
                }
                if (Abilities.Count >= 4)
                {
                    Ability4 = Abilities[3];
                }
            }

            // Interaction
            InParty = inParty;
            Activate = delegate () // What happens when you try to talk to it
            {
                if (Name != null)
                {
                    Say("This is just a " + Name + ".");
                }
            };

            // Make yourself accessible
            Globals.ObjectDict[Name] = this;
        }

        public void Update()
        // What to do on each frame
        {
            // Update resources
            Health.Update();
            Stamina.Update();
            Electricity.Update();

            // Use abilities
            foreach (Ability ability in Abilities)
            {
                ability.Update();
            }
        }

        public override string ToString()
        // Return name, useful for debugging.
        {
            if(Name == null)
            {
                return "Unnamed GameObject";
            }
            else
            {
                return Name;
            }
        }

        public void Spawn()
        // Populates grid with self. This DOES NOT add sprite.
        {
            for (int EachX = (int)Math.Floor(Position.X); EachX <= Math.Ceiling(Position.X + Dimensions.X - 1); EachX++)
            {
                for (int EachY = (int)Math.Floor(Position.Y); EachY <= Math.Ceiling(Position.Y + Dimensions.Y - 1); EachY++)
                {
                    Globals.Grid.SetObject(EachX, EachY, this);
                }
            }
        }

        public void Despawn()
        // Removes self from grid. This DOES NOT remove sprite.
        {
            for (int EachX = (int)Math.Floor(Position.X); EachX <= Math.Ceiling(Position.X + Dimensions.X - 1); EachX++)
            {
                for (int EachY = (int)Math.Floor(Position.Y); EachY <= Math.Ceiling(Position.Y + Dimensions.Y - 1); EachY++)
                {
                    Globals.Grid.SetObject(EachX, EachY, null);
                }
            }
        }

        public void Die()
        // Plays death animation and despawns
        {
            // TODO: Play death animation

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
            Vector2 Offsets = Globals.OffsetFromRadians(Direction);
            GameObject TargettedObject = Globals.Grid.GetObject(
                (int)Math.Round(Position.X + (range + Dimensions.X / 2) * Offsets.X),
                (int)Math.Round(Position.Y + (range + Dimensions.Y / 2) * Offsets.Y));
            return TargettedObject;
        }

        public GameObject GetTargetInRange(int range = 1)
        // Gets whichever object is [distance] away or closer in the current direction
        // TODO: Fix this. I'm pretty sure it crashes the game.
        {
            GameObject ReturnObject = null;
            int TargetRange = 1;

            // Loop from 1 to [range], seeing if anything is in the way
            while (ReturnObject == null && TargetRange <= range)
            {
                ReturnObject = GetTarget(TargetRange);
                range++;
            }
            return ReturnObject;
        }

        public bool CanMoveTo(Vector3 position)
        // Sees if the GameObject can move to the specified location unimpeded.
        {
            // Loop through each x coordinate you're trying to move to
            for (int MoveToX = (int)Math.Floor(position.X); MoveToX < (int)Math.Ceiling(position.X + Dimensions.X); MoveToX++)
            {

                // Loop through each y coordinate you're trying to move to
                for (int MoveToY = (int)Math.Floor(position.Y); MoveToY < (int)Math.Ceiling(position.Y + Dimensions.Y); MoveToY++)
                {

                    // If location is not empty or self
                    if (Globals.Grid.GetObject(MoveToX, MoveToY) != null 
                        && Globals.Grid.GetObject(MoveToX, MoveToY) != this)
                    {
                        Globals.Log(
                            "[" + MoveToX.ToString() + ", " + MoveToY.ToString() + "]" +
                            " is not empty or self: " + Globals.Grid.GetObject(MoveToX, MoveToY).ToString());
                        return false;
                    }
                }
            }

            // If none of the above return false then it's passable
            return true;
        }

        public void Move(
            float radians = 0,
            float? speed = null
        )
        // Attempts to move the object in a direction (radians).
        {

            // Get distance
            if (speed == null)
            {
                speed = (float)Math.Sqrt(Speed.CurrentValue);
            }
            float distance = (float)speed / Globals.RefreshRate;

            // Convert from radian direction to X/Y offsets
            Vector2 Offsets = Globals.OffsetFromRadians(radians);
            Vector3 NewPosition = new Vector3(
                Position.X + distance * Offsets.X,
                Position.Y + distance * Offsets.Y,
                Position.Z);

            // Set direction to the square you're aiming at
            Direction = (float)Math.Atan2(NewPosition.X - Position.X, NewPosition.Y - Position.Y);

            // See if you can move to the location
            Globals.Log(Name + " currently at:      " + Position.X.ToString() 
                + ", " + Position.Y.ToString());
            Globals.Log(Name + " trying to move to: " + NewPosition.X + ", " + NewPosition.Y);
            if (CanMoveTo(NewPosition))
            {
                // Null out all previous locations
                Despawn();

                // Assing new attributes for object
                Position = new Vector3(
                    Globals.Mod(NewPosition.X, Globals.Grid.GetLength(0)),
                    Globals.Mod(NewPosition.Y, Globals.Grid.GetLength(1)),
                    Position.Z);

                // Assign all new locations
                Spawn();
            }
        }

        public void Say(string Message)
        // Submit message to the screen with icon
        {
            Globals.DisplayTextQueue.Enqueue(Message);
            Globals.TalkingObjectQueue.Enqueue(this);
        }
    }
}
