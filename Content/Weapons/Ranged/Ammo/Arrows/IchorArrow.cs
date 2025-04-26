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
    public class IchorArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.IchorArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = 0;

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 17 + trailOffsetAmount; //14
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
            int mod = Math.Clamp(3 * EU, 3, 100);

            if ((timer + dustRandomOffsetTime) % mod == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                Color dustCol = Color.Lerp(Color.Purple, Color.MediumPurple, 0.5f);

                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowFlare>(), newColor: Color.Orange * 0.5f, Scale: Main.rand.NextFloat(0.35f, 0.4f) * 1f);
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
                Main.dust[d].customData = new GlowFlareBehavior(0.4f, 2f);
            }

            if (timer % 2 == 0 && Main.rand.NextBool())
            {
                int num207 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Ichor, 0f, 0f, 255);
                Main.dust[num207].scale = (float)Main.rand.Next(3, 9) * 0.1f;
                Main.dust[num207].noGravity = true;
                Main.dust[num207].fadeIn = 0.5f;
                Main.dust[num207].velocity *= 0.35f;
                Main.dust[num207].velocity = projectile.velocity * 0.25f;
            }


            float fadeInTime = Math.Clamp((timer + 15f) / 35f, 0f, 1f);
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
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.Orange with { A = 0 } * 0.2f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            Main.EntitySpriteDraw(orb, drawPos, null, Color.Orange with { A = 0 } * 0.1f * overallAlpha, projectile.rotation, orb.Size() / 2, new Vector2(0.35f, 0.65f) * overallScale, SE);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);

            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            Color betweenYellow = Color.Lerp(Color.OrangeRed, Color.Orange, 0.9f);

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                float size1 = 1f * projectile.scale;

                Color grad = Color.Lerp(Color.LightGoldenrodYellow, betweenYellow, Easings.easeOutQuart(1f - progress));

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.Orange with { A = 0 } * Easings.easeInQuart(progress) * 0.25f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);


                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size3 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(3f, 0.75f * size3) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, grad with { A = 0 } * 0.1f * middleProg * overallAlpha,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 220; i < 4; i++)
            {
                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.25f);

                float arrowVel = Math.Clamp(projectile.oldVelocity.Length(), 1f, 7f);
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.7f) * 0.7f);
                dust.velocity += projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.15f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 2f, fadePower: 0.85f, shouldFadeColor: false);
            }

            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num418 = 0; num418 < 3; num418++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.IchorTorch);
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
