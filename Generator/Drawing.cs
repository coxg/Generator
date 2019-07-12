﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator
{
    public static class Drawing
    {

        public static Vector3 GetBrightness(float x, float y)
        {

            var brightness = new Vector3(.02f, .02f, .02f);

            // TODO: Rather than looping through all objects for each tile, 
            // create a mapping layer for brightness which gets computed on each
            // update.
            foreach (var gameObject in GameObjectManager.ObjectFromName.Values)
            {
                var objectDistance = Math.Sqrt(
                    Math.Pow(gameObject.Center.X - x, 2) + Math.Pow(gameObject.Center.Y - y, 2));

                // Make sure we're not being blocked by a gameObject
                if (objectDistance < gameObject.Brightness.Length() * 2 * MathHelper.Pi
                    && gameObject.CanSee(new Vector3(x, y, 0)))
                {
                    var flutteryBrightness = .01f * (float)Math.Cos(Timer.Clock / 10) * gameObject.Brightness;
                    brightness += flutteryBrightness + gameObject.Brightness * (float)
                        Math.Pow(Math.Cos(.25 / gameObject.Brightness.Length() * objectDistance), 2);
                }
            }

            // Smooth lighting, multiple lighting effects can stack together
            brightness = new Vector3(
                (float)Math.Sqrt(brightness.X),
                (float)Math.Sqrt(brightness.Y),
                (float)Math.Sqrt(brightness.Z));
            return brightness;
        }

        public static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, Vector2 bottomLeft, Vector2 topRight)
        // Draw a single sprite
        {
            spriteBatch.Draw(
                texture,
                new Rectangle(
                    (int)bottomLeft.X,
                    (int)(Globals.Resolution.Y - topRight.Y - bottomLeft.Y),
                    (int)(topRight.X - bottomLeft.X),
                    (int)(topRight.Y - bottomLeft.Y)),
                null,
                Color.White,
                0,
                new Vector2(0, 0),
                SpriteEffects.None,
                .04f);
        }

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
                && Globals.TalkingObjectQueue.Peek().Sprite != null)
            {
                spriteBatch.Draw(
                    Globals.TalkingObjectQueue.Peek().Sprite,
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

        public static VertexPositionColorTexture[] GetComponentVertices(
            Component component, 
            Vector3 size, 
            Vector3 normalizationDirection, 
            Vector3 normalizationOffset)
        {
            var vertices = commonVertices["Component"];
            var bottomLeft = component.Position;
            var rotationPoint = bottomLeft + component.RotationPoint;
            var rotationDirection = MathTools.PointRotatedAroundPoint(
                component.RotationOffset,
                Vector3.Zero,
                new Vector3(0, 0, component.Direction));

            // Bottom left
            vertices[0].Position = MathTools.PointRotatedAroundPoint(
                bottomLeft,
                rotationPoint,
                rotationDirection);
            vertices[0].Position = MathTools.PointRotatedAroundPoint(
                vertices[0].Position,
                component.SourceObject.Center,
                normalizationDirection);
            vertices[0].Position += normalizationOffset;

            // Top left
            vertices[1].Position = MathTools.PointRotatedAroundPoint(
                new Vector3(
                    bottomLeft.X,
                    bottomLeft.Y,
                    bottomLeft.Z + size.Z),
                rotationPoint,
                rotationDirection);
            vertices[1].Position = MathTools.PointRotatedAroundPoint(
                vertices[1].Position,
                component.SourceObject.Center,
                normalizationDirection);
            vertices[1].Position += normalizationOffset;

            // Bottom right
            vertices[2].Position = MathTools.PointRotatedAroundPoint(
                new Vector3(
                    bottomLeft.X + size.X,
                    bottomLeft.Y,
                    bottomLeft.Z),
                rotationPoint,
                rotationDirection);
            vertices[2].Position = MathTools.PointRotatedAroundPoint(
                vertices[2].Position,
                component.SourceObject.Center,
                normalizationDirection);
            vertices[2].Position += normalizationOffset;
            vertices[3].Position = vertices[1].Position;

            // Top right
            vertices[4].Position = MathTools.PointRotatedAroundPoint(
                new Vector3(
                    bottomLeft.X + size.X,
                    bottomLeft.Y,
                    bottomLeft.Z + size.Z),
                rotationPoint,
                rotationDirection);
            vertices[4].Position = MathTools.PointRotatedAroundPoint(
                vertices[4].Position,
                component.SourceObject.Center,
                normalizationDirection);
            vertices[4].Position += normalizationOffset;
            vertices[5].Position = vertices[2].Position;

            return vertices;
        }

        public static void DrawComponentShadow(
                Component component,
                Vector3 size,
                float direction,
                GameObject lightSource)
        // This should be used to draw shadows for character components.
        // TODO: Use the lightSource's position to calculate shadow size/fade/blur/whatever
        {
            var normalizationDirection = new Vector3(-MathHelper.PiOver2, 0, direction + MathHelper.PiOver2);
            var normalizationOffset = MathTools.PointRotatedAroundPoint(
                Vector3.Zero,
                new Vector3(component.SourceObject.Size.X / 2, 0, 0),
                new Vector3(0, 0, direction));
            normalizationOffset += new Vector3(
                -component.SourceObject.Size.X / 2,
                -component.SourceObject.Size.Z / 2,
                0);
            var vertices = GetComponentVertices(component, size, normalizationDirection, normalizationOffset);

            // Null out color and position
            for (var vertexIndex = 0; vertexIndex < 6; vertexIndex++)
            {
                vertices[vertexIndex].Position.Z = 0;
                vertices[vertexIndex].Color = Color.Black;
            }

            // Draw it
            GameControl.effect.Texture = component.Sprite;
            foreach (var pass in GameControl.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, 2);
            }
        }

        public static void DrawComponent(
                Component component,
                Vector3 size)
            // This should be used to draw characters.
            // These should be able to move, rotate, etc.
        {
            var vertices = GetComponentVertices(
                component, 
                size, 
                new Vector3(-MathHelper.PiOver2, 0, 0), 
                Vector3.Zero);

            // Generate shadow gradients by calculating brightness at each vertex
            var leftBrightness = new Color(GetBrightness(
                component.SourceObject.Center.X - .5f, component.SourceObject.Position.Y - .5f));
            var rightBrightness = new Color(GetBrightness(
                component.SourceObject.Center.X + .5f, component.SourceObject.Position.Y - .5f));
            vertices[0].Color = leftBrightness;
            vertices[1].Color = leftBrightness;
            vertices[2].Color = rightBrightness;
            vertices[3].Color = leftBrightness;
            vertices[4].Color = rightBrightness;
            vertices[5].Color = rightBrightness;

            // Null out position
            for (var vertexIndex = 0; vertexIndex < 6; vertexIndex++)
            {
                vertices[vertexIndex].Position.Z = 0;
            }

            // Draw it
            GameControl.effect.Texture = component.Sprite;
            foreach (var pass in GameControl.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, 2);
            }
        }

        // Draws all necessary layers for each tile
        public static void DrawTile(int x, int y, float opacity = 1)
        {
            // Draw the base tile itself
            var tileIndex = TileManager.GetIndex(x, y);
            var bottomLeft = new Vector2(x, y);
            DrawTileLayer(TileManager.NameFromIndex[tileIndex], bottomLeft, opacity: opacity);

            // Figure which tiles surround the current tile
            var surroundingTileMap = new Dictionary<string, Tile>
                    {
                        { "Top", TileManager.Get(x, y + 1) },
                        { "Bottom", TileManager.Get(x, y - 1) },
                        { "Right", TileManager.Get(x + 1, y) },
                        { "Left", TileManager.Get(x - 1, y) },
                        { "Top Left", TileManager.Get(x - 1, y + 1) },
                        { "Top Right", TileManager.Get(x + 1, y + 1) },
                        { "Bottom Right", TileManager.Get(x + 1, y - 1) },
                        { "Bottom Left", TileManager.Get(x - 1, y - 1) },
                    };

            // Figure out which unique tiles surround the current tile
            var uniqueSurroundingTileMap = new Dictionary<int, HashSet<string>>();
            foreach (var surroundingTile in surroundingTileMap)
            {
                if (surroundingTile.Value.BaseTileIndex > tileIndex)
                {
                    if (!uniqueSurroundingTileMap.ContainsKey(surroundingTile.Value.BaseTileIndex))
                    {
                        uniqueSurroundingTileMap.Add(surroundingTile.Value.BaseTileIndex, new HashSet<string>());
                    }
                    uniqueSurroundingTileMap[surroundingTile.Value.BaseTileIndex].Add(surroundingTile.Key);
                }
            }

            // Loop through each unique tile from smallest to largest index, applying all layers for each
            foreach (var uniqueSurroundingTile in uniqueSurroundingTileMap.OrderBy(uniqueSurroundingTile => uniqueSurroundingTile.Key))
            {
                var tileName = TileManager.NameFromIndex[uniqueSurroundingTile.Key].Split(' ')[0];

                // If we are being drawn over the tiles on all sides
                if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Bottom")
                    && uniqueSurroundingTile.Value.Contains("Left") && uniqueSurroundingTile.Value.Contains("Right"))
                    DrawTileLayer(tileName + " O", bottomLeft, opacity: opacity);

                else
                {
                    // If we are being drawn over by the bottom three sides
                    if (uniqueSurroundingTile.Value.Contains("Bottom") && uniqueSurroundingTile.Value.Contains("Left")
                        && uniqueSurroundingTile.Value.Contains("Right"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Bottom", opacity: opacity);

                    // If we are being drawn over by the left three sides
                    else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Bottom")
                        && uniqueSurroundingTile.Value.Contains("Left"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Left", opacity: opacity);

                    // If we are being drawn over by the top three sides
                    else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Left")
                        && uniqueSurroundingTile.Value.Contains("Right"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Top", opacity: opacity);

                    // If we are being drawn over by the bottom right sides
                    else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Bottom")
                        && uniqueSurroundingTile.Value.Contains("Right"))
                        DrawTileLayer(tileName + " U", bottomLeft, "Right", opacity: opacity);

                    else
                    {
                        // If we are being drawn over the bottom and the left
                        if (uniqueSurroundingTile.Value.Contains("Bottom") && uniqueSurroundingTile.Value.Contains("Left"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Bottom", opacity: opacity);

                            // If we are being drawn over the tile on the top right
                            if (uniqueSurroundingTile.Value.Contains("Top Right") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Top", opacity: opacity);
                        }

                        // If we are being drawn over the top and the left
                        else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Left"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Left", opacity: opacity);

                            // If we are being drawn over the tile on the bottom right
                            if (uniqueSurroundingTile.Value.Contains("Bottom Right") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Right", opacity: opacity);
                        }

                        // If we are being drawn over the top and the right
                        else if (uniqueSurroundingTile.Value.Contains("Top") && uniqueSurroundingTile.Value.Contains("Right"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Top", opacity: opacity);

                            // If we are being drawn over the tile on the bottom left
                            if (uniqueSurroundingTile.Value.Contains("Bottom Left") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Bottom", opacity: opacity);
                        }

                        // If we are being drawn over the bottom and the right
                        else if (uniqueSurroundingTile.Value.Contains("Bottom") && uniqueSurroundingTile.Value.Contains("Right"))
                        {
                            DrawTileLayer(tileName + " L", bottomLeft, "Right", opacity: opacity);

                            // If we are being drawn over the tile on the top left
                            if (uniqueSurroundingTile.Value.Contains("Top Left") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Left", opacity: opacity);
                        }

                        else
                        {
                            // If we are being drawn over the tile on the right
                            if (uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Right", opacity: opacity);

                            // If we are being drawn over the tile on the left
                            if (uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Left", opacity: opacity);

                            // If we are being drawn over the tile on the bottom
                            if (uniqueSurroundingTile.Value.Contains("Bottom"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Bottom", opacity: opacity);

                            // If we are being drawn over the tile on the top
                            if (uniqueSurroundingTile.Value.Contains("Top"))
                                DrawTileLayer(tileName + " side", bottomLeft, "Top", opacity: opacity);

                            // If we are being drawn over the tile on the top right
                            if (uniqueSurroundingTile.Value.Contains("Top Right") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Top", opacity: opacity);

                            // If we are being drawn over the tile on the top left
                            if (uniqueSurroundingTile.Value.Contains("Top Left") && !uniqueSurroundingTile.Value.Contains("Top")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Left", opacity: opacity);

                            // If we are being drawn over the tile on the bottom right
                            if (uniqueSurroundingTile.Value.Contains("Bottom Right") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Right"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Right", opacity: opacity);

                            // If we are being drawn over the tile on the bottom left
                            if (uniqueSurroundingTile.Value.Contains("Bottom Left") && !uniqueSurroundingTile.Value.Contains("Bottom")
                                && !uniqueSurroundingTile.Value.Contains("Left"))
                                DrawTileLayer(tileName + " corner", bottomLeft, "Bottom", opacity: opacity);
                        }
                    }
                }
            }
        }

        // Draws a light source
        public static void DrawLight(
                Vector3 position,
                float size,
                Color color)
        {
            // Generate the vertices
            var vertices = commonVertices["Bottom"];

            // Bottom left
            var bottomLeft = position - Vector3.One * size / 2;
            vertices[0].Position = new Vector3(bottomLeft.X, bottomLeft.Y, 0);

            // Top left
            vertices[1].Position = new Vector3(bottomLeft.X, bottomLeft.Y + size, 0);

            // Bottom right
            vertices[2].Position = new Vector3(bottomLeft.X + size, bottomLeft.Y, 0);
            vertices[3].Position = vertices[1].Position;

            // Top right
            vertices[4].Position = new Vector3(bottomLeft.X + size, bottomLeft.Y + size, 0);
            vertices[5].Position = vertices[2].Position;

            // Set the colors to white - the shadows will lay over this
            for (var i = 0; i < 6; i++)
            {
                vertices[i].Color = color;
            }

            // Draw it
            GameControl.effect.Texture = Globals.LightTexture;
            foreach (var pass in GameControl.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, 2);
            }
        }

        public static VertexPositionColorTexture[] GetVertices(string side, Color color)
        {
            var vertices = new VertexPositionColorTexture[6];

            switch (side)
            {
                case "Bottom":
                    vertices[0].TextureCoordinate = new Vector2(0, 1); // Bottom left
                    vertices[1].TextureCoordinate = new Vector2(0, 0); // Top left
                    vertices[2].TextureCoordinate = new Vector2(1, 1); // Bottom right
                    vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
                    vertices[4].TextureCoordinate = new Vector2(1, 0); // Top right
                    vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;
                    break;
                case "Top":
                    vertices[0].TextureCoordinate = new Vector2(1, 0); // Bottom left
                    vertices[1].TextureCoordinate = new Vector2(1, 1); // Top left
                    vertices[2].TextureCoordinate = new Vector2(0, 0); // Bottom right
                    vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
                    vertices[4].TextureCoordinate = new Vector2(0, 1); // Top right
                    vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;
                    break;
                case "Left":
                    vertices[0].TextureCoordinate = new Vector2(1, 1); // Bottom left
                    vertices[1].TextureCoordinate = new Vector2(0, 1); // Top left
                    vertices[2].TextureCoordinate = new Vector2(1, 0); // Bottom right
                    vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
                    vertices[4].TextureCoordinate = new Vector2(0, 0); // Top right
                    vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;
                    break;
                case "Right":
                    vertices[0].TextureCoordinate = new Vector2(0, 0); // Bottom left
                    vertices[1].TextureCoordinate = new Vector2(1, 0); // Top left
                    vertices[2].TextureCoordinate = new Vector2(0, 1); // Bottom right
                    vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
                    vertices[4].TextureCoordinate = new Vector2(1, 1); // Top right
                    vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;
                    break;
                case "Component":
                    vertices[0].TextureCoordinate = new Vector2(1, 1); // Bottom left
                    vertices[1].TextureCoordinate = new Vector2(1, 0); // Top left
                    vertices[2].TextureCoordinate = new Vector2(0, 1); // Bottom right
                    vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
                    vertices[4].TextureCoordinate = new Vector2(0, 0); // Top right
                    vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;
                    break;
            }

            // Set the colors to white - the shadows will lay over this
            for (var i = 0; i < 6; i++)
            {
                vertices[i].Color = color;
            }

            return vertices;
        }

        public static Dictionary<string, VertexPositionColorTexture[]> commonVertices = new Dictionary<string, VertexPositionColorTexture[]>()
        {
            { "Bottom", GetVertices("Bottom", Color.White) },
            { "Top", GetVertices("Top", Color.White) },
            { "Left", GetVertices("Left", Color.White) },
            { "Right", GetVertices("Right", Color.White) },
            { "Component", GetVertices("Component", Color.White) },
        };

        // Draws a single layer of a tile
        public static void DrawTileLayer(
                string tileName,
                Vector2 bottomLeft,
                string bottomSide = "Bottom",
                float opacity = 1)
        {
            // Generate the vertices
            var vertices = commonVertices[bottomSide];

            // Bottom left
            vertices[0].Position = new Vector3(bottomLeft.X, bottomLeft.Y, 0);

            // Top left
            vertices[1].Position = new Vector3(bottomLeft.X, bottomLeft.Y + 1, 0);

            // Bottom right
            vertices[2].Position = new Vector3(bottomLeft.X + 1, bottomLeft.Y, 0);

            // Top left
            vertices[3] = vertices[1];

            // Top right
            vertices[4].Position = new Vector3(bottomLeft.X + 1, bottomLeft.Y + 1, 0);

            // Bottom right
            vertices[5] = vertices[2];

            // Draw it
            GameControl.effect.Texture = TileManager.ObjectFromName[tileName].Sprite;
            foreach (var pass in GameControl.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                
                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, vertices, 0, 2);
            }
        }

        // Draws a single layer of a tile
        public static void DrawTilesFromBuffer()
        {
            // Draw it
            GameControl.effect.Texture = TileManager.ObjectFromName.Values.ToList()[11].Sprite;
            foreach (var pass in GameControl.effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                /*
                var myVertexBuffer = new VertexBuffer(GameControl.graphics.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, TileManager.vertices.Length, BufferUsage.WriteOnly);
                myVertexBuffer.SetData(TileManager.vertices);

                var myIndexBuffer = new IndexBuffer(GameControl.graphics.GraphicsDevice, typeof(Int16), TileManager.indices.Length, BufferUsage.WriteOnly);
                myIndexBuffer.SetData(TileManager.indices);

                GameControl.graphics.GraphicsDevice.Indices = myIndexBuffer;
                GameControl.graphics.GraphicsDevice.SetVertexBuffer(myVertexBuffer);
                GameControl.graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TileManager.indices.Length / 3);
                */

                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, TileManager.vertices, 0, TileManager.vertices.Length / 3);
            }
        }

        // Draws the creative mode UI, including tile previews
        public static void DrawCreativeUI(SpriteBatch spriteBatch)
        {
            // Draw the object on the left
            DrawSprite(
                spriteBatch,
                TileManager.ObjectFromName[
                    TileManager.NameFromIndex[
                        TileManager.BaseTileIndexes[
                            (int)MathTools.Mod(Globals.CreativeObjectIndex - 1, TileManager.BaseTileIndexes.Count)]]].Sprite,
                new Vector2(Globals.Resolution.X / 2 - 125, 10),
                new Vector2(Globals.Resolution.X / 2 - 75, 60));

            // Draw the object in the middle
            DrawSprite(
                spriteBatch,
                TileManager.ObjectFromName[
                    TileManager.NameFromIndex[
                         TileManager.BaseTileIndexes[
                            Globals.CreativeObjectIndex]]].Sprite,
                new Vector2(Globals.Resolution.X / 2 - 50, 10),
                new Vector2(Globals.Resolution.X / 2 + 50, 100));

            // Draw the object on the right
            DrawSprite(
                spriteBatch,
                TileManager.ObjectFromName[
                    TileManager.NameFromIndex[
                         TileManager.BaseTileIndexes[
                            (int)MathTools.Mod(Globals.CreativeObjectIndex + 1, TileManager.BaseTileIndexes.Count)]]].Sprite,
                new Vector2(Globals.Resolution.X / 2 + 75, 10),
                new Vector2(Globals.Resolution.X / 2 + 125, 60));
        }
    }
}