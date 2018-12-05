using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Generator
{
    public static class Drawing
    {
        public static void DrawTextBox(SpriteBatch spriteBatch)
            // Use this for text displayed at the bottom of the screen
        {
            var TextBoxHeight = 256;
            var Margin = 16;

            // Draw the background
            spriteBatch.Draw(
                Globals.WhiteDot,
                new Rectangle(
                    0,
                    (int) Globals.Resolution.Y - TextBoxHeight,
                    (int) Globals.Resolution.X,
                    TextBoxHeight),
                null,
                Color.FromNonPremultiplied(0, 0, 0, 200),
                0f,
                new Vector2(0, 0),
                SpriteEffects.None,
                .05f);

            // Draw the sprite
            var spriteWidthAndExtraMargin = 0;
            if (Globals.DisplayTextQueue.Count != 0
                && Globals.TalkingObjectQueue.Count != 0
                && Globals.TalkingObjectQueue.Peek().Avatar != null)
            {
                spriteBatch.Draw(
                    Globals.TalkingObjectQueue.Peek().Avatar,
                    new Rectangle(
                        Margin,
                        (int) Globals.Resolution.Y - TextBoxHeight + Margin,
                        TextBoxHeight - 2 * Margin,
                        TextBoxHeight - 2 * Margin),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    .04f);
                spriteWidthAndExtraMargin = TextBoxHeight - Margin;
            }

            // Draw the text itself
            spriteBatch.DrawString(
                Globals.Font,
                Globals.DisplayTextQueue.Peek(),
                new Vector2(
                    Margin + spriteWidthAndExtraMargin,
                    Globals.Resolution.Y - (TextBoxHeight - Margin)),
                Color.White);
        }

        public static void DrawResource(SpriteBatch spriteBatch, Resource resource, int partyNumber)
            // Draw a single resource bar
        {
            // Figure out the resource specific information
            var margin = 16;
            var barHeight = 20;
            var barColor = new Vector3(0, 0, 0);
            var height = margin + partyNumber * 3 * (margin + barHeight);            
            switch (resource.Name)
            {
                case "Health":
                    barColor = new Vector3(255, 0, 0);
                    break;
                case "Stamina":
                    barColor = new Vector3(0, 255, 0);
                    height += margin + barHeight;
                    break;
                case "Electricity":
                    barColor = new Vector3(0, 0, 255);
                    height += 2 * (margin + barHeight);
                    break;
            }

            // Draw the bar
            spriteBatch.Draw(
                Globals.WhiteDot,
                new Rectangle(
                    margin,
                    height,
                    (int) (256 * resource.Current / resource.Max),
                    barHeight),
                null,
                Color.FromNonPremultiplied((int) barColor.X, (int) barColor.Y, (int) barColor.Z, 255),
                0f,
                new Vector2(0, 0),
                SpriteEffects.None,
                .05f);
        }

        public static void DrawSprite(
                Texture2D sprite,
                Vector3 bottomLeft,
                Vector3 size)
            // This should be used to draw characters.
            // These should be able to move, rotate, etc.
        {
            // Generate the vertices
            var vertices = new VertexPositionTexture[6];

            // Bottom left
            vertices[0].Position = new Vector3(
                bottomLeft.X,
                bottomLeft.Y,
                bottomLeft.Z);

            // Top left
            vertices[1].Position = new Vector3(
                bottomLeft.X,
                bottomLeft.Y,
                bottomLeft.Z + size.Z);

            // Bottom right
            vertices[2].Position = new Vector3(
                bottomLeft.X + size.X,
                bottomLeft.Y,
                bottomLeft.Z);
            vertices[3].Position = vertices[1].Position;

            // Top right
            vertices[4].Position = new Vector3(
                bottomLeft.X + size.X,
                bottomLeft.Y,
                bottomLeft.Z + size.Z);
            vertices[5].Position = vertices[2].Position;

            // Generate the texture coordinates
            vertices[0].TextureCoordinate = new Vector2(1, 1);
            vertices[1].TextureCoordinate = new Vector2(1, 0);
            vertices[2].TextureCoordinate = new Vector2(0, 1);
            vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
            vertices[4].TextureCoordinate = new Vector2(0, 0);
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

            // Draw it
            var effect = new BasicEffect(GameControl.graphics.GraphicsDevice)
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

            effect.Dispose();
        }

        public static void DrawTile(
                Texture2D sprite,
                Vector2 bottomLeft,
                Vector2 size,
                int repetitions = 1)
            // This should be used to draw all tiles. 
            // These should pretty much remain stationary on the floor.
        {
            // Generate the vertices
            var vertices = new VertexPositionTexture[6];

            // Bottom left
            vertices[0].Position = new Vector3(bottomLeft.X, bottomLeft.Y, 0);

            // Top left
            vertices[1].Position = new Vector3(bottomLeft.X, bottomLeft.Y + size.Y, 0);

            // Bottom right
            vertices[2].Position = new Vector3(bottomLeft.X + size.X, bottomLeft.Y, 0);
            vertices[3].Position = vertices[1].Position;

            // Top right
            vertices[4].Position = new Vector3(bottomLeft.X + size.X, bottomLeft.Y + size.Y, 0);
            vertices[5].Position = vertices[2].Position;

            // Generate the texture coordinates
            vertices[0].TextureCoordinate = new Vector2(repetitions, repetitions);
            vertices[1].TextureCoordinate = new Vector2(repetitions, 0);
            vertices[2].TextureCoordinate = new Vector2(0, repetitions);
            vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
            vertices[4].TextureCoordinate = new Vector2(0, 0);
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

            // Draw it
            var effect = new BasicEffect(GameControl.graphics.GraphicsDevice)
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

            effect.Dispose();
        }
    }
}