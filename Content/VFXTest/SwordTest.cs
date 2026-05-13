using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
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
using Terraria.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;
using VFXPLus.Common;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;
using static tModPorter.ProgressUpdate;

namespace VFXPlus.Content.VFXTest
{
    public class SwordTrailTest : ModProjectile
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
            int trailCount = 10; //45 -- > 35
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * Projectile.velocity.Length(), 0.25f);

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

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("Playground/Effects/TrailShaders/ChainShader", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;


            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 1f;

            trailEffect.Parameters["progress"].SetValue(0f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);

            Color col = Color.DarkRed;
            for (int i = 0; i < 5; i++)
            {
                if (i == 4)
                    col = Color.Red;


                Color StripColor(float progress) => col * progress;
                Vector2 offset = (2f * (i * MathHelper.PiOver2).ToRotationVector2());

                if (i == 4)
                    offset = Vector2.Zero;

                vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

                Matrix transform = Matrix.CreateTranslation(new Vector3(offset, 0f));
                Matrix view = Matrix.Identity;
                Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

                trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);

                trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

                vertexStrip.DrawTrail();


            }

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class SwordTrailTest2 : ModProjectile
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
            int trailCount = 10; //30
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Vector2 toMouse = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * Projectile.velocity.Length(), 0.25f);

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

            Main.spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShader", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/RampTop").Value;

            trailEffect.Parameters["progress"].SetValue(0f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 50f;
            Color StripColor(float progress) => Color.Red with { A = 255 };
            Color StripColor2(float progress) => Color.White with { A = 255 } * 0f * progress;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor2, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();



            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

}