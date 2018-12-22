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

            camera = new Camera();

            Globals.Content = Content;

            Globals.GameObjects = new GameObjectManager();
            Globals.Tiles = new TileManager();

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
            Globals.GameObjects.Update();

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

            // Draw all tiles which the camera can see
            effect.View = camera.View;
            effect.Projection = camera.Projection;
            for (var x = (int) camera.ViewMinCoordinates().X; x < (int) camera.ViewMaxCoordinates().X; x++)
            {
                for (var y = (int) camera.ViewMinCoordinates().Y; y < (int) camera.ViewMaxCoordinates().Y; y++)
                {
                    // Draw the base tile itself
                    var tileIndex = Globals.Tiles.GetIndex(x, y);
                    var bottomLeft = new Vector2(x, y);
                    Drawing.DrawTile(Globals.Tiles.NameFromIndex[tileIndex], bottomLeft);

                    // Figure out if we need to lay other tiles over it
                    var topTileIndex = Globals.Tiles.GetIndex(x, y + 1);
                    var bottomTileIndex = Globals.Tiles.GetIndex(x, y - 1);
                    var rightTileIndex = Globals.Tiles.GetIndex(x + 1, y);
                    var leftTileIndex = Globals.Tiles.GetIndex(x - 1, y);
                    var topLeftTileIndex = Globals.Tiles.GetIndex(x - 1, y + 1);
                    var topRightTileIndex = Globals.Tiles.GetIndex(x + 1, y + 1);
                    var bottomRightTileIndex = Globals.Tiles.GetIndex(x + 1, y - 1);
                    var bottomLeftTileIndex = Globals.Tiles.GetIndex(x - 1, y - 1);

                    // If we're surrounded by entirely similar tiles then we can avoid all layering logic
                    if (tileIndex != topTileIndex || tileIndex != bottomTileIndex 
                        || tileIndex != leftTileIndex || tileIndex != rightTileIndex
                        || tileIndex != topLeftTileIndex || tileIndex != topRightTileIndex 
                        || tileIndex != bottomRightTileIndex || tileIndex != bottomLeftTileIndex)
                    {
                        // TODO: I need to abstract this, since I'll need to go in order from smallest to largest in order to preserve layering

                        // If we are being drawn over the tile on the right
                        if (tileIndex < rightTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[rightTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " side", bottomLeft, "Right");
                        }

                        // If we are being drawn over the tile on the left
                        if (tileIndex < leftTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[leftTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " side", bottomLeft, "Left");
                        }

                        // If we are being drawn over the tile on the bottom
                        if (tileIndex < bottomTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[bottomTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " side", bottomLeft, "Bottom");
                        }

                        // If we are being drawn over the tile on the top
                        if (tileIndex < topTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[topTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " side", bottomLeft, "Top");
                        }

                        // If we are being drawn over the tile on the top right
                        if (tileIndex < topRightTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[topRightTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " corner", bottomLeft, "Top");
                        }

                        // If we are being drawn over the tile on the top left
                        if (tileIndex < topLeftTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[topLeftTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " corner", bottomLeft, "Left");
                        }

                        // If we are being drawn over the tile on the bottom right
                        if (tileIndex < bottomRightTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[bottomRightTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " corner", bottomLeft, "Right");
                        }

                        // If we are being drawn over the tile on the bottom left
                        if (tileIndex < bottomLeftTileIndex)
                        {
                            var tileName = Globals.Tiles.NameFromIndex[bottomLeftTileIndex].Split(' ')[0];
                            Drawing.DrawTile(tileName + " corner", bottomLeft, "Bottom");
                        }

                    }
                }
            }

            // Draw the GameObjects
            foreach (var Object in Globals.GameObjects.ActiveGameObjects.Select(
                i => Globals.GameObjects.ObjectFromName[i]).OrderBy(i => -i.Position.Y))
            {
                foreach (var component in Object.ComponentDictionary.OrderBy(i => -i.Value.Position.Y))
                {
                    spriteBatch.Begin();
                    Drawing.DrawComponent(
                        component.Value,
                        Object.Size * component.Value.Size);
                    spriteBatch.End();
                }

                // Draw resource bars if they're in the party
                if (Object.PartyNumber >= 0)
                {
                    spriteBatch.Begin(
                        SpriteSortMode.Texture,
                        null,
                        SamplerState.LinearWrap);
                    Drawing.DrawResource(spriteBatch, Object.Health, Object.PartyNumber);
                    if (Object.Stamina.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Stamina, Object.PartyNumber);
                    if (Object.Electricity.Max > 0)
                        Drawing.DrawResource(spriteBatch, Object.Electricity, Object.PartyNumber);
                    spriteBatch.End();
                }
            }

            spriteBatch.Begin(
                SpriteSortMode.BackToFront,
                null,
                SamplerState.LinearWrap);

            // Draw the selected objects in creative mode
            if (Globals.CreativeMode)
            {
                // Draw the object on the left
                Drawing.DrawSprite(
                    spriteBatch,
                    Globals.Tiles.ObjectFromName[
                        Globals.Tiles.NameFromIndex[
                            Globals.Tiles.BaseObjectIndexes[
                                (int)MathTools.Mod(Globals.CreativeObjectIndex - 1, Globals.Tiles.BaseObjectIndexes.Count)]]],
                    new Vector2(Globals.Resolution.X / 2 - 125, 10),
                    new Vector2(Globals.Resolution.X / 2 - 75, 60));

                // Draw the object in the middle
                Drawing.DrawSprite(
                    spriteBatch,
                    Globals.Tiles.ObjectFromName[
                        Globals.Tiles.NameFromIndex[
                             Globals.Tiles.BaseObjectIndexes[
                                Globals.CreativeObjectIndex]]],
                    new Vector2(Globals.Resolution.X / 2 - 50, 10),
                    new Vector2(Globals.Resolution.X / 2 + 50, 100));

                // Draw the object on the right
                Drawing.DrawSprite(
                    spriteBatch,
                    Globals.Tiles.ObjectFromName[
                        Globals.Tiles.NameFromIndex[
                             Globals.Tiles.BaseObjectIndexes[
                                (int)MathTools.Mod(Globals.CreativeObjectIndex + 1, Globals.Tiles.BaseObjectIndexes.Count)]]],
                    new Vector2(Globals.Resolution.X / 2 + 75, 10),
                    new Vector2(Globals.Resolution.X / 2 + 125, 60));
            }

            // Draw text box
            if (Globals.DisplayTextQueue.Count != 0) Drawing.DrawTextBox(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}