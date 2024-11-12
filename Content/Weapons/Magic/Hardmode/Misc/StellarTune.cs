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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class StellarTune : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SparkleGuitar);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {



            return true;
        }

    }
    public class StellarTuneShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SparkleGuitar);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Vector2 previousPosition = projectile.Center;

            //projectile.scale = 0.5f;

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


            timer++;


            pulseIntensity = Math.Clamp(MathHelper.Lerp(pulseIntensity, -0.25f, 0.03f), 0f, 2f);
            alpha = Math.Clamp(MathHelper.Lerp(alpha, 1.25f, 0.06f), 0f, 1f);

            return false;

            //return base.PreAI(projectile);

            return false;

        }


        float alpha = 0f;
        float pulseIntensity = 0f;
        float currentRot = 0f;
        public List<float> previousVelRots = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //return true;
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                //MarbleStarDraw(projectile);
                //Draw(projectile);
            });

            CerobaStyleDraw(projectile);
            MarbleStarDraw(projectile);

            return true;

            //MarbleStarDraw(projectile);
            //return true;


            return false;
        }

        public void Draw(Projectile projectile)
        {
            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D FireBallPixel = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;

            //Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/ImpactTextures/star_07").Value;
            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;


            Texture2D Buster = Mod.Assets.Request<Texture2D>("Assets/Pixel/BusterGlow").Value;
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 off = (currentRot).ToRotationVector2() * 10f * projectile.scale;

            Vector2 pos = projectile.Center - Main.screenPosition;
            float rot = currentRot + MathHelper.PiOver2;


            Main.EntitySpriteDraw(Glow, pos + off * 1.5f, null, Color.HotPink with { A = 0 } * alpha * 0.5f, rot, Glow.Size() / 2f, projectile.scale * 1f, SpriteEffects.None);

            float starScale = alpha * 2.5f * projectile.scale;
            Main.EntitySpriteDraw(Star, pos + off * 2f, null, Color.DeepPink with { A = 0 } * (1f - alpha) * 1f, rot + (timer * 0.15f), Star.Size() / 2f, starScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, pos + off * 2f, null, Color.White with { A = 0 } * (1f - alpha) * 2f, rot + (timer * 0.15f), Star.Size() / 2f, starScale * 0.75f, SpriteEffects.None);


            Color outerCol = Color.HotPink * 0.5f;//Color.Lerp(Color.DeepSkyBlue * 0.5f, Color.SkyBlue with { A = 0 } * 0.8f, pulseIntensity);
            float scale = MathHelper.Lerp(1f, 1.25f, pulseIntensity);
            for (int i = 0; i < 1; i++)
            {
                Main.EntitySpriteDraw(FireBall, pos, null, outerCol with { A = 0 } * alpha, rot, FireBall.Size() / 2f, projectile.scale * 1f * scale, SpriteEffects.None);
            }

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
                    Main.EntitySpriteDraw(FireBallPixel, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 0.85f * alpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                    Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                    Main.EntitySpriteDraw(FireBall, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 1.25f * alpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
                }

            }
            #endregion

            Vector2 v2scale = new Vector2(1.25f, 1f);

            Main.EntitySpriteDraw(FireBall, pos + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink with { A = 0 } * alpha, rot, FireBall.Size() / 2f, projectile.scale * v2scale, SpriteEffects.None);

            Main.EntitySpriteDraw(FireBall, pos + off, null, Color.White with { A = 0 } * 1f * alpha, rot, FireBall.Size() / 2f, v2scale * projectile.scale * 0.5f, SpriteEffects.None);
        }

        public void MarbleStarDraw(Projectile projectile)
        {
            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D StarBlack = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;
            Texture2D Line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Nightglow").Value;

            float scale = projectile.scale * 1f;

            //Nightglow
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            if (previousVelRots != null && previousPostions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i++)
                {
                    float progress = (float)i / previousVelRots.Count;

                    float size = (1f - (progress * 0.5f)) * scale;

                    float colVal = progress * alpha;

                    Color col = Color.Lerp(Color.HotPink * 0.75f, Color.DeepPink, progress) * progress * 0.25f;

                    float size2 = (1f - (progress * 0.15f)) * scale;
                    Vector2 vec2Scale = new Vector2(2f, 3f) * size;

                    //Black
                    Main.EntitySpriteDraw(Line, previousPostions[i] - Main.screenPosition, null, Color.Black * 0.15f * (colVal * colVal),
                            previousVelRots[i] + MathHelper.PiOver2, Line.Size() / 2f, vec2Scale * size2, SpriteEffects.None);

                    Main.EntitySpriteDraw(StarBlack, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 0.7f * colVal,
                            previousVelRots[i], StarBlack.Size() / 2f, size2, SpriteEffects.None);

                    Main.EntitySpriteDraw(Line, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 2f * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, Line.Size() / 2f, vec2Scale * size2, SpriteEffects.None);

                }

            }

            for (int i = 0; i < 6; i++)
            {
                Color col = Color.DeepPink;
                Main.EntitySpriteDraw(Star, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), null, col with { A = 0 } * 0.8f * alpha, projectile.rotation, Star.Size() / 2f, scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(Star, drawPos, null, Color.HotPink * alpha, projectile.rotation, Star.Size() / 2f, scale * 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(StarBlack, drawPos, null, Color.White with { A = 0 } * 0.35f * alpha, projectile.rotation, StarBlack.Size() / 2f, scale * 1.1f, SpriteEffects.None);

        }


        //trailCount = 34
        public void CerobaStyleDraw(Projectile projectile)
        {
            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D FireBallPixel = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;

            //Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/ImpactTextures/star_07").Value;
            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;


            Texture2D Buster = Mod.Assets.Request<Texture2D>("Assets/Pixel/BusterGlow").Value;
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 off = (currentRot).ToRotationVector2() * 10f * projectile.scale;

            Vector2 pos = projectile.Center - Main.screenPosition;
            float rot = currentRot + MathHelper.PiOver2;

            alpha = 0.15f;

            Main.EntitySpriteDraw(Glow, pos + off * 1.5f, null, Color.HotPink with { A = 0 } * alpha * 0.5f, rot, Glow.Size() / 2f, projectile.scale * 1f, SpriteEffects.None);

            float starScale = alpha * 2.5f * projectile.scale;
            Main.EntitySpriteDraw(Star, pos + off * 2f, null, Color.DeepPink with { A = 0 } * (1f - alpha) * 1f, rot + (timer * 0.15f), Star.Size() / 2f, starScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, pos + off * 2f, null, Color.White with { A = 0 } * (1f - alpha) * 2f, rot + (timer * 0.15f), Star.Size() / 2f, starScale * 0.75f, SpriteEffects.None);


            Color outerCol = Color.HotPink * 0.5f;//Color.Lerp(Color.DeepSkyBlue * 0.5f, Color.SkyBlue with { A = 0 } * 0.8f, pulseIntensity);
            float scale = MathHelper.Lerp(1f, 1.25f, pulseIntensity);
            for (int i = 0; i < 1; i++)
            {
                Main.EntitySpriteDraw(FireBall, pos, null, outerCol with { A = 0 } * alpha, rot, FireBall.Size() / 2f, projectile.scale * 1f * scale, SpriteEffects.None);
            }

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
                    Main.EntitySpriteDraw(FireBallPixel, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 0.85f * alpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                    Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                    Main.EntitySpriteDraw(FireBall, previousPostions[i] - Main.screenPosition, null, col with { A = 0 } * 1.25f * alpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
                }

            }
            #endregion

            Vector2 v2scale = new Vector2(1.25f, 1f);

            Main.EntitySpriteDraw(FireBall, pos + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink with { A = 0 } * alpha, rot, FireBall.Size() / 2f, projectile.scale * v2scale, SpriteEffects.None);

            Main.EntitySpriteDraw(FireBall, pos + off, null, Color.White with { A = 0 } * 1f * alpha, rot, FireBall.Size() / 2f, v2scale * projectile.scale * 0.5f, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;
            
            Vector2 dustStartPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 3f;
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);

                Color starCol = Color.Goldenrod;
                Color lineCol = Main.rand.NextBool() ? Color.Gold : new Color(255, 180, 0);

                if (Main.rand.NextBool())
                {
                    Dust star = Dust.NewDustPerfect(dustStartPos, ModContent.DustType<GlowPixelCross>(),
                        vel, newColor: starCol, Scale: 0.3f + Main.rand.NextFloat(0.0f, 0.07f));

                    star.velocity += projectile.velocity * 0.3f;

                    star.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                                    rotPower: 0.07f, preSlowPower: 0.95f, timeBeforeSlow: 20, postSlowPower: 0.86f, velToBeginShrink: 3f, fadePower: 0.90f, shouldFadeColor: false);
                }
                else
                {
                    Dust d = Dust.NewDustPerfect(dustStartPos, ModContent.DustType<MuraLineBasic>(),
                        Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.75f, 2f), Alpha: Main.rand.Next(10, 15), lineCol, 0.23f);
                    d.velocity += projectile.velocity * 0.35f;
                }

            }

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Pitch = 0.35f, PitchVariance = .12f, Volume = 0.3f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);


            //Dust on Trail
            if (previousVelRots != null && previousPostions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i += 1)
                {
                    if (i % 3 == 0)
                    {
                        int dust2 = Dust.NewDust(previousPostions[i], 1, 1, ModContent.DustType<GlowPixelCross>(), Scale: 0.18f + Main.rand.NextFloat(-0.05f, 0.05f),
                            newColor: Main.rand.NextBool() ? Color.Gold : Color.Goldenrod);
                        Main.dust[dust2].velocity *= 0.5f;
                        Main.dust[dust2].velocity += previousVelRots[i].ToRotationVector2() * projectile.velocity.Length() * 0.25f;
                        Main.dust[dust2].alpha = 5;
                        Main.dust[dust2].noLight = false;
                    }
                }

            }

            return true;
        }

    }

}
