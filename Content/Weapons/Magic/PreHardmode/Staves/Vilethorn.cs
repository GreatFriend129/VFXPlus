using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Weapons.Magic.Hardmode.Staves;
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Staves
{
    
    public class Vilethorn : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Vilethorn) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.VilethornToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            return true;
        }

    }
    public class VilethornBaseShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VilethornBase) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.VilethornToggle && false;
        }


        public float scale = 0f;
        public float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 8)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);
                float pitch3 = 0.4f + (projectile.ai[1] * 0.12f);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .015f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; 
                SoundEngine.PlaySound(style, projectile.Center);

                //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                //SoundEngine.PlaySound(style2, projectile.Center);

                SoundStyle style3 = new SoundStyle("VFXPlus/Sounds/Effects/Earth/PlantGrowth") with { Volume = 0.15f, Pitch = pitch3, PitchVariance = 0.05f, MaxInstances = -1 };
                SoundEngine.PlaySound(style3, projectile.Center);
            }


            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 3f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);

            timer++;
            return true;
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
                        new Color(61, 2, 92) with { A = 0 } * 0.85f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
                }
            });

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);

            return false;            
        }
    }

    public class VilethornBaseShotOverride2 : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VilethornBase) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.VilethornToggle;
        }


        public float overallScale = 0f;
        public float overallAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 8)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);
                float pitch3 = 0.4f + (projectile.ai[1] * 0.12f);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .015f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, };
                SoundEngine.PlaySound(style, projectile.Center);

                //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                //SoundEngine.PlaySound(style2, projectile.Center);

                SoundStyle style3 = new SoundStyle("VFXPlus/Sounds/Effects/Earth/PlantGrowth") with { Volume = 0.15f, Pitch = pitch3, PitchVariance = 0.05f, MaxInstances = -1 };
                SoundEngine.PlaySound(style3, projectile.Center);
            }


            if (timer >= 2 && timer <= 9 && timer % 3 == 0) //7
            {
                for (int i = 0; i < 1; i++) //5 + Main.rand.Next(1, 3)
                {

                    Vector2 posOffset = Main.rand.NextVector2Circular(7f, 7f) + new Vector2(0f, 0f);
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f);

                    
                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixel>(), vel * Main.rand.NextFloat(0.8f, 1.05f), 
                        newColor: new Color(163, 51, 255), Scale: Main.rand.NextFloat(0.8f, 1.2f) * projectile.scale * 0.75f); //3

                    //new Color(172, 71, 255)
                    //new Color(45, 1, 70)
                    p.velocity += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * -1f;
                    p.velocity *= 0.85f;
                    p.rotation = MathHelper.PiOver4;

                    GlowPixelBehavior gpb = new GlowPixelBehavior(TimeForFadeIn: 1, TimeBeforeFadeOut: 7, VelFadePower: 0.9f, ScaleFadePower: 0.85f, AlphaFadePower: 0.9f, ColorFadePower: 1f);
                    gpb.earlyVelFadePower = Main.rand.NextFloat(0.92f, 0.95f);
                    gpb.randomVelRotatePower = 0.1f;
                    gpb.colorAlpha = 20;

                    p.customData = gpb;
                    

                    /*
                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelCircle>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: new Color(163, 51, 255), Scale: Main.rand.NextFloat(0.8f, 1.2f) * projectile.scale * 0.25f); //3

                    //new Color(172, 71, 255)
                    //new Color(45, 1, 70)
                    p.velocity += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * -1f;
                    p.velocity *= 0.85f;
                    p.rotation = Main.rand.NextFloat(6.28f);

                    GlowPixelBehavior gpb = new GlowPixelBehavior(TimeForFadeIn: 1, TimeBeforeFadeOut: 5, VelFadePower: 0.88f, ScaleFadePower: 0.9f, AlphaFadePower: 0.8f, ColorFadePower: 1f);
                    gpb.earlyVelFadePower = Main.rand.NextFloat(0.92f, 0.95f);
                    gpb.randomVelRotatePower = 0.1f;
                    */

                }

            }

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2f));

            if (overallScale == 1f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.15f, 0.05f), 0f, 1f);

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
                for (int i = 0; i < 4; i++)
                {
                    float myAlpha = projectile.Opacity * Easings.easeInCirc(overallAlpha);
                    Vector2 offset = (2f * (i * MathHelper.PiOver2).ToRotationVector2());

                    Main.spriteBatch.Draw(vanillaTex, drawPos + offset, null,
                        new Color(61, 2, 92) with { A = 20 } * 2f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
                }
            });

            Effect myEffect = ModContent.Request<Effect>("Playground/Effects/Filter/Dissolve", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["progress"].SetValue(1f - overallAlpha);

            //Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Mask/JackOMask4").Value;

            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value;
            //myEffect.Parameters["progress"].SetValue(1f - maskVal);
            myEffect.Parameters["maskTexture"].SetValue(Mask);
            myEffect.Parameters["zoom"].SetValue(1f);

            //myEffect.Parameters["innerCol"].SetValue(new Vector3(1f, 0.5f, 1f));
            myEffect.Parameters["innerCol"].SetValue(new Color(45, 1, 70).ToVector3());
            myEffect.Parameters["outerCol"].SetValue(new Color(45, 1, 70).ToVector3());
            myEffect.Parameters["dissolveColMult"].SetValue(1f);

            myEffect.Parameters["mainTexWidth"].SetValue(vanillaTex.Width / 2f);
            myEffect.Parameters["mainTexHeight"].SetValue(vanillaTex.Height / 2f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.pixelShader.GraphicsDevice.BlendState = BlendState.AlphaBlend;


            return false;
        }
    }


    public class VilethornTipShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VilethornTip) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.VilethornToggle;
        }


        float scale = 0;
        float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 8)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .035f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);
            }


            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 4f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);


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
                        new Color(61, 2, 92) with { A = 0 } * 0.85f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
                }
            });


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);
            return false;
        }
    }


    //This exists literally just so we can draw it in the behind projectiles layer ||| Not currently used but will do so in future
    public class VilethornVFX : ModProjectile
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

            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }

        float alpha = 1f;
        float scale = 0f;

        public int parent = -1;
        int timer = 0;
        public override void AI()
        {
            if (Main.projectile[parent].type == ProjectileID.VilethornBase)
            {
                float timeForPopInAnim = 30;
                float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

                scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 3f));

                if (scale == 1f)
                    alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);
            }
            else if (Main.projectile[parent].type == ProjectileID.VilethornTip)
            {

                float timeForPopInAnim = 30;
                float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

                scale = 0.15f + MathHelper.Lerp(0f, 0.85f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 4f));

                if (scale == 1f)
                    alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);
            }
            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            if (parent == -1) return false;

            Projectile parentProj = Main.projectile[parent];


            Texture2D vanillaTex = TextureAssets.Projectile[parentProj.type].Value;
            Vector2 drawPos = parentProj.Center - Main.screenPosition;
            Vector2 vec2Scale = new Vector2(scale * parentProj.scale, parentProj.scale);


            for (int i = 0; i < 10; i++)
            {
                //Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.PiOver2 * i);
                //Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * parentProj.direction);
                //float myAlpha = parentProj.Opacity * alpha;

                float myAlpha = parentProj.Opacity * alpha;

                Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null,
                    new Color(61, 2, 92) with { A = 0 } * 1f * myAlpha, parentProj.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None, 0f); //1.1f
            }

            return false;
        }

       
    }
}
