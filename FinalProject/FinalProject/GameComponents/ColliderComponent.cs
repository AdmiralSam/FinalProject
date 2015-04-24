﻿using FinalProject.Screens;
using FinalProject.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FinalProject.GameComponents
{
    internal class ColliderComponent : Component, Drawable
    {
        private float boundingRadius;
        private List<ColliderComponent> colliderList;
        private Rectangle source;
        private List<Triangle> triangles;

        public ColliderComponent(Entity entity, Rectangle source, List<Triangle> triangles, List<ColliderComponent> colliderList)
            : base(entity)
        {
            this.source = source;
            this.triangles = triangles;
            boundingRadius = 0;
            foreach (Triangle triangle in triangles)
            {
                float aDistance = Vector2.Distance(entity.Position, triangle.A);
                float bDistance = Vector2.Distance(entity.Position, triangle.B);
                float cDistance = Vector2.Distance(entity.Position, triangle.C);
                boundingRadius = MathHelper.Max(MathHelper.Max(boundingRadius, aDistance), MathHelper.Max(bDistance, cDistance));
            }
            this.colliderList = colliderList;
            colliderList.Add(this);
        }

        public bool CollidesWith(ColliderComponent other)
        {
            if (Vector2.DistanceSquared(other.entity.Position, other.entity.Position) < (boundingRadius + other.boundingRadius) * (boundingRadius + other.boundingRadius))
            {
                foreach (Triangle triangle in TransformedTriangles())
                {
                    foreach (Triangle otherTriangle in other.TransformedTriangles())
                    {
                        if (triangle.Intersects(otherTriangle))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }

        public void DebugDraw()
        {
            GameScreen.LayerDebug.Add(this);
        }

        public override void Dispose()
        {
            colliderList.Remove(this);
            GameScreen.LayerDebug.Remove(this);
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            foreach (Triangle triangle in TransformedTriangles())
            {
                spriteBatch.Draw(GraphicsUtilities.PlainTexture, new Rectangle((int)triangle.A.X - 2, (int)triangle.A.Y - 2, 4, 4), Color.Green);
                spriteBatch.Draw(GraphicsUtilities.PlainTexture, new Rectangle((int)triangle.B.X - 2, (int)triangle.B.Y - 2, 4, 4), Color.Green);
                spriteBatch.Draw(GraphicsUtilities.PlainTexture, new Rectangle((int)triangle.C.X - 2, (int)triangle.C.Y - 2, 4, 4), Color.Green);
            }
        }

        public Entity GetEntity()
        {
            return entity;
        }

        public void NotifyOfCollision(Entity collidedWith)
        {
            entity.MessageCenter.Broadcast<Entity, Entity>("Collided With", entity, collidedWith);
        }

        public override void Update(float secondsPassed)
        {
        }

        private List<Triangle> TransformedTriangles()
        {
            List<Triangle> transformed = new List<Triangle>();
            foreach (Triangle triangle in triangles)
            {
                transformed.Add(triangle.Transform(entity.ToScreenMatrix(source)));
            }
            return transformed;
        }
    }
}