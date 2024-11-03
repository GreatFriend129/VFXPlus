using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Microsoft.CodeAnalysis;
using Terraria.GameContent.Drawing;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;

namespace VFXPlus.Content.VFXTest
{
    public class ThunderBall : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; //false;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 800;
            Projectile.width = 20;
            Projectile.height = 20;
        }

        int timer = 0;
        public override void AI()
        {
            Projectile.scale = 0.75f;

            ballScale = 1 - ((float)Math.Sin((float)timer * 0.03f) * 0.12f);
            ballBright = (float)Math.Sin((float)timer * 0.02f);

            Projectile.rotation += 0.04f;
            Projectile.timeLeft = 2;

            //Projectile.velocity.Y += 0.05f;
            Projectile.velocity *= 0.92f;

            timer++;
        }

        float ballScale = 0;
        float ballBright = 0;
        Vector2 drawScale = Vector2.Zero;
        public override bool PreDraw(ref Color lightColor)
        {
            //THE SHADER IS FUCKED UP WHENEVER IT HAS TO DRAW BLACK
            //WHETHER IT IS FROM THE TEXTURE OR VIGNETTE
            //float vignetteSize = (float)Math.Abs(Math.Sin(Main.timeForVisualEffects * 0.01f));

            //Utils.DrawBorderString(Main.spriteBatch, "" + vignetteSize, Projectile.Center - Main.screenPosition + new Vector2(0f, -100f), Color.White);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;
                Texture2D Lightning = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;

                //ElectricRadialEffect
                Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/BoFIrisAlt", AssetRequestMode.ImmediateLoad).Value;

                myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/foam_mask_bloom_noblack").Value); //foam_mask_bloom
                myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/ThunderGrad").Value);
                myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Swirl").Value);



                myEffect.Parameters["flowSpeed"].SetValue(0.3f);
                myEffect.Parameters["vignetteSize"].SetValue(0.2f); //0.2
                myEffect.Parameters["vignetteBlend"].SetValue(1f); //1f
                myEffect.Parameters["distortStrength"].SetValue(0.1f); //0.1
                myEffect.Parameters["xOffset"].SetValue(0.0f);
                myEffect.Parameters["uTime"].SetValue(timer * 0.015f);


                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                Color col = Color.DeepSkyBlue;
                Main.spriteBatch.Draw(Ball, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation * -1, Ball.Size() / 2, 0.15f * ballScale * 2f * Projectile.scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Ball, Projectile.Center - Main.screenPosition, null, col * 0.8f, Projectile.rotation, Ball.Size() / 2, 0.2f * ballScale * 2f * Projectile.scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Ball, Projectile.Center - Main.screenPosition, null, col * 0.3f, Projectile.rotation * -1, Ball.Size() / 2, 0.3f * ballScale * 2f * Projectile.scale, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

                Main.spriteBatch.Draw(Lightning, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), Projectile.rotation, Lightning.Size() / 2, 0.4f * Projectile.scale * (ballScale + 0.4f), SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Lightning, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), Projectile.rotation * -1f, Lightning.Size() / 2, 0.4f * Projectile.scale * 0.7f * (ballScale + 0.4f), SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                //Need to restart twice for some reason
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            }, drawAdditive: true);

            return false;
        }
    }
}