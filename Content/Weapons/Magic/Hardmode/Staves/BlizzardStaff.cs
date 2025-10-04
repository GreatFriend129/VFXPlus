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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Reflection.Metadata;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{

    public class BlizzardStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Blizzard) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BlizzardStaffToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 24; //11 | 18

            if (timer % 1 == 0)
            {
                previousRotations.Add(projectile.rotation);
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);

                if (timer % 6 == 0 && Main.rand.NextBool(2))
                {
                    Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0f);

                    Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: col * 1f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * 1.35f);
                    d.velocity += projectile.velocity * 0.2f;
                    d.alpha = 2;
                    d.rotation = Main.rand.NextFloat(6.28f);
                }


            }


            if (timer < 7)
            {
                Vector2 vel2 = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.15f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel2, newColor: col2 * 0.75f, Scale: Main.rand.NextFloat(0.3f, 0.65f));
                d2.velocity += projectile.velocity.RotatedByRandom(0.75f) * 0.7f;

                HighResSmokeBehavior hrsb = new HighResSmokeBehavior();
                hrsb.velSlowAmount = 0.89f;
                hrsb.overallAlpha = 0.7f;
                hrsb.isPixelated = true;
                d2.customData = hrsb;
            }

            if (timer % 3 == 0 && Main.rand.NextBool(2) && timer > 3 && false)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 2f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f) + projectile.velocity * 0.25f;

                Dust dp = Dust.NewDustPerfect(dustPos, ModContent.DustType<Snowflakes>(), dustVel, newColor: Color.White, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.5f);
                dp.rotation = Main.rand.NextFloat(6.28f);
                SnowflakeBehavior sb = new SnowflakeBehavior(VelShrinkAmount: 0.93f, ScaleShrinkAmount: 0.91f, AlphaShrinkAmount: 0.92f, ColorIntensity: 1f);
                dp.customData = sb;
            }


            Lighting.AddLight(projectile.Center, Color.SkyBlue.ToVector3() * 0.7f);

            //float timeForPopInAnim = 30;
            //float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            //drawScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1f));

            timer++;


            #region vanillaCode
            if (projectile.position.Y > Main.player[projectile.owner].position.Y - 300f)
            {
                projectile.tileCollide = true;
            }
            if ((double)projectile.position.Y < Main.worldSurface * 16.0)
            {
                projectile.tileCollide = true;
            }
            projectile.frame = (int)projectile.ai[1];
            if (Main.rand.Next(2) == 0 && false)
            {
                int num116 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 197);
                Main.dust[num116].velocity *= 0.5f;
                Main.dust[num116].noGravity = true;
            }
            #endregion

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            return false;
            return base.PreAI(projectile);
        }


        float drawScale = 1f;
        List<float> previousRotations = new List<float>();
        List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(projectile, false);
            });
            DrawAfterImage(projectile, true);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 10f).RotatedBy(projectile.rotation);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 2f;

                Vector2 offset2 = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset2.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                float opacitySquared = projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.White with { A = 0 } * 0.35f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * 1f, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * 0.65f, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);


            for (int i = 0; i < 3; i++)
            {
                //Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3f, 3f), sourceRectangle, Color.White with { A = 0 } * 0.05f, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);
            }

            /*
             *             //Orb
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color[] cols = { Color.LightSkyBlue * 0.75f, Color.SkyBlue * 0.525f, Color.DeepSkyBlue * 0.375f };
            float[] scales = { 1.15f, 1.6f, 2.5f };

            float orbRot = projectile.rotation;
            float orbAlpha = 0.3f;
            Vector2 orbScale = new Vector2(0.4f, 1f) * 0.3f * projectile.scale;
            Vector2 orbOrigin = orb.Size() / 2f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[2] * sineScale2, SpriteEffects.None);
            */
            return false;
        }

        public void DrawAfterImage(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 offset = new Vector2(0f, 10f).RotatedBy(projectile.rotation);
            Vector2 drawPos = projectile.Center - Main.screenPosition + offset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            Color between = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 0.75f);
            Color between2 = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 1f);
            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;


                float offsetIntensity = (1.5f * (1f - progress)) + 2.5f;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(offsetIntensity * 0f, offsetIntensity * 0f);

                float startScale = 1f + sineScale;

                Color col = Color.Lerp(between, Color.SkyBlue, 1f - progress);

                float easedFadeValue = progress * progress;


                Vector2 lineScale = new Vector2(0.5f, (0.45f * progress) * drawScale * 2f); //0.5
                Vector2 lineScale2 = new Vector2(0.5f, (0.25f * progress) * drawScale * 2f); //0.2f

                //Main
                Main.EntitySpriteDraw(line, AfterImagePos - offset, null, col with { A = 0 } * 0.15f * easedFadeValue, //0.25
                    previousRotations[i] + MathHelper.PiOver2, line.Size() / 2f, lineScale * startScale, SpriteEffects.None);

                //White
                Main.EntitySpriteDraw(line, AfterImagePos - offset, null, col with { A = 0 } * 0.5f * easedFadeValue, //1f
                    previousRotations[i] + MathHelper.PiOver2, line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_1") with { Volume = .05f, Pitch = 0.9f, PitchVariance = 0.3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .27f, Pitch = .7f, PitchVariance = 0.2f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            for (int i = 0; i < 4; i++)
            {
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.15f);

                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel, newColor: col2, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1.15f);
                HighResSmokeBehavior hrsb = new HighResSmokeBehavior();
                hrsb.overallAlpha = 0.3f;
                hrsb.isPixelated = true;
                d.customData = hrsb;
            }

            Color betweenBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);
            for (int i = 0; i < 5 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: betweenBlue * 0.65f, Scale: Main.rand.NextFloat(0.15f, 0.3f) * projectile.scale);

                p.velocity += projectile.velocity * 0.05f;
            }

            return false;
        }

    }
}
