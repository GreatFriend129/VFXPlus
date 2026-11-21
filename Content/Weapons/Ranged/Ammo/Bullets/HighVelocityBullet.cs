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

    public class HighVelocityBulletProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BulletHighVelocity);
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

                projectile.light = 0f;
            }

            //Trail1 Info Dump
            trail1.trailPointLimit = 150 + trailRandomLengthOffset; 
            trail1.trailMaxLength = 500 + trailRandomLengthOffset; //300

            trail1.shouldSmooth = false;

            Color trailCol = Color.Lerp(Color.Gold, Color.Orange, 0.55f);

            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -5f;

            if (timer > 5) 
                trail1.TrailLogic();


            if (timer > 0 && (timer + (int)randomTimeOffset) % 4 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Color dustCol = Color.Lerp(Color.Yellow, Color.DarkGoldenrod, 1f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.Yellow, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.55f);
                d.alpha = 2;
                d.velocity += projectile.velocity.RotatedByRandom(0.1f) * 0.75f;
                d.velocity *= 0.35f;

            }

            //Just more messy than anything tbh
            if (false && (timer + (int)randomTimeOffset) % 6 == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(-projectile.velocity.Length() * 3f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(10f, 18f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<WindLine>(), vel, newColor: Color.DarkGoldenrod, Scale: Main.rand.NextFloat(0.75f, 1f));

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.9f, TimeToStartShrink: 5, ShrinkYScalePower: 0.75f, XScale: 2f, YScale: Main.rand.NextFloat(0.75f, 1f), false, KillEarlyTime: 10);
                wlb.colorAlpha = 100;
                wlb.whiteCoreIntensity = 0.5f;
                dp.customData = wlb;
            }

            //Quickly fade in 
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.5f, 0.05f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);

            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2f)) * 1f;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.5f);

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

            DrawTrail(projectile, false);


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawBody(projectile, true);
            });
            DrawBody(projectile, false);

            return false;
        }

        public void DrawBody(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;


            Color darkest = Color.Lerp(Color.Gold, Color.Orange, 1f);
            Color middle = Color.Lerp(Color.Gold, Color.Orange, 0.75f);
            Color brightest = Color.Lerp(Color.Gold, Color.Orange, 0.55f);


            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
            Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -30f);
            drawPos += new Vector2(0f, 0f);

            float drawRot = projectile.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = projectile.scale * (5f / 6f);

            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale * 1f) * 0.5f;
            outSpikeScale.X *= 3f;

            Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, darkest with { A = 140 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            Vector2 orbScale = new Vector2(1f, 0.25f * totalScale * 1f) * 0.7f * adjustedScale; //0.3
            orbScale.X *= 3f;
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, middle with { A = 140 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


            Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

            Vector2 drawScale2 = new Vector2(adjustedScale * 2f, adjustedScale * totalScale * 1f) * 0.45f;
            drawScale2.X *= 3f;

            drawPos += new Vector2(0f, 0f);
            Main.spriteBatch.Draw(spike2, drawPos, null, brightest with { A = 90 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 90 } * totalAlpha * 0.75f, drawRot, drawOrigin, drawScale2 * 0.6f, SpriteEffects.None, 0f);
        }

        Effect trailEffect = null;
        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            //isBlue = true;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/BasicGlowTrail", AssetRequestMode.ImmediateLoad).Value;


            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine3").Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = trail1.trailPositions.ToArray();
            float[] rot_arr = trail1.trailRotations.ToArray();

            Color trailDark = Color.Lerp(Color.Gold, Color.Orange, 1f);
            Color trailLight = Color.Lerp(Color.Gold, Color.Orange, 0.55f);
            Color StripColor(float progress) => Color.Lerp(trailDark, trailLight, progress) with { A = 215 } * Easings.easeOutQuad(progress) * 0.4f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.18f;

                float toReturn = Easings.easeOutSine(progress);

                return 15f * totalScale;// * toReturn * totalScale; 
            }

            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.05f);

            trailEffect.Parameters["bodyIntensity"].SetValue(1.25f);
            trailEffect.Parameters["bodyPower"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(0f);

            trailEffect.Parameters["whiteGlowSize"].SetValue(0.5f);
            trailEffect.Parameters["whiteGlowPower"].SetValue(2f);
            trailEffect.Parameters["whiteGlowIntensity"].SetValue(0f);


            trailEffect.Parameters["TrailTexture1"].SetValue(trailTexture1);
            trailEffect.Parameters["tex1reps"].SetValue(3f);
            trailEffect.Parameters["tex1Intensity"].SetValue(1f); //2f
            trailEffect.Parameters["tex1YStretch"].SetValue(1.25f); //2f

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(2.5f);
            trailEffect.Parameters["tex2YStretch"].SetValue(0.75f); //2f

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color trailCol = Color.Gold;


            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: trailCol, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

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


                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), vel, newColor: trailCol, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
                    d.alpha = 2;
                    d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.35f;
                    d.velocity *= 0.35f;
                }
            }


            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 220; i < 2 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(5f, 15f);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineBasic>(), vel * -0.5f, newColor: Color.DarkGoldenrod, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.6f);
                dp.alpha = 10 + Main.rand.Next(-5, 5);

            }

            int dustCount = 5 + Main.rand.Next(0, 3);
            for (int i = 2220; i < 5 + Main.rand.Next(0, 3); i++)
            {
                float prog = (float)(i + 1f) / dustCount;

                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1f) * (3f + 6f * prog);

                //float rotDir = (Main.rand.NextBool() ? MathHelper.PiOver2 : -MathHelper.PiOver2) + Main.rand.NextFloat(-0.2f, 0.2f);
                //Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(rotDir) * Main.rand.NextFloat(10f, 15);


                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<WindLine>(), -vel, newColor: Color.DarkGoldenrod, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1.25f);

                p.customData = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 11, ShrinkYScalePower: 0.5f, XScale: 1f, YScale: 0.5f, Pixelize: true);
            }

            for (int i = 0; i < 5 + Main.rand.Next(0, 3); i++)
            {
                float prog = (float)(i + 1f) / dustCount;

                float rotAmount = ((i % 2 == 0 ? 0.35f : -0.35f) * prog) + Main.rand.NextFloat(-0.1f, 0.1f);

                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(rotAmount) * (7f + (13f * prog));

                //float rotDir = (Main.rand.NextBool() ? MathHelper.PiOver2 : -MathHelper.PiOver2) + Main.rand.NextFloat(-0.2f, 0.2f);
                //Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(rotDir) * Main.rand.NextFloat(10f, 15);


                Dust p = Dust.NewDustPerfect(projectile.Center + vel, ModContent.DustType<WindLine>(), -vel, newColor: Color.DarkGoldenrod, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 2f);
                //I like this x-scale but it starts behind the center if we don't do this
                p.position += p.velocity;

                float velFadePower = Main.rand.NextFloat(0.9f, 0.93f);
                int shrinkTime = Main.rand.Next(4, 8);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: velFadePower, TimeToStartShrink: shrinkTime, ShrinkYScalePower: 0.5f, XScale: 1f, YScale: 0.5f, Pixelize: false);
                wlb.colorAlpha = 100;
                wlb.whiteCoreIntensity = 0.5f;

                p.customData = wlb;
            }

            for (int i = 220; i < 4 + Main.rand.Next(0, 3); i++)
            {
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1f) * Main.rand.NextFloat(10f, 30f);
                
                //float rotDir = (Main.rand.NextBool() ? MathHelper.PiOver2 : -MathHelper.PiOver2) + Main.rand.NextFloat(-0.2f, 0.2f);
                //Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(rotDir) * Main.rand.NextFloat(10f, 15);


                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<WindLine>(), -vel, newColor: Color.DarkGoldenrod, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1.5f);


                p.customData = new WindLineBehavior(VelFadePower: 0.92f, TimeToStartShrink: 11, ShrinkYScalePower: 0.5f, XScale: 1f, YScale: 0.5f, Pixelize: true);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
