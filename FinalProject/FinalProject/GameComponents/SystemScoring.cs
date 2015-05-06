﻿using FinalProject.Screens;
using FinalProject.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalProject.GameComponents
{
    internal class SystemScoring
    {
        private float health;
        private float maxHealth;
        private int maxScore;
        private Entity player;
        private int score;

        public SystemScoring()
        {
            score = 0;
            maxScore = 1;
        }

        public void AddWorth(int worth)
        {
            score += worth;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (player != null)
            {
                player.MessageCenter.Broadcast("Get Health");
            }
            int height = (int)(1041 * (score / (float)maxScore));
            int scoreTop = 1061 - height;
            float healthAmount = health / maxHealth;
            int healthHeight = (int)(1041 * healthAmount);
            int healthTop = 1061 - healthHeight;
            Vector3 healthColor = Vector3.Zero;
            if (healthAmount > .5f)
            {
                healthColor = Vector3.Lerp(new Vector3(1, 1, 0), new Vector3(0, 1, 0), healthAmount - .5f);
            }
            else
            {
                healthColor = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(1, 1, 0), healthAmount);
            }
            spriteBatch.Draw(UtilitiesGraphics.PlainTexture, new Rectangle(1479, healthTop, 15, healthHeight), new Color(healthColor));
            spriteBatch.Draw(UtilitiesGraphics.PlainTexture, new Rectangle(428, scoreTop, 15, height), new Color(255, 215, 0));
        }

        public void Reset()
        {
            player.MessageCenter.RemoveListener<float, float>("Health", SetHealth);
            player = null;
            score = 0;
        }

        public void SetHealth(float health, float maxHealth)
        {
            this.health = health;
            this.maxHealth = maxHealth;
        }

        public void SetMaxScore(int max)
        {
            maxScore = max;
        }

        public void SetPlayer(Entity player)
        {
            this.player = player;
            player.MessageCenter.AddListener<float, float>("Health", SetHealth);
        }
    }
}