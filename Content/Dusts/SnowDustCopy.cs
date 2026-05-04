using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace VFXPlus.Content.Dusts
{
    //Copy of the Vanilla snow dust but without it moving with your y-velocity
	public class SnowDustCopy : ModDust
	{
        public override string Texture => "VFXPlus/Content/Dusts/Dust_76";

        public override void OnSpawn(Dust dust)
		{
			Texture2D texture = Mod.Assets.Request<Texture2D>("Content/Dusts/Dust_76").Value;
			dust.frame = new Rectangle(0, texture.Height / 3 * Main.rand.Next(3), texture.Width, texture.Height / 3);
            dust.noLight = true;
		}

		public override bool Update(Dust dust)
		{
            dust.scale += 0.009f;
            if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) && dust.fadeIn == 0f)
            {
                dust.scale *= 0.9f;
                dust.velocity *= 0.25f;
            }
            return true;

        }
	}

    public class SnowDustCopyQuickFade : ModDust
    {
        public override string Texture => "VFXPlus/Content/Dusts/Dust_76";

        public override void OnSpawn(Dust dust)
        {
            Texture2D texture = Mod.Assets.Request<Texture2D>("Content/Dusts/Dust_76").Value;
            dust.frame = new Rectangle(0, texture.Height / 3 * Main.rand.Next(3), texture.Width, texture.Height / 3);
            dust.noLight = true;
        }

        public override bool Update(Dust dust)
        {
            dust.scale += 0.009f;

            dust.velocity *= 0.95f;
            
            if (dust.velocity.Length() < 2f)
                dust.scale *= 0.9f;

            return true;

        }
    }

    public class PaintDripDust : ModDust
    {
        public override string Texture => "VFXPlus/Content/Dusts/PaintDrip";

        public override void OnSpawn(Dust dust)
        {
            dust.noLight = true;
            dust.noGravity = false;

            //dust.alpha is used as a timer
            dust.alpha = 0;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
            {
                dust.customData = new PaintDripBehavoir();
            }

            PaintDripBehavoir behavior = (dust.customData as PaintDripBehavoir);

            dust.velocity.Y += 0.2f;

            if (!behavior.hasHitTile)
            {

                float minXScale = 0.3f;
                float maxYScale = 2f;

                float xClamped = Math.Clamp(dust.velocity.X * 0.1f, minXScale, 1f);
                float yClamped = Math.Clamp(dust.velocity.Y * 0.1f, 1f, maxYScale);

                behavior.xScale = MathHelper.Lerp(behavior.xScale, xClamped, 0.08f);
                behavior.yScale = MathHelper.Lerp(behavior.yScale, yClamped, 0.08f);


                dust.position += dust.velocity * 0.5f;
                if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 5, 5))
                {
                    behavior.yScale = 1f;
                    behavior.hasHitTile = true;
                }
                else
                {
                    dust.position += dust.velocity * 0.5f;
                    if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 5, 5))
                    {
                        behavior.yScale = 1f;
                        behavior.hasHitTile = true;
                    }
                }

            }
            else
            {
                if (behavior.xScale >= 0.94f)
                    dust.scale *= 0.95f;
                dust.velocity *= 0f;

                float maxXScale = 3f;
                float minYScale = 0.35f;

                float xClamped = Math.Clamp(dust.velocity.X * 0.15f, 1.25f, maxXScale);
                float yClamped = Math.Clamp(dust.velocity.Y * 0.15f, minYScale, 1f);

                behavior.xScale = MathHelper.Lerp(behavior.xScale, xClamped, 0.14f);
                behavior.yScale = MathHelper.Lerp(behavior.yScale, yClamped, 0.24f);
            }


            if (dust.scale < 0.1f)
                dust.active = false;


            dust.rotation = dust.velocity.ToRotation();

            dust.alpha++;
            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            if (dust.customData == null)
                dust.customData = new PaintDripBehavoir();

            PaintDripBehavoir behavior = (dust.customData as PaintDripBehavoir);


            Texture2D texture = Mod.Assets.Request<Texture2D>("Assets/CircleBorder").Value;

            Vector2 vec2Scale = new Vector2(behavior.xScale, behavior.yScale) * dust.scale;

            Main.spriteBatch.Draw(texture, dust.position - Main.screenPosition, null, dust.color * 0.75f, 0f, texture.Size() / 2f, vec2Scale * 0.2f, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class PaintDripBehavoir
    {
        public bool hasHitTile = false;

        public float xScale = 0.35f;
        public float yScale = 1f;
    }
}