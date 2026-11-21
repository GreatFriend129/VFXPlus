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
            trail1.trailWidth = (int)(10 * totalAlpha * Easings.easeOutCubic(1f - justTileCollidePower)); //20
            trail1.trailMaxLength = 120 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = col with { A = 50 } * totalAlpha * 1f;
            trail1.fadeOut = true;

            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity * 0.5f;
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


            DrawTrail(projectile, false);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawPixelated(projectile, true);
            });
            DrawPixelated(projectile, false);

            return false;
        }

        public void DrawPixelated(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
            if (projectile.active == false)
                totalAlpha = 0f;

            float easedJustHitPower = Easings.easeOutCirc(1f - justTileCollidePower);
        
            Color darkest = new Color(89, 31, 31);
            Color middle = Color.Brown;
            Color brightest = new Color(244, 40, 60);

            //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
            if (projectile.active == false)
                totalAlpha = 0f;

            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
            Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10f);
            drawPos += new Vector2(0f, 0f);

            float drawRot = projectile.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets (1.4 for meteor shot), so normalize this to 1f
            float adjustedScale = projectile.scale * (5f / 6f); //5/7

            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f * easedJustHitPower, adjustedScale * 1.5f * totalScale * 1f) * 0.5f;

            Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, darkest with { A = 90 } * 0.75f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            Vector2 orbScale = new Vector2(1f * easedJustHitPower, 0.25f * totalScale * 1f) * 0.7f * adjustedScale; //0.3
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, middle with { A = 90 } * 0.45f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


            Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

            Vector2 drawScale2 = new Vector2(adjustedScale * 2f * easedJustHitPower, adjustedScale * totalScale * 1f) * 0.45f;

            drawPos += new Vector2(0f, 0f);
            Main.spriteBatch.Draw(spike2, drawPos, null, brightest with { A = 90 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 90 } * totalAlpha, drawRot, drawOrigin, drawScale2 * 0.55f, SpriteEffects.None, 0f);
        }

        Effect trailEffect = null;
        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/BasicGlowTrail", AssetRequestMode.ImmediateLoad).Value;


            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine3").Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = trail1.trailPositions.ToArray();
            float[] rot_arr = trail1.trailRotations.ToArray();

            Color StripColor(float progress) => Color.Lerp(new Color(89, 31, 31), new Color(244, 40, 60), progress) with { A = 185 } * Easings.easeOutQuad(progress) * 0.5f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.18f;

                float toReturn = Easings.easeOutSine(progress);

                return 15;// * toReturn * totalScale; 
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
            trailEffect.Parameters["tex1Intensity"].SetValue(1.35f); //2f

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(4f);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


        //Need this for DrawAdditve to have projectile pos and stuff
        Projectile proj = null;
        public void DrawAdditive(SpriteBatch sb)
        {
            if (proj == null)
                return;
            //trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
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
