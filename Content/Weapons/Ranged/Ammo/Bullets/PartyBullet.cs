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
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Interfaces;
using Terraria.GameContent.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Bullets
{

    public class PartyBulletProjOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PartyBullet);
        }

        Color thisCol = Color.Red;

        float randomTrailSpeed = 1f;
        float randomTimeOffset = 0;
        int trailRandomLengthOffset = 0;
        BaseTrailInfo trail1 = new BaseTrailInfo();
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //We want each bullet to feel a little different, so we randomize the length of the trail a little bit and offset the trail time
            //Without this, bullets can often feel very weird when fired at the same (shotguns)
            if (timer == 0)
            {
                randomTimeOffset = Main.rand.NextFloat(0f, 10f);
                trailRandomLengthOffset = Main.rand.Next(0, 35);
                randomTrailSpeed = Main.rand.NextFloat(0.85f, 1.15f);

                projectile.ai[2] = Main.rand.NextFloat();

                projectile.light = 0f;
            }

            Color col = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value;
            trail1.trailPointLimit = 135 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(12 * totalAlpha);
            trail1.trailMaxLength = 135 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = Main.hslToRgb(projectile.ai[2], 0.9f, 0.5f) with { A = 70 } * totalAlpha * 0.5f;


            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();



            int trailCount = 14; //34
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer > 5 && timer % 3 == 0 && Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Main.hslToRgb(projectile.ai[2], 0.9f, 0.5f), Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.25f, 0.08f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);
            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

            Lighting.AddLight(projectile.Center, Main.hslToRgb(projectile.ai[2], 0.9f, 0.5f).ToVector3() * 0.7f);

            timer++;
            return base.PreAI(projectile);
        }

        float totalScale = 0f;
        float totalAlpha = 0f;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Dont draw on frame one
            if (timer == 0)
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                trail1.TrailDrawing(Main.spriteBatch, false);

                Color col = Main.hslToRgb(projectile.ai[2], 0.9f, 0.5f);

                Color darkest = Main.hslToRgb(projectile.ai[2], 0.9f, 0.5f);
                Color middle = Main.hslToRgb(projectile.ai[2], 0.9f, 0.58f);
                Color brightest = Main.hslToRgb(projectile.ai[2], 0.9f, 0.65f);

                //Color darkest = new Color(69, 130, 0);
                //Color middle = new Color(87, 153, 0);
                //Color brightest = new Color(87, 153, 0);

                //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
                if (projectile.active == false)
                    totalAlpha = 0f;

                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
                Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;


                Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10f);
                drawPos += new Vector2(0f, 0f);

                float drawRot = projectile.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = projectile.scale * (5f / 6f);

                Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale) * 0.5f;

                Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, darkest with { A = 130 } * 0.4f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

                Vector2 orbScale = new Vector2(1f, 0.25f * totalScale) * 0.7f * adjustedScale; //0.3
                Main.EntitySpriteDraw(orb, drawPos, null, middle with { A = 130 } * 0.25f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


                Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

                Vector2 drawScale2 = new Vector2(adjustedScale * 2f, adjustedScale * totalScale) * 0.5f;

                drawPos += new Vector2(0f, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, brightest with { A = 130 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 130 } * totalAlpha, drawRot, drawOrigin, drawScale2 * 0.5f, SpriteEffects.None, 0f);
            });
            return false;
        }

        public void DrawAdditive(SpriteBatch sb)
        {

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color col = Main.hslToRgb(projectile.ai[2], 0.9f, 0.5f); ;// Main.rand.NextBool() ? Color.HotPink : Color.SkyBlue;

            float randomRot = Main.rand.NextFloat(6.28f);

            int dustCount = 20; //40

            int crossCountA = 5; //10
            for (int i = 0; i < crossCountA; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCountA) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3f, 6.5f) * 1f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color dustCol = Main.hslToRgb(projectile.ai[2] /*Main.rand.NextFloat()*/, 1f, 0.5f);

                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), dustVel, newColor: dustCol, Scale: Main.rand.NextFloat(0.25f, 0.55f) * 1.5f);
                gd.velocity -= projectile.oldVelocity * 0.4f;

                gd.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.2f, timeBeforeSlow: 5,
                    preSlowPower: 0.97f, postSlowPower: 0.88f, velToBeginShrink: 3f, fadePower: 0.95f);

            }

            int crossCount = 4;
            for (int i = 0; i < crossCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3.5f, 6.5f);
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.55f) * 0.85f);
                gd.velocity -= projectile.oldVelocity * 0.4f;

                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.92f, shouldFadeColor: false);
            }

            int pgoCount = 7;
            for (int i = 220; i < pgoCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)pgoCount) * i;

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), newColor: col, Scale: Main.rand.NextFloat(0.55f, 0.8f));
                d.velocity = dir.ToRotationVector2() * Main.rand.NextFloat(0.5f, 6f);
                d.velocity = d.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));

                d.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.04f, timeBeforeSlow: 4, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.8f, colorFadePower: 1f, glowIntensity: 0.4f);
            }


            //Particles on trail
            int count = trail1.trailPositions.Count;
            for (int i = (int)(count * 0.3f); i < count; i += 5)
            {
                if (Main.rand.NextBool())
                {
                    
                    Vector2 pos = trail1.trailPositions[i];
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
                    d.alpha = 2;
                    d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.35f;
                    d.velocity *= 0.35f;
                }
            }


            #region vanillaKill but less dust
            for (int num666 = 220; num666 < 2; num666++)
            {
                int num667 = Main.rand.Next(139, 143);
                int num668 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num667, (0f - projectile.velocity.X) * 0.3f, (0f - projectile.velocity.Y) * 0.3f, 0, default(Color), 1.2f);
                Main.dust[num668].velocity.X += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.X += (float)Main.rand.Next(-50, 51) * 0.05f;
                Main.dust[num668].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.05f;
                Dust dust163 = Main.dust[num668];
                Dust dust334 = dust163;
                dust334.scale *= 1f + (float)Main.rand.Next(-30, 31) * 0.01f;
            }
            for (int num669 = 220; num669 < 3; num669++)
            {
                int num670 = Main.rand.Next(276, 283);
                int num671 = Gore.NewGore(null, projectile.position, -projectile.velocity * 0.3f, num670);
                Main.gore[num671].velocity.X += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.gore[num671].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.gore[num671].velocity.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.gore[num671].velocity.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Gore gore39 = Main.gore[num671];
                Gore gore64 = gore39;
                gore64.scale *= 1f + (float)Main.rand.Next(-20, 21) * 0.01f;
                Main.gore[num671].velocity.X += (float)Main.rand.Next(-50, 51) * 0.05f;
                Main.gore[num671].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.05f;
            }
            #endregion

            for (int i = 220; i < 4 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4.25f, 4.25f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f) * 1.25f,
                    newColor: Color.DeepPink * 1f, Scale: Main.rand.NextFloat(0.25f, 0.45f));

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, postSlowPower: 0.91f, shouldFadeColor: false);
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
