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

    public class SmokeTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 370;

        }

        public Color col = Color.OrangeRed * 0.75f;
        public float alphaFade = 0.94f * Main.rand.NextFloat(0.9f, 1f);
        public float scaleFade = 0.97f;
        public float velFade = 0.9f;
        public float ticksBetweenFrames = 3;
        public float rotPower = 0.02f;

        public float randomRotatePower = 0f;

        float whiteAmount = 0f;// Main.rand.NextFloat(0.15f);
        float whiteFade = 0.8f;

        int timer = 0;

        float overallScale = 0f;
        float overallAlpha = 1f;
        public override void AI()
        {
            if (timer == 0)
                Projectile.ai[0] = Main.rand.NextBool() ? 1 : 2;
            
            if (timer % ticksBetweenFrames == 0 && Projectile.frame <= 4 && timer != 0)
                Projectile.frame = Projectile.frame + 1;

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1f));

            //overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.07f), 0f, 1f);

            Projectile.rotation += Projectile.velocity.Length() * rotPower * (Projectile.velocity.X > 0 ? 1f : -1f);

            Projectile.velocity *= velFade;

            if (Projectile.frame >= 4)
            {
                Projectile.scale *= scaleFade;
                overallAlpha *= alphaFade;
                overallAlpha *= alphaFade;
            }
            Projectile.scale *= scaleFade;
            overallAlpha *= alphaFade;

            if (overallAlpha <= 0.05f || Projectile.scale <= 0.05f)
                Projectile.active = false;

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

            return false;
        }

        public void DrawSmoke(bool returnImmediately)
        {
            if (returnImmediately || timer == 0)
                return;

            //Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/SmokeSheet" + Projectile.ai[0]).Value;
            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/SmokeSheetSharp1").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Smoke.Frame(1, 6, frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = SpriteEffects.None;// Projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color toUse = Color.Lerp(col, Color.White, Easings.easeOutSine(whiteAmount));

            //Main Tex
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            //Main.spriteBatch.Draw(Smoke, drawPos, sourceRectangle, Color.White with { A = 0 } * Easings.easeInCirc(overallAlpha) * 0.1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.75f, SE, 0f); //0.3
            Main.spriteBatch.Draw(Smoke, drawPos, sourceRectangle, toUse with { A = 0 } * overallAlpha * 5f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            //Main.spriteBatch.Draw(Smoke, drawPos, sourceRectangle, col with { A = 0 } * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            //Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
    }

    public class SmokeTest2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 370;

        }

        public Color col = Color.OrangeRed * 0.75f;
        public float alphaFade = 0.95f * Main.rand.NextFloat(0.9f, 1f);
        public float scaleFade = 0.97f;
        public float velFade = 0.9f;
        public float ticksBetweenFrames = 3;
        public float rotPower = 0.02f;

        public float randomRotatePower = 0f;

        float whiteAmount = Main.rand.NextFloat(0.5f);
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

            Projectile.rotation += Projectile.velocity.X * rotPower * (Projectile.velocity.X > 0 ? 1f : -1f);

            Projectile.velocity *= velFade;

            if (Projectile.frame >= 4)
            {
                Projectile.scale *= scaleFade;
                overallAlpha *= alphaFade;
                overallAlpha *= alphaFade;
            }
            Projectile.scale *= scaleFade;
            overallAlpha *= alphaFade;

            if (overallAlpha <= 0.05f || Projectile.scale <= 0.05f)
                Projectile.active = false;

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

            return false;
        }

        public void DrawSmoke(bool returnImmediately)
        {
            if (returnImmediately)
                return;

            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/Smoke1SheetClear").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Smoke.Frame(1, 6, frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = SpriteEffects.None;// Projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/GlowMisc", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["uColor"].SetValue(col.ToVector3() * Easings.easeInOutCubic(overallAlpha) * 2f);
            myEffect.Parameters["uTime"].SetValue(0);
            myEffect.Parameters["uOpacity"].SetValue(0.7f); //0.6
            myEffect.Parameters["uSaturation"].SetValue(0.5f);

            Color toUse = Color.Lerp(col, Color.White, 0.5f * Easings.easeOutQuad(whiteAmount));

            //Main Tex
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            //Main.spriteBatch.Draw(Smoke, drawPos, sourceRectangle, Color.White with { A = 0 } * Easings.easeInCirc(overallAlpha) * 0.1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.75f, SE, 0f); //0.3
            Main.spriteBatch.Draw(Smoke, drawPos, sourceRectangle, toUse with { A = 255 } * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3
            //Main.spriteBatch.Draw(Smoke, drawPos, sourceRectangle, col with { A = 0 } * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

        }
    }

    public class SmokeTest3 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 370;

        }

        public Color col = Color.OrangeRed * 0.75f;
        public float alphaFade = 1f;//0.95f * Main.rand.NextFloat(0.9f, 1f);
        public float scaleFade = 0.97f;
        public float velFade = 0.85f;
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

            Projectile.rotation += Projectile.velocity.X * rotPower * (Projectile.velocity.X > 0 ? 1f : -1f);

            Projectile.velocity *= velFade;

            if (Projectile.frame >= 4)
            {
                Projectile.scale *= scaleFade;
                overallAlpha *= alphaFade;
                overallAlpha *= alphaFade;
            }
            Projectile.scale *= scaleFade;
            overallAlpha *= alphaFade;

            if (Projectile.scale <= 0.2)
                Projectile.scale -= 0.02f;

            if (overallAlpha <= 0.05f || Projectile.scale <= 0.05f)
                Projectile.active = false;

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

            return false;
        }

        Effect myEffect = null;
        bool AorB = Main.rand.NextBool();
        public void DrawSmoke(bool returnImmediately)
        {
            if (returnImmediately)
                return;

            int sheet = AorB ? 1 : 2;
            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/SmokeC").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Smoke.Frame(1, 6, frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = SpriteEffects.None;

            Color toUse = Color.Lerp(col, Color.Wheat, Easings.easeOutQuad(whiteAmount));

            //Main Tex
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            //Main.spriteBatch.Draw(Orb, drawPos, null, Color.Black with { A = 255 } * overallAlpha * 0.2f, Projectile.rotation, Orb.Size() / 2f, Projectile.scale * overallScale, SE, 0f); //0.3

            Main.spriteBatch.Draw(Orb, drawPos, null, toUse with { A = 0 } * overallAlpha * 0.25f, Projectile.rotation, Orb.Size() / 2f, Projectile.scale * overallScale, SE, 0f); //0.3

            Main.spriteBatch.Draw(Smoke, drawPos, sourceRectangle, toUse with { A = 0 } * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3


            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

        }
    }

    public class SmokeTest4 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 370;

        }

        public Color col = Color.OrangeRed * 0.75f;
        public float alphaFade = 0.99f;//0.95f * Main.rand.NextFloat(0.9f, 1f);
        public float scaleFade = 0.97f;
        public float velFade = 0.85f;
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

            Projectile.rotation += Projectile.velocity.X * rotPower * (Projectile.velocity.X > 0 ? 1f : -1f);

            Projectile.velocity *= velFade;

            if (Projectile.frame >= 4)
            {
                Projectile.scale *= scaleFade;
                overallAlpha *= alphaFade;
                overallAlpha *= alphaFade;
            }
            Projectile.scale *= scaleFade;
            overallAlpha *= alphaFade;

            if (Projectile.scale <= 0.2)
                Projectile.scale -= 0.02f;

            if (overallAlpha <= 0.05f || Projectile.scale <= 0.05f)
                Projectile.active = false;

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

            return false;
        }

        Effect myEffect = null;
        bool AorB = Main.rand.NextBool();
        public void DrawSmoke(bool returnImmediately)
        {
            if (returnImmediately)
                return;

            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/BlurSmoke").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle;// Smoke.Frame(1, 6, frameY: Projectile.frame);
            Vector2 TexOrigin = Smoke.Size() / 2f;
            SpriteEffects SE = SpriteEffects.None;

            Color toUse = Color.Lerp(col, Color.Wheat, Easings.easeOutQuad(whiteAmount));

            //Main Tex
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            //Main.spriteBatch.Draw(Orb, drawPos, null, Color.Black with { A = 255 } * overallAlpha * 0.2f, Projectile.rotation, Orb.Size() / 2f, Projectile.scale * overallScale, SE, 0f); //0.3

            //Main.spriteBatch.Draw(Orb, drawPos, null, toUse with { A = 0 } * overallAlpha * 0.25f, Projectile.rotation, Orb.Size() / 2f, Projectile.scale * overallScale, SE, 0f); //0.3

            Main.spriteBatch.Draw(Smoke, drawPos, null, col with { A = 0 } * overallAlpha * 0.75f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.075f, SE, 0f); //0.3
            Main.spriteBatch.Draw(Smoke, drawPos, null, Color.LightGoldenrodYellow with { A = 0 } * overallAlpha, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.075f * 0.35f, SE, 0f); //0.3


            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

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
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawSmoke(false);
            });
            DrawSmoke(true);

            return false;
        }

        Effect myEffect = null;
        bool AorB = Main.rand.NextBool();
        public void DrawSmoke(bool returnImmediately)
        {
            if (returnImmediately)
                return;

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



            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/Smoke/WispSmoke").Value; //spark_02 | smoke_02
            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Smoke/InvertMask").Value; //WispSmokeMask, LavaNoise, noise/Swirl, vnoise is fine, Trail_2 

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle;// = Smoke.Frame(1, 6, frameY: Projectile.frame);
            Vector2 TexOrigin = Smoke.Size() / 2f;
            SpriteEffects SE = SpriteEffects.None;


            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Compiler/SmokeColShader", AssetRequestMode.ImmediateLoad).Value;

            float maskVal = 1f - overallAlpha;// 0.5f + ((float)Math.Sin(Main.timeForVisualEffects * 0.05f) * 0.5f);

            Color myCol = Color.OrangeRed;// Color.Lerp(col, Color.Red, Easings.easeInOutQuad(maskVal));

            myEffect.Parameters["color"].SetValue(myCol.ToVector3() * 15f * overallAlpha); //30
            myEffect.Parameters["glowThreshold"].SetValue(0.8f); //0.9f
            myEffect.Parameters["glowPower"].SetValue(3.5f); //3.5
            myEffect.Parameters["fadeProgress"].SetValue(maskVal);
            myEffect.Parameters["endAlpha"].SetValue(1f);


            myEffect.Parameters["maskTexture"].SetValue(Mask);

            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            Main.spriteBatch.Draw(Ball, drawPos, null, myCol with { A = 0 } * Easings.easeInSine(overallAlpha) * 0.25f, Projectile.rotation, Ball.Size() / 2f, Projectile.scale * overallScale * 1.45f, SE, 0f); //0.3


            //Main Tex
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(Smoke, drawPos, null, myCol * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.25f, SE, 0f); //0.3

            //myEffect.Parameters["color"].SetValue(Color.GreenYellow.ToVector3() * 15f * overallAlpha); //30
            //myEffect.CurrentTechnique.Passes[0].Apply();
            //Main.spriteBatch.Draw(Smoke, drawPos + new Vector2(200f, 0f), null, myCol * overallAlpha * 1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale * 0.25f, SE, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
    }

}