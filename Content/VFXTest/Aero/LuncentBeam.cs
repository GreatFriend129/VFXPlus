using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using VFXPlus.Content.Dusts;
using VFXPlus.Common.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using Terraria.DataStructures;
using static Terraria.NPC;
using ReLogic.Content;
using VFXPlus.Content.Projectiles;
using Terraria.Graphics;
using UtfUnknown.Core.Models.SingleByte.Finnish;

namespace VFXPlus.Content.VFXTest.Aero
{
    public class LuncentBeam : ModItem
    {
        private int shotCounter = 0;
        
        public override void SetDefaults()
        {
            Item.damage = 26;
            Item.knockBack = 2f;
            Item.crit = 4;
            Item.DamageType = DamageClass.Magic;
            Item.rare = ItemRarityID.Yellow;
            Item.width = 58;
            Item.height = 20;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shootSpeed = 7f;
            Item.shoot = ModContent.ProjectileType<LucentBeamHeldProj>();

            Item.autoReuse = false;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            

            return true;
        }

        public override bool OnPickup(Player player) //Another check for the problem mentioned above
        {
            shotCounter = 0;
            return base.OnPickup(player);
        }
    }

    public class LucentBeamHeldProj : ModProjectile
    {
        public override string Texture => "VFXPlus/Content/VFXTest/Aero/GaussianStar";

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        //Tendril positions relative to itself
        Vector2[] arr_positions = new Vector2[150];
        float[] arr_rotations = new float[150];

        //Actual draw position of the tendril (in world coords)
        float[] draw_rotations = new float[150];
        Vector2[] draw_positions = new Vector2[150];

        int TotalPoints = 150;

        public Vector2 anchor = Vector2.Zero;

        int timer = 0;
        public override void AI()
        {
            #region held proj code
            ProjectileExtensions.KillHeldProjIfPlayerDeadOrStunned(Projectile);
            Player player = Main.player[Projectile.owner];

            Projectile.Center = player.MountedCenter + Projectile.rotation.ToRotationVector2() * 0f;

            player.heldProj = Projectile.whoAmI;
            player.ChangeDir(Projectile.Center.X < player.Center.X ? -1 : 1);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            player.itemTime = 2;

            Projectile.timeLeft = 2;

            //How much we can turn towards cursor
            float turningLerpValue = 0.2f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, player.DirectionTo(Main.MouseWorld), turningLerpValue);
            Projectile.rotation = Projectile.velocity.ToRotation();

            player.velocity = Vector2.Zero;

            #endregion

            #region tendril code
            if (timer == 0)
            {
                anchor = Projectile.Center;

                //Create all of the points and set all the rotations to be the same
                float distBetweenEachPoint = 2.5f;
                for (int i = 0; i < TotalPoints; i++)
                {
                    arr_positions[i] = Vector2.Zero + Projectile.rotation.ToRotationVector2() * (distBetweenEachPoint * i);
                    arr_rotations[i] = Projectile.rotation;
                }
            }

            //Have all points try to rotate towards the acnhor
            for (int j = 0; j < TotalPoints; j++)
            {
                float progress = (j / (float)TotalPoints);

                //The further along the trail, the weaker the turning
                float lerpValue = MathHelper.Lerp(1f, 0.3f, progress);

                //Keep angle within 2pi 
                float NormalizedGoalRotation = Projectile.rotation;

                float newRotation = MathHelper.Lerp(arr_rotations[j], NormalizedGoalRotation, lerpValue * 0.185f); //0.175

                arr_rotations[j] = newRotation;
                arr_positions[j] = Vector2.Zero + newRotation.ToRotationVector2() * (2.5f * j);
            }

            for (int k = 0; k < TotalPoints - 1 * beamWidth; k++)
            {
                //We have to flip the first point over for some reason or else we get a weird tear.
                if (k == 0)
                    draw_rotations[k] = arr_rotations[k] + MathHelper.Pi;
                else
                    draw_rotations[k] = (arr_positions[k - 1] - arr_positions[k]).ToRotation();
                draw_positions[k] = arr_positions[k] + anchor;
            }


            #endregion


            timer++;
        }

        public override bool PreKill(int timeLeft)
        {
           
            return base.PreKill(timeLeft);
        }

        float beamWidth = 1f;
        float overallAlpha = 1f;
        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);

            return false;

        }

        public void DrawTrail(bool giveUp = false)
        {

            //Create arrays
            //Vector2[] pos_arr = l_positions.ToArray();
            //float[] rot_arr = l_rotations.ToArray();

            float widthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.07f) * 0.15f;

            Color StripColor(float progress) => Color.White * 1f;
            //float StripWidth(float progress) => 45f * Easings.easeInSine(1f) * widthMult;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(draw_positions, draw_rotations, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            ShaderParams();

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
        public void ShaderParams()
        {
            //float fadeOut = shouldFade ? Math.Clamp(Easings.easeInQuad(true_alpha), 0.15f, 1f) : true_alpha;

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value);
            myEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/RainbowGrad1").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["satPower"].SetValue(0.8f); //higher power -> less affected by background  |95 | 3f looks very goozma

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/EvenThinnerGlowLine").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_06").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail5Loop").Value); //smokeTrail4_512


            myEffect.Parameters["grad1Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad2Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad3Speed"].SetValue(3.1f / 3f);
            myEffect.Parameters["grad4Speed"].SetValue(2.3f / 3f);

            myEffect.Parameters["tex1Mult"].SetValue(1.25f);
            myEffect.Parameters["tex2Mult"].SetValue(1.5f);
            myEffect.Parameters["tex3Mult"].SetValue(1.15f);
            myEffect.Parameters["tex4Mult"].SetValue(2.5f * 0f); //1.5
            myEffect.Parameters["totalMult"].SetValue(1f);


            //We want the number of repititions to be relative to the number of points
            float repValue = 0.05f * 35f;
            myEffect.Parameters["gradientReps"].SetValue(0.35f * repValue); //1f
            myEffect.Parameters["tex1reps"].SetValue(1f * repValue); //2.5
            myEffect.Parameters["tex2reps"].SetValue(0.3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(1f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(0.25f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.025f); //-0.015

        }

        public float StripWidth(float progress)
        {
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 60f, Easings.easeInCirc(num)) * 1.15f * Easings.easeInSine(beamWidth); // 0.3f 
        }
    }

    public class LucentBeamTest : ModProjectile
    {
        public override string Texture => "VFXPlus/Content/VFXTest/Aero/GaussianStar";

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        //Tendril positions relative to itself
        Vector2[] arr_positions = new Vector2[150];
        float[] arr_rotations = new float[150];

        //Actual draw position of the tendril (in world coords)
        float[] draw_rotations = new float[150];
        Vector2[] draw_positions = new Vector2[150];

        int TotalPoints = 150;

        public Vector2 anchor = Vector2.Zero;

        float width = 1f;
        int timer = 0;
        public override void AI()
        {
            if (timer == 0)
            {
                anchor = Projectile.Center;

                //Create all of the points and set all the rotations to be the same
                for (int i = 0; i < TotalPoints; i++)
                {
                    arr_positions[i] = Vector2.Zero + Projectile.rotation.ToRotationVector2() * (2.5f * i);
                    arr_rotations[i] = Projectile.rotation;
                }

                Projectile.ai[1] = 1f;
            }

            if (timer % 11170 == 0)
                Projectile.ai[1] *= -1f;

            Projectile.rotation += 0.06f * Projectile.ai[1];// * Easings.easeInSine(width) * Projectile.ai[0];

            //Have all points try to rotate towards the acnhor
            for (int j = 0; j < TotalPoints; j++)
            {
                float progress = (j / (float)TotalPoints);

                //The further along the trail, the weaker the turning
                float lerpValue = MathHelper.Lerp(1f, 0.3f, progress);

                //Keep angle within 2pi 
                float NormalizedGoalRotation = Projectile.rotation;

                float newRotation = MathHelper.Lerp(arr_rotations[j], NormalizedGoalRotation, lerpValue * 0.125f); //0.175

                arr_rotations[j] = newRotation;
                arr_positions[j] = Vector2.Zero + newRotation.ToRotationVector2() * (2.5f * j);
            }

            for (int k = 0; k < TotalPoints - 1 * width; k++)
            {
                //We have to flip the first point over for some reason or else we get a weird tear.
                if (k == 0)
                    draw_rotations[k] = arr_rotations[k] + MathHelper.Pi;
                else
                    draw_rotations[k] = (arr_positions[k - 1] - arr_positions[k]).ToRotation();
                draw_positions[k] = arr_positions[k] + anchor;
            }

            timer++;
        }

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);

            return false;

        }

        public void DrawTrail(bool giveUp = false)
        {

            //Create arrays
            //Vector2[] pos_arr = l_positions.ToArray();
            //float[] rot_arr = l_rotations.ToArray();

            float widthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.07f) * 0.15f;

            Color StripColor(float progress) => Color.White * 1f;
            //float StripWidth(float progress) => 45f * Easings.easeInSine(1f) * widthMult;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(draw_positions, draw_rotations, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            ShaderParams();

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
        public void ShaderParams()
        {
            //float fadeOut = shouldFade ? Math.Clamp(Easings.easeInQuad(true_alpha), 0.15f, 1f) : true_alpha;

            //Color rainbow = Main.hslToRgb((timer * 0.01f + rainbowOffset) % 1f, 1f, 0.72f, 0) * 0.75f * fadeOut; //0.6 gives a fucking amazing orange

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value);
            myEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/RainbowGrad1").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["satPower"].SetValue(0.8f); //higher power -> less affected by background  |95 | 3f looks very goozma

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/ThinGlowLine").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_06").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail5Loop").Value); //smokeTrail4_512


            myEffect.Parameters["grad1Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad2Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad3Speed"].SetValue(3.1f / 3f);
            myEffect.Parameters["grad4Speed"].SetValue(2.3f / 3f);

            myEffect.Parameters["tex1Mult"].SetValue(1.25f);
            myEffect.Parameters["tex2Mult"].SetValue(1.5f);
            myEffect.Parameters["tex3Mult"].SetValue(1.15f);
            myEffect.Parameters["tex4Mult"].SetValue(2.5f * 0f); //1.5
            myEffect.Parameters["totalMult"].SetValue(1f);


            //We want the number of repititions to be relative to the number of points
            float repValue = 0.05f * 35f;
            myEffect.Parameters["gradientReps"].SetValue(0.35f * repValue); //1f
            myEffect.Parameters["tex1reps"].SetValue(1f * repValue); //2.5
            myEffect.Parameters["tex2reps"].SetValue(0.3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(1f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(0.25f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.025f); //-0.015

        }

        public float StripWidth(float progress)
        {
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 60f, Easings.easeInCirc(num)) * 1.15f * Easings.easeInSine(width); // 0.3f 
        }
    }

}