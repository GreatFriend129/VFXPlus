using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;

namespace VFXPlus.Content.QueenBee
{
    public partial class ReduxQueenBee : ModNPC
    {
        Texture2D BaseAnim => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnim");
        Texture2D BaseAnimGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnimGlow");

        Texture2D BaseAnimBodyGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnimBodyGlow");
        Texture2D BaseAnimWingGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnimWingGlow");

        Texture2D DashAnim => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBDashingAnim");
        Texture2D DashAnimGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBDashingAnimGlow");


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
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
            {
                DrawDashAfterImage();
            });

            DrawOphaTelegraph();

            Utils.DrawBorderStringBig(spriteBatch, "Please ignore me, I am a side project", NPC.Center - screenPos + new Vector2(0f, -210f), Color.White, scale: 0.5f);
            Utils.DrawBorderStringBig(spriteBatch, "" + attackSpeed, NPC.Center - screenPos + new Vector2(0f, -110f), Color.White, scale: 1f);

            Vector2 drawPos = NPC.Center - Main.screenPosition;// + (Main.rand.NextVector2Circular(7f, 7f) * randomShakePower);

            Texture2D MainTexture = isDashing ? DashAnim : BaseAnim;

            int numberOfFrames = (isDashing ? 4 : 7);
            int currentFrame = (int)NPC.frameCounter % numberOfFrames;
            int frameHeight = MainTexture.Height / numberOfFrames;

            Rectangle sourceRectangle = new Rectangle(0, frameHeight * currentFrame, MainTexture.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            SpriteEffects SE = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            DrawBasicAfterImage(drawPos, sourceRectangle);

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
            {
                DrawSmokyBorder(drawPos, sourceRectangle);
            });

            Texture2D glorb = CommonTextures.feather_circle128PMA.Value;

            Vector2 glorbOffsetPos = isDashing ? new Vector2(0f, 20f) : new Vector2(0f, 0f);
            Vector2 glorbScale = isDashing ? new Vector2(0.95f, 0.65f) : new Vector2(0.65f, 0.95f);
            glorbScale *= overallScale * NPC.scale * 2f;

            Color glorbCol = Color.Lerp(Color.Orange, Color.OrangeRed, 0.25f);

            float alpha = 0.5f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.5f;

            Main.spriteBatch.Draw(glorb, drawPos + glorbOffsetPos, null, glorbCol with { A = 0 } * 0.3f, NPC.rotation, glorb.Size() / 2f, glorbScale, SE, 0f);

            //Border
            for (int i = 0; i < 4; i++)
            {
                if (isDashing)
                {
                    Main.EntitySpriteDraw(DashAnimGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle, 
                        Color.Goldenrod with { A = 0 } * 0.4f * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SE);
                }
                else
                {
                    Main.EntitySpriteDraw(BaseAnimWingGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle, 
                        Color.Goldenrod with { A = 0 } * 0.1f * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SE);

                    Main.EntitySpriteDraw(BaseAnimBodyGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                        Color.Goldenrod with { A = 0 } * 0.3f * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SE);
                }
            }


            Main.EntitySpriteDraw(MainTexture, drawPos, sourceRectangle, drawColor * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SE);


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


            Vector2 orbOffsetPos = isDashing ? new Vector2(0f, 20) : new Vector2(0f, 0f);

            Vector2 orbScale = isDashing ? new Vector2(1f, 0.65f): new Vector2(0.65f, 1f);
            orbScale *= overallScale * NPC.scale * 0.7f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, smokeEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(SmokeBorder, drawPos + orbOffsetPos, null, Color.White, NPC.rotation, SmokeBorder.Size() / 2f, orbScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }

        public void DrawBasicAfterImage(Vector2 drawPos, Rectangle source)
        {
            SpriteEffects SE = NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Basic After-Image
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float prog = (float)i / (float)previousPositions.Count;

                Color between = Color.Lerp(Color.DarkGoldenrod, Color.Orange, 0.75f);

                Color col = between with { A = 0 }; // * Easings.easeInQuad(prog) * 0.2f;

                if (isDashing)
                    col *= Easings.easeInQuad(prog) * 0.25f;
                else
                    col *= Easings.easeInQuad(prog) * 0.2f;

                if (isDashing) 
                {
                    Main.EntitySpriteDraw(DashAnimGlow, previousPositions[i] - Main.screenPosition, source, col * overallAlpha, previousRotations[i], source.Size() / 2f, NPC.scale * overallScale, SE);
                }
                else
                {
                    Main.EntitySpriteDraw(BaseAnimWingGlow, previousPositions[i] - Main.screenPosition, source, col * overallAlpha * 0.5f, previousRotations[i], source.Size() / 2f, NPC.scale * overallScale, SE);
                    Main.EntitySpriteDraw(BaseAnimBodyGlow, previousPositions[i] - Main.screenPosition, source, col * overallAlpha, previousRotations[i], source.Size() / 2f, NPC.scale * overallScale, SE);
                }
            }
        }

        public void DrawDashAfterImage()
        {
            //Flare After-Image
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;
            for (int i = 0; i < dashTrailPositions.Count; i++)
            {
                float progress = (float)i / (float)dashTrailPositions.Count;

                Vector2 offset1 = new Vector2(0f, -20f * progress * overallScale).RotatedBy(dashTrailRotations[i]);
                Vector2 offset2 = new Vector2(0f, 20f * progress * overallScale).RotatedBy(dashTrailRotations[i]);

                offset1 += Main.rand.NextVector2Circular(4f, 9f * progress);
                offset2 += Main.rand.NextVector2Circular(4f, 9f * progress);


                Vector2 flarePos = dashTrailPositions[i] - Main.screenPosition + new Vector2(0f, 20f);

                Color col = Color.Lerp(Color.DarkGoldenrod, Color.Orange, progress) * overallAlpha;

                Vector2 lineScale = new Vector2(2f, 1.5f) * overallScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, col with { A = 0 } * 0.45f * progress,
                    dashTrailRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Main.EntitySpriteDraw(line, flarePos + offset2, null, col with { A = 0 } * 0.45f * progress,
                    dashTrailRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Vector2 innerScale = new Vector2(2f, 1.5f * 0.1f) * overallScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, Color.White with { A = 0 } * 0.6f * progress * overallAlpha,
                    dashTrailRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);

                Main.EntitySpriteDraw(line, flarePos + offset2, null, Color.White with { A = 0 } * 0.6f * progress * overallAlpha,
                    dashTrailRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
            }
        }

        bool drawOphaLines = false;
        public void DrawOphaTelegraph()
        {
            if (!drawOphaLines)
                return;

            Vector2 startPos = NPC.Center;

            float spineCount = 6f;
            for (int j = 0; j < spineCount; j++)
            {
                float jProg = (float)j / spineCount;

                float rot = (MathHelper.TwoPi * jProg) + ophaOffset;

                Utils.DrawLine(Main.spriteBatch, startPos, startPos + rot.ToRotationVector2() * 600f, Color.Gold, Color.Orange * 0.1f, 4f);
            }

        }

        public float NextFloatF(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }
    }
}
