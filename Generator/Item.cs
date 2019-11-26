using Microsoft.Xna.Framework.Graphics;
using System;

namespace Generator
{
    public class Item
        // Anything which can be in the party's inventory. Potions, quest items, equipments, whatever.
    {
        public int Quantity;
        public Loaded<Texture2D> Sprite;
        public string Name;
        public Loaded<Action<GameObject>> Effect;

        // Constructor
        public Item(string name, Loaded<Texture2D> sprite, int quantity=1, Loaded<Action<GameObject>> effect=null)
        {
            Name = name;
            Sprite = sprite;
            Quantity = quantity;
            Effect = effect;
        }

        public void Use(GameObject gameObject)
            // Use the item
        {
            Effect?.Value(gameObject);
        }
    }
}