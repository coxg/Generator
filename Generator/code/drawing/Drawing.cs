﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator
{
    public static class Drawing
    {
        private const int Margin = 16;
        private const int SpriteSize = 256;

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
            int y = (int)Globals.Resolution.Y - 25;
            for (int i = Globals.RecentLogs.Count - 1; i >= 0; i--)
            {
                var logLine = Globals.RecentLogs[i];
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

        private static Vector2 DrawTextBox(
            SpriteBatch spriteBatch, string text, int x, int y, int maxWidth, Color? color=null, bool drawText=true, 
            string align="left", Color? highlightColor=null, int highlightMargin = 0, int borderThickness = 0)
        // Use this to draw any text boxes.
        // This is super overloaded - also draws speech bubbles, for example. Should this be split up? Or renamed?
        {
            void DrawText(
                string _text, int _x, int _y, Vector2 _dimensions)
            {
                if (align == "right")
                {
                    _x += (int)(maxWidth - _dimensions.X);
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
                    spriteBatch.DrawString(
                        Globals.Font,
                        _text,
                        new Vector2(_x, _y),
                        color ?? Color.White);
                }
            }

            // Super janky, but draw the border and THEN the background and THEN the text
            if (drawText && highlightColor != null)
            {
                DrawTextBox(spriteBatch, text, x, y, maxWidth, drawText: false, align: align,
                    highlightMargin: highlightMargin, borderThickness: borderThickness);
                DrawTextBox(spriteBatch, text, x, y, maxWidth, drawText: false, align: align, 
                    highlightColor: highlightColor, highlightMargin: highlightMargin);
                return DrawTextBox(spriteBatch, text, x, y, maxWidth, color, true, align, 
                    highlightMargin: highlightMargin);
            }

            GameControl.drawBatch.Begin();

            // Get the dimensions of the text
            var dimensions = Globals.Font.MeasureString(text);

            // If we are too large then walk through the words one by one, appending as we go
            if (dimensions.X > maxWidth)
            {
                var maxX = 0;
                var totalY = 0;
                var words = text.Split(' ');
                var currentLine = "";
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
                        DrawText(currentLine, x, y + totalY, currentLineDimensions);
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
                    DrawText(currentLine, x, y + totalY, finalLineDimensions);
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
                DrawText(text, x, y, dimensions);
            }

            GameControl.drawBatch.End();
            return dimensions;
        }

        private static Vector2 GetOptionsDimensions(IEnumerable<string> options, int maxWidth)
        {
            float width = 0;
            float height = -Margin;
            foreach (var option in options)
            {
                var (x, y) = DrawTextBox(null, option, 0, 0, maxWidth, drawText: false);
                width = Math.Max(width, x);
                height += y + Margin;
            }
            return new Vector2(width, height);
        }

        public static void DrawConversation(SpriteBatch spriteBatch)
        {
            var choices = Globals.CurrentConversation.CurrentChoices;
            DrawOptions(
                spriteBatch, choices.Nodes.Select(x => x.Text[0]), choices.GetCurrentNode().Text[0], choices.ChoiceSelected, 
                choices.GetCurrentNode().GetCurrentSpeaker() == Globals.CurrentConversation.SourceObject ? "right" : "left");
        }

        public static void DrawTargeter(SpriteBatch spriteBatch, Targeter targeter)
        {
            foreach (var target in targeter.GetTargets())
            {
                var bottomLeft = MathTools.PixelsFromPosition(target);
                var targetColor = Color.FromNonPremultiplied(255, 0, 0, 150);
                spriteBatch.Draw(
                    Globals.WhiteDot, 
                    new Rectangle(
                        (int)bottomLeft.X, 
                        (int)bottomLeft.Y, 
                        (int)GameControl.camera.SquareSize, 
                        (int)GameControl.camera.SquareSize), 
                    null, 
                    targetColor, 
                    0f, 
                    new Vector2(0, 0), 
                    SpriteEffects.None, 
                    .05f);
            }
        }

        public static void DrawSelector<T>(SpriteBatch spriteBatch, Selector<T> selector)
        {
            DrawOptions(spriteBatch, selector.Options.Select(x => x.ToString()), selector.GetSelection().ToString(), false);
        }

        private static void DrawOptions(SpriteBatch spriteBatch, IEnumerable<string> options, string selection,
            bool responseOverride, string alignment = null)
        {
            // Draw the background
            var backGroundColor = Color.FromNonPremultiplied(0, 0, 0, 150);
            spriteBatch.Draw(
                Globals.WhiteDot, new Rectangle(
                    0, 0, (int)Globals.Resolution.X, (int)Globals.Resolution.Y),
                null, backGroundColor, 0f, new Vector2(0, 0), SpriteEffects.None, .05f);

            // Get dimensions for the different conversation options
            var maxWidth = (int) Globals.Resolution.X - 4 * Margin - 2 * SpriteSize;
            var optionsDimensions = GetOptionsDimensions(options, maxWidth);
            var textWidth = (int) optionsDimensions.X;
            var textBoxHeight = (int) optionsDimensions.Y + 2 * Margin;

            // Draw the text itself
            var xOffset = (maxWidth - textWidth) / 2 + Margin;
            Color? selectionColor = Color.FromNonPremultiplied(105, 69, 169, 255);
            if (responseOverride)
            {
                // If we've selected a choice then just draw that choice
                var textDimensions = DrawTextBox(spriteBatch, selection, 0, 0, textWidth, drawText: false);
                DrawTextBox(spriteBatch, selection, xOffset + SpriteSize + Margin,
                    (int)(Globals.Resolution.Y - textDimensions.Y) / 2, textWidth, align: alignment,
                    highlightColor: selectionColor, highlightMargin: Margin, borderThickness: 5);
            }

            else
            {
                // If we haven't selected a choice yet then show all available choices
                var yOffset = (int)(Globals.Resolution.Y - textBoxHeight) / 2 + Margin;
                foreach (var option in options)
                {
                    // Draw the choice
                    var textDimensions = DrawTextBox(spriteBatch, option, xOffset + SpriteSize + Margin, yOffset,
                        textWidth, highlightColor: option == selection ? selectionColor : null, highlightMargin: Margin,
                        borderThickness: option == selection ? 5 : 0);
                    yOffset += (int)textDimensions.Y + 2 * Margin;
                }
            }
        }

        private static void DrawResource(Resource resource, int partyNumber)
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
                case "Mana":
                    barColor = new Vector3(0, 0, 255);
                    height += barHeight;
                    break;
            }

            // Draw the bar itself
            var radius = 0;
            var barWidth = 256;
            DrawRoundedRectangle(
                new Rectangle(margin, height, (int)(barWidth * resource.Current / resource.Max), barHeight),
                radius,
                new Color((int)barColor.X, (int)barColor.Y, (int)barColor.Z, 75));
            DrawRoundedRectangle(
                new Rectangle(margin, height, barWidth, barHeight),
                radius,
                borderWidth: 1);
        }

        private static void AddComponentToVertices(
            Component component, 
            Vector3 normalizationDirection, 
            Vector3 normalizationOffset,
            List<VertexPositionColorTexture> vertices,
            SpriteSheet spriteSheet,
            Color color)
        {
            var textureCoordinates = spriteSheet.GetTextureCoordinates(component);
            if (textureCoordinates.Length == 0)
            {
                return;
            }
            
            var cornerVertices = new VertexPositionColorTexture[4];
            cornerVertices[0].Position = component.Position;
            cornerVertices[1].Position = new Vector3(
                component.Position.X,
                component.Position.Y,
                component.Position.Z + component.Size.Z);
            cornerVertices[2].Position = new Vector3(
                component.Position.X + component.Size.X,
                component.Position.Y,
                component.Position.Z);
            cornerVertices[3].Position = new Vector3(
                component.Position.X + component.Size.X,
                component.Position.Y,
                component.Position.Z + component.Size.Z);
            
            var rotationPoint = component.Position + component.RotationPoint * component.Size;
            var rotationAxis = MathTools.PointRotatedAroundPoint(
                component.RelativeRotation + component.RotationOffset,
                Vector3.Zero,
                new Vector3(0, 0, component.Direction - MathHelper.PiOver2));
            for (var i = 0; i < 4; i++)
            {
                cornerVertices[i].Position = MathTools.PointRotatedAroundPoint(
                    cornerVertices[i].Position,
                    rotationPoint,
                    rotationAxis);
                cornerVertices[i].Position = MathTools.PointRotatedAroundPoint(
                    cornerVertices[i].Position,
                    component.SourceObject.Center,
                    normalizationDirection);
                cornerVertices[i].Position += normalizationOffset;
                cornerVertices[i].Position.Y += component.SourceObject.Position.Z;
                cornerVertices[i].Position.Z = 0;
                cornerVertices[i].Color = color;
                cornerVertices[i].TextureCoordinate = textureCoordinates[i];
            }

            vertices.Add(cornerVertices[0]);  // bottom left
            vertices.Add(cornerVertices[1]);  // top left
            vertices.Add(cornerVertices[2]);  // bottom right
            vertices.Add(cornerVertices[1]);  // top left
            vertices.Add(cornerVertices[3]);  // top right
            vertices.Add(cornerVertices[2]);  // bottom right
        }
        
        public static void DrawShadows()
        {
            // TODO: Just draw little shadow circles under each character
        }

        public static void DrawGameObjects()
        {
            var vertices = new List<VertexPositionColorTexture>();
            foreach (var gameObject in Globals.GameObjectManager.GetVisible().OrderBy(i => -i.Position.Y))
            {
                foreach (var component in gameObject.Components.OrderBy(i => -i.Value.Center.Y))
                {
                    AddComponentToVertices(
                        component.Value, 
                        new Vector3(-MathHelper.PiOver2, 0, 0), 
                        Vector3.Zero,
                        vertices,
                        Globals.SpriteSheet,
                        Color.White);
                }
            }
            DrawVertices(vertices, Globals.SpriteSheet);
        }

        private static void DrawVertices(List<VertexPositionColorTexture> vertices, SpriteSheet spriteSheet)
        {
            var vertexArray = vertices.ToArray();
            if (vertexArray.Length == 0)
            {
                return;
            }
            GameControl.effect.Texture = spriteSheet.Texture;
            foreach (var pass in GameControl.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList, vertexArray, 0, vertexArray.Length / 3);
            }
        }

        // Draw all tiles for a Zone
        public static void DrawTiles()
        {
            var dynamicVertexArray = Globals.TileManager.DynamicVertices.ToArray();
            GameControl.tileEffect.Texture = Globals.TileManager.TileSheet.Texture;  // No performance impact
            foreach (var pass in GameControl.tileEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GameControl.graphics.GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0, Globals.TileManager.Indices.Length / 3);
                if (dynamicVertexArray.Length > 0)
                {
                    GameControl.graphics.GraphicsDevice.DrawUserPrimitives(
                        PrimitiveType.TriangleList, dynamicVertexArray, 0, dynamicVertexArray.Length / 3);
                }
            }
        }

        // Draws the creative mode UI, including tile previews
        public static void DrawCreativeUI(SpriteBatch spriteBatch)
        {
            var tileSheet = Globals.TileManager.TileSheet;
            var tileIndex = tileSheet.Tiles.IndexOf(Selectors.CreativeTileSelector.GetSelection());

            // Draw the object on the left
            DrawSprite(
                spriteBatch,
                tileSheet.Texture,
                new Vector2(Globals.Resolution.X / 2 - 125, 10),
                new Vector2(Globals.Resolution.X / 2 - 75, 60),
                tileSheet.TextureCoordinatesFromId(
                    MathTools.Mod(tileIndex - 1, tileSheet.Tiles.Count)));

            // Draw the object in the middle
            DrawSprite(
                spriteBatch,
                tileSheet.Texture,
                new Vector2(Globals.Resolution.X / 2 - 50, 10),
                new Vector2(Globals.Resolution.X / 2 + 50, 100),
                tileSheet.TextureCoordinatesFromId(tileIndex));

            // Draw the object on the right
            DrawSprite(
                spriteBatch,
                tileSheet.Texture,
                new Vector2(Globals.Resolution.X / 2 + 75, 10),
                new Vector2(Globals.Resolution.X / 2 + 125, 60),
                tileSheet.TextureCoordinatesFromId(
                    MathTools.Mod(tileIndex + 1, tileSheet.Tiles.Count)));
        }
        
        public static void DrawTextBoxes(SpriteBatch spriteBatch) {
            // Show all text blurbs
            foreach (var gameObject in Globals.GameObjectManager.GetVisible().OrderBy(i => -i.Position.Y))
            {
                if (gameObject.IsSaying != null)
                {
                    var textBoxCenter = MathTools.PixelsFromPosition(
                        gameObject.Center + new Vector3(0, 3, 0));
                    DrawTextBox(
                        spriteBatch,
                        gameObject.IsSaying,
                        (int)textBoxCenter.X,
                        (int)textBoxCenter.Y,
                        300,
                        Color.White,
                        align: "center",
                        borderThickness: 5,
                        highlightColor: Color.MediumPurple,
                        highlightMargin: 16);
                }
            }
        }

        public static void DrawResourceBars()
        {
            var partyMembers = Globals.Party.Value.GetMembers().ToList();
            for (int i = 0; i < partyMembers.Count; i++)
            {
                var gameObject = partyMembers[i];
                DrawResource(gameObject.Health, i);
                if (gameObject.Mana.Max > 0) DrawResource(gameObject.Mana, i);
            }
        }
    }
}