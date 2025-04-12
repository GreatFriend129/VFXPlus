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
using Terraria.GameContent.Drawing;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class LuminiteArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MoonlordArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = Main.rand.Next(0, 3);

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 18 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);

            int EU = 1 + projectile.extraUpdates;

            //Want less dust when the arrow has extra updates (magic quiver)
            int mod = Math.Clamp(2 * EU, 2, 100);

            if ((timer + dustRandomOffsetTime) % mod == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowFlare>(), newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.35f, 0.4f) * 0.85f);
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
                Main.dust[d].customData = new GlowFlareBehavior(0.4f, 3f);
            }


            float fadeInTime = Math.Clamp((timer + 7f * EU) / 20f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;

            return true;
        }

        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;
            Texture2D flare2 = CommonTextures.GlowingFlare.Value;

            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Aqua * progress;

                float size1 = 1f * Easings.easeOutSine(progress) * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.4f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);

                Color betweenAqua = Color.Lerp(Color.White, Color.Aqua, 0.15f);
                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPositions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(2f, 0.7f * size2) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare2, AfterImagePos, null, betweenAqua with { A = 0 } * 0.2f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare2.Size() / 2f, vec2Scale, SE);
                }
            }

            Main.EntitySpriteDraw(orb, drawPos, null, Color.DeepSkyBlue with { A = 0 } * 0.15f, projectile.rotation, orb.Size() / 2, new Vector2(0.2f, 0.4f) * overallScale, SE);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.White with { A = 0 } * 0.35f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 } * 0.15f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 6 + Main.rand.Next(0, 3); i++)
            {
                float arrowVel = Math.Clamp(projectile.oldVelocity.Length(), 1f, 7f);
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.6f, 0.7f) * 0.8f);
                dust.velocity += projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.05f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.89f, shouldFadeColor: false);
            }

            #region vanillaKill

            int num = projectile.timeLeft;
            if (projectile.owner == Main.myPlayer && projectile.type == 639)
            {
                int num314 = num + 1;
                int nextSlot = Projectile.GetNextSlot();
                if (Main.ProjectileUpdateLoopIndex < nextSlot && Main.ProjectileUpdateLoopIndex != -1)
                {
                    num314++;
                }
                Vector2 vector38 = new Vector2(projectile.ai[0], projectile.ai[1]);
                Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.localAI[0], projectile.localAI[1], vector38.X, vector38.Y, 640, projectile.damage, projectile.knockBack, projectile.owner, 0f, num314);
            }
            #endregion

            return false;
        }


    }

    public class LuminiteArrowGhostOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MoonlordArrowTrail);
        }

        float storedRotation = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (projectile.velocity != Vector2.Zero)
                storedRotation = projectile.velocity.ToRotation();

            int trailCount = 25;
            previousVelRots.Add(storedRotation);
            previousPositions.Add(projectile.Center);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (projectile.velocity == Vector2.Zero)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.08f), 0f, 1f);

            int EU = 1 + projectile.extraUpdates;

            //Want less dust when the arrow has extra updates (magic quiver)
            int mod = Math.Clamp(2 * EU, 2, 100);

            if (timer % mod == 0 && Main.rand.NextBool(3) && timer > 5)
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowFlare>(), newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.35f, 0.4f) * 0.75f);
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
                Main.dust[d].customData = new GlowFlareBehavior(0.4f, 3f);
            }


            float fadeInTime = Math.Clamp((timer + 3f) / 30f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;

            #region vanillaAI
            projectile.alpha -= 25;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            if (projectile.velocity == Vector2.Zero)
            {
                projectile.ai[0] = 0f;
                bool flag = true;
                for (int num230 = 1; num230 < projectile.oldPos.Length; num230++)
                {
                    if (projectile.oldPos[num230] != projectile.oldPos[0])
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    projectile.Kill();
                    return false;
                }
                if (Main.rand.Next(projectile.extraUpdates) == 0 && (projectile.velocity != Vector2.Zero || Main.rand.Next((projectile.localAI[1] == 2f) ? 2 : 6) == 0) && false)
                {
                    for (int num231 = 0; num231 < 2; num231++)
                    {
                        float num233 = projectile.rotation + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
                        float num234 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                        Vector2 vector13 = new Vector2((float)Math.Cos(num233) * num234, (float)Math.Sin(num233) * num234);
                        int num235 = Dust.NewDust(projectile.Center, 0, 0, 229, vector13.X, vector13.Y);
                        Main.dust[num235].noGravity = true;
                        Main.dust[num235].scale = 1.2f;
                    }
                    if (Main.rand.Next(10) == 0)
                    {
                        Vector2 vector14 = projectile.velocity.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * projectile.width;
                        int num236 = Dust.NewDust(projectile.Center + vector14 - Vector2.One * 4f, 8, 8, 31, 0f, 0f, 100, default(Color), 1.5f);
                        Main.dust[num236].velocity *= 0.5f;
                        Main.dust[num236].velocity.Y = 0f - Math.Abs(Main.dust[num236].velocity.Y);
                    }
                }
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
        }


        float overallScale = 0f;
        float overallAlpha = 1f;
        public List<float> previousVelRots = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(projectile, false);
            });
            DrawAfterImage(projectile, true);

            
            return false;
        }

        public void DrawAfterImage(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            //Texture2D flare = CommonTextures.Flare.Value;
            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            for (int i = 0; i < previousPositions.Count - 1; i++)
            {
                float progress = (float)i / previousPositions.Count;

                //Start End
                Color col = Color.Aqua * progress;

                float size1 = 1f * progress * projectile.scale * overallScale;

                Vector2 lineScale = new Vector2(1.15f, 0.55f) * size1;
                Vector2 lineScale2 = new Vector2(1.15f, 0.3f) * size1;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(flare, AfterImagePos, null, col with { A = 0 } * progress * 0.5f * overallAlpha,
                    previousVelRots[i], flare.Size() / 2f, lineScale, 0);

                Main.EntitySpriteDraw(flare, AfterImagePos, null, Color.White with { A = 0 } * progress * 0.5f * overallAlpha,
                    previousVelRots[i], flare.Size() / 2f, lineScale2, 0);
            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 6 + Main.rand.Next(0, 3); i++)
            {
                float arrowVel = Math.Clamp(projectile.oldVelocity.Length(), 1f, 7f);
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.6f, 0.7f) * 0.8f);
                dust.velocity += projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.05f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.89f, shouldFadeColor: false);
            }

            return false;
        }

    }

}
