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
using Terraria.Utilities;
using Terraria.GameContent;
using Microsoft.Build.Evaluation;
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.Dusts;
using System.Collections.Generic;
using VFXPLus.Common;
using Terraria.Graphics;

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
            Projectile.scale = 1.5f;

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

            //newDraw();


            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;
            Texture2D Lightning = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle").Value;

                //ElectricRadialEffect
            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/foam_mask_bloom").Value); //foam_mask_bloom
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/ThunderGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Swirl").Value);
            myEffect.Parameters["flowSpeed"].SetValue(0.3f);
            myEffect.Parameters["distortStrength"].SetValue(0.1f); //0.1
            myEffect.Parameters["uTime"].SetValue(timer * 0.015f);
            myEffect.Parameters["colorIntensity"].SetValue(1.0f);


            ///Main.spriteBatch.End();
            ///Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            Color col = Color.DeepSkyBlue;
            //Main.spriteBatch.Draw(Ball, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation * -1, Ball.Size() / 2, 0.15f * ballScale * 2f * Projectile.scale, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Ball, Projectile.Center - Main.screenPosition, null, col * 0.8f, Projectile.rotation, Ball.Size() / 2, 0.2f * ballScale * 2f * Projectile.scale, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Ball, Projectile.Center - Main.screenPosition, null, col * 0.3f, Projectile.rotation * -1, Ball.Size() / 2, 0.3f * ballScale * 2f * Projectile.scale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            //Main.spriteBatch.Draw(Lightning, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), Projectile.rotation, Lightning.Size() / 2, 0.4f * Projectile.scale * (ballScale + 0.4f), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Lightning, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0), Projectile.rotation * -1f, Lightning.Size() / 2, 0.4f * Projectile.scale * 0.7f * (ballScale + 0.4f), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //}, drawAdditive: true);
            
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
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Need to restart twice for some reason
            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            //Main.pixelShader.CurrentTechnique.Passes[0].Apply();
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

            //Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Black * opacity * 0.35f, timeA, Tex.Size() / 2, scale * 1.65f, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Black * opacity * 0.35f, timeB, Tex.Size() / 2, scale * 1.65f + (0.15f * scale), SpriteEffects.None, 0f);

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                
                //Main.spriteBatch.End();
                //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.EffectMatrix);

                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Red * opacity, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.OrangeRed * opacity, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity * 1f, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), Color.OrangeRed * opacity * 1f, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), new Color(255, 130, 30) * opacity * 1f, timeA, Tex.Size() / 2, scale * 1.5f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex2, Projectile.Center - Main.screenPosition, Tex2.Frame(1, 1, 0, 0), Color.OrangeRed * opacity * 1f, timeB, Tex.Size() / 2, scale * 1.5f + (0.15f * scale), SpriteEffects.None, 0f);


                //Main.spriteBatch.End();
                //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
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
            Projectile.timeLeft = 22200;
            Projectile.penetrate = -1;
            //Projectile.scale = 0.1f;

            Projectile.alpha = 255;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
        }

        Vector2 startingCenter;
        public override void AI()
        {
            Projectile.alpha -= 5;
            if (Projectile.alpha < 0)
            {
                Projectile.alpha = 0;
            }
            if (Projectile.direction == 0)
            {
                Projectile.direction = Main.player[Projectile.owner].direction;
            }
            Projectile.rotation -= ((float)Projectile.direction * ((float)Math.PI * 2f) / 120f) * 1.5f;
            Projectile.scale = Projectile.Opacity;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.9f, 0.7f) * Projectile.Opacity);
            if (false && Main.rand.Next(2) == 0)
            {
                Vector2 vector59 = Vector2.UnitY.RotatedByRandom(6.2831854820251465);
                Dust dust199 = Main.dust[Dust.NewDust(Projectile.Center - vector59 * 30f, 0, 0, 229)];
                dust199.noGravity = true;
                dust199.position = Projectile.Center - vector59 * Main.rand.Next(10, 21);
                dust199.velocity = vector59.RotatedBy(1.5707963705062866) * 6f;
                dust199.scale = 0.5f + Main.rand.NextFloat();
                dust199.fadeIn = 0.5f;
                dust199.customData = Projectile.Center;
            }
            if (false && Main.rand.Next(2) == 0)
            {
                Vector2 vector60 = Vector2.UnitY.RotatedByRandom(6.2831854820251465);
                Dust dust200 = Main.dust[Dust.NewDust(Projectile.Center - vector60 * 30f, 0, 0, 240)];
                dust200.noGravity = true;
                dust200.position = Projectile.Center - vector60 * 30f;
                dust200.velocity = vector60.RotatedBy(-1.5707963705062866) * 3f;
                dust200.scale = 0.5f + Main.rand.NextFloat();
                dust200.fadeIn = 0.5f;
                dust200.customData = Projectile.Center;
            }
            if (Projectile.ai[0] < 0f)
            {
                Vector2 center11 = Projectile.Center;
                int num1024 = Dust.NewDust(center11 - Vector2.One * 8f, 16, 16, 229, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f);
                Dust dust31 = Main.dust[num1024];
                Dust dust212 = dust31;
                dust212.velocity *= 2f;
                Main.dust[num1024].noGravity = true;
                Main.dust[num1024].scale = Utils.SelectRandom<float>(Main.rand, 0.8f, 1.65f);
                Main.dust[num1024].customData = this;
            }

            if (Main.rand.Next(2) == 0)
            {
                Vector2 vector60 = Vector2.UnitY.RotatedByRandom(6.2831854820251465);
                Dust dust200 = Main.dust[Dust.NewDust(Projectile.Center - vector60 * 30f, 0, 0, 16)];
                dust200.noGravity = true;
                dust200.position = Projectile.Center - vector60 * 30f;
                dust200.velocity = vector60.RotatedBy(-1.5707963705062866) * 3f;
                dust200.scale = 0.5f + Main.rand.NextFloat();
                //dust200.fadeIn = 0.5f;
                //dust200.customData = Projectile.Center;
            }

            //Projectile.velocity = new Vector2(8f, 0f);

            //Looks cool, but makes the weapon a little messy, maybe use this for shadowflame knife and/or bow?
            if (timer % 1 == 0 && false)
            {
                Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a = Dust.NewDustPerfect(Projectile.Center + posOffset, ModContent.DustType<GlowPixelAlts>(), Velocity: initVel, newColor: new Color(42, 2, 82), Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.9f);

                a.velocity *= 0.25f;
                a.velocity += Projectile.velocity * 0.15f;


                Vector2 posOffset2 = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel2 = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a2 = Dust.NewDustPerfect(Projectile.Center + posOffset2 + (Projectile.velocity * 0.5f), ModContent.DustType<GlowPixelAlts>(), Velocity: initVel2, newColor: new Color(42, 2, 82), Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.9f);

                a2.velocity *= 0.25f;
                a2.velocity += Projectile.velocity * 0.15f;
            }

            //size = Math.Clamp(size - 0.032f, 0f, 1f);

            if (size == 0)
                Projectile.active = false;

            size = 1f;

            //Projectile.alpha = 0;
            //Projectile.rotation += 0.07f;
            
            timer++;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Extra_50").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Content/VFXTest/PinkSwirl").Value;

            //Texture2D Tex3 = ;

            Projectile.scale = 1.2f;
            float ProjScale = Projectile.scale * 1.5f;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                GoddamnMonsoon(0);
                GoddamnMonsoonCirc(250); //50
                //UniqueCircularOrbitBehaviorThing(50);
            });
            return false;

            //UniqueCircularOrbitBehaviorThing(30);
            //GoddamnMonsoonCirc(75);

            //return false;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color projectileColor = Lighting.GetColor((int)((double)Projectile.position.X + (double)Projectile.width * 0.5) / 16, (int)(((double)Projectile.position.Y + (double)Projectile.height * 0.5) / 16.0));
            Projectile proj = Projectile;

            Vector2 vector161 = proj.position + new Vector2(proj.width, proj.height) / 2f + Vector2.UnitY * proj.gfxOffY - Main.screenPosition;
            Texture2D value180 = TextureAssets.Projectile[617].Value; //641 = lunarportal
            Color color155 = proj.GetAlpha(projectileColor);
            Vector2 origin21 = new Vector2(value180.Width, value180.Height) / 2f;
            float num314 = proj.rotation;
            Vector2 vector162 = Vector2.One * proj.scale;
            Rectangle? sourceRectangle5 = null;
            
            if (proj.type == 578 || proj.type == 579 || proj.type == 641 || proj.type == 813 || true)
            {
                Color color158 = color155 * 0.8f;
                color158.A /= 2;
                Color color159 = Color.Lerp(color155, Color.Black, 0.5f);
                color159.A = color155.A;
                float num317 = 0.95f + (proj.rotation * 0.75f).ToRotationVector2().Y * 0.1f;
                color159 *= num317;
                float scale22 = 0.6f + proj.scale * 0.6f * num317;
                Texture2D value183 = TextureAssets.Extra[50].Value;
                bool flag36 = true;
                Vector2 origin24 = value183.Size() / 2f;
                Main.EntitySpriteDraw(value183, vector161, null, color159, 0f - num314 + 0.35f, origin24, scale22, SpriteEffects.FlipHorizontally);
                Main.EntitySpriteDraw(value183, vector161, null, color155, 0f - num314, origin24, proj.scale, SpriteEffects.FlipHorizontally);
                if (flag36)
                {
                    Main.EntitySpriteDraw(value180, vector161, null, color158, (0f - num314) * 0.7f, origin21, proj.scale, SpriteEffects.FlipHorizontally);
                    Main.EntitySpriteDraw(value180, vector161, null, color158, (0f - num314) * -0.7f, origin21, proj.scale, SpriteEffects.None);
                }
                Main.EntitySpriteDraw(value183, vector161, null, color155 with { A = 0 } * 0.8f, num314 * 0.5f, origin24, proj.scale * 0.9f, SpriteEffects.None);
                color155.A = 0;
            }

            return false;
        }


        public void GoddamnMonsoon(int count = 50)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/anotheranotherorb").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 0.25f;

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 70f; //120
            for (int i = 0; i < count; i++)
            {

                Texture2D texture = Tex;
                Rectangle frame = texture.Bounds;
                Vector2 scale = new Vector2(0.33f, 0.6f) * 0.4f;
                float rotation = 3.14f / 2f;
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f);
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance * scaleWave;
                drawPosition += Main.rand.NextVector2Circular(2f, 2f);

                //Vector2 drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -20f, 14f) + yOffset * yDir); //-20 14 | -120

                //float prog = (float)i / (float)count - 1f;
                //Color col = Main.hslToRgb((prog + timer * 0.005f) % 1f, 1f, 0.7f, 0);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.Red with { A = 0 }, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 4f, SpriteEffects.None);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White with { A = 0 } * 0.5f, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave, scale.Y * scaleWave) * 2f, SpriteEffects.None);
            }
        }

        public void GoddamnMonsoonCirc(int count = 50)
        {
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/AnotherLineGlow").Value;
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 2f;

            float minRange = 40f; //40f | 240 920 for full screen | 140 220
            float maxRange = 120f; //120
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.3f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.66f, 0.3f) * 0.55f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
                    rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f); //0.8f, 4f
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance;

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.LightSkyBlue with { A = 0 }, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);
            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }


        public void UniqueCircularOrbitBehaviorThing(int count)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/AnotherLineGlow").Value;

            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Twinkle").Value;

            //Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 0.5f;

            //240 420

            float minRange = 55f * size; //40f | 240 920 for full screen
            float maxRange = 160f * size; //120
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = 0f;
                if (r.NextFloat() < 0.2f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 1, frameY: r.Next(0));
                    scale = new Vector2(0.5f, 0.5f) * 0.35f;
                    //rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 1.8f, 4f); //0.8, 4f
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance * scaleWave;

                //Vector2 drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -20f, 14f) + yOffset * yDir); //-20 14 | -120

                //float prog = (float)i / (float)count - 1f;
                //Color col = Main.hslToRgb((prog + timer * 0.005f) % 1f, 1f, 0.7f, 0);

                //DeepSkyBlue3White1.5

                Color purp = Color.Lerp(Color.Purple, Color.DeepPink, 0.65f);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, purp with { A = 0 } * 3f, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White with { A = 0 }, randomRot + rotation + MathHelper.PiOver2, origin,
    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 1.85f, SpriteEffects.None);

            }
        }

        public void storedVariant1()
        {

        }
        public void storedVariant2()
        {

        }
        public void storedVariant3()
        {

        }
        public void storedVariant4()
        {

        }
        public void storedVariant5()
        {

        }
    }

    public class WindAnimTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public int timer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 30;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.timeLeft = 20000;
            Projectile.penetrate = -1;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {

            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 1)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            timer++;
        }

        float pulseVal = 0f;

        float alpha = 1f;
        float scale = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Smoke = Mod.Assets.Request<Texture2D>("Assets/Anim/Smoke30Frames").Value;


            int reverseFrame = (30 - Projectile.frame) - 1;

            int frameHeight = Smoke.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * reverseFrame;

            Rectangle sourceRectangle = new Rectangle(0, startY, Smoke.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Projectile.scale = 0.5f;



            Main.spriteBatch.Draw(Smoke, Projectile.Center - Main.screenPosition, sourceRectangle, Color.DeepSkyBlue with { A = 0 }, Projectile.rotation, origin, 1f * scale * Projectile.scale, SpriteEffects.FlipHorizontally, 0f);
            Main.spriteBatch.Draw(Smoke, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White with { A = 0 }, Projectile.rotation, origin, 0.6f * scale * Projectile.scale, SpriteEffects.FlipHorizontally, 0f);

            return false;
        }
    }

    public class OtherWindOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";



        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;


            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
        }

        float animProgress = 0;
        float alpha = 0f;
        float scale = 0f;

        int timer = 0;
        public int advancer = 0;
        public int startDir = 1;
        public float additionAmount = 0.1f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override void AI()
        {
            int trailCount = 7; //5
            previousRotations.Add(Projectile.rotation);
            previousPostions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            Projectile.rotation -= 0.25f;

            Projectile.velocity = new Vector2(0f, 0f);


            if (timer % 1 == 0)
            {
                float scale = 1.4f;

                SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 4f, 0.92f);

                Vector2 random = Main.rand.NextVector2CircularEdge(40f, 40f) * Main.rand.NextFloat(1f, 5f);

                Vector2 vel = random.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.25f;

                Dust star = Dust.NewDustPerfect(Projectile.Center + random, ModContent.DustType<SmallSmoke>(), vel, newColor: Color.White, Scale: scale);
                star.customData = ssb;
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVanillaSwirl(true);
                GoddamnMonsoonCirc(50); //460 | 50 | 10
                DrawVanillaSwirl2(true);
            });

            DrawVanillaSwirl2(false);

            return false;
        }

        public void DrawVanillaSwirl(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Extra_50").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Content/VFXTest/PinkSwirl").Value;
            Texture2D Tex3 = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirl").Value;

            float ProjScale = Projectile.scale * 1f;


            Texture2D TexA = Tex3;// Tex3;// TextureAssets.Projectile[641].Value;
            Texture2D TexB = Tex;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Vector2 TexAOrigin = TexA.Size() / 2f;
            Vector2 TexBOrigin = TexB.Size() / 2f;

            Color projectileColor = Lighting.GetColor((int)((double)Projectile.position.X + (double)Projectile.width * 0.5) / 16, (int)(((double)Projectile.position.Y + (double)Projectile.height * 0.5) / 16.0));
            Color color155 = Projectile.GetAlpha(projectileColor);

            float rot = Projectile.rotation;

            Color color158 = color155 * 0.8f;
            color158.A /= 2;

            Color color159 = Color.Lerp(color155, Color.Black, 0.5f);
            color159.A = color155.A;
            float num317 = 0.95f + (rot * 0.75f).ToRotationVector2().Y * 0.1f;
            color159 *= num317;
            float scale22 = 0.6f + ProjScale * 0.6f * num317;

            Main.EntitySpriteDraw(TexB, drawPos, null, color159, -rot + 0.35f, TexBOrigin, scale22, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(TexB, drawPos, null, color155, -rot, TexBOrigin, ProjScale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(TexA, drawPos, null, color158 with { A = 0 }, -rot * 0.7f, TexAOrigin, ProjScale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(TexA, drawPos, null, color158 with { A = 0 }, -rot * -0.7f, TexAOrigin, ProjScale, SpriteEffects.None);
            Main.EntitySpriteDraw(TexB, drawPos, null, color155 with { A = 0 } * 0.8f, rot * 0.5f, TexBOrigin, ProjScale * 0.9f, SpriteEffects.None);
            color155.A = 0;
        }

        public void DrawVanillaSwirl2(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Swirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirl").Value; //FireSpot goes kinda crazy| same with PixelSwirl

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 origin = Swirl.Size() / 2f;

            float sinOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.2f;
            float cosOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.077f) * 0.1f;

            float startScale = sinOff;
            float endScale = 7.47f * cosOff;

            float scale = 1f;

            Main.EntitySpriteDraw(Swirl, drawPos, null, Color.Black * 0.35f, (float)Main.timeForVisualEffects * 0.08f, origin, endScale * 0.25f, SpriteEffects.FlipHorizontally);

            for (int i = 0; i < 12; i++) //18
            {
                float prog = 1f - ((float)i / 12f);

                //prog = Easings.easeOutSine(prog);

                //End Color <--> Start color
                Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.95f);
                Color col = Color.Lerp(between * 1f, Color.LightSkyBlue, prog);

                //Color col = Color.Lerp(Color.DeepSkyBlue * 0.5f, Color.LightSkyBlue * 1f, prog);

                float alpha = prog;

                float newRot = (float)Main.timeForVisualEffects * 0.025f * scale; //(i % 2 == 0 ? 1f : -1f);

                //
                //if (i < 1)
                //Main.EntitySpriteDraw(Swirl, drawPos, null, Color.Black * alpha, newRot, origin, scale * 1f, SpriteEffects.FlipHorizontally);

                float newScale = MathHelper.Lerp(endScale, startScale, Easings.easeOutCubic(prog));

                Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha, newRot, origin, newScale * 1.25f, SpriteEffects.FlipHorizontally);




                //8
                if (i >= 8)
                    scale = scale * 1.25f;
                else
                    scale = scale * 1.15f;
            }

        }

        public void GoddamnMonsoonCirc2(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 2f;

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 1120f; //120
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.3f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
                    rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f);
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance;

                //Vector2 drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -20f, 14f) + yOffset * yDir); //-20 14 | -120

                //float prog = (float)i / (float)count - 1f;
                //Color col = Main.hslToRgb((prog + timer * 0.005f) % 1f, 1f, 0.7f, 0);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.Aquamarine with { A = 0 }, randomRot + rotation + MathHelper.PiOver2, origin,
    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

                //Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White, randomRot + rotation + MathHelper.PiOver2, origin,
                //new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public void GoddamnMonsoonCirc(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value; //PixelSwirl
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 0.1f;

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 220f; //120
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.3f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
                    rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f);
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance;

                //Vector2 drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -20f, 14f) + yOffset * yDir); //-20 14 | -120

                //float prog = (float)i / (float)count - 1f;
                //Color col = Main.hslToRgb((prog + timer * 0.005f) % 1f, 1f, 0.7f, 0);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.SkyBlue with { A = 0 } * 0.9f, randomRot + rotation + MathHelper.PiOver2, origin,
    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

                //Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White, randomRot + rotation + MathHelper.PiOver2, origin,
                //new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

    }

    public class FireBar : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }

        float alpha = 0f;
        float scale = 0f;

        int timer = 0;

        BaseTrailInfo trail1 = new BaseTrailInfo();
        public override void AI()
        {
            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/s06sBloom").Value;
            trail1.trailPointLimit = 265;
            trail1.trailWidth = 60;
            trail1.trailMaxLength = 265;
            trail1.timesToDraw = 1;
            trail1.shouldSmooth = false;
            trail1.pinch = false;

            trail1.pinchAmount = 0.5f;

            trail1.trailTime = (float)timer * 0.05f;
            trail1.trailColor = Color.OrangeRed;

            trail1.trailRot = Projectile.velocity.ToRotation();
            trail1.trailPos = Projectile.Center + Projectile.velocity;
            trail1.TrailLogic();

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(true);
            });

            DrawTrail(false);

            return false;
        }

        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp)
                return;

            trail1.trailColor = Color.OrangeRed;
            trail1.trailWidth = 60;
            trail1.TrailDrawing(Main.spriteBatch);
            trail1.TrailDrawing(Main.spriteBatch);


            trail1.trailColor = Color.LightGoldenrodYellow;
            trail1.trailWidth = 28;
            trail1.TrailDrawing(Main.spriteBatch);
        }

    }

    public class BurdenOfFleshBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

            Projectile.extraUpdates = 10;
        }

        float startAngle = 0f;
        float endAngle = 0f;
        int timer = 0;
        float width = 1f;

        float intensity = 0f;
        float zProg = 0f;

        float justStartIntensity = 1f;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        Vector2 origin;
        Vector2[] positions;
        float[] rotations;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            int state = 0;


            if (timer < 120 * 10) state = 1;
            else if (timer < 240 * 10) state = 2;
            else state = 3;


            if (state == 1)
            {
                float progress = Utils.GetLerpValue(0f, 1f, (float)timer / (100f * 10f), false);
                intensity = Easings.easeOutQuint(progress);

                if (timer == 0)
                {
                    origin = Projectile.Center;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                }

                Projectile.velocity = Vector2.Zero;

            }
            else if (state == 2)
            {
                float adjustedTime = timer - (120f * 10f);
                float zProgress = Math.Clamp(MathHelper.Lerp(0f, 1f, adjustedTime / (100f * 10f)), 0f, 1f);
                zProg = Easings.easeOutQuad(zProgress);

                origin = Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.rotation.ToRotationVector2() * 140, zProg);

            }
            else if (state == 3)
            {
                if (timer == 240 * 10)
                {
                    previousPositions = new List<Vector2>();
                    previousRotations = new List<float>();
                    Projectile.Center = origin;

                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * 20f;

                    Projectile.timeLeft = 3400;
                    justStartIntensity = 2f;


                    for (int i = 0; i < 100; i++)
                    {
                        Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                        Vector2 vel = Projectile.rotation.ToRotationVector2().RotatedByRandom(1.5f) * Main.rand.NextFloat(1f, 15f);
                        Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(1.5f, 3f));
                        d.fadeIn = Main.rand.Next(0, 4);
                        d.alpha = Main.rand.Next(0, 2);
                    }

                    SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Fire/SireFire") with { Pitch = 0.8f, PitchVariance = .12f, MaxInstances = -1, Volume = 0.55f };
                    SoundEngine.PlaySound(style, Projectile.Center);

                    SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Fire/FireBomb") with { Volume = .6f, Pitch = 0.35f, PitchVariance = .28f, MaxInstances = -1 };
                    SoundEngine.PlaySound(style2, Projectile.Center);

                    SoundStyle style3 = new SoundStyle("VFXPlus/Sounds/Effects/flame_thrower_airblast_rocket_redirect") with { Volume = .15f, Pitch = .19f, PitchVariance = 0.15f, MaxInstances = -1 };
                    SoundEngine.PlaySound(style3, Projectile.Center);


                    Main.player[Main.myPlayer].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = 60f;

                }

                if (timer < 250 * 10)
                {
                    previousPositions.Add(Projectile.Center);
                    previousRotations.Add(Projectile.velocity.ToRotation());

                    //Projectile.velocity = Projectile.velocity.RotatedBy(0.01f);

                    positions = previousPositions.ToArray();
                    rotations = previousRotations.ToArray();
                }
                else
                    Projectile.velocity = Vector2.Zero;

                if (timer % 3 == 0)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        float randomDustPercent = Main.rand.NextFloat(0f, 1f);
                        Vector2 dustPercentPoint = (origin - Projectile.Center) * randomDustPercent;
                        Vector2 dustVel = Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(4.5f, 6.1f) * 4f;
                        //new Color(255,155,190)
                        Dust d = Dust.NewDustPerfect(Projectile.Center + dustPercentPoint + Main.rand.NextVector2Circular(85f, 85f), ModContent.DustType<MuraLineBasic>(), dustVel, 100, Color.OrangeRed, Main.rand.NextFloat(0.45f, 0.65f) * 0.5f);
                        d.fadeIn = 1;

                    }
                }
            }

            justStartIntensity = Math.Clamp(MathHelper.Lerp(justStartIntensity, 0.8f, 0.01f), 1f, 10f);

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            //ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            //{
            //});

            Texture2D glow = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/EasyLightray").Value;
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Flare/Simple Lens Flare_11").Value;
            Texture2D star2 = Mod.Assets.Request<Texture2D>("Assets/Flare/flare_16").Value;

            Texture2D sigil = Mod.Assets.Request<Texture2D>("Assets/Orbs/whiteFireEyeA").Value;

            Vector2 newScale = new Vector2(2f, 1.5f) * 1f;

            bool drawLaser = (timer > 240 * 10f + 1f);
            float preLaserBoost = drawLaser ? 1f : 3f;
            float burstScale = 1f + (justStartIntensity * 0.2f);

            myEffect = null;
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertex", AssetRequestMode.ImmediateLoad).Value;

            if (drawLaser)
            {
                VertexStrip strip = new VertexStrip();
                strip.PrepareStripWithProceduralPadding(positions, rotations, StripColor, StripWidth, -Main.screenPosition, true);

                //Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition - new Vector2(0f, 0f), null, Color.Black * 0.35f, Projectile.rotation, glow.Size() / 2, newScale, SpriteEffects.None, 0f);
                ShaderParams();

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);
                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                //Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition - new Vector2(0f, 0f), null, Color.White, Projectile.rotation, glow.Size() / 2, newScale, SpriteEffects.None, 0f);

                strip.DrawTrail();
                strip.DrawTrail();
            }


            #region sigil
            Effect myEffect2 = ModContent.Request<Effect>("VFXPlus/Effects/Radial/Sigil", AssetRequestMode.ImmediateLoad).Value;
            myEffect2.Parameters["rotation"].SetValue((float)Main.timeForVisualEffects * 0.05f);
            myEffect2.Parameters["inputColor"].SetValue(new Color(255, 140, 10).ToVector3());
            myEffect2.Parameters["intensity"].SetValue(3.5f * intensity * MathF.Pow(justStartIntensity, 3) * preLaserBoost);
            myEffect2.Parameters["fadeStrength"].SetValue(0.5f);
            myEffect2.Parameters["glowThreshold"].SetValue(0.8f - (0.2f * (justStartIntensity - 1f)));

            float sin1 = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);
            float sin2 = MathF.Cos((float)Main.timeForVisualEffects * 0.06f);
            float sin3 = -MathF.Cos(((float)Main.timeForVisualEffects * 0.05f) / 2f) + 1f;

            Vector2 sigilScale1 = new Vector2(MathHelper.Lerp(1.2f, 0.2f, zProg), 1.2f) * burstScale;
            Vector2 sigilScale2 = sigilScale1 * (1.75f + (0.25f * sin1)) * burstScale;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect2, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(sigil, origin - Main.screenPosition, null, Color.Orange, Projectile.rotation, sigil.Size() / 2, sigilScale1, 0, 0f);
            Main.spriteBatch.Draw(sigil, origin - Main.screenPosition, null, Color.Orange, Projectile.rotation, sigil.Size() / 2, sigilScale1, 0, 0f);

            if (drawLaser)
                Main.spriteBatch.Draw(star, origin - Main.screenPosition + Projectile.rotation.ToRotationVector2() * (20f * sin3 * zProg), null, Color.Orange, Projectile.rotation, star.Size() / 2, sigilScale2, 0, 0f);

            if (drawLaser)
                Main.spriteBatch.Draw(star2, origin - Main.screenPosition, null, Color.Orange, Projectile.rotation, star2.Size() / 2, sigilScale1 * 1f, 0, 0f);

            #endregion

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public void ShaderParams()
        {
            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value); //ThinLineGlowClear
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            myEffect.Parameters["satPower"].SetValue(1f);

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/TextureLaser").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_06").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail5Loop").Value);

            Color c1 = new Color(255, 96, 40, 255);
            Color c2 = new Color(240, 79, 40, 255);
            Color c3 = new Color(255, 173, 40, 255);
            Color c4 = new Color(255, 149, 40, 255);

            myEffect.Parameters["Color1"].SetValue(c1.ToVector4());
            myEffect.Parameters["Color2"].SetValue(c2.ToVector4());
            myEffect.Parameters["Color3"].SetValue(c3.ToVector4());
            myEffect.Parameters["Color4"].SetValue(c4.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(1.25f);
            myEffect.Parameters["Color2Mult"].SetValue(1.5f);
            myEffect.Parameters["Color3Mult"].SetValue(1.15f);
            myEffect.Parameters["Color4Mult"].SetValue(1.5f);
            myEffect.Parameters["totalMult"].SetValue(1f * justStartIntensity);

            myEffect.Parameters["tex1reps"].SetValue(0.15f);
            myEffect.Parameters["tex2reps"].SetValue(0.15f);
            myEffect.Parameters["tex3reps"].SetValue(0.15f);
            myEffect.Parameters["tex4reps"].SetValue(0.15f);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.006f); //0.005
        }

        public Color StripColor(float progress)
        {
            Color color = Color.White * 0f;
            color.A = 0;
            return color;
        }
        public float StripWidth(float progress)
        {
            float size = 1f;//Utils.GetLerpValue(3400f, 2800f, Projectile.timeLeft, true) * Utils.GetLerpValue(0f, 200f, Projectile.timeLeft, true);
            float start = (float)Math.Cbrt(Utils.GetLerpValue(0f, 0.5f, progress, true));// Math.Clamp(1f * (float)Math.Pow(progress, 0.5f), 0f, 1f);
            float cap = (float)Math.Cbrt(Utils.GetLerpValue(1f, 0.95f, progress, true));
            return 330f * Easings.easeOutCirc(1f - (justStartIntensity - 1f));

        }
    }

}