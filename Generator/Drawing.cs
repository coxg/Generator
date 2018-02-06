using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System;

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
                ObjectCoordinates = Globals.RotateAroundCenterOfScreen(ObjectCoordinates);

                // Make sure we want to draw the object
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

        public static void GetRectanglesParallel(
            SpriteBatch spriteBatch, Queue queue, DrawingFunction drawingFunction)
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

        public static void GetRectanglesNonParallel(
            SpriteBatch spriteBatch, Queue queue, DrawingFunction drawingFunction)
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

        public static void DrawTextBox(SpriteBatch spriteBatch)
        // Use this for text displayed at the bottom of the screen
        {
            int TextBoxHeight = 256;
            int Margin = 16;

            // Draw the background
            spriteBatch.Draw(
                Globals.WhiteDot,
                new Rectangle(
                    0,
                    (int)Globals.Resolution.Y - TextBoxHeight,
                    (int)Globals.Resolution.X,
                    TextBoxHeight),
                null,
                Color.FromNonPremultiplied(0, 0, 0, 200),
                0f,
                new Vector2(0, 0),
                SpriteEffects.None,
                .05f);

            // Draw the sprite
            int SpriteWidthAndExtraMargin = 0;
            if (Globals.DisplayTextQueue.Count != 0 
                && Globals.TalkingObjectQueue.Count != 0
                && Globals.TalkingObjectQueue.Peek().Avatar != null)
            {
                spriteBatch.Draw(
                    Globals.TalkingObjectQueue.Peek().Avatar,
                    new Rectangle(
                        Margin,
                        (int)Globals.Resolution.Y - TextBoxHeight + Margin,
                        TextBoxHeight - 2 * Margin,
                        TextBoxHeight - 2 * Margin),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    .04f);
                SpriteWidthAndExtraMargin = TextBoxHeight - Margin;
            }

            // Draw the text itself
            spriteBatch.DrawString(
                Globals.Font, 
                Globals.DisplayTextQueue.Peek(), 
                new Vector2(
                    Margin + SpriteWidthAndExtraMargin, 
                    Globals.Resolution.Y - (TextBoxHeight - Margin)), 
                Color.White);
        }

        public static void DrawGridLines(SpriteBatch spriteBatch)
        // Because we don't want to calculate each square individually
        {
            int BiggerResolution = Math.Max((int)Globals.Resolution.X, (int)Globals.Resolution.Y);

            // Get vertical lines
            for (
                int xValue = -(int)(.5 * BiggerResolution / Globals.SquareSize + Math.Abs(Globals.MapOffset.X)); 
                xValue < (int)(1.5 * BiggerResolution / Globals.SquareSize + Math.Abs(Globals.MapOffset.X)); 
                xValue ++)
            {
                // Rotate the center coordinate
                Vector2 RotatedCoordinates = Globals.RotateAroundCenterOfScreen(
                    new Vector2(
                        ((float)Math.Truncate(Globals.MapOffset.X) + xValue) * Globals.SquareSize, 
                        .5f * Globals.Resolution.Y - (float)Math.Truncate(Globals.MapOffset.Y) * Globals.SquareSize));

                // Draw the line
                spriteBatch.Draw(
                    Globals.WhiteDot,
                    new Rectangle(
                        (int)RotatedCoordinates.X,
                        (int)RotatedCoordinates.Y,
                        1,
                        (int)(1.5 * BiggerResolution)),
                    null,
                    Color.FromNonPremultiplied(0, 0, 0, Globals.GridAlpha),
                    (float)Globals.MapRotation,
                    new Vector2(
                        Globals.WhiteDot.Width / 2,
                        Globals.WhiteDot.Height / 2),
                    SpriteEffects.None,
                    .99f);
            }

            // Get horizontal lines
            for (
                int yValue = -(int)(.5 * BiggerResolution / Globals.SquareSize + Math.Abs(Globals.MapOffset.Y));
                yValue < (int)(1.5 * BiggerResolution / Globals.SquareSize + Math.Abs(Globals.MapOffset.Y));
                yValue++)
            {
                // Rotate the center coordinate
                Vector2 RotatedCoordinates = Globals.RotateAroundCenterOfScreen(
                    new Vector2(
                        .5f * Globals.Resolution.X + (float)Math.Truncate(Globals.MapOffset.X) * Globals.SquareSize,
                        (-(float)Math.Truncate(Globals.MapOffset.Y) + yValue) * Globals.SquareSize 
                            + Globals.Mod(Globals.Resolution.Y, Globals.SquareSize)));

                // Draw the line
                spriteBatch.Draw(
                    Globals.WhiteDot,
                    new Rectangle(
                        (int)RotatedCoordinates.X,
                        (int)RotatedCoordinates.Y,
                        (int)(1.5 * BiggerResolution),
                        1),
                    null,
                    Color.FromNonPremultiplied(0, 0, 0, Globals.GridAlpha),
                    (float)Globals.MapRotation,
                    new Vector2(
                        Globals.WhiteDot.Width / 2,
                        Globals.WhiteDot.Height / 2),
                    SpriteEffects.None,
                    .99f);
            }
        }

    }
}
