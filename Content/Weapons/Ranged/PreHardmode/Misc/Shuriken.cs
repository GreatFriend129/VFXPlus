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


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class Shuriken : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Shuriken);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 14; 
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);
            previousVelrots.Add(projectile.velocity.ToRotation());

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousVelrots.Count > trailCount)
                previousVelrots.RemoveAt(0);


            if (timer % 5 == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-5f, 5f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(2f, 5f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 1f, newColor: Color.SlateGray * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.3f);
                dp.alpha = 13;

                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0f);
            }

            float fadeInTime = Math.Clamp((timer + 9f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

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
            Vector2 TexOrigin = vanillaTex.Size() / 2f;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    new Color(150, 150, 120) with { A = 0 } * 0.55f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Color thisGray = new Color(150, 150, 120);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(thisGray, Color.Gray, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, col with { A = 0 } * progress * 0.25f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, thisGray with { A = 0 } * 0.15f * middleProg,
                        previousVelrots[i], flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 vel = projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(1f, 3f);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineDust>(), vel.RotateRandom(0.5) * 1f, newColor: Color.SlateGray * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.3f);
                dp.alpha = 13;

                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0f);
            }

            #region vanillaKill
            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num631 = 0; num631 < 8; num631++)
            {
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, 1, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 0, default(Color), 0.75f);

                if (num631 < 4)
                    Main.dust[a].noGravity = true;
            }
            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = 7f;
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: new Color(120, 120, 90), Scale: Main.rand.NextFloat(0.35f, 0.45f));
                dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.35f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

}
