using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
namespace VFXPlus.Content.VFXTest
{
    public class ConstellationTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private class Constellation
        {
            public Vector2 position;
            public Vector2 velocity;
            public Color color;

            public List<int> connectingIndices = new List<int>();

            public void Update()
            {
                velocity *= 0.97f;
                position += velocity;
            }

            public Constellation(Vector2 position, Vector2 velocity, Color color)
            {
                this.position = position;
                this.velocity = velocity;
                this.color = color;
            }
        }

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
        }



        int timer = 0;
        float overallAlpha = 1f;
        float overallScale = 1f;

        List<Constellation> constellations = new List<Constellation>();
        public override void AI()
        {
            if (timer == 0)
            {
                Vector2 randomDir = Main.rand.NextVector2CircularEdge(8f, 8f);

                Vector2 randomDir1 = Main.rand.NextVector2CircularEdge(8f, 8f); //16
                Vector2 randomDir2 = Main.rand.NextVector2CircularEdge(8f, 8f);
                Vector2 randomDir3 = Main.rand.NextVector2CircularEdge(16f, 16f);

                //Constellation A = new Constellation(Projectile.Center, randomDir + Main.rand.NextVector2CircularEdge(2f, 2f), Color.Gold);
                //Constellation B = new Constellation(Projectile.Center, randomDir + Main.rand.NextVector2CircularEdge(2f, 2f), Color.Gold);
                //Constellation C = new Constellation(Projectile.Center, randomDir + Main.rand.NextVector2CircularEdge(2f, 2f), Color.Gold);

                Constellation A = new Constellation(Projectile.Center + randomDir1 * Main.rand.NextFloat(1f, 3f), randomDir1 * 0.1f, Color.Gold);
                Constellation B = new Constellation(Projectile.Center + randomDir2 * Main.rand.NextFloat(1f, 3f), randomDir2 * 0.1f, Color.Gold);
                Constellation C = new Constellation(Projectile.Center + randomDir3 * Main.rand.NextFloat(1f, 3f), Vector2.Zero, Color.Gold);

                constellations.Add(A);
                constellations.Add(B);
                //constellations.Add(C);

                A.connectingIndices.Add(constellations.IndexOf(B));
                //B.connectingIndices.Add(constellations.IndexOf(C));
            }

            foreach (Constellation c in constellations)
            {
                c.Update();
            }

            if (timer == 30)
                Projectile.active = false;

            float fadeInTime = Math.Clamp((timer + 12f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 2f);

            float fadeInTimeAlpha = Math.Clamp((timer - 15) / 15f, 0f, 1f);
            overallAlpha = 1f - Easings.easeInCubic(fadeInTimeAlpha);

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Pixel = CommonTextures.CrispStarPMA.Value;// Mod.Assets.Request<Texture2D>("Assets/Pixel/ConstellationPixel").Value;
            Texture2D Line = Mod.Assets.Request<Texture2D>("Assets/Trails/Clear/BasicGlowSliver").Value;

            Color betweenPink = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f); //bp/white
            Color golden = Color.Lerp(Color.Gold, Color.Orange, 0.75f); //bp/white

            //Draw connections
            foreach (Constellation c in constellations)
            {
                Vector2 drawPos = c.position - Main.screenPosition;

                foreach (int connection in c.connectingIndices)
                {
                    Vector2 endPos = constellations[connection].position - Main.screenPosition;

                    Vector2 between = (endPos - drawPos);
                    Vector2 scale = new Vector2(between.Length(), 0.65f * overallScale);

                    Main.spriteBatch.Draw(Line, drawPos, null, golden with { A = 150 } * overallAlpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(Line, drawPos, null, Color.White with { A = 150 } * overallAlpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), new Vector2(scale.X, scale.Y * 0.25f), SpriteEffects.None, 0f);
                }

            }

            //Draw pixels
            foreach (Constellation c in constellations)
            {
                Vector2 drawPos = c.position - Main.screenPosition;


                Main.spriteBatch.Draw(Pixel, drawPos, null, golden with { A = 160 } * overallAlpha, 0f, Pixel.Size() / 2f, 0.5f * overallScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Pixel, drawPos, null, Color.White with { A = 160 } * overallAlpha, 0f, Pixel.Size() / 2f, 0.25f * overallScale, SpriteEffects.None, 0f);
            }

            return false;
        }

    }


    public class BasicGlowTrailTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 1000;

            Projectile.width = Projectile.height = 10;
        }

        int timer = 0;

        float justShotPower = 1f;

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            int trailCount = 60;
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * Projectile.velocity.Length(), 0.15f);

            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20;

            timer++;
        }

        Effect trailEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            if (timer == 0)
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);

            return false;
        }

        float trailWidth = 1f;
        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp)
                return;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/BasicGlowTrail", AssetRequestMode.ImmediateLoad).Value;


            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value; //OuterLavaTrail with 10f mult |Extra_196_Black

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color between = Color.Lerp(Color.Aqua, Color.Blue, 0.15f);

            //new Color(255, 45, 0), new Color(255, 100, 15) | Color.Blue, Color.DodgerBlue

            //Blue DodgerBlue
            Color StripColor(float progress) => Color.Lerp(Color.Blue, Color.DodgerBlue, progress) with { A = 200 } * 1f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.05f;

                /*
                float toReturn = 0f;
                if (progress < 0.5f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = 1f;// Easings.easeOutSine(1f - LV);
                }
                */

                
                float toReturn = 0f;
                if (progress < 0.85f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.85f, progress, true);
                    toReturn = Easings.easeInSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.85f, 1f, progress, true);
                    toReturn = Easings.easeOutSine(1f - LV);
                }

                return 125f * toReturn * sineWidthMult;
                return 50;// toReturn * sineWidthMult * trailWidth * 15f; //50 25
            }




            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.05f);

            trailEffect.Parameters["bodyIntensity"].SetValue(1f);
            trailEffect.Parameters["bodyPower"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(0f);

            trailEffect.Parameters["whiteGlowSize"].SetValue(0.5f);
            trailEffect.Parameters["whiteGlowPower"].SetValue(2f);


            trailEffect.Parameters["TrailTexture1"].SetValue(trailTexture1);
            trailEffect.Parameters["tex1reps"].SetValue(2f);
            trailEffect.Parameters["tex1Intensity"].SetValue(2f); //2f

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(1f);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


    }

    public class StarTrailTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 1000;

            Projectile.width = Projectile.height = 10;
        }

        int timer = 0;

        float justShotPower = 1f;

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            int trailCount = 14;
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * Projectile.velocity.Length(), 0.15f);

            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20;

            timer++;
        }

        Effect trailEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            if (timer == 0)
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);

            return false;
        }

        float trailWidth = 1f;
        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp)
                return;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/BasicGlowTrail", AssetRequestMode.ImmediateLoad).Value;


            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value; //OuterLavaTrail with 10f mult |Extra_196_Black

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color between = Color.Lerp(Color.Aqua, Color.Blue, 0.15f);

            //new Color(255, 45, 0), new Color(255, 100, 15) | Color.Blue, Color.DodgerBlue

            Color darkerPink = new Color(255, 61, 158);

            //Blue DodgerBlue
            Color StripColor(float progress) => Color.Lerp(Color.DeepPink, darkerPink, progress) with { A = 0 } * 1f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.1f;

                /*
                float toReturn = 0f;
                if (progress < 0.5f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = 1f;// Easings.easeOutSine(1f - LV);
                }
                */


                float toReturn = 0f;
                if (progress < 0.85f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.85f, progress, true);
                    toReturn = Easings.easeInSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.85f, 1f, progress, true);
                    toReturn = Easings.easeOutSine(1f - LV);
                }

                toReturn = Easings.easeInQuad(progress);

                if (toReturn < 0.05f)
                    toReturn = 0f;

                return 40f * sineWidthMult * toReturn;// toReturn * sineWidthMult;
                return 50;// toReturn * sineWidthMult * trailWidth * 15f; //50 25
            }




            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.025f);

            trailEffect.Parameters["bodyIntensity"].SetValue(1f);
            trailEffect.Parameters["bodyPower"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(0f);

            trailEffect.Parameters["whiteGlowSize"].SetValue(0.5f);
            trailEffect.Parameters["whiteGlowPower"].SetValue(2f);


            trailEffect.Parameters["TrailTexture1"].SetValue(trailTexture1);
            trailEffect.Parameters["tex1reps"].SetValue(2f);
            trailEffect.Parameters["tex1Intensity"].SetValue(1f); //2f

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(2f);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


    }

}