using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;

namespace VFXPlus.Content.Dusts
{
    public class PulseInOutDust : ModDust
    {
        public override string Texture => "VFXPlus/Assets/Pixel";

        public override void OnSpawn(Dust dust)
        {
            dust.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.ShakyStar, 18, 0.5f, 0.5f, Pixelize: true);
            dust.noLight = true;
        }

        public override bool Update(Dust dust)
        {
            PulseInOutDustBehavior behavoir = (PulseInOutDustBehavior)dust.customData;

            float progress = Utils.GetLerpValue(0, behavoir.totalTime, behavoir.timer, true);
            behavoir.drawScale = GeneralUtilities.FadeLinear(progress, behavoir.inAmount, behavoir.outAmount);

            if (behavoir.timer == behavoir.totalTime)
                dust.active = false;

            if (!dust.noLight || true)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.4f * dust.scale * behavoir.drawScale);

            if (behavoir.drawType == PulseInOutDustBehavior.DrawOptions.ShakyStar)
            {
                dust.rotation += dust.velocity.X * 0.03f;
                dust.velocity *= 0.9f;
            }


            dust.position += dust.velocity;

            behavoir.timer++;
            return false;
        }


        public override bool PreDraw(Dust dust)
        {
            if ((dust.customData as PulseInOutDustBehavior).pixelize)
            {
                ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
                {
                    Draw(dust);
                });
            }
            else
            {
                Draw(dust);
            }
            return false;
        }

        public void Draw(Dust dust)
        {
            if (dust.customData == null || !dust.customData.GetType().Equals(typeof(PulseInOutDustBehavior)))
                return;

            PulseInOutDustBehavior behavoir = (PulseInOutDustBehavior)dust.customData;

            Texture2D Tex = behavoir.texture;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = Tex.Size() / 2f;

            if (behavoir.drawType == PulseInOutDustBehavior.DrawOptions.GlowStarSharp)
            {
                Main.spriteBatch.Draw(Tex, drawPos, null, dust.color, dust.rotation, origin, dust.scale * behavoir.drawScale * 0.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex, drawPos, null, Color.White with { A = dust.color.A }, dust.rotation, origin, dust.scale * behavoir.drawScale * 0.25f, SpriteEffects.None, 0f);

            }
            else if (behavoir.drawType == PulseInOutDustBehavior.DrawOptions.GlowPixel)
            {
                Main.spriteBatch.Draw(Tex, drawPos, null, dust.color, dust.rotation, origin, dust.scale * behavoir.drawScale, SpriteEffects.None, 0f);
            }
            else if (behavoir.drawType == PulseInOutDustBehavior.DrawOptions.ShakyStar)
            {
                Tex = ModContent.Request<Texture2D>("VFXPlus/Assets/SmallAACircle", AssetRequestMode.ImmediateLoad).Value;
                Texture2D Tex2 = ModContent.Request<Texture2D>("VFXPlus/Assets/SmallAACircleGlow", AssetRequestMode.ImmediateLoad).Value;

                origin = Tex.Size() / 2f;


                float adjustedScale = 0.9f + MathF.Sin(((float)Main.timeForVisualEffects * 0.2f) + dust.dustIndex) * 0.1f;

                Main.spriteBatch.Draw(Tex2, drawPos, null, dust.color with { A = 70 } * 0.85f, dust.rotation, origin, dust.scale * behavoir.drawScale * adjustedScale * 1.5f, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex, drawPos, null, dust.color with { A = 70 } * 1f, dust.rotation, origin, dust.scale * behavoir.drawScale * adjustedScale * 1.5f, SpriteEffects.None, 0f);
            }
        }
    }

    public class PulseInOutDustBehavior
    {
        public Texture2D texture;

        public DrawOptions drawType;

        public enum DrawOptions
        {
            GlowPixel = 1,
            GlowStarSharp = 2,
            ShakyStar = 3,
        }

        public int totalTime;

        //These two should total up to 1
        public float inAmount = 0.5f;
        public float outAmount = 0.5f;

        public int timer = 0;
        public float drawScale = 1f;

        public bool pixelize = false;

        public PulseInOutDustBehavior(DrawOptions DrawType, int TotalTime, float InAmount, float OutAmount, bool Pixelize = false)
        {
            totalTime = TotalTime;
            inAmount = InAmount;
            outAmount = OutAmount;
            pixelize = Pixelize;

            drawType = DrawType;
            if (drawType == DrawOptions.GlowPixel)
                texture = ModContent.Request<Texture2D>("VFXPlus/Content/Dusts/Textures/PixelGlow", AssetRequestMode.ImmediateLoad).Value;
            else if (drawType == DrawOptions.GlowStarSharp)
                texture = CommonTextures.CrispStarPMA.Value;
            else if (drawType == DrawOptions.ShakyStar)
                texture = ModContent.Request<Texture2D>("VFXPlus/Assets/PartyStar", AssetRequestMode.ImmediateLoad).Value;
            else
                texture = CommonTextures.CrispStarPMA.Value;

        }
    }

}