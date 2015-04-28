﻿using FinalProject.GameComponents;
using FinalProject.GameSaving;
using FinalProject.GameWaves;
using FinalProject.Messaging;
using FinalProject.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace FinalProject.Screens
{
    internal class GameScreen : Screen
    {
        public static Rectangle Bounds = new Rectangle(420, 0, 1080, Constants.VirtualHeight);
        public static List<ColliderComponent> CollidersEnemies;
        public static List<ColliderComponent> CollidersEnemyBullets;
        public static List<ColliderComponent> CollidersPlayer;
        public static List<ColliderComponent> CollidersPlayerBullets;
        public static List<Drawable> LayerDebug;
        public static List<Drawable> LayerEnemies;
        public static List<Drawable> LayerEnemyBullets;
        public static List<Drawable> LayerHealthBars;
        public static List<Drawable> LayerPlayer;
        public static List<Drawable> LayerPlayerBullets;
        public static MessageCenter MessageCenter;
        private Texture2D background;
        private Texture2D bullet;
        private SaveGame currentGame;
        private List<Entity> entities;
        private MenuItemGroup menuItems;
        private bool otherScreenReady;
        private bool paused;
        private bool readyToSwitch;
        private Random rng = new Random();
        private InterpolatedValue scaleIn, scaleOut;
        private Texture2D test;
        private Texture2D testHealth;
        private List<Entity> toRemove;
        private WaveManager waveManager;

        public GameScreen(ContentManager contentManager, GraphicsDevice graphicsDevice)
            : base(contentManager, graphicsDevice)
        {
            InitializeMessageCenter();
            scaleIn = new ExponentialInterpolatedValue(.002f, .25f, .5f);
            scaleIn.InterpolationFinished = ScaleInFinished;
            scaleOut = new ExponentialInterpolatedValue(.25f, .002f, .5f);
            scaleOut.InterpolationFinished = ScaleOutFinished;
            readyToSwitch = false;
            otherScreenReady = false;
            InitializeMenu();
            InitializeStaticVariables();
            entities = new List<Entity>();
            toRemove = new List<Entity>();
            GameMain.MessageCenter.AddListener<SaveGame, string>("Save Game and Stage Pass to Game", SetCurrentGameAndStage);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (state)
            {
                case ScreenState.TransitioningIn:
                    {
                        GraphicsUtilities.BeginDrawingPixelated(spriteBatch, Vector2.Zero, Constants.VirtualWidth, Constants.VirtualHeight, scaleIn.GetValue(), graphicsDevice);
                        DrawScreen(spriteBatch);
                        GraphicsUtilities.EndDrawingPixelated(spriteBatch, Constants.VirtualWidth, Constants.VirtualHeight, Vector2.Zero, scaleIn.GetValue(), graphicsDevice);
                    } break;
                case ScreenState.Active:
                    {
                        DrawScreen(spriteBatch);
                    } break;
                case ScreenState.TransitioningOut:
                    {
                        GraphicsUtilities.BeginDrawingPixelated(spriteBatch, Vector2.Zero, Constants.VirtualWidth, Constants.VirtualHeight, scaleOut.GetValue(), graphicsDevice);
                        DrawScreen(spriteBatch);
                        GraphicsUtilities.EndDrawingPixelated(spriteBatch, Constants.VirtualWidth, Constants.VirtualHeight, Vector2.Zero, scaleOut.GetValue(), graphicsDevice);
                    } break;
            }
        }

        public override void KeyPressed(Keys key)
        {
            MessageCenter.Broadcast<Keys>("Key Pressed", key);
            switch (state)
            {
                case ScreenState.Active:
                    {
                        switch (key)
                        {
                            case Keys.Enter:
                                {
                                } break;
                            case Keys.Up:
                                {
                                    menuItems.MoveUp();
                                } break;
                            case Keys.Down:
                                {
                                    menuItems.MoveDown();
                                } break;
                            case Keys.Escape:
                                {
                                    BeginTransitioningOut();
                                } break;
                        }
                    } break;
            }
        }

        public override void KeyReleased(Keys key)
        {
            MessageCenter.Broadcast<Keys>("Key Released", key);
        }

        public override void LoadContent()
        {
            test = content.Load<Texture2D>("ship01");
            background = content.Load<Texture2D>("MenuBackground");
            bullet = content.Load<Texture2D>("TestBulletTri");
            testHealth = content.Load<Texture2D>("CircularHealthBarTest");
            GameAssets.LoadContent(content);
            base.LoadContent();
        }

        public override void Start()
        {
            List<Wave> waves = new List<Wave>();
            SpawnInformation test1 = new SpawnInformation(0);
            test1.Information["Unit Type"] = "Jellyfish";
            test1.Information["Spawn Position"] = new Vector2(500, -200);
            test1.Information["Shoot Position"] = new Vector2(700, 200);
            SpawnInformation test2 = new SpawnInformation(0);
            test2.Information["Unit Type"] = "Jellyfish";
            test2.Information["Spawn Position"] = new Vector2(1420, -200);
            test2.Information["Shoot Position"] = new Vector2(1220, 200);
            for (int i = 0; i < 4; i++)
            {
                Wave wave = new Wave();
                wave.AddSpawnInformation(test1);
                wave.AddSpawnInformation(test2);
                waves.Add(wave);
            }
            waveManager = new WaveManager(waves);
            Entity ship = new Entity();
            ship.Position = new Vector2(700, 700);
            ship.Rotation = -(float)(Math.PI / 2);
            new PlayerControllerComponent(ship, 200);
            new ConstantRateFireComponent(ship, 0.1f);
            new SpreadShotProjectileWeaponComponent(ship, 1, (float)(-Math.PI / 2), new Vector2(0, -50));
            new SpreadShotProjectileWeaponComponent(ship, 1, (float)(-Math.PI / 2 - Math.PI / 16), new Vector2(-5, -50));
            new SpreadShotProjectileWeaponComponent(ship, 1, (float)(-Math.PI / 2 + Math.PI / 16), new Vector2(5, -50));
            new VelocityAccelerationComponent(ship, Vector2.Zero, Vector2.Zero);
            new RestrictPositionComponent(ship, 50, 50, Bounds);
            new ColliderComponent(ship, GameAssets.Unit["Spread Shot Ship"], GameAssets.UnitTriangles["Spread Shot Ship"], CollidersPlayer).DebugDraw();
            new HealthComponent(ship, 20);
            new CircularHealthBarComponent(ship, (float)(Math.PI * 4 / 5));
            new RemoveOnDeathComponent(ship);
            new TextureRendererComponent(ship, GameAssets.UnitTexture, GameAssets.Unit["Spread Shot Ship"], Color.White, LayerPlayer);
            entities.Add(ship);
            base.Start();
        }

        protected override void BeginTransitioningOut()
        {
            GameMain.MessageCenter.Broadcast<SaveGame>("Save Game Pass to Select Stage", currentGame);
            GameMain.MessageCenter.Broadcast<string>("Start Loading Content", "Select Stage");
            GameMain.MessageCenter.AddListener("Finished Loading", OtherScreenFinishedLoading);
            TransitionOut();
        }

        protected override void FinishedLoading()
        {
            GameMain.MessageCenter.Broadcast("Finished Loading");
        }

        protected override void FinishTransitioningOut()
        {
            if (otherScreenReady)
            {
                SwitchScreens();
            }
            else
            {
                readyToSwitch = true;
            }
        }

        protected override void Reset()
        {
            scaleIn.SetParameter(0);
            scaleOut.SetParameter(0);
            entities.Clear();
            CollidersEnemies.Clear();
            CollidersEnemyBullets.Clear();
            CollidersPlayer.Clear();
            CollidersPlayerBullets.Clear();
            LayerDebug.Clear();
            LayerEnemies.Clear();
            LayerEnemyBullets.Clear();
            LayerHealthBars.Clear();
            LayerPlayer.Clear();
            LayerPlayerBullets.Clear();
            GameMain.MessageCenter.RemoveListener("Finished Loading", OtherScreenFinishedLoading);
        }

        protected override void ScreenUpdate(float secondsPassed)
        {
            switch (state)
            {
                case ScreenState.TransitioningIn:
                    {
                        scaleIn.Update(secondsPassed);
                    } break;
                case ScreenState.Active:
                    {
                        if (!paused)
                        {
                            waveManager.Update(secondsPassed);
                            List<Entity> toSpawn = waveManager.GetToSpawn();
                            if (toSpawn != null)
                            {
                                foreach (Entity entity in toSpawn)
                                {
                                    entities.Add(entity);
                                }
                            }
                            CheckForCollisions();
                            foreach (Entity entity in toRemove)
                            {
                                entities.Remove(entity);
                                entity.Dispose();
                            }
                            toRemove.Clear();
                            foreach (Entity entity in entities)
                            {
                                entity.MessageCenter.Broadcast("Clean Up");
                            }
                            foreach (Entity entity in entities)
                            {
                                entity.Update(secondsPassed);
                            }
                        }
                    } break;
                case ScreenState.TransitioningOut:
                    {
                        scaleOut.Update(secondsPassed);
                    } break;
            }
        }

        private static void InitializeStaticVariables()
        {
            CollidersEnemies = new List<ColliderComponent>();
            CollidersEnemyBullets = new List<ColliderComponent>();
            CollidersPlayer = new List<ColliderComponent>();
            CollidersPlayerBullets = new List<ColliderComponent>();
            LayerDebug = new List<Drawable>();
            LayerEnemies = new List<Drawable>();
            LayerEnemyBullets = new List<Drawable>();
            LayerHealthBars = new List<Drawable>();
            LayerPlayer = new List<Drawable>();
            LayerPlayerBullets = new List<Drawable>();
        }

        private static void SwitchScreens()
        {
            GameMain.MessageCenter.Broadcast<string>("Switch Screens", "Select Stage");
        }

        private void CheckForCollisions()
        {
            foreach (ColliderComponent player in CollidersPlayer)
            {
                foreach (ColliderComponent enemyBullet in CollidersEnemyBullets)
                {
                    if (enemyBullet.CollidesWith(player))
                    {
                        enemyBullet.NotifyOfCollision(player.GetEntity());
                        player.NotifyOfCollision(enemyBullet.GetEntity());
                    }
                }
            }
            foreach (ColliderComponent player in CollidersPlayer)
            {
                foreach (ColliderComponent enemy in CollidersEnemies)
                {
                    if (player.CollidesWith(enemy))
                    {
                        enemy.NotifyOfCollision(player.GetEntity());
                        player.NotifyOfCollision(enemy.GetEntity());
                    }
                }
            }
            foreach (ColliderComponent enemy in CollidersEnemies)
            {
                foreach (ColliderComponent playerBullet in CollidersPlayerBullets)
                {
                    if (playerBullet.CollidesWith(enemy))
                    {
                        playerBullet.NotifyOfCollision(enemy.GetEntity());
                        enemy.NotifyOfCollision(playerBullet.GetEntity());
                    }
                }
            }
        }

        private Vector2 ClosestCollider(Entity entity, List<ColliderComponent> colliderList, float maxAngle)
        {
            ColliderComponent closest = null;
            Vector2 closestFromTo = Vector2.Zero;
            Vector2 entityVector = Vector2.Transform(Vector2.UnitX, Matrix.CreateRotationZ(entity.Rotation));
            foreach (ColliderComponent collider in colliderList)
            {
                Vector2 fromTo = collider.GetEntity().Position - entity.Position;
                if (MathUtilities.angleBetween(entityVector, fromTo) < maxAngle)
                {
                    if (closest == null || fromTo.LengthSquared() < closestFromTo.LengthSquared())
                    {
                        closest = collider;
                        closestFromTo = fromTo;
                    }
                }
            }
            return closest == null ? new Vector2(-1, -1) : closest.GetEntity().Position;
        }

        private void ClosestPlayer(Entity parameterOne)
        {
            parameterOne.MessageCenter.Broadcast<Vector2>("Closest Player", ClosestCollider(parameterOne, CollidersPlayer, (float)Math.PI));
        }

        private void DrawScreen(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(0, 0, Constants.VirtualWidth, Constants.VirtualHeight), Color.White);
            foreach (Drawable drawable in LayerPlayerBullets)
            {
                drawable.Draw(spriteBatch);
            }
            foreach (Drawable drawable in LayerPlayer)
            {
                drawable.Draw(spriteBatch);
            }
            foreach (Drawable drawable in LayerHealthBars)
            {
                drawable.Draw(spriteBatch);
            }
            foreach (Drawable drawable in LayerEnemyBullets)
            {
                drawable.Draw(spriteBatch);
            }
            foreach (Drawable drawable in LayerEnemies)
            {
                drawable.Draw(spriteBatch);
            }
            foreach (Drawable drawable in LayerDebug)
            {
                drawable.Draw(spriteBatch);
            }
            if (paused)
            {
                menuItems.Draw(spriteBatch);
            }
        }

        private void InitializeMenu()
        {
            menuItems = new MenuItemGroup();
            menuItems.AddItem(new MenuItem(new Vector2(280, 320), "LEVEL SELECT"));
            menuItems.AddItem(new MenuItem(new Vector2(280, 450), "UPGRADES"));
        }

        private void InitializeMessageCenter()
        {
            MessageCenter = new MessageCenter();
            MessageCenter.AddListener<Entity>("Remove Entity", RemoveEntity);
            MessageCenter.AddListener<Entity>("Find Closest Player", ClosestPlayer);
        }

        private void OtherScreenFinishedLoading()
        {
            if (readyToSwitch)
            {
                SwitchScreens();
            }
            else
            {
                otherScreenReady = true;
            }
        }

        private void RemoveEntity(Entity entity)
        {
            toRemove.Add(entity);
        }

        private void ScaleInFinished(float parameter)
        {
            state = ScreenState.Active;
        }

        private void ScaleOutFinished(float parameter)
        {
            FinishTransitioningOut();
        }

        private void SetCurrentGameAndStage(SaveGame saveGame, string stage)
        {
            currentGame = saveGame;
            UnitFactory.Difficulty = currentGame.difficulty;
            switch (stage)
            {
                case "LEVEL 1": { UnitFactory.Stage = 1; } break;
                case "LEVEL 2": { UnitFactory.Stage = 2; } break;
                case "LEVEL 3": { UnitFactory.Stage = 3; } break;
            }
        }
    }
}