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
using System.Runtime.InteropServices;
using Terraria.GameContent;


namespace VFXPlus.Content.Projectiles
{
    public class BeeOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Bee);
        }

        int timer = 0;
        public List<Vector2> previousPostions = new List<Vector2>();
        public override void AI(Projectile projectile)
        {
            int trailCount = 9; //4
            previousPostions.Add(projectile.Center);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            //projectile.localAI[1] = Math.Clamp(MathHelper.Lerp(projectile.localAI[1], 1.25f, 0.04f), 0f, 1f);
            projectile.localAI[1] = Math.Clamp(MathHelper.Lerp(projectile.localAI[1], 1.25f, 0.07f), 0f, 1f);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 0) / timeForPopInAnim, 0f, 1f);


            //scale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress));

            scale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 2f));

            //float easeVal = Easings.easeInOutBack(inFadePower, 0f, 10f);


            timer++;
            base.AI(projectile);
        }

        float alpha = 0f;
        float scale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float rot = projectile.velocity.ToRotation();
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/GlowCircleFlare").Value;
            Color orbCol1 = Color.DarkGoldenrod with { A = 0 };

            float scale1 = 0.75f * scale;
            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol1 with { A = 0 } * 0.6f, rot, Glow.Size() / 2f, projectile.scale * scale1 * 0.65f, SpriteEffects.None);


            //Bee
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            float drawRot = projectile.rotation;
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (previousPostions != null)
            {
                for (int i = 0; i < previousPostions.Count; i++)
                {
                    float progress = (float)i / previousPostions.Count;

                    float size = (projectile.scale * scale) - (0.33f - (progress * 0.33f));
                    //float size = Easings.easeOutSine(1f * progress) * projectile.scale;
                    //float size = (0.75f + (progress * 0.25f)) * projectile.scale;


                    Color col = Color.Gold with { A = 0 } * progress;


                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col * 0.25f,
                            projectile.rotation, TexOrigin, size, SpriteEffects.None);

                }

            }


            //Border
            for (int i = 0; i < 6; i++)
            {
                Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle, Color.White with { A = 0 } * 1f * projectile.localAI[1], drawRot, TexOrigin, projectile.scale * scale, SE, 0f); //0.3
            }

            for (int i = 10; i < 4; i++)
            {
                float dist = 3f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.01f * projectile.direction);

                float opacity = projectile.Opacity * projectile.localAI[1];
                //Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    //Color.Yellow with { A = 0 } * 0.25f * opacity, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            Main.spriteBatch.Draw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.localAI[1], drawRot, TexOrigin, projectile.scale * scale, SE, 0f); //0.3

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }

    }

    public class HivePackBeeOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.GiantBee;
        }

        int timer = 0;
        public List<Vector2> previousPostions = new List<Vector2>();
        public override void AI(Projectile projectile)
        {
            int trailCount = 9; 
            previousPostions.Add(projectile.Center);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            projectile.localAI[1] = Math.Clamp(MathHelper.Lerp(projectile.localAI[1], 1.25f, 0.07f), 0f, 1f);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 0) / timeForPopInAnim, 0f, 1f);

            scale = 0.5f + MathHelper.Lerp(0f, 0.5f, Easings.easeInOutBack(animProgress, 0f, 1f));



            timer++;
            base.AI(projectile);
        }

        float scale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float rot = projectile.velocity.ToRotation();
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/GlowCircleFlare").Value;
            Color orbCol1 = Color.DarkGoldenrod with { A = 0 };

            float scale1 = 1.15f * scale;
            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol1 with { A = 0 } * 0.6f, rot, Glow.Size() / 2f, projectile.scale * scale1 * 0.65f, SpriteEffects.None);


            //Bee
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            float drawRot = projectile.rotation;
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (previousPostions != null)
            {
                for (int i = 0; i < previousPostions.Count; i++)
                {
                    float progress = (float)i / previousPostions.Count;

                    float size = (projectile.scale * scale) - (0.33f - (progress * 0.33f));

                    Color col = Color.Gold with { A = 0 } * progress;


                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col * 0.25f,
                            projectile.rotation, TexOrigin, size, SpriteEffects.None);

                }

            }


            //Border
            for (int i = 0; i < 6; i++)
            {
                Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle, Color.White with { A = 0 } * 1f * projectile.localAI[1], drawRot, TexOrigin, projectile.scale * scale, SE, 0f); //0.3
            }

            Main.spriteBatch.Draw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.localAI[1], drawRot, TexOrigin, projectile.scale * scale, SE, 0f); //0.3

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

    }


}
