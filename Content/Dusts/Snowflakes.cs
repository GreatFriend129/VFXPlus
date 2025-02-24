using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Terraria.Map;

namespace VFXPlus.Content.Dusts
{
    public class Snowflakes : ModDust
    {
        public override string Texture => "VFXPlus/Content/Dusts/Textures/Snowflakes";

        public override void OnSpawn(Dust dust)
        {
            //alpha is used as a timer, fadeIn is used as opacity
            dust.alpha = 0;
            dust.fadeIn = 1f;

            dust.customData = null;
        }
        
        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
            {
                dust.customData = new SnowflakeBehavior(0.94f, 0.93f, 0.96f, 1f);
            }

            if (dust.customData is SnowflakeBehavior sb)
            {
                dust.velocity *= sb.velShrinkAmount;
                dust.scale *= sb.scaleShrinkAmount;
                dust.fadeIn *= sb.alphaShrinkAmount;

                if (dust.scale <= 0.03f)
                    dust.active = false;

                if (dust.fadeIn <= 0.05f)
                    dust.active = false;
            }

            dust.position += dust.velocity;
            dust.rotation += dust.velocity.X * 0.04f;

            dust.alpha++;
            return false;
        }


        public override bool PreDraw(Dust dust)
        {
            Texture2D TexMain = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Snowflakes").Value;
            Texture2D TexGlow = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/SnowflakesGlow").Value;
            Texture2D TexOrb = CommonTextures.SoftGlow64.Value;

            if (dust.customData is SnowflakeBehavior sb)
            {
                if (sb.currentFrame == -1)
                    Main.NewText("How did this happen");

                Vector2 drawPos = dust.position - Main.screenPosition;

                int frameHeight = TexMain.Height / 3;
                int startY = frameHeight * (int)sb.currentFrame;
                Rectangle sourceRectangle = new Rectangle(0, startY, TexMain.Width, frameHeight);
                Vector2 origin = sourceRectangle.Size() / 2f;

                //Color lightColor = Lighting.GetColor((int)dust.position.X / 16, (int)dust.position.Y / 16);
                //Color col = (lightColor.MultiplyRGBA(dust.color) * sb.totalOpacity);
                Main.EntitySpriteDraw(TexOrb, drawPos, null, Color.SkyBlue with { A = 0 } * dust.fadeIn * 0.4f, dust.rotation, TexOrb.Size() / 2f, dust.scale * 0.8f, SpriteEffects.None);


                Main.EntitySpriteDraw(TexGlow, drawPos, sourceRectangle, Color.DeepSkyBlue with { A = 0 } * dust.fadeIn, dust.rotation, origin, dust.scale * 1f, SpriteEffects.None);
                Main.EntitySpriteDraw(TexGlow, drawPos, sourceRectangle, Color.DeepSkyBlue with { A = 0 } * dust.fadeIn, dust.rotation, origin, dust.scale * 1f, SpriteEffects.None);
                Main.EntitySpriteDraw(TexMain, drawPos, sourceRectangle, Color.White with { A = 0 } * dust.fadeIn * 1f, dust.rotation, origin, dust.scale * 1f, SpriteEffects.None);
            }

            return false;
        }

    }

    public class SnowflakeBehavior
    {
        public int currentFrame = -1;

        public float velShrinkAmount = 0.98f;
        public float scaleShrinkAmount = 0.97f;
        public float alphaShrinkAmount = 0.99f;
        public float colorIntensity = 1f;

        public float whiteIntensity = 1f;

        public SnowflakeBehavior(float VelShrinkAmount = 0.98f, float ScaleShrinkAmount = 0.97f, float AlphaShrinkAmount = 0.99f, float ColorIntensity = 1f)
        {
            velShrinkAmount = VelShrinkAmount;
            scaleShrinkAmount = ScaleShrinkAmount;
            alphaShrinkAmount = AlphaShrinkAmount;
            colorIntensity = ColorIntensity;

            currentFrame = Main.rand.Next(0, 3);
        }

    }

}