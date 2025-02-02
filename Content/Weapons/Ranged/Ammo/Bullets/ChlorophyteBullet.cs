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
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Bullets
{

    public class ChlorophyteBullet : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ChlorophyteBullet);
        }

        Color CursedGreen = new Color(162, 230, 47);

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

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value;
            trail1.trailPointLimit = 255 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(20 * totalAlpha);
            trail1.trailMaxLength = 255 + trailRandomLengthOffset; 

            trail1.shouldSmooth = false;

            Color between = Color.Lerp(Color.ForestGreen, Color.LawnGreen, 0.65f);

            Color trailCol = Color.Lerp(Color.ForestGreen, Color.LawnGreen, 0.45f);
            trail1.trailColor = trailCol * totalAlpha * 0.75f;
            trail1.timesToDraw = 2;


            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();


            if (timer % 5 == 0 && Main.rand.NextBool() && timer > 15)
            {
                Dust grass = Dust.NewDustPerfect(projectile.Center, DustID.ChlorophyteWeapon, Main.rand.NextVector2Circular(2, 2), 0, Scale: 1f);
                grass.velocity -= projectile.velocity * 0.65f;
                grass.noGravity = true;
                grass.color = Color.Green;
            }

            if (timer > 5 && timer % 4 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: between, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.45f);
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

            timer++;

            #region vanillaAI
            if (projectile.type == 207 && projectile.alpha < 170)
            {
                for (int num92 = 0; num92 < 0; num92++) //!10 dear god why
                {
                    float x2 = projectile.position.X - projectile.velocity.X / 10f * (float)num92;
                    float y2 = projectile.position.Y - projectile.velocity.Y / 10f * (float)num92;
                    int num93 = Dust.NewDust(new Vector2(x2, y2), 1, 1, 75);
                    Main.dust[num93].alpha = projectile.alpha;
                    Main.dust[num93].position.X = x2;
                    Main.dust[num93].position.Y = y2;
                    Main.dust[num93].velocity *= 0f;
                    Main.dust[num93].noGravity = true;
                }
            }
            float num94 = (float)Math.Sqrt(projectile.velocity.X * projectile.velocity.X + projectile.velocity.Y * projectile.velocity.Y);
            float num95 = projectile.localAI[0];
            if (num95 == 0f)
            {
                projectile.localAI[0] = num94;
                num95 = num94;
            }
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 25;
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            float num96 = projectile.position.X;
            float num97 = projectile.position.Y;
            float num98 = 300f;
            bool flag5 = false;
            int num99 = 0;
            if (projectile.ai[1] == 0f)
            {
                for (int num101 = 0; num101 < 200; num101++)
                {
                    if (Main.npc[num101].CanBeChasedBy(this) && (projectile.ai[1] == 0f || projectile.ai[1] == (float)(num101 + 1)))
                    {
                        float num102 = Main.npc[num101].position.X + (float)(Main.npc[num101].width / 2);
                        float num103 = Main.npc[num101].position.Y + (float)(Main.npc[num101].height / 2);
                        float num104 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num102) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num103);
                        if (num104 < num98 && Collision.CanHit(new Vector2(projectile.position.X + (float)(projectile.width / 2), projectile.position.Y + (float)(projectile.height / 2)), 1, 1, Main.npc[num101].position, Main.npc[num101].width, Main.npc[num101].height))
                        {
                            num98 = num104;
                            num96 = num102;
                            num97 = num103;
                            flag5 = true;
                            num99 = num101;
                        }
                    }
                }
                if (flag5)
                {
                    projectile.ai[1] = num99 + 1;
                }
                flag5 = false;
            }
            if (projectile.ai[1] > 0f)
            {
                int num105 = (int)(projectile.ai[1] - 1f);
                if (Main.npc[num105].active && Main.npc[num105].CanBeChasedBy(this, ignoreDontTakeDamage: true) && !Main.npc[num105].dontTakeDamage)
                {
                    float num106 = Main.npc[num105].position.X + (float)(Main.npc[num105].width / 2);
                    float num107 = Main.npc[num105].position.Y + (float)(Main.npc[num105].height / 2);
                    if (Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num106) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num107) < 1000f)
                    {
                        flag5 = true;
                        num96 = Main.npc[num105].position.X + (float)(Main.npc[num105].width / 2);
                        num97 = Main.npc[num105].position.Y + (float)(Main.npc[num105].height / 2);
                    }
                }
                else
                {
                    projectile.ai[1] = 0f;
                }
            }
            if (!projectile.friendly)
            {
                flag5 = false;
            }
            if (flag5)
            {
                float num244 = num95;
                Vector2 vector19 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                float num108 = num96 - vector19.X;
                float num109 = num97 - vector19.Y;
                float num112 = (float)Math.Sqrt(num108 * num108 + num109 * num109);
                num112 = num244 / num112;
                num108 *= num112;
                num109 *= num112;
                int num113 = 8;
                if (projectile.type == 837)
                {
                    num113 = 32;
                }
                projectile.velocity.X = (projectile.velocity.X * (float)(num113 - 1) + num108) / (float)num113;
                projectile.velocity.Y = (projectile.velocity.Y * (float)(num113 - 1) + num109) / (float)num113;
            }
            #endregion

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            return false;
        }

        float totalScale = 0f;
        float totalAlpha = 0f;
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

            Color outSpikeColor = Color.Green;
            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f * 0.9f, adjustedScale * 1.5f * totalScale) * 0.5f;
            Main.EntitySpriteDraw(spike, drawPos, null, outSpikeColor with { A = 0 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            Color orbColor = Color.Lerp(Color.ForestGreen, Color.LawnGreen, 0.75f);
            Vector2 orbScale = new Vector2(1f * 0.9f, 0.3f * totalScale) * 0.7f * adjustedScale;
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, orbColor with { A = 0 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);

            //trail1.TrailDrawing(Main.spriteBatch);

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
            Vector2 drawScale = new Vector2(adjustedScale * 2f * 0.9f, adjustedScale * totalScale) * 0.5f;

            Color spikeColor = Color.Lerp(Color.ForestGreen, Color.LawnGreen, 0.25f);

            sb.Draw(spike, drawPos, null, spikeColor * 2f * totalAlpha, drawRot, drawOrigin, drawScale, SpriteEffects.None, 0f);
            sb.Draw(spike, drawPos, null, Color.White * totalAlpha, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None, 0f);

            trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            Color dustCol = Color.Lerp(Color.ForestGreen, Color.LawnGreen, 0.45f);


            for (int i = 0; i < 2 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCol, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            for (int i = 0; i < 1 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-2f, 2f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, DustID.ChlorophyteWeapon, dustVel, newColor: dustCol, Scale: 0.85f);
                p.noGravity = true;
                p.noLight = true;
            }

            //Particles on trail
            int count = trail1.trailPositions.Count;
            for (int i = (int)(count * 0.15f); i < count; i += 5)
            {
                if (Main.rand.NextBool())
                {
                    
                    Vector2 pos = trail1.trailPositions[i];
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);


                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelCross>(), vel, newColor: dustCol, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
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
