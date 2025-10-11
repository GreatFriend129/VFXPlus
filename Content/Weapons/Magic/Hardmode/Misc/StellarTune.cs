using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class StellarTuneShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SparkleGuitar) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.StellarTuneToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Vector2 previousPosition = projectile.Center;

            #region vanillaCode
            float num = 90f;
            if ((projectile.localAI[0] += 1f) >= num - 1f)
            {
                projectile.Kill();
                return false;
            }
            float num2 = projectile.localAI[0] / num;
            Vector2 center = Main.player[projectile.owner].Center;
            Vector2 vector = new Vector2(projectile.ai[0], projectile.ai[1]);
            Vector2 vector2 = -projectile.velocity;
            Vector2 value = center + vector2 * 2f;
            Vector2 value2 = vector + vector2 * (1f - num2 * 3f);

            projectile.Center = Vector2.CatmullRom(value, center, vector, value2, num2);
            if (projectile.type == 856)
            {
                Lighting.AddLight(projectile.Center, Color.HotPink.ToVector3() * 0.3f);
                projectile.rotation = (float)Math.PI * 2f * num2 * 1f;
            }
            #endregion

            currentRot = (projectile.Center - previousPosition).ToRotation();

            int trailCount = 34;
            previousVelRots.Add(currentRot);
            previousPositions.Add(previousPosition);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            #region dust
            float velPower = (projectile.Center - previousPosition).Length();
            Vector2 velDir = currentRot.ToRotationVector2();

            if (timer % 3 == 0 && Main.rand.NextBool(5) && timer > 3)
            {
                Color betweenCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);

                //Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                //Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.Lerp(Color.HotPink, Color.Pink, 0.5f), Scale: 0.6f);
                //de.customData = new GlowFlareBehavior(0.5f, 2.5f, 2f);

                //Dust dust57 = de;
                //Dust dust212 = dust57;
                //dust212.velocity *= 0.45f;
                //dust57 = de;
                //dust212 = dust57;
                //dust212.velocity += currentRot.ToRotationVector2() * 6f;

                //Color betweenCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
                Vector2 randomStart = Main.rand.NextVector2Circular(2f, 2f) * 1.75f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), randomStart, newColor: betweenCol, Scale: Main.rand.NextFloat(0.65f, 0.75f) * 0.3f);

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GSSBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 2, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.94f, shouldFadeColor: false);

                Dust dust57 = dust;
                Dust dust212 = dust57;
                dust212.velocity *= 0.45f;
                dust57 = dust;
                dust212 = dust57;
                dust212.velocity += currentRot.ToRotationVector2() * 6f;

            }

            if (timer % 4 == 0 && timer > 8f)
            {
                Vector2 sideOffset = new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(projectile.velocity.ToRotation());
                Vector2 vel = -velDir * velPower;

                Dust line = Dust.NewDustPerfect(projectile.Center + sideOffset, ModContent.DustType<MuraLineBasic>(), vel, 255,
                    newColor: Color.DeepPink * 0.35f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.75f);

                line.customData = new MuraLineBehavior(new Vector2(0.75f, 1f), WhiteIntensity: 0.45f);
            }

            #endregion

            timer++;


            pulseIntensity = Math.Clamp(MathHelper.Lerp(pulseIntensity, -0.25f, 0.03f), 0f, 2f);
            alpha = Math.Clamp(MathHelper.Lerp(alpha, 3f, 0.08f), 0f, 1f); //0.06

            //alpha = 1f;

            return false;
        }


        float alpha = 0f;
        float pulseIntensity = 0f;
        float currentRot = 0f;
        public List<float> previousVelRots = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (timer < 1)
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                CerobaStyleDraw(projectile, false);
            });
            CerobaStyleDraw(projectile, true);

            float scale = MathHelper.Lerp(1f, 1.25f, pulseIntensity) * alpha;
            float easedStarScale = Easings.easeInCubic(alpha);

            Texture2D VStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D VStarBlack = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;
            //Pink Star
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            for (int i = 0; i < 6; i++)
            {
                Color col = Color.DeepPink;
                Main.EntitySpriteDraw(VStar, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), null, col with { A = 0 } * 0.8f * 0.15f * easedStarScale, projectile.rotation, VStar.Size() / 2f, scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(VStar, drawPos, null, Color.HotPink * 0.15f, projectile.rotation, VStar.Size() / 2f, scale * 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(VStarBlack, drawPos, null, Color.White with { A = 0 } * 0.35f * 0.15f * easedStarScale, projectile.rotation, VStarBlack.Size() / 2f, scale * 1.1f, SpriteEffects.None);


            #region vanillaDraw
            Color color = new Color(255, 255, 255, lightColor.A - projectile.alpha);

            if (true) //Draw normal star
            {
                float rotation = projectile.rotation + projectile.localAI[1];
                _ = (float)Main.timeForVisualEffects / 240f;
                _ = Main.GlobalTimeWrappedHourly;
                float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
                globalTimeWrappedHourly %= 5f;
                globalTimeWrappedHourly /= 2.5f;
                if (globalTimeWrappedHourly >= 1f)
                {
                    globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
                }
                globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
                Vector2 position = projectile.Center - Main.screenPosition;
                Main.instance.LoadItem(75);
                Texture2D value4 = TextureAssets.Item[75].Value;
                Rectangle rectangle = value4.Frame(1, 8);
                Main.EntitySpriteDraw(origin: rectangle.Size() / 2f, texture: value4, position: position, sourceRectangle: rectangle, color: color * easedStarScale, rotation: rotation, scale: projectile.scale * scale, effects: SpriteEffects.None);
            }
            #endregion

            return false;
        }

        public void CerobaStyleDraw(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;
            float easedStarScale = Easings.easeInCubic(alpha);


            Texture2D FireBall = CommonTextures.FireBallBlur.Value;
            Texture2D FireBallPixel = CommonTextures.Extra_91.Value;

            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;

            Texture2D VStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D VStarBlack = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;

            Vector2 off = (currentRot).ToRotationVector2() * -25f * projectile.scale; 

            Vector2 pos = projectile.Center - Main.screenPosition;
            float rot = currentRot + MathHelper.PiOver2;

            float thisAlpha = alpha * 0.15f;

            Color orbCol1 = Color.Pink * 0.75f;
            Color orbCol2 = Color.HotPink * 0.525f;
            Color orbCol3 = Color.DeepPink * 0.375f;

            float scale1 = 0.75f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            Vector2 orbScale = new Vector2(0.75f, 1f) * projectile.scale * 0.55f * easedStarScale;

            Main.EntitySpriteDraw(Glow, pos + off * 0.35f, null, orbCol1 with { A = 0 } * alpha * 0.45f, rot, Glow.Size() / 2f, orbScale * scale1, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos + off * 0.35f, null, orbCol2 with { A = 0 } * alpha * 0.45f, rot, Glow.Size() / 2f, orbScale * scale2, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos + off * 0.35f, null, orbCol3 with { A = 0 } * alpha * 0.45f, rot, Glow.Size() / 2f, orbScale * scale3, SpriteEffects.None);


            float scale = MathHelper.Lerp(1f, 1.25f, pulseIntensity);


            #region after image
            for (int i = 0; i < previousVelRots.Count; i += 2)
            {
                float progress = (float)i / previousVelRots.Count;

                float size = (1f - (progress * 0.5f)) * projectile.scale;

                float colVal = progress;

                Color col = Color.Lerp(Color.Pink * 0.75f, Color.HotPink, progress) * progress * 0.5f;

                float size2 = (1f - (progress * 0.15f)) * projectile.scale * easedStarScale;

                Main.EntitySpriteDraw(FireBallPixel, previousPositions[i] - Main.screenPosition + off, null, col with { A = 0 } * 1.15f * thisAlpha * colVal,
                        previousVelRots[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size * easedStarScale;

                Main.EntitySpriteDraw(FireBall, previousPositions[i] - Main.screenPosition + off, null, col with { A = 0 } * 1.25f * thisAlpha * colVal,
                        previousVelRots[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
            }
            #endregion

            Vector2 v2scale = new Vector2(1.25f, 1f);

            //black
            Main.EntitySpriteDraw(FireBallPixel, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink * thisAlpha * 0.15f, rot, FireBallPixel.Size() / 2f, projectile.scale * v2scale, SpriteEffects.None);
            Main.EntitySpriteDraw(FireBall, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink with { A = 0 } * thisAlpha, rot, FireBall.Size() / 2f, projectile.scale * v2scale, SpriteEffects.None);

            #region vanillaDraw
            Color color = new Color(255, 255, 255, Color.White.A - projectile.alpha);
            Vector2 vector = projectile.velocity;
            Color color2 = Color.Blue * 0.1f;
            Vector2 spinningpoint = new Vector2(0f, -4f);
            float num = 0f;
            float t = vector.Length();
            float num2 = Utils.GetLerpValue(3f, 5f, t, clamped: true);
            bool flag = true;
            if (projectile.type == 856 || projectile.type == 857) //TRUE
            {
                vector = projectile.position - projectile.oldPos[1];
                float num3 = vector.Length();
                if (num3 == 0f)
                {
                    vector = Vector2.UnitY;
                }
                else
                {
                    vector *= 5f / num3;
                }
                Vector2 origin = new Vector2(projectile.ai[0], projectile.ai[1]);
                Vector2 center = Main.player[projectile.owner].Center;
                float lerpValue = Utils.GetLerpValue(0f, 120f, origin.Distance(center), clamped: true);
                float num4 = 90f;
                if (projectile.type == 857)
                {
                    num4 = 60f;
                    flag = false;
                }
                float lerpValue2 = Utils.GetLerpValue(num4, num4 * (5f / 6f), projectile.localAI[0], clamped: true);
                float lerpValue3 = Utils.GetLerpValue(0f, 120f, projectile.Center.Distance(center), clamped: true);
                lerpValue *= lerpValue3;
                lerpValue2 *= Utils.GetLerpValue(0f, 15f, projectile.localAI[0], clamped: true);
                color2 = Color.HotPink * 0.15f * (lerpValue2 * lerpValue);
                if (projectile.type == 857)
                {
                    color2 = projectile.GetFirstFractalColor() * 0.15f * (lerpValue2 * lerpValue);
                }
                spinningpoint = new Vector2(0f, -2f);
                float lerpValue4 = Utils.GetLerpValue(num4, num4 * (2f / 3f), projectile.localAI[0], clamped: true);
                lerpValue4 *= Utils.GetLerpValue(0f, 20f, projectile.localAI[0], clamped: true);
                num = -0.3f * (1f - lerpValue4);
                num += -1f * Utils.GetLerpValue(15f, 0f, projectile.localAI[0], clamped: true);
                num *= lerpValue;
                num2 = lerpValue2 * lerpValue;
            } 
            Vector2 vector5 = projectile.Center + vector;
            Texture2D value = TextureAssets.Projectile[projectile.type].Value;
            _ = new Rectangle(0, 0, value.Width, value.Height).Size() / 2f;
            Texture2D value2 = CommonTextures.FireBallBlur.Value;// 
            Rectangle value3 = value2.Frame();
            Vector2 origin2 = new Vector2((float)value3.Width / 2f, 10f);
            _ = Color.Cyan * 0.5f * num2;
            Vector2 vector2 = new Vector2(0f, projectile.gfxOffY);
            float num5 = (float)Main.timeForVisualEffects / 60f;
            Vector2 vector3 = vector5 + vector * 0.5f;
            Color color3 = Color.White * 0.5f * num2;
            color3.A = 0;
            Color color4 = color2 * num2;
            color4.A = 0;
            Color color5 = color2 * num2;
            color5.A = 0;
            Color color6 = color2 * num2;
            color6.A = 0;
            float num6 = vector.ToRotation();
            num *= 1.5f;
            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5), value3, color4 * alpha, num6 + (float)Math.PI / 2f, origin2, 1.5f + num, SpriteEffects.None);
            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + (float)Math.PI * 2f / 3f), value3, color5 * alpha, num6 + (float)Math.PI / 2f, origin2, 1.1f + num, SpriteEffects.None);
            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + 4.18879032f), value3, color6 * alpha, num6 + (float)Math.PI / 2f, origin2, 1.3f + num, SpriteEffects.None);
            //^ Outer fireballs

            Vector2 vector4 = vector5 - vector * 0.5f;
            for (float num7 = 0f; num7 < 1f; num7 += 0.5f) //draw white inner fireball
            {
                float num8 = num5 % 0.5f / 0.5f;
                num8 = (num8 + num7) % 1f;
                float num9 = num8 * 2f;
                if (num9 > 1f)
                {
                    num9 = 2f - num9;
                }
                Main.EntitySpriteDraw(value2, vector4 - Main.screenPosition + vector2, value3, color3 * num9 * alpha, num6 + (float)Math.PI / 2f, origin2, 0.3f + num8 * 0.5f, SpriteEffects.None);
            }
            #endregion
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            #region vanillaKill
            
            ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.StellarTune, new ParticleOrchestraSettings
            {
                PositionInWorld = projectile.Center
            }, projectile.owner); 
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = 128);
            projectile.Center = projectile.position;
            projectile.maxPenetrate = -1;
            projectile.penetrate = -1;
            projectile.Damage();
            #endregion

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.HotPink, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.11f, DrawWhiteCore: false, 1f, 1f);

            Color betweenCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
            for (int i = 0; i < 7 + Main.rand.Next(0, 5); i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.DarkGoldenrod : betweenCol;

                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1.75f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), randomStart, newColor: col, Scale: Main.rand.NextFloat(0.65f, 0.75f) * 0.65f);

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GSSBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 2, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            //Impact
            for (int i = 220; i < 8; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1.5f;
                Vector2 randomStartOffsetPos = projectile.Center + Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;

                Color col = Main.rand.NextBool(2) ? Color.DarkGoldenrod : Color.HotPink;

                Dust dust = Dust.NewDustPerfect(randomStartOffsetPos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f) * 1.35f);

                //dust.customData = new GlowFlareBehavior(0.4f, 2.5f);
            }

            for (int i = 0; i < previousVelRots.Count; i += 1)
            {
                if (i % 6 == 0 && i > previousPositions.Count * 0.55f)
                {
                    int a = Dust.NewDust(previousPositions[i], 0, 0, ModContent.DustType<GlowFlare>(), 0, 0, newColor: Color.HotPink, Scale: 0.3f);
                    Main.dust[a].customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);
                    Main.dust[a].velocity *= 0.5f;
                    Main.dust[a].velocity += previousVelRots[i].ToRotationVector2() * 5f;

                }
            }

            return false;
        }

    }

}
