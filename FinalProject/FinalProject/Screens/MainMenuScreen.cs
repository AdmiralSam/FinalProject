﻿using FinalProject.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FinalProject.Screens
{
    internal class MainMenuScreen : Screen
    {
        public ScreenEvent FinishedTransitioningOut;
        public ScreenEvent StartingTransitioningOut;
        private Texture2D background;
        private bool firstIteration;
        private MenuItemGroup menuItems;
        private InterpolatedValue scaleIn, scaleOut;
        private Texture2D snapshot;

        public MainMenuScreen(ContentManager contentManager, GraphicsDevice graphicsDevice)
            : base(contentManager, graphicsDevice)
        {
            scaleIn = new ExponentialInterpolatedValue(.005f, 1, 3);
            scaleIn.InterpolationFinished = ScaleInFinished;
            scaleOut = new ExponentialInterpolatedValue(1, .005f, 3);
            scaleOut.InterpolationFinished = ScaleOutFinished;
            menuItems = new MenuItemGroup();
            menuItems.AddItem(new MenuItem(new Vector2(280, 160), "NEW GAME"));
            menuItems.AddItem(new MenuItem(new Vector2(280, 320), "LOAD GAME"));
            menuItems.AddItem(new MenuItem(new Vector2(280, 480), "SETTINGS") { Disabled = true });
            menuItems.AddItem(new MenuItem(new Vector2(280, 640), "CREDITS") { Disabled = true });
            menuItems.AddItem(new MenuItem(new Vector2(280, 800), "QUIT GAME"));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case ScreenState.TransitioningIn:
                    {
                        if (firstIteration)
                        {
                            firstIteration = false;
                            GraphicsUtilities.BeginDrawingToTexture(spriteBatch, graphicsDevice);
                            DrawScreen(spriteBatch);
                            GraphicsUtilities.EndDrawingToTexture(spriteBatch, graphicsDevice);
                            snapshot = GraphicsUtilities.DuplicateTexture(GraphicsUtilities.GetTexture(), graphicsDevice);
                        }
                        GraphicsUtilities.DrawPixelatedTexture(spriteBatch, snapshot, Vector2.Zero, scaleIn.GetValue(), graphicsDevice);
                    } break;
                case ScreenState.Active:
                    {
                        GraphicsUtilities.BeginDrawingToTexture(spriteBatch, graphicsDevice);
                        DrawScreen(spriteBatch);
                        GraphicsUtilities.EndDrawingToTexture(spriteBatch, graphicsDevice);
                        spriteBatch.Draw(GraphicsUtilities.GetTexture(), new Rectangle(0, 0, Constants.VirtualWidth, Constants.VirtualHeight), Color.White);
                    } break;
                case ScreenState.TransitioningOut:
                    {
                        GraphicsUtilities.DrawPixelatedTexture(spriteBatch, snapshot, Vector2.Zero, scaleOut.GetValue(), graphicsDevice);
                    } break;
            }
        }

        public override void KeyPressed(Keys key)
        {
            switch (state)
            {
                case ScreenState.Active:
                    {
                        switch (key)
                        {
                            case Keys.Space:
                                {
                                    StartingTransitioningOut(menuItems.GetSelected());
                                } break;
                            case Keys.Up:
                                {
                                    menuItems.MoveUp();
                                } break;
                            case Keys.Down:
                                {
                                    menuItems.MoveDown();
                                } break;
                        }
                    } break;
            }
        }

        public override void LoadContent()
        {
            background = content.Load<Texture2D>("MenuBackground");
            base.LoadContent();
        }

        public override void Reset()
        {
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public override void TransitionOut()
        {
            snapshot = GraphicsUtilities.DuplicateTexture(GraphicsUtilities.GetTexture(), graphicsDevice);
            base.TransitionOut();
        }

        protected override void ScreenUpdate(float secondsPassed)
        {
            switch (state)
            {
                case ScreenState.TransitioningIn:
                    {
                        scaleIn.Update(secondsPassed);
                    } break;
                case ScreenState.TransitioningOut:
                    {
                        scaleOut.Update(secondsPassed);
                    } break;
            }
        }

        protected override void Set()
        {
            firstIteration = true;
        }

        private void DrawScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(0, 0, Constants.VirtualWidth, Constants.VirtualHeight), Color.White);
            menuItems.Draw(spriteBatch);
        }

        private void ScaleInFinished(float parameter)
        {
            state = ScreenState.Active;
        }

        private void ScaleOutFinished(float parameter)
        {
            FinishedTransitioningOut(menuItems.GetSelected());
        }
    }
}