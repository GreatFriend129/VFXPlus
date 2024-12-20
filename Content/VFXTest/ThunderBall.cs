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

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Black * opacity * 0.35f, timeA, Tex.Size() / 2, scale * 1.65f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Tex, Projectile.Center - Main.screenPosition, Tex.Frame(1, 1, 0, 0), Color.Black * opacity * 0.35f, timeB, Tex.Size() / 2, scale * 1.65f + (0.15f * scale), SpriteEffects.None, 0f);

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

            float ProjScale = Projectile.scale * 1.5f;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                //GoddamnMonsoon(50);
                GoddamnMonsoonCirc(0); //50
                UniqueCircularOrbitBehaviorThing(50);
            });
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

            float minRange = 20f; //40f | 240 920 for full screen
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
                drawPosition += Main.rand.NextVector2Circular(5f, 5f);

                //Vector2 drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -20f, 14f) + yOffset * yDir); //-20 14 | -120

                //float prog = (float)i / (float)count - 1f;
                //Color col = Main.hslToRgb((prog + timer * 0.005f) % 1f, 1f, 0.7f, 0);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.Red with { A = 0 }, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 4f, SpriteEffects.None);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White with { A = 0 }, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 1.3f, SpriteEffects.None);
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

            float minRange = 40f; //40f | 240 920 for full screen
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
}