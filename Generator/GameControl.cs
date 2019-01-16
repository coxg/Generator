using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

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
        public static Rectangle screenSize = new Rectangle(0, 0, (int)Globals.Resolution.X, (int)Globals.Resolution.Y);
        public static List<BlendState> lightingBlendStates = new List<BlendState>();

        // Player
        public SpriteBatch spriteBatch;
        private GameObject terrain1;
        private GameObject terrain2;

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
            lightingBlendState = new BlendState
            {
                //ColorDestinationBlend = Blend.InverseSourceAlpha,
                //ColorDestinationBlend = Blend.InverseSourceColor,
                ColorDestinationBlend = Blend.InverseDestinationColor,
                //ColorSourceBlend = Blend.BlendFactor,
                //ColorBlendFunction = BlendFunction.ReverseSubtract
                //ColorDestinationBlend = Blend.SourceAlphaSaturation
            };
            lightingBlendStates = new List<BlendState>();
            var ColorBlendFunctions = new List<BlendFunction>
            {
                //BlendFunction.Subtract,
                BlendFunction.Add,
                //BlendFunction.ReverseSubtract
            };
            var ColorDestinationBlends = new List<Blend>
            {
                //Blend.DestinationAlpha, // Close
                //Blend.DestinationColor,
                //Blend.InverseBlendFactor,
                Blend.InverseDestinationAlpha,
                //Blend.InverseDestinationColor,
                //Blend.InverseSourceAlpha,
                //Blend.InverseSourceColor, // Spooky
                //Blend.One, // Spooky
                //Blend.SourceAlpha,
                //Blend.SourceAlphaSaturation, // Spooky
                //Blend.SourceColor,
                //Blend.Zero
            };
            var AlphaSourceBlends = new List<Blend>
            {
                //Blend.BlendFactor,
                Blend.DestinationAlpha,
                Blend.DestinationColor,
                Blend.InverseBlendFactor,
                Blend.InverseDestinationAlpha,
                Blend.InverseDestinationColor,
                Blend.InverseSourceAlpha,
                Blend.InverseSourceColor,
                Blend.One,
                Blend.SourceAlpha,
                Blend.SourceAlphaSaturation,
                Blend.SourceColor,
                Blend.Zero
            };
            var AlphaDestinationBlends = new List<Blend>
            {
                Blend.BlendFactor,
                Blend.DestinationAlpha,
                Blend.DestinationColor,
                Blend.InverseBlendFactor,
                Blend.InverseDestinationAlpha,
                Blend.InverseDestinationColor,
                Blend.InverseSourceAlpha,
                Blend.InverseSourceColor,
                Blend.One,
                Blend.SourceAlpha,
                Blend.SourceAlphaSaturation,
                Blend.SourceColor,
                Blend.Zero
            };
            var AlphaBlendFunctions = new List<BlendFunction>
            {
                BlendFunction.Add,
            };
            var ColorSourceBlends = new List<Blend>
            {
                Blend.BlendFactor,
                Blend.DestinationAlpha,
                Blend.DestinationColor,
                Blend.InverseBlendFactor,
                Blend.InverseDestinationAlpha,
                Blend.InverseDestinationColor,
                Blend.InverseSourceAlpha,
                Blend.InverseSourceColor,
                Blend.One,
                Blend.SourceAlpha,
                Blend.SourceAlphaSaturation,
                Blend.SourceColor,
                Blend.Zero
            };
            foreach (var ColorBlendFunction in ColorBlendFunctions)
            {
                foreach (var ColorDestinationBlend in ColorDestinationBlends)
                {
                    foreach (var AlphaSourceBlend in AlphaSourceBlends)
                    {
                        foreach (var AlphaDestinationBlend in AlphaDestinationBlends)
                        {
                            foreach (var AlphaBlendFunction in AlphaBlendFunctions)
                            {
                                foreach (var ColorSourceBlend in ColorSourceBlends)
                                {
                                    lightingBlendStates.Add(new BlendState
                                    {
                                        ColorBlendFunction = ColorBlendFunction,
                                        ColorDestinationBlend = ColorDestinationBlend,
                                        AlphaSourceBlend = AlphaSourceBlend,
                                        AlphaDestinationBlend = AlphaDestinationBlend,
                                        AlphaBlendFunction = AlphaBlendFunction,
                                        ColorSourceBlend = ColorSourceBlend
                                    });
                                }
                            }
                        }
                    }
                }
            }

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            effect = new BasicEffect(GraphicsDevice) { 
                TextureEnabled = true, 
                VertexColorEnabled = true};

            camera = new Camera();

            Globals.Content = Content;

            Globals.Tiles = new TileManager();
            Globals.GameObjects = new GameObjectManager();

            // Create player
            Globals.Player = Globals.GameObjects.ObjectFromName["Niels"];
            Globals.Player.AddToGrid();

            // Create terrain
            terrain1 = Globals.GameObjects.ObjectFromName["angry terrain"];
            terrain2 = Globals.GameObjects.ObjectFromName["medium terrain"];
            terrain1.AddToGrid();
            terrain2.AddToGrid();

            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            // Load in the sprites
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
            // This should happen before anything else
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Globals.Clock += 1;

            // Get input for character
            Input.GetInput(Globals.Player);

            // Update the GameObjects
            foreach (var ObjectName in new HashSet<string>(Globals.GameObjects.ActiveGameObjects))
                Globals.GameObjects.ObjectFromName[ObjectName].Update();

            // TODO: Why is this broken? 
            // Until fixed, I won't be able to move outside of the starting acres
            // Globals.GameObjects.Update();

            // Update the tiles
            Globals.Tiles.Update();

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
            foreach (var LightSource in Globals.GameObjects.ActiveGameObjects.Select(
                i => Globals.GameObjects.ObjectFromName[i]).OrderBy(i => -i.Position.Y))
            {
                var brightness = 25 * LightSource.Brightness.Length();
                if (brightness != 0)
                {
                    // Give it a unique renderTarget
                    if (!lightingRenderTargets.ContainsKey(LightSource))
                    {
                        lightingRenderTargets[LightSource] = new RenderTarget2D(
                            GraphicsDevice,
                            GraphicsDevice.PresentationParameters.BackBufferWidth,
                            GraphicsDevice.PresentationParameters.BackBufferHeight,
                            false,
                            GraphicsDevice.PresentationParameters.BackBufferFormat,
                            DepthFormat.Depth24);
                    }
                    GraphicsDevice.SetRenderTarget(lightingRenderTargets[LightSource]);
                    GraphicsDevice.Clear(Color.Transparent);

                    // Draw the light
                    Drawing.DrawLight(LightSource.Center, brightness, Color.White);

                    // Draw the shadows
                    foreach (var Object in Globals.GameObjects.ActiveGameObjects.Select(
                        i => Globals.GameObjects.ObjectFromName[i]).OrderBy(i => -i.Position.Y))
                    {
                        if (Object != LightSource)
                        {
                            var lightAngle = (float)MathTools.Angle(LightSource.Center, Object.Center);
                            Object.Direction = MathTools.Mod(-Object.Direction - lightAngle + MathHelper.PiOver2, MathHelper.TwoPi);
                            foreach (var component in Object.ComponentDictionary.OrderBy(i => -i.Value.Position.Y))
                            {
                                Drawing.DrawComponentShadow(
                                    component.Value,
                                    Object.Size * component.Value.Size,
                                    lightAngle);
                            }
                            Object.Direction = MathTools.Mod(-Object.Direction - lightAngle + MathHelper.PiOver2, MathHelper.TwoPi);
                        }
                    }
                }
            }

            // Draw the tile layer
            GraphicsDevice.SetRenderTarget(tileRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            for (var x = (int)camera.ViewMinCoordinates().X; x < (int)camera.ViewMaxCoordinates().X; x++)
            {
                for (var y = (int)camera.ViewMinCoordinates().Y; y < (int)camera.ViewMaxCoordinates().Y; y++)
                {
                    Drawing.DrawTile(x, y);
                }
            }

            // Draw the GameObjects
            GraphicsDevice.SetRenderTarget(objectRenderTarget);
            GraphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            foreach (var Object in Globals.GameObjects.ActiveGameObjects.Select(
                i => Globals.GameObjects.ObjectFromName[i]).OrderBy(i => -i.Position.Y))
            {
                // Draw components for the object
                foreach (var component in Object.ComponentDictionary.OrderBy(i => -i.Value.Position.Y))
                {
                    Drawing.DrawComponent(
                        component.Value,
                        Object.Size * component.Value.Size);
                }

                // Draw resource bars if they're in the party
                if (Object.PartyNumber >= 0)
                {
                    Drawing.DrawResource(spriteBatch, Object.Health, Object.PartyNumber);
                    if (Object.Stamina.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Stamina, Object.PartyNumber);
                    if (Object.Electricity.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Electricity, Object.PartyNumber);
                }
            }
            spriteBatch.End();

            // Pre-compute the lighting layer
            GraphicsDevice.SetRenderTarget(shadowRenderTarget);
            spriteBatch.Begin(blendState: lightingBlendState);
            GraphicsDevice.Clear(Color.Black);
            foreach (var lightingRenderTarget in lightingRenderTargets)
            {
                spriteBatch.Draw(lightingRenderTarget.Value, screenSize, new Color(new Vector4(1, 1, 1, 1f)));
            }
            spriteBatch.End();

            // Draw the tile layer
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.AliceBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(tileRenderTarget, screenSize, Color.White);
            spriteBatch.End();

            // Draw the lighting layer
            Globals.Log(Globals.Clock / 30);
            spriteBatch.Begin(blendState: lightingBlendStates[2]);
            spriteBatch.Draw(shadowRenderTarget, screenSize, new Color(new Vector4(1, 1, 1, .5f)));
            spriteBatch.End();

            // Draw the object layer
            spriteBatch.Begin();
            spriteBatch.Draw(objectRenderTarget, screenSize, Color.White);

            // Draw the selected objects in creative mode
            if (Globals.CreativeMode)
            {
                Drawing.DrawCreativeUI(spriteBatch);
            }

            // Draw text box
            if (Globals.DisplayTextQueue.Count != 0) Drawing.DrawTextBox(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}