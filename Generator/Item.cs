using Microsoft.Xna.Framework.Graphics;
using System;

namespace Generator
{
    public class Item
        // Anything which can be in the party's inventory. Potions, quest items, equipments, whatever.
    {
        public int Quantity;
        public Texture2D Sprite;
        public string Name;
        public GameObjectManager.Delegate Effect;

        // Constructor
        public Item(string name, Texture2D sprite, int quantity=1, GameObjectManager.Delegate effect=null)
        {
            Name = name;
            Sprite = sprite;
            Quantity = quantity;
            Effect = effect;
        }

        public void Use(GameObject gameObject)
            // Use the item
        {
            Effect(gameObject);
        }
    }
}