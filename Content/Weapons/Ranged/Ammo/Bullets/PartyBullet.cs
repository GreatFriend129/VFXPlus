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

    public class PartyBulletProjOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PartyBullet);
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

                projectile.light = 0f;
            }

            Color col = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value;
            trail1.trailPointLimit = 135 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(12 * totalAlpha);
            trail1.trailMaxLength = 135 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = col * totalAlpha;


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

            Lighting.AddLight(projectile.Center, Color.HotPink.ToVector3() * 0.7f);

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

            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
            Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;


            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10);
            float drawRot = projectile.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = projectile.scale * (5f / 6f);

            Color spikeCol = Color.HotPink;
            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale) * 0.5f;
            Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, spikeCol with { A = 0 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            Color orbCol = Color.HotPink;
            Vector2 orbScale = new Vector2(1f, 0.3f * totalScale) * 0.7f * adjustedScale;
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, orbCol with { A = 0 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);

            return false;
        }

        //Need this for DrawAdditve to have projectile pos and stuff
        Projectile proj = null;
        public void DrawAdditive(SpriteBatch sb)
        {
            if (proj == null)
                return;

            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;

            Vector2 drawPos = proj.Center - Main.screenPosition + (proj.velocity.SafeNormalize(Vector2.UnitX) * -10);
            float drawRot = proj.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = proj.scale * (5f / 6f);
            Vector2 drawScale = new Vector2(adjustedScale * 2f, adjustedScale * totalScale) * 0.5f;

            Color col = Color.HotPink;

            sb.Draw(spike, drawPos, null, col * totalAlpha, drawRot, drawOrigin, drawScale, SpriteEffects.None, 0f);
            sb.Draw(spike, drawPos, null, Color.White * totalAlpha, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None, 0f);

            trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color col = Color.HotPink;


            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.5f) * 1.5f);

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


            #region vanillaKill
            for (int num666 = 0; num666 < 10; num666++)
            {
                int num667 = Main.rand.Next(139, 143);
                int num668 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num667, (0f - projectile.velocity.X) * 0.3f, (0f - projectile.velocity.Y) * 0.3f, 0, default(Color), 1.2f);
                Main.dust[num668].velocity.X += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.dust[num668].velocity.X += (float)Main.rand.Next(-50, 51) * 0.05f;
                Main.dust[num668].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.05f;
                Dust dust163 = Main.dust[num668];
                Dust dust334 = dust163;
                dust334.scale *= 1f + (float)Main.rand.Next(-30, 31) * 0.01f;
            }
            for (int num669 = 0; num669 < 5; num669++)
            {
                int num670 = Main.rand.Next(276, 283);
                int num671 = Gore.NewGore(null, projectile.position, -projectile.velocity * 0.3f, num670);
                Main.gore[num671].velocity.X += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.gore[num671].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.gore[num671].velocity.X *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Main.gore[num671].velocity.Y *= 1f + (float)Main.rand.Next(-50, 51) * 0.01f;
                Gore gore39 = Main.gore[num671];
                Gore gore64 = gore39;
                gore64.scale *= 1f + (float)Main.rand.Next(-20, 21) * 0.01f;
                Main.gore[num671].velocity.X += (float)Main.rand.Next(-50, 51) * 0.05f;
                Main.gore[num671].velocity.Y += (float)Main.rand.Next(-50, 51) * 0.05f;
            }
            #endregion

            for (int i = 0; i < 4 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4.25f, 4.25f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f) * 1.25f,
                    newColor: Color.DeepPink * 1f, Scale: Main.rand.NextFloat(0.25f, 0.45f));

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, postSlowPower: 0.91f, shouldFadeColor: false);
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
