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
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class NimbusRodMovingOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RainCloudMoving) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NimbusRodToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 10;
            
            //Add 2 afterimages this frame
            for (int i = 0; i < 2; i++)
            {
                Vector2 bonus = i == 0 ? Vector2.Zero : projectile.velocity * 0.5f;

                previousRotations.Add(projectile.rotation);
                previousPostions.Add(projectile.Center + bonus);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPostions.Count > trailCount)
                    previousPostions.RemoveAt(0);
            }
            

            int reps = timer < 4 ? 0 : 2;
            for (int i = 0; i < reps; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);

                Color col = Color.Lerp(Color.Blue, Color.DodgerBlue, 0.85f) * 0.4f;

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkBasic>(), dustVel, newColor: col with { A = 0 }, Scale: Main.rand.NextFloat(0.4f, 0.6f) * 1f);
                da.velocity -= projectile.velocity.RotatedByRandom(0.25f) * 0.25f;
            }

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            drawScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1f));

            timer++;

            return base.PreAI(projectile);
        }


        float drawScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.Lerp(Color.Blue, Color.DodgerBlue, progress) * progress * projectile.Opacity;

                    float size2 = 0.5f + (progress * 0.5f) * projectile.scale * drawScale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f,
                            previousRotations[i], TexOrigin, size2, SpriteEffects.None);
                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.DodgerBlue with { A = 0 } * 0.5f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.1f * drawScale, SpriteEffects.None);
            }

            //Main
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);

            return false;
        }

    }

    public class NimbusRodStationaryOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RainCloudRaining) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NimbusRodToggle;
        }

        float inFadePower = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //Play Sound and particle burst
            if (timer == 0)
            {
                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ENV_water_splash_01") with { Volume = 0.45f,  Pitch = .66f, PitchVariance = .15f, MaxInstances = 1 }; 
                SoundEngine.PlaySound(style, projectile.Center);

                for (int i = 0; i < 12; i++)
                {
                    Color col = Color.Lerp(Color.Blue, Color.DodgerBlue, 0.85f) * 0.4f;

                    int da = Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<ElectricSparkBasic>(), newColor: col with { A = 0 }, Scale: Main.rand.NextFloat(0.4f, 0.6f) * 1.3f);
                    Main.dust[da].velocity *= 1.35f;
                }
            }


            if (Main.rand.NextBool())
            {
                Color col = Color.Lerp(Color.Blue, Color.DodgerBlue, 0.85f) * 0.4f;

                int da = Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<ElectricSparkBasic>(), newColor: col with { A = 0 }, Scale: Main.rand.NextFloat(0.4f, 0.6f) * 1.25f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.94f, FadeVelPower: 0.93f, Pixelize: true, XScale: 1f, YScale: 0.5f, WhiteLayerPower: 0f, UnderGlowPower: 1f);
                esb.randomVelRotatePower = 0.1f;
            }

            //Initial burst 
            inFadePower = Math.Clamp(MathHelper.Lerp(inFadePower, 1.35f, 0.1f), 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        float drawScale = 1f;
        float drawAlpha = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Orb Bloom
            Texture2D glorb = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;

            Main.EntitySpriteDraw(glorb, projectile.Center - Main.screenPosition, null, Color.DodgerBlue with { A = 0 } * projectile.Opacity * 0.25f, 
                projectile.rotation, glorb.Size() / 2f, new Vector2(1.15f, 0.5f) * projectile.scale * 2f, SpriteEffects.None);



            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            float easeVal = Easings.easeInOutBack(inFadePower, 0f, 5f);
            Vector2 vec2Scale = new Vector2(easeVal, 1f + (0.35f * (1f - easeVal))) * projectile.scale;


            Color thisCol = Color.Lerp(Color.Blue, Color.DodgerBlue, 0.85f);

            for (int i = 0; i < 4; i++)
            {
                float dist = 5f * inFadePower;

                float[] offsetAmounts = [0.065f, 0.11f, 0.06f, 0.09f];
                float dir = i > 1 ? 1f : -1f;

                float sinScale = 1.05f + (float)Math.Sin(Main.timeForVisualEffects * 0.03f) * 0.18f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * offsetAmounts[i] * dir);

                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    thisCol with { A = 0 } * 0.55f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * sinScale * vec2Scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, vec2Scale, SpriteEffects.None);

            return false;

        }

    }

    public class NimbusRodRainOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RainFriendly) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NimbusRodToggle;
        }

        public override bool PreAI(Projectile projectile)
        {
            drawAlpha = Math.Clamp(MathHelper.Lerp(drawAlpha, 1.5f, 0.08f), 0f, 1f);
            Lighting.AddLight(projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.35f);

            return base.PreAI(projectile);
        }

        float drawAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float sineScale = 1f + (MathF.Sin((float)Main.timeForVisualEffects * 0.07f) * 0.05f);
            
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/MuraLine120x120").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            float rot = projectile.rotation + MathHelper.PiOver2;
            Color col = Color.Lerp(Color.DodgerBlue, Color.SkyBlue, 0.15f);
            Vector2 vec2Scale = new Vector2(1.15f, 0.6f) * projectile.scale * 0.4f * sineScale;
            
            Main.EntitySpriteDraw(line, drawPos, null, Color.Black * projectile.Opacity * 0.3f * drawAlpha, rot, line.Size() / 2f, vec2Scale * 1.25f, SpriteEffects.None);

            Main.EntitySpriteDraw(line, drawPos, null, col with { A = 0 } * projectile.Opacity * 0.75f * drawAlpha, rot, line.Size() / 2f, vec2Scale * 1.25f, SpriteEffects.None);
                
            Vector2 offset = new Vector2(0f, 10f * projectile.scale);
            Vector2 randomOffset = Main.rand.NextVector2Circular(1.5f, 1.5f);
            Main.EntitySpriteDraw(line, drawPos + offset + randomOffset, null, Color.DeepSkyBlue with { A = 0 } * projectile.Opacity * 0.4f * drawAlpha, rot, line.Size() / 2f, vec2Scale * 0.8f, SpriteEffects.None);

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++)
            {

                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f).RotatedBy(projectile.velocity.ToRotation());
                Dust d = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, projectile.height / 2f), ModContent.DustType<GlowFlare>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.35f, 0.8f) * 0.4f);
                d.velocity += new Vector2(0f, -1f);
                
                d.customData = new GlowFlareBehavior(GlowThreshold: 0.45f, GlowPower: 2.5f, TotalBoost: 0.95f);
            }

            return false;
        }

    }

}
