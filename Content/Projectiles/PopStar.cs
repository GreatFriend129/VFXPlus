using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using System.Linq;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using System.Runtime.InteropServices;
using Terraria.GameContent;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Projectiles
{

    public class PopStar : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.timeLeft = 1000;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

        }

        public override bool? CanDamage() => false;

        public override bool? CanCutTiles() => false;

        int timer = 0;
        float scale = 1f;
        float alpha = 1f;

        public int timeForScaleIn = 5;
        public float scaleInAmount = 0.15f;
        public int timeForScaleOut = 16;
        public override void AI()
        {
            if (timer == 0)
            {
                //Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;
            }

            if (timer < 5)
            {
                scale += 0.15f;
            }
            else
            {
                float easeProg = Utils.GetLerpValue(0f, 1f, (timer - 5) / 16f, true);

                scale = MathHelper.Lerp(1.75f, 0f, Easings.easeOutCubic(easeProg));
                alpha = MathHelper.Lerp(1f, 0f, Easings.easeInQuad(easeProg));

                if (scale <= 0.1f)
                    Projectile.active = false;
            }

            timer++;
        }

        public Color starCol = Color.DeepPink;
        public float overallAlpha = 1f;
        public bool pixelize = false;
        public bool useCircleTex = true;

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawStar(!pixelize);
            });
            DrawStar(pixelize);


            return false;
        }

        public void DrawStar(bool giveUp = false)
        {
            if (giveUp)
                return;
            
            Texture2D Spike = CommonTextures.CrispStarPMA.Value;
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (useCircleTex)
            {
                float orbAlpha = 0.2f;
                float orbScale = 0.3f * Projectile.scale * scale;
                Vector2 orbOrigin = orb.Size() / 2f;

                float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
                float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

                Main.EntitySpriteDraw(orb, drawPos, null, Color.DeepPink with { A = 0 } * orbAlpha, 0f, orbOrigin, orbScale * sineScale2, SpriteEffects.None);
                Main.EntitySpriteDraw(orb, drawPos, null, Color.White with { A = 0 } * orbAlpha, 0f, orbOrigin, orbScale * 0.5f * sineScale1, SpriteEffects.None);
            }

            Vector2 origin = new Vector2(Spike.Width / 2f, Spike.Height / 2f);
            Vector2 scale2 = new Vector2(1f, 0.5f) * Projectile.scale * scale;

            Main.EntitySpriteDraw(Spike, drawPos, null, Color.DeepPink with { A = 0 } * alpha, Projectile.rotation, origin, scale2, SpriteEffects.None);
            Main.EntitySpriteDraw(Spike, drawPos, null, Color.Pink with { A = 0 } * alpha, Projectile.rotation, origin, scale2 * 0.75f, SpriteEffects.None);

            Main.EntitySpriteDraw(Spike, drawPos, null, Color.DeepPink with { A = 0 } * alpha, Projectile.rotation + MathHelper.PiOver2, origin, scale2, SpriteEffects.None);
            Main.EntitySpriteDraw(Spike, drawPos, null, Color.Pink with { A = 0 } * alpha, Projectile.rotation + MathHelper.PiOver2, origin, scale2 * 0.75f, SpriteEffects.None);

        }
    }
}
