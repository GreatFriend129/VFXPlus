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


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class WoodenArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WoodenArrowFriendly);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 20; 
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            //Looks cool, but makes the weapon a little messy, maybe use this for shadowflame knife and/or bow?
            if (timer % 1 == 0 && false)
            {
                Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelAlts>(), Velocity: initVel, newColor: new Color(42, 2, 82), Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.45f);

                a.velocity *= 0.25f;
                a.velocity += projectile.velocity * 0.15f;


                Vector2 posOffset2 = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel2 = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a2 = Dust.NewDustPerfect(projectile.Center + posOffset2 + (projectile.velocity * 0.5f), ModContent.DustType<GlowPixelAlts>(), Velocity: initVel2, newColor: new Color(42, 2, 82), Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.45f);

                a2.velocity *= 0.25f;
                a2.velocity += projectile.velocity * 0.15f;
            }


            /*
            if (timer % 4 == 0 && Main.rand.NextBool() && false)
            {
                Dust grass = Dust.NewDustPerfect(projectile.Center, DustID.JungleGrass, Main.rand.NextVector2Circular(2, 2), 0, Scale: 0.9f);
                grass.velocity += projectile.velocity;
                grass.noGravity = true;
                grass.alpha = 50;
            }
            */

            float fadeInTime = Math.Clamp((timer + 5f) / 15f, 0f, 1f);
            fadeInAlpha = Easings.easeInCirc(fadeInTime);

            timer++;
            return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/AnotherLineGlow").Value;


            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = (0.75f + (progress * 0.25f)) * projectile.scale;

                    Color col = Color.Lerp(Color.White, Color.SaddleBrown, 1f - Easings.easeInCubic(progress)) * progress * projectile.Opacity;

                    float size2 = (0.75f + (0.25f * progress)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f * progress, 
                        previousRotations[i], TexOrigin, size2 * fadeInAlpha, SpriteEffects.None);


                    if (i > 1)
                    {
                        float middleProg = (float)(i - 1) / previousPostions.Count;
                        Vector2 vec2Scale = new Vector2(1f, 0.75f * size) * fadeInAlpha * 0.4f;
                        Main.EntitySpriteDraw(spike, AfterImagePos, null, Color.SaddleBrown with { A = 0 } * 0.85f * middleProg,
                            previousRotations[i] + MathHelper.PiOver2, spike.Size() / 2f, vec2Scale, SpriteEffects.None);
                    }

                }



            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.SandyBrown with { A = 0 } * 0.3f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * fadeInAlpha, SpriteEffects.None);
            }


            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * fadeInAlpha, SpriteEffects.None);
            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
