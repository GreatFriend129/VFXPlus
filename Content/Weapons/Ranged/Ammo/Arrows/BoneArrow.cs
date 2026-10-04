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


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class BoneArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BoneArrowFromMerchant);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 11 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            int EU = 1 + projectile.extraUpdates;

            //Want less dust when the arrow has extra updates (magic quiver)
            int mod = Math.Clamp(3 * EU, 3, 100);

            if (timer % mod == 0 && Main.rand.NextBool(2) && timer > 5 && false)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3f, 9f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 1f, newColor: Color.DarkSlateGray * 0.65f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.4f);
                dp.alpha = 13;

                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0f);
            }

            if (timer % mod == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-5f, 5f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 7f);

                Color col = new Color(120, 120, 90);// Main.rand.NextBool(2) ? new Color(114, 81, 56) : new Color(151, 107, 75); //new Color(120, 120, 90);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<WindLine>(), vel * 1f, newColor: col * 1.5f, Scale: Main.rand.NextFloat(1f, 1.3f));

                float velFadePower = Main.rand.NextFloat(0.9f, 0.93f);
                int shrinkTime = Main.rand.Next(2, 5);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: velFadePower, TimeToStartShrink: shrinkTime, ShrinkYScalePower: 0.85f, XScale: 0.5f, YScale: 0.5f, Pixelize: true);
                wlb.colorAlpha = 255;
                wlb.whiteCoreIntensity = 0f;

                dp.customData = wlb;
            }

            float fadeInTime = Math.Clamp((float)(timer + 5f) / 12f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);

            Color thisGray = new Color(120, 120, 90);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(projectile, false);
            });
            DrawAfterImage(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    new Color(120, 120, 90) with { A = 50 } * 0.45f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawAfterImage(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D Flare = CommonTextures.SoulSpikePMA.Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);

            Color thisGray = new Color(120, 120, 90);

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(thisGray, Color.Gray, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                //Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 70 } * progress * 0.25f,
                //    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPositions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(3f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(Flare, AfterImagePos, null, thisGray with { A = 170 } * 0.5f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, Flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color thisSilver = new Color(120, 120, 90);

            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Vector2 vel = projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3.5f, 11f);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineDust>(), vel.RotateRandom(0.6f) * 1f, newColor: thisSilver * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                dp.alpha = 12;
                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), VelFadeSpeed: 0.83f, SizeChangeSpeed: 0.98f, WhiteIntensity: 0.1f);
            }

            #region vanillaKill
            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num624 = 0; num624 < 7; num624++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 26, 0f, 0f, 0, default(Color), 0.9f);
            }
            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.penetrate != 1)
            {
                for (int i = 220; i < 5; i++)
                {
                    float arrowVel = 7f;
                    Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                    Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: new Color(120, 120, 90), Scale: Main.rand.NextFloat(0.35f, 0.45f));
                    dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.35f;

                    dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
                }

                int dustCount = 3 + Main.rand.Next(0, 3);
                for (int i = 0; i < dustCount; i++)
                {
                    float prog = (float)(i + 1f) / dustCount;
                    Color col = new Color(120, 120, 90) * 1.35f;// Main.rand.NextBool(2) ? new Color(114, 81, 56) : new Color(151, 107, 75);


                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 3.5f);

                    Dust p = Dust.NewDustPerfect(target.Center + vel, ModContent.DustType<WindLine>(), vel, newColor: col * 1f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 2f);
                    p.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * 4f;

                    float velFadePower = Main.rand.NextFloat(0.9f, 0.93f);
                    int shrinkTime = Main.rand.Next(2, 5);

                    WindLineBehavior wlb = new WindLineBehavior(VelFadePower: velFadePower, TimeToStartShrink: shrinkTime, ShrinkYScalePower: 0.85f, XScale: 0.5f, YScale: 0.5f, Pixelize: true);
                    wlb.colorAlpha = 255;
                    wlb.whiteCoreIntensity = 0f;

                    p.customData = wlb;
                }

            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.Center, oldVelocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
