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
using Terraria.GameContent;
using System.Threading;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class MagicalHarpShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            bool a = entity.type == ProjectileID.QuarterNote;
            bool b = entity.type == ProjectileID.EighthNote;
            bool c = entity.type == ProjectileID.TiedEighthNote;

            return lateInstantiation && (a || b || c) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.MagicHarpToggle;
        }

        float timeOffset = Main.rand.NextFloat(0f, 10f);

        float fadeInAlpha = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 12;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 5 == 0 && Main.rand.NextBool())
            {   
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(1f, 2f),
                    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity -= projectile.velocity * 0.5f;
            }

            float progress = Math.Clamp((timer + 9) / 45f, 0f, 1f);
            fadeInAlpha = MathHelper.Lerp(0f, 1f, progress);

            timer++;
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float totalAlpha = Easings.easeOutBack(fadeInAlpha);
            float totalScale = Easings.easeInOutBack(fadeInAlpha, 0f, 1.5f);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            float drawScale = projectile.scale * totalScale;

            //Orb
            Color purp = new Color(105, 63, 191);

            //Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color col1 = new Color(105, 63, 191) * 1f;
            Color col2 = new Color(77, 43, 130) * 1f;
            Color col3 = new Color(52, 21, 101) * 1f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            float scale = 0.25f * totalScale;

            //Main.EntitySpriteDraw(orb, drawPos, null, col1 with { A = 0 } * totalAlpha * 0.75f, 0f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            //Main.EntitySpriteDraw(orb, drawPos, null, col2 with { A = 0 } * totalAlpha * 0.75f, 0f, orb.Size() / 2f, scale2 * scale, SpriteEffects.None);
            //Main.EntitySpriteDraw(orb, drawPos, null, col3 with { A = 0 } * totalAlpha * 0.75f, 0f, orb.Size() / 2f, scale3 * scale, SpriteEffects.None);


            float animTime = (float)Main.timeForVisualEffects * 0.15f + timeOffset;
            float drawRot = (MathF.Sin(animTime / 2f) * 0.25f) + projectile.rotation * 0.5f;
            float scaleQuashVal = MathF.Sin(animTime) * 0.1f;
            Vector2 vecScale = drawScale * new Vector2(1f + scaleQuashVal, 1f - scaleQuashVal);

            float xPosOffset = MathF.Sin(animTime / 2) * 3f;
            float yPosOffset = MathF.Sin(animTime) * 1.5f;
            Vector2 posOffset = new Vector2(xPosOffset, yPosOffset);


            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Color[] cols = { purp, Color.Purple * 0.525f, Color.Purple * 0.375f };
            float[] scales = { 0.85f, 1.6f, 2.5f };

            Vector2 orbScale = vecScale * 0.25f * drawScale;
            float orbAlpha = totalAlpha * 0.5f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.13f) * 0.1f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.05f;

            Main.EntitySpriteDraw(orb, drawPos + posOffset, null, cols[0] with { A = 0 } * orbAlpha, drawRot, orb.Size() / 2f, scales[0] * orbScale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos + posOffset, null, cols[1] with { A = 0 } * orbAlpha, drawRot, orb.Size() / 2f, scales[1] * orbScale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos + posOffset, null, cols[2] with { A = 0 } * orbAlpha, drawRot, orb.Size() / 2f, scales[2] * orbScale * sineScale2, SpriteEffects.None);


            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.Lerp(Color.Purple, Color.MediumPurple, progress) * progress * totalAlpha;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos + posOffset, sourceRectangle, col with { A = 0 } * 0.75f, //0.5f
                        drawRot, TexOrigin, Easings.easeOutQuad(progress) * vecScale, SpriteEffects.None);

            }


            //Border
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = projectile.rotation.ToRotationVector2().RotatedBy(MathF.PI / 2f * i) * 2f;
                Main.EntitySpriteDraw(vanillaTex, drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.1f) + posOffset, null, Color.White with { A = 0 } * 2f, drawRot, TexOrigin, vecScale, 0f);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos + posOffset, sourceRectangle, Color.White * totalAlpha, drawRot, TexOrigin, vecScale, SpriteEffects.None);
            return false;

        }
    }
}
