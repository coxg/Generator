using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        // Player
        private GameObject player;
        public SpriteBatch spriteBatch;
        private GameObject terrain1;
        private GameObject terrain2;

        public GameControl()
        {
            // Populate global variables
            Globals.Populate();

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
            camera = new Camera(new Vector3(20, -40, 20));

            // We’ll be assigning texture values later
            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load in the sprites
            Globals.WhiteDot = Content.Load<Texture2D>("Sprites/white_dot");
            Globals.Checker = Content.Load<Texture2D>("Sprites/checkerboard");

            // Load in the fonts
            Globals.Font = Content.Load<SpriteFont>("Fonts/Score");

            Globals.Content = Content;

            // Create player
            player = new GameObject(// TODO: This shouldn't be hard coded
                x: 1, y: 3, stamina: 100, strength: 10, speed: 10, perception: 10, name: "Niels", partyNumber: 0, weapon: new Weapon(
                    name: "Sword",
                    type: "Cut",
                    damage: 10,
                    spriteFile: "Sprites/sword"));

            // Create terrain
            terrain1 = new GameObject(avatarFile: "Sprites/angry", width: 1, x: 5, y: 6, name: "angry terrain");
            terrain1.Activate = delegate
            {
                terrain1.Say("Check it out I do something weird");
                terrain1.Say("Did you see how weird that was?!");
                var terrain3 = new GameObject(width: 5, length: 5, height: 5, x: 10, y: 10, name: "big terrain");
                terrain3.Activate = delegate
                {
                    terrain3.Say("I don't do anything weird.");
                    terrain3.Say("...I'm just really fat.");
                };
            };
            terrain2 = new GameObject(width: 2, length: 2, height: 2, x: 5, y: 9, name: "medium terrain");
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
            Input.GetInput(player);

            // Update the GameObjects
            Globals.DeathList = new List<string>();
            foreach (var Object in Globals.ObjectDict) Object.Value.Update();
            foreach (var Name in Globals.DeathList) Globals.ObjectDict.Remove(Name);

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            spriteBatch.Begin(
                SpriteSortMode.BackToFront,
                null,
                SamplerState.LinearWrap);

            // Draw text box
            if (Globals.DisplayTextQueue.Count != 0) Drawing.DrawTextBox(spriteBatch);

            // Draw the grid
            Drawing.DrawTile(
                Globals.Checker,
                new Vector2(0, 0),
                new Vector2(Globals.Grid.GetLength(0), Globals.Grid.GetLength(1)),
                Globals.Grid.GetLength(0));

            // Draw the GameObjects
            foreach (var Object in Globals.ObjectDict)
            {
                Drawing.DrawSprite(
                    Object.Value.Sprite,
                    new Vector3(Object.Value.Position.X, Object.Value.Position.Y, Object.Value.Position.Z),
                    new Vector3(Object.Value.Dimensions.X, Object.Value.Dimensions.Y, Object.Value.Dimensions.Z));

                // Draw resource bars if they're in the party
                if (Object.Value.PartyNumber >= 0)
                {
                    Drawing.DrawResource(spriteBatch, Object.Value.Health, Object.Value.PartyNumber);
                    if (Object.Value.Stamina.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Value.Stamina, Object.Value.PartyNumber);
                    if (Object.Value.Electricity.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Value.Electricity, Object.Value.PartyNumber);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}