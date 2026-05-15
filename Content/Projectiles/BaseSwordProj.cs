using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;


namespace VFXPlus.Content.Projectiles
{
    public class BaseSwordProj : ModProjectile
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

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public SwordProjInfo info;

        public int itemAnimationTime
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        int timer = 0;

        List<float> previousRotations = new List<float>();
        List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            #region Calculate pos and rot
            float vanillaRot = ((float)player.itemAnimation / (float)player.itemAnimationMax - 0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;

            float startingRot = (0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;
            float endingRot = ((float)1f / (float)player.itemAnimationMax - 0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;

            float lerpVal = Utils.GetLerpValue(0, player.itemAnimationMax - 1, timer, true);
            float easedRotation = MathHelper.Lerp(startingRot, endingRot, Easings.easeInOutQuad(lerpVal));
            //float easedRotation = MathHelper.Lerp(startingRot, endingRot, lerpVal);


            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135) * player.direction);

            Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135) * player.direction);

            Vector2 dir = (Projectile.Center - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            //Projectile.Center = player.MountedCenter + dir * 8f;
            //Projectile.Center -= new Vector2(3f * player.direction, 0f);

            Projectile.rotation = easedRotation;

            player.heldProj = Projectile.whoAmI;

            player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135) * player.direction);

            #endregion

            Main.NewText((Projectile.Center - player.MountedCenter).Length());

            #region HandAnimation
            
            if (1f - Easings.easeInOutQuad(lerpVal) < 0.333f)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 3;
            }
            else if (1f - Easings.easeInOutQuad(lerpVal) < 0.666f)
            {
                player.bodyFrame.Y = player.bodyFrame.Height * 2;
            }
            else
            {
                player.bodyFrame.Y = player.bodyFrame.Height;
            }
            
            #endregion


            if (timer >= itemAnimationTime - 1)
            {
                Projectile.Kill();
            }

            Vector2 offset = (Projectile.rotation - MathHelper.PiOver4 * player.direction).ToRotationVector2() * player.direction;
            offset *= info.positionOffset;

            int trailCount = (int)(Projectile.ai[1] / 2.5f) * 1; // /3
            previousRotations.Add(Projectile.rotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add(Projectile.Center - player.MountedCenter + offset);


            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

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

            Player player = Main.player[Projectile.owner];

            //Debug
            //Vector2 offsetPos = (Projectile.rotation - MathHelper.PiOver4 * player.direction).ToRotationVector2() * 200f * player.direction;
            //Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + offsetPos, Color.White);


            #region DrawSword

            Texture2D Sword = TextureAssets.Item[info.itemID].Value;

            Vector2 swordPos = Projectile.Center - Main.screenPosition;

            Vector2 swordOrigin = new Vector2(player.direction == 1 ? 0f : Sword.Width, Sword.Height);
            swordOrigin += new Vector2(info.originOffset * player.direction, -info.originOffset);

            SpriteEffects fx = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.spriteBatch.Draw(Sword, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, swordOrigin, Projectile.scale, fx, 0f);

            #endregion


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

            pos_arr = Array.ConvertAll(pos_arr, n => n + Projectile.Center);


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShaderGradient", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = info.trailTexture;
            Texture2D noiseTexture = info.noiseTexture;
            Texture2D flowTexture = info.flowTexture;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(info.posterizationSteps);

            trailEffect.Parameters["noiseScale"].SetValue(info.noiseScale);
            trailEffect.Parameters["noiseIntensity"].SetValue(info.noiseIntensity);

            trailEffect.Parameters["flowScale"].SetValue(info.flowScale);
            trailEffect.Parameters["flowSpeed"].SetValue(info.flowSpeed);
            trailEffect.Parameters["flowYOffset"].SetValue(info.flowYOffset);
            trailEffect.Parameters["flowGammaBoost"].SetValue(info.flowGammaBoost);

            trailEffect.Parameters["finalColMult"].SetValue(info.finalColMult * 2f);
            trailEffect.Parameters["totalMult"].SetValue(info.totalMult);

            trailEffect.Parameters["gradColors"].SetValue(info.gradientColors);
            trailEffect.Parameters["numberOfColors"].SetValue(info.gradientColors.Length);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => info.trailWidth;
            Color StripColor(float progress) => Color.White;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }


    //In a separate class so it can be used for other things if need be
    //Does not support non-gradient sword trail shader
    public class SwordProjInfo
    {
        public int itemID = 1;
        
        public float positionOffset = 0f;
        public float originOffset = 0f;

        public float trailWidth = 70f;

        //Shader info
        public Vector3[] gradientColors;

        public Texture2D trailTexture;
        public Texture2D noiseTexture;
        public Texture2D flowTexture;

        public float posterizationSteps = 4.0f;

        public Vector2 noiseScale;
        public float noiseIntensity = 1f;

        public Vector2 flowScale;
        public float flowSpeed = 2f;
        public float flowYOffset = 0f;
        public float flowGammaBoost;

        public float finalColMult = 2f;
        public float totalMult = 1f;

        //Basic Constructor
        public SwordProjInfo(int ItemID, Vector3[] GradientColors, float PositionOffset, float OriginOffset, float TrailWidth = 70, float PosterizationSteps = 4.0f)
        {
            itemID = ItemID;
            positionOffset = PositionOffset;
            originOffset = OriginOffset;
            trailWidth = TrailWidth;
            gradientColors = GradientColors;
            posterizationSteps = PosterizationSteps;

            noiseScale = new Vector2(0.5f, 1f);
            noiseIntensity = 1f;

            flowScale = new Vector2(0.5f, 1f);
            flowSpeed = 2f;
            flowYOffset = 0f;
            flowGammaBoost = 0f;

            finalColMult = 1f;
            totalMult = 1f;

            trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/SwordSmear1").Value;
            noiseTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Trail_2").Value;
            flowTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Test/T_Random_54Stretch").Value;
        }

        public SwordProjInfo(int ItemID, Vector3[] GradientColors, float PositionOffset, float OriginOffset, float TrailWidth = 70, float PosterizationSteps = 4.0f, float FinalColMult = 1f, float TotalMult = 1f)
        {
            itemID = ItemID;
            positionOffset = PositionOffset;
            originOffset = OriginOffset;
            trailWidth = TrailWidth;
            gradientColors = GradientColors;
            posterizationSteps = PosterizationSteps;

            noiseScale = new Vector2(0.5f, 1f);
            noiseIntensity = 1f;

            flowScale = new Vector2(0.5f, 1f);
            flowSpeed = 2f;
            flowYOffset = 0f;
            flowGammaBoost = 0f;

            finalColMult = FinalColMult;
            totalMult = TotalMult;

            trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/SwordSmear1").Value;
            noiseTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Trail_2").Value;
            flowTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Test/T_Random_54Stretch").Value;
        }

        //Kitchen Sink Constructor
    }
}
