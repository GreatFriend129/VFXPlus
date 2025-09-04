using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using static log4net.Appender.ColoredConsoleAppender;

namespace VFXPlus.Content.QueenBee
{
    public partial class ReduxQueenBee : ModNPC
    {
        Texture2D BaseAnim => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnim");
        Texture2D BaseAnimGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnimGlow");

        Texture2D BaseAnimBodyGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnimBodyGlow");
        Texture2D BaseAnimWingGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnimWingGlow");

        Texture2D DashAnim => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBDashAnimGlow");
        Texture2D DashAnimGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBDashAnimGlow");


        //public float randomShakePower = 0f;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        float stretchIntensity = 0f;
        float squashAmount = 0f;


        public List<Vector2> dashTrailPositions = new List<Vector2>();
        public List<float> dashTrailRotations = new List<float>();

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawWindDrill(-1, drawColor * 0.3f); //33

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
            {
                DrawDashAfterImage();
            });


            Vector2 drawPos = NPC.Center - Main.screenPosition;// + (Main.rand.NextVector2Circular(7f, 7f) * randomShakePower);

            Texture2D MainTexture = BaseAnim;

            int numberOfFrames = (isDashing ? 4 : 7);
            int currentFrame = (int)NPC.frameCounter % numberOfFrames;
            int frameHeight = MainTexture.Height / numberOfFrames;

            Rectangle sourceRectangle = new Rectangle(0, frameHeight * currentFrame, MainTexture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;


            DrawBasicAfterImage(drawPos, sourceRectangle);

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
            {
                DrawSmokyBorder(drawPos, sourceRectangle);
            });

            Texture2D glorb = CommonTextures.feather_circle128PMA.Value;
            Vector2 glorbScale = new Vector2(0.65f, 0.95f) * overallScale * NPC.scale * 2f;
            Color glorbCol = Color.Lerp(Color.Orange, Color.OrangeRed, 0.25f);

            float alpha = 0.5f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.5f;

            Main.spriteBatch.Draw(glorb, drawPos + new Vector2(0, 0f), null, glorbCol with { A = 0 } * 0.3f, NPC.rotation, glorb.Size() / 2f, glorbScale, SpriteEffects.None, 0f);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(BaseAnimWingGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                    Color.Goldenrod with { A = 0 } * 0.1f * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SpriteEffects.None);

                Main.EntitySpriteDraw(BaseAnimBodyGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle, 
                    Color.Goldenrod with { A = 0 } * 0.3f * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SpriteEffects.None);
            }


            Main.EntitySpriteDraw(MainTexture, drawPos, sourceRectangle, drawColor * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SpriteEffects.None);


            DrawWindDrill(1, drawColor * 0.8f); //0.7f

            return false;
        }


        Texture2D SmokeBorder => (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle");

        Effect smokeEffect = null;
        float smokeAlpha = 1f;
        public void DrawSmokyBorder(Vector2 drawPos, Rectangle source)
        {
            drawPos += new Vector2(-8f, 5f);

            if (smokeEffect == null)
                smokeEffect = ModContent.Request<Effect>("VFXPlus/Effects/Compiler/SmokyNoise", AssetRequestMode.ImmediateLoad).Value;

            Color smokeCol = Color.Lerp(Color.Orange, Color.OrangeRed, 0.25f);

            float alpha = 0.85f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.15f;

            smokeEffect.Parameters["zoom"].SetValue(2.0f);
            smokeEffect.Parameters["color"].SetValue(smokeCol.ToVector3() * alpha * 1f); //0f
            smokeEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.15f);

            Vector2 orbScale = new Vector2(0.65f, 1f) * overallScale * NPC.scale * 0.7f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, smokeEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(SmokeBorder, drawPos + new Vector2(0f, 0f), null, Color.White, NPC.rotation, SmokeBorder.Size() / 2f, orbScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }


        public float drillRotation = 0;
        public float drillAlpha = 1f;
        public bool drawDrill = true;
        public void DrawWindDrill(int yDir, Color color)
        {
            if (!drawDrill)
                return;

            FastRandom r = new("mule".GetHashCode());

            float speedTime = Main.GlobalTimeWrappedHourly * 1f;

            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/Bee").Value;
            Vector2 origin = dustTexture.Size() / 2f;
            Vector2 scale = new Vector2(1f, 1f) * 0.8f;

            float ringMinLength = 150f;
            float ringMaxLength = 20f;

            float particleMinSpeed = 1f;
            float particleMaxSpeed = 2.25f;

            float yHeightTop = -320f;
            float yHeightBottom = 20f;

            for (int i = 0; i < 0; i++) //160
            {
                //Rectangle frame = null;
                float rotation = drillRotation + speedTime * NextFloatF(r, 0.8f, 1.2f);

                float speed = NextFloatF(r, particleMinSpeed, particleMaxSpeed); //0.8 | 4f

                float randScale = NextFloatF(r, 0.99f, 1f);
                scale *= randScale;

                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float waveDistance = NextFloatF(r, ringMinLength, ringMaxLength) + 10f; //40 120 overall distance of the ring

                float time = speedTime * 2f;
                float min = 1f; //Controls how much the ring sways 
                float max = 1f; //^

                float yWave = min + ((float)Math.Sin(time) + 1f) / 2f * (max - min);

                float yOffset = scaleWave * waveDistance * 0.15f * yWave; //0.3 --> How wide is the gap of the ring ()
                float xWave = MathF.Sin(progress * MathHelper.Pi - MathHelper.PiOver2) * yDir;

                Vector2 posOffset = new Vector2(0f, 60f).RotatedBy(drillRotation);

                var drawPosition = NPC.Center + new Vector2(xWave * waveDistance, NextFloatF(r, yHeightTop, yHeightBottom) + yOffset * yDir).RotatedBy(drillRotation) + posOffset;

                //Makes the rings closer to tip smaller
                float ringDistProg = 1f - Utils.GetLerpValue(ringMinLength, ringMaxLength, waveDistance, true);
                float adjustedScale = 0.9f + (0.45f * ringDistProg);

                Main.spriteBatch.Draw(dustTexture, (drawPosition - Main.screenPosition).Floor(), null, color * drillAlpha, rotation - xWave / 3f * yWave * yDir, origin, 
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 1f, SpriteEffects.None, 0f);
            }
        }

        public void DrawBasicAfterImage(Vector2 drawPos, Rectangle source)
        {
            //Basic After-Image
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float prog = (float)i / (float)previousPositions.Count;

                Color between = Color.Lerp(Color.DarkGoldenrod, Color.Orange, 0.75f);

                Color col = between with { A = 0 } * Easings.easeInQuad(prog) * 0.2f;

                //float scaleFallOff = 


                Main.EntitySpriteDraw(BaseAnimWingGlow, previousPositions[i] - Main.screenPosition, source, col * overallAlpha * 0.5f, previousRotations[i], source.Size() / 2f, NPC.scale * overallScale, SpriteEffects.None);
                Main.EntitySpriteDraw(BaseAnimBodyGlow, previousPositions[i] - Main.screenPosition, source, col * overallAlpha, previousRotations[i], source.Size() / 2f, NPC.scale * overallScale, SpriteEffects.None);


                //Main.EntitySpriteDraw(BaseAnim, previousPositions[i] - Main.screenPosition, source, col * overallAlpha, previousRotations[i], source.Size() / 2f, NPC.scale * overallScale, SpriteEffects.None);
            }
        }

        public void DrawDashAfterImage()
        {
            //Flare After-Image
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;
            for (int i = 0; i < dashTrailPositions.Count; i++)
            {
                float progress = (float)i / (float)dashTrailPositions.Count;

                Vector2 offset1 = new Vector2(0f, -35f * progress * overallScale).RotatedBy(dashTrailRotations[i]);
                Vector2 offset2 = new Vector2(0f, 35f * progress * overallScale).RotatedBy(dashTrailRotations[i]);

                offset1 += Main.rand.NextVector2Circular(15f, 15f);
                offset2 += Main.rand.NextVector2Circular(15f, 15f);


                Vector2 flarePos = dashTrailPositions[i] - Main.screenPosition;

                Color col = Color.Lerp(Color.Blue, Color.DeepSkyBlue, progress) * overallAlpha;

                Vector2 lineScale = new Vector2(1.5f, 1.5f) * progress * overallScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, col with { A = 0 } * 0.45f * progress,
                    dashTrailRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Main.EntitySpriteDraw(line, flarePos + offset2, null, col with { A = 0 } * 0.45f * progress,
                    dashTrailRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Vector2 innerScale = new Vector2(1.5f, 1.5f * 0.1f) * progress * overallScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, Color.White with { A = 0 } * 0.6f * progress * overallAlpha,
                    dashTrailRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);

                Main.EntitySpriteDraw(line, flarePos + offset2, null, Color.White with { A = 0 } * 0.6f * progress * overallAlpha,
                    dashTrailRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
            }
        }


        public float NextFloatF(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }
    }
}
