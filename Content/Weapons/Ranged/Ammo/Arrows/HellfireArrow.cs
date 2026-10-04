using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class HellfireArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.HellfireArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = 0;

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 13 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);

            int EU = 1 + projectile.extraUpdates;

            //Want less dust when the arrow has extra updates (magic quiver)
            int mod = Math.Clamp(2 * EU, 2, 100);


            //Fire Particles
            if (timer % 1 == 0)// && Main.rand.NextBool(1))
            {

                //Vector2 dustVel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) - projectile.velocity * 0.2f; //0.5

                //Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f);


                Color fireRed = Color.Lerp(Color.OrangeRed, Color.Orange, 0.05f);//0.15

                for (int i = 0; i < 3; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) + projectile.velocity * 0.2f; //0.5

                    float posOffset = -4f + (5f * i);

                    Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * posOffset;

                    //.35 1.08 96
                    //0.4 1.08 0.94 quick fade

                    //3f bloom 0.5 endAlpha

                    FireParticleAlpha fire = new FireParticleAlpha(dustPos + new Vector2(0f, 0f - (i * 0f)), dustVel, 0.4f, fireRed, colorMult: 1.5f, bloomAlpha: 2f, AlphaFade: 0.97f,
                        EndAlpha: 0.25f, BlackRemoveThreshold: 0.5f);
                    fire.bloomColor = fireRed with { A = 150 };
                    //fire.scaleFadePower = 1f;
                    fire.scaleFadePower = 1.02f; //.08
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }
            if (timer % 1 == 0 && false)// && Main.rand.NextBool(1))
            {

                Vector2 dustVel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) - projectile.velocity * 0.2f; //0.5

                Color fireRed = Color.Lerp(Color.OrangeRed, Color.Orange, 0.05f);//0.15

                for (int i = 0; i < 2; i++)
                {
                    float posOffset = 14f + (10f * i);

                    Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * posOffset;

                    //.35 1.08 96
                    //0.4 1.08 0.94 quick fade

                    FireParticleAlpha fire = new FireParticleAlpha(dustPos + new Vector2(0f, 0f - (i * 0f)), dustVel, 0.65f, fireRed, colorMult: 1.5f, bloomAlpha: 2f, AlphaFade: 0.96f, 
                        EndAlpha: 0.5f, BlackRemoveThreshold: 0.5f);
                    fire.bloomColor = fireRed with { A = 150 };
                    fire.scaleFadePower = 1f;
                    //fire.scaleFadePower = 1.08f;
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }

            float fadeInTime = Math.Clamp((timer + 3f * EU) / 15f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.06f), 0f, 1f);

            timer++;

            #region vanillaAI
            if (Main.rand.NextBool())
            {
                int num240 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch, 0f, 0f, 100, default(Color), 1.15f);
                Main.dust[num240].noGravity = true;
                Main.dust[num240].velocity -= projectile.velocity * 0.2f;
            }

            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 15f)
            {
                projectile.ai[0] = 15f;
                projectile.velocity.Y += 0.1f;
            }

            if (projectile.type != ProjectileID.NorthPoleSnowflake && projectile.type != ProjectileID.Nail)
            {
                projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            }

            if (projectile.velocity.Y < -16f)
            {
                projectile.velocity.Y = -16f;
            }
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            #endregion

            return false;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                    Color.White with { A = 100 } * 0.25f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, Color.OrangeRed with { A = 100 } * 0.25f * overallAlpha, projectile.rotation, orb.Size() / 2, 0.85f * new Vector2(0.35f, 0.65f) * overallScale, SE);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);
            //Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.Orange with { A = 0 } * 0.15f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);
            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color betweenOrange = Color.Lerp(Color.Orange, Color.OrangeRed, 0.45f) * overallAlpha; //65
            Color betweenOrange2 = Color.Lerp(Color.Orange, Color.OrangeRed, 0.85f) * overallAlpha;
            Color betweenOrange3 = Color.Lerp(Color.Orange, Color.OrangeRed, 0.4f) * overallAlpha;

            Color betweenOrange4 = Color.Lerp(Color.Orange, Color.OrangeRed, 0.1f) * overallAlpha;


            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.Orange, betweenOrange, 1f - progress) * progress * overallAlpha;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;
                float size2 = (0.5f + (0.5f * progress)) * projectile.scale;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.35f,
                    previousRotations[i], TexOrigin, size2 * overallScale, SpriteEffects.None);

                if (i < previousPostions.Count - 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size3 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(3f, 1f * size3) * overallScale * projectile.scale * 0.35f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, betweenOrange4 with { A = 160 } * 0.5f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            Color fireRed = Color.Lerp(Color.OrangeRed, Color.Orange, 0.05f);//0.15

            for (int i = 0; i < 8; i++)
            {
                float prog = (float)i / 8;

                Vector2 veloF = Main.rand.NextVector2CircularEdge(6f, 6f) * Easings.easeOutSine(prog) * 1f;

                float alphaFade = Main.rand.NextFloat(0.95f, 0.96f);
                float fireScale = Main.rand.NextFloat(1.75f, 2.25f) * 0.5f;

                FireParticleAlpha fire = new FireParticleAlpha(projectile.Center, veloF, fireScale, fireRed, colorMult: 1.5f, bloomAlpha: 3f, AlphaFade: alphaFade, VelFade: 0.9f);
                fire.scaleFadePower = 1.05f; //1.05
                ShaderParticleHandler.SpawnParticle(fire);
            }

            #region Explosion
            for (int i = 0; i < 2 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 4f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 220; i < 12; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f);

                float progress = (float)i / 11;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.75f, 2.5f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.7f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            //Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.2f);

            //softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
            //    overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 220; i < 5 + Main.rand.Next(0, 3); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 13, postSlowPower: 0.92f,
                    velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            #endregion

            int soundVar = Main.rand.Next(1, 4);
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Impact/BoltCrash" + soundVar) with { Volume = 1f, Pitch = -0.15f, PitchVariance = 0.25f, };
            SoundEngine.PlaySound(style, projectile.Center);

            //SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Fire/small fireball projectile death") with { Volume = 0.5f, Pitch = -0.15f, PitchVariance = .3f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style2, projectile.Center);

            SoundStyle styleA = new SoundStyle("VFXPlus/Sounds/Effects/Fire/FlareImpact") with { Volume = 0.25f, Pitch = -.1f, PitchVariance = .3f, MaxInstances = -1 };
            //SoundEngine.PlaySound(styleA, projectile.Center);

            //SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.65f, Pitch = 0f, PitchVariance = 0.15f, MaxInstances = -1 }, projectile.Center);

            //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);


            #region vanillaKill
            for (int num731 = 2220; num731 < 10; num731++)
            {
                int num732 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
            }
            for (int num733 = 2220; num733 < 5; num733++)
            {
                int num734 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[num734].noGravity = true;
                Dust dust144 = Main.dust[num734];
                Dust dust334 = dust144;
                dust334.velocity *= 3f;
                num734 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
                dust144 = Main.dust[num734];
                dust334 = dust144;
                dust334.velocity *= 2f;
            }
            //int num735 = Gore.NewGore(null, projectile.position, default(Vector2), Main.rand.Next(61, 64));
            //Gore gore36 = Main.gore[num735];
            //Gore gore64 = gore36;
            //gore64.velocity *= 0.4f;
            //Main.gore[num735].velocity.X += (float)Main.rand.Next(-10, 11) * 0.1f;
            //Main.gore[num735].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.1f;
            //num735 = Gore.NewGore(null, projectile.position, default(Vector2), Main.rand.Next(61, 64));
            //gore36 = Main.gore[num735];
            //gore64 = gore36;
            //gore64.velocity *= 0.4f;
            //Main.gore[num735].velocity.X += (float)Main.rand.Next(-10, 11) * 0.1f;
            //Main.gore[num735].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.1f;
            if (projectile.owner == Main.myPlayer)
            {
                projectile.penetrate = -1;
                projectile.position.X += projectile.width / 2;
                projectile.position.Y += projectile.height / 2;
                projectile.width = 64;
                projectile.height = 64;
                projectile.position.X -= projectile.width / 2;
                projectile.position.Y -= projectile.height / 2;
                projectile.Damage();
            }
            #endregion

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }
}
