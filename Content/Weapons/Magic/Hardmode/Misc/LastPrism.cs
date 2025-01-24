using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using System.Linq;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using Terraria.GameContent;
using System.Threading;
using ReLogic.Utilities;
using VFXPlus.Common.Drawing;
using rail;
using Terraria.Graphics;
using System.Runtime.Intrinsics.X86;
using VFXPlus.Content.VFXTest;
using System.Collections;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class LastPrism : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.LastPrism);
        }

        public override void SetDefaults(Item entity)
        {
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }

    public class LastPrismHeldProjectileOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LastPrism);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            timer++;
            return base.PreAI(projectile);
        }


        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //Border
            Color[] rainbow = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.DodgerBlue, Color.Violet };
            for (int i = 0; i < 6; i++)
            {
                float rot = i * (MathHelper.TwoPi / 6f);

                Vector2 off = new Vector2(3f, 0f).RotatedBy(rot + (float)Main.timeForVisualEffects * 0.09f);
                Main.EntitySpriteDraw(vanillaTex, drawPos + off + new Vector2(0f, 0f), sourceRectangle,
                   rainbow[i] with { A = 0 } * 0.25f, projectile.rotation, TexOrigin, projectile.scale * 1.07f, SpriteEffects.None);
            }


            return true;

        }
    }

    public class LastPrismLaserOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LastPrismLaser);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            

            timer++;
            return true;
        }

        float overallAlpha = 1f;
        float overallScale = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            DrawVertexTrail(projectile, false);

            return false;

        }

        Effect myEffect = null;
        public void DrawVertexTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            float colAlpha = Easings.easeOutQuad(projectile.Opacity);

            float laserLuminance = 0.5f;
            float laserAlphaMultiplier = 0f;
            Color lastPrismCol = Main.hslToRgb(projectile.GetLastPrismHue(projectile.ai[0], ref laserLuminance, ref laserAlphaMultiplier), 1f, laserLuminance) * colAlpha;

            lastPrismCol = Color.Lerp(lastPrismCol, Color.White, 0.1f);
            //Main.NewText(projectile.Opacity);

            Vector2 startPoint = projectile.Center;
            Vector2 endPoint = startPoint + (projectile.velocity * projectile.localAI[1]);
            float dist = (endPoint - startPoint).Length();


            Texture2D bloom = Mod.Assets.Request<Texture2D>("Assets/Trails/Clear/BloomSliver").Value;

            Vector2 bloomScale = new Vector2(dist, 1f * colAlpha);
            Main.EntitySpriteDraw(bloom, projectile.Center - Main.screenPosition, null, lastPrismCol with { A = 0 } * 0.15f, projectile.velocity.ToRotation(), bloom.Size() * new Vector2(0f, 0.5f), bloomScale, 0, 0);

            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/EnergyTex").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value;

            Vector2[] pos_arr = { startPoint, endPoint };
            float[] rot_arr = { projectile.velocity.ToRotation(), projectile.velocity.ToRotation() };

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.0f;

            Color StripColor(float progress) => Color.White;
            float StripWidth1(float progress) => 30f * overallScale * sineWidthMult * colAlpha; //25
            float StripWidth2(float progress) => 120f * overallScale * sineWidthMult * colAlpha; //25

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth1, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);

            #region shaderInfo
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/NoFadeBasicTrailShader", AssetRequestMode.ImmediateLoad).Value;

            float repValue = dist / 800f;
            myEffect.Parameters["reps"].SetValue(repValue * 1f);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.03f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture1);
            myEffect.Parameters["ColorOne"].SetValue((Color.White * colAlpha).ToVector4());
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();
            //vertexStrip1.DrawTrail();

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["ColorOne"].SetValue(lastPrismCol.ToVector4());
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();

            vertexStrip2.DrawTrail();
            //vertexStrip2.DrawTrail();


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }


    }

    public class RainbowSigilTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;
        }

        public override bool? CanDamage() => false;

        public float direction;

        int timer = 0;
        public override void AI()
        {

            //Populate Points
            if (timer == 0)
            {
                int pointsToCreate = 1000;
                float distance = 2000f;

                for (int i = 0; i < pointsToCreate; i++)
                {
                    float distUnit = distance / (float)pointsToCreate;

                    trailPositions.Add(Projectile.Center + new Vector2(distUnit * i, 0f));
                    trailRotations.Add(0f);
                }

                trailPositions.Add(Projectile.Center + new Vector2(distance, 0f));
                trailRotations.Add(0f);

            }

            overallScale = Math.Clamp(MathHelper.Lerp(overallScale, 1.25f, 0.09f), 0f, 1f);

            timer++;
        }

        float overallScale = 0f;
        float overallAlpha = 1f;


        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();
        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {

            RainbowLaser();
            RainbowSigil();

            ///return false;

            /*
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RainbowSigil", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["rotation"].SetValue((float)Main.timeForVisualEffects * 0.03f);
            myEffect.Parameters["rainbowRotation"].SetValue((float)Main.timeForVisualEffects * 0.01f);

            myEffect.Parameters["intensity"].SetValue(1f);
            myEffect.Parameters["fadeStrength"].SetValue(1f);

            Texture2D Sigil = Mod.Assets.Request<Texture2D>("Assets/Orbs/whiteFireEyeA").Value;
            Texture2D Sigil2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/ElectricPopE").Value;

            Vector2 scale = new Vector2(0.3f, 1f) * 1f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);
            myEffect.CurrentTechnique.Passes[0].Apply();

            Main.EntitySpriteDraw(Sigil, Projectile.Center - Main.screenPosition, null, Color.White, 0f, Sigil.Size() / 2f, scale, SpriteEffects.None);
            Main.EntitySpriteDraw(Sigil, Projectile.Center - Main.screenPosition, null, Color.White, 0f, Sigil.Size() / 2f, scale, SpriteEffects.None);
            Main.EntitySpriteDraw(Sigil, Projectile.Center - Main.screenPosition, null, Color.White, 0f, Sigil.Size() / 2f, scale, SpriteEffects.None);

            //Main.EntitySpriteDraw(Sigil2, Projectile.Center - Main.screenPosition, null, Color.White, 0f, Sigil2.Size() / 2f, scale, SpriteEffects.None);

            //Main.EntitySpriteDraw(Sigil, Projectile.Center - Main.screenPosition, null, Color.White, 0f, Sigil.Size() / 2f, scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            RainbowLaser();
            */
            return false;
        }

        Effect laserEffect = null;
        public void RainbowLaser()
        {
            Vector2 startPoint = Projectile.Center;
            Vector2 endPoint = startPoint + new Vector2(2000f, 0f);

            Vector2[] pos_arr = trailPositions.ToArray();// { startPoint, endPoint };
            float[] rot_arr = trailRotations.ToArray();// { 0f, 0f };

            Color StripColor(float progress) => Color.White;
            //float StripWidth1(float progress) => 150f * overallScale; //200

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            if (laserEffect == null)
                laserEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            #region Params

            laserEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            laserEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value); //ThinLineGlowClear
            laserEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/RainbowGrad1").Value);
            laserEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            laserEffect.Parameters["satPower"].SetValue(0.9f);

            laserEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/ThinGlowLine").Value);
            laserEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_06").Value);
            laserEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            laserEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail5Loop").Value);


            laserEffect.Parameters["grad1Speed"].SetValue(2f);
            laserEffect.Parameters["grad2Speed"].SetValue(2f);
            laserEffect.Parameters["grad3Speed"].SetValue(3.1f);
            laserEffect.Parameters["grad4Speed"].SetValue(2.3f);

            laserEffect.Parameters["tex1Mult"].SetValue(1.25f);
            laserEffect.Parameters["tex2Mult"].SetValue(1.5f);
            laserEffect.Parameters["tex3Mult"].SetValue(1.15f);
            laserEffect.Parameters["tex4Mult"].SetValue(2.5f); //1.5
            laserEffect.Parameters["totalMult"].SetValue(1f);

            laserEffect.Parameters["gradientReps"].SetValue(0.75f); //1f
            laserEffect.Parameters["tex1reps"].SetValue(0.15f);
            laserEffect.Parameters["tex2reps"].SetValue(0.15f);
            laserEffect.Parameters["tex3reps"].SetValue(0.15f);
            laserEffect.Parameters["tex4reps"].SetValue(0.15f);

            laserEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.006f); //0.005
            #endregion

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, laserEffect, Main.GameViewMatrix.TransformationMatrix);
            laserEffect.CurrentTechnique.Passes["MainPS"].Apply();

            vertexStrip1.DrawTrail();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        public float StripWidth(float progress)
        {
            float size = 1f * overallScale;
            float start = (float)Math.Cbrt(Utils.GetLerpValue(0f, 0.2f, progress, true));// Math.Clamp(1f * (float)Math.Pow(progress, 0.5f), 0f, 1f);
            float cap = (float)Math.Cbrt(Utils.GetLerpValue(1f, 0.95f, progress, true));
            return 950 * start * overallScale; //150

        }

        public void RainbowSigil()
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Flare/Simple Lens Flare_11").Value;
            Texture2D star2 = Mod.Assets.Request<Texture2D>("Assets/Flare/flare_16").Value;

            Texture2D sigil = Mod.Assets.Request<Texture2D>("Assets/Orbs/whiteFireEyeA").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RainbowSigil", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["rotation"].SetValue((float)Main.timeForVisualEffects * 0.03f * 2f);
            myEffect.Parameters["rainbowRotation"].SetValue((float)Main.timeForVisualEffects * 0.01f * 2f);

            myEffect.Parameters["intensity"].SetValue(1f);
            myEffect.Parameters["fadeStrength"].SetValue(1f);

            float sin1 = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);
            float sin2 = MathF.Cos((float)Main.timeForVisualEffects * 0.06f);
            float sin3 = -MathF.Cos(((float)Main.timeForVisualEffects * 0.05f) / 2f) + 1f;

            Vector2 sigilScale1 = new Vector2(0.2f, 1f) * 0.55f * 5f * overallScale;
            Vector2 sigilScale2 = sigilScale1 * (1.75f + (0.25f * sin1)) * 1f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(sigil, drawPos, null, Color.White, 0f, sigil.Size() / 2, sigilScale1, 0, 0f);
            Main.spriteBatch.Draw(sigil, drawPos, null, Color.White, 0f, sigil.Size() / 2, sigilScale1, 0, 0f);

            Main.spriteBatch.Draw(star, drawPos + new Vector2(1f, 0f) * (15f * sin3), null, Color.White, Projectile.rotation, star.Size() / 2, sigilScale2, 0, 0f);

            Main.spriteBatch.Draw(star2, drawPos, null, Color.White, 0f, star2.Size() / 2, sigilScale1 * 1f, 0, 0f);
            Main.spriteBatch.Draw(star2, drawPos, null, Color.White, 0f, star2.Size() / 2, sigilScale1 * 1f, 0, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
    }

}
