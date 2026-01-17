using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Particles;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
   
    public class InfernoForkBoltOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.InfernoFriendlyBolt) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.InfernoForkToggle;
        }


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14; //14
            
            if (timer % 2 == 0)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }


            if (timer % 1 == 0 && timer > 10 && Main.rand.NextBool(3) && false)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -6f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) - projectile.velocity * 1.25f;


                FireParticle fire = new FireParticle(dustPos + new Vector2(0f, 0f) + Main.rand.NextVector2Circular(5f, 5f), dustVel, 0.75f, Color.Lerp(Color.OrangeRed, Color.Red, 0.5f), colorMult: 1f, bloomAlpha: 2f, 
                    AlphaFade: 0.95f, RotPower: 0.01f);
                ShaderParticleHandler.SpawnParticle(fire);

                //FireParticle fire2 = new FireParticle(dustPos + new Vector2(0f, 0f), dustVel * 0.5f, 0.75f, Color.Lerp(Color.OrangeRed, Color.Red, 0f), colorMult: 1f, bloomAlpha: 2f, 
                //    AlphaFade: 0.94f, RotPower: 0.01f);
                //ShaderParticleHandler.SpawnParticle(fire2);
            }


            if (timer % 3 == 0 && timer > 3)
            {
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f);
                FireParticle fire = new FireParticle(projectile.Center, vel, 0.75f, Color.Lerp(Color.OrangeRed, Color.Red, 0.5f), colorMult: 3f, bloomAlpha: 1f, AlphaFade: 0.9f);
                fire.scaleFadePower = 1.08f;
                ShaderParticleHandler.SpawnParticle(fire);
            }

            float timeForPopInAnim = 40;
            float animProgress = Math.Clamp((timer + 15) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.1f + MathHelper.Lerp(0f, 0.9f, Easings.easeInOutBack(animProgress, 0f, 1.35f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);

            Color lightColor = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);
            Lighting.AddLight(projectile.position, lightColor.ToVector3() * 1.25f * overallScale);

            timer++;
            return false;
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawFireball(projectile, false);
            });

            DrawFireball(projectile, true);

            return false;
        }

        public void DrawFireball(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D FireBallPixel = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float rot = projectile.velocity.ToRotation();

            Vector2 totalScale = new Vector2(overallScale, 1f) * projectile.scale * 0.85f;

            Color betweenOrangeRed = Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f);

            Vector2 off = rot.ToRotationVector2() * -10f * totalScale;
            Main.EntitySpriteDraw(Glow, drawPos, null, Color.OrangeRed with { A = 0 } * overallAlpha * 0.5f, rot + MathHelper.PiOver2, Glow.Size() / 2f, totalScale, SpriteEffects.None);


            Color outerCol = Color.OrangeRed * 0.5f;
            for (int i = 0; i < 1; i++)
            {
                Main.EntitySpriteDraw(FireBall, drawPos + off, null, outerCol with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale, SpriteEffects.None);
            }

            #region after image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                Vector2 pos = previousPositions[i] - Main.screenPosition + off;

                float progress = (float)i / previousRotations.Count;

                Vector2 size = (1f - (progress * 0.5f)) * totalScale;

                float colVal = progress;

                Color col = Color.Lerp(Color.Red * 0.75f, betweenOrangeRed, progress) * progress * 0.7f;

                Vector2 size2 = (1f - (progress * 0.15f)) * totalScale;
                Main.EntitySpriteDraw(FireBallPixel, pos + Main.rand.NextVector2Circular(10f, 10f) * (1f - progress), null, col with { A = 0 } * 0.85f * overallAlpha * colVal,
                        previousRotations[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                Main.EntitySpriteDraw(FireBall, pos + Main.rand.NextVector2Circular(0f, 0f) * (1f - progress), null, col with { A = 0 } * 1.25f * overallAlpha * colVal,
                        previousRotations[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
            }
            #endregion

            Vector2 v2scale = new Vector2(1f, 1f);

            Main.EntitySpriteDraw(FireBall, drawPos + off + off + Main.rand.NextVector2Circular(2f, 2f), null, betweenOrangeRed with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale * v2scale, SpriteEffects.None);

            Main.EntitySpriteDraw(FireBall, drawPos + off, null, Color.White with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, v2scale * totalScale * 0.5f, SpriteEffects.None);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < previousPositions.Count; i++)
            {
                Vector2 pos = previousPositions[i];
                Vector2 velRot = previousRotations[i].ToRotationVector2();

                if (i % 1 == 0 && i > 2)
                {
                    Color col = Color.OrangeRed;

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelFast>(), Alpha: 100, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f));

                    Vector2 dustVel = (velRot * Main.rand.NextFloat(1f, 4.1f) * -0.5f).RotateRandom(0.3f);
                    d.velocity = dustVel + Main.rand.NextVector2Circular(3f, 3f);
                }

            }

            for (int i = 0; i < previousPositions.Count; i++)
            {
                Vector2 pos = previousPositions[i];
                Vector2 velRot = previousRotations[i].ToRotationVector2();

                if (i % 3 == 0 && i > 7)
                {
                    //Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f);
                    Vector2 dustVel = (velRot * Main.rand.NextFloat(1f, 4.1f) * -0.5f).RotateRandom(0.3f);
                    //dustVel += Main.rand.NextVector2Circular(3f, 3f);

                    FireParticle fire = new FireParticle(pos, dustVel * 3f, 0.75f, Color.Lerp(Color.OrangeRed, Color.Red, 0.5f), colorMult: 1f, bloomAlpha: 1f, AlphaFade: 0.88f);
                    fire.scaleFadePower = 1.08f;
                    ShaderParticleHandler.SpawnParticle(fire);
                }

            }

            return true;
        }
    }

    public class InfernoBlastOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.InfernoFriendlyBlast && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.InfernoForkToggle;
        }

        int vfxBlastIndex = -1;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                if (Main.myPlayer == projectile.owner)
                    vfxBlastIndex = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, ModContent.ProjectileType<InfernoForkVFX>(), 0, 0, projectile.owner);

                CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.6f, true, 1, 0.9f, 0.9f);

                Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.35f);
                d1.scale = 0.07f;
                d1.customData = cpb2;
                d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.35f);
                d2.customData = cpb2;
                d2.scale = 0.05f;
                d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

                //Fire particles

                for (int i = 0; i < 10; i++)
                {
                    float prog = (float)i / 10;


                    Vector2 veloF = Main.rand.NextVector2CircularEdge(10f, 10f) * Easings.easeOutCirc(prog) * 1f; //12

                    float fireScale = Main.rand.NextFloat(1.75f, 2.25f);

                    FireParticle fire = new FireParticle(projectile.Center, veloF, fireScale, Color.Lerp(Color.OrangeRed, Color.Red, 0.5f), colorMult: 1.5f, bloomAlpha: 1.5f, AlphaFade: 0.95f, VelFade: 0.86f);
                    fire.scaleFadePower = 1.01f; //1.05
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }

            #region vanillaAI
            if (projectile.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(in SoundID.Item74, projectile.position);
                projectile.localAI[0] += 1f;
            }
            projectile.ai[0] += 1f;
            if (projectile.type == 296)
            {
                projectile.ai[0] += 3f;
            }
            float num396 = 25f;
            if (projectile.ai[0] > 540f)
            {
                num396 -= (projectile.ai[0] - 180f) / 2f;
            }
            if (num396 <= 0f)
            {
                num396 = 0f;
                projectile.Kill();
            }

            #endregion
            timer++;
            return false;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor) => false;

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (vfxBlastIndex != -1)
                (Main.projectile[vfxBlastIndex].ModProjectile as InfernoForkVFX).shouldFadeOut = true;


            return base.PreKill(projectile, timeLeft);
        }
    }

    public class InfernoForkVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 2900;
        }

        public override bool? CanDamage() => false;

        public bool shouldFadeOut = false;

        int timer = 0;

        public override void AI()
        {
            //Grow to scale
            if (!shouldFadeOut)
            {
                overallScale = Math.Clamp(MathHelper.Lerp(overallScale, 1.5f, 0.04f), 0f, 1f);
            }
            //FadeOut
            else
            {
                overallScale = Math.Clamp(MathHelper.Lerp(overallScale, -0.35f, 0.04f), 0f, 1f);

                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.09f), 0f, 1f);

                if (overallAlpha == 0f)
                    Projectile.active = false;
            }


            if (timer % 1 == 0 && !shouldFadeOut)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(5f, 8f);
                FireParticle fire = new FireParticle(Projectile.Center + vel * 1.5f, vel, 1.25f, Color.Lerp(Color.OrangeRed, Color.Red, 0.5f), colorMult: 2f, bloomAlpha: 0.75f, AlphaFade: 0.9f);
                fire.scaleFadePower = 1.08f;
                ShaderParticleHandler.SpawnParticle(fire);
            }

            if (timer > 5 && timer % 2 == 0 && !shouldFadeOut)
            {
                int count = overallScale < 1f ? 8 : 4;
                for (int i = 0; i < count; i++)
                {
                    float rot = Main.rand.NextFloat(6.28f);

                    Vector2 pos = Projectile.Center;// new Vector2(0f, -1f) * Main.rand.NextFloat(0, 160);


                    if (Main.rand.NextBool())
                    {
                        Vector2 offset = rot.ToRotationVector2() * Main.rand.NextFloat(-10, 10);
                        Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.75f) * Main.rand.NextFloat(2f, 8.25f);

                        Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.45f);
                        d.alpha = 10;
                    }

                    //Color col = Color.Lerp(Color.Orange, Color.OrangeRed, Main.rand.NextFloat(0f, 1f));

                    if (i % 2 == 0 && Main.rand.NextBool())
                    {
                        Vector2 offset2 = rot.ToRotationVector2() * Main.rand.NextFloat(-10, 10);
                        Vector2 vel2 = rot.ToRotationVector2().RotatedByRandom(0.45f) * Main.rand.NextFloat(2f, 6f);

                        Dust d2 = Dust.NewDustPerfect(pos + offset2, ModContent.DustType<GlowPixelCross>(), vel2, newColor: Color.OrangeRed * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.35f);
                        d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 3, postSlowPower: 0.92f, velToBeginShrink: 1.5f, fadePower: 0.93f, shouldFadeColor: false);
                    }
                }

            }

            Color lightColor = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);
            Lighting.AddLight(Projectile.position, lightColor.ToVector3() * 1.4f * overallScale);

            timer++;
        }

        float overallScale = 0f;
        float overallAlpha = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D ball2 = CommonTextures.feather_circle128PMA.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            float drawScale = Projectile.scale * Easings.easeOutCirc(overallScale);
            float ball2Scale = drawScale * 2.9f;

            //Black layer
            Main.spriteBatch.Draw(ball2, drawPos, null, Color.Black * 0.5f * overallAlpha, 0f, ball2.Size() / 2, 0.45f * ball2Scale, 0f, 0f);
            Main.spriteBatch.Draw(ball2, drawPos, null, Color.Black * 0.75f * overallAlpha, 0f, ball2.Size() / 2, 0.55f * ball2Scale, 0f, 0f);

            //Large orange glow
            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.055f) * 0.07f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.07f;
            float sineColor = (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.1f;
            Color betweenORED = Color.Lerp(Color.Orange, Color.OrangeRed, 0.9f + sineColor);
            Main.spriteBatch.Draw(ball2, drawPos, null, betweenORED with { A = 0 } * 0.2f * overallAlpha, Projectile.rotation, ball2.Size() / 2, 1f * ball2Scale * sineScale2, SpriteEffects.None, 0f);


            //Orange Core
            Color between2 = Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);
            Main.spriteBatch.Draw(ball2, drawPos, null, between2 with { A = 0 } * 0.9f * overallAlpha, 0f, ball2.Size() / 2, 0.3f * ball2Scale * sineScale1, 0f, 0f);

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawInferno(true);
                DrawInfernoNew(false);
            });
            DrawInfernoNew(true);
            DrawInferno(true);

            return false;
        }

        public void DrawInferno(bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;
            Texture2D ball2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float drawScale = Projectile.scale * Easings.easeOutCirc(overallScale);
            float ball2Scale = drawScale * 3f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.055f) * 0.07f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.07f;
            float sineScale3 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.2f + timer * 0.05f) * 0.03f;
            float sineColor = (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.2f;

            //Shader info
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Noise_1").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/FireGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value);
            myEffect.Parameters["uTime"].SetValue(timer * -0.015f);
            myEffect.Parameters["flowSpeed"].SetValue(-1.5f); //-0.75
            myEffect.Parameters["distortStrength"].SetValue(0.1f);
            myEffect.Parameters["colorIntensity"].SetValue(1.5f * overallAlpha);
            myEffect.Parameters["vignetteSize"].SetValue(0.1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.32f);


            //Large orange glow
            Color betweenORED = Color.Lerp(Color.Orange, Color.OrangeRed, 0.6f + sineColor);
            Main.spriteBatch.Draw(ball2, drawPos, null, betweenORED with { A = 0 } * 0.15f * overallAlpha, Projectile.rotation, ball2.Size() / 2, 1f * ball2Scale * sineScale2, SpriteEffects.None, 0f);

            //Main shader
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            float rot1 = timer * 0.01f;
            Main.spriteBatch.Draw(ball, drawPos, null, Color.Orange with { A = 0 }, rot1, ball.Size() / 2, drawScale * 0.5f * sineScale3, 0f, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Orange Core
            Main.spriteBatch.Draw(ball2, drawPos, null, Color.Orange with { A = 0 } * 0.9f * overallAlpha, 0f, ball2.Size() / 2, 0.3f * ball2Scale * sineScale1, 0f, 0f);

        }


        public void DrawInfernoNew(bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D ball = Mod.Assets.Request<Texture2D>("Assets/InfernoOrb").Value;
            Texture2D ball2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, 0f);

            float drawScale = Projectile.scale * Easings.easeOutCirc(overallScale);
            float ball2Scale = drawScale * 5f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.055f) * 0.07f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.07f;
            float sineScale3 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.2f + timer * 0.05f) * 0.03f;
            float sineColor = (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.2f;

            //Shader info
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Noise_1").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/FireGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value);
            myEffect.Parameters["uTime"].SetValue(timer * -0.01f);
            myEffect.Parameters["flowSpeed"].SetValue(-1.5f); //-0.75
            myEffect.Parameters["distortStrength"].SetValue(0.1f);
            myEffect.Parameters["colorIntensity"].SetValue(1f * overallAlpha);
            myEffect.Parameters["vignetteSize"].SetValue(0.1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.22f);

            //Black layer | This literally shouldn't draw but if I remove it the orange balls just disappear????????????
            //Main.spriteBatch.Draw(ball2, drawPos, null, Color.Black * 0.5f * overallAlpha, 0f, ball2.Size() / 2, 0.45f * ball2Scale, 0f, 0f);
            //Main.spriteBatch.Draw(ball2, drawPos, null, Color.Black * 0.75f * overallAlpha, 0f, ball2.Size() / 2, 0.55f * ball2Scale, 0f, 0f);

            //Large orange glow
            Color betweenORED = Color.Lerp(Color.Orange, Color.OrangeRed, 0.6f + sineColor);
            //Main.spriteBatch.Draw(ball2, drawPos, null, betweenORED with { A = 0 } * 0.2f * overallAlpha, Projectile.rotation, ball2.Size() / 2, 1f * ball2Scale * sineScale2, SpriteEffects.None, 0f);

            //Main shader
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot1 = timer * 0.01f;
            Main.spriteBatch.Draw(ball, drawPos, null, Color.Orange with { A = 0 }, rot1, ball.Size() / 2, drawScale * 0.6f * sineScale3, 0f, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Orange Core
            //Main.spriteBatch.Draw(ball2, drawPos, null, Color.Orange with { A = 0 } * 0.9f * overallAlpha, 0f, ball2.Size() / 2, 0.3f * ball2Scale * sineScale1, 0f, 0f);

        }

    }

    public class InfernoForkVFXImOld : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;
        }

        public override bool? CanDamage() => false;

        public bool shouldFadeOut = false;

        int timer = 0;

        public override void AI()
        {
            //Grow to scale
            if (!shouldFadeOut)
            {
                overallScale = Math.Clamp(MathHelper.Lerp(overallScale, 1.5f, 0.05f), 0f, 1f);
            }
            //FadeOut
            else
            {
                overallScale = Math.Clamp(MathHelper.Lerp(overallScale, -0.35f, 0.04f), 0f, 1f);

                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.09f), 0f, 1f);

                if (overallAlpha == 0f)
                    Projectile.active = false;
            }


            if (timer > 5 && timer % 2 == 0 && !shouldFadeOut)
            {
                int count = overallScale < 1f ? 8 : 4;
                for (int i = 0; i < count; i++)
                {
                    float rot = Main.rand.NextFloat(6.28f);

                    Vector2 pos = Projectile.Center;// new Vector2(0f, -1f) * Main.rand.NextFloat(0, 160);


                    if (Main.rand.NextBool())
                    {
                        Vector2 offset = rot.ToRotationVector2() * Main.rand.NextFloat(-10, 10);
                        Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.75f) * Main.rand.NextFloat(2f, 8.25f);

                        Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.45f);
                        d.alpha = 10;
                    }

                    //Color col = Color.Lerp(Color.Orange, Color.OrangeRed, Main.rand.NextFloat(0f, 1f));

                    if (i % 2 == 0 && Main.rand.NextBool())
                    {
                        Vector2 offset2 = rot.ToRotationVector2() * Main.rand.NextFloat(-10, 10);
                        Vector2 vel2 = rot.ToRotationVector2().RotatedByRandom(0.45f) * Main.rand.NextFloat(2f, 6f);

                        Dust d2 = Dust.NewDustPerfect(pos + offset2, ModContent.DustType<GlowPixelCross>(), vel2, newColor: Color.OrangeRed * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.35f);
                        d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 3, postSlowPower: 0.92f, velToBeginShrink: 1.5f, fadePower: 0.93f, shouldFadeColor: false);
                    }
                }

            }

            Color lightColor = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);
            Lighting.AddLight(Projectile.position, lightColor.ToVector3() * 1.4f * overallScale);

            timer++;
        }

        float overallScale = 0f;
        float overallAlpha = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawInferno(true);
            });

            DrawInferno(false);

            return false;
        }

        public void DrawInferno(bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;
            Texture2D ball2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float drawScale = Projectile.scale * Easings.easeOutCirc(overallScale);
            float ball2Scale = drawScale * 3f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.055f) * 0.07f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.07f;
            float sineScale3 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.2f + timer * 0.05f) * 0.03f;
            float sineColor = (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.2f;

            //Shader info
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Noise_1").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/FireGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value);
            myEffect.Parameters["uTime"].SetValue(timer * -0.01f);
            myEffect.Parameters["flowSpeed"].SetValue(-1.5f); //-0.75
            myEffect.Parameters["distortStrength"].SetValue(0.1f);
            myEffect.Parameters["colorIntensity"].SetValue(1.5f * overallAlpha);
            myEffect.Parameters["vignetteSize"].SetValue(0.1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.32f);

            //Black layer
            Main.spriteBatch.Draw(ball2, drawPos, null, Color.Black * 0.5f * overallAlpha, 0f, ball2.Size() / 2, 0.45f * ball2Scale, 0f, 0f);
            Main.spriteBatch.Draw(ball2, drawPos, null, Color.Black * 0.75f * overallAlpha, 0f, ball2.Size() / 2, 0.55f * ball2Scale, 0f, 0f);

            //Large orange glow
            Color betweenORED = Color.Lerp(Color.Orange, Color.OrangeRed, 0.6f + sineColor);
            //Main.spriteBatch.Draw(ball2, drawPos, null, betweenORED with { A = 0 } * 0.15f * overallAlpha, Projectile.rotation, ball2.Size() / 2, 1f * ball2Scale * sineScale2, SpriteEffects.None, 0f);

            //Main shader
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            float rot1 = timer * 0.01f;
            Main.spriteBatch.Draw(ball, drawPos, null, Color.Orange with { A = 0 }, rot1, ball.Size() / 2, drawScale * 0.5f * sineScale3, 0f, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Orange Core
            Main.spriteBatch.Draw(ball2, drawPos, null, Color.Orange with { A = 0 } * 0.9f * overallAlpha, 0f, ball2.Size() / 2, 0.3f * ball2Scale * sineScale1, 0f, 0f);

        }

    }

}
