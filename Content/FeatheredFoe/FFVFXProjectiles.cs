using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Dusts;
using VFXPlus.Common;
using Terraria.Utilities.Terraria.Utilities;

namespace VFXPlus.Content.FeatheredFoe
{
    public class WindBarrier : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 4400; //180
            Projectile.extraUpdates = 25;
        }


        float startAngle = 0f;
        float endAngle = 0f;
        int timer = 0;
        float width = 1f;

        public List<Vector2> previousPositions;
        public List<float> previousRotations;

        Vector2 origin;
        Vector2[] positions;
        float[] rotations;
        public override void AI()
        {
            if (timer == 0)
            {
                previousPositions = new List<Vector2>();
                previousRotations = new List<float>();
                origin = Projectile.Center;

                Projectile.timeLeft = 13400;
            }

            if (timer < 1400)
            {
                previousPositions.Add(Projectile.Center);
                previousRotations.Add(new Vector2(3f + (timer * 0.002f), 0f).RotatedBy((timer * 0.012f) + startAngle).ToRotation());

                Projectile.velocity = new Vector2(3f + (timer * 0.002f), 0f).RotatedBy((timer * 0.012f) + startAngle);

                positions = previousPositions.ToArray();
                rotations = previousRotations.ToArray();
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.extraUpdates = 20;
            }

            int mod = 10;
            if (timer % mod == 0)
            {
                int maxAdd = Projectile.timeLeft < 300 ? 50 : 100;
                for (int i = Main.rand.Next(0, 15); i < previousPositions.Count; i += Main.rand.Next(5, maxAdd))
                {
                    int randChance = 20;
                    if (Main.rand.NextBool(randChance))
                    {

                        float size = Main.rand.NextFloat(0.35f, 0.5f) * 0.5f;
                        Color col = Color.White;
                        Vector2 sideOffset = (previousRotations[i] + MathHelper.PiOver2).ToRotationVector2() * (Projectile.timeLeft < 300 ? Main.rand.NextFloat(-5, 5.01f) : Main.rand.NextFloat(-10, 10.1f));

                        float scale = 2f;

                        SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 5f, 0.92f);

                        Dust star = Dust.NewDustPerfect(previousPositions[i] + sideOffset, ModContent.DustType<SmallSmoke>(),
                        previousRotations[i].ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.75f, newColor: col, Scale: scale);
                        star.customData = ssb;

                    }
                }
            }

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            //Texture2D glow = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/circle_053").Value;
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexFade", AssetRequestMode.ImmediateLoad).Value;

            VertexStrip strip = new VertexStrip();
            strip.PrepareStripWithProceduralPadding(positions, rotations, StripColor, StripWidth, -Main.screenPosition, true);
            ShaderParams();

            //myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //strip.DrawTrail();
            //Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                strip.DrawTrail();

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            });


            return false;
        }

        public void ShaderParams()
        {
            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/WackBeam").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.Gray.ToVector3() * 0f);
            myEffect.Parameters["satPower"].SetValue(0f); //0.25

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail7").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/FlameTrail").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);


            Color c1 = Color.DimGray * 0.5f;
            Color c2 = Color.LightSlateGray * 0.5f;
            Color c3 = Color.DimGray * 0.5f;
            Color c4 = Color.DimGray * 0.5f;

            myEffect.Parameters["Color1"].SetValue(c1.ToVector4());
            myEffect.Parameters["Color2"].SetValue(c2.ToVector4());
            myEffect.Parameters["Color3"].SetValue(c3.ToVector4());
            myEffect.Parameters["Color4"].SetValue(c4.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(0f);
            myEffect.Parameters["Color2Mult"].SetValue(0.5f); //0.5 ^-vv
            myEffect.Parameters["Color3Mult"].SetValue(0f);
            myEffect.Parameters["Color4Mult"].SetValue(0.25f);
            myEffect.Parameters["totalMult"].SetValue(0.75f);

            myEffect.Parameters["tex1reps"].SetValue(4f);
            myEffect.Parameters["tex2reps"].SetValue(4f);
            myEffect.Parameters["tex3reps"].SetValue(4f);
            myEffect.Parameters["tex4reps"].SetValue(4f);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.017f * 0.8f);

        }
        public Color StripColor(float progress)
        {
            Color color = Color.White * 0f;
            color.A = 0;
            return color;
        }
        public float StripWidth(float progress)
        {
            float size = Utils.GetLerpValue(13400f, 12800f, Projectile.timeLeft, true) * Utils.GetLerpValue(0f, 200f, Projectile.timeLeft, true);
            float start = Math.Clamp(1.5f * (float)Math.Pow(progress, 0.5f), 0f, 1f);
            float cap = (float)Math.Cbrt(Utils.GetLerpValue(1f, 0.85f, progress, true));

            //!Very potential source of lag! 
            float sineScale = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.14f) * 0.08f; 

            return start * size * 125f * cap * sineScale;// * (1.1f + (float)Math.Cos(timer) * (0.08f - progress * 0.06f));

        }
    }

    public class UmbrellaTelegraph : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 224400; //180
            Projectile.extraUpdates = 0;
        }


        
        int timer = 0;
        float overallWidth = 1f;
        float overallAlpha = 1f;
        float initialProg = 0f;

        public override void AI()
        {

            int timeForInitialAnim = 30;

            float initProg = Math.Clamp((float)timer / (float)timeForInitialAnim, 0f, 1f);

            initialProg = initProg;


            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //ignore the fact this is in a separate function
            DrawWalls(false);
            return false;
        }

        public void DrawWalls(bool giveUp = false)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D glow = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_06").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06
            Texture2D glowThick = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06


            int leftX = (int)MathHelper.Lerp(-1000, -100, Easings.easeOutQuart(initialProg));// -100;
            int rightX = (int)MathHelper.Lerp(1000, 100, Easings.easeOutQuart(initialProg));//100;


            int wallWidth = (int)MathHelper.Lerp(2300f, 120f, Easings.easeOutSine(initialProg));//120;
            int wallHeight = 2000;
            Rectangle glowFrame = new Rectangle(0, 0, glow.Width, glow.Height / 2);
            Rectangle glowThickFrame = new Rectangle(0, 0, glowThick.Width, glowThick.Height / 2);


            //Left Wall---------------------------------------
            Rectangle glowTarget = new Rectangle((int)drawPos.X + leftX - wallWidth, (int)drawPos.Y, wallHeight, wallWidth);

            Main.spriteBatch.Draw(glowThick, glowTarget, glowThickFrame, Color.DeepSkyBlue with { A = 0 } * 0.25f, MathHelper.PiOver2, glowThick.Size() / 2f, SpriteEffects.FlipVertically, 0);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.DeepSkyBlue with { A = 0 } * 2f, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);

            //White
            glowTarget = new Rectangle((int)drawPos.X + leftX - (wallWidth / 3) - 2, (int)drawPos.Y, wallHeight, wallWidth / 3);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.White with { A = 0 } * 2f, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);


            //Right Wall--------------------------------------------------
            glowTarget = new Rectangle((int)drawPos.X + rightX, (int)drawPos.Y, wallHeight, wallWidth);

            Main.spriteBatch.Draw(glowThick, glowTarget, glowThickFrame, Color.DeepSkyBlue with { A = 0 } * 0.25f, MathHelper.PiOver2, glowThick.Size() / 2f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.DodgerBlue with { A = 0 } * 1.5f, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.None, 0);

            //White
            glowTarget = new Rectangle((int)drawPos.X + rightX + 2, (int)drawPos.Y, wallHeight, wallWidth / 3);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.White with { A = 0 } * 2f, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.None, 0);



        }

    }

    public class WindPulse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public float intensity = 1f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 9000;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = Projectile.hostile = false;

            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 800;
        }

        public override bool? CanDamage() => false;
    
        int timer = 0;
        public float scale = 0.25f;
        float alpha = 1;


        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            if (timer == 0)
            {
                Projectile.ai[0] = 0;
                Projectile.rotation = Main.rand.NextFloat(6.28f);
            }

            if (timer <= 40)
            {
                scale = MathHelper.Lerp(0f, 1f, Easings.easeOutQuint(timer / 40f));
            }

            if (timer >= 0)
            {

                if (timer >= 6)
                    alpha -= 0.065f;
            }

            Projectile.timeLeft = 2;

            if (alpha <= 0)
            {
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawShit(false);
            });

            DrawShit(true);

            return false;
        }

        public void DrawShit(bool giveUp = false)
        {
            if (giveUp)
                return;

            //String toAsset = "Assets/Orbs/whiteFireEyeA";

            String toAsset = "Assets/Orbs/ElectricPopC"; //circle_05

            //if (special) toAsset = "Assets/Orbs/ElectricPopC";

            Texture2D Flare = Mod.Assets.Request<Texture2D>(toAsset).Value;

            float rot = ((float)Main.timeForVisualEffects * 0.12f * Projectile.ai[0]) + Projectile.rotation;
            float scale2 = scale * 0.25f;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/BoFIrisAlt", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/T_Lu_Noise_30").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/BarelyBlueGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/T_Lu_Noise_30").Value);

            myEffect.Parameters["flowSpeed"].SetValue(0.3f);
            myEffect.Parameters["vignetteSize"].SetValue(1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.8f);
            myEffect.Parameters["distortStrength"].SetValue(0.06f);
            myEffect.Parameters["xOffset"].SetValue(0.0f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.01f);
            myEffect.Parameters["colorIntensity"].SetValue(alpha * 1f * intensity);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            myEffect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        }
    }

}