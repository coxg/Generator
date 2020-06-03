﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator
{
    public static class Drawing
    {
        public static void DrawSprite(SpriteBatch spriteBatch, Texture2D texture, Vector2 bottomLeft, Vector2 topRight, 
                Rectangle? textureCoordinates=null)
        // Draw a single sprite
        {
            spriteBatch.Draw(
                texture,
                new Rectangle(
                    (int)bottomLeft.X,
                    (int)(Globals.Resolution.Y - topRight.Y - bottomLeft.Y),
                    (int)(topRight.X - bottomLeft.X),
                    (int)(topRight.Y - bottomLeft.Y)),
                textureCoordinates,
                Color.White,
                0,
                new Vector2(0, 0),
                SpriteEffects.None,
                .04f);
        }

        public static void DrawFPS(SpriteBatch spriteBatch)
        // Display FPS at top right
        {
            if (Timing.NumDraws >= Timing.FrameTimes.Length)
            {
                spriteBatch.DrawString(
                    Globals.Font,
                    String.Format("{0:0.0}", Timing.FPS),
                    new Vector2(
                        Globals.Resolution.X - 100,
                        30),
                    Color.Yellow);
            }
        }
        
        public static void DrawPlayerCoordinates(SpriteBatch spriteBatch)
            // Display player coordinates at top right
        {
            spriteBatch.DrawString(
                Globals.Font,
                String.Format("({0:F1}, {1:F1})", Globals.Player.Center.X, Globals.Player.Center.Y),
                new Vector2(
                    Globals.Resolution.X - 400,
                    30),
                Color.Yellow);
        }

        public static void DrawLogs(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            int y = (int)Globals.Resolution.Y - 25;
            for (int i = Globals.Logs.Count - 1; i >= 0; i--)
            {
                var logLine = Globals.Logs[i];
                var color = Color.White;
                if (logLine.StartsWith("[WARNING]", StringComparison.Ordinal)) color = Color.Yellow;
                spriteBatch.DrawString(
                    Globals.Font,
                    logLine,
                    new Vector2(10, y),
                    color,
                    0,
                    Vector2.Zero,
                    .4f,
                    SpriteEffects.None,
                    .05f);
                y -= 15;
            }
            spriteBatch.End();
        }

        public static void DrawRoundedRectangle(
            Rectangle rectangle, int radius, Color? color = null, int borderWidth = 0, Color? borderColor = null)
            // Draw a rounded rectangle
        {
            var top = rectangle.Y + radius;
            var bottom = rectangle.Y + rectangle.Height - radius;
            var left = rectangle.X + radius;
            var right = rectangle.X + rectangle.Width - radius;

            // If a color was provided, fill the inside of the shape
            if (color != null)
            {
                // Draw the rectangle excluding its corners
                var brush = new LilyPath.SolidColorBrush((Color)color);
                GameControl.drawBatch.FillRectangle(
                    brush, new Rectangle(rectangle.X, rectangle.Y + radius, rectangle.Width, rectangle.Height - 2 * radius));
                GameControl.drawBatch.FillRectangle(
                    brush, new Rectangle(rectangle.X + radius, rectangle.Y, rectangle.Width - 2 * radius, rectangle.Height));

                // Draw the corners
                GameControl.drawBatch.FillArc(brush, new Vector2(left, top), radius,
                    MathHelper.Pi, MathHelper.PiOver2, LilyPath.ArcType.Sector);
                GameControl.drawBatch.FillArc(brush, new Vector2(right, top), radius,
                    3 * MathHelper.PiOver2, MathHelper.PiOver2, LilyPath.ArcType.Sector);
                GameControl.drawBatch.FillArc(brush, new Vector2(left, bottom), radius,
                    MathHelper.PiOver2, MathHelper.PiOver2, LilyPath.ArcType.Sector);
                GameControl.drawBatch.FillArc(brush, new Vector2(right, bottom), radius,
                    0, MathHelper.PiOver2, LilyPath.ArcType.Sector);
            }

            // If a border width was provided, draw a border
            if (borderWidth != 0)
            {
                // Draw the curves
                var pen = new LilyPath.Pen(borderColor ?? Color.White, borderWidth);
                GameControl.drawBatch.DrawCircle(pen, new Vector2(left, top), radius);
                GameControl.drawBatch.DrawCircle(pen, new Vector2(right, top), radius);
                GameControl.drawBatch.DrawCircle(pen, new Vector2(left, bottom), radius);
                GameControl.drawBatch.DrawCircle(pen, new Vector2(right, bottom), radius);

                // Draw the lines
                top = rectangle.Y;
                bottom = rectangle.Y + rectangle.Height;
                left = rectangle.X;
                right = rectangle.X + rectangle.Width;
                GameControl.drawBatch.DrawLine(pen, new Vector2(left + radius, top), new Vector2(right - radius, top));
                GameControl.drawBatch.DrawLine(pen, new Vector2(right, bottom - radius), new Vector2(right, top + radius));
                GameControl.drawBatch.DrawLine(pen, new Vector2(left + radius, bottom), new Vector2(right - radius, bottom));
                GameControl.drawBatch.DrawLine(pen, new Vector2(left, top + radius), new Vector2(left, bottom - radius));
            }
        }

        public static Vector2 DrawTextBox(
            SpriteBatch spriteBatch, string text, int x, int y, int maxWidth, Color? color=null, 
            bool drawText=true, string align="left", Color? highlightColor=null, int highlightMargin = 0, int borderThickness = 0)
        // Use this to draw any text boxes.
        // This is super overloaded - also draws speech bubbles, for example. Should this be split up? Or renamed?
        {
            void _draw(string _text, int _x, int _y, Vector2 _dimensions)
            {
                if (align == "right")
                {
                    _x = _x + (int)(maxWidth - _dimensions.X);
                }
                else if (align == "center")
                {
                    _x = (int)(_x - _dimensions.X / 2);
                }


                if (highlightColor != null || borderThickness > 0)
                {
                    DrawRoundedRectangle(
                        new Rectangle(
                            _x - highlightMargin,
                            _y - highlightMargin,
                            (int)_dimensions.X + 2 * highlightMargin,
                            (int)_dimensions.Y + 2 * highlightMargin),
                        highlightMargin,
                        highlightColor,
                        borderThickness,
                        color);
                }

                if (drawText)
                {
                    spriteBatch.Begin();
                    spriteBatch.DrawString(
                        Globals.Font,
                        _text,
                        new Vector2(_x, _y),
                        color ?? Color.White);
                    spriteBatch.End();
                }
            }

            // Super janky, but draw the border and THEN the background and THEN the text
            if (drawText && highlightColor != null)
            {
                DrawTextBox(spriteBatch, text, x, y, maxWidth, drawText: false, align: align,
                    highlightMargin: highlightMargin, borderThickness: borderThickness);
                DrawTextBox(spriteBatch, text, x, y, maxWidth, drawText: false, align: align, 
                    highlightColor: highlightColor, highlightMargin: highlightMargin);
                return DrawTextBox(spriteBatch, text, x, y, maxWidth, color, drawText: drawText, align: align, 
                    highlightMargin: highlightMargin);
            }

            GameControl.drawBatch.Begin();

            // Get the dimensions of the text
            Vector2 dimensions = Globals.Font.MeasureString(text);

            // If we are too large then walk through the words one by one, appending as we go
            if (dimensions.X > maxWidth)
            {
                int maxX = 0;
                int totalY = 0;
                var words = text.Split(' ');
                string currentLine = "";
                foreach (var word in words)
                {
                    var newLine = currentLine + " " + word;
                    if (currentLine == "")
                    {
                        newLine = word;
                    }

                    // If this word puts us over the edge then write the prior line and set this as the new line
                    if (Globals.Font.MeasureString(newLine).X > maxWidth)
                    {
                        var currentLineDimensions = Globals.Font.MeasureString(currentLine);
                        _draw(currentLine, x, y + totalY, currentLineDimensions);
                        totalY += (int)currentLineDimensions.Y;
                        if (currentLineDimensions.X > maxX)
                        {
                            maxX = (int)currentLineDimensions.X;
                        }
                        currentLine = word;
                    }

                    // If not, append it to the list of words and continue looping
                    else
                    {
                        currentLine = newLine;
                    }
                }

                // Draw whatever the remainder for the final line is
                if (currentLine != "")
                {
                    var finalLineDimensions = Globals.Font.MeasureString(currentLine);
                    _draw(currentLine, x, y + totalY, finalLineDimensions);
                    totalY += (int)finalLineDimensions.Y;
                    if (finalLineDimensions.X > maxX)
                    {
                        maxX = (int)finalLineDimensions.X;
                    }
                }
                dimensions = new Vector2(maxX, totalY);
            }

            // If we fit then just go ahead and write it
            else
            {
                _draw(text, x, y, dimensions);
            }

            GameControl.drawBatch.End();
            return dimensions;
        }

        public static void DrawCharacter(SpriteBatch spriteBatch, GameObject character, int size, int x, int y)
        {
            // Draw the sprite if they have one
            if (character.Sprite != null)
            {
                spriteBatch.Draw(
                    character.Sprite,
                    new Rectangle(
                        x,
                        y,
                        size,
                        size),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    .04f);
            }

            // If they don't have a sprite then piece together their components
            else if (character.Components.ContainsKey("Head") && character.Components.ContainsKey("Face"))
            {
                var head = character.Components["Head"];
                var face = character.Components["Face"];
                var faceSize = (int)(size * face.Size / head.Size);
                spriteBatch.Draw(
                    head.Sprites["Front"].Value,
                    new Rectangle(
                        x,
                        y,
                        size,
                        size),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    .04f);
                spriteBatch.Draw(
                    face.Sprites["Front"].Value,
                    new Rectangle(
                        x + size / 2 - faceSize / 2,
                        y + 5 * size / 8 - faceSize / 2,
                        faceSize,
                        faceSize),
                    null,
                    Color.White,
                    0,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    .05f);
            }
        }

        public static void DrawConversation(SpriteBatch spriteBatch)
            // Use this for text displayed at the bottom of the screen
        {
            var Margin = 16;
            var spriteSize = 256;
            var choices = Globals.CurrentConversation.CurrentChoices;
            var backGroundColor = Color.FromNonPremultiplied(0, 0, 0, 150);
            Color? selectionColor = Color.FromNonPremultiplied(105, 69, 169, 255);

            // Draw the background
            spriteBatch.Begin();
            spriteBatch.Draw(
                Globals.WhiteDot,
                new Rectangle(
                    0,
                    0,
                    (int)Globals.Resolution.X,
                    (int)Globals.Resolution.Y),
                null,
                backGroundColor,
                0f,
                new Vector2(0, 0),
                SpriteEffects.None,
                .05f);

            // Get dimensions for the different conversation options
            int maxWidth = (int)Globals.Resolution.X - 4 * Margin - 2 * spriteSize;
            var TextWidth = 0;
            var TextBoxHeight = Margin;
            for (int i = 0; i < choices.Nodes.Count; i++)
            {
                var textDimensions = DrawTextBox(
                    spriteBatch,
                    choices.Nodes[i].Text[0],
                    0,
                    0,
                    maxWidth: maxWidth,
                    drawText: false);

                TextWidth = Math.Max(TextWidth, (int)textDimensions.X);
                TextBoxHeight += (int)textDimensions.Y + 2 * Margin;
            }
            int xOffset = (maxWidth - TextWidth) / 2 + Margin;
            int yOffset = (int)(Globals.Resolution.Y - TextBoxHeight) / 2;

            // Figure out who from the party is talking
            GameObject talkingObject = null;
            for (int i = 0; i <= choices.CurrentNodeIndex; i++)
            {
                var currentTalkingObject = choices.Nodes[i].GetCurrentSpeaker();
                if (currentTalkingObject != Globals.CurrentConversation.SourceObject)
                {
                    talkingObject = currentTalkingObject;
                }
            }
            if (talkingObject == null)
            {
                talkingObject = Globals.Player;
            }

            // Draw the characters who are talking
            DrawCharacter(spriteBatch, talkingObject, spriteSize, xOffset, (int)(Globals.Resolution.Y - spriteSize) / 2);
            DrawCharacter(spriteBatch, Globals.CurrentConversation.SourceObject,
                spriteSize, xOffset + spriteSize + TextWidth + 2 * Margin, (int)(Globals.Resolution.Y - spriteSize) / 2);
            spriteBatch.End();

            // Draw the text itself
            // If we've selected a choice then just draw that choice
            if (choices.ChoiceSelected)
            {
                var currentNode = choices.GetCurrentNode();
                var currentMessage = currentNode.GetCurrentMessage();
                var textDimensions = DrawTextBox(
                    spriteBatch,
                    currentMessage,
                    0,
                    0,
                    TextWidth,
                    drawText: false);
                DrawTextBox(
                    spriteBatch, 
                    currentMessage,
                    xOffset + spriteSize + Margin,
                    (int)(Globals.Resolution.Y - textDimensions.Y) / 2,
                    TextWidth,
                    align: currentNode.GetCurrentSpeaker() == Globals.CurrentConversation.SourceObject ? "right" : "left",
                    highlightColor: selectionColor,
                    highlightMargin: Margin,
                    borderThickness: 5);
            }

            // If we haven't selected a choice yet then show all available choices
            else
            {
                yOffset += Margin;
                foreach (Conversation.Choices.Node node in choices.Nodes)
                {
                    // Draw the choice
                    Vector2 textDimensions = DrawTextBox(
                        spriteBatch,
                        node.GetCurrentMessage(),
                        xOffset + spriteSize + Margin,
                        yOffset,
                        TextWidth,
                        highlightColor: node == choices.GetCurrentNode() ? selectionColor : null,
                        highlightMargin: Margin,
                        borderThickness: node == choices.GetCurrentNode() ? 5 : 0);
                    yOffset += (int)textDimensions.Y + 2 * Margin;
                }
            }
        }

        public static void DrawResource(Resource resource, int partyNumber)
            // Draw a single resource bar
        {
            // Figure out the resource specific information
            var margin = 16;
            var barHeight = 20;
            var barColor = new Vector3(0, 0, 0);
            var height = margin + partyNumber * (margin + 2 * barHeight);            
            switch (resource.Name)
            {
                case "Health":
                    barColor = new Vector3(255, 0, 0);
                    break;
                case "Electricity":
                    barColor = new Vector3(0, 0, 255);
                    height += barHeight;
                    break;
            }

            // Draw the bar itself
            var radius = 0;
            var barWidth = 256;
            DrawRoundedRectangle(
                new Rectangle(
                    margin,
                    height,
                    (int)(barWidth * resource.Current / resource.Max),
                    barHeight),
                radius,
                new Color((int)barColor.X, (int)barColor.Y, (int)barColor.Z, 75));
            DrawRoundedRectangle(
                new Rectangle(
                    margin,
                    height,
                    barWidth,
                    barHeight),
                radius,
                borderWidth: 1);
        }

        public static VertexPositionColorTexture[] GetComponentVertices(
            Component component, 
            Vector3 size, 
            Vector3 normalizationDirection, 
            Vector3 normalizationOffset)
        {
            var vertices = commonVertices["Component"];
            var bottomLeft = component.Position;
            var rotationPoint = bottomLeft + component.RotationPoint * component.SourceObject.Size;
            var rotationOffsets = MathTools.PointRotatedAroundPoint(
                component.RelativeRotation + component.RotationOffset,
                Vector3.Zero,
                new Vector3(0, 0, component.Direction - MathHelper.PiOver2));

            // Bottom left
            vertices[0].Position = MathTools.PointRotatedAroundPoint(
                bottomLeft,
                rotationPoint,
                rotationOffsets);
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
                rotationOffsets);
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
                rotationOffsets);
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
                rotationOffsets);
            vertices[4].Position = MathTools.PointRotatedAroundPoint(
                vertices[4].Position,
                component.SourceObject.Center,
                normalizationDirection);
            vertices[4].Position += normalizationOffset;
            vertices[5].Position = vertices[2].Position;

            return vertices;
        }
        
        public static void DrawShadows()
        // Unused but leaving in for future reference
        {
            foreach (var gameObject in Globals.Objects.OrderBy(i => -i.Position.Y))
            {
                // Draw components for the object
                foreach (var component in gameObject.Components.OrderBy(i => -i.Value.Position.Y))
                {
                    DrawComponentShadow(component.Value, gameObject.Size * component.Value.Size, MathHelper.PiOver4);
                }
            }
        }

        public static void DrawComponentShadow(
                Component component,
                Vector3 size,
                float direction)
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

        public static void DrawGameObjects()
        {
            foreach (var gameObject in Globals.Objects.OrderBy(i => -i.Position.Y))
            {
                // Draw components for the object
                foreach (var component in gameObject.Components.OrderBy(i => -i.Value.Position.Y))
                {
                    DrawComponent(component.Value, gameObject.Size * component.Value.Size);
                }
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
            Color leftBrightness = Color.White;
            Color rightBrightness = Color.White;
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
        
        // Draw all tiles for a Zone
        public static void DrawTiles()
        {
            GameControl.tileEffect.Texture = Globals.Zone.TileManager.TileSheet.Sprite;  // No performance impact
            foreach (var pass in GameControl.tileEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0, Globals.Zone.TileManager.Indices.Length / 3);
            }
        }

        // Draws a light source
        public static void DrawLightSource(
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

        public static void ComputeLighting()
        {
                        // Draw the light effects from each object into their own renderTargets
            foreach (var lightSource in Globals.Objects.OrderBy(i => -i.Position.Y))
            {
                var brightness = 25 * lightSource.Brightness.Length();
                if (brightness != 0)
                {
                    // Draw the light
                    DrawLightSource(lightSource.Center, brightness, new Color(lightSource.Brightness));
                }
            }
        }

        public static void DrawLighting()
        {
            GameControl.spriteBatch.Draw(
                GameControl.lightingRenderTarget, GameControl.screenSize, Color.White);
        }

        public static VertexPositionColorTexture[] GetVertices(string side, Color color)
        {
            var vertices = new VertexPositionColorTexture[6];

            vertices[0].TextureCoordinate = new Vector2(0, 1); // Bottom left
            vertices[1].TextureCoordinate = new Vector2(0, 0); // Top left
            vertices[2].TextureCoordinate = new Vector2(1, 1); // Bottom right
            vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
            vertices[4].TextureCoordinate = new Vector2(1, 0); // Top right
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

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

        // Draws the creative mode UI, including tile previews
        public static void DrawCreativeUI(SpriteBatch spriteBatch)
        {
            // Draw the object on the left
            var tileSheet = Globals.Zone.TileManager.TileSheet;
            DrawSprite(
                spriteBatch,
                tileSheet.Sprite,
                new Vector2(Globals.Resolution.X / 2 - 125, 10),
                new Vector2(Globals.Resolution.X / 2 - 75, 60),
                tileSheet.TextureCoordinatesFromId(
                    (int)MathTools.Mod(Globals.CreativeObjectIndex - 1, tileSheet.Tiles.Count)));

            // Draw the object in the middle
            DrawSprite(
                spriteBatch,
                tileSheet.Sprite,
                new Vector2(Globals.Resolution.X / 2 - 50, 10),
                new Vector2(Globals.Resolution.X / 2 + 50, 100),
                tileSheet.TextureCoordinatesFromId(Globals.CreativeObjectIndex));

            // Draw the object on the right
            DrawSprite(
                spriteBatch,
                tileSheet.Sprite,
                new Vector2(Globals.Resolution.X / 2 + 75, 10),
                new Vector2(Globals.Resolution.X / 2 + 125, 60),
                tileSheet.TextureCoordinatesFromId(
                    (int)MathTools.Mod(Globals.CreativeObjectIndex - 1, tileSheet.Tiles.Count)));
        }
    }
}