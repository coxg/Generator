using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public class SpriteSheet
    {
        public SpriteSheet(
            string textureName,
            List<Sprite> sprites,
            int blockSize=64
        )
        {
            TextureName = textureName;
            Texture = Globals.ContentManager.Load<Texture2D>(TextureName);
            BlockSize = blockSize;
            Height = Texture.Height / BlockSize;
            Width = Texture.Width / BlockSize;
            Sprites = sprites;

            foreach (var sprite in Sprites)
            {
                SpriteDict[sprite.Name] = sprite;
            }
        }
        
        private string TextureName;
        private int BlockSize;
        private int Height;  // In blocks
        private int Width;  // In blocks
        private List<Sprite> Sprites;
        private Dictionary<string, Sprite> SpriteDict = new Dictionary<string, Sprite>();

        public Sprite GetCopy(string name)
        {
            // TODO: Better to just manually create new object?
            return (Sprite)Globals.Copy(SpriteDict[name]);
        }
        
        public Vector2[] GetTextureCoordinates(Component component)
        {
            if (component.Sprite == null)
            {
                return new Vector2[0];
            }
            
            var row = component.Sprite.Y;
            var col = component.Sprite.X;
            
            // Factor in component direction
            if (component.Sprite.Directional)
            {
                var directionOffset = component.Sprite.Directions.IndexOf(
                    MathTools.StringFromRadians(component.Direction));
                if (directionOffset == -1)
                {
                    return new Vector2[0];
                }
                row += directionOffset * component.Sprite.Height;
            }

            // Factor in animation frames
            col += component.Sprite.GetAnimationFrame();
            
            var xMin = (float)col / Width;
            var xMax = (float)(col + component.Sprite.Width) / Width;
            var yMin = (float)row / Height;
            var yMax = (float)(row + component.Sprite.Height) / Height;
            
            var bottomLeft = new Vector2(xMin, yMax);
            var topLeft = new Vector2(xMin, yMin);
            var bottomRight = new Vector2(xMax, yMax);
            var topRight = new Vector2(xMax, yMin);
            
            return new [] { bottomLeft, topLeft, bottomRight, topRight };
        }

        [Newtonsoft.Json.JsonIgnore]
        public Texture2D Texture;
    }
}