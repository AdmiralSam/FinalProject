﻿using FinalProject.GameSaving;
using FinalProject.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FinalProject.Screens
{
    internal class UpgradeScreen : Screen
    {
        private Texture2D background;

        private SaveGame currentGame;

        private InterpolatedValue scaleIn, scaleOut;

        private UpgradeItemGroup upgrades;

        public UpgradeScreen(ContentManager contentManager, GraphicsDevice graphicsDevice)
            : base(contentManager, graphicsDevice)
        {
            scaleIn = new ExponentialInterpolatedValue(.002f, .25f, .5f);
            scaleIn.InterpolationFinished = ScaleInFinished;
            scaleOut = new ExponentialInterpolatedValue(.25f, .002f, .5f);
            scaleOut.InterpolationFinished = ScaleOutFinished;
            upgrades = new UpgradeItemGroup();
            GameMain.MessageCenter.AddListener<SaveGame>("Save Game Pass to Upgrade", SetCurrentGame);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case ScreenState.TransitioningIn:
                    {
                        GraphicsUtilities.BeginDrawingPixelated(spriteBatch, graphicsDevice, scaleIn.GetValue());
                        DrawScreen(spriteBatch);
                        GraphicsUtilities.EndDrawingPixelated(spriteBatch, graphicsDevice, scaleIn.GetValue());
                    } break;
                case ScreenState.Active:
                    {
                        DrawScreen(spriteBatch);
                    } break;
                case ScreenState.TransitioningOut:
                    {
                        GraphicsUtilities.BeginDrawingPixelated(spriteBatch, graphicsDevice, scaleOut.GetValue());
                        DrawScreen(spriteBatch);
                        GraphicsUtilities.EndDrawingPixelated(spriteBatch, graphicsDevice, scaleOut.GetValue());
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
                            case Keys.Enter:
                                {
                                    int cost = upgrades.GetSelectedCost();
                                    if (cost < currentGame.Credits)
                                    {
                                        currentGame.Credits -= cost;
                                        upgrades.UpgradeSelected();
                                    }
                                } break;
                            case Keys.Up:
                                {
                                    upgrades.MoveUp();
                                } break;
                            case Keys.Down:
                                {
                                    upgrades.MoveDown();
                                } break;
                            case Keys.Escape:
                                {
                                    UpdateUpgrades();
                                    BeginTransitioningOut();
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

        public override void Start()
        {
            if (currentGame == null)
            {
                throw new Exception("A Save Game Must Be Passed In");
            }
            GetUpgrades();
            base.Start();
        }

        protected override void BeginTransitioningOut()
        {
            SaveGameManager.SaveGame(currentGame);
            GameMain.MessageCenter.Broadcast<SaveGame>("Save Game Pass to Command Center", currentGame);
            GameMain.MessageCenter.Broadcast<string>("Start Loading Content", "Command Center");
            base.BeginTransitioningOut();
        }

        protected override void Reset()
        {
            currentGame = null;
            scaleIn.SetParameter(0);
            scaleOut.SetParameter(0);
            upgrades.Reset(); base.Reset();
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

        protected override void SwitchScreens()
        {
            GameMain.MessageCenter.Broadcast<string>("Switch Screens", "Command Center");
        }

        private void DrawScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(0, 0, GameMain.VirtualWidth, GameMain.VirtualHeight), Color.White);
            upgrades.Draw(spriteBatch);
            GraphicsUtilities.DrawStringVerticallyCentered(spriteBatch, Fonts.MenuTitleFont, Fonts.Green, new Vector2(320, 210), "UPGRADES");
            GraphicsUtilities.DrawStringVerticallyCentered(spriteBatch, Fonts.UpgradeCreditTextFont, Fonts.Red, new Vector2(1155, 245), "CREDITS:");
            GraphicsUtilities.DrawStringVerticallyCentered(spriteBatch, Fonts.UpgradeCreditsFont, Fonts.Red, new Vector2(1440, 245), "" + currentGame.Credits);
        }

        private void GetUpgrades()
        {
            upgrades.AddItem(new UpgradeItem(new Vector2(280, 320), "SHIELDS", currentGame.Shields));
            upgrades.AddItem(new UpgradeItem(new Vector2(280, 450), "MOVE SPEED", currentGame.MovementSpeed));
            upgrades.AddItem(new UpgradeItem(new Vector2(280, 580), "DAMAGE", currentGame.Damage));
            upgrades.AddItem(new UpgradeItem(new Vector2(280, 710), "FIRE RATE", currentGame.FireRate));
            upgrades.AddItem(new UpgradeItem(new Vector2(280, 840), "WEAPON STR", currentGame.WeaponStrength));
        }

        private void ScaleInFinished(float parameter)
        {
            state = ScreenState.Active;
        }

        private void ScaleOutFinished(float parameter)
        {
            FinishTransitioningOut();
        }

        private void SetCurrentGame(SaveGame saveGame)
        {
            currentGame = saveGame;
        }

        private void UpdateUpgrades()
        {
            Dictionary<string, int> upgradeLevels = upgrades.GetLevels();
            currentGame.Shields = upgradeLevels["SHIELDS"];
            currentGame.MovementSpeed = upgradeLevels["MOVE SPEED"];
            currentGame.Damage = upgradeLevels["DAMAGE"];
            currentGame.FireRate = upgradeLevels["FIRE RATE"];
            currentGame.WeaponStrength = upgradeLevels["WEAPON STR"];
        }
    }
}