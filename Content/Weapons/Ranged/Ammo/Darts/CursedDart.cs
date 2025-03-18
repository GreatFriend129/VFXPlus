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
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Darts
{
    public class CursedDartOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CursedDart);
        }

        int dustRandomOffsetTime = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Color CursedGreen = new Color(100, 255, 34);

            int trailCount = 19; // 14
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


            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);

            if ((timer + dustRandomOffsetTime) % 5 == 0 && Main.rand.NextBool(4) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3f, 9f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 0.8f, newColor: CursedGreen * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                dp.alpha = 12;
                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0.35f);
            }

            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;

            return true;
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
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
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.GreenYellow with { A = 0 } * 0.5f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
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

            Color CursedGreen = new Color(100, 255, 34) * 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(CursedGreen, CursedGreen, 1f - Easings.easeInCubic(progress)) * progress;

                float size1 = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.45f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, CursedGreen with { A = 0 } * 0.1f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SE);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //SoundEngine.PlaySound(SoundID.Dig, projectile.Center);

            return true;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.Center, oldVelocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }
    public class CursedDartFlameOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CursedDartFlame);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Color CursedGreen = new Color(100, 255, 34);

            for (int i = 220; i < 1; i++)
            {
                Color col = Color.GreenYellow;

                Vector2 vel = new Vector2(0f, -3f);

                Dust d = Dust.NewDustPerfect(projectile.Center + new Vector2(100f, 5f), ModContent.DustType<MediumSmoke>(), Velocity: vel, newColor: CursedGreen with { A = 55 } * 2f, 
                    Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.4f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            if (timer % 2 == 0)
            {
                Vector2 pos = projectile.Center + new Vector2(100f, 4f);

                Vector2 dustVel = new Vector2(0f, Main.rand.NextFloat(-4f, -1f)).RotatedByRandom(0.35f);

                Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: CursedGreen * 1.5f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.25f);
                d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 3, postSlowPower: 0.92f, velToBeginShrink: 1.5f, fadePower: 0.93f, shouldFadeColor: false);

            }


            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;

            return true;
        }

        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D Fire = Mod.Assets.Request<Texture2D>("Content/Weapons/Ranged/Ammo/Darts/CursedFire").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(100f, 0f) + Main.rand.NextVector2Circular(2f, 2f);

            Main.EntitySpriteDraw(Fire, drawPos, null, Color.White with { A = 0 }, 0f, Fire.Size() / 2f, 0.75f * overallScale, SpriteEffects.None);
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //SoundEngine.PlaySound(SoundID.Dig, projectile.Center);

            return true;
        }

    }

}
