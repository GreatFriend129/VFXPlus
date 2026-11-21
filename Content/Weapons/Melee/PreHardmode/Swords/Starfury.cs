
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Projectiles;
using static tModPorter.ProgressUpdate;


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

            int a = Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<StarImpactVFX>(), 0, 0f, player.whoAmI);
            (Main.projectile[a].ModProjectile as StarImpactVFX).impactType = StarImpactVFX.StarImpactType.StarfuryMelee;

            int pgoCount = 3; //10
            for (int i = 0; i < pgoCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)pgoCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * 1f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color col = Main.rand.NextBool(4) ? Color.Orange : Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);

                Dust gd = Dust.NewDustPerfect(target.Center, ModContent.DustType<PixelGlowOrb>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.9f));
                //gd.velocity -= projectile.oldVelocity * 0.2f;

                int timeBeforeSlow = Main.rand.Next(3, 7);
                float postSlowPower = Main.rand.NextFloat(0.86f, 0.91f);
                float fadePower = Main.rand.NextFloat(0.92f, 0.96f);

                gd.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.2f, timeBeforeSlow: timeBeforeSlow,
                    preSlowPower: 0.95f, postSlowPower: postSlowPower, velToBeginShrink: 3f, fadePower: fadePower);

            }

            int crossCount = 4;
            for (int i = 0; i < crossCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(2.5f, 5f) * 1f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color col = Main.rand.NextBool(4) ? Color.Orange : Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);


                Dust gd = Dust.NewDustPerfect(target.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.55f));
                //gd.velocity -= projectile.oldVelocity * 0.2f;

                int timeBeforeSlow = Main.rand.Next(3, 7);
                float postSlowPower = Main.rand.NextFloat(0.88f, 0.92f);
                float fadePower = Main.rand.NextFloat(0.89f, 0.93f);

                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: timeBeforeSlow,
                    preSlowPower: 0.92f, postSlowPower: postSlowPower, velToBeginShrink: 1.5f, fadePower: fadePower, shouldFadeColor: false);
            }


            float randRot = Main.rand.NextFloat(6.28f);
            /*
            int pulse = Projectile.NewProjectile(null, target.Center, (randRot - MathHelper.PiOver4).ToRotationVector2() * 0.1f, ModContent.ProjectileType<StarfuryRadialPulse>(), 0, 0, Main.myPlayer);
            Main.projectile[pulse].scale = 0.75f;
            Main.projectile[pulse].rotation = randRot - MathHelper.PiOver4 * 1f;

            int pulse2 = Projectile.NewProjectile(null, target.Center, (randRot + MathHelper.PiOver4).ToRotationVector2() * 0.1f, ModContent.ProjectileType<StarfuryRadialPulse>(), 0, 0, Main.myPlayer);
            Main.projectile[pulse2].scale = 0.75f;
            Main.projectile[pulse2].rotation = randRot + MathHelper.PiOver4 * 1f;
            */
            Color newPink = Main.hslToRgb(0.92f, 1f, 0.6f) with { A = 50 };

            //int pulse = Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
            //(Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            //(Main.projectile[pulse].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            //Main.projectile[pulse].rotation = randRot - MathHelper.PiOver4 * 1f;

            //int pulse2 = Projectile.NewProjectile(null, target.Center, Vector2.Zero, ModContent.ProjectileType<PaintballGunPulseBIG>(), 0, 0, Main.myPlayer);
            //(Main.projectile[pulse2].ModProjectile as PaintballGunPulseBIG).color = newPink;// Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
            //Main.projectile[pulse2].rotation = randRot + MathHelper.PiOver4 * 1f;

            //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Magic/QuickShine") with { Volume = 0.35f, Pitch = -.25f, PitchVariance = .2f, MaxInstances = 1 };
            //SoundEngine.PlaySound(style, player.Center);

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
            if (timer == 0)
            {
                Vector2 pulseVel = projectile.velocity.SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(null, projectile.Center, pulseVel * 0.25f, ModContent.ProjectileType<StarfuryRadialPulse>(), 0, 0f, projectile.owner);
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
                DrawPixelatedStuff(projectile, true);
            });
            DrawPixelatedStuff(projectile, false);

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

            Texture2D Trail = CommonTextures.SoulSpikePMA.Value; //SoulSpike

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
                    previousRotations[i], Trail.Size() / 2f, new Vector2(trailScale.X, trailScale.Y * 0.35f), SpriteEffects.None);
            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle starPulse = new SoundStyle("VFXPlus/Sounds/Effects/Magic/Ding") with { Volume = 0.35f, Pitch = -.85f, PitchVariance = .2f, MaxInstances = -1 };
            SoundEngine.PlaySound(starPulse, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/star_impact_01") with { Volume = 0.25f, Pitch = 0.35f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);


            int a = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<StarImpactVFX>(), 0, 0f, projectile.owner);
            (Main.projectile[a].ModProjectile as StarImpactVFX).impactType = StarImpactVFX.StarImpactType.Starfury;

            Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepPink, Scale: 0.25f);

            da.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 20,
                overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            //Impact
            /*
            for (int i = 0; i < 12; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.35f, 0.65f));
                //dust.velocity += Projectile.velocity * 0.25f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 15; i++)
            {
                Color col = Main.rand.NextBool(3) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 9f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticleAlpha>(), vel, newColor: col with { A = 100 }, Scale: Main.rand.NextFloat(0.75f, 1.5f));
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

            }
            */

            for (float num636 = 220f; num636 < 1f; num636 += 0.25f)  //0.125
            {
                Dust.NewDustPerfect(projectile.Center, DustID.FireworksRGB, Vector2.UnitY.RotatedBy(num636 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (4f + Main.rand.NextFloat() * 8f), 150, Color.HotPink).noGravity = true;
            }
            for (float num637 = 220f; num637 < 1f; num637 += 0.35f) //0.25f
            {
                Dust.NewDustPerfect(projectile.Center, DustID.FireworksRGB, Vector2.UnitY.RotatedBy(num637 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (2f + Main.rand.NextFloat() * 6f), 150, Color.Gold, 0.85f).noGravity = true;
            }

            int crossCountA = 5; //10
            for (int i = 0; i < crossCountA; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCountA) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * 1.5f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color col = Main.rand.NextBool(4) ? Color.Orange : Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);

                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.55f) * 2f);
                //gd.velocity -= projectile.oldVelocity * 0.2f;

                int timeBeforeSlow = Main.rand.Next(3, 7);
                float postSlowPower = Main.rand.NextFloat(0.86f, 0.91f);
                float fadePower = Main.rand.NextFloat(0.92f, 0.96f);

                gd.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.2f, timeBeforeSlow: timeBeforeSlow,
                    preSlowPower: 0.95f, postSlowPower: postSlowPower, velToBeginShrink: 3f, fadePower: fadePower);

            }

            int crossCount = 7;
            for (int i = 0; i < crossCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3.5f, 7f) * 1.5f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color col = Main.rand.NextBool(4) ? Color.Orange : Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);


                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.55f));
                //gd.velocity -= projectile.oldVelocity * 0.2f;

                int timeBeforeSlow = Main.rand.Next(3, 7);
                float postSlowPower = Main.rand.NextFloat(0.88f, 0.92f);
                float fadePower = Main.rand.NextFloat(0.89f, 0.93f);

                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: timeBeforeSlow,
                    preSlowPower: 0.92f, postSlowPower: postSlowPower, velToBeginShrink: 1.5f, fadePower: fadePower, shouldFadeColor: false);
            }

            //farCross
            int farCrossCount = 3 + Main.rand.Next(3);
            for (int i = 0; i < farCrossCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)farCrossCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * 2f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color col = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PulseInOutDust>(), dustVel, 0, col with { A = 200 }, Main.rand.NextFloat(0.75f, 1f) * 0.65f);
                //d.rotation = MathHelper.PiOver4;

                int pulseTime = Main.rand.Next(35, 41);
                float velFadeAmount = Main.rand.NextFloat(0.93f, 0.95f);

                PulseInOutDustBehavior poudb = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, pulseTime, 0.15f, 0.35f, Pixelize: true);
                poudb.velFadeAmount = velFadeAmount;

                d.customData = poudb;
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            previousPositions.Clear();
            previousRotations.Clear();
            
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    public class StarfuryRadialPulse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }


        int timer = 0;

        float progress = 0f;
        public override void AI()
        {
            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);

            float timeForPulse = 22f; //18
            float myProg = Utils.GetLerpValue(0f, timeForPulse, (float)timer, true);

            progress = myProg;

            if (progress > 0.99f)
            {
                progress = 1f;
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawEffect(false);
            });

            DrawEffect(true);

            return false;
        }

        public void DrawEffect(bool giveUp = false)
        {
            if (giveUp)
                return;

            bool isSpecial = Projectile.ai[0] == 1;
            bool isGold = Projectile.ai[1] == 1;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;

            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                Color.DeepPink.ToVector3() * 0.6f,
                Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f).ToVector3() * 1f,
                Color.HotPink.ToVector3(),
                Color.White.ToVector3()
            };

            Vector3[] gradColsGold = {
                Color.Black.ToVector3(),
                Color.Orange.ToVector3() * 0.6f,
                Color.Lerp(Color.Orange, Color.Gold, 0.35f).ToVector3() * 1f,
                Color.Gold.ToVector3(),
                Color.White.ToVector3()
            };

            float thickness = 2f * Easings.easeOutQuad(progress);

            if (isSpecial || isGold)
                thickness = 2f * Easings.easeOutSine(progress);

            Effect thisEffect = ShaderHelper.SetRadialPulse2Params(
                causticTexture: Mod.Assets.Request<Texture2D>("Assets/Noise/Test/T_Random_54").Value, 
                causticTexture2: Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value, 
                uTime: (float)Main.timeForVisualEffects * 0.02f, 
                progress: Easings.easeOutSine(progress), 
                tex1Alpha: 0.8f, //0.8
                tex2Alpha: 0.55f, //0.55f
                rotation: Projectile.rotation + (float)Main.timeForVisualEffects * 0.1f,
                flowSpeed: 3f,
                zoom: 1f, 

                colors: isGold ? gradColsGold : gradCols, 
                finalColIntensity: 4f,
                posterizationSteps: 0f,
                totalAlpha: Easings.easeOutCubic(1f - progress),
                zFadeStrength: 1.1f,

                ringRadiusStart: 0f,
                ringThicknessStart: 2f * Easings.easeOutQuad(progress) * (isSpecial ? 0.85f : 1f),
                ringPower: 0.2f,
                ringMult: 2f,
                ringWaveSpeed: 0f,
                ringWaveStrength: 0f,
                ringWaveLength: 0f
                );
            
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, thisEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.velocity.ToRotation(), Tex.Size() / 2f, 155 * new Vector2(0.4f, 1f) * Projectile.scale, SpriteEffects.None, 0f); //160
            //Main.spriteBatch.Draw(Tex2, drawPos, null, Color.White, 0f, Tex2.Size() / 2f, 2f * new Vector2(1f, 1f), SpriteEffects.None, 0f); //160

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
    }

    public class StarImpactVFX : ModProjectile
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

        public enum StarImpactType
        {
            Starfury,
            StarfuryMelee,
            StarCannon, 
            SuperStarShooter
        }

        public StarImpactType impactType = StarImpactType.Starfury;
        public override void AI()
        {
            if (timer == 0)
            {
                //Star rotaion
                Projectile.ai[0] = Main.rand.NextFloat(6.28f);

                //Ring rotation
                Projectile.ai[1] = Main.rand.NextFloat(6.28f);

            }


            int timeForStarEffect = 14;
            starProgress = Utils.GetLerpValue(0, timeForStarEffect, timer, true);

            int timeForRingEffect = 22;
            ringProgress = Utils.GetLerpValue(0f, timeForRingEffect, timer, true);

            if (timer == timeForStarEffect && ringProgress > 0.99f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                //TODO: Fix zoom being fucked up if rings are being drawn
                if (impactType != StarImpactType.StarfuryMelee)
                    DrawRings(true);
                DrawStar(true);
            });
            if (impactType != StarImpactType.StarfuryMelee)
                DrawRings(false);

            DrawStar(false);

            return false;
        }

        float starProgress = 0f;
        public void DrawStar(bool giveUp)
        {
            if (giveUp)
                return;

            Color starColor = Color.DeepPink with { A = 120 };
            float totalStarScale = 1f;

            if (impactType == StarImpactType.Starfury)
            {
                starColor = Color.DeepPink with { A = 120 };
                totalStarScale = 2f;
            }
            else if (impactType == StarImpactType.StarfuryMelee)
            {
                starColor = Color.DeepPink with { A = 120 };
            }
            else if (impactType == StarImpactType.StarCannon)
            {
                Color deepBlue = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.85f);
                Color golden = Color.Lerp(Color.Gold, Color.Orange, 0.75f);
                float yellowValue = GeneralUtilities.OscSine(0f, 0.85f, 2f, 0f);
                starColor = Color.Lerp(deepBlue with { A = 160 }, golden with { A = 160 }, yellowValue);
                totalStarScale = 2f;
            }
            else if (impactType == StarImpactType.SuperStarShooter)
            {
                starColor = Color.Lerp(Color.Gold, Color.Orange, 0.45f);
                totalStarScale = 2f;
            }

            Texture2D Star = CommonTextures.CrispStarPMA.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float starScale = MathF.Sin(MathHelper.Pi * starProgress);
            Main.EntitySpriteDraw(Star, drawPos, null, starColor * overallAlpha, Projectile.ai[0], Star.Size() / 2f, starScale * Projectile.scale * totalStarScale, SpriteEffects.None);

            float innerStarScale = MathF.Pow(starScale, 4);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 120 } * overallAlpha, Projectile.ai[0], Star.Size() / 2f, innerStarScale * 0.65f * Projectile.scale * totalStarScale, SpriteEffects.None); //0.75
        }

        float ringProgress = 0f;
        public void DrawRings(bool giveUp)
        {
            if (giveUp)
                return;

            Vector3[] gradColsBlue = {
                Color.Black.ToVector3(),
                Color.Lerp(Color.Blue, Color.DodgerBlue, 0f).ToVector3() * 1.5f,
                Color.Lerp(Color.Blue, Color.DodgerBlue, 0.5f).ToVector3() * 1.25f,
                Color.Lerp(Color.Blue, Color.DodgerBlue, 0.85f).ToVector3() * 1f,
                Color.White.ToVector3()
            };
            
            Vector3[] gradColsGold = {
                Color.Black.ToVector3(),
                new Color(255, 140, 0).ToVector3() * 0.5f,
                Color.Lerp(Color.Orange, Color.Gold, 0.25f).ToVector3() * 0.75f,
                Color.Lerp(Color.Orange, Color.Gold, 0.75f).ToVector3(),
                Color.White.ToVector3()
            };

            Vector3[] gradCols = { Color.White.ToVector3() };
            float thickness = 1f;
            float texScale = 1f;

            if (impactType == StarImpactType.Starfury)
            {
                gradCols = new Vector3[] { 
                    Color.Black.ToVector3(),
                    Color.DeepPink.ToVector3() * 0.6f,
                    Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f).ToVector3() * 1f,
                    Color.HotPink.ToVector3(),                
                    Color.White.ToVector3()            
                };

                texScale = 0.85f;
                thickness = 0.85f;
            }
            else if (impactType == StarImpactType.StarCannon)
            {
                Color deepBlue = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.85f);
                Color golden = Color.Lerp(Color.Gold, Color.Orange, 0.75f);
                float yellowValue = 1f * GeneralUtilities.OscSine(0f, 1f, 2f, 0f);
                //starColor = Color.Lerp(deepBlue with { A = 160 }, golden with { A = 160 }, yellowValue);

                gradCols = new Vector3[] {
                    Vector3.Lerp(gradColsBlue[0], gradColsGold[0], yellowValue),
                    Vector3.Lerp(gradColsBlue[1], gradColsGold[1], yellowValue),
                    Vector3.Lerp(gradColsBlue[2], gradColsGold[2], yellowValue),
                    Vector3.Lerp(gradColsBlue[3], gradColsGold[3], yellowValue),
                    Vector3.Lerp(gradColsBlue[4], gradColsGold[4], yellowValue),
                };

                thickness = 0.85f;
            }
            else if (impactType == StarImpactType.SuperStarShooter)
            {
                gradCols = gradColsGold;
                thickness = 0.85f;
                texScale = 1f;
            }

            Effect thisEffect = ShaderHelper.SetRadialPulse2Params(
                causticTexture: Mod.Assets.Request<Texture2D>("Assets/Noise/Test/T_Random_54").Value,
                causticTexture2: Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value,
                uTime: (float)Main.timeForVisualEffects * 0.02f,
                progress: Easings.easeOutSine(ringProgress),
                tex1Alpha: 0.8f, //0.8
                tex2Alpha: 0.55f, //0.55f
                rotation: Projectile.rotation + (float)Main.timeForVisualEffects * 0.1f,
                flowSpeed: 3f,
                zoom: 1f,

                colors: gradCols,
                finalColIntensity: 4f,
                posterizationSteps: 0f,
                totalAlpha: Easings.easeOutCubic(1f - ringProgress),
                zFadeStrength: 1.1f,

                ringRadiusStart: 0f,
                ringThicknessStart: 2f * Easings.easeOutQuad(ringProgress) * thickness,
                ringPower: 0.2f,
                ringMult: 2f,
                ringWaveSpeed: 0f,
                ringWaveStrength: 0f,
                ringWaveLength: 0f
                );

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, thisEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.ai[1] + MathHelper.PiOver4, Tex.Size() / 2f, 155 * new Vector2(0.4f, 1f) * Projectile.scale * texScale, SpriteEffects.None, 0f); //160
            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.ai[1] - MathHelper.PiOver4, Tex.Size() / 2f, 155 * new Vector2(0.4f, 1f) * Projectile.scale * texScale, SpriteEffects.None, 0f); //160

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }

    }
}
