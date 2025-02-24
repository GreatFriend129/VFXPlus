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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VFXPlus.Common.Drawing;
using Terraria.GameContent.Drawing;


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
            previousPostions.Add(previousPosition);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            #region dust
            float velPower = (projectile.Center - previousPosition).Length();
            Vector2 velDir = currentRot.ToRotationVector2();

            if (timer % 3 == 0 && Main.rand.NextBool(5) && timer > 3)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.HotPink, Scale: 0.6f);


                Dust dust57 = de;
                Dust dust212 = dust57;
                dust212.velocity *= 0.45f;
                dust57 = de;
                dust212 = dust57;
                dust212.velocity += currentRot.ToRotationVector2() * 6f;

            }

            if (timer % 3 == 0 && timer > 8f)
            {
                Vector2 sideOffset = new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(projectile.velocity.ToRotation());
                Vector2 vel = -velDir * velPower;

                Dust line = Dust.NewDustPerfect(projectile.Center + sideOffset, ModContent.DustType<MuraLineBasic>(), vel, 255,
                    newColor: Color.DeepPink * 0.35f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.75f);

                line.customData = new MuraLineBehavior(new Vector2(1f, 1f), WhiteIntensity: 0.6f);
            }

            #endregion

            timer++;


            pulseIntensity = Math.Clamp(MathHelper.Lerp(pulseIntensity, -0.25f, 0.03f), 0f, 2f);
            alpha = Math.Clamp(MathHelper.Lerp(alpha, 1.25f, 0.04f), 0f, 1f); //0.06

            alpha = 1f;

            return false;
        }


        float alpha = 0f;
        float pulseIntensity = 0f;
        float currentRot = 0f;
        public List<float> previousVelRots = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            CerobaStyleDraw(projectile);

            if (timer < 2)
                return false;

            return true;
        }

        public void CerobaStyleDraw(Projectile projectile)
        {
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

            Main.EntitySpriteDraw(Glow, pos, null, orbCol1 with { A = 0 } * alpha * 0.35f, rot, Glow.Size() / 2f, projectile.scale * scale1 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol2 with { A = 0 } * alpha * 0.35f, rot, Glow.Size() / 2f, projectile.scale * scale2 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol3 with { A = 0 } * alpha * 0.35f, rot, Glow.Size() / 2f, projectile.scale * scale3 * 0.55f, SpriteEffects.None);


            float scale = MathHelper.Lerp(1f, 1.25f, pulseIntensity);


            #region after image
            if (previousVelRots != null && previousPostions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i++)
                {
                    float progress = (float)i / previousVelRots.Count;

                    float size = (1f - (progress * 0.5f)) * projectile.scale;

                    float colVal = progress;

                    Color col = Color.Lerp(Color.Pink * 0.75f, Color.HotPink, progress) * progress * 0.5f;

                    float size2 = (1f - (progress * 0.15f)) * projectile.scale;

                    Main.EntitySpriteDraw(FireBallPixel, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 1.15f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                    Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                    Main.EntitySpriteDraw(FireBall, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 1.25f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
                }

            }
            #endregion

            Vector2 v2scale = new Vector2(1.25f, 1f);

            //black
            Main.EntitySpriteDraw(FireBallPixel, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.Black * thisAlpha * 0.5f, rot, FireBallPixel.Size() / 2f, projectile.scale * v2scale, SpriteEffects.None);


            Main.EntitySpriteDraw(FireBall, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink with { A = 0 } * thisAlpha, rot, FireBall.Size() / 2f, projectile.scale * v2scale, SpriteEffects.None);

            //Pink Star
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            for (int i = 0; i < 6; i++)
            {
                Color col = Color.DeepPink;
                Main.EntitySpriteDraw(VStar, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), null, col with { A = 0 } * 0.8f * thisAlpha, projectile.rotation, VStar.Size() / 2f, scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(VStar, drawPos, null, Color.HotPink * thisAlpha, projectile.rotation, VStar.Size() / 2f, scale * 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(VStarBlack, drawPos, null, Color.White with { A = 0 } * 0.35f * thisAlpha, projectile.rotation, VStarBlack.Size() / 2f, scale * 1.1f, SpriteEffects.None);

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


            //Impact
            for (int i = 0; i < 8; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1.5f;
                Vector2 randomStartOffsetPos = projectile.Center + Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;

                Color col = Main.rand.NextBool(2) ? Color.DarkGoldenrod : Color.HotPink;

                Dust dust = Dust.NewDustPerfect(randomStartOffsetPos, ModContent.DustType<GlowFlare>(), randomStart, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f) * 1.35f);

                dust.customData = new GlowFlareBehavior(0.4f, 2.5f);
            }

            if (previousVelRots != null && previousPostions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i += 1)
                {
                    if (i % 6 == 0 && i > previousPostions.Count * 0.55f)
                    {
                        int a = Dust.NewDust(previousPostions[i], 0, 0, ModContent.DustType<GlowFlare>(), 0, 0, newColor: Color.HotPink, Scale: 0.3f);
                        Main.dust[a].customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);
                        Main.dust[a].velocity *= 0.5f;
                        Main.dust[a].velocity += previousVelRots[i].ToRotationVector2() * 5f;

                    }
                }

            }

            return false;
        }

    }

}
