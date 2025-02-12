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
using rail;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class WoodenArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WoodenArrowFriendly);
        }

        int dustRandomOffsetTime = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 10; // 14
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);

            if ((timer + dustRandomOffsetTime) % 5 == 0  && Main.rand.NextBool(2) && timer > 5)
            {
                Color thisBrown = new Color(50, 25, 0) * 1.5f;

                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3f, 9f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 0.8f, newColor: thisBrown * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                dp.alpha = 12;
                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0.25f);
            }

            float EU = 1f + projectile.extraUpdates;

            float fadeInTime = Math.Clamp((timer + 5f * EU) / 15f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color thisBrown = new Color(88, 50, 50);

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count - 1; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    //Start End
                    Color col = Color.Lerp(Color.SaddleBrown, Color.White, 1f - Easings.easeInCubic(progress)) * progress;

                    float size1 = (0.5f + (0.5f * progress)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.25f, 
                        previousRotations[i], TexOrigin, size1 * overallScale, SE);

                    if (i > 1)
                    {
                        float middleProg = (float)(i - 1) / previousPostions.Count;

                        float size2 = (0.5f + (0.5f * progress));
                        Vector2 vec2Scale = new Vector2(3f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                        Main.EntitySpriteDraw(flare, AfterImagePos, null, Color.SaddleBrown with { A = 0 } * 0.15f * middleProg,
                            previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SE);
                    }
                }
            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.SandyBrown with { A = 0 } * 0.3f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            if (overallScale != 1)
            {
                Vector2 flareScale = new Vector2(0.5f, 0.3f) * projectile.scale * 1.5f * (1f - overallScale);
                Main.EntitySpriteDraw(flare, drawPos, null, thisBrown with { A = 0 } * 0.25f, projectile.rotation + MathHelper.PiOver2, flare.Size() / 2, flareScale, SE);
                Main.EntitySpriteDraw(flare, drawPos, null, thisBrown with { A = 0 } * 0.75f, projectile.rotation + MathHelper.PiOver2, flare.Size() / 2, flareScale * 0.75f, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 5 + Main.rand.Next(0, 3); i++)
            {
                Vector2 vel = projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3.5f, 11f);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineDust>(), vel.RotateRandom(0.6f) * 1f, newColor: new Color(50, 25, 0) * 1.5f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                dp.alpha = 12;
                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), VelFadeSpeed: 0.83f, SizeChangeSpeed: 0.98f, WhiteIntensity: 0.15f);
            }

            #region vanillaKill
            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num418 = 0; num418 < 5; num418++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 7);
            }
            #endregion

            //return false;
            return false;// base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.Center, oldVelocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
