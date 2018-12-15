using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
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
        public static int[,] tileMap;

        // Player
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
            camera = new Camera();

            // We’ll be assigning texture values later
            base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            effect = new BasicEffect(GraphicsDevice)
            {
                TextureEnabled = true,
                // TODO: Figure out why these don't work on Sasha's macbook
                // AmbientLightColor = new Vector3(0.8f, 0.8f, 0.8f),
                // LightingEnabled = true
            };

            // Load in the sprites
            Globals.WhiteDot = Content.Load<Texture2D>("Sprites/white_dot");

            // Load in the fonts
            Globals.Font = Content.Load<SpriteFont>("Fonts/Score");

            // Load in the tiles
            Globals.TileNameToTexture = new Dictionary<string, Texture2D>();
            Globals.TileIndexToTexture = new Dictionary<int, string>();
            var tileIndex = 0;
            foreach (var tileFile in Directory.GetFiles(
                Globals.Directory + "/Content/Tiles", "*.png", SearchOption.TopDirectoryOnly
                ).Select(Path.GetFileName).Select(Path.GetFileNameWithoutExtension))
            {
                Globals.TileNameToTexture.Add(tileFile, Content.Load<Texture2D>("Tiles/" + tileFile));
                Globals.TileIndexToTexture.Add(tileIndex, tileFile);
                tileIndex += 1;
            }

            // Load in the map
            tileMap = new int[100, 100];
            using (var sr = new StreamReader(Globals.Directory + "/Maps/Tiles/map_1_1.csv"))
            {
                var rows = sr.ReadToEnd().Split('\n');
                for (int rowNumber = 0; rowNumber < rows.Length; rowNumber++)
                {
                    var row = rows[rowNumber].Split(',');
                    for (int columnNumber = 0; columnNumber < row.Length; columnNumber++)
                        tileMap[columnNumber, 99 - rowNumber] = int.Parse(row[columnNumber]);
                }
            }

            Globals.Content = Content;

            // Create player
            Globals.Player = new GameObject( // TODO: This shouldn't be hard coded
                x: 50f, y: 50f, stamina: 100, strength: 10, speed: 10, perception: 10, name: "Niels", partyNumber: 0, weapon: new Weapon(
                    name: "Sword",
                    type: "Cut",
                    damage: 10));

            // Create terrain
            terrain1 = new GameObject(spriteFile: "Sprites/angry_boy", x: 55, y: 56, name: "angry terrain");
            terrain1.Activate = delegate
            {
                terrain1.Say("Check it out I do something weird");
                terrain1.Say("Did you see how weird that was?!");
                var terrain3 = new GameObject(width: 5, length: 5, height: 5, x: 60, y: 60, name: "big terrain");
                terrain3.Activate = delegate
                {
                    terrain3.Say("I don't do anything weird.");
                    terrain3.Say("...I'm just really fat.");
                };
            };
            terrain2 = new GameObject(width: 2, length: 2, height: 2, x: 55, y: 59, name: "medium terrain");
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
            Globals.DeathList = new List<string>();
            foreach (var Object in Globals.ObjectDict) Object.Value.Update();
            foreach (var name in Globals.DeathList) Globals.ObjectDict.Remove(name);
            
            // Keep the camera focused on the player
            camera.Position = new Vector3(
                Globals.Player.Center.X, 
                Globals.Player.Center.Y - 10, 
                Globals.Player.Center.Z + 10);
            camera.Target = new Vector3(
                Globals.Player.Center.X,
                Globals.Player.Center.Y - 1,
                Globals.Player.Center.Z);

            base.Update(gameTime);
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            // Draw all tiles which the camera can see
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            for (var x = (int) camera.Position.X - 17; x < (int)camera.Position.X + 17; x++)
            {
                for (var y = (int)camera.Position.Y + 4; y < (int)camera.Position.Y + 2 * camera.Position.Z + 2; y++)
                {
                    Drawing.DrawTile(
                        Globals.TileNameToTexture[Globals.TileIndexToTexture[tileMap[x, y]]],
                        new Vector2(x, y));
                }
            }

            // Draw the GameObjects
            foreach (var Object in Globals.ObjectDict.OrderBy(i => -i.Value.Position.Y))
            {
                foreach (var component in Object.Value.ComponentDictionary.OrderBy(i => -i.Value.Position.Y))
                {
                    spriteBatch.Begin();
                    Drawing.DrawComponent(
                        component.Value,
                        Object.Value.Size * component.Value.Size);
                    spriteBatch.End();
                }

                // Draw resource bars if they're in the party
                if (Object.Value.PartyNumber >= 0)
                {
                    spriteBatch.Begin(
                        SpriteSortMode.Texture,
                        null,
                        SamplerState.LinearWrap);
                    Drawing.DrawResource(spriteBatch, Object.Value.Health, Object.Value.PartyNumber);
                    if (Object.Value.Stamina.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Value.Stamina, Object.Value.PartyNumber);
                    if (Object.Value.Electricity.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Value.Electricity, Object.Value.PartyNumber);
                    spriteBatch.End();
                }
            }

            // Draw text box
            if (Globals.DisplayTextQueue.Count != 0) Drawing.DrawTextBox(spriteBatch);

            base.Draw(gameTime);
        }
    }
}