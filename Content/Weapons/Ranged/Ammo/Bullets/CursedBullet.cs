using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Interfaces;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Bullets
{

    public class CursedBulletProjOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CursedBullet);
        }

        Color CursedGreen = new Color(162, 250, 47);

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
            trail1.trailPointLimit = 160 + trailRandomLengthOffset;
            trail1.trailMaxLength = 160 + trailRandomLengthOffset;

            trail1.shouldSmooth = false;

            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 5f;
            trail1.TrailLogic();


            if (timer > 5 && timer % 3 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelRise>(), vel, newColor: CursedGreen, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.45f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;

            }

            if (timer % 1 == 0 && Main.rand.NextBool() && false)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -6f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) - projectile.velocity * 0.35f; //0.5

                //FireParticle fire = new FireParticle(dustPos + projectile.velocity + Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -100f), dustVel, 0.5f, Color.Lerp(Color.Green, Color.GreenYellow, 0.3f), colorMult: 0.75f, bloomAlpha: 1f,


                FireParticle fire = new FireParticle(dustPos + projectile.velocity + Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, 0f), dustVel, 0.5f, Color.Lerp(Color.OrangeRed, Color.Red, 0.35f), colorMult: 1.5f, bloomAlpha: 2f,
                    AlphaFade: 0.91f, RotPower: 0.01f);
                fire.renderLayer = RenderLayer.UnderProjectiles;

                ShaderParticleHandler.SpawnParticle(fire);
            }

            if (timer % 1 == 0 && Main.rand.NextBool(1) && timer > 5 && false)
            {

                Color cursedGreen = Color.Lerp(Color.Green, Color.GreenYellow, 0.25f);
                Color cursedGreen2 = Color.Lerp(Color.Green, Color.GreenYellow, 0.25f);


                for (int i = 0; i < 1; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) + projectile.velocity * 0.5f; //0.5

                    float posOffset = -30f + (10f * i);

                    Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * posOffset;

                    float alphaFade = Main.rand.NextFloat(0.95f, 0.96f);
                    float scaleFade = Main.rand.NextFloat(1.065f, 1.1f);

                    FireParticleAlpha fire = new FireParticleAlpha(dustPos + new Vector2(0f, 0f), dustVel, 0.6f, cursedGreen with { A = 0 }, colorMult: 1f, bloomAlpha: 0.5f, AlphaFade: alphaFade, //col * 0.5f
                        EndAlpha: 0.5f, BlackRemoveThreshold: 0.5f);
                    fire.bloomColor = cursedGreen with { A = 150 };
                    fire.scaleFadePower = scaleFade;
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }

            if (timer % 3 == 0 && Main.rand.NextBool(4))
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: new Color(87, 153, 0), Scale: Main.rand.NextFloat(0.15f, 0.35f));
                d.velocity += -projectile.velocity * 0.2f;

                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.1f, timeBeforeSlow: 3, preSlowPower: 0.98f, postSlowPower: 0.88f, velToBeginShrink: 3f, fadePower: 0.9f, shouldFadeColor: false);

                //int num4 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.CursedTorch, projectile.velocity.X * -0.4f, projectile.velocity.Y * -0.4f, 100, default(Color), 1.2f);
                //Main.dust[num4].noGravity = true;
                //Main.dust[num4].velocity.X *= 2f;
                //Main.dust[num4].velocity.Y *= 2f;
                //Main.dust[num4].velocity += projectile.velocity * 0.4f;
            }

            if (timer % 1 == 0 && Main.rand.NextBool() && false)
            {
                Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, 0f);
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f);

                Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<PulseInOutDust>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: new Color(87, 153, 0) with { A = 0 }, Scale: Main.rand.NextFloat(0.3f, 0.4f) * projectile.scale * 1);
                p.velocity += projectile.velocity * 0.2f;
                p.rotation = MathHelper.PiOver4;
                p.noLight = false;

                p.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowPixel, 8, 0.15f, 0.5f, true);
            }

            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.25f, 0.08f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);

            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2f)) * 1f;

            Lighting.AddLight(projectile.Center, CursedGreen.ToVector3() * 0.55f);

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

            Color darkest = new Color(69, 130, 0);
            Color middle = new Color(87, 153, 0);
            Color brightest = new Color(87, 153, 0);

            //darkest = new Color(50, 112, 0);
            //middle = new Color(69, 130, 0);
            //brightest = new Color(87, 153, 0);

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

            Color darkest = new Color(69, 130, 0);
            Color middle = new Color(87, 153, 0);
            Color brightest = new Color(87, 153, 0);

            darkest = new Color(50, 112, 0);
            middle = new Color(69, 130, 0);
            brightest = new Color(87, 153, 0);

            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
            Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10f);
            drawPos += new Vector2(0f, 0f);

            float drawRot = projectile.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = projectile.scale * (5f / 6f);

            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale * 1f) * 0.5f;

            Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, darkest with { A = 140 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            Vector2 orbScale = new Vector2(1f, 0.25f * totalScale * 1f) * 0.7f * adjustedScale; //0.3
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, middle with { A = 140 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


            Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

            Vector2 drawScale2 = new Vector2(adjustedScale * 2f, adjustedScale * totalScale * 1f) * 0.45f;

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

            Color StripColor(float progress) => Color.Lerp(new Color(50, 112, 0), new Color(69, 130, 0), progress) with { A = 215 } * Easings.easeOutQuad(progress) * 0.5f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.18f;

                float toReturn = Easings.easeOutSine(progress);

                return 14f * totalScale;// * toReturn * totalScale; 
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
            trailEffect.Parameters["tex1Intensity"].SetValue(2f); //2f
            trailEffect.Parameters["tex1YStretch"].SetValue(1.25f); //2f

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(3f);
            trailEffect.Parameters["tex2YStretch"].SetValue(0.75f); //2f

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


        public void DrawAdditive(SpriteBatch sb)
        {

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color trailCol = CursedGreen;

            for (int i = 0; i < 2 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: trailCol, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }
            for (int i = 0; i < 2 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelRise>(), dustVel, newColor: trailCol, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);
                p.alpha = 2;
            }

            //Particles on trail
            int count = trail1.trailPositions.Count;
            for (int i = (int)(count * 0.3f); i < count; i += 5)
            {
                if (Main.rand.NextBool())
                {
                    
                    Vector2 pos = trail1.trailPositions[i];
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);


                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelRise>(), vel, newColor: trailCol, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
                    d.alpha = 2;
                    d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.35f;
                    d.velocity *= 0.35f;
                }
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
