using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class Item
        // Anything which can be in the party's inventory. Potions, quest items, equipments, whatever.
    {
        public int Quantity;
        public Texture2D Sprite;
        public string Name;

        // Constructor
        public Item(string name, Texture2D sprite, int quantity=1)
        {
            Name = name;
            Sprite = sprite;
            Quantity = quantity;
        }
    }
}