﻿using System;
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
        // Accessory
        private Accessory equippedAccessory;

        // Armor
        private Armor equippedArmor;

        // Generation
        private Generation equippedGenerator;

        // OffHand
        private OffHand equippedOffHand;

        // Weapon
        private Weapon equippedWeapon;

        // Constructor
        public GameObject(
            // Sprites
            string spriteFile = "Sprites/face",
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
            Sprite = Globals.Content.Load<Texture2D>(spriteFile);
            if (avatarFile != null)
                Avatar = Globals.Content.Load<Texture2D>(avatarFile);
            else
                Avatar = null;

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
                        start: delegate { Speed.CurrentValue *= 4; },
                        stop: delegate { Speed.CurrentValue /= 4; }),
                    new Ability(
                        "Attack",
                        staminaCost: EquippedWeapon.Weight + 10,
                        start: delegate
                        {
                            // Figure out which one you hit
                            var target = GetTarget(EquippedWeapon.Range);

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
                        "Always Sprint",
                        staminaCost: 1,
                        isToggleable: true,
                        start: delegate { Speed.CurrentValue *= 4; },
                        stop: delegate { Speed.CurrentValue /= 4; },
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
        // TODO: Make sure this can be an AnimatedSprite
        public Texture2D Sprite { get; set; }
        public Texture2D Avatar { get; set; }

        // Location
        public Vector3 Dimensions { get; set; }
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
            get => equippedWeapon;
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

        public OffHand EquippedOffHand
        {
            get => equippedOffHand;
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

        public Armor EquippedArmor
        {
            get => equippedArmor;
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

        public Generation EquippedGenerator
        {
            get => equippedGenerator;
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

        public Accessory EquippedAccessory
        {
            get => equippedAccessory;
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

        public void Update()
            // What to do on each frame
        {
            // Update resources
            Health.Update();
            Stamina.Update();
            Electricity.Update();

            // Use abilities
            foreach (var ability in Abilities) ability.Update();
        }

        public override string ToString()
            // Return name, useful for debugging.
        {
            if (Name == null)
                return "Unnamed GameObject";
            return Name;
        }

        public void Spawn()
            // Populates grid with self. This DOES NOT add sprite.
        {
            for (var EachX = (int) Math.Floor(Position.X);
                EachX <= Math.Ceiling(Position.X + Dimensions.X - 1);
                EachX++)
            for (var EachY = (int) Math.Floor(Position.Y);
                EachY <= Math.Ceiling(Position.Y + Dimensions.Y - 1);
                EachY++)
                Globals.Grid.SetObject(EachX, EachY, this);
        }

        public void Despawn()
            // Removes self from grid. This DOES NOT remove sprite.
        {
            for (var EachX = (int) Math.Floor(Position.X);
                EachX <= Math.Ceiling(Position.X + Dimensions.X - 1);
                EachX++)
            for (var EachY = (int) Math.Floor(Position.Y);
                EachY <= Math.Ceiling(Position.Y + Dimensions.Y - 1);
                EachY++)
                Globals.Grid.SetObject(EachX, EachY, null);
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
            var Offsets = Globals.OffsetFromRadians(Direction);
            var TargettedObject = Globals.Grid.GetObject(
                (int) Math.Round(Position.X + (range + Dimensions.X / 2) * Offsets.X),
                (int) Math.Round(Position.Y + (range + Dimensions.Y / 2) * Offsets.Y));
            return TargettedObject;
        }

        public GameObject GetTargetInRange(int range = 1)
            // Gets whichever object is [distance] away or closer in the current direction
            // TODO: Fix this. I'm pretty sure it crashes the game.
        {
            GameObject ReturnObject = null;
            var TargetRange = 1;

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
            for (var MoveToX = (int) Math.Floor(position.X);
                    MoveToX < (int) Math.Ceiling(position.X + Dimensions.X);
                    MoveToX++)
                // Loop through each y coordinate you're trying to move to
            for (var MoveToY = (int) Math.Floor(position.Y);
                    MoveToY < (int) Math.Ceiling(position.Y + Dimensions.Y);
                    MoveToY++)
                // If location is not empty or self
                if (Globals.Grid.GetObject(MoveToX, MoveToY) != null
                    && Globals.Grid.GetObject(MoveToX, MoveToY) != this)
                {
                    Globals.Log(
                        "[" + MoveToX + ", " + MoveToY + "]" +
                        " is not empty or self: " + Globals.Grid.GetObject(MoveToX, MoveToY));
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
            // Get distance
            if (speed == null) speed = (float) Math.Sqrt(Speed.CurrentValue);
            var distance = (float) speed / Globals.RefreshRate;

            // Convert from radian direction to X/Y offsets
            var Offsets = Globals.OffsetFromRadians(radians);
            var NewPosition = new Vector3(
                Position.X + distance * Offsets.X,
                Position.Y + distance * Offsets.Y,
                Position.Z);

            // Set direction to the square you're aiming at
            Direction = (float) Math.Atan2(NewPosition.X - Position.X, NewPosition.Y - Position.Y);

            // See if you can move to the location
            if (CanMoveTo(NewPosition)) Position = NewPosition;
        }

        public void Say(string Message)
            // Submit message to the screen with icon
        {
            Globals.DisplayTextQueue.Enqueue(Message);
            Globals.TalkingObjectQueue.Enqueue(this);
        }
    }
}