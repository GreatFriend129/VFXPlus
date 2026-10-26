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
using static Terraria.ModLoader.PlayerDrawLayer;


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

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Magic/QuickShine") with { Volume = 0.35f, Pitch = -.25f, PitchVariance = .2f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

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

            if (timer == 0 && Main.myPlayer == projectile.owner)
            {
                ConstellationTrail ConTrail = (ConstellationTrail)Projectile.NewProjectileDirect(null, projectile.Center, projectile.velocity, ModContent.ProjectileType<ConstellationTrail>(), 0, 0, projectile.owner, projectile.whoAmI).ModProjectile;
                ConTrail.starCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f) with { A = 160 };
                ConTrail.lineCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f) with { A = 175 }; //Color.Lerp(Color.Gold, Color.Orange, 0.75f);
                ConTrail.parentType = ProjectileID.Starfury;
            }

            int trailCount = 10;
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

            //Sound
            if (timer == 0)
            {
                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Magic/QuickShine") with { Volume = 1f, Pitch = -.3f, PitchVariance = .2f, MaxInstances = -1 };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Tech/MagicImpactShort") with { Volume = .3f, Pitch = 0.20f, PitchVariance = .1f, MaxInstances = -1 };
                SoundEngine.PlaySound(style2, projectile.Center);

                //SoundStyle style3 = new SoundStyle("VFXPlus/Sounds/Effects/Tech/MagicPunch") with { Volume = 0.15f, Pitch = -.15f, PitchVariance = 0.1f, MaxInstances = -1 };
                //SoundEngine.PlaySound(style3, projectile.Center);

            }
            else if (timer % 30 == 0)
            {
                //SoundStyle starPulse = new SoundStyle("VFXPlus/Sounds/Effects/Magic/Ding") with { Volume = 0.3f, Pitch = -.85f, PitchVariance = .2f, MaxInstances = -1 };
                //SoundEngine.PlaySound(starPulse, projectile.Center);
            }

            timer++;
            projectile.ai[2] = timer;

            projectile.soundDelay = 10;

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
            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;

            //Starfury uses 0.8 scale so x1.25 that is one
            float drawScale = projectile.scale * 1.25f * overallScale;
            Vector2 drawPos = projectile.Center - Main.screenPosition;


            //Glorb
            Texture2D Glorb = CommonTextures.feather_circle128PMA.Value;
            Vector2 glorbPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -8f;
            Vector2 glorbScale = new Vector2(1f, 0.75f) * drawScale;
            Main.EntitySpriteDraw(Glorb, glorbPos + new Vector2(0f, 0f), null, Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f) with { A = 200 } * 0.35f, projectile.velocity.ToRotation(), Glorb.Size() / 2f, glorbScale * 1.15f, SpriteEffects.None);


            //Outer fireball
            Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -25f;
            Color fireBallColor = Color.Lerp(Color.DeepPink, Color.HotPink, 0.8f);
            for (int i = 0; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, fireBallColor with { A = 150 } * 0.5f, fireballRot, FireBall.Size() / 2f, drawScale * 1.05f, SpriteEffects.None);
            }

            //Inner white fireball
            Vector2 fireballPos2 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * 0f;
            float time = (float)Main.timeForVisualEffects / 60f;
            for (float i = 0f; i < 1f; i += 0.5f)
            {
                float drawS = time % 0.5f / 0.5f;
                drawS = (drawS + i) % 1f;

                float drawA = drawS * 2f;
                if (drawA > 1f)
                    drawA = 2f - drawA;

                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                Main.EntitySpriteDraw(FireBall, fireballPos2 + new Vector2(0f, 0f), null, Color.White with { A = 100 } * drawA, fireballRot, new Vector2((float)FireBall.Width / 2f, 10f), 0.3f + drawS * 0.5f, SpriteEffects.None);
            }

            //Main star 
            Main.EntitySpriteDraw(StarGlow, drawPos, null, Color.DeepPink with { A = 200 } * overallAlpha, projectile.rotation, StarGlow.Size() / 2f, drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 50 } * overallAlpha, projectile.rotation, Star.Size() / 2f, drawScale, SpriteEffects.None);

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
                Color col = Color.DeepPink;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);

                Vector2 trailScale = new Vector2(1.5f, 0.7f * drawScale * Easings.easeInOutSine(progress));

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, col with { A = 150 } * 1f * progress,
                       previousRotations[i], Trail.Size() / 2f, trailScale, SpriteEffects.None);
            }

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);

                Vector2 trailScale = new Vector2(1.5f, 0.7f * drawScale * Easings.easeInOutSine(progress));

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, Color.White with { A = 150 } * 0.85f * progress,
                    previousRotations[i], Trail.Size() / 2f, new Vector2(trailScale.X, trailScale.Y * 0.5f), SpriteEffects.None);
            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //return true;
            
            SoundStyle starPulse = new SoundStyle("VFXPlus/Sounds/Effects/Magic/Ding") with { Volume = 0.35f, Pitch = -.85f, PitchVariance = .2f, MaxInstances = -1 };
            SoundEngine.PlaySound(starPulse, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/star_impact_01") with { Volume = 0.25f, Pitch = 0.35f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            for (int i = 0; i < 9 + Main.rand.Next(2); i++)
            {
                Color col = Main.rand.NextBool(3) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 7f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.8f, 1.25f));
                d.velocity -= projectile.velocity * 0.45f;

                d.velocity *= 0.7f;


                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

            }


            for (int i = 0; i < 7 + Main.rand.Next(0, 5); i++)
            {
                Color col = Main.rand.NextBool(3) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col, Scale: Main.rand.NextFloat(0.65f, 0.75f));

                dust.velocity -= projectile.velocity * 0.1f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            return false;

            for (int i = 0; i < 9 + Main.rand.Next(3); i++)
            {
                Color col = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 5f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f));
            }

            return false;
            
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

            Texture2D Star = CommonTextures.CrispStarPMA.Value;
            //Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingB").Value;


            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            //OuterStar
            //float outerStarScale = GeneralUtilities.FadeLinear(effectProgress, 0.5f, 0.5f) * Projectile.scale * overallScale;
            //Vector2 outerDrawScale = new Vector2(outerStarScale, outerStarScale);
            //Main.EntitySpriteDraw(Star, drawPos, null, Color.HotPink with { A = 50 } * overallAlpha, Projectile.rotation, Star.Size() / 2f, outerDrawScale * 1.5f, SpriteEffects.None);

            //InnerStar
            //float innerStarScale = GeneralUtilities.FadeLinear(effectProgress, 0.25f, 0.75f) * Projectile.scale * overallScale;
            //Vector2 innerDrawScale = new Vector2(innerStarScale, innerStarScale);

            float starScale = MathF.Sin(MathHelper.Pi * effectProgress);

            //Texture2D glorb = CommonTextures.feather_circle128PMA.Value;
            //Main.EntitySpriteDraw(glorb, drawPos, null, Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f) with { A = 120 } * overallAlpha * 0.35f, Projectile.rotation, glorb.Size() / 2f, starScale * Projectile.scale * 0.75f, SpriteEffects.None);


            Main.EntitySpriteDraw(Star, drawPos, null, Color.DeepPink with { A = 120 } * overallAlpha, Projectile.rotation, Star.Size() / 2f, starScale * Projectile.scale, SpriteEffects.None);

            float innerStarScale = MathF.Pow(starScale, 4);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 120 } * overallAlpha, Projectile.rotation, Star.Size() / 2f, innerStarScale * 0.65f * Projectile.scale, SpriteEffects.None); //0.75


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
