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


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Darts
{
    public class CrystalDartOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalDart);
        }

        int dustRandomOffsetTime = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 24;
            previousRotations.Add(projectile.rotation);
            previousVelrots.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousVelrots.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);


            Color dustCol = new Color(238, 129, 252);
            if ((timer + dustRandomOffsetTime) % 5 == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3f, 9f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 0.8f, newColor: dustCol, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                dp.alpha = 12;
                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0.35f);
            }

            float fadeInTime = Math.Clamp((timer + 16f * 2f) / (35f * 2f), 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousVelrots = new List<float>();
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, true);
            });
            DrawTrail(projectile, false);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.White with { A = 0 } * 0.4f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }
        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.White, Color.DarkGray, 1f - Easings.easeInCubic(progress)) * progress;

                float size1 = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.45f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, Color.HotPink with { A = 0 } * 0.1f * middleProg,
                        previousVelrots[i], flare.Size() / 2f, vec2Scale, SE);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_death_1") with { Volume = .25f, Pitch = .2f, PitchVariance = .15f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            Color dustCol = new Color(238, 129, 252);

            for (int i = 0; i < 6 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: dustCol, Scale: Main.rand.NextFloat(0.25f, 0.45f) * projectile.scale);
            }

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_2") with { Volume = 0.45f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1, };
            SoundEngine.PlaySound(style, projectile.Center);

            Color dustCol = new Color(238, 129, 252);
            for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: dustCol, Scale: Main.rand.NextFloat(0.2f, 0.35f) * projectile.scale);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

    }

}
