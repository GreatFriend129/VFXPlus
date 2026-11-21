using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
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
using VFXPlus.Content.Weapons.Melee.PreHardmode.Swords;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class StarCannonItemOverride : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.StarCannon);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.StarCannon,
                    AnimTime: 15,
                    NormalXOffset: 16f,
                    DestXOffset: 3f,
                    YRecoilAmount: 0.13f,
                    HoldOffset: new Vector2(0f, 1f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_67") with { Volume = 0.2f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style5 = new SoundStyle("Terraria/Sounds/Research_2") with { Volume = 0.1f, Pitch = 1f, };
            SoundEngine.PlaySound(style5, player.Center);

            SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/starUIToss") with { Volume = .05f, Pitch = 0.7f, PitchVariance = 0.15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Volume = .15f, Pitch = .55f, PitchVariance = .1f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, player.Center);

            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.25f;

            //Dust d2 = Dust.NewDustPerfect(position + velNormalized * 3f, ModContent.DustType<CirclePulse>(), velNormalized * 3f, newColor: Color.OrangeRed);
            //CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            //b2.drawLayer = "Dusts";
            //d2.customData = b2;
            //d2.scale = circlePulseSize * 0.05f;

            return true;
        }
    }
    public class StarCannonShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.StarCannonStar);
        }

        public int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Color deepBlue = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.85f);
            Color golden = Color.Lerp(Color.Gold, Color.Orange, 0.75f);

            if (timer == 0 && Main.myPlayer == projectile.owner)
            {
                ConstellationTrail ConTrail = (ConstellationTrail)Projectile.NewProjectileDirect(null, projectile.Center, projectile.velocity, ModContent.ProjectileType<ConstellationTrail>(), 0, 0, projectile.owner, projectile.whoAmI).ModProjectile;
                ConTrail.starCol = deepBlue with { A = 160 };
                ConTrail.lineCol = deepBlue with { A = 175 };
                ConTrail.parentType = ProjectileID.StarCannonStar;
                ConTrail.timeBetweenStars = 6;
            }

            int trailCount = 10;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center); //40

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer % 3 == 0 && Main.rand.NextBool() && false && false)
            {
                Color dustCol = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DodgerBlue, Color.Blue, 0.5f);

                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f) + projectile.velocity * 0.02f;

                Dust d = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(10f, 10f), ModContent.DustType<PulseInOutDust>(), dustVel, newColor: dustCol with { A = 200 });
                d.scale *= Main.rand.NextFloat(1f, 1.5f) * 1f;

                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 14, 0.15f, 0.85f, Pixelize: true);
            }

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.06f), 0f, 1f);

            float fadeInTime = Math.Clamp((timer + 3f) / 20f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            projectile.soundDelay = 10;

            timer++;

            //So the constellation proj can look back to check if we are synced
            projectile.ai[2] = timer;

            return true;
        }

        float overallScale = 0f;
        float overallAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            Color thisBlue = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.65f);


            float yellowAmount = GeneralUtilities.OscSine(0f, 0.85f, 2f, 0f);// Easings.easeInOutQuad(GeneralUtilities.OscSine(0f, 1f, 1.35f, projectile.whoAmI * 0f));
            Color glorbCol = Color.Lerp(thisBlue, Color.Lerp(Color.Orange, Color.Gold, 0.5f), yellowAmount);
            Color fireballColor = Color.Lerp(thisBlue with { A = 120 }, Color.Lerp(Color.Orange, Color.Gold, 0.75f) with { A = 150 }, yellowAmount);
            Color starCol = Color.Lerp(thisBlue, Color.Orange, yellowAmount);

            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D StarGlow = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarGlow").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float drawScale = projectile.scale * overallScale;

            //Glorb
            Texture2D Glorb = CommonTextures.feather_circle128PMA.Value;
            Vector2 glorbPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -8f;
            Vector2 glorbScale = new Vector2(1f, 0.75f) * drawScale;
            Main.EntitySpriteDraw(Glorb, glorbPos + new Vector2(0f, 0f), null, glorbCol with { A = 200 } * 0.35f, projectile.velocity.ToRotation(), Glorb.Size() / 2f, glorbScale * 1.15f, SpriteEffects.None);


            //Intial Star
            if (overallScale < 1f && false)
            {
                Texture2D CrispStar = CommonTextures.CrispStarPMA.Value;

                Vector2 crispStarScale = new Vector2(1.5f, 1f) * projectile.scale * (1f - overallScale);
                Main.EntitySpriteDraw(CrispStar, drawPos + new Vector2(0f, 0f), null, Color.DodgerBlue with { A = 125 } * 1f, projectile.velocity.ToRotation(), CrispStar.Size() / 2f, crispStarScale * 1f, SpriteEffects.None);
                Main.EntitySpriteDraw(CrispStar, drawPos + new Vector2(0f, 0f), null, Color.White with { A = 125 } * 1f, projectile.velocity.ToRotation(), CrispStar.Size() / 2f, crispStarScale * 0.5f, SpriteEffects.None);
            }


            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;

            //Outer blue fireball
            Vector2 fireballPos = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -25f;
            for (int i = 0; i < 4; i++)
            {
                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = fireballPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(FireBall, offsetDrawPos, null, fireballColor * 0.35f, fireballRot, FireBall.Size() / 2f, drawScale * 1.05f, SpriteEffects.None); //175A
            }

            //Inner white fireball
            float time = (float)Main.timeForVisualEffects / 60f;
            for (float i = 0f; i < 1f; i += 0.5f)
            {
                float drawS = time % 0.5f / 0.5f;
                drawS = (drawS + i) % 1f;

                float drawA = drawS * 2f;
                if (drawA > 1f)
                    drawA = 2f - drawA;

                float fireballRot = projectile.velocity.ToRotation() + MathHelper.PiOver2;

                Main.EntitySpriteDraw(FireBall, drawPos, null, Color.White with { A = 100 } * drawA, fireballRot, new Vector2((float)FireBall.Width / 2f, 10f), drawScale * (0.3f + drawS * 0.5f), SpriteEffects.None);
            }


            //Main star
            Main.EntitySpriteDraw(StarGlow, drawPos, null, starCol with { A = 200 } * overallAlpha, projectile.rotation, StarGlow.Size() / 2f, drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, drawPos, null, Color.White with { A = 50 } * overallAlpha, projectile.rotation, Star.Size() / 2f, drawScale, SpriteEffects.None);


            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D Trail = CommonTextures.Flare.Value;

            float drawScale = projectile.scale * 1f * overallScale;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            float yellowAmount = GeneralUtilities.OscSine(0f, 0.85f, 2f, 0f);// Easings.easeInOutQuad(GeneralUtilities.OscSine(0f, 1f, 1.35f, projectile.whoAmI * 0f));
            Color trailCol = Color.Lerp(Color.Blue, Color.Lerp(Color.Orange, Color.Gold, 0.35f), yellowAmount);

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float colorProg = (progress * 4f) % 1f;
                Color col = Color.Lerp(Color.DodgerBlue, Color.Blue, 1f);

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);

                Vector2 trailScale = new Vector2(1.5f, 0.7f * drawScale * Easings.easeInOutSine(progress));

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, trailCol with { A = 150 } * 1f * progress,
                       previousRotations[i], Trail.Size() / 2f, trailScale, SpriteEffects.None);

                //Main.EntitySpriteDraw(Trail, AfterImagePos, null, Color.White with { A = 150 } * 0.85f * progress,
                //    previousRotations[i], Trail.Size() / 2f, new Vector2(trailScale.X, trailScale.Y * 0.5f), SpriteEffects.None);
            }


            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float colorProg = (progress * 4f) % 1f;
                Color col = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.65f);

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);

                Vector2 trailScale = new Vector2(1.5f, 0.7f * drawScale * Easings.easeInOutSine(progress));

                //Main.EntitySpriteDraw(Trail, AfterImagePos, null, col with { A = 150 } * 1f * progress,
                //       previousRotations[i], Trail.Size() / 2f, trailScale, SpriteEffects.None);

                Main.EntitySpriteDraw(Trail, AfterImagePos, null, Color.White with { A = 150 } * 0.85f * progress,
                    previousRotations[i], Trail.Size() / 2f, new Vector2(trailScale.X, trailScale.Y * 0.5f), SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.75f, Pitch = 0.15f, PitchVariance = 0.05f, MaxInstances = -1 }, projectile.position);


            int a = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<StarImpactVFX>(), 0, 0f, projectile.owner);
            (Main.projectile[a].ModProjectile as StarImpactVFX).impactType = StarImpactVFX.StarImpactType.StarCannon;

            /*
            //Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Gold, Scale: 0.25f);

            //da.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 20,
            //    overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            Color betweenBlue = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.35f);

            for (float num636 = 0f; num636 < 1f; num636 += 0.25f)  //0.125
            {
                Dust.NewDustPerfect(projectile.Center, DustID.FireworksRGB, Vector2.UnitY.RotatedBy(num636 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (4f + Main.rand.NextFloat() * 4f), 150, Color.Gold).noGravity = true;
            }
            for (float num637 = 0f; num637 < 1f; num637 += 0.35f) //0.25f
            {
                Dust.NewDustPerfect(projectile.Center, DustID.FireworksRGB, Vector2.UnitY.RotatedBy(num637 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (2f + Main.rand.NextFloat() * 3f), 150, betweenBlue, 0.85f).noGravity = true;
            }

            int crossCountA = 5; //10
            for (int i = 0; i < crossCountA; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCountA) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * 1.15f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color col = Main.rand.NextBool() ? Color.Orange : betweenBlue;

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

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3.5f, 7f) * 1.15f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color col = Main.rand.NextBool() ? Color.Orange : betweenBlue;


                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.55f));
                //gd.velocity -= projectile.oldVelocity * 0.2f;

                int timeBeforeSlow = Main.rand.Next(3, 7);
                float postSlowPower = Main.rand.NextFloat(0.88f, 0.92f);
                float fadePower = Main.rand.NextFloat(0.89f, 0.93f);

                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: timeBeforeSlow,
                    preSlowPower: 0.92f, postSlowPower: postSlowPower, velToBeginShrink: 1.5f, fadePower: fadePower, shouldFadeColor: false);
            }

            return false;
            */

            Dust da2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Gold, Scale: 0.2f);

            da2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 20,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 7 + Main.rand.Next(3); i++)
            {
                Color col = Main.rand.NextBool() ? Color.Gold : Color.DodgerBlue;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 7f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }

            #region vanillaKill
            Color newColor7 = Color.CornflowerBlue;
            if (Main.tenthAnniversaryWorld && (projectile.type == 12 || projectile.type == 955))
            {
                newColor7 = Color.HotPink;
                newColor7.A /= 2;
            }
            for (int num635 = 0; num635 < 7; num635++)
            {
                ////Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            }
            for (float num636 = 0f; num636 < 1f; num636 += 0.225f)  //0.125
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num636 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
            }
            for (float num637 = 0f; num637 < 1f; num637 += 0.35f) //0.25f
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num637 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
            }
            Vector2 vector54 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector54 / 2f, vector54 + new Vector2(400f))))
            {
                for (int num638 = 0; num638 < 4; num638++)
                {
                    Gore.NewGore(projectile.GetSource_FromThis(), projectile.position, Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * projectile.velocity.Length(), Utils.SelectRandom<int>(Main.rand, 16, 17, 17, 17, 17, 17, 17, 17));
                }
            }
            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5 + Main.rand.Next(3); i++)
            {
                Color col = Main.rand.NextBool() ? Color.Gold : Color.DodgerBlue;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }
        }
    }


}
