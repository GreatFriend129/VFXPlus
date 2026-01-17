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

    public class ExplodingBulletProjOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ExplosiveBullet);
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
            trail1.trailPointLimit = 125 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(20 * totalAlpha);
            trail1.trailMaxLength = 125 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = new Color(255, 44, 7) * totalAlpha;


            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();


            if (timer > 5 && timer % 3 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: new Color(255, 44, 7), Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.25f, 0.08f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);

            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

            Lighting.AddLight(projectile.Center, Color.OrangeRed.ToVector3() * 0.7f);

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
            Vector2 drawScale = new Vector2(adjustedScale * 2f, adjustedScale * totalScale) * 0.5f;

            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale) * 0.5f;
            Main.EntitySpriteDraw(spike, drawPos, null, Color.OrangeRed with { A = 0 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            Vector2 orbScale = new Vector2(1f, 0.3f * totalScale) * 0.7f * adjustedScale;
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, new Color(255, 55, 6) with { A = 0 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


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

            Color spikeColor = Color.Lerp(Color.OrangeRed, Color.Red, 0.25f);

            sb.Draw(spike, drawPos, null, Color.OrangeRed * 2f * totalAlpha, drawRot, drawOrigin, drawScale, SpriteEffects.None, 0f);
            sb.Draw(spike, drawPos, null, Color.White * totalAlpha, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None, 0f);

            trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: new Color(255, 55, 6), Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

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

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), vel, newColor: new Color(255, 44, 7), Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
                    d.alpha = 2;
                    d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.35f;
                    d.velocity *= 0.35f;
                }
            }

            #region Explosion
            for (int i = 0; i < 2 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 8; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);

                float progress = (float)i / 7;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 2.5f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.55f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 5 + Main.rand.Next(0, 3); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 13, postSlowPower: 0.92f,
                    velToBeginShrink: 4f, fadePower: 0.91f, shouldFadeColor: false);
            }

            #endregion

            SoundStyle styleA = new SoundStyle("VFXPlus/Sounds/Effects/Fire/FlareImpact") with { Volume = 0.25f, Pitch = -.1f, PitchVariance = .2f, };
            SoundEngine.PlaySound(styleA, projectile.Center);

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.35f, Pitch = -0.1f, PitchVariance = 0.15f }, projectile.Center);


            #region vanillaKill
            //SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0f, PitchVariance = 0f }, projectile.position);
            for (int num672 = 0; num672 < 7; num672++)
            {
                //int num674 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
            }
            for (int num675 = 0; num675 < 3; num675++)
            {
                //int num676 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2.5f);
                //Main.dust[num676].noGravity = true;
                //Dust dust161 = Main.dust[num676];
                //Dust dust334 = dust161;
                //dust334.velocity *= 3f;
                //num676 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
                //dust161 = Main.dust[num676];
                //dust334 = dust161;
                //dust334.velocity *= 2f;
            }
            //int num677 = Gore.NewGore(null, new Vector2(projectile.position.X - 10f, projectile.position.Y - 10f), default(Vector2), Main.rand.Next(61, 64));
            //Gore gore38 = Main.gore[num677];
            //Gore gore64 = gore38;
            //gore64.velocity *= 0.3f;
            //Main.gore[num677].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
            //Main.gore[num677].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
            if (projectile.owner == Main.myPlayer)
            {
                projectile.localAI[1] = -1f;
                projectile.maxPenetrate = 0;
                projectile.position.X += projectile.width / 2;
                projectile.position.Y += projectile.height / 2;
                projectile.width = 80;
                projectile.height = 80;
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
