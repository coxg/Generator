using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // Grid logic
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        // Resources
        public Resource Health { get; set; }
        public Resource Stamina { get; set; }
        public Resource Capacity { get; set; }

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

        // Interaction
        public string Disposition { get; set; } // {"Party", "Neutral", "Enemy", "Terrain"}
        public bool Passable { get; set; } // If true, passable to all objects. If false, passable to objects of same disposition.
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
                Capacity.Max += value.Capacity - equippedWeapon.Capacity;

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
                Capacity.Max += value.Capacity - equippedOffHand.Capacity;

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
                Capacity.Max += value.Capacity - equippedArmor.Capacity;

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
                Capacity.Max += value.Capacity - equippedGenerator.Capacity;

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
                Capacity.Max += value.Capacity - equippedAccessory.Capacity;

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
            int width = 1,
            int height = 1,
            int x = 0,
            int y = 0,

            // Resources
            int health = 100,
            int stamina = 0,
            int capacity = 0,

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

            // Interaction
            string disposition = "Terrain", // {"Party", "Neutral", "Enemy", "Terrain"}
            bool passable = false, // If true, passable to all objects. If false, passable to objects of same disposition.

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
            Width = width;
            Height = height;
            X = x;
            Y = y;
            Spawn();

            // Resources
            Health = new Resource("Health", health);
            Stamina = new Resource("Stamina", stamina);
            Capacity = new Resource("Capacity", capacity);

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

            // Interaction
            Disposition = disposition; // {"Party", "Neutral", "Enemy", "Terrain"}
            Passable = passable; // If true, passable to all objects. If false, passable to objects of same disposition.
            Activate = delegate () // What happens when you try to talk to it
            {
                if (Name != null)
                {
                    Say("This is just a " + Name + ".");
                }
            };

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
        }

        public void Update()
        /*
        TODO: Use this for animated sprites.
        */
        {
            
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
            for (int EachX = X; EachX < X + Width; EachX++)
            {
                for (int EachY = Y; EachY > Y - Height; EachY--)
                {
                    Globals.Grid.SetObject(EachX, EachY, this);
                }
            }
        }

        public void Despawn()
        // Removes self from grid. This DOES NOT remove sprite.
        {
            for (int EachX = X; EachX < X + Width; EachX++)
            {
                for (int EachY = Y; EachY > Y - Height; EachY--)
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

        public void Attack(int range = 1)
        // Attack in a given direction
        {

            // TODO: Play attack animation

            // Figure out which one you hit
            // TODO: Different types of attacks
            GameObject target = GetTarget(range);
            if (target == null)
            {
                Globals.Log(this + " attacks and misses.");
            }
            else
            {
                Globals.Log(this + " attacks, hitting " + target + ".");
            }

            // Deal damage
            if (target != null)
            {
                DealDamage(target, EquippedWeapon.Damage + Strength.CurrentValue);
            }
        }

        public GameObject GetTarget(float range = 1.4f)
        // Gets whichever object is [distance] away in the current direction
        {
            Vector2 Offsets = Globals.OffsetFromRadians(Direction);
            GameObject TargettedObject = Globals.Grid.GetObject(
                X + (int)Math.Round(range * Offsets.X), 
                Y + (int)Math.Round(range * Offsets.Y));
            return TargettedObject;
        }

        public GameObject GetTargetInRange(int range = 1)
        // Gets whichever object is [distance] away or closer in the current direction
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

        public bool CanMoveTo(int x, int y)
        // Sees if the GameObject can move to the specified location unimpeded.
        {
            // Loop through each x coordinate you're trying to move to
            for (int MoveToX = x; MoveToX < x + Width; MoveToX++)
            {

                // Loop through each y coordinate you're trying to move to
                for (int MoveToY = y; MoveToY > y - Height; MoveToY--)
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

                    // If any of the locations along the x-axis are blocked
                    for (
                        int MoveThroughX = Math.Min(X, MoveToX) + 1; 
                        MoveThroughX < Math.Min(X, MoveToX); 
                        MoveThroughX++)
                    {
                        // Position is not empty, passable, or friendly
                        if (Globals.Grid.GetObject(MoveThroughX, Y) != null 
                            && !Globals.Grid.GetObject(MoveThroughX, Y).Passable 
                            && !(Globals.Grid.GetObject(MoveThroughX, Y).Disposition == Disposition))
                        {
                            Globals.Log("Location on x-axis blocked");
                            return false;
                        }
                    }

                    // If any of the locations along the y-axis are blocked
                    for (
                        int MoveThroughY = Math.Min(Y, MoveToY) + 1; 
                        MoveThroughY < Math.Max(Y, MoveToY); 
                        MoveThroughY++)
                    {
                        // Position is not empty, passable, or friendly
                        if (Globals.Grid.GetObject(X, MoveThroughY) != null 
                            && !Globals.Grid.GetObject(X, MoveThroughY).Passable 
                            && !(Globals.Grid.GetObject(X, MoveThroughY).Disposition == Disposition))
                        {
                            Globals.Log("Location on y-axis blocked");
                            return false;
                        }
                    }
                }
            }

            // If none of the above return false then it's passable
            return true;
        }

        public void Move(
            float radians = 0,
            float distance = 1.4f
        )
        // Attempts to move the object in a direction (radians).
        {
            // Convert from cardinal direction to X/Y offsets
            Vector2 Offsets = Globals.OffsetFromRadians(radians);
            int NewX = X + (int)Math.Round(distance * Offsets.X);
            int NewY = Y + (int)Math.Round(distance * Offsets.Y);

            // Set direction to the square you're aiming at
            Direction = (float)Math.Atan2(NewX - X, NewY - Y);

            // See if you can move to the location
            Globals.Log(Name + " currently at:      " + X.ToString() + ", " + Y.ToString());
            Globals.Log(Name + " trying to move to: " + NewX + ", " + NewY);
            if (CanMoveTo(NewX, NewY))
            {
                // Null out all previous locations
                Despawn();

                // Assing new attributes for object
                X = (int)Globals.Mod(NewX, Globals.Grid.GetLength(0));
                Y = (int)Globals.Mod(NewY, Globals.Grid.GetLength(1));

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
