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

            //ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            //{

            newDraw();
            
            /*
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
            myEffect.Parameters["squashValue"].SetValue(0.0f);
            myEffect.Parameters["colorIntensity"].SetValue(1.0f);


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

            //}, drawAdditive: true);
            */
            return false;
        }


        public void newDraw()
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
            myEffect.Parameters["squashValue"].SetValue(0.0f);
            myEffect.Parameters["colorIntensity"].SetValue(1.0f);


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
        }
    }

    public class SolsearBombExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        int timer = 0;
        public float opacity = 1f;
        public float size = 0.5f;
        public bool maxPower = false;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 200;
            Projectile.penetrate = -1;
            Projectile.scale = 0.1f;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        Vector2 startingCenter;
        public override void AI()
        {
            if (timer == 0)
            {
                startingCenter = Projectile.Center;
                timer = Main.rand.Next(0, 200);
            }

            timer++;

            Projectile.scale = MathHelper.Clamp(MathHelper.Lerp(Projectile.scale, 1.25f * size, 0.08f), 0f, 1.25f * size);

            if (Projectile.scale >= 0.8f * size)
                opacity = MathHelper.Clamp(MathHelper.Lerp(opacity, -0.2f, 0.15f), 0, 2);

            if (opacity <= 0)
                Projectile.active = false;

            Projectile.width = (int)(375 * Projectile.scale);
            Projectile.height = (int)(375 * Projectile.scale);
            Projectile.Center = startingCenter;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Orbs/ElectricPopDA").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/ElectricPopE").Value;

            float scale = Projectile.scale * 0.25f;
            float timeFade = 1f - (0.25f * (Projectile.scale / size));

            float timeA = timer * 0.045f * timeFade;
            float timeB = timer * -0.07f * timeFade;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                //Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Black * opacity * 0.35f, timeA, Tex.Size() / 2, scale * 1.65f, SpriteEffects.None, 0f);
                //Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Black * opacity * 0.35f, timeB, Tex.Size() / 2, scale * 1.65f + (0.15f * scale), SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.EffectMatrix);

                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Red * opacity, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.OrangeRed * opacity, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity * 1f, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), Color.OrangeRed * opacity * 1f, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity * 1f, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), Color.OrangeRed * opacity * 1f, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);


                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            });
            return false;
        }

    }

    public class OrbTests : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        int timer = 0;
        public float opacity = 1f;
        public float size = 1f;
        public bool maxPower = false;

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 200;
            Projectile.penetrate = -1;
            //Projectile.scale = 0.1f;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
        }

        Vector2 startingCenter;
        public override void AI()
        {
            //if (timer > 0)

            Projectile.velocity *= 0.96f;

            if (Projectile.velocity.Length() < 1.5f)
                Projectile.scale *= 0.98f;

            if (Projectile.scale < 0f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Orbs/anotheranotherorb").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/ElectricPopE").Value;

            float scale = Projectile.scale * 2f;

            Color innerCol = Color.White with { A = 0 };
            Color outerCol = Color.DodgerBlue with { A = 0 };


            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = Tex.Size() / 2f;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {

                Main.spriteBatch.Draw(Tex, drawPos, null, outerCol * 0.5f, 0f, origin, scale * 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex, drawPos, null, innerCol * 1f, 0f, origin, scale * 0.2f, SpriteEffects.None, 0f);

            });

            //Main.spriteBatch.Draw(Tex, drawPos, null, outerCol, 0f, origin, scale * 1f, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Tex, drawPos, null, innerCol, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f);

            return false;
        }

    }
}