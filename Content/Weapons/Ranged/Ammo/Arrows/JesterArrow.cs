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
    public class JesterArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.JestersArrow);
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();
        int timer = 0;

        int dustRandomOffsetTime = 0;

        float trailLengthOffset = Main.rand.NextFloat(0.85f, 1.15f);
        float trailGradientOffset = Main.rand.NextFloat();
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 16; // 14
            
            if (timer % 2 == 0)
            {
                previousRotations.Add(projectile.rotation);
                previousPostions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPostions.Count > trailCount)
                    previousPostions.RemoveAt(0);
            }


            #region trailInfo
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value;
            trail1.trailPointLimit = 300;
            trail1.trailWidth = (int)(20 * overallScale);
            trail1.trailMaxLength = 300f * trailLengthOffset;
            trail1.timesToDraw = 1;
            trail1.shouldSmooth = false;
            trail1.pinchHead = false;
            
            trail1.gradient = true;
            trail1.gradientTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/BrightEosGrad").Value;
            trail1.shouldScrollColor = true;

            trail1.trailTime = (timer * 0.04f);
            trail1.gradientTime = trailGradientOffset;

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center;// + projectile.velocity;
            trail1.TrailLogic();
            #endregion

            if ((timer + dustRandomOffsetTime) % 3 == 0 && Main.rand.NextBool(1) && timer > 5)
            {
                Color dustCol = getEosColor(Main.rand.NextFloat());

                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowFlare>(), newColor: dustCol, Scale: Main.rand.NextFloat(0.35f, 0.4f) * 1.35f);
                Main.dust[d].velocity -= projectile.velocity * 0.5f;
                Main.dust[d].velocity *= 0.75f; //75
                Main.dust[d].customData = new GlowFlareBehavior(0.4f, 2.15f);
            }

            if ((timer + dustRandomOffsetTime) % 5 == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                Color dustCol = getEosColor(Main.rand.NextFloat());

                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowPixelCross>(), newColor: dustCol, Scale: Main.rand.NextFloat(0.35f, 0.4f) * 0.9f);
                Main.dust[d].velocity -= projectile.velocity * 0.5f;
                Main.dust[d].velocity *= 0.55f; //45

                Main.dust[d].customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, shouldFadeColor: false);
            }

            float fadeInTime = Math.Clamp((timer + 8f) / 45f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 2f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            TrailDrawing(projectile);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    //Start End
                    Color col = getEosColor(1f - progress);

                    float size = (0.2f + (0.8f * progress)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.5f, 
                        previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);
                }
            }

            //Flare
            Texture2D flare = CommonTextures.GlowingFlare.Value;
            float flareRot = projectile.rotation + MathHelper.PiOver2;
            Vector2 flarePos = drawPos + flareRot.ToRotationVector2() * 10f;

            Vector2 vec2Scale = new Vector2(0.55f, 0.35f) * projectile.scale * overallScale;
            float timeScale = ((float)Main.timeForVisualEffects * 0.02f) % 1;

            Color[] cols = { getEosColor(0f + timeScale), getEosColor(0.33f + timeScale), getEosColor(0.66f + timeScale) };
            float[] scales = { 1.15f, 1.6f, 2.5f };

            Main.EntitySpriteDraw(flare, flarePos, null, cols[0] with { A = 0 } * 1f, flareRot, flare.Size() / 2f, vec2Scale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(flare, flarePos, null, cols[1] with { A = 0 } * 0.75f, flareRot, flare.Size() / 2f, vec2Scale * scales[1], SpriteEffects.None);
            Main.EntitySpriteDraw(flare, flarePos, null, cols[2] with { A = 0 } * 0.525f, flareRot, flare.Size() / 2f, vec2Scale * scales[2], SpriteEffects.None);

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 } * 0.15f, projectile.rotation, TexOrigin, projectile.scale * overallScale * 1.1f, SpriteEffects.None);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * 0.25f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 100 }, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void TrailDrawing(Projectile projectile)
        {
            trail1.TrailDrawing(Main.spriteBatch);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 13; i++)
            {
                float prog = (float)i / 13f;

                //Vector2 vel = projectile.oldVelocity.RotatedByRandom(1.25f) * Main.rand.NextFloat(0.5f, 1.15f) * 0.7f;
                //Dust d = Dust.NewDustPerfect(projectile.Center + vel, ModContent.DustType<GlowPixelCross>(), vel, newColor: getEosColor(prog), Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.5f);
                //d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, shouldFadeColor: false);

                Vector2 randomStart = Main.rand.NextVector2Circular(5f, 5f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center + randomStart, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: getEosColor(prog), Scale: Main.rand.NextFloat(0.35f, 0.45f));
                dust.velocity += projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * 3.35f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
            }
            for (int i = 0; i < 9; i++)
            {
                float prog = (float)i / 9f;

                Vector2 vel = projectile.oldVelocity.RotatedByRandom(1.25f) * Main.rand.NextFloat(0.5f, 1.15f) * 0.5f;

                Dust d = Dust.NewDustPerfect(projectile.Center + vel, ModContent.DustType<GlowFlare>(), vel, newColor: getEosColor(prog), Scale: Main.rand.NextFloat(0.35f, 0.65f));
            }

            int count = trail1.trailPositions.Count;
            for (int i = (int)(count * 0.3f); i < count; i += 5)
            {
                if (Main.rand.NextBool())
                {
                    Vector2 pos = trail1.trailPositions[i];
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), vel, newColor: getEosColor(Main.rand.NextFloat()), Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
                    d.alpha = 2;
                    d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.35f;
                    d.velocity *= 0.35f;
                }
            }

            return false;// base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.Center, oldVelocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


        public Color getEosColor(float progress)
        {
            Color myCol = Color.White;

            Color purple = new Color(207, 130, 244); //214, 143, 247
            Color mediumBlue = new Color(97, 136, 186); //97
            Color brightBlue = new Color(110, 234, 215);


            if (progress < 0.33f)
            {
                myCol = Color.Lerp(purple, mediumBlue, progress * 3f);
            }
            else if (progress < 0.66f)
            {
                float fakeProgress = progress - 0.33f;
                myCol = Color.Lerp(mediumBlue, brightBlue, fakeProgress * 3f);

            }
            else
            {
                float fakeProgress = progress - 0.66f;
                myCol = Color.Lerp(brightBlue, mediumBlue, fakeProgress * 3f);

            }
            return myCol * 1f;
        }

    }

}
