using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;
using System.IO;

namespace Generator
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class GameControl : Game
    {
        // Generics
        public static GraphicsDeviceManager graphics;

        // For drawing... stuff
        public static Camera camera;
        public static BasicEffect effect;
        public static RenderTarget2D tileRenderTarget;
        public static RenderTarget2D shadowRenderTarget;
        public static RenderTarget2D objectRenderTarget;
        public static Dictionary<GameObject, RenderTarget2D> lightingRenderTargets = new Dictionary<GameObject, RenderTarget2D>();
        public static BlendState lightingBlendState;
        public static BlendState lightingLayerBlendState;
        public static Rectangle screenSize = new Rectangle(0, 0, (int)Globals.Resolution.X, (int)Globals.Resolution.Y);
        public static SpriteBatch spriteBatch;
        public static LilyPath.DrawBatch drawBatch;

        public GameControl()
        {
            // Setup stuff
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = (int) Globals.Resolution.Y,
                PreferredBackBufferWidth = (int) Globals.Resolution.X,
                IsFullScreen = false
            };
            Content.RootDirectory = "Content";
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Delete whatever's lingering in the tmp directory
            if (Directory.Exists(Saving.TempSaveDirectory))
            {
                Directory.Delete(Saving.TempSaveDirectory, true);
            }

            // Create common vertices for reuse

            // Set up the GraphicsDevice, which is used for all drawing
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            tileRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            shadowRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            objectRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            lightingBlendState = new BlendState { ColorDestinationBlend = Blend.InverseDestinationColor };
            lightingLayerBlendState = new BlendState { ColorSourceBlend = Blend.DestinationColor };

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            drawBatch = new LilyPath.DrawBatch(GraphicsDevice);
            effect = new BasicEffect(GraphicsDevice) { 
                TextureEnabled = true, 
                VertexColorEnabled = true};

            Globals.ContentManager = Content;

            camera = new Camera();

            Globals.Zone = Zone.Load(Globals.ZoneName.Value);
            Saving.PopulateSaveKeywords();
            Timing.AddEvent(300, Saving.Autosave);

            IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load in the textures
            Globals.WhiteDot = Content.Load<Texture2D>("Sprites/white_dot");
            Globals.LightTexture = Content.Load<Texture2D>("Sprites/light");

            // Load in the fonts
            Globals.Font = Content.Load<SpriteFont>("Fonts/Score");

        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            // Content.Unload();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Launch any scheduled events
            Timing.Update();

            // Get input for character
            Input.GetInput(Globals.Player);

            // Update the GameObjects
            foreach (var gameObject in Globals.Objects.ToList())
                gameObject.Update();

            // Keep the camera focused on the player
            camera.Update();

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            // Pre-compute the lighting layer
            GraphicsDevice.SetRenderTarget(shadowRenderTarget);
            GraphicsDevice.Clear(Color.Black);

            // Draw the light effects from each object into their own renderTargets
            foreach (var lightSource in Globals.Objects.OrderBy(i => -i.Position.Y))
            {
                var brightness = 25 * lightSource.Brightness.Length();
                if (brightness != 0)
                {
                    // Give it a unique renderTarget
                    if (!lightingRenderTargets.ContainsKey(lightSource))
                    {
                        lightingRenderTargets[lightSource] = new RenderTarget2D(
                            GraphicsDevice,
                            GraphicsDevice.PresentationParameters.BackBufferWidth,
                            GraphicsDevice.PresentationParameters.BackBufferHeight,
                            false,
                            GraphicsDevice.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
                    }
                    GraphicsDevice.SetRenderTarget(lightingRenderTargets[lightSource]);
                    GraphicsDevice.Clear(Color.Transparent);

                    // Draw the light
                    Drawing.DrawLight(lightSource.Center, brightness, Color.White);

                    // Draw the shadows
                    foreach (var Object in Globals.Objects.Where(i => i.CastsShadow))
                    {
                        if (Object != lightSource)
                        {
                            var lightAngle = (float)MathTools.Angle(Object.Center, lightSource.Center);
                            var originalDirection = Object.Direction;
                            Object.Direction = MathTools.Mod(-Object.Direction + lightAngle + MathHelper.PiOver2, MathHelper.TwoPi);
                            foreach (var component in Object.Components.Values.OrderBy(i => -i.Position.Y).Where(i => i.CastsShadow))
                            {
                                Drawing.DrawComponentShadow(
                                    component,
                                    Object.Size * component.Size,
                                    lightAngle,
                                    lightSource);
                            }
                            Object.Direction = originalDirection;
                        }
                    }
                }
            }

            // Draw the tile layer
            GraphicsDevice.SetRenderTarget(tileRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            for (var x = (int)camera.VisibleArea.Left; x <= (int)camera.VisibleArea.Right; x++)
            {
                for (var y = (int)camera.VisibleArea.Top; y <= (int)camera.VisibleArea.Bottom; y++)
                {
                    Drawing.DrawTile(x, y);
                }
            }

            // Draw the GameObjects
            GraphicsDevice.SetRenderTarget(objectRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            foreach (var gameObject in Globals.Objects.OrderBy(i => -i.Position.Y))
            {
                // Draw components for the object
                foreach (var component in gameObject.Components.OrderBy(i => -i.Value.Position.Y))
                {
                    Drawing.DrawComponent(
                        component.Value,
                        gameObject.Size * component.Value.Size);
                }
            }
            spriteBatch.End();

            // Pre-compute the lighting layer
            GraphicsDevice.SetRenderTarget(shadowRenderTarget);
            spriteBatch.Begin(blendState: lightingBlendState);
            GraphicsDevice.Clear(Color.Black);
            foreach (var lightingRenderTarget in lightingRenderTargets)
            {
                if (Globals.Zone.GameObjects.Objects.ContainsKey(lightingRenderTarget.Key.ID))
                {
                    spriteBatch.Draw(lightingRenderTarget.Value, screenSize, new Color(lightingRenderTarget.Key.Brightness));
                }
            }
            spriteBatch.End();

            // Draw the tile layer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.AliceBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(tileRenderTarget, screenSize, Color.White);
            spriteBatch.End();

            // Draw the lighting layer
            spriteBatch.Begin(blendState: lightingLayerBlendState);
            spriteBatch.Draw(shadowRenderTarget, screenSize, Color.White);
            spriteBatch.End();

            // Draw the object layer
            spriteBatch.Begin();
            spriteBatch.Draw(objectRenderTarget, screenSize, Color.White);
            spriteBatch.End();

            // Draw the UI layer
            if (Globals.CurrentConversation != null)
            {
                Drawing.DrawConversation(spriteBatch);
            }
            else
            {
                // Draw the resource bars
                drawBatch.Begin();
                var partyMembers = Globals.Party.Value.GetMembers().ToList();
                for (int i = 0; i < partyMembers.Count; i++)
                {
                    var gameObject = partyMembers[i];
                    Drawing.DrawResource(gameObject.Health, i);
                    if (gameObject.Electricity.Max > 0)
                        Drawing.DrawResource(gameObject.Electricity, i);
                }
                drawBatch.End();

                // Show which tiles are selected
                spriteBatch.Begin();
                if (Globals.CreativeMode)
                {
                    Drawing.DrawCreativeUI(spriteBatch);
                }
                spriteBatch.End();

                // Show all text blurbs
                foreach (var gameObject in Globals.Objects.OrderBy(i => -i.Position.Y))
                {
                    if (gameObject.IsSaying != null)
                    {
                        var textBoxCenter = MathTools.PixelsFromPosition(
                            gameObject.Center + new Vector3(0, 3, 0));
                        Drawing.DrawTextBox(
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

            // Draw FPS counter
            if (Timing.ShowFPS)
            {
                Timing.NumDraws++;
                Timing.FrameTimes[(int)MathTools.Mod(Timing.NumDraws, Timing.FrameTimes.Length)] = DateTime.Now;
                spriteBatch.Begin();
                Drawing.DrawFPS(spriteBatch);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}