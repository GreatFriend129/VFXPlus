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
using VFXPlus.Common.Drawing;
using Terraria.Graphics;
using VFXPlus.Content.Weapons.Magic.Hardmode.Staves;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    public class CrystalVileShardBaseOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalVileShardShaft) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.CrystalVileShardToggle;
        }


        public float overallScale = 0f;
        public float overallAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && projectile.ai[1] % 1 == 0)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.3f + (projectile.ai[1] * 0.12f);

                float soundID = Main.rand.Next(3);

                //Both death_2 and death_0 sound really cool (slot machine vibes)
                //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_death_2") with { Volume = 1f, Pitch = pitch2, MaxInstances = -1 };

                SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_crystal_cart_impact_" + soundID) with { Volume = 0.5f, Pitch = pitch2, MaxInstances = -1 }; 
                SoundEngine.PlaySound(style, projectile.Center);
            }

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2.5f));

            if (overallScale == 1f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.05f), 0f, 1f);

            //float adjustedAI1 = Math.Max(projectile.ai[1] - 1, 0f);

            //if (overallScale == 1f && timer > 40 + (2 * adjustedAI1))
            //    overallAlpha = Math.Clamp(overallAlpha - 0.12f, 0f, 1f);
            //overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.01f), 0f, 1f);

            if (timer == 3)
            {
                for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++)
                {
                    Color col = Main.rand.NextBool() ? Color.DeepSkyBlue : Color.DeepPink;

                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(5f, 5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowStarSharp>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: col * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.3f) * projectile.scale * 2f); //3
                }
            }

            timer++;
            return true;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Vector2 vec2Scale = new Vector2(overallScale * projectile.scale, projectile.scale);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    float myAlpha = projectile.Opacity * overallAlpha;

                    Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3.5f, 3.5f), null,
                        Color.White with { A = 20 } * 0.3f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
                }
            });

            Effect myEffect = ModContent.Request<Effect>("Playground/Effects/Filter/Dissolve", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["progress"].SetValue(1f - overallAlpha);

            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Mask/JackOMask2").Value; ;// Mod.Assets.Request<Texture2D>("Assets/Noise/Swirl").Value;
            myEffect.Parameters["maskTexture"].SetValue(Mask);
            myEffect.Parameters["zoom"].SetValue(1f);

            myEffect.Parameters["innerCol"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["outerCol"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["dissolveColMult"].SetValue(0.25f);

            myEffect.Parameters["mainTexWidth"].SetValue((vanillaTex.Width / 2f) - 2); //Vile shard texture is fucked up
            myEffect.Parameters["mainTexHeight"].SetValue(vanillaTex.Height / 2f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            //Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.pixelShader.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);

            return false;            
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }
    }

    public class CrystalVileShardTipShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalVileShardHead) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.FlowerOfFrostToggle;
        }


        float scale = 0;
        float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 4f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);

            if (timer == 3) //10
            {
                for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++)
                {
                    Color col = Main.rand.NextBool() ? Color.DeepSkyBlue : Color.DeepPink;

                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(5f, 5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowStarSharp>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: col * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.3f) * projectile.scale * 3f);
                }
            }

            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    float myAlpha = projectile.Opacity * alpha;

                    Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null,
                        Color.White with { A = 0 } * 0.25f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
                }
            });


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);
            return false;
        }
    }
}
