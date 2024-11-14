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

            #region dust
            float velPower = (projectile.Center - previousPosition).Length();
            Vector2 velDir = currentRot.ToRotationVector2();

            if (timer % 3 == 0 && Main.rand.NextBool(5) && timer > 3)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.HotPink, Scale: 0.6f);
                //de.customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);


                Dust dust57 = de;
                Dust dust212 = dust57;
                dust212.velocity *= 0.45f;
                dust57 = de;
                dust212 = dust57;
                ///dust212.velocity += currentRot.ToRotationVector2() * velPower * 0.5f;  //currentRot.ToRotationVector2() * 6f;
                dust212.velocity += currentRot.ToRotationVector2() * 6f;

            }

            if (timer % 3 == 0 && timer > 8f)
            {
                Vector2 sideOffset = new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(projectile.velocity.ToRotation());
                Vector2 vel = -velDir * velPower;

                Dust line = Dust.NewDustPerfect(projectile.Center + sideOffset, ModContent.DustType<MuraLineBasic>(), vel, 255,
                    newColor: Color.DeepPink * 0.15f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.75f);

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
            //Utils.DrawBorderString(Main.spriteBatch, "" + projectile.alpha, projectile.Center - Main.screenPosition + new Vector2(40f, 0f), Color.White );
            

            //return true;
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                //MarbleStarDraw(projectile);
                //Draw(projectile);
            });

            CerobaStyleDraw(projectile);
            //MarbleStarDraw(projectile);

            if (timer < 2)
                return false;

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

            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;


            Texture2D VStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D VStarBlack = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;


            Vector2 off = (currentRot).ToRotationVector2() * -25f * projectile.scale; //-30f

            Vector2 pos = projectile.Center - Main.screenPosition;
            float rot = currentRot + MathHelper.PiOver2;

            float thisAlpha = alpha * 0.15f;
            //alpha = 0.15f;

            //Draw 1: Yellow - ish white, Scale of 0.85, Opacity of 0.75
            //Draw 2: Yellow - orange, Scale of 1.6, Opacity of 0.525
            //Draw 3: Orange - red, Scale of 2.5, Opacity of 0.375

            Color orbCol1 = Color.Pink * 0.75f;
            Color orbCol2 = Color.HotPink * 0.525f;
            Color orbCol3 = Color.DeepPink * 0.375f;

            float scale1 = 0.75f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;

            Main.EntitySpriteDraw(Glow, pos, null, orbCol1 with { A = 0 } * alpha * 0.35f, rot, Glow.Size() / 2f, projectile.scale * scale1 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol2 with { A = 0 } * alpha * 0.35f, rot, Glow.Size() / 2f, projectile.scale * scale2 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol3 with { A = 0 } * alpha * 0.35f, rot, Glow.Size() / 2f, projectile.scale * scale3 * 0.55f, SpriteEffects.None);

            //Main.EntitySpriteDraw(Glow, pos, null, Color.HotPink with { A = 0 } * alpha * 10.5f, rot, Glow.Size() / 2f, projectile.scale * 1f, SpriteEffects.None);



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
            //Main.EntitySpriteDraw(FireBall, pos + off, null, Color.White with { A = 0 } * 100f * thisAlpha, rot, FireBall.Size() / 2f, v2scale * projectile.scale * 0.5f, SpriteEffects.None);


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
            for (int num662 = 0; num662 < 6; num662++)
            {
                //Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, 0f, 0f, 150, default(Color), 0.8f);
            }
            for (float num673 = 0f; num673 < 1f; num673 += 0.34f)
            {
                Color col = Color.Lerp(Color.White, Color.HotPink, Main.rand.NextFloat() * 0.5f + 0.5f);
                //Dust.NewDustPerfect(projectile.Center, 278, 1.5f * Vector2.UnitY.RotatedBy(num673 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * ((float)Math.PI * 2f) * 0.5f) * (4f + Main.rand.NextFloat() * 2f), 150, Color.Lerp(Color.White, Color.HotPink, Main.rand.NextFloat() * 0.5f + 0.5f), 0.5f).noGravity = true;
            }
            for (float num684 = 0f; num684 < 1f; num684 += 0.34f)
            {
                //Dust.NewDustPerfect(projectile.Center, 278, 1.5f * Vector2.UnitY.RotatedBy(num684 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * ((float)Math.PI * 2f) * 0.5f) * (2f + Main.rand.NextFloat() * 1f), 150, Color.Lerp(Color.White, Color.Orange, Main.rand.NextFloat() * 0.5f + 0.5f), 0.5f).noGravity = true;
            }
            Vector2 vector11 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector11 / 2f, vector11 + new Vector2(400f))))
            {
                for (int num696 = 0; num696 < 1; num696++)
                {
                    //Gore.NewGore(null, projectile.position, Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * 3f, Utils.SelectRandom<int>(Main.rand, 16));
                }
            }
            
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
            //Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.HotPink, Scale: 0.35f);

            //softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                //overallAlpha: 0.1f, DrawWhiteCore: false, 1f, 1f);


            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.45f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.HotPink * 0.15f);
            d1.customData = cpb2;
            d1.velocity = projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.01f;

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.HotPink * 0.15f);
            d2.customData = cpb2;
            d2.velocity = projectile.velocity.SafeNormalize(Vector2.UnitX) * -0.01f;




            //Impact
            for (int i = 0; i < 8; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1.5f;
                Vector2 randomStartOffsetPos = projectile.Center + Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;

                Color col = Main.rand.NextBool(2) ? Color.DarkGoldenrod : Color.DeepPink;

                Dust dust = Dust.NewDustPerfect(randomStartOffsetPos, ModContent.DustType<GlowFlare>(), randomStart, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.45f) * 1.3f);

                //dust.noLight = false;
                //dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    //rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 12, postSlowPower: 0.88f, velToBeginShrink: 3f, fadePower: 0.89f, shouldFadeColor: false);
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
