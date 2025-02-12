using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Steamworks;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace VFXPlus.Content.FeatheredFoe
{
    public partial class FeatheredFoe : ModNPC
    {
        Texture2D NPCTexture => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "FeatheredFoe");
        Texture2D BorderTexture => (Texture2D)ModContent.Request<Texture2D>(AssetDirectory + "FeatheredFoeBorder");


        public float randomShakePower = 0f;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        float stretchIntensity = 0f;
        float squashAmount = 0f;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (windOverlayOpacity > 0.05f)
                DrawScrollOverlay();

            DrawWindDrill(-1, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.85f) * 0.45f); //33

            Vector2 drawPos = NPC.Center - Main.screenPosition + (Main.rand.NextVector2Circular(7f, 7f) * randomShakePower);
            Vector2 origin = NPCTexture.Size() / 2;

            Main.EntitySpriteDraw(NPCTexture, drawPos, null, drawColor * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SpriteEffects.None);

            for (int i = 0; i < 5; i++)
            {
                Color col = Color.DeepSkyBlue;
                Main.EntitySpriteDraw(BorderTexture, drawPos + Main.rand.NextVector2Circular(3f, 3f), null, col with { A = 0 } * overallAlpha, NPC.rotation, origin, NPC.scale * overallScale, SpriteEffects.None);
            }

            //Utils.DrawBorderString(Main.spriteBatch, "" + windOverlayOpacity, drawPos, Color.Black);

            DrawTriStar();

            DrawWindDrill(1, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.85f) * 1.25f); //0.7f

            return false;
        }


        float windOverlayOpacityGoal = 0f;
        float windOverlayOpacity = 0f;
        float windOverlayRotation = 0f;

        Effect ScrollEffect = null;
        Texture2D ScrollTex1 = null;
        Texture2D ScrollTex2 = null;
        public void DrawScrollOverlay()
        {
            ScrollEffect ??= ModContent.Request<Effect>("VFXPlus/Effects/Scroll/CheapScroll", AssetRequestMode.ImmediateLoad).Value;
            ScrollTex1 = Mod.Assets.Request<Texture2D>("Assets/Noise/CoolNoise").Value;
            ScrollTex2 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value;

            #region ScrollEffect Parameters
            ScrollEffect.Parameters["sampleTexture1"].SetValue(ScrollTex1);
            ScrollEffect.Parameters["sampleTexture2"].SetValue(ScrollTex2);

            ScrollEffect.Parameters["Color1"].SetValue(Color.LightSkyBlue.ToVector4());
            ScrollEffect.Parameters["Color2"].SetValue(Color.White.ToVector4()); //Dont ask idk why OrangeRed for this looks best
            ScrollEffect.Parameters["Color1Mult"].SetValue(0.25f);
            ScrollEffect.Parameters["Color2Mult"].SetValue(0.25f);
            ScrollEffect.Parameters["totalMult"].SetValue(1f * windOverlayOpacity);

            ScrollEffect.Parameters["tex1reps"].SetValue(0.25f);
            ScrollEffect.Parameters["tex2reps"].SetValue(0.25f);
            ScrollEffect.Parameters["satPower"].SetValue(1f);
            ScrollEffect.Parameters["time1Mult"].SetValue(1f);
            ScrollEffect.Parameters["time2Mult"].SetValue(1f);
            ScrollEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.01f);
            #endregion

            Texture2D pixel = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;
            Vector2 scale = new Vector2(Main.screenWidth * 1.25f, Main.screenHeight * 2f) / pixel.Size() * 1.1f;
            Vector2 drawPosition = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            ScrollEffect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(pixel, drawPosition, null, Color.White, windOverlayRotation, pixel.Size() * 0.5f, scale, 0, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public void DrawTriStar()
        {
            if (drawTriSpinStar)
            {
                Vector2 drawPos = NPC.Center - Main.screenPosition;

                Texture2D triStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/Medusa_Gray").Value;
                Vector2 triStarOrigin = new Vector2(0f, triStar.Height / 2f);


                float starRot1 = triSpinStarAngle;
                float starRot2 = starRot1 + (MathHelper.TwoPi * 0.33f);
                float starRot3 = starRot1 + (MathHelper.TwoPi * 0.66f);
                Vector2 triStarScale = new Vector2(0.4f, 1.2f * justShotTristarPower) * 2f;

                Main.EntitySpriteDraw(triStar, drawPos, null, Color.Black * 0.25f, starRot1, new Vector2(0f, triStar.Height / 2f), triStarScale, SpriteEffects.None);
                Main.EntitySpriteDraw(triStar, drawPos, null, Color.DodgerBlue with { A = 0 }, starRot1, new Vector2(0f, triStar.Height / 2f), triStarScale, SpriteEffects.None);
                Main.EntitySpriteDraw(triStar, drawPos, null, Color.LightSkyBlue with { A = 0 }, starRot1, new Vector2(0f, triStar.Height / 2f), triStarScale * 0.4f, SpriteEffects.None);

                Main.EntitySpriteDraw(triStar, drawPos, null, Color.Black * 0.25f, starRot2, new Vector2(0f, triStar.Height / 2f), triStarScale, SpriteEffects.None);
                Main.EntitySpriteDraw(triStar, drawPos, null, Color.DodgerBlue with { A = 0 }, starRot2, new Vector2(0f, triStar.Height / 2f), triStarScale, SpriteEffects.None);
                Main.EntitySpriteDraw(triStar, drawPos, null, Color.LightSkyBlue with { A = 0 }, starRot2, new Vector2(0f, triStar.Height / 2f), triStarScale * 0.4f, SpriteEffects.None);

                Main.EntitySpriteDraw(triStar, drawPos, null, Color.Black * 0.25f, starRot3, new Vector2(0f, triStar.Height / 2f), triStarScale, SpriteEffects.None);
                Main.EntitySpriteDraw(triStar, drawPos, null, Color.DodgerBlue with { A = 0 }, starRot3, new Vector2(0f, triStar.Height / 2f), triStarScale, SpriteEffects.None);
                Main.EntitySpriteDraw(triStar, drawPos, null, Color.LightSkyBlue with { A = 0 }, starRot3, new Vector2(0f, triStar.Height / 2f), triStarScale * 0.4f, SpriteEffects.None);
            }
        }


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
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.1f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new(0.15f, 0.33f); //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
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
                var drawPosition = NPC.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -160f, 40f) + yOffset * yDir) + new Vector2(0f, 60f); //Vertical distance of the ring

                //Makes the rings closer to tip smaller
                float ringDistProg = 1f - Utils.GetLerpValue(ringMinLength, ringMaxLength, waveDistance, true);
                float adjustedScale = 0.9f + (0.45f * ringDistProg);

                Main.spriteBatch.Draw(texture, (drawPosition - Main.screenPosition).Floor(), frame, Color.Black * 0.25f, rotation - xWave / 3f * yWave * yDir, origin, 
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 3.25f, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(
                    texture,
                    (drawPosition - Main.screenPosition).Floor(),
                    frame,
                    color with { A = 0 } * drillAlpha * 4f,
                    rotation - xWave / 3f * yWave * yDir,
                    origin,
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 3.25f,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        public float NextFloatF(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }
    }
}
