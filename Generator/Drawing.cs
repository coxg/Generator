using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

namespace Generator
{
    public static class Drawing
    {
        // It's like a placeholder for a function
        public delegate void DrawingFunction(
            Texture2D texture2D,
            Rectangle spriteRectangle,
            object sourceRectangle,
            Color color,
            float direction,
            Vector2 vector2,
            SpriteEffects spriteEffects,
            float height,
            Queue queue,
            SpriteBatch spriteBatch);

        // Submits the drawing to the queue
        public static void SubmitToQueue(
            Texture2D texture2D,
            Rectangle spriteRectangle,
            object sourceRectangle,
            Color color,
            float direction,
            Vector2 vector2,
            SpriteEffects spriteEffects,
            float height,
            Queue queue,
            SpriteBatch spriteBatch)
        {
            queue.Enqueue(new DrawEntry(
                texture2D,
                spriteRectangle,
                null,
                color,
                direction,
                vector2,
                spriteEffects,
                height));
        }

        // Just does the drawing
        public static void SubmitToSpriteBatch(
            Texture2D texture2D,
            Rectangle spriteRectangle,
            object sourceRectangle,
            Color color,
            float direction,
            Vector2 vector2,
            SpriteEffects spriteEffects,
            float height,
            Queue queue,
            SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture2D,
                spriteRectangle,
                null,
                color,
                direction,
                vector2,
                spriteEffects,
                height);
        }

        public class DrawEntry
        // So I can submit drawing instructions to a queue
        {
            // The fields I need
            public Texture2D _Texture2D { get; set; }
            public Rectangle SpriteRectangle { get; set; }
            public Rectangle SourceRectangle { get; set; }
            public Color _Color { get; set; }
            public float Direction { get; set; }
            public Vector2 _Vector2 { get; set; }
            public SpriteEffects _SpriteEffects { get; set; }
            public float Height { get; set; }

            // Constructor
            public DrawEntry(
                Texture2D texture2D,
                Rectangle spriteRectangle,
                object sourceRectangle,
                Color color,
                float direction,
                Vector2 vector2,
                SpriteEffects spriteEffects,
                float height)
            {
                _Texture2D = texture2D;
                SpriteRectangle = spriteRectangle;
                _Color = color;
                Direction = direction;
                _Vector2 = vector2;
                _SpriteEffects = spriteEffects;
                Height = height;
            }
        }

        public static void DrawASquare(
            int xValue, int yValue, DrawingFunction drawingFunction,
            Queue queue, SpriteBatch spriteBatch)
        {
            // See if we need to draw grid lines
            if (Globals.GridAlpha > 0)
            {
                // Get coordinates of the square
                Vector2 SquareCoordinates = new Vector2(
                    xValue * Globals.SquareSize,
                    Globals.Resolution.Y - yValue * Globals.SquareSize);

                // Rotate
                SquareCoordinates = Globals.GetRotatedCoordinates(SquareCoordinates);

                // Double check that we need to draw grid lines
                if (SquareCoordinates.X > 0
                    && SquareCoordinates.X < Globals.Resolution.X + Globals.SquareSize
                    && SquareCoordinates.Y > 0
                    && SquareCoordinates.Y < Globals.Resolution.Y + Globals.SquareSize)
                {
                    // Submit the horizontal line
                    drawingFunction(
                        Globals.WhiteDot,
                        new Rectangle(
                            (int)SquareCoordinates.X,
                            (int)SquareCoordinates.Y,
                            Globals.SquareSize,
                            1),
                        null,
                        Color.FromNonPremultiplied(Globals.GridAlpha, 100, 100, 100),
                        (float)Globals.MapRotation,
                        new Vector2(
                            Globals.WhiteDot.Width,
                            Globals.WhiteDot.Height),
                        SpriteEffects.None,
                        1,
                        queue,
                        spriteBatch);

                    // Submit the vertical line
                    drawingFunction(
                        Globals.WhiteDot,
                        new Rectangle(
                            (int)SquareCoordinates.X,
                            (int)SquareCoordinates.Y,
                            1,
                            Globals.SquareSize),
                        null,
                        Color.FromNonPremultiplied(Globals.GridAlpha, 100, 100, 100),
                        (float)Globals.MapRotation,
                        new Vector2(
                            Globals.WhiteDot.Width,
                            Globals.WhiteDot.Height),
                        SpriteEffects.None,
                        1,
                        queue,
                        spriteBatch);
                }
            }

            // See if we need to draw an object
            var current_object = Globals.Grid.GetObject(xValue, yValue);
            if (current_object != null)
            {

                // Get center of object
                Vector2 ObjectCoordinates = new Vector2(
                    current_object.X * Globals.SquareSize
                        + current_object.Width * Globals.SquareSize / 2,
                    Globals.Resolution.Y - current_object.Y * Globals.SquareSize
                        + current_object.Height * Globals.SquareSize / 2);

                // Rotate
                ObjectCoordinates = Globals.GetRotatedCoordinates(ObjectCoordinates);

                // Double check that we need to draw grid lines
                if (ObjectCoordinates.X > 0
                    && ObjectCoordinates.X < Globals.Resolution.X + Globals.SquareSize
                    && ObjectCoordinates.Y > 0
                    && ObjectCoordinates.Y < Globals.Resolution.Y + Globals.SquareSize)
                {
                    // Draw the object
                    drawingFunction(
                        current_object.StandingSprite,
                        new Rectangle(
                            (int)ObjectCoordinates.X,
                            (int)ObjectCoordinates.Y,
                            Globals.SquareSize * current_object.Width,
                            Globals.SquareSize * current_object.Height),
                        null,
                        Color.White,
                        current_object.Direction + (float)Globals.MapRotation,
                        new Vector2(
                            current_object.StandingSprite.Width / 2,
                            current_object.StandingSprite.Height / 2),
                        SpriteEffects.None,
                        .5f,
                        queue,
                        spriteBatch);

                    // Draw the weapon
                    drawingFunction(
                        current_object.EquippedWeapon.Sprite,
                        new Rectangle(
                            (int)ObjectCoordinates.X,
                            (int)ObjectCoordinates.Y,
                            Globals.SquareSize / 2,
                            Globals.SquareSize),
                        null,
                        Color.White,
                        current_object.Direction + (float)Globals.MapRotation,
                        new Vector2(
                            current_object.EquippedWeapon.Sprite.Width / 2,
                            current_object.StandingSprite.Height + current_object.EquippedWeapon.Sprite.Height),
                        SpriteEffects.None,
                        0,
                        queue,
                        spriteBatch);
                }
            }
        }

        public static void GetRectanglesParallel(SpriteBatch spriteBatch, Queue queue, DrawingFunction drawingFunction)
        {
            // Loop through each element in the grid... in parallel!
            Parallel.ForEach(Enumerable.Range(
                    -(int)(.2f * (float)Globals.Grid.GetLength(0)), 
                     (int)(1.4f * (float)Globals.Grid.GetLength(0))
                ), (xValue) =>
            {
                Parallel.ForEach(Enumerable.Range(
                        -(int)(.2f * (float)Globals.Grid.GetLength(1)), 
                         (int)(1.4f * (float)Globals.Grid.GetLength(1))
                    ), (yValue) =>
                {
                    DrawASquare(xValue, yValue, drawingFunction, queue, spriteBatch);
                });
            });
        }

        public static void GetRectanglesNonParallel(SpriteBatch spriteBatch, Queue queue, DrawingFunction drawingFunction)
        {
            // Loop through each element in the grid
            for (
                int xValue = -(int)(.2f * (float)Globals.Grid.GetLength(0)); 
                xValue < (int)(1.2f * (float)Globals.Grid.GetLength(0)); 
                xValue++)
            {
                for (
                    int yValue = -(int)(.2f * (float)Globals.Grid.GetLength(1)); 
                    yValue < (int)(1.2f * (float)Globals.Grid.GetLength(1)); 
                    yValue++)
                {
                    DrawASquare(xValue, yValue, drawingFunction, queue, spriteBatch);
                }
            }
        }

        public static void DrawFromBatch(SpriteBatch spriteBatch, Queue queue)
        // Moves everything from the queue to the spriteBatch
        {
            while (queue.Count != 0)
            {
                DrawEntry currentObject = (DrawEntry)queue.Dequeue();
                spriteBatch.Draw(
                    currentObject._Texture2D,
                    currentObject.SpriteRectangle,
                    currentObject.SourceRectangle,
                    currentObject._Color,
                    currentObject.Direction,
                    currentObject._Vector2,
                    currentObject._SpriteEffects,
                    currentObject.Height);
            }
        }
    }
}
