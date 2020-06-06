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
        public static BasicEffect tileEffect;
        public static RenderTarget2D lightingRenderTarget;
        public static BlendState lightingBlendState;
        // TODO: Add setters to Resolution to change this during runtime
        public static Rectangle screenSize = new Rectangle(0, 0, (int)Globals.Resolution.X, (int)Globals.Resolution.Y);
        public static SpriteBatch spriteBatch;
        public static LilyPath.DrawBatch drawBatch;
        public static VertexBuffer VertexBuffer;
        public static IndexBuffer IndexBuffer;

        public GameControl()
        {
            // Setup stuff
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = (int)Globals.Resolution.Y,
                PreferredBackBufferWidth = (int)Globals.Resolution.X,
                IsFullScreen = false,
                GraphicsProfile = GraphicsProfile.HiDef
            };
            Content.RootDirectory = Globals.ProjectDirectory + "/Content";
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
            Globals.GraphicsDevice = GraphicsDevice;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            lightingRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            lightingBlendState = new BlendState { ColorSourceBlend = Blend.DestinationColor };

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            drawBatch = new LilyPath.DrawBatch(GraphicsDevice);
            camera = new Camera();
            effect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                Projection = Camera.Projection
            };
            tileEffect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                Projection = Camera.Projection
            };

            Globals.ContentManager = Content;

            // TODO: This will be replaced once we have an intro screen
            Globals.DefaultTileSheet = new TileSheet(
                "Tiles/tiles",
                new List<Tile>
                {
                    // Can avoid passing this in if I stop using a bunch of empty space in my
                    new Tile("clay", 7,  1, 0, 11),
                    new Tile("grass", 5,  0, 16)
                });
            Saving.LoadAreaFromDisk(Globals.ZoneName.Value);
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
            Content.RootDirectory = Globals.ProjectDirectory + "Content";
            
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
            Input.ProcessInput(Globals.Player);

            // Update the GameObjects
            foreach (var gameObject in Globals.GameObjectManager.ObjectList.Values.ToList())
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
            tileEffect.View = camera.View;

            // Pre-compute the lighting layer
            if (Globals.LightingEnabled)
            {
                GraphicsDevice.SetRenderTarget(lightingRenderTarget);
                GraphicsDevice.Clear(Color.Black);
                Drawing.DrawLighting();
            }

            // Draw all game elements
            GraphicsDevice.SetRenderTarget(null);
            Drawing.DrawTiles();
            Drawing.DrawGameObjects();

            // Draw the lighting on top of the other layers
            if (Globals.LightingEnabled)
            {
                spriteBatch.Begin(blendState: lightingBlendState);
                spriteBatch.Draw(lightingRenderTarget, screenSize, Color.White);
                spriteBatch.End();
            }

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
                foreach (var gameObject in Globals.GameObjectManager.ObjectList.Values.OrderBy(i => -i.Position.Y))
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
                Drawing.DrawPlayerCoordinates(spriteBatch);
                spriteBatch.End();
            }

            // Draw the logs
            Drawing.DrawLogs(spriteBatch);

            base.Draw(gameTime);
        }
    }
}