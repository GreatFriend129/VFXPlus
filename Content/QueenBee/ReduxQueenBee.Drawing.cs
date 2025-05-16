using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace VFXPlus.Content.QueenBee
{
    public partial class ReduxQueenBee : ModNPC
    {
        Texture2D BaseAnim => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnim");
        Texture2D BaseAnimGlow => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "QBBaseAnimGlow");

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
            //DrawWindDrill(-1, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.25f) * 0.35f); //33

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


            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(BaseAnimGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                    Color.Goldenrod with { A = 0 } * 0.2f * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SpriteEffects.None);
            }


            Main.EntitySpriteDraw(MainTexture, drawPos, sourceRectangle, drawColor * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SpriteEffects.None);


            //DrawWindDrill(1, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) * 1f); //0.7f

            return false;
        }


        public float drillRotation = MathHelper.PiOver2;
        public float drillAlpha = 0f;
        public bool drawDrill = false;
        public void DrawWindDrill(int yDir, Color color)
        {
            if (!drawDrill)
                return;

            FastRandom r = new("mule".GetHashCode());

            float speedTime = Main.GlobalTimeWrappedHourly * 1.25f;

            var windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            var dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            float ringMinLength = 145f; //20f 135
            float ringMaxLength = 2f; //115 //5

            for (int i = 0; i < 80; i++) //160
            {
                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = drillRotation + MathHelper.PiOver2;
                if (r.NextFloat() < 0.1f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new(0.11f, 0.25f); //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.45f, 0.45f);
                    rotation += speedTime * NextFloatF(r, 0.8f, 1.2f);
                }
                var origin = frame.Size() / 2f;
                float speed = NextFloatF(r, 0.8f, 4f);

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

                var drawPosition = NPC.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -160f, 40f) + yOffset * yDir).RotatedBy(drillRotation) + posOffset;

                //Makes the rings closer to tip smaller
                float ringDistProg = 1f - Utils.GetLerpValue(ringMinLength, ringMaxLength, waveDistance, true);
                float adjustedScale = 0.9f + (0.45f * ringDistProg);

                Main.spriteBatch.Draw(texture, (drawPosition - Main.screenPosition).Floor(), frame, Color.Black * 0.25f, rotation - xWave / 3f * yWave * yDir, origin, 
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 3.25f, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(
                    texture,
                    (drawPosition - Main.screenPosition).Floor(),
                    frame,
                    color with { A = 0 } * drillAlpha * 3f,
                    rotation - xWave / 3f * yWave * yDir,
                    origin,
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 3.25f,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        public void DrawBasicAfterImage(Vector2 drawPos, Rectangle source)
        {
            //Basic After-Image
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float prog = (float)i / (float)previousPositions.Count;

                Color between = Color.Lerp(Color.Gold, Color.DarkGoldenrod, 0.5f);

                Color col = between with { A = 0 } * Easings.easeInCubic(prog) * 0.2f;

                Main.EntitySpriteDraw(BaseAnim, previousPositions[i] - Main.screenPosition, source, col * overallAlpha, previousRotations[i], source.Size() / 2f, NPC.scale * overallScale, SpriteEffects.None);
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
