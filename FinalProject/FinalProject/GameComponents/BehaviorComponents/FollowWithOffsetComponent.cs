﻿using Microsoft.Xna.Framework;

namespace FinalProject.GameComponents
{
    internal class FollowWithOffsetComponent : Component
    {
        private Vector2 offset;
        private Entity toFollow;

        public FollowWithOffsetComponent(Entity entity, Entity toFollow, Vector2 offset)
            : base(entity)
        {
            this.offset = offset;
            this.toFollow = toFollow;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Update(float secondsPassed)
        {
            entity.Position = toFollow.Position + offset;
        }
    }
}