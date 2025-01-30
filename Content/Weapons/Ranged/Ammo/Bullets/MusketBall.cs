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


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Bullets
{

    public class MusketBallProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Bullet);
        }

        float randomTrailSpeed = 1f;
        float randomTimeOffset = 0;
        int trailRandomWidthOffset = 0;
        BaseTrailInfo trail1 = new BaseTrailInfo();
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //We want each bullet to feel a little different, so we randomize the length of the trail a little bit and offset the trail time
            //Without this, bullets can often feel very weird when fired at the same (shotguns)
            if (timer == 0)
            {
                randomTimeOffset = Main.rand.NextFloat(0f, 10f);
                trailRandomWidthOffset = Main.rand.Next(0, 35);
                randomTrailSpeed = Main.rand.NextFloat(0.85f, 1.15f);
            }

            //Main.NewText(projectile.velocity.Length());
            //if (timer > 0)
            //    Main.NewText(trail1.trailPositions.Count);

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value; //Laser1 |spark_07_Black?
            trail1.trailPointLimit = 120 + trailRandomWidthOffset;
            trail1.trailWidth = (int)(15 * totalAlpha);
            trail1.trailMaxLength = 120 + trailRandomWidthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = new Color(255, 111, 20) * totalAlpha;


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


            if (timer > 5 && timer % 3 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                
                //Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), vel, newColor: new Color(255, 90, 10), Scale: Main.rand.NextFloat(0.45f, 0.5f));
                //d.fadeIn = Main.rand.NextFloat(0.5f, 0.75f);
                //d.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.9f, velToBeginShrink: 4f, colorFadePower: 1f);
                //d.velocity += -projectile.velocity * 0.55f;
                //d.velocity *= 0.55f;

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: new Color(255, 120, 40), Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;

            }

            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.25f, 0.08f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);

            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

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

            DrawOne(projectile, false);

            trail1.TrailDrawing(Main.spriteBatch);

            return false;
        }

        public void DrawOne(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
            Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value; //SolidBloom


            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10);
            float drawRot = projectile.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = projectile.scale * (5f / 6f);
            Vector2 drawScale = new Vector2(adjustedScale * 2f, adjustedScale * totalScale) * 0.5f;


            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale) * 0.5f;
            Main.EntitySpriteDraw(spike, drawPos, null, Color.OrangeRed with { A = 0 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            //Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, new Color(255, 111, 20) with { A = 0 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, new Vector2(1f, 0.3f * totalScale) * 0.7f, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, new Color(255, 90, 10) with { A = 0 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, new Vector2(1f, 0.3f * totalScale) * 0.7f, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(spike, drawPos, null, Color.OrangeRed * 2f * totalAlpha, drawRot, drawOrigin, drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, drawPos, null, Color.White * totalAlpha, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }

        public void DrawTwo(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10);
            float drawRot = projectile.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = projectile.scale * (5f / 6f);
            Vector2 drawScale = new Vector2(adjustedScale * 2f, adjustedScale) * 0.5f;


            Main.EntitySpriteDraw(spike, drawPos, null, new Color(255, 111, 20) with { A = 0 } * 1f, drawRot, drawOrigin, drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(spike, drawPos, null, Color.White with { A = 0 }, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None);


            //After Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                Vector2 AIDrawpos = previousPositions[i] - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10);
                float AIDrawrot = previousRotations[i];

                float progress = (float)i / previousRotations.Count;

                float easedProg = Easings.easeInQuad(progress);

                Color col = Color.Lerp(new Color(255, 111, 20), Color.OrangeRed, progress);

                Vector2 vec2Scale1 = new Vector2(adjustedScale * 2f, adjustedScale * easedProg) * 0.5f;

                Main.EntitySpriteDraw(spike, AIDrawpos + Main.rand.NextVector2Circular(2f, 2f), null, col with { A = 0 } * 0.5f,
                    AIDrawrot, drawOrigin, vec2Scale1, SpriteEffects.None);

                Vector2 vec2Scale2 = new Vector2(adjustedScale * 2f, adjustedScale * easedProg * 0.5f) * 0.55f;

                Main.EntitySpriteDraw(spike, AIDrawpos + Main.rand.NextVector2Circular(3f, 3f), null, Color.White with { A = 0 } * 0.25f, 
                    AIDrawrot, drawOrigin, vec2Scale2, SpriteEffects.None);
            }
            //new Color(255, 111, 20);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: new Color(255, 90, 10), Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }


            for (int i = 20; i < 2; i++)
            {
                
                Vector2 vel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) * Main.rand.NextFloat(0.5f, 2.35f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: Color.OrangeRed * 1f, Scale: Main.rand.NextFloat(0.5f, 1.2f) * 1f);
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

                d.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * 4f;
            }

            return false;
            //return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
