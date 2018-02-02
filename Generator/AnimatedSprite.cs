using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Generator
{
    class AnimatedSprite
    {
        // Instance variables
        public Texture2D Texture { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        private int currentFrame;
        private int totalFrames;

        // Constructor
        public AnimatedSprite(
            Texture2D texture, 
            int rows = 1, 
            int columns = 1,
            int currentFrame = 0)
        {
            Texture = texture;
            Rows = rows;
            Columns = columns;
            CurrentFrame = currentFrame;
            totalFrames = Rows * Columns;
        }

        // For updating the frames
        public void Update()
        {
            CurrentFrame++;
            if (CurrentFrame == totalFrames)
                CurrentFrame = 0;
        }

        // The brunt of the logic
        public void Draw(SpriteBatch spriteBatch, Vector2 location)
        {
            int width = Texture.Width / Columns;
            int height = Texture.Height / Rows;
            int row = (int)((float)CurrentFrame / (float)Columns);
            int column = CurrentFrame % Columns;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Rectangle destinationRectangle = new Rectangle((int)location.X, (int)location.Y, width, height);

            spriteBatch.Begin();
            spriteBatch.Draw(Texture, destinationRectangle, sourceRectangle, Color.White);
            spriteBatch.End();
        }
    }
}
