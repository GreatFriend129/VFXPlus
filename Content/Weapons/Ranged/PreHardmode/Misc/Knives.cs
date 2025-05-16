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
using rail;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class ThrowingKnife : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ThrowingKnife);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 14; 
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 12f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, true);
            });
            DrawTrail(projectile, false);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    new Color(150, 150, 120) with { A = 0 } * 0.75f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Color thisGray = new Color(150, 150, 120);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(thisGray, Color.Gray, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            #region vanillaKill
            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num631 = 0; num631 < 8; num631++)
            {
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, 1, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 0, default(Color), 0.75f);

                if (num631 < 4)
                    Main.dust[a].noGravity = true;
            }
            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = 7f;
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: new Color(120, 120, 90), Scale: Main.rand.NextFloat(0.35f, 0.4f));
                dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.35f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

    public class PoisonedKnife : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PoisonedKnife);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 12f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, true);
            });
            DrawTrail(projectile, false);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    new Color(120, 170, 120) with { A = 0 } * 0.75f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Color thisGray = new Color(120, 170, 120);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(thisGray, Color.Gray, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            #region vanillaKill
            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num631 = 0; num631 < 8; num631++)
            {
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, 1, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 0, default(Color), 0.75f);

                if (num631 < 4)
                    Main.dust[a].noGravity = true;
            }
            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = 7f;
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: new Color(80, 130, 80), Scale: Main.rand.NextFloat(0.35f, 0.4f));
                dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.35f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

    public class FrostDaggerfish : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FrostDaggerfish);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 10f) / 26f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            if (timer % 3 == 0 && Main.rand.NextBool(2) && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.35f;

                Dust dp = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: Color.SkyBlue, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.5f);
                dp.alpha = 2;
            }

            if (timer % 4 == 0 && timer > 5) //4
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<HighResSmoke>(), newColor: Color.LightSkyBlue, Scale: Main.rand.NextFloat(0.25f, 0.35f) * 0.9f);
                Main.dust[d].velocity += projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;

                HighResSmokeBehavior hrsb = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.35f); //0.5
                hrsb.isPixelated = true;
                Main.dust[d].customData = hrsb;
            }

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, true);
            });
            DrawTrail(projectile, false);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.SkyBlue with { A = 0 } * 0.15f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * 0.6f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.LightSkyBlue with { A = 0 } * 0.35f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.White, Color.White, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 3f);

                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.15f, 0.3f));
                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.92f, shouldFadeColor: false);
            }

            for (int i = 0; i < 1 + Main.rand.Next(0, 2); i++) //2 //0,3
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Snowflakes>(), vel, newColor: Color.White, Scale: Main.rand.NextFloat(0.35f, 0.55f) * 0.75f);
                dp.rotation = Main.rand.NextFloat(6.28f);

                float velFade = Main.rand.NextFloat(0.89f, 0.93f);
                SnowflakeBehavior sb = new SnowflakeBehavior(VelShrinkAmount: velFade, ScaleShrinkAmount: 0.91f, AlphaShrinkAmount: 0.92f, ColorIntensity: 1f);
                dp.customData = sb;
            }

            for (int i = 0; i < 3; i++)
            {
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.35f);

                Vector2 vel = Main.rand.NextVector2Circular(1.75f, 1.75f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel, newColor: col2, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1f);
                d.customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.65f);
            }

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .23f, Pitch = .7f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = 7f;
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.SkyBlue, Scale: Main.rand.NextFloat(0.35f, 0.4f));
                dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.35f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {


            return base.OnTileCollide(projectile, oldVelocity);
        }
    }

    public class BoneThrowingKnife : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BoneDagger);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 12f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, true);
            });
            DrawTrail(projectile, false);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    new Color(150, 150, 120) with { A = 0 } * 0.75f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Color thisGray = new Color(150, 150, 120);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(thisGray, Color.Gray, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {


            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = 7f;
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.SandyBrown, Scale: Main.rand.NextFloat(0.35f, 0.4f));
                dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.35f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

}
