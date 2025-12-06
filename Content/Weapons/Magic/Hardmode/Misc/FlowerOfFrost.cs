using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Misc
{
    public class FlowerOfFrostShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BallofFrost) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.FlowerOfFrostToggle;
        }

        Vector2 Vec2Scale
        {
            get => new(X_scale, Y_scale);
            set
            {
                X_scale = value.X;
                Y_scale = value.Y;
            }
        }


        int scaleState = 0;
        float X_scale = 1f;
        float Y_scale = 1f;

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 15;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 2 == 0)
            {
                int num4 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.IceTorch, projectile.velocity.X * -0.4f, projectile.velocity.Y * -0.4f, 100, default(Color), 1.2f);
                Main.dust[num4].noGravity = true;
                Main.dust[num4].velocity.X *= 4f;
                Main.dust[num4].velocity.Y *= 4f;
                Main.dust[num4].velocity = (Main.dust[num4].velocity + projectile.velocity) / 2f;
            }

            if (timer % 2 == 0 && timer > 10 && false)
            {
                Vector2 dustPos = projectile.Center;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(0.75f, 0.75f) - projectile.velocity * 0.25f;


                Color dustCol = Color.SkyBlue;
                float dustScale = Main.rand.NextFloat(0.4f, 0.7f) * 0.9f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 0.6f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if (timer % 2 == 0 && Main.rand.NextBool(2))
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -6f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) - projectile.velocity * 0.35f;


                FireParticle fire = new FireParticle(dustPos + new Vector2(0f, 0f) + Main.rand.NextVector2Circular(2f, 2f), dustVel, 0.45f, Color.Lerp(Color.LightSkyBlue, Color.DeepSkyBlue, 0.5f), colorMult: 0.5f, bloomAlpha: 1f,
                    AlphaFade: 0.93f, RotPower: 0.01f);
                fire.renderLayer = RenderLayer.UnderProjectiles;

                ShaderParticleHandler.SpawnParticle(fire);
            }

            //Based off of coralite mod slime emperor 
            float firstStretchX = 1.6f;
            float firstStretchY = 0.65f;

            float secondStretchX = 0.75f;
            float secondStretchY = 1.3f;

            switch (scaleState)
            {
                default:
                case 0:
                    projectile.rotation += 0.3f * (float)projectile.direction;
                    break;
                case 1:

                    Vec2Scale = Vector2.Lerp(Vec2Scale, new Vector2(firstStretchX * bounceIntensity, firstStretchY), 0.3f * bounceIntensity); //1,6 0.65 0.3f
                    if (Vec2Scale.X >= 1.55f * bounceIntensity)
                        scaleState = 2;
                    break;
                case 2:
                    Vec2Scale = Vector2.Lerp(Vec2Scale, new Vector2(secondStretchX, secondStretchY * bounceIntensity), 0.3f * bounceIntensity); //0.75 1.3 0.3
                    if (Vec2Scale.Y >= 1.25f * bounceIntensity)
                        scaleState = 3;
                    break;
                case 3:
                    Vec2Scale = Vector2.Lerp(Vec2Scale, Vector2.One, 0.2f);
                    if (Math.Abs(Vec2Scale.X - 1) < 0.05f)
                    {
                        Vec2Scale = Vector2.One;
                        scaleState = 0;
                    }
                    break;
            }


            justBouncedPower = Math.Clamp(MathHelper.Lerp(justBouncedPower, -0.35f, 0.04f), 0, 1f);
            true_alpha = Math.Clamp(MathHelper.Lerp(true_alpha, 1.35f, 0.07f), 0f, 1f);

            timer++;


            #region vanillaAI

            if (projectile.type == 27)
            {
                for (int num1054 = 0; num1054 < 5; num1054++)
                {
                    float num1065 = projectile.velocity.X / 3f * (float)num1054;
                    float num3 = projectile.velocity.Y / 3f * (float)num1054;
                    int num14 = 4;
                    int num25 = Dust.NewDust(new Vector2(projectile.position.X + (float)num14, projectile.position.Y + (float)num14), projectile.width - num14 * 2, projectile.height - num14 * 2, DustID.DungeonWater, 0f, 0f, 100, default(Color), 1.2f);
                    Main.dust[num25].noGravity = true;
                    Dust dust51 = Main.dust[num25];
                    Dust dust212 = dust51;
                    dust212.velocity *= 0.1f;
                    dust51 = Main.dust[num25];
                    dust212 = dust51;
                    dust212.velocity += projectile.velocity * 0.1f;
                    Main.dust[num25].position.X -= num1065;
                    Main.dust[num25].position.Y -= num3;
                }
                if (Main.rand.Next(5) == 0)
                {
                    int num36 = 4;
                    int num47 = Dust.NewDust(new Vector2(projectile.position.X + (float)num36, projectile.position.Y + (float)num36), projectile.width - num36 * 2, projectile.height - num36 * 2, 172, 0f, 0f, 100, default(Color), 0.6f);
                    Dust dust50 = Main.dust[num47];
                    Dust dust212 = dust50;
                    dust212.velocity *= 0.25f;
                    dust50 = Main.dust[num47];
                    dust212 = dust50;
                    dust212.velocity += projectile.velocity * 0.5f;
                }
            }
            else if (projectile.type == 502)
            {
                float num58 = (float)Main.DiscoR / 255f;
                float num69 = (float)Main.DiscoG / 255f;
                float num80 = (float)Main.DiscoB / 255f;
                num58 = (0.5f + num58) / 2f;
                num69 = (0.5f + num69) / 2f;
                num80 = (0.5f + num80) / 2f;
                Lighting.AddLight(projectile.Center, num58, num69, num80);
            }
            else if (projectile.type == 95 || projectile.type == 96)
            {
                int num84 = Dust.NewDust(new Vector2(projectile.position.X + projectile.velocity.X, projectile.position.Y + projectile.velocity.Y), projectile.width, projectile.height, 75, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 3f * projectile.scale);
                Main.dust[num84].noGravity = true;
            }
            else if (projectile.type == 253)
            {
                for (int num85 = 0; num85 < 2; num85++)
                {
                    //int num87 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 135, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 2f);
                    //Main.dust[num87].noGravity = true;
                    //Main.dust[num87].velocity.X *= 0.3f;
                    //Main.dust[num87].velocity.Y *= 0.3f;
                }
            }
            else
            {
                for (int num88 = 221; num88 < 2; num88++)
                {
                    int num89 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 2f);
                    if (projectile.type == 258 && Main.getGoodWorld)
                    {
                        Main.dust[num89].noLight = true;
                    }
                    Main.dust[num89].noGravity = true;
                    Main.dust[num89].velocity.X *= 0.3f;
                    Main.dust[num89].velocity.Y *= 0.3f;
                }
            }
            if (projectile.type != 27 && projectile.type != 96 && projectile.type != 258)
            {
                projectile.ai[1] += 1f;
            }
            if (projectile.ai[1] >= 20f)
            {
                projectile.velocity.Y += 0.2f;
            }
            if (projectile.type == 502)
            {
                projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;
                if (projectile.velocity.X != 0f)
                {
                    projectile.spriteDirection = (projectile.direction = Math.Sign(projectile.velocity.X));
                }
            }
            else
            {
                //projectile.rotation += 0.3f * (float)projectile.direction;
            }
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            #endregion

            projectile.spriteDirection = (projectile.direction = Math.Sign(projectile.velocity.X));


            return false;
        }


        float true_scale = 1f;
        float true_alpha = 0f;
        float bounceIntensity = 1f;
        float justBouncedPower = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 finalDrawScale = Vec2Scale * projectile.scale;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawPixelTrail(projectile);
            });


            //Border
            for (int i = 0; i < 6; i++)
            {
                float dist = 1.5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.15f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * true_alpha * 0.7f, projectile.rotation, TexOrigin, finalDrawScale * 1.05f, se);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 } * 0.15f * true_alpha, projectile.rotation, TexOrigin, finalDrawScale, se);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * 0.8f * true_alpha, projectile.rotation, TexOrigin, finalDrawScale, se);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.LightSkyBlue with { A = 0 } * 0.26f * true_alpha, projectile.rotation, TexOrigin, finalDrawScale, se);

            return false;
        }

        public void DrawPixelTrail(Projectile projectile)
        {
            Texture2D AfterImage = CommonTextures.Flare.Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 finalDrawScale = Vec2Scale * projectile.scale;


            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, Easings.easeInCirc(progress));

                float size2 = Easings.easeInSine(progress) * projectile.scale * 1f;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f * progress, 3f);

                //Vector2 newVec2 = new Vector2(1f, 0.5f) * size2;
                //Vector2 newVec22 = new Vector2(1f, 0.13f) * size2;

                float XScale = Math.Clamp(1f * size2, 0.55f, 2f);
                if (i == previousRotations.Count - 1)
                    XScale = 0.41f;

                Vector2 newVec2 = new Vector2(XScale, 0.42f * size2);
                Vector2 newVec22 = new Vector2(XScale, 0.11f * size2);

                Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.Black * 0.7f * progress,
                    previousRotations[i], AfterImage.Size() / 2f, newVec2 * 0.6f, SpriteEffects.None);

                Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, col with { A = 0 } * 1f,
                       previousRotations[i], AfterImage.Size() / 2f, newVec2 * 1f, SpriteEffects.None);

                Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.White with { A = 0 } * 0.65f * progress,
                       previousRotations[i], AfterImage.Size() / 2f, newVec22 * 1f, SpriteEffects.None);

            }

            //Glorb
            #region drawGlorb
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Color orbCol3 = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 1f) * 0.375f * true_alpha;
            Color orbCol2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.55f) * 0.525f * true_alpha;

            float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.5f) * 0.2f;
            float sineScale2 = MathF.Cos((float)Main.timeForVisualEffects * 0.22f + 0.32578f) * 0.15f;

            float scale2 = 1.8f + sineScale2;
            float scale3 = 2.4f + sineScale + (0.5f + (1f * justBouncedPower));

            Main.EntitySpriteDraw(Glow, drawPos, null, Color.Black * 0.3f * true_alpha, projectile.velocity.ToRotation(), Glow.Size() / 2f, finalDrawScale * 0.3f * true_alpha, SpriteEffects.None);

            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol2 with { A = 0 } * 0.55f, projectile.velocity.ToRotation(), Glow.Size() / 2f, finalDrawScale * scale2 * 0.25f * true_alpha, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol3 with { A = 0 } * 0.55f, projectile.velocity.ToRotation(), Glow.Size() / 2f, finalDrawScale * scale3 * 0.15f * true_alpha, SpriteEffects.None);


            #endregion
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 4 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 spawnOffset = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center + spawnOffset, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.75f, 1f),
                    newColor: Color.SkyBlue, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);

                p.velocity -= projectile.velocity * 0.15f;
            }

            #region vanillaKill
            SoundEngine.PlaySound(in SoundID.Item10, projectile.position);
            int dustCount = 8; //Vanilla = 20
            for (int num699 = 0; num699 < dustCount; num699++)
            {
                int num700 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 135, (0f - projectile.velocity.X) * 0.2f, (0f - projectile.velocity.Y) * 0.2f, 100, default(Color), 2f);
                Main.dust[num700].noGravity = true;
                Dust dust156 = Main.dust[num700];
                Dust dust334 = dust156;
                dust334.velocity *= 2f;
                num700 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 135, (0f - projectile.velocity.X) * 0.2f, (0f - projectile.velocity.Y) * 0.2f, 100);
                dust156 = Main.dust[num700];
                dust334 = dust156;
                dust334.velocity *= 2f;
            }
            #endregion

            return false;
            return base.PreKill(projectile, timeLeft);
        }

        bool justTileCollided = false;
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            scaleState = 1;
            justBouncedPower = 1f;

            bool hitWall = false;

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = -0.15f, PitchVariance = 0.15f, MaxInstances = -1 }, projectile.Center);


            #region vanillaCode
            //SoundEngine.PlaySound(in SoundID.Item10, projectile.position);
            projectile.ai[0] += 1f;
            int num44 = 5;
            switch (projectile.type)
            {
                case 15:
                    num44 = 6;
                    break;
                case 253:
                    num44 = 8;
                    break;
            }
            if (projectile.ai[0] >= (float)num44)
            {
                projectile.position += projectile.velocity;
                projectile.Kill();
            }
            else
            {
                if (projectile.type == 15 && projectile.velocity.Y > 4f)
                {
                    if (projectile.velocity.Y != oldVelocity.Y)
                    {
                        projectile.velocity.Y = (0f - oldVelocity.Y) * 0.8f;
                    }
                }
                else if (projectile.velocity.Y != oldVelocity.Y)
                {
                    projectile.velocity.Y = 0f - oldVelocity.Y;
                }
                if (projectile.velocity.X != oldVelocity.X)
                {
                    hitWall = true;

                    projectile.velocity.X = 0f - oldVelocity.X;
                }
            }
            #endregion

            float getSpeed = hitWall ? Math.Abs(projectile.velocity.X) : Math.Abs(projectile.velocity.Y);
            float clampedSpeed = Math.Clamp(getSpeed, 0f, 15f);
            float adjustMent = MathHelper.Lerp(0.5f, 1.35f, clampedSpeed / 15f) * 1.15f;
            bounceIntensity = adjustMent;

            projectile.rotation = projectile.velocity.ToRotation();


            //Impact Dust
            float circlePulseSize = 0.1f;

            Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 2f, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.2f, newColor: Color.DeepSkyBlue);
            CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            b2.drawLayer = "Dusts";
            d2.customData = b2;
            d2.scale = circlePulseSize * 0.5f;

            for (int i = 0; i < 5 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 spawnOffset = Main.rand.NextVector2Circular(5f, 5f);

                Dust p = Dust.NewDustPerfect(projectile.Center + spawnOffset + oldVelocity, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.75f, 1.05f),
                    newColor: Color.SkyBlue, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);
                //p.velocity += oldVelocity * Main.rand.NextFloat(0.2f, 0.3f) * 0.5f;
            }


            return false;
        }

    }

    
}
