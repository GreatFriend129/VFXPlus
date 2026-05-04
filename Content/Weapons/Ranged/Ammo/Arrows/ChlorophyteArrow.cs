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
using static Terraria.ModLoader.PlayerDrawLayer;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class ChlorophyteArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ChlorophyteArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = Main.rand.Next(0, 3);

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 40 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            if (timer % 4 == 0 && Main.rand.NextBool() && timer > 15)
            {
                Dust grass = Dust.NewDustPerfect(projectile.Center, DustID.ChlorophyteWeapon, Main.rand.NextVector2Circular(2, 2), 0, Scale: 1f);
                grass.velocity -= projectile.velocity * 0.65f;
                grass.noGravity = true;
                grass.color = Color.Green;
            }

            if (timer > 5 && timer % 4 == 0 && Main.rand.NextBool(3))
            {
                Color between = Color.Lerp(Color.ForestGreen, Color.LawnGreen, 0.65f);

                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: between, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.45f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }


            float EU = 1f + projectile.extraUpdates;

            float fadeInTime = Math.Clamp((timer + 7f * EU) / 20f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;

            #region vanillaAI

            if (timer % 2 == 0 && Main.rand.NextBool(4))
            {
                int num208 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 40);
                Main.dust[num208].noGravity = true;
                Main.dust[num208].scale = 0.9f;
                Main.dust[num208].velocity *= 0.5f;
                Main.dust[num208].velocity -= projectile.velocity * 0.15f;

            }

            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 15f)
            {
                projectile.ai[0] = 15f;
                projectile.velocity.Y += 0.1f;
            }

            if (projectile.type != 344 && projectile.type != 498)
            {
                projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            }

            if (projectile.velocity.Y < -16f)
            {
                projectile.velocity.Y = -16f;
            }
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            #endregion



            return false;
            return base.PreAI(projectile);
        }

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

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawPixelTrail(projectile, false);
            });
            DrawPixelTrail(projectile, true);

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.ForestGreen, Color.DarkGreen, 1f - Easings.easeInCubic(progress)) * Easings.easeInCubic(progress);

                float size1 = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.White * Easings.easeInQuint(progress) * 0.35f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);
            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3f, 3f), sourceRectangle,
                    Color.LawnGreen with { A = 0 } * 0.8f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            return false;
        }

        public void DrawPixelTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count - 2; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.ForestGreen, Color.DarkGreen, 1f - Easings.easeInCubic(progress)) * progress;

                float size1 = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                //Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.35f,
                //    previousRotations[i], TexOrigin, size1 * overallScale, SE);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(3f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, Color.ForestGreen with { A = 0 } * 0.1f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SE);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color dustCol = Color.ForestGreen;

            for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCol, Scale: Main.rand.NextFloat(0.3f, 0.5f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, DustID.ChlorophyteWeapon, dustVel, newColor: Color.DarkGreen, Scale: 0.95f);
                p.noGravity = true;
                p.noLight = true;
            }

            for (int num481 = 0; num481 < 3; num481++)
            {
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, 40);

                Main.dust[a].scale = 0.8f;
                Main.dust[a].noGravity = false;
            }

            SoundStyle style = new SoundStyle("Terraria/Sounds/Grab") with { Pitch = .85f, PitchVariance = .3f, Volume = 0.35f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/NPC_Hit_11") with { Pitch = .05f, PitchVariance = .25f, Volume = 0.45f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            return false;
        }


    }

}
