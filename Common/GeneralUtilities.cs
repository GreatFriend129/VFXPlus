using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Utilities;

namespace VFXPlus.Common
{
    internal static class GeneralUtilities
    {
        /// <summary>
        /// Simple smooth movement function. Returns a desired velocity vector.
        /// </summary>
        /// <param name="currentPos">Current position this object.</param>
        /// <param name="currentVel">Current velocity of this object.</param>
        /// <param name="goalPos">Where this object is moving towards</param>
        /// <param name="moveSpeed"></param>
        /// <param name="clampDistance"></param>
        public static Vector2 BasicMovement(Vector2 currentPos, Vector2 currentVel, Vector2 goalPos, float moveSpeed = 6f, float clampDistance = 240f)
        {
            currentVel *= 0.875f;

            Vector2 trueGoal = goalPos - currentPos;
            Vector2 lerpGoal = Vector2.Lerp(currentPos, goalPos, 0.8f);

            Vector2 goTo = trueGoal;
            float speed = moveSpeed * MathHelper.Clamp(goTo.Length() / clampDistance, 0, 1);
            if (goTo.Length() < speed)
            {
                speed = goTo.Length();
            }
            currentVel += goTo.SafeNormalize(Vector2.Zero) * speed;

            return currentVel;
        }

        /// <summary>
        /// Simple smooth movement function.
        /// </summary>
        /// <param name="entity">Entity that is moving.</param>
        /// <param name="goalPos">Where this object is moving towards</param>
        /// <param name="moveSpeed"></param>
        /// <param name="clampDistance"></param>
        public static void BasicMovement(Entity entity, Vector2 goalPos, float moveSpeed = 6f, float clampDistance = 240f)
        {
            entity.velocity *= 0.875f;

            Vector2 trueGoal = goalPos - entity.Center;
            Vector2 lerpGoal = Vector2.Lerp(entity.Center, goalPos, 0.8f);

            Vector2 goTo = trueGoal;
            float speed = moveSpeed * MathHelper.Clamp(goTo.Length() / clampDistance, 0, 1);
            if (goTo.Length() < speed)
            {
                speed = goTo.Length();
            }
            entity.velocity += goTo.SafeNormalize(Vector2.Zero) * speed;
        }

        public static float FadeLinear(float progress, float fadeIn = 0.5f, float fadeOut = 0.5f)
        {
            if (progress <= 0f)
                return 0f;

            if (progress <= fadeIn)
            {
                return progress / fadeIn;
            }

            if (progress > (1f - fadeOut))
            {
                return 1f - (progress - (1f - fadeOut)) / fadeOut;
            }

            return 1f;
        }

        public static float NextFloatF(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }
    }
}
