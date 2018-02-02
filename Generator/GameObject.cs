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
        public Texture2D DyingSprite { get; set; }

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
        public Attribute Intellect { get; set; }
        public Attribute Speed { get; set; }
        public Attribute Perception { get; set; }
        public Attribute Weight { get; set; } // Roughly in pounds

        // ...Other Attributes
        public string Name { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public float Direction { get; set; }
        public string Disposition { get; set; } // {"Party", "Neutral", "Enemy", "Terrain"}
        public bool Passable { get; set; } // If true, passable to all objects. If false, passable to objects of same disposition.

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
                Intellect.CurrentValue += value.Intellect - equippedWeapon.Intellect;
                Speed.CurrentValue += value.Speed - equippedWeapon.Speed;
                Perception.CurrentValue += value.Perception - equippedWeapon.Perception;
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
                Intellect.CurrentValue += value.Intellect - equippedOffHand.Intellect;
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
                Intellect.CurrentValue += value.Intellect - equippedArmor.Intellect;
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
                Intellect.CurrentValue += value.Intellect - equippedGenerator.Intellect;
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
                Intellect.CurrentValue += value.Intellect - equippedAccessory.Intellect;
                Speed.CurrentValue += value.Speed - equippedAccessory.Speed;
                Perception.CurrentValue += value.Perception - equippedAccessory.Perception;
                equippedAccessory = value;
            }
        }

        // Constructor
        public GameObject(

            // Sprites
            string standingSpriteFile = "Sprites/face",
            string movingSpriteFile = "Sprites/face",
            string aimingSpriteFile = "Sprites/face",
            string deadSpriteFile = "Sprites/face",
            string dyingSpriteFile = "Sprites/face",

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
            string name = "",
            int level = 1,
            int experience = 0,
            float direction = 0f,
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
            MovingSprite = Globals.Content.Load<Texture2D>(movingSpriteFile);
            AimingSprite = Globals.Content.Load<Texture2D>(aimingSpriteFile);
            DeadSprite = Globals.Content.Load<Texture2D>(deadSpriteFile);
            DyingSprite = Globals.Content.Load<Texture2D>(dyingSpriteFile);

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
            Intellect = new Attribute(intellect);
            Speed = new Attribute(speed);
            Perception = new Attribute(perception);
            Weight = new Attribute(weight); // Roughly in pounds

            // ...Other Attributes
            Name = name;
            Level = level;
            Experience = experience;
            Direction = direction;
            Disposition = disposition; // {"Party", "Neutral", "Enemy", "Terrain"}
            Passable = passable; // If true, passable to all objects. If false, passable to objects of same disposition.

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
            if(Name == "")
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

        public void Attack(float direction = -1, int range = 1)
        // Attack in a given direction
        {
            // Get direction if not provided
            if (direction == -1)
            {
                direction = Direction;
            }

            // TODO: Play attack animation

            // Figure out which one you hit
            // TODO: Different types of attacks
            GameObject target = GetTarget(direction, range);
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

        public GameObject GetTarget(float direction, int range)
        {
            // TODO: Rewrite all of this, should make it based on sprite collision
            if (direction == 0f)
            {
                return Globals.Grid.GetObject(X, Y + 1);
            }
            else if (direction == 90f)
            {
                return Globals.Grid.GetObject(X + 1, Y);
            }
            else if (direction == 180f)
            {
                return Globals.Grid.GetObject(X, Y - 1);
            }
            else if (direction == 270f)
            {
                return Globals.Grid.GetObject(X - 1, Y);
            }
            else
            {
                return null;
            }
        }

        public bool CanMoveTo(int x, int y)
        /*
        Sees if the GameObject can move to the specified location unimpeded.
        Note that x and y refer to the top left square of the object.
        */
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
            string direction,
            int distance = 1
        )
        /*
        Attempts to move the object in a direction.
        */
        {
            string first_char = direction.Substring(0, 1).ToLower();
            int DeltaX = 0;
            int DeltaY = 0;

            // Trying to move up
            if (first_char == "n" || first_char == "u")
            {
                DeltaY = distance;
                Direction = 0f;
            }

            // Trying to move down
            else if (first_char == "s" || first_char == "d")
            {
                DeltaY = -distance;
                Direction = (float)Math.PI;
            }

            // Trying to move to the right
            else if (first_char == "e" || first_char == "r")
            {
                DeltaX = distance;
                Direction = .5f * (float)Math.PI; ;
            }

            // Trying to move to the left
            else if (first_char == "w" || first_char == "l")
            {
                DeltaX = -distance;
                Direction = 1.5f * (float)Math.PI; ;
            }

            // See if you can move to the location
            Globals.Log(Name + " currently at:      " + X.ToString() + ", " + Y.ToString());
            Globals.Log(Name + " trying to move to: " + (X + DeltaX).ToString() + ", " + (Y + DeltaY).ToString());
            if (CanMoveTo(X + DeltaX, Y + DeltaY))
            {
                // Null out all previous locations
                Despawn();

                // Assing new attributes for object
                X = Globals.Mod(X + DeltaX, Globals.Grid.GetLength(0));
                Y = Globals.Mod(Y + DeltaY, Globals.Grid.GetLength(1));

                // Assign all new locations
                Spawn();
            }
        }
    }
}
