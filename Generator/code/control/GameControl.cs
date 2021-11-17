using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Newtonsoft.Json;

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
        // TODO: Add setters to Resolution to change this during runtime
        public static Rectangle screenSize = new Rectangle(0, 0, (int)Globals.Resolution.X, (int)Globals.Resolution.Y);
        private static SpriteBatch spriteBatch;
        public static LilyPath.DrawBatch drawBatch;

        private static GameScreen currentScreen = GameScreen.WalkingAround;
        public static GameScreen CurrentScreen
        {
            get => currentScreen;
            set
            {
                Globals.Log("Now on the " + value + " screen");
                currentScreen = value;
            }
        }

        public enum GameScreen
        {
            WalkingAround,
            Conversation,
            CombatOptionSelector,
            AbilitySelector,
            AbilityTargeter,
            ItemSelector,
            ItemTargeter,
            CombatMovement,
            CombatLookAround,
            CombatPlayEvents
        }

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
            
            Globals.Serializer = new JsonSerializer { Formatting = Globals.IsRelease ? Formatting.None : Formatting.Indented};

            // Set up the GraphicsDevice, which is used for all drawing
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

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
            
            Globals.DefaultTileSheet = new TileSheet(
                "Tiles/tiles",
                new List<Tile>
                {
                    // Can avoid passing this in if I stop using a bunch of empty space in my
                    new Tile("clay", 7,  0, 0, 11),
                    new Tile("grass", 5,  1, 16, 27),
                    new Tile("sand", 3,  2, 32, 43),
                    new Tile("ice", 11,  4, 48, 59, .01f),
                    new Tile("lava", 8,  3, 64, 75),
                    new Tile("wall", 10,  80, 80),
                    new Tile("snow", 3,  5, 96, 107, .1f)
                });
            
            Globals.DefaultSpriteSheet = new SpriteSheet(
                "Sprites/spriteSheet",
                new List<Sprite>
                {
                    new Sprite("PinkArm", false, 1, 2, 0, 0),
                    new Sprite("NinjaArm", false, 1, 2, 1, 0),
                    new Sprite("PurpleArm", false, 1, 2, 2, 0),
                    new Sprite("PinkBody", true, 2, 2, 0, 2),
                    new Sprite("NinjaBody", true, 2, 2, 2, 2),
                    new Sprite("PurpleBody", true, 2, 2, 4, 2),
                    new Sprite("Hand", false, 1, 1, 0, 10),
                    new Sprite("MetalBall", false, 1, 1, 1, 10),
                    new Sprite("PinkLeg", false, 1, 1, 0, 11),
                    new Sprite("NinjaLeg", false, 1, 1, 1, 11),
                    new Sprite("PurpleLeg", false, 1, 1, 2, 11),
                    new Sprite("NormalEyes", true, 4, 4, 0, 14, directions: new List<string>{"Front", "Left", "Right"}),
                    new Sprite("HurtEyes", true, 4, 4, 4, 14, directions: new List<string>{"Front", "Left", "Right"}),
                    new Sprite("Goggles", true, 4, 4, 8, 14, directions: new List<string>{"Front", "Left", "Right"}),
                    new Sprite("GirlHead", true, 4, 4, 0, 26),
                    new Sprite("NinjaHead", true, 4, 4, 4, 26),
                    new Sprite("SamuraiHead", true, 5, 4, 8, 26),
                });
            Globals.SpriteSheet = Globals.DefaultSpriteSheet;
            
            // TODO: This will be replaced once we have an intro screen
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
            // Get input for character
            Input.Update();

            // Launch any scheduled events
            Timing.Update();
            
            // Update the world in response to the above
            Globals.GameObjectManager.Update();
            Globals.TileManager.Update();

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

            // Draw all game elements
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            Drawing.DrawTiles();
            Drawing.DrawGameObjects();
            
            spriteBatch.Begin();
            drawBatch.Begin();
            Drawing.DrawResourceBars();
            drawBatch.End();
            if (Globals.CreativeMode) Drawing.DrawCreativeUI(spriteBatch);
            Drawing.DrawTextBoxes(spriteBatch);

            switch (CurrentScreen)
            {
                case GameScreen.WalkingAround:
                    // this line is intentionally left blank
                    break;
                case GameScreen.Conversation:
                    Drawing.DrawConversation(spriteBatch);
                    break;
                case GameScreen.CombatOptionSelector:
                    Drawing.DrawSelector(spriteBatch, Selectors.CombatScreenSelector);
                    break;
                case GameScreen.AbilitySelector:
                    Drawing.DrawSelector(spriteBatch, Selectors.AbilitySelector);
                    break;
                case GameScreen.AbilityTargeter:
                    break;
                case GameScreen.ItemSelector:
                    Drawing.DrawSelector(spriteBatch, Selectors.ItemSelector);
                    break;
                case GameScreen.ItemTargeter:
                    break;
                case GameScreen.CombatMovement:
                    break;
                case GameScreen.CombatLookAround:
                    break;
                case GameScreen.CombatPlayEvents:
                    // this line is intentionally left blank
                    break;
                default:
                    throw new Exception("uh oh!");
            }
            
            if (!Globals.IsRelease)
            {
                Timing.NumDraws++;
                Timing.FrameTimes[MathTools.Mod(Timing.NumDraws, Timing.FrameTimes.Length)] = DateTime.Now;
                Drawing.DrawFPS(spriteBatch);
                Drawing.DrawPlayerCoordinates(spriteBatch);
                Drawing.DrawLogs(spriteBatch);
            }
            
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}