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
using Terraria.Graphics;
using VFXPlus.Common.Drawing;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.VFXTest;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class InfernoFork : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.InfernoFork);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }
    public class InfernoForkBoltOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.InfernoFriendlyBolt;
        }


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14; //16
            
            if (timer % 2 == 0)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }


            if (timer % 2 == 0 && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -6f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) - projectile.velocity;

                Color dustCol = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 1f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 0.5f, Scale: dustScale);
                smoke.alpha = 2;
            }

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.1f + MathHelper.Lerp(0f, 0.9f, Easings.easeInOutBack(animProgress, 0f, 1.25f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);
            timer++;
            return false;
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawFireball(projectile, false);
            });

            DrawFireball(projectile, true);

            return false;
        }

        public void DrawFireball(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D FireBallPixel = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float rot = projectile.velocity.ToRotation();

            float totalScale = overallScale * projectile.scale * 0.85f;

            Color betweenOrangeRed = Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f);

            Vector2 off = rot.ToRotationVector2() * -10f * totalScale;
            Main.EntitySpriteDraw(Glow, drawPos, null, Color.OrangeRed with { A = 0 } * overallAlpha * 0.5f, rot + MathHelper.PiOver2, Glow.Size() / 2f, totalScale, SpriteEffects.None);


            Color outerCol = Color.OrangeRed * 0.5f;
            for (int i = 0; i < 1; i++)
            {
                Main.EntitySpriteDraw(FireBall, drawPos + off, null, outerCol with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale, SpriteEffects.None);
            }

            #region after image
            if (previousRotations != null && previousPositions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    Vector2 pos = previousPositions[i] - Main.screenPosition + off;

                    float progress = (float)i / previousRotations.Count;

                    float size = (1f - (progress * 0.5f)) * totalScale;

                    float colVal = progress;

                    Color col = Color.Lerp(Color.Red * 0.75f, betweenOrangeRed, progress) * progress * 0.7f;

                    float size2 = (1f - (progress * 0.15f)) * totalScale;
                    Main.EntitySpriteDraw(FireBallPixel, pos + Main.rand.NextVector2Circular(10f, 10f) * (1f - progress), null, col with { A = 0 } * 0.85f * overallAlpha * colVal,
                            previousRotations[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                    Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                    Main.EntitySpriteDraw(FireBall, pos + Main.rand.NextVector2Circular(0f, 0f) * (1f - progress), null, col with { A = 0 } * 1.25f * overallAlpha * colVal,
                            previousRotations[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
                }

            }
            #endregion

            Vector2 v2scale = new Vector2(1f, 1f);

            Main.EntitySpriteDraw(FireBall, drawPos + off + off + Main.rand.NextVector2Circular(2f, 2f), null, betweenOrangeRed with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale * v2scale, SpriteEffects.None);

            Main.EntitySpriteDraw(FireBall, drawPos + off, null, Color.White with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, v2scale * totalScale * 0.5f, SpriteEffects.None);

        }
    }


    public class InfernoForkVFX : ModProjectile
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

        int timer = 0;

        public override void AI()
        {
            
            timer++;
        }

        float overallScale = 1f;
        float overallAlpha = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawInferno(true);
            });

            DrawInferno(false);

            return false;
        }

        public void DrawInferno(bool giveUp = false)
        {
            if (giveUp)
                return;


            Texture2D ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/impact_2fade2").Value;


            float drawScale = 1f * Projectile.scale * overallScale;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;
            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/foam_mask_bloom").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/FireGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value);
            myEffect.Parameters["flowSpeed"].SetValue(-0.5f);
            myEffect.Parameters["distortStrength"].SetValue(0.1f);
            myEffect.Parameters["colorIntensity"].SetValue(1.5f);

            myEffect.Parameters["uTime"].SetValue(timer * -0.007f);

            //Main.spriteBatch.Draw(ball, Projectile.Center - Main.screenPosition, null, Color.Black * 0.95f, Projectile.rotation, ball.Size() / 2, 0.5f * drawScale, SpriteEffects.None, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(ball, Projectile.Center - Main.screenPosition, null, Color.Orange, Projectile.rotation, ball.Size() / 2, 0.5f * drawScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }

    }

}
