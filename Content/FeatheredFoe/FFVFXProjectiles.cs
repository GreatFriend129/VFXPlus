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

            myEffect.Parameters["Color1Mult"].SetValue(0.5f);
            myEffect.Parameters["Color2Mult"].SetValue(1f);
            myEffect.Parameters["Color3Mult"].SetValue(0.5f);
            myEffect.Parameters["Color4Mult"].SetValue(0.5f);
            myEffect.Parameters["totalMult"].SetValue(1f);

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

}