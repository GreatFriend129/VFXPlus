using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Graphics;
using ReLogic.Content;
using Microsoft.CodeAnalysis;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Common;
using Microsoft.Build.Evaluation;
using System.Runtime.CompilerServices;
using VFXPlus.Common.Drawing;
using Steamworks;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace VFXPlus.Content.QueenBee
{
    public class BeeDrawingTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = false;
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.tileCollide = true;
            Projectile.timeLeft = 370;

        }

        bool hittable = true;

        int timer = 0;

        float overallScale = 0f;
        float overallAlpha = 1f;
        public override void AI()
        {
            if (timer == 0)
                hittable = Main.rand.NextBool();

            int trailCount = 9;
            previousPositions.Add(Projectile.Center);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 5 == 0)
                Projectile.frame = (Projectile.frame + 1) % 4;

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 2f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.07f), 0f, 1f);

            Projectile.rotation = Projectile.velocity.X * 0.05f;

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Bee = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/BeeSheet").Value;
            Texture2D BeeBorder = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/BeeSheetBorder").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Bee.Frame(1, 4, frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = Projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Orb
            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;
            Color orbCol1 = hittable ? Color.DarkGoldenrod : Color.Red;

            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol1 with { A = 0 } * 0.6f, Projectile.rotation, Glow.Size() / 2f, Projectile.scale * 0.3f * overallScale, SpriteEffects.None);


            //Trail
            Color afterImageCol = hittable ? Color.Gold : Color.Red;
            float afterImageAlpha = hittable ? 0.125f : 0.25f;
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                float size = (Projectile.scale * overallScale) - (0.33f - (progress * 0.33f));
                Color col = afterImageCol with { A = 0 } * progress;


                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(Bee, AfterImagePos, sourceRectangle, col * afterImageAlpha,
                        Projectile.rotation, TexOrigin, size, SE);
            }

            //Border
            Color borderCol = hittable ? Color.White : Color.Red;
            for (int i = 0; i < 4; i++)
            {
                Main.spriteBatch.Draw(Bee, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle, borderCol with { A = 0 } * overallAlpha * 1.5f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3
            }

            //Red Border on non-hittable bees
            //if (!hittable)
                //Main.spriteBatch.Draw(BeeBorder, drawPos, sourceRectangle, Color.Red with { A = 0 } * overallAlpha, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.9f, SE, 0f); //0.3

            //Main Tex
            Main.spriteBatch.Draw(Bee, drawPos, sourceRectangle, lightColor * overallAlpha, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            return false;
        }

    }

    public class StingerTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = false;
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.tileCollide = true;
            Projectile.timeLeft = 370;

        }

        int timer = 0;

        float overallScale = 0f;
        float overallAlpha = 1f;
        public override void AI()
        {
            int trailCount = 14;
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 5 == 0)
                Projectile.frame = (Projectile.frame + 1) % 4;

            Projectile.velocity *= 1.04f;

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 2f));

            overallAlpha = 1f;// Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.07f), 0f, 1f);

            Projectile.rotation = Projectile.velocity.ToRotation();

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                NewTrail(false);
            });
            NewTrail(true);

            Texture2D Stinger = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/Stinger").Value;
            Texture2D StingerBorder = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/StingerGlow").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = Stinger.Size() / 2f;
            SpriteEffects SE = Projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;


            Main.spriteBatch.Draw(StingerBorder, drawPos, null, Color.Gold with { A = 0 } * overallAlpha * 0.5f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.95f, SE, 0f); //0.3

            //Main Tex
            Main.spriteBatch.Draw(Stinger, drawPos, null, lightColor * overallAlpha, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            return false;
        }

        public void NewTrail(bool returnImmediately)
        {
            if (returnImmediately)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            //Orb
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color[] cols = { Color.Gold * 0.75f, Color.Orange * 0.525f, Color.OrangeRed * 0.375f };
            float[] scales = { 1.15f, 1.6f, 2.5f };

            float orbRot = Projectile.velocity.ToRotation();
            float orbAlpha = 0.25f * overallAlpha * 0f;
            Vector2 orbScale = new Vector2(0.85f, 0.55f) * 0.3f * Projectile.scale * overallScale;
            Vector2 orbOrigin = orb.Size() / 2f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[2] * sineScale2, SpriteEffects.None);

            Texture2D Spike = CommonTextures.SoulSpike.Value;
            
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float scale = (float)i / previousPositions.Count;
                Vector2 vec2ScaleTrail = new Vector2(scale * Easings.easeOutSine(overallAlpha) * 1.15f, Easings.easeInQuad(scale) * 0.6f) * Projectile.scale;

                Vector2 drawPosAI = previousPositions[i] - Main.screenPosition;

                Color betweenBlue = Color.Lerp(Color.Orange, Color.Goldenrod, 0f);
                Color col = Color.Lerp(Color.OrangeRed, Color.Orange, scale) * scale * overallAlpha;

                Main.spriteBatch.Draw(Spike, drawPosAI, null, col with { A = 0 } * 0.5f, previousRotations[i], Spike.Size() / 2f, vec2ScaleTrail, SpriteEffects.None, 0f);
            }
        }

    }

    public class StingerTest2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = false;
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.tileCollide = true;
            Projectile.timeLeft = 370;

        }

        int timer = 0;
        float alpha = 0f;
        float pulseIntensity = 0f;

        public int velShrinkTime = 35;
        public int velGrowTime = 60;
        public float velShrinkAmount = 0.93f;
        public float velGrowAmount = 1.15f;

        public override void AI()
        {
            if (timer == 0)
            {
                Projectile.ai[0] = Projectile.velocity.Length();
                previousRotations = new List<float>();
                previousPositions = new List<Vector2>();

                SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_ogre_spit") with { Pitch = 1f, PitchVariance = .33f, MaxInstances = 1 };
                SoundEngine.PlaySound(style, Projectile.Center);

                SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_ballista_tower_shot_1") with { Pitch = .54f, PitchVariance = 0.2f, Volume = 0.3f, MaxInstances = 1 };
                SoundEngine.PlaySound(style3, Projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_42") with { Pitch = .2f, PitchVariance = .2f, Volume = 0.55f, MaxInstances = 1 };
                SoundEngine.PlaySound(style2, Projectile.Center);

                Projectile.rotation = Projectile.velocity.ToRotation();

                pulseIntensity = 1f;
            }


            if (timer <= velShrinkTime)
                Projectile.velocity *= velShrinkAmount;
            else if (timer < velGrowTime)
                Projectile.velocity *= velGrowAmount;


            int trailCount = 10;
            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            //Dust
            if (timer % 2 == 0 && Main.rand.NextBool(3) && timer > velShrinkTime)
            {


            }



            pulseIntensity = Math.Clamp(MathHelper.Lerp(pulseIntensity, -0.25f, 0.045f), 0f, 2f);
            alpha = Math.Clamp(MathHelper.Lerp(alpha, 1.25f, 0.045f), 0f, 1f);

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;
            Texture2D Feather = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/Stinger").Value;
            Texture2D FeatherGray = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/StingerGray").Value;
            Texture2D FeatherWhite = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/StingerWhite").Value;

            Vector2 vec2MainScale = new Vector2(1f, 0.25f + (alpha * 0.75f)) * Projectile.scale;

            #region after image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float size = (0.75f + (progress * 0.25f)) * Projectile.scale;

                Color betweenBlue = Color.Lerp(Color.Orange, Color.Yellow, 0.5f);

                Color col = Color.Lerp(Color.OrangeRed, Color.Orange, progress) * progress;

                float size2 = (1f + (progress * 0.25f)) * Projectile.scale;
                Main.EntitySpriteDraw(FeatherGray, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.55f * alpha,
                        previousRotations[i], FeatherGray.Size() / 2f, size2, SpriteEffects.None);

                Vector2 vec2Scale = new Vector2(1f, 0.25f) * size;

                Main.EntitySpriteDraw(FeatherWhite, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.85f * alpha,
                        previousRotations[i], FeatherGray.Size() / 2f, vec2Scale, SpriteEffects.None);
            }

            #endregion

            Color outerCol = Color.Lerp(Color.Orange with { A = 0 } * 0.5f, Color.Gold with { A = 0 } * 0.8f, pulseIntensity);
            float scale = MathHelper.Lerp(1f, 1.25f, pulseIntensity);
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(FeatherWhite, Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f), null, outerCol * alpha, Projectile.rotation, Feather.Size() / 2f, vec2MainScale * scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(Feather, Projectile.Center - Main.screenPosition, null, lightColor * Easings.easeOutCirc(alpha), Projectile.rotation, Feather.Size() / 2f, vec2MainScale, SpriteEffects.None);

            Main.EntitySpriteDraw(Feather, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 0.4f * alpha, Projectile.rotation, Feather.Size() / 2f, vec2MainScale * 1f, SpriteEffects.None);


            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.Orange, Scale: Main.rand.NextFloat(0.35f, 0.45f));
                dust.velocity += Projectile.velocity * 0.25f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }
        }

    }

    public class StopAndStartStinger : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = false;
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.tileCollide = true;
            Projectile.timeLeft = 370;

            Projectile.scale = 1.15f;
        }

        int timer = 0;
        float alpha = 0f;
        float pulseIntensity = 0f;

        public int velShrinkTime = 35;
        public int velGrowTime = 60;
        public float velShrinkAmount = 0.93f;
        public float velGrowAmount = 1.15f;

        public float maxVel = 15f;
        public override void AI()
        {
            if (timer == 0)
            {
                Projectile.ai[0] = Projectile.velocity.Length();
                previousRotations = new List<float>();
                previousPositions = new List<Vector2>();

                SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_ogre_spit") with { Pitch = 1f, PitchVariance = .33f, MaxInstances = 1 };
                SoundEngine.PlaySound(style, Projectile.Center);

                SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_ballista_tower_shot_1") with { Pitch = .54f, PitchVariance = 0.2f, Volume = 0.3f, MaxInstances = 1 };
                SoundEngine.PlaySound(style3, Projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_42") with { Pitch = .2f, PitchVariance = .2f, Volume = 0.55f, MaxInstances = 1 };
                SoundEngine.PlaySound(style2, Projectile.Center);

                Projectile.rotation = Projectile.velocity.ToRotation();

                pulseIntensity = 1f;
            }


            if (timer <= velShrinkTime)
                Projectile.velocity *= velShrinkAmount;
            else if (timer < velGrowTime)
                Projectile.velocity *= velGrowAmount;

            if (Projectile.velocity.Length() > maxVel)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * maxVel;


            int trailCount = 10;
            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            //Dust
            if (timer % 2 == 0 && Main.rand.NextBool(3) && timer > velShrinkTime)
            {


            }



            pulseIntensity = Math.Clamp(MathHelper.Lerp(pulseIntensity, -0.25f, 0.045f), 0f, 2f);
            alpha = Math.Clamp(MathHelper.Lerp(alpha, 1.25f, 0.045f), 0f, 1f);

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;
            Texture2D Feather = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/Stinger").Value;
            Texture2D FeatherGray = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/StingerGray").Value;
            Texture2D FeatherWhite = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/StingerWhite").Value;

            Vector2 vec2MainScale = new Vector2(1f, 0.25f + (alpha * 0.75f)) * Projectile.scale;

            #region after image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float size = (0.75f + (progress * 0.25f)) * Projectile.scale;

                Color betweenBlue = Color.Lerp(Color.Orange, Color.Yellow, 0.5f);

                Color col = Color.Lerp(Color.OrangeRed, Color.Orange, progress) * progress;

                float size2 = (1f + (progress * 0.25f)) * Projectile.scale;
                Main.EntitySpriteDraw(FeatherGray, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.55f * alpha,
                        previousRotations[i], FeatherGray.Size() / 2f, size2, SpriteEffects.None);

                Vector2 vec2Scale = new Vector2(1f, 0.25f) * size;

                Main.EntitySpriteDraw(FeatherWhite, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.85f * alpha,
                        previousRotations[i], FeatherGray.Size() / 2f, vec2Scale, SpriteEffects.None);
            }

            #endregion

            Color outerCol = Color.Lerp(Color.Orange with { A = 0 } * 0.5f, Color.Gold with { A = 0 } * 0.8f, pulseIntensity);
            float scale = MathHelper.Lerp(1f, 1.25f, pulseIntensity);
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(FeatherWhite, Projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f), null, outerCol * alpha, Projectile.rotation, Feather.Size() / 2f, vec2MainScale * scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(Feather, Projectile.Center - Main.screenPosition, null, lightColor * Easings.easeOutCirc(alpha), Projectile.rotation, Feather.Size() / 2f, vec2MainScale, SpriteEffects.None);

            Main.EntitySpriteDraw(Feather, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 0.4f * alpha, Projectile.rotation, Feather.Size() / 2f, vec2MainScale * 1f, SpriteEffects.None);


            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.Orange, Scale: Main.rand.NextFloat(0.35f, 0.45f));
                dust.velocity += Projectile.velocity * 0.25f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }
        }

    }



    public class SmokeTest5 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 11370;

        }

        public Color col = Color.OrangeRed * 0.75f;
        public float alphaFade = 0.92f * Main.rand.NextFloat(0.9f, 1f); //0.92
        public float scaleFade = 0.97f; //0.97f
        public float velFade = 0.85f; //0.85f
        public float ticksBetweenFrames = 3;
        public float rotPower = 0.02f;

        public float randomRotatePower = 0f;

        float whiteAmount = 0f * Main.rand.NextFloat(0.25f);
        float whiteFade = 0.8f;

        int timer = 0;


        float overallScale = 0f;
        float overallAlpha = 1f;
        public override void AI()
        {
            if (timer % ticksBetweenFrames == 0 && Projectile.frame <= 4 && timer != 0)
                Projectile.frame = Projectile.frame + 1;

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1f));

            //overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.07f), 0f, 1f);

            Projectile.rotation += Projectile.velocity.X * 0.25f * rotPower * (Projectile.velocity.X > 0 ? 1f : -1f);

            Projectile.velocity *= velFade;

            if (Projectile.frame >= 4)
            {
                //Projectile.scale *= scaleFade;
                //overallAlpha *= alphaFade;
                overallAlpha *= alphaFade;
            }
            if (overallAlpha < 0.09f)
                overallAlpha *= alphaFade;
            overallAlpha *= alphaFade;

            if (Projectile.scale <= 0.2)
                Projectile.scale -= 0.02f;

            if (overallAlpha <= 0.03f || Projectile.scale <= 0.03f)
                Projectile.active = false;

            //col = new Color(col.R * 0.98f, col.G * 0.98f, col.B * 0.98f);

            whiteAmount *= whiteFade;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawSmoke(false);
            });
            DrawSmoke(true);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Color myCol = Color.Lerp(Color.OrangeRed, Color.Red, 0.25f);

                Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
                Main.spriteBatch.Draw(Ball, drawPos, null, myCol with { A = 0 } * Easings.easeInSine(overallAlpha) * 0.25f, Projectile.rotation, Ball.Size() / 2f, Projectile.scale * overallScale * 1.45f, 0, 0f); //0.3

            });

            return false;
        }

        Effect myEffect = null;
        int smokeTex = Main.rand.NextBool(3) ? 4 : 1;
        int maskTex = Main.rand.NextBool() ? 2 : 1;
        public void DrawSmoke(bool returnImmediately)
        {
            if (returnImmediately)
                return;

            //TODO: put the underglow in another pixelation system for TransformMatrix

            //float colorTimeOffset = (float)Main.timeForVisualEffects * 0.5f;
            //float sin1 = (float)Math.Sin(MathHelper.ToRadians(colorTimeOffset));
            //float sin2 = (float)Math.Sin(MathHelper.ToRadians(colorTimeOffset + 120));
            //float sin3 = (float)Math.Sin(MathHelper.ToRadians(colorTimeOffset + 240));
            //int middle = 180;
            //int length = 75;
            //float r = middle + length * sin1;
            //float g = middle + length * sin2;
            //float b = middle + length * sin3;
            //Color color = new Color((int)r, (int)g, (int)b);

            //4

            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/Smoke/WispSmokeClear").Value; //spark_02 | smoke_02

            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Smoke/InvertMask" + maskTex).Value; 
            
            //Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Smoke/InvertMask").Value; //WispSmokeMask, LavaNoise, noise/Swirl, vnoise is fine, Trail_2 

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle;// = Smoke.Frame(1, 6, frameY: Projectile.frame);
            Vector2 TexOrigin = Smoke.Size() / 2f;
            SpriteEffects SE = SpriteEffects.None;


            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Compiler/SmokeColShader", AssetRequestMode.ImmediateLoad).Value;

            float maskVal = 1f - overallAlpha;// 0.5f + ((float)Math.Sin(Main.timeForVisualEffects * 0.05f) * 0.5f);

            Color myCol = Color.Lerp(Color.OrangeRed, Color.Red, 0.25f);

            myEffect.Parameters["color"].SetValue(myCol.ToVector3() * 15f * overallAlpha); //15
            myEffect.Parameters["glowThreshold"].SetValue(0.4f); //0.8f
            myEffect.Parameters["glowPower"].SetValue(3.5f); //3.5
            myEffect.Parameters["fadeProgress"].SetValue(maskVal);
            myEffect.Parameters["endAlpha"].SetValue(1f * overallAlpha); //* overallAlpha


            //15 | 0.2 | 3.5 | 1f | clear smoke | Additive | Pixelation System

            myEffect.Parameters["maskTexture"].SetValue(Mask);

            //Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            //Main.spriteBatch.Draw(Ball, drawPos, null, myCol with { A = 0 } * Easings.easeInSine(overallAlpha) * 0.25f, Projectile.rotation, Ball.Size() / 2f, Projectile.scale * overallScale * 1.45f, SE, 0f); //0.3


            //Main Tex
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            myEffect.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(Smoke, drawPos, null, myCol * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.25f, SE, 0f); //0.3

            myEffect.Parameters["color"].SetValue(Color.GreenYellow.ToVector3() * 15f * overallAlpha); //30
            myEffect.CurrentTechnique.Passes[0].Apply();
            //Main.spriteBatch.Draw(Smoke, drawPos + new Vector2(200f, 0f), null, myCol * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.25f, SE, 0f); //0.3


            myEffect.Parameters["color"].SetValue(Color.SkyBlue.ToVector3() * 10f * overallAlpha); //30
            myEffect.CurrentTechnique.Passes[0].Apply();
            //Main.spriteBatch.Draw(Smoke, drawPos + new Vector2(400f, 0f), null, myCol * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.25f, SE, 0f); //0.3


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
    }

}