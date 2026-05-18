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
using Terraria.GameContent.UI;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Bullets
{

    public class SilverBulletProjOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SilverBullet);
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
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value;
            trail1.trailPointLimit = 120 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(7 * totalAlpha); //15
            trail1.trailMaxLength = 120 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = Color.DarkGray with { A = 75 } * totalAlpha * 0.75f; //255 111 20
            trail1.timesToDraw = 2;
            trail1.fadeOut = true;

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

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.Silver, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.25f, 0.08f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);
            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

            Lighting.AddLight(projectile.Center, Color.Silver.ToVector3() * 0.5f);

            proj = projectile;
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

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            trail1.TrailDrawing(Main.spriteBatch, false);
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
                if (projectile.active == false)
                    totalAlpha = 0f;


                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
                Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;


                Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10f);
                drawPos += new Vector2(0f, 0f);

                float drawRot = projectile.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = projectile.scale * (5f / 6f);

                Color spikeCol = new Color(75, 75, 75);
                Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale) * 0.5f;

                Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, spikeCol with { A = 100 } * 0.75f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

                Color orbCol = new Color(140, 140, 140);
                Vector2 orbScale = new Vector2(1f, 0.25f * totalScale) * 0.7f * adjustedScale; //0.3
                Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, orbCol with { A = 100 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


                Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

                Vector2 drawScale2 = new Vector2(adjustedScale * 2f, adjustedScale * totalScale) * 0.5f;

                Color col = new Color(125, 125, 125);

                drawPos += new Vector2(0f, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, col with { A = 100 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 100 } * totalAlpha, drawRot, drawOrigin, drawScale2 * 0.5f, SpriteEffects.None, 0f);
            });

            return false;
        }

        //Need this for DrawAdditve to have projectile pos and stuff
        Projectile proj = null;
        public void DrawAdditive(SpriteBatch sb)
        {

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: Color.Silver, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                //new Color(255, 90, 10)

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

                    //new Color(255, 120, 40)
                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.Silver, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
                    d.alpha = 2;
                    d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.35f;
                    d.velocity *= 0.35f;
                }
            }

            Color starCol = Color.Silver * 0.5f;
            DustBehaviorUtil.StarDustDrawInfo sdic = new DustBehaviorUtil.StarDustDrawInfo(true, false, false, true, false, 1f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: starCol, Scale: 1f);
            d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.88f, shouldFadeColor: true, sdci: sdic);
            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
