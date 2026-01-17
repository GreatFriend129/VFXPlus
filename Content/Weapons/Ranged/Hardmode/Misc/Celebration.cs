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


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    public class Celebration : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.FireworksLauncher);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicGunProjMiddle>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicGunProjMiddle held)
            {
                held.SetProjInfo(
                    GunID: ItemID.FireworksLauncher,
                    AnimTime: 20,
                    NormalXOffset: 16f,
                    DestXOffset: 0f,
                    YRecoilAmount: 0.35f,
                    HoldOffset: new Vector2(0f, 0f),
                    TipPos: new Vector2(26f, -1f),
                    StarPos: new Vector2(14f, -1f)
                    );

                held.timeToStartFade = 1;
                held.isShotgun = true;
            }

            //Explosion
            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(28f, -2f * dir).RotatedBy(velocity.ToRotation()) + new Vector2(0f, 1f); //4448

            for (int i = 0; i < 11; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);

                float progress = (float)i / 10;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.45f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(10, 24), 0.98f, 0.01f, 0.75f);

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 1f;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.14f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++)
            {
                int index = Main.rand.Next(0, 4);
                Color[] cols = { Color.Red, Color.DodgerBlue, Color.Green, Color.Goldenrod };

                Color col1 = cols[index];

                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.3f, 0.6f) * 1.5f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.9f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 5f;
            }

            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.25f;

            Dust d2 = Dust.NewDustPerfect(position + velNormalized * 3f, ModContent.DustType<CirclePulse>(), velNormalized * 3f, newColor: Color.OrangeRed);
            CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            b2.drawLayer = "Dusts";
            d2.customData = b2;
            d2.scale = circlePulseSize * 0.05f;

            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Volume = 0.3f, PitchVariance = 0.08f, Pitch = 0.5f, MaxInstances = 1 }, player.Center);
            return true;
        }
    }
    public class CelebrationRedRocketOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RocketFireworkRed);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 28; //24
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            //Trailing Fire Dust
            if (timer % 3 == 0 && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.3f;

                Color dustCol = Color.Lerp(Color.Red, Color.OrangeRed, 0.3f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 0.6f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 1f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if ((timer + 1) % 3 == 0 && false)
            {
                int num4 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, projectile.velocity.X * -0.4f, projectile.velocity.Y * -0.4f, 100, default(Color), 1.2f);
                Main.dust[num4].noGravity = true;
                Main.dust[num4].velocity.X *= 4f;
                Main.dust[num4].velocity.Y *= 4f;
                Main.dust[num4].velocity = -(Main.dust[num4].velocity + projectile.velocity) / 2f;
            }


            if (timer % 1 == 0 && timer > 5)
            {
                Color between = Color.Lerp(Color.Red, Color.OrangeRed, 0.25f);


                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f, 
                    newColor: between with { A = 55 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
                d.velocity += projectile.velocity * -0.1f;

                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d2.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f;
            }

            float fadeInTime = Math.Clamp((timer + 6f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            timer++;

            #region vanillaAI
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;
            #endregion
            return false;
        }


        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {   
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float scale = projectile.scale * overallScale;

            DrawTrail(projectile, false);

            //Bloomball
            Texture2D Ball = CommonTextures.FireBallBlur.Value;

            Vector2 ballOff1 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f + new Vector2(0f, 0f);
            Vector2 ballOff2 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -10f + new Vector2(0f, -100f);

            Main.EntitySpriteDraw(Ball, ballOff1, null, Color.Red with { A = 0 } * 0.35f, projectile.rotation, Ball.Size() / 2f, new Vector2(0.7f, 0.7f) * projectile.scale * overallScale, SE);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.OrangeRed with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D flare = CommonTextures.Flare.Value;
            float scale = projectile.scale * overallScale;

            Color col = Color.Lerp(Color.OrangeRed, Color.Red, 0.75f);


            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color trailCol = Color.Lerp(col, Color.DarkRed, 1f - progress) * progress;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(6f, 6f) * progress;

                Vector2 flareScale = new Vector2(1f, 0.55f * progress) * scale;

                Main.EntitySpriteDraw(flare, AfterImagePos, null, trailCol with { A = 0 } * progress * 0.8f,
                    previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, flareScale, 0);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            #region vanillaKill
            if (projectile.owner != Main.myPlayer)
            {
                projectile.timeLeft = 60;
            }
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            if (projectile.type == 167)
            {
                for (int num760 = 0; num760 < 300; num760++) //400 300 200 100 -> 300 225 150 75
                {
                    float num761 = 16f;
                    if (num760 < 225)
                    {
                        num761 = 12f;
                    }
                    if (num760 < 150)
                    {
                        num761 = 8f;
                    }
                    if (num760 < 75)
                    {
                        num761 = 4f;
                    }
                    int num763 = ModContent.DustType<PixelGlowOrb>();
                    int num764 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), 6, 6, num763, 0f, 0f, 100, newColor: Color.Red);

                    if (Main.rand.NextBool())
                        Main.dust[num764].noLight = false;

                    float num765 = Main.dust[num764].velocity.X;
                    float y2 = Main.dust[num764].velocity.Y;
                    if (num765 == 0f && y2 == 0f)
                    {
                        num765 = 1f;
                    }
                    float num766 = (float)Math.Sqrt(num765 * num765 + y2 * y2);
                    num766 = num761 / num766;
                    num765 *= num766;
                    y2 *= num766;
                    Dust dust135 = Main.dust[num764];
                    Dust dust334 = dust135;
                    dust334.velocity *= 0.5f;
                    Main.dust[num764].velocity.X += num765;
                    Main.dust[num764].velocity.Y += y2;
                    Main.dust[num764].scale = 1.3f * 0.65f;
                    Main.dust[num764].noGravity = true;
                }
            }
            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 192;
            projectile.height = 192;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            projectile.penetrate = -1;
            projectile.Damage();

            #endregion

            return false;
        }
    }

    public class CelebrationBlueRocketOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RocketFireworkBlue);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 28;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            //Trailing Fire Dust
            if (timer % 3 == 0 && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.3f;

                Color dustCol = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.3f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 0.6f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 1f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if (timer % 1 == 0 && timer > 5)
            {
                Color between = Color.DeepSkyBlue;//Color.Lerp(Color.Red, Color.OrangeRed, 0.25f);


                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 55 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
                d.velocity += projectile.velocity * -0.1f;

                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d2.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f;
            }

            float fadeInTime = Math.Clamp((timer + 6f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            timer++;

            #region vanillaAI
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;
            #endregion
            return false;
        }


        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawTrail(projectile, false);

            //Bloomball
            Texture2D Ball = CommonTextures.FireBallBlur.Value;

            Vector2 ballOff1 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f + new Vector2(0f, 0f);

            Main.EntitySpriteDraw(Ball, ballOff1, null, Color.DodgerBlue with { A = 0 } * 0.35f, projectile.rotation, Ball.Size() / 2f, new Vector2(0.7f, 0.7f) * projectile.scale * overallScale, SE);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.DeepSkyBlue with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D flare = CommonTextures.Flare.Value;
            float scale = projectile.scale * overallScale;

            Color col = Color.DodgerBlue;// Color.Lerp(Color.OrangeRed, Color.Red, 0.75f);


            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color trailCol = Color.Lerp(col, Color.Blue, 1f - progress) * progress;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(6f, 6f) * progress;

                Vector2 flareScale = new Vector2(1f, 0.55f * progress) * scale;

                Main.EntitySpriteDraw(flare, AfterImagePos, null, trailCol with { A = 0 } * progress * 0.8f,
                    previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, flareScale, 0);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            #region vanillaKill
            if (projectile.owner != Main.myPlayer)
            {
                projectile.timeLeft = 60;
            }
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            if (projectile.type == 169)
            {
                Vector2 vector59 = ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2();
                float num774 = Main.rand.Next(5, 9);
                float num775 = Main.rand.Next(12, 17);
                float value13 = Main.rand.Next(3, 7);
                float num776 = 20f;
                for (float num777 = 0f; num777 < num774; num777++)
                {
                    for (int num778 = 0; num778 < 2; num778++)
                    {
                        Vector2 value2 = vector59.RotatedBy(((num778 == 0) ? 1f : (-1f)) * ((float)Math.PI * 2f) / (num774 * 2f));
                        for (float num779 = 0f; num779 < num776; num779++)
                        {
                            Vector2 vector60 = Vector2.Lerp(vector59, value2, num779 / num776);
                            float num780 = MathHelper.Lerp(num775, value13, num779 / num776);
                            
                            
                            int num781 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), 6, 6, ModContent.DustType<PixelGlowOrb>(), 0f, 0f, 100, 
                                Color.Goldenrod, 1.3f * 0.65f);
                            
                            if (Main.rand.NextBool())
                                Main.dust[num781].noLight = false;

                            if (Main.rand.NextBool(3))
                                Main.dust[num781].active = false;

                            Dust dust128 = Main.dust[num781];
                            Dust dust334 = dust128;
                            dust334.velocity *= 0.1f;
                            Main.dust[num781].noGravity = true;
                            dust128 = Main.dust[num781];
                            dust334 = dust128;
                            dust334.velocity += vector60 * num780;
                        }
                    }
                    vector59 = vector59.RotatedBy((float)Math.PI * 2f / num774);
                }
                for (float num782 = 0f; num782 < num774; num782++)
                {
                    for (int num783 = 0; num783 < 2; num783++)
                    {
                        Vector2 value3 = vector59.RotatedBy(((num783 == 0) ? 1f : (-1f)) * ((float)Math.PI * 2f) / (num774 * 2f));
                        for (float num785 = 0f; num785 < num776; num785++)
                        {
                            Vector2 vector61 = Vector2.Lerp(vector59, value3, num785 / num776);
                            float num786 = MathHelper.Lerp(num775, value13, num785 / num776) / 2f;
                            
                            int num787 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), 6, 6, ModContent.DustType<PixelGlowOrb>(), 0f, 0f, 100, 
                                Color.Goldenrod, 1.3f * 0.65f);

                            if (Main.rand.NextBool())
                                Main.dust[num787].noLight = false;

                            if (Main.rand.NextBool(3))
                                Main.dust[num787].active = false;

                            Dust dust130 = Main.dust[num787];
                            Dust dust334 = dust130;
                            dust334.velocity *= 0.1f;
                            Main.dust[num787].noGravity = true;
                            dust130 = Main.dust[num787];
                            dust334 = dust130;
                            dust334.velocity += vector61 * num786;
                        }
                    }
                    vector59 = vector59.RotatedBy((float)Math.PI * 2f / num774);
                }
                for (int num788 = 0; num788 < 100; num788++)
                {
                    float num789 = num775;
                    int num790 = ModContent.DustType<PixelGlowOrb>();
                    int num791 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), 6, 6, num790, 0f, 0f, 100, newColor: Color.DodgerBlue, Scale: 0.65f);

                    if (Main.rand.NextBool())
                        Main.dust[num791].noLight = false;

                    float num792 = Main.dust[num791].velocity.X;
                    float y4 = Main.dust[num791].velocity.Y;
                    if (num792 == 0f && y4 == 0f)
                    {
                        num792 = 1f;
                    }
                    float num793 = (float)Math.Sqrt(num792 * num792 + y4 * y4);
                    num793 = num789 / num793;
                    num792 *= num793;
                    y4 *= num793;
                    Dust dust133 = Main.dust[num791];
                    Dust dust334 = dust133;
                    dust334.velocity *= 0.5f;
                    Main.dust[num791].velocity.X += num792;
                    Main.dust[num791].velocity.Y += y4;
                    Main.dust[num791].scale = 1.3f * 0.65f;
                    Main.dust[num791].noGravity = true;
                }
            }

            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 192;
            projectile.height = 192;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            projectile.penetrate = -1;
            projectile.Damage();

            #endregion

            return false;
        }
    }

    public class CelebrationGreenRocketOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RocketFireworkGreen);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 28; //24
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            //Trailing Fire Dust
            if (timer % 3 == 0 && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.3f;

                Color dustCol = Color.Green;// Color.Lerp(Color.Red, Color.OrangeRed, 0.3f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 0.6f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 1f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if (timer % 1 == 0 && timer > 5)
            {
                Color between = Color.ForestGreen;// Color.Lerp(Color.Red, Color.OrangeRed, 0.25f);


                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 55 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
                d.velocity += projectile.velocity * -0.1f;

                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d2.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f;
            }

            float fadeInTime = Math.Clamp((timer + 6f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            timer++;

            #region vanillaAI
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;
            #endregion
            return false;
        }


        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawTrail(projectile, false);

            //Bloomball
            Texture2D Ball = CommonTextures.FireBallBlur.Value;

            Vector2 ballOff1 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f + new Vector2(0f, 0f);

            Main.EntitySpriteDraw(Ball, ballOff1, null, Color.LawnGreen with { A = 0 } * 0.35f, projectile.rotation, Ball.Size() / 2f, new Vector2(0.7f, 0.7f) * projectile.scale * overallScale, SE);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.Green with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D flare = CommonTextures.Flare.Value;
            float scale = projectile.scale * overallScale;

            Color col = Color.Green;// Color.Lerp(Color.OrangeRed, Color.Red, 0.75f);


            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color trailCol = Color.Lerp(col, Color.ForestGreen, 1f - progress) * progress;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(6f, 6f) * progress;

                Vector2 flareScale = new Vector2(1f, 0.55f * progress) * scale;

                Main.EntitySpriteDraw(flare, AfterImagePos, null, trailCol with { A = 0 } * progress * 0.8f,
                    previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, flareScale, 0);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            #region vanillaKill
            if (projectile.owner != Main.myPlayer)
            {
                projectile.timeLeft = 60;
            }
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            if (projectile.type == 168)
            {
                for (int num767 = 0; num767 < 400; num767++)
                {
                    float num768 = 2f * ((float)num767 / 100f);
                    if (num767 > 100)
                    {
                        num768 = 10f;
                    }
                    if (num767 > 250)
                    {
                        num768 = 13f;
                    }

                    int num769 = ModContent.DustType<PixelGlowOrb>(); //131
                    int num770 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), 6, 6, num769, 0f, 0f, 100, newColor: Color.Green, Scale: 0.65f);

                    if (Main.rand.NextBool())
                        Main.dust[num770].noLight = false;

                    if (Main.rand.NextBool(2))
                        Main.dust[num770].active = false;

                    float num771 = Main.dust[num770].velocity.X;
                    float y3 = Main.dust[num770].velocity.Y;
                    if (num771 == 0f && y3 == 0f)
                    {
                        num771 = 1f;
                    }
                    float num772 = (float)Math.Sqrt(num771 * num771 + y3 * y3);
                    num772 = num768 / num772;
                    if (num767 <= 200)
                    {
                        num771 *= num772;
                        y3 *= num772;
                    }
                    else
                    {
                        num771 = num771 * num772 * 1.25f;
                        y3 = y3 * num772 * 0.75f;
                    }
                    Dust dust134 = Main.dust[num770];
                    Dust dust334 = dust134;
                    dust334.velocity *= 0.5f; //0.5
                    Main.dust[num770].velocity.X += num771;
                    Main.dust[num770].velocity.Y += y3;
                    if (num767 > 100)
                    {
                        Main.dust[num770].scale = 1.3f * 0.65f;
                        Main.dust[num770].noGravity = true;
                    }
                    else
                    {
                        Main.dust[num770].velocity *= 2;
                    }
                }
            }


            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 192;
            projectile.height = 192;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            projectile.penetrate = -1;
            projectile.Damage();

            #endregion

            return false;
        }
    }

    public class CelebrationYellowRocketOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RocketFireworkYellow);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 28; //24
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            //Trailing Fire Dust
            if (timer % 3 == 0 && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.3f;

                Color dustCol = Color.Lerp(Color.Yellow, Color.Orange, 0.3f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 0.6f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 1f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if (timer % 1 == 0 && timer > 5)
            {
                Color between = Color.Lerp(Color.Yellow, Color.Orange, 0.25f);


                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 55 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
                d.velocity += projectile.velocity * -0.1f;

                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d2.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f;
            }

            float fadeInTime = Math.Clamp((timer + 6f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            timer++;

            #region vanillaAI
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;
            #endregion
            return false;
        }


        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawTrail(projectile, false);

            //Bloomball
            Texture2D Ball = CommonTextures.FireBallBlur.Value;

            Vector2 ballOff1 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f + new Vector2(0f, 0f);

            Main.EntitySpriteDraw(Ball, ballOff1, null, Color.Gold with { A = 0 } * 0.35f, projectile.rotation, Ball.Size() / 2f, new Vector2(0.7f, 0.7f) * projectile.scale * overallScale, SE);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.Orange with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D flare = CommonTextures.Flare.Value;
            float scale = projectile.scale * overallScale;

            Color col = Color.Lerp(Color.Yellow, Color.Orange, 0.25f);

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color trailCol = Color.Lerp(col, Color.DarkGoldenrod, 1f - progress) * progress;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(6f, 6f) * progress;

                Vector2 flareScale = new Vector2(1f, 0.55f * progress) * scale;

                Main.EntitySpriteDraw(flare, AfterImagePos, null, trailCol with { A = 0 } * progress * 0.8f,
                    previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, flareScale, 0);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            #region vanillaKill
            if (projectile.owner != Main.myPlayer)
            {
                projectile.timeLeft = 60;
            }
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            if (projectile.type == 170)
            {
                for (int num794 = 0; num794 < 400; num794++)
                {
                    int num796 = 133;
                    float num797 = 16f;
                    if (num794 > 100)
                    {
                        num797 = 11f;
                    }
                    if (num794 > 100)
                    {
                        num796 = 134;
                    }
                    if (num794 > 200)
                    {
                        num797 = 8f;
                    }
                    if (num794 > 200)
                    {
                        num796 = 133;
                    }
                    if (num794 > 300)
                    {
                        num797 = 5f;
                    }
                    if (num794 > 300)
                    {
                        num796 = 134;
                    }

                    Color col = num796 == 133 ? Color.Goldenrod : Color.DeepPink;

                    int num798 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), 6, 6, ModContent.DustType<PixelGlowOrb>(), 0f, 0f, 100, newColor: col, Scale: 0.65f);

                    if (Main.rand.NextBool())
                        Main.dust[num798].noLight = false;

                    if (Main.rand.NextBool(2))
                        Main.dust[num798].active = false;

                    float num799 = Main.dust[num798].velocity.X;
                    float y5 = Main.dust[num798].velocity.Y;
                    if (num799 == 0f && y5 == 0f)
                    {
                        num799 = 1f;
                    }
                    float num800 = (float)Math.Sqrt(num799 * num799 + y5 * y5);
                    num800 = num797 / num800;
                    if (num794 > 300)
                    {
                        num799 = num799 * num800 * 0.7f;
                        y5 *= num800;
                    }
                    else if (num794 > 200)
                    {
                        num799 *= num800;
                        y5 = y5 * num800 * 0.7f;
                    }
                    else if (num794 > 100)
                    {
                        num799 = num799 * num800 * 0.7f;
                        y5 *= num800;
                    }
                    else
                    {
                        num799 *= num800;
                        y5 = y5 * num800 * 0.7f;
                    }
                    Dust dust127 = Main.dust[num798];
                    Dust dust334 = dust127;
                    dust334.velocity *= 0.5f;
                    Main.dust[num798].velocity.X += num799;
                    Main.dust[num798].velocity.Y += y5;
                    if (Main.rand.Next(3) != 0)
                    {
                        Main.dust[num798].scale = 1.3f * 0.65f;
                        Main.dust[num798].noGravity = true;
                    }
                }
            }

            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 192;
            projectile.height = 192;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            projectile.penetrate = -1;
            projectile.Damage();
            #endregion

            return false;
        }
    }

}
