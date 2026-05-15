using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
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


namespace VFXPlus.Content.Weapons.Melee.Hardmode.Swords
{
    
    public class HMOreSwordsGI : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && ((item.type == ItemID.CobaltSword) || (item.type == ItemID.MythrilSword) || (item.type == ItemID.AdamantiteSword) || (item.type == ItemID.PalladiumSword) || (item.type == ItemID.OrichalcumSword) || (item.type == ItemID.TitaniumSword));
        }

        public override void SetDefaults(Item entity)
        {
            entity.shoot = ProjectileID.WoodenArrowFriendly;
            entity.shootsEveryUse = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int projType = 0;
            if (item.type == ItemID.CobaltSword)
                projType = ModContent.ProjectileType<CobaltSwingFX>();
            else if (item.type == ItemID.MythrilSword)
                projType = ModContent.ProjectileType<MythrilSwingFX>();
            else if (item.type == ItemID.AdamantiteSword)
                projType = ModContent.ProjectileType<AdamantiteSwingFX>();
            else if (item.type == ItemID.PalladiumSword)
                projType = ModContent.ProjectileType<PalladiumSwingFX>();
            else if (item.type == ItemID.OrichalcumSword)
                projType = ModContent.ProjectileType<OrichalcumSwingFX>();
            else if (item.type == ItemID.TitaniumSword)
                projType = ModContent.ProjectileType<TitaniumSwingFX>();

            float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), projType, 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
            return false;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            float vanillaRot = ((float)player.itemAnimation / (float)player.itemAnimationMax - 0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;

            float startingRot = (0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;
            float endingRot = ((float)1f / (float)player.itemAnimationMax - 0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;

            //Main.NewText(player.itemAnimation);

            int timer = player.itemAnimationMax - player.itemAnimation;
            if (player.itemAnimation == 0)
                timer = 0;

            float lerpVal = Utils.GetLerpValue(0, player.itemAnimationMax - 1, timer, true);
            float easedRotation = MathHelper.Lerp(startingRot, endingRot, Easings.easeInOutQuad(lerpVal)); 
            //float easedRotation = MathHelper.Lerp(startingRot, endingRot, lerpVal); //Quad


            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135));

            player.itemRotation = easedRotation;

            Vector2 offset = new Vector2(1f, 0f).RotatedBy(player.itemRotation - MathHelper.PiOver4);
            player.itemLocation = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135)) - offset * 4f;

            player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135));

            //Main.NewText(endingRot + " | " + player.itemRotation);

            base.UseStyle(item, player, heldItemFrame);
        }

    }

    public class CobaltSwingFX : ModProjectile
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

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            //Pos is wrong on first frame if you switch swing dir TODO

            Projectile.localAI[0]++; // Current time that the projectile has been alive.
            Player player = Main.player[Projectile.owner];
            float percentageOfLife = Projectile.localAI[0] / (Projectile.ai[1] * 1f); // The current time over the max time.
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation; // Set the rotation to our to the new rotation we calculated.

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = 1f;

            if (Projectile.localAI[0] >= Projectile.ai[1] * 1f)
            {
                Projectile.Kill();
            }

            Vector2 offset = Projectile.rotation.ToRotationVector2() * 10f;

            int trailCount = (int)(Projectile.ai[1] / 3f) * 1; //30
            //previousRotations.Add(Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f);
            //previousPositions.Add(Projectile.Center + offset);
            previousRotations.Add(player.itemRotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add(player.itemLocation - player.Center);


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


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
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

            Color between = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.15f);
            Color between2 = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.5f);

            //Always start with black probably
            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                between2.ToVector3(),
                between.ToVector3(),
                Color.DeepSkyBlue.ToVector3(),
            };

            Main.spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            pos_arr = Array.ConvertAll(pos_arr, n => n + Projectile.Center);


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShaderGradient", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D noiseTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value;
            Texture2D flowTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Test/T_Random_54Stretch").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(3.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1.0f);

            trailEffect.Parameters["flowScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["flowSpeed"].SetValue(2f);
            trailEffect.Parameters["flowYOffset"].SetValue(0f);
            trailEffect.Parameters["flowGammaBoost"].SetValue(0f);


            trailEffect.Parameters["finalColMult"].SetValue(2.0f);
            trailEffect.Parameters["totalMult"].SetValue(1f);

            trailEffect.Parameters["gradColors"].SetValue(gradCols);
            trailEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 80f; //70
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

    public class MythrilSwingFX : ModProjectile
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

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            //Pos is wrong on first frame if you switch swing dir TODO

            Projectile.localAI[0]++; // Current time that the projectile has been alive.
            Player player = Main.player[Projectile.owner];
            float percentageOfLife = Projectile.localAI[0] / (Projectile.ai[1] * 1f); // The current time over the max time.
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation; // Set the rotation to our to the new rotation we calculated.

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = 1f;

            if (Projectile.localAI[0] >= Projectile.ai[1] * 1f)
            {
                Projectile.Kill();
            }

            Vector2 offset = Projectile.rotation.ToRotationVector2() * 10f;

            int trailCount = (int)(Projectile.ai[1] / 3f) * 1; //30
            //previousRotations.Add(Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f);
            //previousPositions.Add(Projectile.Center + offset);
            previousRotations.Add(player.itemRotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add(player.itemLocation - player.Center);


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


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
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

            pos_arr = Array.ConvertAll(pos_arr, n => n + Projectile.Center);


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShader3", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D noiseTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value;
            Texture2D flowTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Test/T_Random_54Stretch").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1.0f);

            trailEffect.Parameters["flowScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["flowSpeed"].SetValue(2f);
            trailEffect.Parameters["flowYOffset"].SetValue(0f);
            trailEffect.Parameters["flowGammaBoost"].SetValue(0.1f);


            trailEffect.Parameters["finalColMult"].SetValue(2.0f);
            trailEffect.Parameters["totalMult"].SetValue(1f);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 80f; //70
            Color StripColor(float progress) => Color.SeaGreen;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class AdamantiteSwingFX : ModProjectile
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

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            //Pos is wrong on first frame if you switch swing dir TODO

            Projectile.localAI[0]++; // Current time that the projectile has been alive.
            Player player = Main.player[Projectile.owner];
            float percentageOfLife = Projectile.localAI[0] / (Projectile.ai[1] * 1f); // The current time over the max time.
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation; // Set the rotation to our to the new rotation we calculated.

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = 1f;

            if (Projectile.localAI[0] >= Projectile.ai[1] * 1f)
            {
                Projectile.Kill();
            }

            Vector2 offset = Projectile.rotation.ToRotationVector2() * 10f;

            int trailCount = (int)(Projectile.ai[1] / 3f) * 1; //30
            //previousRotations.Add(Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f);
            //previousPositions.Add(Projectile.Center + offset);
            previousRotations.Add(player.itemRotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add((player.itemLocation - player.Center) * 1f);


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


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
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

            //Always start with black probably
            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                Color.DarkRed.ToVector3(),
                Color.Red.ToVector3(),
                new Color(255, 90, 90).ToVector3(),
                new Color(255, 200, 200).ToVector3()
            };


            Main.spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            pos_arr = Array.ConvertAll(pos_arr, n => n + Projectile.Center + new Vector2(0f, 0f));


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShaderGradient", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D noiseTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value;
            Texture2D flowTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Test/T_Random_54Stretch").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1.0f);

            trailEffect.Parameters["flowScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["flowSpeed"].SetValue(2f);
            trailEffect.Parameters["flowYOffset"].SetValue(0f);
            trailEffect.Parameters["flowGammaBoost"].SetValue(0.1f);


            trailEffect.Parameters["finalColMult"].SetValue(2.0f); //2f
            trailEffect.Parameters["totalMult"].SetValue(0.5f);

            trailEffect.Parameters["gradColors"].SetValue(gradCols);
            trailEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 80f; //70
            Color StripColor(float progress) => Color.Red;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class PalladiumSwingFX : ModProjectile
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

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            //Pos is wrong on first frame if you switch swing dir TODO

            Projectile.localAI[0]++; // Current time that the projectile has been alive.
            Player player = Main.player[Projectile.owner];
            float percentageOfLife = Projectile.localAI[0] / (Projectile.ai[1] * 1f); // The current time over the max time.
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation; // Set the rotation to our to the new rotation we calculated.

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = 1f;

            if (Projectile.localAI[0] >= Projectile.ai[1] * 1f)
            {
                Projectile.Kill();
            }

            Vector2 offset = Projectile.rotation.ToRotationVector2() * 10f;

            int trailCount = (int)(Projectile.ai[1] / 3f) * 1; //30
            //previousRotations.Add(Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f);
            //previousPositions.Add(Projectile.Center + offset);
            previousRotations.Add(player.itemRotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add(player.itemLocation - player.Center);


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


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
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

            pos_arr = Array.ConvertAll(pos_arr, n => n + Projectile.Center);


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShader3", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D noiseTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value;
            Texture2D flowTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Test/T_Random_54Stretch").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1.0f);

            trailEffect.Parameters["flowScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["flowSpeed"].SetValue(2f);
            trailEffect.Parameters["flowYOffset"].SetValue(0f);
            trailEffect.Parameters["flowGammaBoost"].SetValue(0f);


            trailEffect.Parameters["finalColMult"].SetValue(2.0f);
            trailEffect.Parameters["totalMult"].SetValue(1f);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 70f; //70
            Color StripColor(float progress) => Color.Orange;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class OrichalcumSwingFX : ModProjectile
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

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            //Pos is wrong on first frame if you switch swing dir TODO

            Projectile.localAI[0]++; // Current time that the projectile has been alive.
            Player player = Main.player[Projectile.owner];
            float percentageOfLife = Projectile.localAI[0] / (Projectile.ai[1] * 1f); // The current time over the max time.
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation; // Set the rotation to our to the new rotation we calculated.

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = 1f;

            if (Projectile.localAI[0] >= Projectile.ai[1] * 1f)
            {
                Projectile.Kill();
            }

            if (timer == 0)
            {
                previousPositions.Clear();
                previousRotations.Clear();

            }


            Vector2 offset = Projectile.rotation.ToRotationVector2() * 10f;

            int trailCount = (int)(Projectile.ai[1] / 3f) * 1; //30
            //previousRotations.Add(Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f);
            //previousPositions.Add(Projectile.Center + offset);
            previousRotations.Add(player.itemRotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add(player.itemLocation - player.Center);

            Main.NewText(player.itemRotation + MathHelper.PiOver4 * player.direction);

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


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
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

            if (previousPositions.Count < 2)
                return;

            Main.spriteBatch.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            pos_arr = Array.ConvertAll(pos_arr, n => n + Projectile.Center + new Vector2(0f, 0f));


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShader3", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D noiseTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value;
            Texture2D flowTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/wnoise").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1.0f);

            trailEffect.Parameters["flowScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["flowSpeed"].SetValue(2f);
            trailEffect.Parameters["flowYOffset"].SetValue(0f);
            trailEffect.Parameters["flowGammaBoost"].SetValue(0f);


            trailEffect.Parameters["finalColMult"].SetValue(2.0f);
            trailEffect.Parameters["totalMult"].SetValue(2f);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 70f; //70
            Color StripColor(float progress) => Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f) * 1.2f;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class TitaniumSwingFX : ModProjectile
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

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            //Pos is wrong on first frame if you switch swing dir TODO

            Projectile.localAI[0]++; // Current time that the projectile has been alive.
            Player player = Main.player[Projectile.owner];
            float percentageOfLife = Projectile.localAI[0] / (Projectile.ai[1] * 1f); // The current time over the max time.
            float direction = Projectile.ai[0];
            float velocityRotation = Projectile.velocity.ToRotation();
            float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;
            Projectile.rotation = adjustedRotation; // Set the rotation to our to the new rotation we calculated.

            Projectile.Center = player.RotatedRelativePoint(player.MountedCenter) - Projectile.velocity;
            Projectile.scale = 1f;

            if (Projectile.localAI[0] >= Projectile.ai[1] * 1f)
            {
                Projectile.Kill();
            }

            Vector2 offset = Projectile.rotation.ToRotationVector2() * 10f;

            int trailCount = (int)(Projectile.ai[1] / 3f) * 1; //30
            //previousRotations.Add(Projectile.rotation + MathHelper.PiOver2 + MathHelper.PiOver4 * 0.5f);
            //previousPositions.Add(Projectile.Center + offset);
            previousRotations.Add(player.itemRotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add(player.itemLocation - player.Center);


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


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderNPCs, () =>
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

            pos_arr = Array.ConvertAll(pos_arr, n => n + Projectile.Center);


            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShader3", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1MoreTop").Value;
            Texture2D noiseTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value;
            Texture2D flowTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Test/T_Random_62").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1.0f);

            trailEffect.Parameters["flowScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["flowSpeed"].SetValue(2f);
            trailEffect.Parameters["flowYOffset"].SetValue(0f);
            trailEffect.Parameters["flowGammaBoost"].SetValue(0.1f);


            trailEffect.Parameters["finalColMult"].SetValue(2.0f);
            trailEffect.Parameters["totalMult"].SetValue(1f);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 80f; //70
            Color StripColor(float progress) => Color.Silver;


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

}
