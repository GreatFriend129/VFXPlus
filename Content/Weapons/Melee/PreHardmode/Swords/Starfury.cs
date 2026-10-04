using Microsoft.Build.Evaluation;
using Microsoft.Build.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Mono.Cecil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Misc;
using VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Swords
{
    
    public class Starfury : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Starfury);
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults(Item entity)
        {
            //entity.shootsEveryUse = true;
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity);
        }

        public override void UseAnimation(Item item, Player player)
        {
            //Starfury shooting is at 45120 in Player.cs | maxmana ding is at 39040 |
            //39718 for flag setting stuff that doesn't shoot every swing (beam sword, ice blade)

            float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
            int trail = Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<BaseSwordProj>(), 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);

            Vector3[] gradCols = {
                    Color.Black.ToVector3(),
                    Color.DeepPink.ToVector3(),
                    Color.HotPink.ToVector3(),
                    Color.Pink.ToVector3(),
                };


            SwordProjInfo info = new SwordProjInfo(item.type, gradCols, 8f, 0f, 50f, 5, 3f, 1f, 1f);
            info.flowSpeed = 0f;
            (Main.projectile[trail].ModProjectile as BaseSwordProj).info = info;

            base.UseAnimation(item, player);
        }

        public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
        {
            if (Main.rand.Next(5) == 0)
            {
                //Color col = Main.rand.NextBool(2) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
                //int d = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<PulseInOutDust>(), 0f, 0f, 0, col, Main.rand.NextFloat(0.35f, 0.55f) * 2f);
                //Main.dust[d].velocity *= 0.25f;
                //Main.dust[d].customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 20, 0.5f, 0.5f, Pixelize: true);
            }

            //Using itemAnimation instead of itemTime is very important here
            //Sometimes itemTime will stay at zero even when using the weapon for some reason
            if (player.itemAnimation % 4 == 0)
            {
                Color col = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                GeneralUtilities.GetPointOnSwungItemPath(player, 40f, 40f, 0.35f + 0.65f * Main.rand.NextFloat(), player.GetAdjustedItemScale(item), out var location2, out var outwardDirection2);

                Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);

                Dust d = Dust.NewDustPerfect(location2, ModContent.DustType<PulseInOutDust>(), vector2 * 1.5f, 0, col with { A = 200 }, Main.rand.NextFloat(0.7f, 0.9f));

                int pulseTime = Main.rand.Next(18, 22);
                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, pulseTime, 0.5f, 0.5f, Pixelize: true);
            }

            if (player.itemAnimation % 4 == 0)
            {
                Color col = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                GeneralUtilities.GetPointOnSwungItemPath(player, 40f, 40f, 0.35f + 0.65f * Main.rand.NextFloat(), player.GetAdjustedItemScale(item), out var location2, out var outwardDirection2);

                Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);

                //Dust d = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<GlowPixelCross>(), 0f, 0f, 0, col, Main.rand.NextFloat(0.15f, 0.3f));
                //d.velocity *= 0.25f;
                //d.velocity += vector2 * 0.5f;

                Dust d = Dust.NewDustPerfect(location2, ModContent.DustType<GlowPixelCross>(), vector2 * 1.5f, 0, col with { A = 200 }, Main.rand.NextFloat(0.15f, 0.3f));
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.1f, timeBeforeSlow: 3, preSlowPower: 0.99f, postSlowPower: 0.92f,
                    velToBeginShrink: 0.75f, fadePower: 0.95f, shouldFadeColor: false);
            }

            Lighting.AddLight(player.itemLocation, Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f).ToVector3() * 0.25f);

            //Vanilla
            /*
            if (Main.rand.Next(5) == 0)
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 58, 0f, 0f, 150, default(Color), 1.2f);
            }
            if (Main.rand.Next(10) == 0)
            {
                Gore.NewGore(null, new Vector2(hitbox.X, hitbox.Y), default(Vector2), Main.rand.Next(16, 18));
            }
            */

            base.MeleeEffects(item, player, hitbox);
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {

            Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<StarfuryImpactVFX>(), 0, 0, Main.myPlayer);


            float randRot = Main.rand.NextFloat(6.28f);

            Color newPink = Main.hslToRgb(0.92f, 1f, 0.6f) with { A = 50 };

            int pulse = Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
            //(Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            (Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            Main.projectile[pulse].rotation = randRot - MathHelper.PiOver4 * 1f;

            int pulse2 = Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
            (Main.projectile[pulse2].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            Main.projectile[pulse2].rotation = randRot + MathHelper.PiOver4 * 1f;

            //Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<StarfuryImpactVFX>(), 0, 0, Main.myPlayer);

            /*
            int dustCount = 5 + Main.rand.Next(0, 3);
            for (int i = 220; i < dustCount; i++)
            {
                float prog = (float)(i + 1f) / dustCount;
                Color col = Main.rand.NextBool(2) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);


                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5f);

                Dust p = Dust.NewDustPerfect(target.Center + vel, ModContent.DustType<WindLine>(), vel, newColor: col * 1.5f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 2f);

                float velFadePower = Main.rand.NextFloat(0.9f, 0.93f);
                int shrinkTime = Main.rand.Next(2, 5);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: velFadePower, TimeToStartShrink: shrinkTime, ShrinkYScalePower: 0.85f, XScale: 0.5f, YScale: 0.5f, Pixelize: true);
                wlb.colorAlpha = 40;
                wlb.whiteCoreIntensity = 0f;

                p.customData = wlb;
            }
            */

            /*
            float randomRot = Main.rand.NextFloat(6.28f);

            int dustCount = 12;
            for (int i = 0; i < dustCount; i++)
            {
                float progress = (float)i / (float)dustCount;
                float theta = progress * MathHelper.TwoPi;

                float numer = MathF.Cos((2f * MathF.Asin(1) + 3f * MathHelper.Pi) / 10f);
                float denom = MathF.Cos((2f * MathF.Asin(MathF.Cos(4f * theta)) + 3f * MathHelper.Pi) / 10f);

                float r = numer / denom;

                Color dustCol = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                Vector2 vel = new Vector2(r * 1.5f, 0f).RotatedBy(randomRot + theta);

                Dust d = Dust.NewDustPerfect(target.Center + vel, ModContent.DustType<PulseInOutDust>(), vel, newColor: dustCol with { A = 200 });
                d.scale *= Main.rand.NextFloat(0.75f, 1f) * 1f;
                //d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.ShakyStar, 40, 0.05f, 0.95f, Pixelize: true);

                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 30, 0.15f, 0.85f, Pixelize: true);

            }
            */
        }

    }

    public class StarfuryShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Starfury);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && false)
            {
                float randRot = (projectile.oldVelocity).ToRotation();

                Color newPink = Main.hslToRgb(0.92f, 0.95f, 0.62f);

                int pulse = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
                //(Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                (Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                Main.projectile[pulse].rotation = randRot - MathHelper.PiOver4 * 0.5f;

                int pulse2 = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
                (Main.projectile[pulse2].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                Main.projectile[pulse2].rotation = randRot + MathHelper.PiOver4 * 0.5f;
            }
            
            if (timer == 0)
            {
                Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<SuperStarShooterConstellationTest>(), 0, 0, projectile.owner, projectile.whoAmI);

            }

            int trailCount = 8;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer % 2 == 0 && Main.rand.NextBool())
            {
                Color dustCol = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f) + projectile.velocity * 0.02f;

                Dust d = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(10f, 10f), ModContent.DustType<PulseInOutDust>(), dustVel, newColor: dustCol with { A = 200 });
                d.scale *= Main.rand.NextFloat(1f, 1.5f) * 1f;

                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 14, 0.15f, 0.85f, Pixelize: true);
            }

            overallAlpha = 1f;
            //overallScale = 1f;

            //overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.05f), 0f, 1f);

            float fadeInTime = Math.Clamp((timer + 6f) / 18f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;
            return true;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        float overallScale = 1f;
        float overallAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawPixelatedStuff(projectile, false);
            });
            DrawPixelatedStuff(projectile, true);

            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D StarGlow = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarGlow").Value;

            //Starfury uses 0.8 scale so x1.25 that is one
            float drawScale = projectile.scale * 1.25f * overallScale;

            Vector2 drawPos = projectile.Center - Main.screenPosition;


            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;

            #region vanillaDraw
            Color color = new Color(255, 255, 255, Color.White.A - projectile.alpha);
            Vector2 vector = projectile.velocity;
            Color color2 = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f) * 0.25f;
            Vector2 spinningpoint = new Vector2(0f, -4f);
            float num = 0f;
            float t = vector.Length();
            float num2 = Utils.GetLerpValue(3f, 5f, t, clamped: true);
            bool flag = true;
            if (projectile.type == ProjectileID.SparkleGuitar || projectile.type == ProjectileID.FirstFractal) //TRUE
            {
                vector = projectile.position - projectile.oldPos[1];
                float num3 = vector.Length();
                if (num3 == 0f)
                {
                    vector = Vector2.UnitY;
                }
                else
                {
                    vector *= 5f / num3;
                }
                Vector2 origin = new Vector2(projectile.ai[0], projectile.ai[1]);
                Vector2 center = Main.player[projectile.owner].Center;
                float lerpValue = Utils.GetLerpValue(0f, 120f, origin.Distance(center), clamped: true);
                float num4 = 90f;
                if (projectile.type == 857)
                {
                    num4 = 60f;
                    flag = false;
                }
                float lerpValue2 = Utils.GetLerpValue(num4, num4 * (5f / 6f), projectile.localAI[0], clamped: true);
                float lerpValue3 = Utils.GetLerpValue(0f, 120f, projectile.Center.Distance(center), clamped: true);
                lerpValue *= lerpValue3;
                lerpValue2 *= Utils.GetLerpValue(0f, 15f, projectile.localAI[0], clamped: true);
                color2 = Color.HotPink * 0.15f * (lerpValue2 * lerpValue);
                if (projectile.type == 857)
                {
                    color2 = projectile.GetFirstFractalColor() * 0.15f * (lerpValue2 * lerpValue);
                }
                spinningpoint = new Vector2(0f, -2f);
                float lerpValue4 = Utils.GetLerpValue(num4, num4 * (2f / 3f), projectile.localAI[0], clamped: true);
                lerpValue4 *= Utils.GetLerpValue(0f, 20f, projectile.localAI[0], clamped: true);
                num = -0.3f * (1f - lerpValue4);
                num += -1f * Utils.GetLerpValue(15f, 0f, projectile.localAI[0], clamped: true);
                num *= lerpValue;
                num2 = lerpValue2 * lerpValue;
            }
            Vector2 vector5 = projectile.Center + vector;
            Texture2D value = TextureAssets.Projectile[projectile.type].Value;
            _ = new Rectangle(0, 0, value.Width, value.Height).Size() / 2f;
            Texture2D value2 = FireBall;// 
            Rectangle value3 = value2.Frame();
            Vector2 origin2 = new Vector2((float)value3.Width / 2f, 10f);
            _ = Color.Cyan * 0.5f * num2;
            Vector2 vector2 = new Vector2(0f, projectile.gfxOffY);
            float num5 = (float)Main.timeForVisualEffects / 60f;
            Vector2 vector3 = vector5 + vector * 0.5f;
            Color color3 = Color.White * 0.5f * num2;
            color3.A = 0;
            Color color4 = color2 * num2;
            color4.A = 0;
            Color color5 = color2 * num2;
            color5.A = 0;
            Color color6 = color2 * num2;
            color6.A = 0;
            float num6 = vector.ToRotation();
            num *= 1.5f;
            //Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5), value3, color4 * overallAlpha, num6 + (float)Math.PI / 2f, origin2, 1.5f + num, SpriteEffects.None);
            //Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + (float)Math.PI * 2f / 3f), value3, color5 * overallAlpha, num6 + (float)Math.PI / 2f, origin2, 1.1f + num, SpriteEffects.None);
            //Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + 4.18879032f), value3, color6 * overallAlpha, num6 + (float)Math.PI / 2f, origin2, 1.3f + num, SpriteEffects.None);
            //^ Outer fireballs

            Vector2 vector4 = vector5 - vector * 0.5f;
            for (float num7 = 0f; num7 < 1f; num7 += 0.5f) //draw white inner fireball
            {
                float num8 = num5 % 0.5f / 0.5f;
                num8 = (num8 + num7) % 1f;
                float num9 = num8 * 2f;
                if (num9 > 1f)
                {
                    num9 = 2f - num9;
                }
                //Main.EntitySpriteDraw(value2, vector4 - Main.screenPosition + vector2, value3, color3 * num9 * overallAlpha, num6 + (float)Math.PI / 2f, origin2, 0.3f + num8 * 0.5f, SpriteEffects.None);
            }
            #endregion


            Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -25f;
            Color fireBallColor = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);
            for (int i = 0; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, fireBallColor with { A = 100 } * 0.5f, fireballRot, FireBall.Size() / 2f, drawScale * 1.05f, SpriteEffects.None);
            }

            Vector2 fireballPosA = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f;
            float time = (float)Main.timeForVisualEffects / 60f;
            for (float i = 220f; i < 1f; i += 0.5f)
            {
                float drawS = time % 0.5f / 0.5f;
                drawS = (drawS + i) % 1f;

                float drawA = drawS * 2f;
                if (drawA > 1f)
                    drawA = 2f - drawA;

                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                //Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                //Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, fireballPosA+ new Vector2(0f, 0f), null, fireBallColor with { A = 100 } * drawA, fireballRot, new Vector2((float)FireBall.Width / 2f, 10f), 0.6f + drawS * 1f, SpriteEffects.None);
            }

            Vector2 fireballPos2 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * 0f;
            for (float i = 0f; i < 1f; i += 0.5f)
            {
                float drawS = time % 0.5f / 0.5f;
                drawS = (drawS + i) % 1f;

                float drawA = drawS * 2f;
                if (drawA > 1f)
                    drawA = 2f - drawA;

                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                //Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                //Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, fireballPos2 + new Vector2(0f, 0f), null, Color.White with { A = 100 } * drawA, fireballRot, new Vector2((float)FireBall.Width / 2f, 10f), 0.3f + drawS * 0.5f, SpriteEffects.None);
            }

            //Main.EntitySpriteDraw(StarGlow, drawPos, null, Color.White with { A = 200 } * overallAlpha, projectile.rotation, StarGlow.Size() / 2f, drawScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(Star, drawPos, null, Color.HotPink * overallAlpha, projectile.rotation, Star.Size() / 2f, drawScale, SpriteEffects.None);

            Main.EntitySpriteDraw(StarGlow, drawPos, null, Color.DeepPink with { A = 200 } * overallAlpha, projectile.rotation, StarGlow.Size() / 2f, drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 50 } * overallAlpha, projectile.rotation, Star.Size() / 2f, drawScale, SpriteEffects.None);



            //Gash
            Texture2D gash = CommonTextures.SoulSpikePMA.Value;

            Vector2 gashPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * 0f;
            float gashProg = Utils.GetLerpValue(0, 20, timer, true);
            float gashRot = 0f;// projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Vector2 gashScale = new Vector2(1.5f * Easings.easeOutSine(gashProg), 0.25f) * drawScale;
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.HotPink with { A = 50 } * Easings.easeOutQuad(1f - gashProg) * 1f, gashRot, gash.Size() / 2f, gashScale * 2f, SpriteEffects.None);
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.White with { A = 50 } * Easings.easeOutQuad(1f - gashProg) * 1f, gashRot, gash.Size() / 2f, gashScale * 1f, SpriteEffects.None);

            Vector2 newGashScale = new Vector2(1f, 0.5f) * drawScale;
            Vector2 newGashScale2 = new Vector2(1f + ((float)(Main.timeForVisualEffects * 0.05f) % 1f), 0.5f) * drawScale;
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.DeepPink with { A = 100 } * 0.5f, gashRot, gash.Size() / 2f, newGashScale * 2f, SpriteEffects.None);
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.White with { A = 100 } * 0.5f, gashRot, gash.Size() / 2f, newGashScale2 * 1f, SpriteEffects.None);



            return false;

        }

        public void DrawPixelatedStuff(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Trail = CommonTextures.SoulSpikePMA.Value;

            //Starfury uses 0.8 scale so x1.25 that is one
            float drawScale = projectile.scale * 1.25f * overallScale;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float colorProg = (progress * 4f) % 1f;
                Color col = Color.DeepPink;// (i + 1) % 3 == 0 ? Color.Gold : Color.HotPink;// Color.Lerp(Color.LightPink, Color.Gold, colorProg);

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Vector2 trailScale = new Vector2(1.5f, 0.7f * drawScale * Easings.easeInOutSine(progress));

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, col with { A = 100 } * 1f * progress,
                       previousRotations[i], Trail.Size() / 2f, trailScale, SpriteEffects.None);

                //Main.EntitySpriteDraw(FireBall, AfterImagePos, null, Color.HotPink with { A = 20 } * 1f * progress,
                //       previousRotations[i] + MathHelper.PiOver2, FireBall.Size() / 2f, new Vector2(trailScale.Y, trailScale.X), SpriteEffects.None);

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, Color.White with { A = 100 } * 0.85f * progress,
                    previousRotations[i], Trail.Size() / 2f, new Vector2(trailScale.X, trailScale.Y * 0.5f), SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;
            
            Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<StarfuryImpactVFX>(), 0, 0, Main.myPlayer);

            
            float randRot = (-projectile.oldVelocity).ToRotation();

            Color newPink = Main.hslToRgb(0.92f, 1f, 0.6f);

            int pulse = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
            //(Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            (Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            Main.projectile[pulse].rotation = randRot - MathHelper.PiOver4 * 0.75f;

            int pulse2 = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
            (Main.projectile[pulse2].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            Main.projectile[pulse2].rotation = randRot + MathHelper.PiOver4 * 0.75f;
            

            float randomRot = Main.rand.NextFloat(6.28f);

            int dustCount = 12 * 0;
            for (int i = 0; i < dustCount; i++)
            {
                float progress = (float)i / (float)dustCount;
                float theta = progress * MathHelper.TwoPi;

                float numer = MathF.Cos((2f * MathF.Asin(1) + 3f * MathHelper.Pi) / 10f);
                float denom = MathF.Cos((2f * MathF.Asin(MathF.Cos(4f * theta)) + 3f * MathHelper.Pi) / 10f);

                float r = numer / denom;

                Color dustCol = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                Vector2 vel = new Vector2(r * 4f, 0f).RotatedBy(randomRot + theta);

                Dust d = Dust.NewDustPerfect(projectile.Center + vel, ModContent.DustType<GlowPixelCross>(), vel, newColor: dustCol with { A = 200 });
                d.scale *= Main.rand.NextFloat(0.5f, 0.65f) * 0.5f;
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(velToBeginShrink: 0.5f, shouldFadeColor: false);

                //d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.ShakyStar, 40, 0.05f, 0.95f, Pixelize: true);

                //d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 20, 0.15f, 0.85f, Pixelize: true);

            }


            //int b = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<StarfuryImpactVFX>(), 0, 0, Main.myPlayer);
            //Main.projectile[b].scale *= 0.75f;
            //Main.projectile[b].rotation = MathHelper.PiOver4;


            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }


    public class StarfuryImpactVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.scale = 1.25f;
            Projectile.timeLeft = 400;
        }

        int timer = 0;
        float overallAlpha = 1f;
        float overallScale = 1f;

        public override void AI()
        {
            int timeForEffect = 14;

            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);

            effectProgress = (Utils.GetLerpValue(0, timeForEffect, timer, true));

            if (timer == timeForEffect)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawEffect(false);
            });
            DrawEffect(true);

            return false;
        }

        float effectProgress = 0f;
        public void DrawEffect(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Star = CommonTextures.RainbowRod.Value;
            //Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingB").Value;


            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            //OuterStar
            //float outerStarScale = GeneralUtilities.FadeLinear(effectProgress, 0.5f, 0.5f) * Projectile.scale * overallScale;
            //Vector2 outerDrawScale = new Vector2(outerStarScale, outerStarScale);
            //Main.EntitySpriteDraw(Star, drawPos, null, Color.HotPink with { A = 50 } * overallAlpha, Projectile.rotation, Star.Size() / 2f, outerDrawScale * 1.5f, SpriteEffects.None);

            //InnerStar
            //float innerStarScale = GeneralUtilities.FadeLinear(effectProgress, 0.25f, 0.75f) * Projectile.scale * overallScale;
            //Vector2 innerDrawScale = new Vector2(innerStarScale, innerStarScale);

            //float innerStarScale = MathF.Pow((float)Math.Sin((TimeInWorld) * (MathHelper.Pi / BurstTime)), 4) * Scale * 0.83f;

            float starScale = MathF.Sin(MathHelper.Pi * effectProgress);
            //float innerStarScale = MathF.Pow((float)Math.Sin((TimeInWorld) * (MathHelper.Pi / BurstTime)), 4) * overallScale * 0.75f;

            Main.EntitySpriteDraw(Star, drawPos, null, Color.DeepPink with { A = 120 } * overallAlpha, Projectile.rotation, Star.Size() / 2f, starScale * Projectile.scale, SpriteEffects.None);

            float innerStarScale = MathF.Pow(starScale, 4);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 120 } * overallAlpha, Projectile.rotation, Star.Size() / 2f, innerStarScale * 0.75f * Projectile.scale, SpriteEffects.None); //0.75


        }
    }

    
    public class StarfuryShotOverrideAlt : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Starfury) && false;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            //Trail
            int trailCount = 12; //12
            previousVelRots.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center + projectile.velocity);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousVelRots = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawShit(false, projectile);
            });
            DrawShit(true, projectile);


            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;
            Vector2 fireballPos = projectile.Center - Main.screenPosition + projectile.velocity.SafeNormalize(Vector2.UnitX) * -25f;
            Color fireBallColor = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);
            for (int i = 220; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, fireBallColor with { A = 200 } * 0.25f, fireballRot, FireBall.Size() / 2f, 1f * 1.05f, SpriteEffects.None);
            }

            Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;
            Texture2D tex2 = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarGlow").Value;
            
            Main.spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, Color.DeepPink, projectile.rotation, tex2.Size() / 2f, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);

            return false;

        }

        Effect myEffect = null;
        public void DrawShit(bool giveUp, Projectile projectile)
        {
            if (giveUp)
                return;

            #region orb

            //Glorb
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color[] cols = { Color.HotPink * 0.75f, Color.DeepPink * 0.525f, Color.DeepPink * 0.375f };
            float[] scales = { 1.15f, 1.6f, 2.5f };

            float orbRot = projectile.velocity.ToRotation();
            float orbAlpha = 0.6f * overallAlpha;
            Vector2 orbScale = new Vector2(0.85f, 0.85f) * 0.45f * projectile.scale * overallScale;
            Vector2 orbOrigin = orb.Size() / 2f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[2] * sineScale2, SpriteEffects.None);

            #endregion

            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;
            Vector2 fireballPos = projectile.Center - Main.screenPosition + projectile.velocity.SafeNormalize(Vector2.UnitX) * -35f;
            Color fireBallColor = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);
            for (int i = 0; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Vector2 fireballScale = new Vector2(1.05f, 1.25f);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, fireBallColor with { A = 0 } * 0.25f, fireballRot, FireBall.Size() / 2f, fireballScale, SpriteEffects.None);
            }

            #region SolidTrail(good) use for fibber 
            //Trail
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousVelRots.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.09f) * 0.15f;

            Color StripColor(float progress) => Color.White * (progress * progress);
            float StripWidthUnder(float progress) => 20f * Easings.easeInCubic(progress) * overallScale * sineWidthMult * 0.6f;
            float StripWidthOver(float progress) => 8f * Easings.easeInCubic(progress) * overallScale * sineWidthMult * 0.6f;

            VertexStrip vertexStripUnder = new VertexStrip();
            vertexStripUnder.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidthUnder, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStripOver = new VertexStrip();
            vertexStripOver.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidthOver, -Main.screenPosition, includeBacksides: true);



            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.05f * 0f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Lerp(Color.HotPink, Color.DeepPink, 0.5f).ToVector3() * 1f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStripUnder.DrawTrail();


            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(Color.Pink.ToVector3() * 1f);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f); //0.6
            myEffect.Parameters["glowIntensity"].SetValue(2f); //2.25
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStripOver.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            #endregion


            //Texture2D tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;
            //Texture2D tex2 = Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Misc/Ranged/Guns/Fibber/VanillaStarBorder").Value;
            //Color col = colorOptions[colorIndex];

            //Main.spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, Color.Blue, Projectile.rotation, tex2.Size() / 2f, 1f, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }
    }
}
