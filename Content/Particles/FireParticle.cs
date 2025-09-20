using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using VFXPlus.Common.Interfaces;
using VFXPlus.Content.Weapons.Ranged.Ammo.Bullets;
using VFXPlus.Common;
using ReLogic.Content;
using Microsoft.Xna.Framework;
using VFXPlus.Common.Drawing;
using System;


namespace VFXPlus.Content.Particles
{
    public class FireParticle : ShaderParticle
    {
        private float alphaFade = 0.92f * Main.rand.NextFloat(0.9f, 1f);
        private float velFade = 0.85f;
        private float rotPower = 0.02f;

        private float ColorMult = 1f;
        private Color myColor;
        private float BloomAlpha = 1f;
        public FireParticle(Vector2 position, Vector2 velocity, float scale, Color color, float colorMult = 1f, float bloomAlpha = 1f)
        {
            active = true;

            Center = position;
            Alpha = 1f;
            Velocity = velocity;
            Scale = scale;
            myColor = color;
            ColorMult = colorMult;
            BloomAlpha = bloomAlpha;
            Rotation = Main.rand.NextFloat(6.28f);
            myShader = VFXPlus.SmokeColShader;
        }

        public FireParticle(Vector2 position, Vector2 velocity, float scale, Color color, float colorMult = 1f, float bloomAlpha = 1f, 
            float AlphaFade = 0.92f, float VelFade = 0.85f, float RotPower = 0.02f)
        {
            active = true;

            Center = position;
            Alpha = 1f;
            Velocity = velocity;
            Scale = scale;
            myColor = color;
            ColorMult = colorMult;
            BloomAlpha = bloomAlpha;

            alphaFade = AlphaFade * Main.rand.NextFloat(0.9f, 1f);
            velFade = VelFade;
            rotPower = RotPower;

            Rotation = Main.rand.NextFloat(6.28f);
            myShader = VFXPlus.SmokeColShader;
        }

        public override void Update()
        {
            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((Timer + 10) / timeForPopInAnim, 0f, 1f);
            drawScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1f));

            
            Rotation += Velocity.X * 0.25f * rotPower * (Velocity.X > 0 ? 1f : -1f);
            Velocity *= velFade;

            //Fade more after a short while
            if (Timer >= 12)
                Alpha *= alphaFade;

            if (Alpha < 0.09f)
                Alpha *= alphaFade;
            Alpha *= alphaFade;

            if (Scale <= 0.2)
                Scale -= 0.02f;

            if (Alpha <= 0.03f)
                ShaderParticleHandler.RemoveParticle(this);

            //Main.NewText(Alpha);
        }

        private float drawScale = 0f;

        public override void Draw(SpriteBatch spriteBatch)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                Vector2 drawPos = Center - Main.screenPosition;

                Texture2D Ball = CommonTextures.feather_circle128PMA.Value;
                Main.spriteBatch.Draw(Ball, drawPos, null, myColor with { A = 0 } * Easings.easeInSine(Alpha) * 0.25f * BloomAlpha, Rotation, Ball.Size() / 2f, Scale * 0.435f, 0, 0f); //0.3
            });
        }

        int smokeTex = Main.rand.NextBool(3) ? 4 : 1;
        int maskTex = Main.rand.NextBool() ? 2 : 1;
        public override void DrawWithShader(SpriteBatch spriteBatch, Effect effect)
        {
            //ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            //{

                Texture2D Smoke = ModContent.Request<Texture2D>("VFXPlus/Assets/Smoke/WispSmoke" + smokeTex).Value; //spark_02 and smoke_02 also look cool
                Texture2D Mask = ModContent.Request<Texture2D>("VFXPlus/Assets/Smoke/InvertMask" + maskTex).Value;

                Vector2 drawPos = Center - Main.screenPosition;
                Vector2 TexOrigin = Smoke.Size() / 2f;
                SpriteEffects SE = SpriteEffects.None;

                float maskVal = 1f - Alpha;

                effect.Parameters["color"].SetValue(myColor.ToVector3() * 15f * Alpha * ColorMult);
                effect.Parameters["glowThreshold"].SetValue(0.8f); //0.9f
                effect.Parameters["glowPower"].SetValue(3.5f); //3.5
                effect.Parameters["fadeProgress"].SetValue(maskVal);
                effect.Parameters["endAlpha"].SetValue(1f);
                effect.Parameters["maskTexture"].SetValue(Mask);
                effect.CurrentTechnique.Passes[0].Apply();

                //spriteBatch.End();
                //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect, Main.GameViewMatrix.TransformationMatrix);

                spriteBatch.Draw(Smoke, drawPos, null, myColor, Rotation, TexOrigin, Scale * 0.075f, SE, 0f);


                //spriteBatch.End();
                //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
           // });
            
        }
    }
}
