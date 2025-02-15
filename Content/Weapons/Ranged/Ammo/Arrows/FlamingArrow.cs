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
using MonoMod.Cil;
using Mono.Cecil.Cil;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class FlamingArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FireArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = 0;

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 12 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);

            int EU = 1 + projectile.extraUpdates;

            //Want less dust when the arrow has extra updates (magic quiver)
            int mod = Math.Clamp(2 * EU, 2, 100);

            if (timer % mod == 0 && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -2f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) - projectile.velocity * 0.15f;

                Color dustCol = Color.Lerp(Color.OrangeRed, Color.Orange, 0.25f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 0.75f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 0.25f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if ((timer + dustRandomOffsetTime) % mod == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                Color dustCol = Color.Lerp(Color.OrangeRed, Color.Orange, 0.25f);

                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowFlare>(), newColor: dustCol, Scale: Main.rand.NextFloat(0.35f, 0.4f) * 1.25f);
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
                Main.dust[d].customData = new GlowFlareBehavior(0.4f, 2f);
            }


            float fadeInTime = Math.Clamp((timer + 3f * EU) / 15f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.06f), 0f, 1f);

            timer++;

            //So apparently flaming arrow dust is HARD CODED into the Update() function for some reason
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
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;// Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
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
                    Color.White with { A = 0 } * 0.2f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, Color.OrangeRed with { A = 0 } * 0.15f * overallAlpha, projectile.rotation, orb.Size() / 2, new Vector2(0.35f, 0.65f) * overallScale, SE);


            for (int num163 = 220; num163 < 4; num163++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
                Main.EntitySpriteDraw(vanillaTex, offset + drawPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)num163) * 2f, null, Color.White with { A = 0 } * 1f, projectile.rotation, TexOrigin,
                    projectile.scale * overallScale, SE);

            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.Orange with { A = 0 } * 0.65f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            Color betweenOrange = Color.Lerp(Color.Orange, Color.OrangeRed, 0.7f) * overallAlpha;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(betweenOrange, Color.Orange, 1f - Easings.easeInCubic(progress)) * progress * overallAlpha;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;
                float size2 = (0.5f + (0.5f * progress)) * projectile.scale;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.25f,
                    previousRotations[i], TexOrigin, size2 * overallScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size3 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(3f, 0.75f * size3) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, betweenOrange with { A = 0 } * 0.35f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);


                    Vector2 randPos = Main.rand.NextVector2Circular(10f, 10f);

                    Main.EntitySpriteDraw(flare, AfterImagePos + randPos, null, betweenOrange with { A = 0 } * 0.2f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = Math.Clamp(projectile.oldVelocity.Length(), 1f, 7f);
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.6f, 0.7f) * 0.8f);
                dust.velocity += projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.15f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 2f, fadePower: 0.85f, shouldFadeColor: false);
            }

            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num418 = 0; num418 < 4; num418++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Torch);
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }
}
