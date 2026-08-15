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
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Bows;
using VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;
using static Terraria.ModLoader.PlayerDrawLayer;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Swords
{
    
    //Not currently enabled
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
            Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -25f;
            Color fireBallColor = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);
            for (int i = 0; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, fireBallColor with { A = 100 } * 0.25f, fireballRot, FireBall.Size() / 2f, drawScale * 1.05f, SpriteEffects.None);
            }

            //Star Border
            for (int i = 220; i < 4; i++)
            {
                float dist = 2f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(Star, offsetDrawPos, null, Color.HotPink with { A = 150 } * 0.5f, projectile.rotation, Star.Size() / 2f, drawScale * 1.05f, SpriteEffects.None);
            }

            //Main.EntitySpriteDraw(StarGlow, drawPos, null, Color.White with { A = 200 } * overallAlpha, projectile.rotation, StarGlow.Size() / 2f, drawScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(Star, drawPos, null, Color.HotPink * overallAlpha, projectile.rotation, Star.Size() / 2f, drawScale, SpriteEffects.None);

            Main.EntitySpriteDraw(StarGlow, drawPos, null, Color.DeepPink with { A = 200 } * overallAlpha, projectile.rotation, StarGlow.Size() / 2f, drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 50 } * overallAlpha, projectile.rotation, Star.Size() / 2f, drawScale, SpriteEffects.None);



            //Gash
            Texture2D gash = CommonTextures.Flare.Value;

            Vector2 gashPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * 0f;
            float gashProg = Utils.GetLerpValue(0, 20, timer, true);
            float gashRot = 0f;// projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Vector2 gashScale = new Vector2(2f * Easings.easeOutSine(gashProg), 0.5f) * drawScale;
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.HotPink with { A = 50 } * Easings.easeOutQuad(1f - gashProg) * 0.25f, gashRot, gash.Size() / 2f, gashScale * 2f, SpriteEffects.None);
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.White with { A = 50 } * Easings.easeOutQuad(1f - gashProg) * 0.25f, gashRot, gash.Size() / 2f, gashScale * 1f, SpriteEffects.None);

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

            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlurPMA").Value;
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

            Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -25f;
            for (int i = 220; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, Color.HotPink with { A = 150 } * 0.35f, fireballRot, FireBall.Size() / 2f, drawScale * 1.05f, SpriteEffects.None);
            }


            float sineScale = 0.5f + MathF.Sin((float)Main.timeForVisualEffects * 0.5f) * 0.5f;
            float fireBallScale = (0.75f + sineScale * 0.25f);

            Vector2 vec2FireballScale = new Vector2(1.25f, 1f) * fireBallScale * drawScale;

            //Main.EntitySpriteDraw(FireBall, fireballPos, null, Color.HotPink with { A = 150 } * 0.5f, projectile.velocity.ToRotation() + MathHelper.PiOver2, FireBall.Size() / 2f, drawScale * fireBallScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(FireBall, fireballPos, null, Color.HotPink with { A = 150 } * 0.5f, projectile.velocity.ToRotation() + MathHelper.PiOver2, FireBall.Size() / 2f, new Vector2(0.75f, 1.2f) * drawScale * fireBallScale, SpriteEffects.None);

            //Main.EntitySpriteDraw(FireBall, fireballPos, null, Color.DeepPink with { A = 50 } * 0.5f, projectile.velocity.ToRotation() + MathHelper.PiOver2, FireBall.Size() / 2f, vec2FireballScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(FireBall, fireballPos, null, Color.Pink with { A = 50 } * 0.5f, projectile.velocity.ToRotation() + MathHelper.PiOver2, FireBall.Size() / 2f, vec2FireballScale * new Vector2(0.6f, 1f), SpriteEffects.None);

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

    public class StarfuryShotOverrideOld : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Starfury) && false;
        }

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousVelRots = new List<float>();

        int timer = 0;
        public override void AI(Projectile projectile)
        {
            int trailCount = 18; //34
            previousVelRots.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            bool addInBetween = true;
            if (addInBetween)
            {
                previousVelRots.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);

                if (previousVelRots.Count > trailCount)
                    previousVelRots.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }

            timer++;

            base.AI(projectile);
        }

        float alpha = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return true;
            
            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D StarBlack = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;
            Texture2D Line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Nightglow").Value;
            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;

            Texture2D Glorb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            //Starfury uses 0.8 scale so x1.25 that is one
            float scale = projectile.scale * 1.25f;

            //Nightglow
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            if (previousVelRots != null && previousPositions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i++)
                {
                    float progress = (float)i / previousVelRots.Count;
                    float size = (1f - (progress * 0.5f)) * scale;

                    float colVal = progress * alpha;

                    Color col = Color.Lerp(Color.LightGoldenrodYellow * 0.75f, Color.HotPink, progress) * progress * 0.5f;

                    float size2 = (1f - (progress * 0.15f)) * scale;
                    Vector2 vec2Scale = new Vector2(2f, 3f) * size;

                    //Black
                    Main.EntitySpriteDraw(Line, previousPositions[i] - Main.screenPosition, null, Color.Black * 0.15f * (colVal * colVal),
                            previousVelRots[i] + MathHelper.PiOver2, Line.Size() / 2f, vec2Scale * size2, SpriteEffects.None);

                    Main.EntitySpriteDraw(StarBlack, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.85f * colVal,
                            previousVelRots[i], StarBlack.Size() / 2f, size2, SpriteEffects.None);

                    Main.EntitySpriteDraw(Line, previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(5f, 5f), null, col with { A = 0 } * 2f * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, Line.Size() / 2f, vec2Scale * size2, SpriteEffects.None);

                }

            }
            float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

            Main.EntitySpriteDraw(Glorb, drawPos, null, Color.HotPink with { A = 0 } * alpha * 0.2f, projectile.rotation, Glorb.Size() / 2f, scale * 1.5f + sineScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Glorb, drawPos, null, Color.LightPink with { A = 0 } * alpha * 0.3f, projectile.rotation, Glorb.Size() / 2f, scale * 1f + sineScale, SpriteEffects.None);


            for (int i = 0; i < 6; i++)
            {
                Color col = Color.DeepPink;
                Main.EntitySpriteDraw(Star, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), null, col with { A = 0 } * 0.8f * alpha, projectile.rotation, Star.Size() / 2f, scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(Star, drawPos, null, Color.HotPink * alpha, projectile.rotation, Star.Size() / 2f, scale * 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(StarBlack, drawPos, null, Color.White with { A = 0 } * 0.35f * alpha, projectile.rotation, StarBlack.Size() / 2f, scale * 1.1f, SpriteEffects.None);

            for (int i = 0; i < 4; i++)
            {
                Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f;
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, Color.HotPink with { A = 0 } * 0.35f, fireballRot, FireBall.Size() / 2f, projectile.scale * 1.05f * alpha, SpriteEffects.None);
            }


            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }
}
