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

    public class MeteorBulletProjOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MeteorShot);
        }

        public bool isBlue;

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
            }

            Color col = Color.Brown;

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value; //spark_07_Black |Extra_196_Black
            trail1.trailPointLimit = 120 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(20 * totalAlpha * Easings.easeOutCubic(1f - justTileCollidePower));
            trail1.trailMaxLength = 120 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = col * totalAlpha * 0.75f * 1f;

            trail1.timesToDraw = 2;

            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();


            if (timer > 5 && timer % 3 == 0 && Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.25f, 0.08f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);
            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

            proj = projectile;

            justTileCollidePower = Math.Clamp(MathHelper.Lerp(justTileCollidePower, -0.75f, 0.11f), 0f, 1f); 

            timer++;
            return base.PreAI(projectile);
        }

        float totalScale = 0f;
        float totalAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Dont draw on frame one
            if (timer == 0)
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
                if (projectile.active == false)
                    totalAlpha = 0f;

                float easedJustHitPower = Easings.easeOutCirc(1f - justTileCollidePower);

                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
                Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;


                Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10 * easedJustHitPower);
                drawPos += new Vector2(0f, 0f);

                float drawRot = projectile.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = projectile.scale * (5f / 6f);

                Color spikeCol = Color.Brown;
                Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f * easedJustHitPower, adjustedScale * 1.5f * totalScale) * 0.5f;
                
                Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, spikeCol with { A = 50 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

                Color orbCol = Color.Red;
                Vector2 orbScale = new Vector2(1f * easedJustHitPower, 0.25f * totalScale) * 0.7f * adjustedScale; //0.3
                Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, orbCol with { A = 50 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


                Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

                Vector2 drawScale2 = new Vector2(adjustedScale * 2f * easedJustHitPower, adjustedScale * totalScale) * 0.5f;

                Color col = new Color(244, 40, 60);

                drawPos += new Vector2(0f, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, col with { A = 50 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 50 } * totalAlpha, drawRot, drawOrigin, drawScale2 * 0.5f, SpriteEffects.None, 0f);
            });

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
                if (projectile.active == false)
                    totalAlpha = 0f;

                float easedJustHitPower = Easings.easeOutCirc(1f - justTileCollidePower);

                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;

                Vector2 drawPos = proj.Center - Main.screenPosition + (proj.velocity.SafeNormalize(Vector2.UnitX) * -10 * easedJustHitPower);
                drawPos += new Vector2(0f, 100f);

                float drawRot = proj.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = proj.scale * (5f / 6f);
                Vector2 drawScale = new Vector2(adjustedScale * 2f * easedJustHitPower, adjustedScale * totalScale) * 0.5f;

                Color col = new Color(244, 40, 60);

                //Main.spriteBatch.Draw(spike, drawPos, null, col * totalAlpha, drawRot, drawOrigin, drawScale, SpriteEffects.None, 0f);
                //Main.spriteBatch.Draw(spike, drawPos, null, Color.White * totalAlpha, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None, 0f);

            });

            return false;
        }

        //Need this for DrawAdditve to have projectile pos and stuff
        Projectile proj = null;
        public void DrawAdditive(SpriteBatch sb)
        {
            if (proj == null)
                return;
            trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color col = new Color(162, 100, 60);


            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
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

            return false;
        }

        float justTileCollidePower = 0f;
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, oldVelocity, projectile.width, projectile.height);

            float rot = Main.rand.NextFloat(6.28f);
            Color starCol = Color.Brown;
            DustBehaviorUtil.StarDustDrawInfo sdic = new DustBehaviorUtil.StarDustDrawInfo(true, false, false, true, false, 1f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center + oldVelocity * 0.6f, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: starCol, Scale: 0.8f);
            d1.rotation = 0f + rot;
            d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: true, sdci: sdic);

            Dust d2 = Dust.NewDustPerfect(projectile.Center + oldVelocity * 0.6f, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: starCol, Scale: 0.8f);
            d2.rotation = MathHelper.PiOver4 + rot;
            d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: true, sdci: sdic);

            trail1.trailPositions.Clear();
            trail1.trailRotations.Clear();

            justTileCollidePower = 1f;

            for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center + oldVelocity * 0.6f, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f) * 1f,
                    newColor: starCol * 1f, Scale: Main.rand.NextFloat(0.2f, 0.4f));

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(shouldFadeColor: false);
            }

            return base.OnTileCollide(projectile, oldVelocity);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: Color.Brown * 1f, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }
            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

}
