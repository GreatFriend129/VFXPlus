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


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Bullets
{

    public class LuminiteBulletProjOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MoonlordBullet) && false;
        }

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
            //Color trailCol = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.4f) * 1f;

            Color trailCol = Color.Lerp(Color.SkyBlue, Color.Aqua, 0.4f) * 1f;

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value;
            trail1.trailPointLimit = 150 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(10 * totalAlpha * totalScale); //10
            trail1.trailMaxLength = 300 + trailRandomLengthOffset;

            trail1.shouldSmooth = false;

            trail1.trailColor = trailCol with { A = 50 } * totalAlpha * 0.6f * 1f;
            trail1.timesToDraw = 1;
            trail1.useEffectMatrix = true;
            trail1.pinchHead = true;

            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -50f) + new Vector2(0f, 0f);

            if (timer > 5)
                trail1.TrailLogic();

            if (timer > 0 && (timer + (int)randomTimeOffset) % 4 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.SkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.55f);
                d.alpha = 2;
                d.velocity += projectile.velocity.RotatedByRandom(0.1f) * 0.75f;
                d.velocity *= 0.35f;

            }

            if ((timer + (int)randomTimeOffset) % 6 == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(-projectile.velocity.Length() * 3f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(10f, 18f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<WindLine>(), vel, newColor: trailCol, Scale: Main.rand.NextFloat(0.65f, 0.85f));

                int KillEarlyTime = Main.rand.Next(8, 14);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.9f, TimeToStartShrink: 5, ShrinkYScalePower: 0.75f, XScale: 2f, YScale: Main.rand.NextFloat(0.75f, 1f), true, KillEarlyTime: KillEarlyTime);
                wlb.colorAlpha = 50;
                wlb.whiteCoreIntensity = 0.75f;
                dp.customData = wlb;
            }

            if (timer > 2 && false)
            {
                for (int i = 0; i < 1; i++)
                {
                    Dust daa = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RenderTargetDustTest>(), Vector2.Zero, Scale: Main.rand.NextFloat(0.85f, 1.15f));
                    daa.rotation = projectile.velocity.ToRotation();
                    daa.scale *= 0.75f;
                }

                
            }


            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.5f, 0.05f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);

            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

            Lighting.AddLight(projectile.Center, Color.SkyBlue.ToVector3() * 0.6f);

            proj = projectile;
            timer++;

            #region vanillaAI
            float num32 = projectile.velocity.Length();
            if (projectile.alpha > 0)
            {
                projectile.alpha -= (byte)((double)num32 * 0.3);
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            Rectangle hitbox = projectile.Hitbox;
            hitbox.Offset((int)projectile.velocity.X, (int)projectile.velocity.Y);
            bool flag2 = false;
            for (int num33 = 0; num33 < 200; num33++)
            {
                NPC nPC = Main.npc[num33];
                if (nPC.active && !nPC.dontTakeDamage && nPC.immune[projectile.owner] == 0 && projectile.localNPCImmunity[num33] == 0 && nPC.Hitbox.Intersects(hitbox) && !nPC.friendly)
                {
                    flag2 = true;
                    break;
                }
            }
            if (flag2)
            {
                int num35 = Main.rand.Next(3, 8);
                for (int num36 = 0; num36 < num35; num36++)
                {
                    int num37 = Dust.NewDust(projectile.Center + projectile.velocity, 0, 0, 229, 0f, 0f, 100, default(Color), 0.8f);
                    Main.dust[num37].velocity *= 1.6f;
                    Main.dust[num37].velocity.Y -= 1f;
                    Main.dust[num37].velocity += -projectile.velocity * 0.75f;
                    Main.dust[num37].noGravity = true;
                }
            }

            projectile.rotation = projectile.velocity.ToRotation();
            #endregion
            return false;
        }

        float totalScale = 0f;
        float totalAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Dont draw on frame one
            if (timer == 0)
                return false;

            //Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.4f)
            //Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.85f)
            //Color.SkyBlue;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                trail1.TrailDrawing(Main.spriteBatch, false);

                Color darkest = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.85f);
                Color middle = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.4f);
                Color brightest = Color.SkyBlue;

                //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
                if (projectile.active == false)
                    totalAlpha = 0f;

                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
                Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;

                Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -50f);
                drawPos += new Vector2(0f, 0f);

                float drawRot = projectile.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = projectile.scale * (5f / 6f);

                Vector2 outSpikeScale = new Vector2(adjustedScale * 7.53f, adjustedScale * 1.5f * totalScale) * 0.5f;

                Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, darkest with { A = 100 } * 0.4f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

                Vector2 orbScale = new Vector2(3.5f, 0.25f * totalScale) * 0.7f * adjustedScale; //0.3
                Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, middle with { A = 100 } * 0.2f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


                Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

                Vector2 drawScale2 = new Vector2(adjustedScale * 5f, adjustedScale * totalScale) * 0.5f;

                drawPos += new Vector2(0f, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, brightest with { A = 100 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 100 } * totalAlpha, drawRot, drawOrigin, drawScale2 * 0.5f, SpriteEffects.None, 0f);
            });


            return false;
        }

        //Need this for DrawAdditve to have projectile pos and stuff
        Projectile proj = null;
        public void DrawAdditive(SpriteBatch sb)
        {
            //if (proj == null || true)
            //    return;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color trailCol = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);


            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: trailCol, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color trailCol = Color.Lerp(Color.SkyBlue, Color.Aqua, 0.5f);
            //for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++) //2 //0,3
            //{
            //    Vector2 pos = projectile.Center;
            //    Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(5f, 25f);

            //    Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LineSpark>(), vel, newColor: trailCol, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.5f);

            //    dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.92f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 5 + Main.rand.Next(-5, 5), killEarlyTime: 80,
            //        1.15f, 0.75f); //80
            //}

            for (int i = 0; i < 6 + Main.rand.Next(0, 3); i++) //2 //0,3
            {
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f)) * Main.rand.NextFloat(5f, 15f);

                Dust dp = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<RenderTargetDustTest>(), vel * -0.5f, newColor: trailCol, Scale: 0.65f);
                dp.position += dp.velocity;
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    public class LuminiteBulletProjOverride2 : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MoonlordBullet);
        }

        Projectile trail = null;

        float randomTimeOffset = 0;
        int trailRandomLengthOffset = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //We want each bullet to feel a little different, so we randomize the length of the trail a little bit and offset the trail time
            //Without this, bullets can often feel very weird when fired at the same (shotguns)
            if (timer == 0)
            {
                trail = Projectile.NewProjectileDirect(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<LuminiteBulletTrail>(), 0, 0, projectile.owner);

                randomTimeOffset = Main.rand.NextFloat(0f, 10f);
                trailRandomLengthOffset = Main.rand.Next(0, 35);
            }

            if (timer % 2 == 0 && timer > 2)
                (trail.ModProjectile as LuminiteBulletTrail).AddPoint(projectile.Center, projectile.velocity);
            trail.position = projectile.Center;

            if (timer > 4 && (timer + (int)randomTimeOffset) % 3 == 0 && Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LunarDustCircle>(), vel, newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.35f, 0.45f) * 1f);
                d.velocity -= projectile.velocity.RotatedByRandom(0.1f) * 0.75f;
                d.velocity *= 0.35f;

            }

            if ((timer + (int)randomTimeOffset) % 6 == 0 && Main.rand.NextBool(2) && timer > 5 && false)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(-projectile.velocity.Length() * 3f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(10f, 18f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<WindLine>(), vel, newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.65f, 0.85f));

                int KillEarlyTime = Main.rand.Next(8, 14);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.9f, TimeToStartShrink: 5, ShrinkYScalePower: 0.75f, XScale: 2f, YScale: Main.rand.NextFloat(0.75f, 1f), true, KillEarlyTime: KillEarlyTime);
                wlb.colorAlpha = 50;
                wlb.whiteCoreIntensity = 0.75f;
                dp.customData = wlb;
            }

            if (timer > 2 && timer % 2 == 0 && false)
            {
                for (int i = 0; i < 1; i++)
                {
                    Dust daa = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LunarDustCircle>(), Vector2.Zero, Scale: Main.rand.NextFloat(0.85f, 1.15f));
                    //daa.rotation = projectile.velocity.ToRotation();
                    daa.scale *= 0.5f;
                    daa.velocity += projectile.velocity.RotatedByRandom(0.1f) * -0.75f;
                }


            }


            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.5f, 0.05f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);

            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

            Lighting.AddLight(projectile.Center, Color.SkyBlue.ToVector3() * 0.6f);

            proj = projectile;
            timer++;

            #region vanillaAI
            float num32 = projectile.velocity.Length();
            if (projectile.alpha > 0)
            {
                projectile.alpha -= (byte)((double)num32 * 0.3);
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            Rectangle hitbox = projectile.Hitbox;
            hitbox.Offset((int)projectile.velocity.X, (int)projectile.velocity.Y);
            bool flag2 = false;
            for (int num33 = 0; num33 < 200; num33++)
            {
                NPC nPC = Main.npc[num33];
                if (nPC.active && !nPC.dontTakeDamage && nPC.immune[projectile.owner] == 0 && projectile.localNPCImmunity[num33] == 0 && nPC.Hitbox.Intersects(hitbox) && !nPC.friendly)
                {
                    flag2 = true;
                    break;
                }
            }
            if (flag2)
            {
                int num35 = Main.rand.Next(3, 8);
                for (int num36 = 0; num36 < num35; num36++)
                {
                    int num37 = Dust.NewDust(projectile.Center + projectile.velocity, 0, 0, 229, 0f, 0f, 100, default(Color), 0.8f);
                    Main.dust[num37].velocity *= 1.6f;
                    Main.dust[num37].velocity.Y -= 1f;
                    Main.dust[num37].velocity += -projectile.velocity * 0.75f;
                    Main.dust[num37].noGravity = true;
                }
            }

            projectile.rotation = projectile.velocity.ToRotation();
            #endregion
            return false;
        }

        float totalScale = 0f;
        float totalAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Dont draw on frame one
            if (timer == 0)
                return false;

            //Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.4f)
            //Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.85f)
            //Color.SkyBlue;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                Color darkest = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.85f);
                Color middle = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.4f);
                Color brightest = Color.SkyBlue;

                //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
                if (projectile.active == false)
                    totalAlpha = 0f;

                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
                Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;

                Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -50f);
                drawPos += new Vector2(0f, 0f);

                float drawRot = projectile.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = projectile.scale * (5f / 6f);

                Vector2 outSpikeScale = new Vector2(adjustedScale * 7.53f, adjustedScale * 1.5f * totalScale) * 0.5f;

                ///Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, darkest with { A = 100 } * 0.4f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

                Vector2 orbScale = new Vector2(3.5f, 0.25f * totalScale) * 0.7f * adjustedScale; //0.3
                ///Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, middle with { A = 100 } * 0.2f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


                Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

                Vector2 drawScale2 = new Vector2(adjustedScale * 5f, adjustedScale * totalScale) * 0.5f;

                drawPos += new Vector2(0f, 0f);
                ///Main.spriteBatch.Draw(spike2, drawPos, null, brightest with { A = 100 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
                ///Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 100 } * totalAlpha, drawRot, drawOrigin, drawScale2 * 0.5f, SpriteEffects.None, 0f);
            });


            return false;
        }

        //Need this for DrawAdditve to have projectile pos and stuff
        Projectile proj = null;
        public void DrawAdditive(SpriteBatch sb)
        {
            //if (proj == null || true)
            //    return;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            trail.active = false;
            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color trailCol = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);


            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: trailCol, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color trailCol = Color.Lerp(Color.SkyBlue, Color.Aqua, 0.5f);
            //for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++) //2 //0,3
            //{
            //    Vector2 pos = projectile.Center;
            //    Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(5f, 25f);

            //    Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LineSpark>(), vel, newColor: trailCol, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.5f);

            //    dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.92f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 5 + Main.rand.Next(-5, 5), killEarlyTime: 80,
            //        1.15f, 0.75f); //80
            //}

            for (int i = 0; i < 6 + Main.rand.Next(0, 3); i++) //2 //0,3
            {
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f)) * Main.rand.NextFloat(5f, 15f);

                Dust dp = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<LunarDustCircle>(), vel * -0.5f, newColor: trailCol, Scale: 0.65f);
                dp.position += dp.velocity;
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    public class LuminiteBulletTrail : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 6250;
            Projectile.extraUpdates = 2;
        }


        public void AddPoint(Vector2 pos, Vector2 vel)
        {
            int trailCount = 30;
            previousRotations.Add(vel.ToRotation()); //
            previousPositions.Add(pos + vel);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
        }

        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        public override void AI()
        {
            float timeForPopInAnim = 23; //33
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.75f)) * 1f;

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {

            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("Playground/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            float sineWidthMult = 1f;// 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.1f;


            Color StripColor(float progress) => Color.White;

            float StripWidth(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.85f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.85f, progress, true);
                    toReturn = Easings.easeInOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.85f, 1f, progress, true);
                    toReturn = Easings.easeOutSine(1f - LV);
                }

                return toReturn * sineWidthMult * overallScale * 7; //20 | 8
            }


            VertexStripFixed vertexStrip = new VertexStripFixed();
            VertexStripFixed vertexStrip2 = new VertexStripFixed();

            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);
            myEffect.Parameters["posterizationSteps"].SetValue(0.0f);

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }

    }

    public class LunarBulletTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 2250; //180
            Projectile.extraUpdates = 2;
        }



        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        public override void AI()
        {
            int trailCount = 60;  //60
            Vector2 trailPos = Projectile.Center + Projectile.velocity;
            previousRotations.Add(Projectile.velocity.ToRotation()); //
            previousPositions.Add(trailPos);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer > 4 && (timer % 3 == 0 && Main.rand.NextBool(2)))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RenderTargetDustTest>(), vel, newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.35f, 0.45f) * 1f);
                d.velocity -= Projectile.velocity.RotatedByRandom(0.1f) * 0.75f;
                d.velocity *= 0.35f;

            }

            float timeForPopInAnim = 23; //33
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.75f)) * 1f;

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            
            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("Playground/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();



            Color StripColor(float progress) => Color.White;

            float StripWidth(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.85f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.85f, progress, true);
                    toReturn = Easings.easeInOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.85f, 1f, progress, true);
                    toReturn = Easings.easeOutSine(1f - LV);
                }

                return toReturn * overallScale * 9; //20 | 8
            }


            VertexStripFixed vertexStrip = new VertexStripFixed();
            VertexStripFixed vertexStrip2 = new VertexStripFixed();

            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);
            myEffect.Parameters["posterizationSteps"].SetValue(0.0f);

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }

    }


}
