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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class ToxicFlask : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ToxicFlask);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_1") with { Volume = 1f, Pitch = -1f, PitchVariance = .33f, };
            //SoundEngine.PlaySound(style, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_javelin_throwers_attack_2") with { Volume = 0.4f, Pitch = 0.8f, PitchVariance = 0.2f }; 
            SoundEngine.PlaySound(style, player.Center);


            return true;
        }

    }
    public class ToxicFlaskBottleOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ToxicFlask);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 12;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            //projectile.velocity *= 0.9f;


            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);

            drawScale = 0.34f + MathHelper.Lerp(0f, 0.66f, Easings.easeInOutBack(animProgress, 1f, 3f));
            drawAlpha = 1f;// Math.Clamp(MathHelper.Lerp(drawAlpha, 1.25f, 0.08f), 0f, 1f);
            starPower = Math.Clamp(MathHelper.Lerp(starPower, 1.25f, 0.04f), 0f, 1f);

            timer++;

            return base.PreAI(projectile);
        }

        float starPower = 0f;
        float drawAlpha = 0f;
        float drawScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = projectile.scale * drawScale;

                    Color col = Color.Aquamarine * progress * projectile.Opacity * drawAlpha;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.15f,
                            previousRotations[i], TexOrigin, size, SpriteEffects.None);

                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 2.5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.Aqua with { A = 0 } * drawAlpha, projectile.rotation, TexOrigin, projectile.scale * 1.05f * drawScale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity * drawAlpha, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);

            if (starPower == 1f)
                return false;

            //Star
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;


            float dir = projectile.velocity.X > 0 ? 1 : -1;

            float starRotation = MathHelper.Lerp(0f, MathHelper.Pi * 2f * dir, Easings.easeInOutQuad(starPower));
            float starScale = Easings.easeOutCirc(1f - starPower) * projectile.scale * 1f;

            
            Main.EntitySpriteDraw(star, drawPos, null, Color.Aquamarine with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScale, SpriteEffects.None);
            Main.EntitySpriteDraw(star, drawPos, null, Color.White with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScale * 0.5f, SpriteEffects.None);

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 20 + Main.rand.Next(1, 10); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.Aqua * 0.5f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * projectile.scale);
                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(velToBeginShrink: 2f, shouldFadeColor: false, fadePower: 0.9f);
            }

            for (int i = 0; i < 10 + Main.rand.Next(1, 10); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelRise>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.Aquamarine * 0.25f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 2f * projectile.scale);
                p.alpha = 2;
            }

            //return false;
            return base.PreKill(projectile, timeLeft);
        }


    }

    public class ToxicFlaskSmokeOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ToxicCloud || entity.type == ProjectileID.ToxicCloud2 || entity.type == ProjectileID.ToxicCloud3);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            
            timer++;
            return base.PreAI(projectile);
        }



        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);
            return true;

        }

    }

}
