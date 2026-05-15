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
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Swords
{
    
    public class BloodButcherer : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BloodButcherer);
        }

        public override void SetDefaults(Item entity)
        {
            entity.shoot = ProjectileID.WoodenArrowFriendly;
            entity.shootsEveryUse = true;
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
            Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<BloodButchererSwingFXDrawn>(), 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
            //return false;

            //Swing FX 2 better with easeInOutQuad | but fx 1 better with linear ease

            return false;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            base.UseStyle(item, player, heldItemFrame);
            return;

            Vector2 offset = new Vector2(1f, 0f).RotatedBy(player.itemRotation - MathHelper.PiOver4);
            float vanillaRot = ((float)player.itemAnimation / (float)player.itemAnimationMax - 0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;

            float startingRot = (0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;
            float endingRot = ((float)1f / (float)player.itemAnimationMax - 0.5f) * (float)(-player.direction) * 3.5f - (float)player.direction * 0.3f;

            //Main.NewText(player.itemAnimation);

            int timer = player.itemTimeMax - player.itemTime;
            if (player.itemTime == 0)
                timer = 0;
            //timer++;

            float lerpVal = Utils.GetLerpValue(0, player.itemAnimationMax - 1, timer, true);
            float easedRotation = MathHelper.Lerp(startingRot, endingRot, Easings.easeInOutQuad(lerpVal)); //Quad
            //float easedRotation = MathHelper.Lerp(startingRot, endingRot, lerpVal); //Quad

            //Main.NewText(lerpVal);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135));

            //player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation + MathHelper.ToRadians(-135));
            //player.itemLocation = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, player.itemRotation + MathHelper.ToRadians(-135)) - offset * 4f;
            player.itemLocation = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135)) - offset * 4f;
            player.itemRotation = easedRotation;
            player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135));

            //float percentageOfLife = Projectile.localAI[0] / (Projectile.ai[1] * 1f); // The current time over the max time.
            //float direction = player.direction * player.gravDir;
            //float velocityRotation = new Vector2(player.direction, 0f).ToRotation();
            //float adjustedRotation = MathHelper.Pi * direction * percentageOfLife + velocityRotation + direction * MathHelper.Pi + player.fullRotation;





            //Main.NewText(endingRot + " | " + player.itemRotation);

            base.UseStyle(item, player, heldItemFrame);
        }

    }

    public class BloodButchererSwingFX : ModProjectile
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
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShader", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/RampTop").Value;

            trailEffect.Parameters["progress"].SetValue(0f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            trailEffect.Parameters["totalMult"].SetValue(1f);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 70f; //80
            Color StripColor(float progress) => new Color(255, 0, 0) with { A = 255 };
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

    public class BloodButchererSwingFX2 : ModProjectile
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
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/SwordTrailShader2", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/SwordSmear1").Value;
            Texture2D noiseTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.005f);
            trailEffect.Parameters["reps"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            trailEffect.Parameters["noiseScale"].SetValue(new Vector2(0.5f, 1.0f));
            trailEffect.Parameters["noiseIntensity"].SetValue(1.0f);

            trailEffect.Parameters["totalMult"].SetValue(0.75f);

            Matrix transform = Matrix.CreateTranslation(new Vector3(Vector2.Zero, 0f));
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            trailEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);



            VertexStripFixed vertexStrip = new VertexStripFixed();

            float StripWidth(float progress) => 70f; //70
            Color StripColor(float progress) => new Color(255, 0, 0) with { A = 255 };


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class BloodButchererSwingFX3 : ModProjectile
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
            Color StripColor(float progress) => new Color(255, 0, 0) with { A = 255 };


            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["NoiseTexture"].SetValue(noiseTexture);
            trailEffect.Parameters["FlowTexture"].SetValue(flowTexture);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


        }


    }

    public class BloodButchererSwingFXDrawn : ModProjectile
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

        public int itemAnimationTime
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        int timer = 0;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            //Pos is wrong on first frame if you switch swing dir TODO

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
            Projectile.rotation = easedRotation;

            player.heldProj = Projectile.whoAmI;

            player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, easedRotation + MathHelper.ToRadians(-135) * player.direction);

            #endregion

            #region HandAnimation
            /*
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
            */
            #endregion


            if (timer >= itemAnimationTime - 1)
            {
                Projectile.Kill();
            }

            Vector2 offset = (Projectile.rotation - MathHelper.PiOver4 * player.direction).ToRotationVector2() * player.direction;
            offset *= -10f; //18

            int trailCount = (int)(Projectile.ai[1] / 3f) * 1; //30
            previousRotations.Add(Projectile.rotation + MathHelper.PiOver4 * player.direction);
            previousPositions.Add(Projectile.Center - player.Center + offset);


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

            Player player = Main.player[Projectile.owner];

            Vector2 offsetPos = (Projectile.rotation - MathHelper.PiOver4 * player.direction).ToRotationVector2() * 200f * player.direction;

            //Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + offsetPos, Color.White);

            #region DrawSword

            Texture2D Sword = TextureAssets.Item[ItemID.BloodButcherer].Value;

            int dir = player.direction;

            Vector2 swordPos = Projectile.Center - Main.screenPosition;

            Vector2 swordOrigin = new Vector2(dir == 1 ? 0f : Sword.Width, Sword.Height);
            swordOrigin += new Vector2(2f * dir, -2f);

            SpriteEffects fx = dir == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

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
            Color StripColor(float progress) => new Color(255, 0, 0) with { A = 255 };


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
