using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Modules;
using Terraria.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;
using VFXPLus.Common;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace VFXPlus.Content.VFXTest
{
    public class TornadoTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 250; //180
            Projectile.extraUpdates = 1;
        }



        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        public override void AI()
        {
            Projectile.timeLeft++;

            if (timer == 0 || true)
            {
                int numberOfPoints = 50;
                float tornadoHeight = 200 * overallScale;

                trailPositions.Clear();
                trailRotations.Clear();

                Vector2 previousPos = Vector2.Zero;
                for (int i = 0; i < numberOfPoints; i++)
                {
                    float prog = (float)i / (float)(numberOfPoints - 1);

                    float offsetMult = Easings.easeOutQuad(GeneralUtilities.FadeLinear(prog, 0.4f, 0.6f));

                    float sinVal = (float)Math.Sin(timer * 0.02f) * 1f;

                    float offsetMult2 = Easings.easeInQuad(GeneralUtilities.FadeLinear(prog, 0.6f, 0.4f));

                    Vector2 offset = new Vector2(50f * offsetMult * sinVal * overallScale, -tornadoHeight * prog);
                    //offset.X += -150 * offsetMult2 * sinVal;

                    trailPositions.Add(Projectile.Center + offset);

                    if (i == 0 || true)
                        trailRotations.Add(-MathHelper.PiOver2);
                    else
                        trailRotations.Add((offset - previousPos).ToRotation());

                    previousPos = offset;
                }
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawVertexTrail(false);
            });
            DrawVertexTrail(true);

            return false;
        }

        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();
        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;


            bool posterize = false;


            Texture2D TrailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/OuterLavaTrailUp").Value; 
            Texture2D TrailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value; 

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/TornadoShader", AssetRequestMode.ImmediateLoad).Value;


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.02f;


            Color StripColor(float progress) => Color.White * overallAlpha * (1f - Utils.GetLerpValue(0.85f, 1f, progress, true));

            float StripWidth(float progress)
            {
                return Easings.easeOutQuad(progress) * 110f * overallScale;
            }


            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(trailPositions.ToArray(), trailRotations.ToArray(), StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            //Everybody say thank you lucille karma!!!
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            myEffect.Parameters["WorldViewProjection"].SetValue(view * projectionMatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //0.02
            myEffect.Parameters["fadeWidth"].SetValue(0.5f); //0.02

            myEffect.Parameters["TrailTexture1"].SetValue(TrailTexture1);
            myEffect.Parameters["tex1FlowSpeed"].SetValue(new Vector2(0.3f, -2.5f));
            myEffect.Parameters["tex1Zoom"].SetValue(new Vector2(2.0f, 0.63f));//2.0
            myEffect.Parameters["tex1Color"].SetValue(Color.LightSkyBlue.ToVector3() * 1.5f);
            myEffect.Parameters["tex1ColorMirrorMult"].SetValue(0.3f);



            myEffect.Parameters["TrailTexture2"].SetValue(TrailTexture2);
            myEffect.Parameters["tex2FlowSpeed"].SetValue(new Vector2(0.3f, -1.37f));
            myEffect.Parameters["tex2Zoom"].SetValue(new Vector2(2.0f, 0.63f));
            myEffect.Parameters["tex2Color"].SetValue(Color.SkyBlue.ToVector3() * 0.5f * 1.5f);
            myEffect.Parameters["tex2ColorMirrorMult"].SetValue(0.3f);

            myEffect.Parameters["sineTimeSpeed"].SetValue(2.0f); //0.02
            myEffect.Parameters["sineMult"].SetValue(1.0f); //0.02
            myEffect.Parameters["startingCurveAmount"].SetValue(0.0f); //0.02


            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }

    public class TornadoTestPosterized : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 250; //180
            Projectile.extraUpdates = 1;
        }



        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        public override void AI()
        {
            Projectile.timeLeft++;

            if (timer == 0 || true)
            {
                int numberOfPoints = 40;
                float tornadoHeight = 200 * overallScale; //250

                trailPositions.Clear();
                trailRotations.Clear();

                Vector2 previousPos = Vector2.Zero;
                for (int i = 0; i < numberOfPoints; i++)
                {
                    float prog = (float)i / (float)(numberOfPoints - 1);

                    float offsetMult = Easings.easeOutQuad(GeneralUtilities.FadeLinear(prog, 0.4f, 0.6f));

                    float sinVal = (float)Math.Sin(timer * 0.02f) * 1f;

                    float offsetMult2 = Easings.easeInQuad(GeneralUtilities.FadeLinear(prog, 0.6f, 0.4f));

                    Vector2 offset = new Vector2(35f * offsetMult * sinVal * overallScale, -tornadoHeight * prog);
                    //offset.X += -150 * offsetMult2 * sinVal;

                    trailPositions.Add(Projectile.Center + offset);

                    if (i == 0)
                        trailRotations.Add(-MathHelper.PiOver2);
                    else
                        trailRotations.Add((offset - previousPos).ToRotation());

                    previousPos = offset;
                }
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawVertexTrail(false);
            });
            DrawVertexTrail(true);

            return false;
        }

        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();
        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            bool posterize = false;


            Texture2D TrailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/OuterLavaTrailUp").Value;
            Texture2D TrailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/TornadoShaderPosterized", AssetRequestMode.ImmediateLoad).Value;


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.02f;


            Color StripColor(float progress) => Color.White * overallAlpha * (1f - Utils.GetLerpValue(0.85f, 1f, progress, true));

            float StripWidth(float progress)
            {
                return Easings.easeOutQuad(progress) * 85f * overallScale; //110
            }


            Color tornadoCol1 = Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, 1f);
            Color tornadoCol2 = Color.Lerp(Color.DeepSkyBlue, Color.DodgerBlue, 0.65f);// Color.DodgerBlue;// new Color(0.3f, 0.9f, 0.3f);

            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(trailPositions.ToArray(), trailRotations.ToArray(), StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            //Everybody say thank you lucille karma!!!
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            myEffect.Parameters["WorldViewProjection"].SetValue(view * projectionMatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.015f); //0.02
            myEffect.Parameters["fadeWidth"].SetValue(0.35f); //0.02

            myEffect.Parameters["TrailTexture1"].SetValue(TrailTexture1);
            myEffect.Parameters["tex1FlowSpeed"].SetValue(new Vector2(0.3f, -2.5f));
            myEffect.Parameters["tex1Zoom"].SetValue(new Vector2(2f, 0.63f));//2.0
            myEffect.Parameters["tex1Color"].SetValue(tornadoCol1.ToVector3() * 1.5f * 1f);
            myEffect.Parameters["tex1ColorMirrorMult"].SetValue(0.6f); //0.45



            myEffect.Parameters["TrailTexture2"].SetValue(TrailTexture2);
            myEffect.Parameters["tex2FlowSpeed"].SetValue(new Vector2(0.3f, -1.37f));
            myEffect.Parameters["tex2Zoom"].SetValue(new Vector2(2.0f, 0.3f));
            myEffect.Parameters["tex2Color"].SetValue(tornadoCol2.ToVector3() * 0.65f * 1f);
            myEffect.Parameters["tex2ColorMirrorMult"].SetValue(0.1f);

            myEffect.Parameters["sineTimeSpeed"].SetValue(2.0f);
            myEffect.Parameters["sineMult"].SetValue(1.0f);
            myEffect.Parameters["startingCurveAmount"].SetValue(0.0f);

            myEffect.Parameters["posterizationSteps"].SetValue(4.0f);
            myEffect.Parameters["hueShiftIntensity"].SetValue(0.0f);

            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }

    public class TornadoTestGreen : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 250; //180
            Projectile.extraUpdates = 1;
        }



        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        public override void AI()
        {
            Projectile.timeLeft++;

            if (timer == 0 || true)
            {
                int numberOfPoints = 20;
                float tornadoHeight = 220 * overallScale;

                trailPositions.Clear();
                trailRotations.Clear();

                Vector2 previousPos = Vector2.Zero;
                for (int i = 0; i < numberOfPoints; i++)
                {
                    float prog = (float)i / (float)(numberOfPoints - 1);

                    float offsetMult = Easings.easeOutQuad(GeneralUtilities.FadeLinear(prog, 0.4f, 0.6f));

                    float sinVal = (float)Math.Sin(timer * 0.02f) * 1f;

                    float offsetMult2 = Easings.easeInQuad(GeneralUtilities.FadeLinear(prog, 0.6f, 0.4f));

                    Vector2 offset = new Vector2(50f * offsetMult * sinVal * overallScale, -tornadoHeight * prog);
                    //offset.X += -150 * offsetMult2 * sinVal;

                    trailPositions.Add(Projectile.Center + offset);

                    if (i == 0)
                        trailRotations.Add(-MathHelper.PiOver2);
                    else
                        trailRotations.Add((offset - previousPos).ToRotation());

                    previousPos = offset;
                }
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawVertexTrail(false);
            });
            DrawVertexTrail(true);

            return false;
        }

        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();
        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;


            Texture2D TrailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/OuterLavaTrailUp").Value;
            Texture2D TrailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/TornadoShaderPosterized", AssetRequestMode.ImmediateLoad).Value;


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.02f;


            Color StripColor(float progress) => Color.White * overallAlpha * (1f - Utils.GetLerpValue(0.8f, 1f, progress, true));

            float StripWidth(float progress)
            {
                return Easings.easeOutQuad(progress) * 60f;
            }

            Color tornadoCol = new Color(0.3f, 0.9f, 0.3f);

            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(trailPositions.ToArray(), trailRotations.ToArray(), StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            //Everybody say thank you lucille karma!!!
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            myEffect.Parameters["WorldViewProjection"].SetValue(view * projectionMatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //0.02
            myEffect.Parameters["fadeWidth"].SetValue(0.35f); //0.02

            myEffect.Parameters["TrailTexture1"].SetValue(TrailTexture1);
            myEffect.Parameters["tex1FlowSpeed"].SetValue(new Vector2(0.3f, -2.5f));
            myEffect.Parameters["tex1Zoom"].SetValue(new Vector2(2f, 0.63f));//2.0
            myEffect.Parameters["tex1Color"].SetValue(tornadoCol.ToVector3() * 1.25f);
            myEffect.Parameters["tex1ColorMirrorMult"].SetValue(0.6f); //0.45



            myEffect.Parameters["TrailTexture2"].SetValue(TrailTexture2);
            myEffect.Parameters["tex2FlowSpeed"].SetValue(new Vector2(0.3f, -1.37f));
            myEffect.Parameters["tex2Zoom"].SetValue(new Vector2(2.0f, 0.3f));
            myEffect.Parameters["tex2Color"].SetValue(tornadoCol.ToVector3() * 0.65f);
            myEffect.Parameters["tex2ColorMirrorMult"].SetValue(0f);

            myEffect.Parameters["sineTimeSpeed"].SetValue(2.0f);
            myEffect.Parameters["sineMult"].SetValue(1.0f);
            myEffect.Parameters["startingCurveAmount"].SetValue(0.0f);

            myEffect.Parameters["posterizationSteps"].SetValue(6.0f);
            myEffect.Parameters["hueShiftIntensity"].SetValue(0.3f);

            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }

}