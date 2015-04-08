using FinalProject.Screens;
using FinalProject.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProject
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Screen current, next;
        private SpriteFont debugFont;

        private SplashScreen splashScreen;
        private MainMenuScreen mainMenuScreen;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, GameUtilities.GetResizeMatrix(GraphicsDevice));
            if (current != null)
            {
                current.Draw(spriteBatch);
            }
            if (gameTime.ElapsedGameTime.TotalSeconds != 0)
            {
                spriteBatch.DrawString(debugFont, string.Format("FPS: {0:00.00}", 1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(300, 10), Color.Red);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            IsFixedTimeStep = false;
            TextureUtilities.CreateRenderTarget(GraphicsDevice);
            TextureUtilities.MakePlainTexture(GraphicsDevice);
            splashScreen = new SplashScreen(GameUtilities.GenerateNewContentManager(Services), GraphicsDevice);
            splashScreen.SplashScreenFinishedPlaying = SplashScreenFinishedPlaying;
            splashScreen.FinishedTransitioningOut = SplashScreenFinishedTransitioningOut;
            mainMenuScreen = new MainMenuScreen(GameUtilities.GenerateNewContentManager(Services), GraphicsDevice);
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
            debugFont = Content.Load<SpriteFont>("DebugFont");
            splashScreen.LoadContent();
            mainMenuScreen.LoadContent();
            current = splashScreen;
            current.Start();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (current != null)
            {
                current.Update(gameTime);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Exit();
            }
            base.Update(gameTime);
        }

        private void SplashScreenFinishedPlaying()
        {
            splashScreen.TransitionOut();
        }

        private void SplashScreenFinishedTransitioningOut()
        {
            current = mainMenuScreen;
            mainMenuScreen.Start();
            splashScreen.Stop();
        }
    }
}