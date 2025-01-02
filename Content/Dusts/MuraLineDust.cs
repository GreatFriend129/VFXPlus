using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace VFXPlus.Content.Dusts
{
	
	public class MuraLineDust : ModDust
	{
		public override string Texture => "VFXPlus/Content/Dusts/Textures/MuraLine120x120";

		public override void OnSpawn(Dust dust)
		{			
			//dust.customData = false;
			dust.noGravity = true;

			dust.fadeIn = 1f;
			//dust.scale = 0;
            dust.frame = new Rectangle(0, 0, 38, 14);
        }

		public override Color? GetAlpha(Dust dust, Color lightColor) { 	return dust.color; }

		public override bool Update(Dust dust)
		{

            dust.rotation = dust.velocity.ToRotation();
            dust.position += dust.velocity;

			if (dust.customData is MuraLineBehavior mlb)
			{
                dust.scale *= mlb.sizeChangeSpeed;
                dust.velocity *= mlb.velFadeSpeed;
            }
			else
			{
                dust.velocity *= 0.95f;
                dust.scale *= 0.98f;
            }
            dust.color.A = 0;

            //dust.scale = MathHelper.Clamp(MathHelper.Lerp(dust.scale, 1f, 0.025f), 0f, 0.5f);


			if (dust.alpha > 15)
			{
				dust.fadeIn = Math.Clamp(MathHelper.Lerp(dust.fadeIn, -0.5f, 0.05f), 0, 1);
			}

			if (dust.fadeIn <= 0)
				dust.active = false;

			dust.alpha++;

            return false;
		}


		public override bool PreDraw(Dust dust)
		{
			Vector2 vec2scale = new Vector2(1f, 1f) * dust.scale;

			if (dust.customData is MuraLineBehavior mlb)
				vec2scale = mlb.XYscale * dust.scale;

			Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, null, dust.color * dust.fadeIn, dust.rotation, new Vector2(60f, 60f), vec2scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * dust.fadeIn, dust.rotation, new Vector2(60, 60f), vec2scale * 0.5f, SpriteEffects.None, 0f);
            return false;
		}
	}

	public class MuraLineBehavior
	{
		public Vector2 XYscale;

		public float velFadeSpeed = 0.95f;

		public float sizeChangeSpeed = 0.98f;

		public MuraLineBehavior(Vector2 xyscale)
		{
			XYscale = xyscale;
		}

        public MuraLineBehavior(Vector2 xyscale, float VelFadeSpeed = 0.95f, float SizeChangeSpeed = 0.98f)
        {
            XYscale = xyscale;
            velFadeSpeed = VelFadeSpeed;
            sizeChangeSpeed = SizeChangeSpeed;
        }

    }

	public class MuraLineBasic : ModDust
	{
		public override string Texture => "VFXPlus/Content/Dusts/Textures/MuraLine120x120";

		public override void OnSpawn(Dust dust)
		{
			dust.customData = false;
			dust.noGravity = true;
			dust.fadeIn = 1f;
			dust.frame = new Rectangle(0, 0, 38, 14);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor) 
		{
			return dust.color;
		}

		public override bool Update(Dust dust)
		{

			dust.rotation = dust.velocity.ToRotation();
			dust.position += dust.velocity;

			dust.velocity *= 0.97f;


			dust.color.A = 0;


			if (dust.alpha > 15)
			{
				dust.fadeIn = Math.Clamp(MathHelper.Lerp(dust.fadeIn, -0.5f, 0.08f), 0, 1);
			}

			if (dust.fadeIn <= 0)
				dust.active = false;

			dust.alpha++;

			return false;
		}


		public override bool PreDraw(Dust dust)
		{
			Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, null, dust.color * dust.fadeIn, dust.rotation, new Vector2(60f, 60f), dust.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * dust.fadeIn, dust.rotation, new Vector2(60, 60f), dust.scale * 0.5f, SpriteEffects.None, 0f);

			return false;
		}
	}
  
}