using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Interfaces;

namespace VFXPlus.Content.Projectiles
{
    public class H3Impact : ModProjectile, IDrawAdditive
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public Color[] cols = { Color.White, Color.Crimson, Color.Red };
		public bool pixelize = false;
		public float xScaleMult = 1f;
		public float yScaleMult = 1f;
        public bool additive = false;

        float timer = 0;
        public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.timeLeft = 2200;
			Projectile.tileCollide = false;
		}

		public override bool? CanDamage() => false;

		public override void AI()
        {
			Player player = Main.player[Projectile.owner];

			//Random rot of flare
			if (timer == 1)
				Projectile.ai[0] = Main.rand.NextFloat(6.28f);


			int timeForAnim = 9;
			progress = Math.Clamp((float)timer / (float)timeForAnim, 0f, 1f);

			if (progress == 1f)
				Projectile.active = false;

			timer++;
		}


		float progress = 0f;
		public override bool PreDraw(ref Color lightColor)
		{
			if (pixelize)
                ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () => { DrawProj(); });
			else
                DrawProj();

            return false;
		}

		//In a separate function to be a little cleaner
		public void DrawProj()
		{
            if (additive)
                return;

            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Texture2D line = CommonTextures.SoulSpike.Value;

            float inverseProg = 1f - progress;
			float easedInverseProg = Easings.easeInQuad(inverseProg);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float[] scales = { 1f, 1.6f, 2.5f };

            //Orb
            float orbAlpha = 1f;
            float orbScale = 0.35f * inverseProg * Projectile.scale;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, 0f, orb.Size() / 2f, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, 0f, orb.Size() / 2f, orbScale * scales[1], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, 0f, orb.Size() / 2f, orbScale * scales[2], SpriteEffects.None);


            //Line
            Vector2 lineScale = new Vector2(4f - (3.5f * easedInverseProg), 0.25f + 0.85f * easedInverseProg) * Projectile.scale; //4f - 4f
			lineScale.X *= xScaleMult;
			lineScale.Y *= yScaleMult;
			float lineAlpha = Easings.easeOutCubic(inverseProg);
            float lineRot = Projectile.rotation;

            Main.EntitySpriteDraw(line, drawPos, null, cols[0] with { A = 0 } * lineAlpha, lineRot, line.Size() / 2f, lineScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(line, drawPos, null, cols[1] with { A = 0 } * lineAlpha, lineRot, line.Size() / 2f, lineScale * scales[1], SpriteEffects.None);
            Main.EntitySpriteDraw(line, drawPos, null, cols[2] with { A = 0 } * lineAlpha, lineRot, line.Size() / 2f, lineScale * scales[2], SpriteEffects.None);
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (additive)
			{
                Texture2D orb = CommonTextures.feather_circle128PMA.Value;
                Texture2D line = CommonTextures.SoulSpike.Value;
                Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Flare/flare_4Black").Value;

                float inverseProg = 1f - progress;
                float easedInverseProg = Easings.easeInQuad(inverseProg);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                float[] scales = { 1f, 1.6f, 2.5f };

                //Orb
                float orbAlpha = 1f;
                float orbScale = 0.35f * inverseProg * Projectile.scale;

                Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 255 } * orbAlpha, 0f, orb.Size() / 2f, orbScale * scales[0], SpriteEffects.None);
                Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 255 } * orbAlpha, 0f, orb.Size() / 2f, orbScale * scales[1], SpriteEffects.None);
                Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 255 } * orbAlpha, 0f, orb.Size() / 2f, orbScale * scales[2], SpriteEffects.None);


                //Line
                Vector2 lineScale = new Vector2(4f - (3.5f * easedInverseProg), 0.25f + 0.85f * easedInverseProg) * Projectile.scale; //4f - 4f
                lineScale.X *= xScaleMult;
                lineScale.Y *= yScaleMult;
                float lineAlpha = Easings.easeOutCubic(inverseProg);
                float lineRot = Projectile.rotation;

                Main.EntitySpriteDraw(line, drawPos, null, cols[0] with { A = 255 } * lineAlpha, lineRot, line.Size() / 2f, lineScale * scales[0], SpriteEffects.None);
                Main.EntitySpriteDraw(line, drawPos, null, cols[1] with { A = 255 } * lineAlpha, lineRot, line.Size() / 2f, lineScale * scales[1], SpriteEffects.None);
                Main.EntitySpriteDraw(line, drawPos, null, cols[2] with { A = 255 } * lineAlpha, lineRot, line.Size() / 2f, lineScale * scales[2], SpriteEffects.None);
            }
        }
    }
}
