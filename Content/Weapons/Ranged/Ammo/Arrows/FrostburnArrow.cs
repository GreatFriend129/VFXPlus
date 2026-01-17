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
    public class FrostburnArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FrostburnArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = 0;

        float randomSineOffset = Main.rand.NextFloat();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 11 + trailOffsetAmount;
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
            if (timer % mod == 0 && timer > 10 && Main.rand.NextBool())
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -2f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) - projectile.velocity * 0.15f;

                Color dustCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.25f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 0.75f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: dustCol * 0.15f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if (timer % mod == 0 && Main.rand.NextBool())
            {
                int num4 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.IceTorch, projectile.velocity.X * -0.55f, projectile.velocity.Y * -0.55f, 150,
                    default(Color), 1.3f);
                Main.dust[num4].noGravity = true;
                Main.dust[num4].velocity.X *= 3f;
                Main.dust[num4].velocity.Y *= 3f;
                Main.dust[num4].velocity = (Main.dust[num4].velocity + projectile.velocity) / 2f;
            }


            float fadeInTime = Math.Clamp((timer + 3f * EU) / 15f * EU, 0f, 1f);
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
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;// Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);


            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.White with { A = 0 } * 0.15f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            Main.EntitySpriteDraw(orb, drawPos, null, Color.SkyBlue with { A = 0 } * 0.1f * overallAlpha, projectile.rotation, orb.Size() / 2, new Vector2(0.35f, 0.65f) * overallScale, SE);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.SkyBlue with { A = 0 } * 0.65f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color betweenBlue = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.8f) * overallAlpha; //0.65

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.White, betweenBlue, Easings.easeOutQuad(1f - progress)) * progress * overallAlpha;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;
                float size2 = (0.5f + (0.5f * progress)) * projectile.scale;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.35f,
                    previousRotations[i], TexOrigin, size2 * overallScale, SpriteEffects.None);

                if (i < previousPostions.Count - 1)
                {
                    float yScaleMult = 1f + (float)Math.Sin((Main.timeForVisualEffects * 0.11f) + randomSineOffset) * 0.25f;

                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size3 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(3f, 1f * size3 * yScaleMult) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, col with { A = 0 } * 0.35f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            Color dustCol = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = Math.Clamp(projectile.oldVelocity.Length(), 1f, 7f);
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: dustCol, Scale: Main.rand.NextFloat(0.6f, 0.7f) * 0.8f);
                dust.velocity += projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.15f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 2f, fadePower: 0.85f, shouldFadeColor: false);
            }

            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num418 = 0; num418 < 4; num418++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.IceTorch);
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
