using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
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
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Bows;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Bows
{
    
    public class BloodRainBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BloodRainBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            return true;
        }
    }

    public class BloodRainBowShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BloodArrow);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                Vector2 portalVel = projectile.velocity.SafeNormalize(Vector2.UnitX) * 3f;
                int portal = Projectile.NewProjectile(null, projectile.Center, portalVel, ModContent.ProjectileType<BloodRainBowVFX>(), 0, 0, Main.myPlayer);
                Main.projectile[portal].rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            
            //VERY IMPORTANT
            projectile.hide = false;

            int trailCount = 24;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 3 == 0)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f).RotatedBy(projectile.velocity.ToRotation());
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.Red, Scale: Main.rand.NextFloat(0.55f, 0.8f) * 0.8f);
                d.velocity += projectile.velocity * 0.5f;

                d.customData = new GlowFlareBehavior(GlowThreshold: 0.6f, GlowPower: 2.5f, TotalBoost: 1f);
            }

            float fadeInTime = Math.Clamp((timer + 12f) / 55f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.25f);

            timer++;

            if (timer % 1 == 0)
            {
                Vector2 dustPos = projectile.Center;
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Blood, dustVel, Scale: 1f);
                d.velocity += projectile.velocity * 0.1f;

                if (Main.rand.NextBool())
                {
                    Vector2 dustVel2 = Main.rand.NextVector2Circular(0.75f, 0.75f);
                    Dust d2 = Dust.NewDustPerfect(dustPos, DustID.Blood, dustVel, Scale: 0.75f);
                    d2.velocity += projectile.velocity * 0.1f;
                }
            }

            #region vanillaAI
            projectile.ai[0] += 1f;

            if (projectile.ai[0] >= 15f)
            {
                projectile.ai[0] = 15f;

                projectile.velocity.Y += 0.1f;
            }

            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            Lighting.AddLight(projectile.Center, 0.3f, 0.05f, 0.05f);
            #endregion

            return false;
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D flare = CommonTextures.Flare.Value;
            Texture2D flare2 = CommonTextures.AnotherLineGlow.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = flare.Size() / 2f;

            float drawScale = projectile.scale * overallScale;
            Color darkerRed = new Color(103, 0, 0);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {

                //After-Image
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    //Start End
                    Color col = Color.Lerp(Color.DarkRed, darkerRed, progress) * progress;

                    Vector2 size = new Vector2(1f, 0.3f * progress) * drawScale;

                    Main.EntitySpriteDraw(flare, AfterImagePos, null, col * progress * 2f,
                        previousRotations[i], TexOrigin, size, SpriteEffects.None);

                    //SIZE 2 IS REAL
                    Vector2 size2 = new Vector2(0.7f, 3f * progress) * drawScale;

                    Main.EntitySpriteDraw(flare2, AfterImagePos, null, Color.Red with { A = 0 } * 0.11f * progress,
                        previousRotations[i], flare2.Size() / 2f, size2, SpriteEffects.None);
                }

            });

            Main.EntitySpriteDraw(flare, projectile.Center - Main.screenPosition, null, Color.Red * 1f, projectile.velocity.ToRotation(), TexOrigin, new Vector2(1f, 0.75f), SpriteEffects.None);
            Main.EntitySpriteDraw(flare, projectile.Center - Main.screenPosition, null, new Color(255, 175, 175) with { A = 0 } * 0f, projectile.velocity.ToRotation(), TexOrigin, 0.55f, SpriteEffects.None);

            return false;
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 9 + Main.rand.Next(0, 5); i++)
            {

                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f).RotatedBy(projectile.velocity.ToRotation());
                Dust d = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, projectile.height / 2f), ModContent.DustType<GlowFlare>(), vel, newColor: Color.Red, Scale: Main.rand.NextFloat(0.45f, 0.8f) * 0.8f);
                d.velocity += new Vector2(0f, -1f);

                d.customData = new GlowFlareBehavior(GlowThreshold: 0.6f, GlowPower: 2.5f, TotalBoost: 1f);
            }

            Dust.NewDustPerfect(projectile.Center + projectile.velocity * 1f, ModContent.DustType<PaintSplotch>(), newColor: Color.Lerp(Color.Red, Color.DarkRed, 0.5f), Scale: 1f);

            return true;
        }
    }

    public class NewBloodRainProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 1000;

            Projectile.width = Projectile.height = 10;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        int timer = 0;

        List<float> previousRotations = new List<float>();
        List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            int trailCount = 20; //8
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center);


            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Projectile.velocity.Y += 0.13f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            timer++;
        }

        Effect trailEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];

            Color thisLightColor = lightColor;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(true, thisLightColor);
                DrawTrailSworded(false, thisLightColor);
            });
            DrawTrailSworded(true, thisLightColor);
            DrawTrail(true, thisLightColor);

            return false;
        }

        public void DrawTrail(bool giveUp, Color lightColor)
        {
            if (giveUp)
                return;

            Player player = Main.player[Projectile.owner];


            Main.spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/PosterizedTrailShader", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/GooeyPixelTrail").Value;
            Texture2D noiseTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Trail_2").Value;

            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                Color.DarkRed.ToVector3(),
                new Color(200, 0, 0).ToVector3(),
            };


            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["posterizationSteps"].SetValue(2.0f);

            trailEffect.Parameters["scrollScale"].SetValue(new Vector2(0.5f, 1f));
            trailEffect.Parameters["scrollSpeed"].SetValue(1.5f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(1f, 1f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1f);

            trailEffect.Parameters["totalMult"].SetValue(1f);

            trailEffect.Parameters["gradColors"].SetValue(gradCols);
            trailEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 12f * Easings.easeOutCubic(progress);
            Color StripColor(float progress) => lightColor;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }

        public void DrawTrailSworded(bool giveUp, Color lightColor)
        {
            if (giveUp)
                return;

            Player player = Main.player[Projectile.owner];


            Main.spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShaderGradient", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/LavaTrailV1").Value;
            Texture2D noiseTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Trail_2").Value;
            Texture2D flowTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Test/T_Random_54StretchLoop").Value;

            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                Color.Red.ToVector3(),
                Color.Red.ToVector3(),
            };


            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(3f);
            trailEffect.Parameters["posterizationSteps"].SetValue(3.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(2f, 0.25f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1f);

            trailEffect.Parameters["flowScale"].SetValue(new Vector2(1f, 1f));
            trailEffect.Parameters["flowSpeed"].SetValue(1f);
            trailEffect.Parameters["flowYOffset"].SetValue(0f);
            trailEffect.Parameters["flowGammaBoost"].SetValue(0f);

            trailEffect.Parameters["finalColMult"].SetValue(2f);
            trailEffect.Parameters["totalMult"].SetValue(1f);

            trailEffect.Parameters["gradColors"].SetValue(gradCols);
            trailEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 30f * Easings.easeOutCubic(progress);
            Color StripColor(float progress) => lightColor;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class BloodRainBowVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        //Safety Checks
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2400;
        }

        int timer = 0;
        float true_width = 1f;
        float true_alpha = 1f;

        public override void AI()
        {
            if (timer > 2)
                true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.08f), 0, 1f);

            if (timer == 100 || true_width <= 0.05f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawPortal(false);
            });
            DrawPortal(true);

            return false;
        }

        public void DrawPortal(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //Portal at first node
            Vector2 portalPos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;

            float easedScale = true_width;
            Vector2 v2Scale = new Vector2(1f * easedScale, 0.25f + (easedScale * 0.75f)) * Projectile.scale * 1.25f;


            Main.EntitySpriteDraw(portal, portalPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.DarkRed with { A = 60 } * 0.5f, rot, portal.Size() / 2f, v2Scale * 1.25f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, portalPos, null, Color.Red with { A = 60 } * 1f, rot, portal.Size() / 2f, v2Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, portalPos, null, Color.White with { A = 60 } * 1f, rot, portal.Size() / 2f, v2Scale * 0.5f, SpriteEffects.None);

        }
    }

}
