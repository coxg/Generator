using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace Generator
{

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameControl : Game
    {
        // Generics
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Player
        private GameObject player;
        private GameObject terrain1;
        private GameObject terrain2;

        public GameControl()
        {

            // Populate global variables
            Globals.Populate();

            // Setup stuff
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = (int)Globals.Resolution.Y,
                PreferredBackBufferWidth = (int)Globals.Resolution.X,
            };
            graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load in the sprites
            Globals.WhiteDot = Content.Load<Texture2D>("Sprites/white_dot");

            // Load in the fonts
            Globals.Font = Content.Load<SpriteFont>("Fonts/Score");

            Globals.Content = Content;

            // Create player
            player = new GameObject(
                disposition: "Party", 
                x:-20, 
                y:3, 
                name:"Niels", 
                width:1, 
                height:1, 
                strength:10, 
                standingSpriteFile:"Sprites/face",
                weapon: new Weapon(
                    name: "Sword", 
                    type: "Cut", 
                    damage: 10, 
                    spriteFile: "Sprites/sword"));

            // Create terrain
            terrain1 = new GameObject(disposition: "Party", x: 5, y: 6, name: "angry terrain", width:1, avatarFile: "Sprites/angry");
            terrain1.Activate = delegate ()
            {
                terrain1.Say("Check it out I do something weird");
                terrain1.Say("Did you see how weird that was?!");
                GameObject terrain3 = new GameObject(disposition: "Party", x: 10, y: 10, name: "big terrain", width: 5, height: 5);
                terrain3.Activate = delegate ()
                {
                    terrain3.Say("I don't do anything weird.");
                    terrain3.Say("...I'm just really fat.");
                };
            };
            terrain2 = new GameObject(disposition: "Party", x: 5, y: 9, name:"medium terrain", width:2, height:2);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            // Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // This should happen before anything else
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Get input for character
            Input.GetInput(player);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Initialize
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Queue queue = new Queue();
            queue = Queue.Synchronized(queue);
            spriteBatch.Begin(SpriteSortMode.BackToFront, null);

            // Multithreaded mode
            if (Globals.Multithreaded)
            {
                Drawing.GetRectanglesParallel(spriteBatch, queue, Drawing.SubmitToQueue);
                Drawing.DrawFromBatch(spriteBatch, queue);
            }

            // Singlethreaded mode
            else
            {
                Drawing.GetRectanglesNonParallel(spriteBatch, queue, Drawing.SubmitToSpriteBatch);
            }

            // Draw grid lines
            if (Globals.GridAlpha > 0)
            {
                Drawing.DrawGridLines(spriteBatch);
            }

            // Draw text box
            if (Globals.DisplayTextQueue.Count != 0)
            {
                Drawing.DrawTextBox(spriteBatch);
            }

            // Draw everything in the queue
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
