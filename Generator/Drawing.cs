using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Generator
{
    public static class Drawing
    {
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

        public static Vector2 RotateAroundPoint(Vector2 coordinates)
        // Map rotation logic
        {
            int XOffsetInPixels = (int)(Globals.MapOffset.X * (double)Globals.SquareSize);
            int YOffsetInPixels = (int)(Globals.MapOffset.Y * (double)Globals.SquareSize);
            Vector2 MapCenter = new Vector2(
                Globals.Resolution.X / 2 + XOffsetInPixels,
                Globals.Resolution.Y / 2 - YOffsetInPixels);

            // Rotate around the center of the screen
            coordinates.X -= MapCenter.X;
            coordinates.Y -= MapCenter.Y;
            double newXCoordinate = coordinates.X * Globals.CurrentCos - coordinates.Y * Globals.CurrentSin;
            double newYCoordinate = coordinates.X * Globals.CurrentSin + coordinates.Y * Globals.CurrentCos;
            coordinates.X = (int)newXCoordinate + MapCenter.X;
            coordinates.Y = (int)newYCoordinate + MapCenter.Y;

            // Apply map translation
            coordinates.X -= XOffsetInPixels;
            coordinates.Y += YOffsetInPixels;

            return coordinates;
        }

        public static void DrawSprite(
            Texture2D sprite,
            Vector3 bottomLeft,
            Vector3 size)
        {
            // Generate the vertices
            VertexPositionTexture[] vertices = new VertexPositionTexture[6];
            vertices[0].Position = new Vector3(
                bottomLeft.X,
                bottomLeft.Y,
                bottomLeft.Z);
            vertices[1].Position = new Vector3(
                bottomLeft.X,
                bottomLeft.Y + size.Y,
                bottomLeft.Z + size.Z);
            vertices[2].Position = new Vector3(
                bottomLeft.X + size.X,
                bottomLeft.Y,
                bottomLeft.Z);
            vertices[3].Position = vertices[1].Position;
            vertices[4].Position = new Vector3(
                bottomLeft.X + size.X,
                bottomLeft.Y + size.Y,
                bottomLeft.Z + size.Z);
            vertices[5].Position = vertices[2].Position;

            // Generate the texture coordinates
            vertices[0].TextureCoordinate = new Vector2(1, 1);
            vertices[1].TextureCoordinate = new Vector2(1, 0);
            vertices[2].TextureCoordinate = new Vector2(0, 1);
            vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
            vertices[4].TextureCoordinate = new Vector2(0, 0);
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

            BasicEffect effect = new BasicEffect(GameControl.graphics.GraphicsDevice)
            {
                View = GameControl.camera.View,
                Projection = GameControl.camera.Projection,
                TextureEnabled = true,
                Texture = sprite
            };

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, 2);
            }
        }

        public static void DrawTile(
            Texture2D sprite,
            Vector2 bottomLeft,
            Vector2 size,
            int repetitions = 1)
        {
            // Generate the vertices
            VertexPositionTexture[] vertices = new VertexPositionTexture[6];
            vertices[0].Position = new Vector3(bottomLeft.X, bottomLeft.Y, 0);
            vertices[1].Position = new Vector3(bottomLeft.X, bottomLeft.Y + size.Y, 0);
            vertices[2].Position = new Vector3(bottomLeft.X + size.X, bottomLeft.Y, 0);
            vertices[3].Position = vertices[1].Position;
            vertices[4].Position = new Vector3(bottomLeft.X + size.X, bottomLeft.Y + size.Y, 0);
            vertices[5].Position = vertices[2].Position;

            // Generate the texture coordinates
            vertices[0].TextureCoordinate = new Vector2(repetitions, repetitions);
            vertices[1].TextureCoordinate = new Vector2(repetitions, 0);
            vertices[2].TextureCoordinate = new Vector2(0, repetitions);
            vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
            vertices[4].TextureCoordinate = new Vector2(0, 0);
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

            BasicEffect effect = new BasicEffect(GameControl.graphics.GraphicsDevice)
            {
                View = GameControl.camera.View,
                Projection = GameControl.camera.Projection,
                TextureEnabled = true,
                Texture = sprite
            };

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, 2);
            }
        }
    }
}
