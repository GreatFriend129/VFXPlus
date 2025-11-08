using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Interfaces;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Projectiles;
using static Terraria.NPC;

namespace VFXPlus.Content.VFXTest.Aero
{

    public class BooyahBomb : ModItem
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = Item.height = 44;

            Item.useTime = Item.useAnimation = 24;

            Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<BooyahHeldProj>();
            Item.UseSound = SoundID.Item1;
            Item.shootSpeed = 34f;

            Item.channel = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            return true;
        }
    }

    public class BooyahHeldProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Magic;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;


        int chargeTime = 75;

        int timer = 0;
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            owner.direction = Main.MouseWorld.X > owner.Center.X ? 1 : -1;

            Vector2 orbPosOffset = new Vector2(-11f * owner.direction, -30f + owner.gfxOffY);

            Projectile.velocity = Vector2.Zero;
            Projectile.Center = owner.Center + orbPosOffset;


            float chargeProg = Utils.GetLerpValue(0f, 1f, (float)timer / (float)chargeTime, true);

            if (owner.channel || chargeProg < 1f)
            {
                owner.itemAnimation = 20;
                owner.itemTime = 20;
                Projectile.timeLeft = 20;

                if (timer % 4 == 0)
                {
                    Dust dp = Dust.NewDustPerfect(owner.Center + orbPosOffset, ModContent.DustType<ElectricSparkGlow>(), newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.75f, 1f) + (chargeProg * 0.5f));
                    dp.velocity *= 1f + chargeProg;

                    ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.91f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 1f); //0.91
                    esb.killEarlyTime = 20;
                    dp.customData = esb;
                }
            }
            else
            {
                if (Projectile.ai[0] == 0)
                {
                    Vector2 toMouse = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 shotVel = toMouse * 12f;

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center + orbPosOffset, shotVel, ModContent.ProjectileType<BooyahBombProj>(), 2, 2, owner.whoAmI);

                    if (owner.velocity.Y != 0)
                        owner.velocity.X += toMouse.X * -6f;

                    owner.velocity.Y *= 0.25f;
                    owner.velocity.Y += toMouse.Y * -10f;

                    //Reset player staring fall pos so they don't explode from fall damage even if they significantly slow their fall with this weapon
                    if (toMouse.Y > 0.25f)
                        owner.fallStart = owner.position.ToTileCoordinates().Y;

                    Projectile.ai[0]++;
                }
            }

            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3() * chargeProg);

            timer++;
        }

        Effect myEffect = null;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            if (timer > chargeTime && !Main.player[Projectile.owner].channel)
                return false;

            float postFullChargeProg = Utils.GetLerpValue(chargeTime, chargeTime + 15, timer, true);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D gash = CommonTextures.SoulSpike.Value;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.1f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.06f;

            Color between = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.5f);

            Vector2 gashScale = new Vector2(1f * Easings.easeOutCubic(postFullChargeProg) * sineScale2, 0.45f * sineScale1) * Projectile.scale;
            Main.EntitySpriteDraw(gash, drawPos, null, between with { A = 0 } * 0.35f, 0f, gash.Size() / 2f, gashScale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(gash, drawPos, null, Color.White with { A = 0 } * 0.35f, 0f, gash.Size() / 2f, gashScale * 1f, SpriteEffects.None);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawBasicBall(false);
            });

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawBall(false);
            });

            return false;
        }

        public void DrawBasicBall(bool giveUp)
        {
            if (giveUp)
                return;

            float chargeProg = Utils.GetLerpValue(0f, 1f, (float)timer / (float)chargeTime, true);
            float postFullChargeProg = Utils.GetLerpValue(chargeTime, chargeTime + 15, timer, true);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            //Draw Orb
            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;

            Color[] cols = { Color.White * 1f, Color.DeepSkyBlue * 0.525f, Color.DodgerBlue * 0.375f };
            float[] scales = { 0.85f, 1.45f, 2.5f };

            float orbAlpha = 1f;
            float totalScale = Projectile.scale * 0.45f * Easings.easeInOutQuad(chargeProg);

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.1f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.06f;

            Main.EntitySpriteDraw(Orb, drawPos, null, Color.DodgerBlue * orbAlpha * 0.35f, 0f, Orb.Size() / 2f, scales[2] * totalScale, SpriteEffects.None);

            Main.EntitySpriteDraw(Orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[0] * totalScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[1] * totalScale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[2] * totalScale * sineScale2, SpriteEffects.None);
        }

        public void DrawBall(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;
            Texture2D ball2 = CommonTextures.feather_circle128PMA.Value;// Mod.Assets.Request<Texture2D>("Assets/InfernoOrb").Value;

            float chargeProg = Utils.GetLerpValue(0f, 1f, (float)timer / 75f, true);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float drawScale = Projectile.scale * Easings.easeInQuad(chargeProg) * 0.3f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.055f) * 0.07f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.07f;
            float sineScale3 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.2f + timer * 0.05f) * 0.03f;
            float sineColor = (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.2f;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/WaterEnergyNoise").Value); //foam_mask_bloom
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/SofterBlueGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/sparkNoiseloop").Value);
            myEffect.Parameters["flowSpeed"].SetValue(1f);
            myEffect.Parameters["distortStrength"].SetValue(0.06f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.01f);

            myEffect.Parameters["vignetteSize"].SetValue(1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.5f);
            myEffect.Parameters["colorIntensity"].SetValue(2f * chargeProg);

            //Main shader
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot1 = (float)Main.timeForVisualEffects * 0.01f; //0.01
            Main.spriteBatch.Draw(ball, drawPos, null, Color.White with { A = 0 }, rot1, ball.Size() / 2, drawScale * sineScale3, 0f, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }




    }

    public class BooyahBombProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public bool isHeld = true;

        float justShotVal = 1f;

        int timer = 0;
        public override void AI()
        {
            Projectile.velocity.Y += 0.25f;

            int trailCount = 30;
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center + Projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            Lighting.AddLight(Projectile.Center, Color.DeepSkyBlue.ToVector3());

            if (timer > 5)
                justShotVal = Math.Clamp(MathHelper.Lerp(justShotVal, -0.35f, 0.04f), 0f, 1f);

            if (timer % 4 == 0)
            {
                Dust dp = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ElectricSparkGlow>(), newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.85f, 1f));
                dp.velocity += Projectile.velocity * 0.5f;

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.8f, FadeScalePower: 0.91f, FadeVelPower: 0.92f, Pixelize: false, XScale: 1f, YScale: 1f); //0.91
                esb.killEarlyTime = 20;
                dp.customData = esb;
            }

            timer++;
        }

        public override bool PreKill(int timeLeft)
        {
            Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.25f);

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.65f, true, 2, 1f, 1f);

            //Dust d1 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CirclePulse2>(), Velocity: Vector2.Zero, newColor: between * 0.15f);
            //d1.customData = cpb2;
            //d1.velocity = new Vector2(-0.01f, 0f);

            //Dust d2 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CirclePulse2>(), Velocity: Vector2.Zero, newColor: between * 0.15f);
            //d2.customData = cpb2;
            //d2.velocity = new Vector2(0.01f, 0f);


            float sparkRotOffset = Main.rand.NextFloat(6.28f);
            int sparkCount = 8;
            for (int i = 220; i < sparkCount; i++) //2 //0,3
            {
                float dir = (MathHelper.TwoPi / (float)sparkCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(2f, 6.5f) * 2f;
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f) + sparkRotOffset);

                Dust dp = Dust.NewDustPerfect(Projectile.Center + dustVel, ModContent.DustType<ElectricSparkGlow>(), dustVel, newColor: Color.DeepSkyBlue * 1f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 3f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.91f, FadeScalePower: 0.93f, FadeVelPower: 0.9f, TimeBetweenFrames: 5, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91
                esb.randomVelRotatePower = 1f;
                dp.customData = esb;
            }


            float crossRotOffset = Main.rand.NextFloat(6.28f);
            int crossCount = 8;
            for (int i = 220; i < crossCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f) + crossRotOffset);

                Color middleBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f + Main.rand.NextFloat(-0.15f, 0.15f));

                Dust gd = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.55f, 0.7f));
                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.9f, velToBeginShrink: 2f, fadePower: 0.92f, shouldFadeColor: false);
            }

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Thunder/ElectricExplode") with { Volume = 0.05f, Pitch = 0.35f, PitchVariance = 0.15f, MaxInstances = -1, };
            SoundEngine.PlaySound(style, Projectile.Center);

            Color between2 = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.15f);
            Dust d11 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowStarSharp>(), Velocity: Vector2.Zero, newColor: between2, Scale: 2.4f);
            Dust d12 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowStarSharp>(), Velocity: Vector2.Zero, newColor: Color.White, Scale: 1.2f);

            d11.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.85f, shouldFadeColor: true);
            d12.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.85f, shouldFadeColor: true);

            int windFX2 = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<GaussExplosionVFX>(), 0, 0, Main.myPlayer);
            int windFX23 = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CrackTest>(), 0, 0, Main.myPlayer);

            for (int i = 220; i < 16 + Main.rand.Next(0, 2); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f) * 1.5f;

                Color col = Color.DeepSkyBlue;

                Dust pa = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedLineSpark>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.5f);

                pa.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 8 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                    1f, 0.5f, shouldFadeColor: false);
            }


            return base.PreKill(timeLeft);
        }

        int initialDir = 1;

        float overallAlpha = 1f;
        float overallScale = 1f;

        Effect myEffect = null;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D gash = CommonTextures.SoulSpike.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.1f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.06f;

            Color between = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.5f);

            Vector2 gashScale = new Vector2(2.5f * Easings.easeOutCubic(1f - justShotVal) * sineScale2, 0.45f * sineScale1) * Projectile.scale;
            Main.EntitySpriteDraw(gash, drawPos, null, between with { A = 0 } * Easings.easeInQuad(justShotVal) * 1f, 0f, gash.Size() / 2f, gashScale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(gash, drawPos, null, Color.White with { A = 0 } * Easings.easeInQuad(justShotVal) * 1f, 0f, gash.Size() / 2f, gashScale * 1f, SpriteEffects.None);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawTrail(false);
                DrawBasicBall(false);
            });

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawBall(false);
            });

            return false;
        }

        public void DrawBasicBall(bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            //Draw Orb
            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;

            Color[] cols = { Color.White * 1f, Color.DeepSkyBlue * 0.525f, Color.DodgerBlue * 0.375f };
            float[] scales = { 0.85f, 1.45f, 2.5f };

            float orbAlpha = 1f;
            float totalScale = Projectile.scale * 0.45f * Easings.easeInOutQuad(overallScale);

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.1f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.06f;

            Main.EntitySpriteDraw(Orb, drawPos, null, Color.DodgerBlue * orbAlpha * 0.35f, 0f, Orb.Size() / 2f, scales[2] * totalScale, SpriteEffects.None);

            Main.EntitySpriteDraw(Orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[0] * totalScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[1] * totalScale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[2] * totalScale * sineScale2, SpriteEffects.None);
        }

        public void DrawBall(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;
            Texture2D ball2 = CommonTextures.feather_circle128PMA.Value;// Mod.Assets.Request<Texture2D>("Assets/InfernoOrb").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float drawScale = Projectile.scale * Easings.easeOutCirc(1f) * 0.3f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.055f) * 0.07f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.07f;
            float sineScale3 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.2f + timer * 0.05f) * 0.03f;
            float sineColor = (float)Math.Sin(Main.timeForVisualEffects * 0.08f) * 0.2f;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/WaterEnergyNoise").Value); //foam_mask_bloom
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/SofterBlueGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/sparkNoiseloop").Value);
            myEffect.Parameters["flowSpeed"].SetValue(1f);
            myEffect.Parameters["distortStrength"].SetValue(0.06f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.01f);

            myEffect.Parameters["vignetteSize"].SetValue(1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.5f);
            myEffect.Parameters["colorIntensity"].SetValue(2f * overallAlpha);

            //Main shader
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot1 = (float)Main.timeForVisualEffects * 0.01f;
            Main.spriteBatch.Draw(ball, drawPos, null, Color.White with { A = 0 }, rot1, ball.Size() / 2, drawScale * sineScale3, 0f, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        Effect trailEffect = null;
        public void DrawTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/EvenThinnerGlowLine").Value; 
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/Trail5Loop").Value;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0f;


            Color StripColor(float progress) => Color.White * Easings.easeInCubic(progress * progress) * overallAlpha * (1f - justShotVal);

            float StripWidth(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.95f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.95f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = 1f;//Easings.easeOutSine(1f - LV);
                }

                return toReturn * sineWidthMult * overallScale * 120f * 0.85f; //50
            }

            float StripWidth2(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.95f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.95f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = 1f;//Easings.easeOutSine(1f - LV);
                }

                return toReturn * overallScale * 70f; //50
            }


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //0.02

            float repPercent = (float)previousPositions.Count / 30f;
            trailEffect.Parameters["reps"].SetValue(0.5f * repPercent);

            //Under layer
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["ColorOne"].SetValue(Color.DeepSkyBlue.ToVector3() * 1f);
            trailEffect.Parameters["glowThreshold"].SetValue(1f);
            trailEffect.Parameters["glowIntensity"].SetValue(1.2f);
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Color between = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.4f);
            //Over 
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            trailEffect.Parameters["glowThreshold"].SetValue(1f);
            trailEffect.Parameters["glowIntensity"].SetValue(1f);
            trailEffect.Parameters["ColorOne"].SetValue(between.ToVector3() * 2.5f); //Hotpink4.5
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }

    public class GaussExplosionVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.hostile = false;
            Projectile.penetrate = -1;

            Projectile.scale = 1f;

            Projectile.timeLeft = 800;
            Projectile.tileCollide = false;
            Projectile.width = 20;
            Projectile.height = 20;
        }

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;

        int timer = 0;
        public override void AI()
        {
            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);

            int timeForPulse = 30;
            if (timer <= timeForPulse)
                overallScale = MathHelper.Lerp(0f, 1f, Easings.easeOutQuint((float)timer / (float)timeForPulse));

            if (timer >= 0)
            {
                if (timer >= (timeForPulse * 0.15f))
                    overallAlpha -= 0.055f;

                if (overallAlpha <= 0)
                    Projectile.active = false;
            }

            timer++;
        }

        float overallScale = 0f;
        float overallAlpha = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawShader(false);
            });
            DrawShader(true);

            return false;
        }

        public void DrawShader(bool giveUp = false)
        {
            if (giveUp)
                return;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/WaterEnergyNoise").Value); //foam_mask_bloom
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/SofterBlueGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/sparkNoiseloop").Value);
            myEffect.Parameters["flowSpeed"].SetValue(1f);
            myEffect.Parameters["distortStrength"].SetValue(0.06f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.01f);

            myEffect.Parameters["vignetteSize"].SetValue(1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.8f);
            myEffect.Parameters["colorIntensity"].SetValue(2f * overallAlpha);

            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/circle_05").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float scale = overallScale * Projectile.scale * 0.65f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(Tex2, drawPos, null, Color.White, Projectile.rotation, Tex2.Size() / 2, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, Color.White, Projectile.rotation, Tex2.Size() / 2, scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
    }
}