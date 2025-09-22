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
using System.Threading;
using VFXPlus.Common.Drawing;
using Terraria.Graphics;
using Terraria.Physics;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest.Aero;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Guns
{
    public class NailGun : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.NailGun);
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
                    GunID: ItemID.NailGun,
                    AnimTime: 16,
                    NormalXOffset: 18f,
                    DestXOffset: 6f,
                    YRecoilAmount: 0.15f,
                    HoldOffset: new Vector2(0f, 2f)
                    );
            }

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_108") with { Volume = 0.55f, PitchVariance = 0.1f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style2, player.Center);

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_ogre_spit") with { Volume = 0.6f, Pitch = .33f, PitchVariance = 0.1f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(style, player.Center);

            return true;
        }
    }


    public class NailGunShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NailFriendly);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            if (projectile.ai[0] == 0)
            {
                int trailCount = 20;
                previousRotations.Add(projectile.rotation);
                previousPositions.Add(projectile.Center + projectile.velocity);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);

                previousRotations.Add(projectile.rotation);
                previousPositions.Add(projectile.Center + projectile.velocity * 1.5f);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }
            else
            {
                if (!hasDonePulse)
                {
                    hasDonePulse = true;
                    pulsePower = 1.5f;
                }

                previousPositions.Clear();
                previousRotations.Clear();
            }

            pulsePower = Math.Clamp(MathHelper.Lerp(pulsePower, 0.5f, 0.1f), 1f, 2f);

            float fadeInTime = Math.Clamp((timer + 11f) / 18f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            float initPowerTime = Math.Clamp(timer / 30f, 0f, 1f);
            initialPower = 1f - Easings.easeOutCubic(initPowerTime);


            timer++;
            return true;
        }

        bool hasDonePulse = false;
        float pulsePower = 1f;
        float initialPower = 0f;

        float overallAlpha = 0f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Shakes before exploding
            if (projectile.ai[0] != 0f)
            {
                float shakeIntesity = Math.Clamp((projectile.ai[1] / 90f), 0f, 1f);
                drawPos += Main.rand.NextVector2Circular(4.5f, 4.5f) * Easings.easeInCubic(shakeIntesity);
            }

            if (initialPower > 0f)
            {
                Texture2D spike = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;
                Vector2 starScaleVec2 = new Vector2(1f, 0.5f) * projectile.scale * initialPower;

                Main.EntitySpriteDraw(spike, drawPos, null, Color.Red with { A = 0 } * initialPower, projectile.rotation + MathHelper.PiOver2, spike.Size() / 2f, starScaleVec2, SpriteEffects.None);
                Main.EntitySpriteDraw(spike, drawPos, null, Color.White with { A = 0 } * initialPower, projectile.rotation + MathHelper.PiOver2, spike.Size() / 2f, starScaleVec2 * 0.65f, SpriteEffects.None);
            }

            //Glorb
            Texture2D glorb = CommonTextures.feather_circle128PMA.Value;

            Vector2 glorbScale = new Vector2(1f, 0.75f) * overallScale * projectile.scale * 0.3f;
            Main.EntitySpriteDraw(glorb, drawPos + new Vector2(0f, 0f), null, Color.Red with { A = 0 } * 0.2f, projectile.rotation + MathHelper.PiOver2, glorb.Size() / 2f, glorbScale, SpriteEffects.None);


            //After Image
            for (int i = 0; i < previousPositions.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.Silver, Color.Red, 1f - progress);

                float size1 = Easings.easeOutQuad(progress) * overallScale * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.75f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);
            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3f, 3f), null,
                    Color.Crimson with { A = 0 } * 0.5f, projectile.rotation, TexOrigin, projectile.scale * overallScale * pulsePower, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale * pulsePower, SE);

            //Just Stick Overlay
            if (pulsePower > 1f)
                Main.EntitySpriteDraw(vanillaTex, drawPos, null, new Color(255, 150, 150) with { A = 0 } * (pulsePower - 1f) * 10f, projectile.rotation, TexOrigin, projectile.scale * overallScale * pulsePower, SE);


            //Star overlay (Easingsmaxing)
            if (projectile.ai[0] != 0f)
            {
                Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;

                float starPower = Utils.GetLerpValue(55, 90, projectile.ai[1], true);
                float starAlpha = Easings.easeOutQuart(starPower) * 1f;
                
                float spinInBonusRot = MathHelper.Lerp(12f * projectile.direction, 0f, Easings.easeOutCirc(starPower));
                
                float starScale = Easings.easeInOutBack(Easings.easeOutCubic(starPower), 0f, 3f) * projectile.scale * overallScale * 0.4f;

                float bonusStarScaleProgress = Utils.GetLerpValue(80, 90, projectile.ai[1], true);
                starScale += bonusStarScaleProgress * 0.5f;

                Main.EntitySpriteDraw(star, drawPos, null, Color.Red with { A = 0 } * starAlpha, projectile.rotation - spinInBonusRot, star.Size() / 2f, starScale, 0);
                Main.EntitySpriteDraw(star, drawPos, null, Color.White with { A = 0 } * starAlpha, projectile.rotation - spinInBonusRot, star.Size() / 2f, 0.65f * starScale, 0);
            }

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            #region Explosion
            //Spark Dust
            for (int i = 0; i < 3 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            //Smoke Dust
            for (int i = 0; i < 11; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.25f);

                float progress = (float)i / 10;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 3f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.6f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Color betweenOrange = Color.Lerp(Color.Orange, Color.OrangeRed, 0.55f);
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: betweenOrange, Scale: 0.2f);

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

            SoundEngine.PlaySound(SoundID.Item70 with { Pitch = 0.2f, Volume = 0.4f, MaxInstances = -1, PitchVariance = 0.35f }, projectile.Center);

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/hero_fury_charm_burst") with { Pitch = 0.6f, PitchVariance = 0.3f, MaxInstances = -1, Volume = 0.2f };
            SoundEngine.PlaySound(style2, projectile.Center);

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_explosive_trap_explode_1") with { PitchVariance = 0.25f, Pitch = 0.25f, Volume = 0.4f };
            SoundEngine.PlaySound(style3, projectile.Center);


            #region vanillaKill
            //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            for (int num736 = 220; num736 < 10; num736++)
            {
                int num737 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.3f);
                Dust dust142 = Main.dust[num737];
                Dust dust334 = dust142;
                dust334.velocity *= 1.4f;
            }
            for (int num738 = 220; num738 < 6; num738++)
            {
                int num739 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2.1f);
                Main.dust[num739].noGravity = true;
                Dust dust143 = Main.dust[num739];
                Dust dust334 = dust143;
                dust334.velocity *= 4.6f;
                num739 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.3f);
                dust143 = Main.dust[num739];
                dust334 = dust143;
                dust334.velocity *= 3.3f;
                if (Main.rand.Next(2) == 0)
                {
                    num739 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.1f);
                    dust143 = Main.dust[num739];
                    dust334 = dust143;
                    dust334.velocity *= 2.7f;
                }
            }
            if (projectile.owner == Main.myPlayer)
            {
                projectile.penetrate = -1;
                projectile.position.X += projectile.width / 2;
                projectile.position.Y += projectile.height / 2;
                projectile.width = 112;
                projectile.height = 112;
                projectile.position.X -= projectile.width / 2;
                projectile.position.Y -= projectile.height / 2;
                projectile.ai[0] = 2f;
                projectile.Damage();
            }
            #endregion
            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = projectile.oldVelocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.25f) * -Main.rand.NextFloat(1f, 2.5f);
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: Color.Crimson, Scale: Main.rand.NextFloat(0.35f, 0.5f));

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

}
