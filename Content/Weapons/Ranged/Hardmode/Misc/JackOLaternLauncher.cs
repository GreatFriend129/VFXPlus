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
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest.Aero;
using VFXPLus.Common;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    
    public class JackOLaternLauncherOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.JackOLanternLauncher);
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicGunProjMiddle>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicGunProjMiddle held)
            {
                held.SetProjInfo(
                    GunID: ItemID.JackOLanternLauncher,
                    AnimTime: 20,
                    NormalXOffset: 16f,
                    DestXOffset: -6f,
                    YRecoilAmount: 0.45f,
                    HoldOffset: new Vector2(0f, 1f),
                    TipPos: new Vector2(34f, -1f),
                    StarPos: new Vector2(22f, -1f)
                    );

                held.timeToStartFade = 1;
                held.isShotgun = true;
            }

            //Explosion
            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(34f, -2f * dir).RotatedBy(velocity.ToRotation()) + new Vector2(0f, 1f); //4448

            for (int i = 0; i < 11; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);

                float progress = (float)i / 10;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.55f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(10, 24), 0.98f, 0.01f, 0.75f);

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 1f;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.14f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.2f);

                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.3f, 0.6f) * 1.5f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.9f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 5f;
            }

            //Circle pulse
            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.2f;

            Dust d2 = Dust.NewDustPerfect(position + velNormalized * 20f, ModContent.DustType<CirclePulse>(), velNormalized * 3.25f, newColor: Color.OrangeRed);
            CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            b2.drawLayer = "Dusts";
            d2.customData = b2;
            d2.scale = circlePulseSize * 0.15f;

            //Sound
            //SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Volume = 0.4f, PitchVariance = 0.08f, Pitch = 0.5f, MaxInstances = 1 }, player.Center);

            //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Fire/FlareImpact") with { Volume = 0.15f, Pitch = -0.8f, MaxInstances = 1 };
            //SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/rescue_ranger_fire") with { Volume = .025f, Pitch = .65f, PitchVariance = .05f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Fire/FlareImpact") with { Volume = 0.2f, Pitch = -0.8f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_61") with { Volume = 0.5f, Pitch = 0.05f, PitchVariance = .12f, };
            SoundEngine.PlaySound(style4, player.Center);

            return true;
        }

    }
    public class JackOLaternLauncherShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.JackOLantern);
        }
        public override bool InstancePerEntity => true;

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                drawRotation = Main.rand.NextFloat(6.28f);

            previousPositions.Add(projectile.Center);
            previousRotations.Add(drawRotation);
            previousVelrots.Add(projectile.velocity.ToRotation());

            int trailCount = 12;
            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
                previousVelrots.RemoveAt(0);
            }

            #region SquashAndStretch
            float firstStretchX = 1.6f;
            float firstStretchY = 0.65f;

            float secondStretchX = 0.75f;
            float secondStretchY = 1.3f;

            if (scaleState == 0)
                drawRotation += 0.3f * projectile.direction;
            else if (scaleState == 1)
            {
                Xscale = MathHelper.Lerp(Xscale, firstStretchX * bounceIntensity, 0.3f * bounceIntensity);
                Yscale = MathHelper.Lerp(Yscale, firstStretchY, 0.3f * bounceIntensity);

                if (Xscale >= 1.55f * bounceIntensity)
                    scaleState = 2;
            }
            else if (scaleState == 2)
            {
                Xscale = MathHelper.Lerp(Xscale, secondStretchX, 0.3f * bounceIntensity);
                Yscale = MathHelper.Lerp(Yscale, secondStretchY * bounceIntensity, 0.3f * bounceIntensity);

                if (Yscale >= 1.25f * bounceIntensity)
                    scaleState = 3;
            }
            else
            {
                Xscale = MathHelper.Lerp(Xscale, 1f, 0.4f);
                Yscale = MathHelper.Lerp(Yscale, 1f, 0.4f);

                if (Math.Abs(Xscale - 1f) < 0.05f)
                {
                    Xscale = 1f;
                    Yscale = 1f;
                    scaleState = 0;
                }
                
            }

            #endregion

            float fadeInTime = Math.Clamp(timer / 22f, 0f, 1f);
            overallScale = Easings.easeOutCubic(fadeInTime);

            timer++;
            return base.PreAI(projectile);
        }

        float Xscale = 1f;
        float Yscale = 1f;
        int scaleState = 0;
        float bounceIntensity = 1f;

        float drawRotation = 0f;

        float overallScale = 1f;
        float overallAlpha = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public List<float> previousVelrots = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D GlowMask = TextureAssets.GlowMask[257].Value;


            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            float totalAlpha = projectile.Opacity * overallAlpha;

            Vector2 mainScale = new Vector2(Xscale, Yscale) * overallScale * projectile.scale;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawPixelTrail(projectile);
            });

            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition;
                float trailAlpha = Easings.easeInCirc(progress) * totalAlpha;

                float trailScale = 1f * progress * overallScale;

                Main.EntitySpriteDraw(vanillaTex, trailPos, null, Color.Orange with { A = 0 } * trailAlpha * 0.6f, previousRotations[i], TexOrigin, trailScale, SE);
            }

            //Border
            for (int i = 0; i < 5; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, null, Color.Orange with { A = 0 } * totalAlpha * 0.8f, drawRotation, TexOrigin, mainScale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * totalAlpha, drawRotation, TexOrigin, mainScale, SE);

            Main.EntitySpriteDraw(GlowMask, drawPos + Main.rand.NextVector2Circular(2f, 2f), null, Color.White with { A = 0 } * totalAlpha, drawRotation, TexOrigin, mainScale, SE);

            //Glorb
            #region drawGlorb
            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;

            Color orbCol2 = Color.Lerp(Color.Orange, Color.OrangeRed, 0.25f) * 0.375f * overallAlpha;
            Color orbCol3 = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f) * 0.525f * overallAlpha;

            float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.5f) * 0.2f;
            float sineScale2 = MathF.Cos((float)Main.timeForVisualEffects * 0.22f + 0.32578f) * 0.15f;

            float scale2 = 1.8f + sineScale2;
            float scale3 = 2.4f + sineScale + 0.5f;

            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol2 with { A = 0 } * 0.75f, projectile.velocity.ToRotation(), Glow.Size() / 2f, mainScale * scale2 * 0.35f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol3 with { A = 0 } * 0.75f, projectile.velocity.ToRotation(), Glow.Size() / 2f, mainScale * scale3 * 0.15f, SpriteEffects.None);

            #endregion

            return false;

        }

        public void DrawPixelTrail(Projectile projectile)
        {
            Texture2D AfterImage = CommonTextures.Flare.Value;

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                if (progress == 1)
                    Main.NewText("PROG=1");

                if (progress != 1)
                {
                    Color orangeToUse = Color.Lerp(Color.Orange, Color.OrangeRed, 0.15f);

                    Color orangeToUseA = Color.Lerp(Color.Orange, Color.OrangeRed, 0.15f);
                    Color orangeToUseB = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);


                    Color col = Color.Lerp(orangeToUseB, orangeToUseA, Easings.easeInCirc(progress));

                    float size2 = Easings.easeInSine(progress) * projectile.scale * 1.5f;

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f * progress, 3f);

                    Vector2 newVec2 = new Vector2(0.75f, 1f * size2 * 1.35f) * overallScale;
                    Vector2 newVec22 = new Vector2(0.75f, 0.26f * size2 * 1.35f) * overallScale;

                    Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.Black * 0.35f * progress,
                           previousVelrots[i], AfterImage.Size() / 2f, newVec2 * 0.6f, SpriteEffects.None);

                    Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, col with { A = 0 } * 0.5f,
                           previousVelrots[i], AfterImage.Size() / 2f, newVec2 * 1f, SpriteEffects.None);

                    Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.White with { A = 0 } * 0.35f * progress,
                           previousVelrots[i], AfterImage.Size() / 2f, newVec22 * 1f, SpriteEffects.None);
                }

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int soundID = Main.rand.NextBool() ? 1 : 2;

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Custom/dd2_kobold_flyer_hurt_" + soundID) with { Volume = 0.75f, Pitch = -.45f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);
            SoundEngine.PlaySound(style2, projectile.Center);

            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.35f, Pitch = -0.2f, PitchVariance = 0.1f, MaxInstances = -1 }, projectile.Center);

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_45") with { Volume = 0.5f, Pitch = -.55f, MaxInstances = -1 };
            SoundEngine.PlaySound(style3, projectile.Center);

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_62") with { Volume = 0.4f, PitchVariance = .15f, };
            SoundEngine.PlaySound(style4, projectile.Center);

            //Dust explo
            #region dust
            Vector2 pos = projectile.Center;

            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 9; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.6f);

                float progress = (float)i / 9;
                Color col = Color.Lerp(Color.Brown * 0.35f, col1 with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.75f) * 2.45f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed * 1.35f, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int fg = 0; fg < 11; fg++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Dust gd = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.5f) * 1.5f, newColor: new Color(255, 125, 5), Scale: Main.rand.NextFloat(1f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.45f);

                Vector2 smvel = Main.rand.NextVector2Circular(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.35f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.35f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
            #endregion

            Projectile.NewProjectile(null, projectile.Center + new Vector2(0f, 0f), Vector2.Zero, ModContent.ProjectileType<JackOFace>(), 0, 0f, Main.myPlayer);

            #region vanillaKill (yes it is actually like this)
            //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 22;
            projectile.height = 22;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;



            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 128;
            projectile.height = 128;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            projectile.Damage();
            #endregion

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {

            //Vanilla
            if (projectile.velocity.X != oldVelocity.X)
            {
                projectile.velocity.X = 0f - oldVelocity.X;
                projectile.ai[1] += 1f;
            }
            if (projectile.velocity.Y != oldVelocity.Y)
            {
                projectile.velocity.Y = 0f - oldVelocity.Y;
                projectile.ai[1] += 1f;
            }

            //Custom
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = -0.15f, PitchVariance = 0.15f, MaxInstances = -1 }, projectile.Center);


            scaleState = 1;

            bool hitWall = false;
            if (projectile.velocity.X != oldVelocity.X)
            {
                hitWall = true;
            }

            float getSpeed = hitWall ? Math.Abs(projectile.velocity.X) : Math.Abs(projectile.velocity.Y);
            float clampedSpeed = Math.Clamp(getSpeed, 0f, 25f);
            float adjustMent = MathHelper.Lerp(0.5f, 1.35f, clampedSpeed / 25f) * 1.1f;
            bounceIntensity = adjustMent;

            drawRotation = projectile.velocity.ToRotation() + (projectile.velocity.X > 0 ? 0 : MathHelper.Pi);

            //Impact Dust
            float circlePulseSize = 0.15f;

            Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 2f, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.2f, newColor: Color.OrangeRed);
            CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            b2.drawLayer = "Dusts";
            d2.customData = b2;
            d2.scale = circlePulseSize * 0.5f;

            for (int i = 0; i < 7 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Vector2 spawnOffset = Main.rand.NextVector2Circular(5f, 5f);

                Dust p = Dust.NewDustPerfect(projectile.Center + spawnOffset + oldVelocity, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.75f, 1.05f),
                    newColor: Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f), Scale: Main.rand.NextFloat(0.25f, 0.45f) * projectile.scale);
            }


            return false;
        }

    }

    public class JackOFace : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.scale = 1f;

            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;


        int timer = 0;
        public override void AI()
        {
            if (timer == 0)
                Projectile.rotation += Main.rand.NextFloat(-0.5f, 0.5f);

            if (overallAlpha < 1f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.11f), 0f, 1f);
            else
                overallScale = Math.Clamp(MathHelper.Lerp(overallScale, -0.25f, 0.11f), 0f, 1f);

            /*
            if (overallAlpha < 1f)
                overallAlpha += 0.1f;
            else
                overallScale -= 0.05f;
            */

            if (overallScale <= 0f)
                Projectile.active = false;

            timer++;
        }

        float overallAlpha = 0f;
        float overallScale = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Face = Mod.Assets.Request<Texture2D>("Content/Weapons/Ranged/Hardmode/Misc/JackFaceGlow").Value;

            float scale = overallAlpha + 0.25f * (1 - overallScale);

            Color betweenOr = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);

            Main.spriteBatch.Draw(Face, Projectile.Center - Main.screenPosition, null, betweenOr with { A = 0 } * overallScale, Projectile.rotation, Face.Size() / 2, Projectile.scale * 1.65f * scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Face, Projectile.Center - Main.screenPosition, null, betweenOr with { A = 0 } * overallScale, Projectile.rotation, Face.Size() / 2, Projectile.scale * 1.65f * scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
