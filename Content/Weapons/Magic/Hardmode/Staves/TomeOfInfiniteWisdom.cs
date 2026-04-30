using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class TomeOfInfiniteWisdomItemOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BookStaff) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.TomeOfInfiniteWisdomToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int dustCount = player.altFunctionUse == 2 ? 12 : 3;

            for (int i = 0; i < dustCount + Main.rand.Next(2); i++)
            {
                Vector2 posOff = Main.rand.NextVector2Circular(1.5f, 1.5f);

                if (player.altFunctionUse == 2)
                {
                    Color col = Main.rand.NextBool(4) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;

                    Vector2 velOff = Main.rand.NextVector2Circular(1f, 3.5f).RotatedBy(velocity.ToRotation());

                    Dust sa = Dust.NewDustPerfect(position + posOff + velocity.SafeNormalize(Vector2.UnitX) * 44f, ModContent.DustType<GlowPixelCross>(),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.75f, 3f) + velOff, 0,
                        col * 0.5f, Main.rand.NextFloat(0.35f, 0.5f) * 0.7f);

                    sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: false);
                }
                else
                {
                    Color col = Main.rand.NextBool(2) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;
                    Vector2 v = Main.rand.NextVector2Circular(1.5f, 1.5f);

                    Dust sa = Dust.NewDustPerfect(position + v + velocity.SafeNormalize(Vector2.UnitX) * 44f, ModContent.DustType<GlowPixelCross>(),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.9f) * Main.rand.NextFloat(1f, 4f), 0,
                        col * 1f, Main.rand.NextFloat(0.35f, 0.5f) * 0.65f);

                    sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: false);
                }


            }

            return true;
        }

    }
    public class ToIWShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BookStaffShot) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.TomeOfInfiniteWisdomToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //The vanilla projectile starts at 255 alpha
            //It randomizes the frame if alpha = 255 
            //It creates dust and and sets alpha to 0 if alpha == 255

            //We dont want it to create the dust, so we will set alpha to 0 and randomize the frame ourselves
            projectile.alpha = 0;
            if (timer == 0)
            {
                projectile.frame = Main.rand.Next(2) * 4;
            }

            int trailCount = 14; //8
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 3 == 0 && Main.rand.NextBool() && timer > 8)
            {
                Color col = Main.rand.NextBool(2) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(2f, 4f) * -1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.15f, 0.2f) * 1.5f);
            }

            float fadeInTime = Math.Clamp((timer + 5f) / 15f, 0f, 1f);
            fadeInAlpha = Easings.easeInCirc(fadeInTime);

            timer++;

            return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAE(projectile);
            });

            //Border
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * 0.7f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * fadeInAlpha, SpriteEffects.None);
            }


            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * fadeInAlpha, SpriteEffects.None);

            return false;
        }

        public void DrawAE(Projectile projectile)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPositions != null)
            {
                Texture2D trailLine = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;


                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.White * progress;

                    float size2 = (0.5f + (progress * 0.5f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.35f * progress, //0.5f
                            previousRotations[i], TexOrigin, size2 * fadeInAlpha, SpriteEffects.None);


                    Color innerCol = Color.Lerp(Color.LightSkyBlue, Color.Tan, 1f);

                    Vector2 vec2Scale = new Vector2(0.55f, 0.35f * progress) * projectile.scale;
                    Main.EntitySpriteDraw(trailLine, AfterImagePos, null, innerCol with { A = 0 } * 0.35f * progress,
                        projectile.velocity.ToRotation(), trailLine.Size() / 2f, vec2Scale, SpriteEffects.None);

                }
            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 5 + Main.rand.Next(1, 4); i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;

                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: col * 1f, Scale: Main.rand.NextFloat(0.35f, 0.45f) * projectile.scale);
                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(velToBeginShrink: 2f, shouldFadeColor: false, fadePower: 0.9f);
            }

            //Vanilla code but without dust (still has gore):
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            SoundEngine.PlaySound(in SoundID.Item10, projectile.position);
            int num693 = Main.rand.Next(6, 12);
            for (int num694 = 220; num694 < num693; num694++)
            {
                int num695 = Dust.NewDust(projectile.Center, 0, 0, 15, 0f, 0f, 100);
                Dust dust236 = Main.dust[num695];
                Dust dust3 = dust236;
                dust3.velocity *= 1.6f;
                Main.dust[num695].velocity.Y -= 1f;
                dust236 = Main.dust[num695];
                dust3 = dust236;
                dust3.velocity += -projectile.velocity * (Main.rand.NextFloat() * 2f - 1f) * 0.5f;
                Main.dust[num695].scale = 1f;
                Main.dust[num695].fadeIn = 1.5f;
                Main.dust[num695].noGravity = true;
                Main.dust[num695].color = new Color(255, 255, 255, 0) * 0.3f;
                dust236 = Main.dust[num695];
                dust3 = dust236;
                dust3.velocity *= 0.7f;
                dust236 = Main.dust[num695];
                dust3 = dust236;
                dust3.position += Main.dust[num695].velocity * 5f;
            }
            for (int num696 = 0; num696 < 3; num696++)
            {
                Gore gore33 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, 1008, 1f + Main.rand.NextFloatDirection() * 0.2f);
                Gore gore34 = gore33;
                Gore gore3 = gore34;
                gore3.velocity *= 4f;
            }

            return false;
        }

    }

    public class ToIWTornadoOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.DD2ApprenticeStorm) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.TomeOfInfiniteWisdomToggle;
        }

        SlotId soundSlot;
        SoundStyle soundStyleTwister = SoundID.DD2_BookStaffTwisterLoop with 
        {
            PauseBehavior = PauseBehavior.PauseWithGame,
            IsLooped = true
        };


        float initialAnimProgress = 0f;

        float overallScale = 1f;
        float overallAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            #region TrailStuff
            Vector2 ProjectileBottom = projectile.Center + new Vector2(0f, projectile.height / 2f);

            if (timer != 0)
            {
                ProjectileBottom.Y = MathHelper.Lerp(ProjectileBottom.Y, projectile.localAI[2], 0.75f);
            }


            float leanProgress = Easings.easeOutQuad(Utils.GetLerpValue(0, 40, timer, true));

            int numberOfPoints = 20;
            float tornadoHeight = 220 * projectile.scale;

            trailPositions.Clear();
            trailRotations.Clear();

            Vector2 previousPos = Vector2.Zero;

            for (int i = 0; i < numberOfPoints; i++)
            {
                float prog = (float)i / (float)(numberOfPoints - 1);

                float offsetMult = Easings.easeOutQuad(GeneralUtilities.FadeLinear(prog, 0.4f, 0.6f));

                float sinVal = (float)Math.Sin(timer * 0.02f) * 1f;

                float offsetMult2 = Easings.easeInQuad(GeneralUtilities.FadeLinear(prog, 0.6f, 0.4f));

                //50
                Vector2 offset = new Vector2(45f * offsetMult * Math.Sign(projectile.velocity.X) * Easings.easeInSine(overallAlpha) * leanProgress, -tornadoHeight * prog);
                //offset.X += -50 * (offsetMult2) * sinVal;

                trailPositions.Add(ProjectileBottom + offset);

                if (i == 0)
                    trailRotations.Add(-MathHelper.PiOver2);
                else
                    trailRotations.Add((offset - previousPos).ToRotation());

                previousPos = offset;
            }
            #endregion


            //Store the y
            projectile.localAI[2] = ProjectileBottom.Y;
            
            //Smoke at bottom
            Color smokeCol = Color.Lerp(new Color(84, 71, 55), Color.Tan, 0.65f);
            if (timer % 1 == 0)
            {

                Vector2 thisVel = new Vector2(projectile.velocity.X, -4f).RotateRandom(0.25f);
                Dust dad = Dust.NewDustPerfect(ProjectileBottom + projectile.velocity, ModContent.DustType<MediumSmoke>(), Velocity: thisVel,
                    newColor: smokeCol * 1.5f, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.75f);
                dad.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 0.15f); //12 28
                dad.rotation = Main.rand.NextFloat(6.28f);
            }

            if (timer % 1 == 0 && overallAlpha >= 0.5f)
            {
                for (int i = 0; i < 1; i++)
                {
                    Vector2 dustPos = projectile.Center + Main.rand.NextVector2Circular(40, 105f);
                    Vector2 dustVel = new Vector2(3f * Math.Sign(projectile.velocity.X), 0f) * Main.rand.NextFloat(1f, 1.5f);
                    Color dustCol = Main.rand.NextBool(4) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.75f) : Color.Tan;

                    
                    dustVel.X += projectile.velocity.X * 1f;


                    Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixel>(), dustVel, newColor: dustCol, Scale: Main.rand.NextFloat(0.25f, 0.35f));
                    d.velocity.Y = Main.rand.NextFloat() * -0.5f - 1.3f;

                    int timeBeforeFade = Main.rand.Next(5, 10);

                    GlowPixelBehavior gpb = new GlowPixelBehavior(TimeForFadeIn: 3, TimeBeforeFadeOut: timeBeforeFade, VelFadePower: 0.92f, ScaleFadePower: 1f, AlphaFadePower: 0.9f, ColorFadePower: 0.9f);
                    //gpb.earlyVelFadePower = Main.rand.NextFloat(0.f, 0.95f);
                    d.customData = gpb;
                    

                    //Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowFlare>(), dustVel, newColor: dustCol, Scale: 0.35f);
                    //d.velocity.Y = Main.rand.NextFloat() * -0.5f - 1.3f;
                }
            }

            //Looping sound
            //Had some issues with having the vanilla code handle it so we're just gonna redo it ourselves
            if (!SoundEngine.TryGetActiveSound(soundSlot, out var _))
            {
                var tracker = new ProjectileAudioTracker(projectile);
                soundSlot = SoundEngine.PlaySound(soundStyleTwister, projectile.Center, soundInstance => {
                    soundInstance.Position = projectile.Center;
                    soundInstance.Volume = 1f - Math.Max(projectile.ai[0] - (300f - 15f), 0f) / 15f;

                    return tracker.IsActiveAndInGame();
                });
            }



            float fadeInProgress = Math.Clamp((float)timer / 35f, 0f, 1f);
            overallAlpha = Easings.easeInOutQuad(fadeInProgress);
            overallAlpha *= 1f - Utils.GetLerpValue(285, 300, projectile.ai[0], true);

            timer++;

            #region VanillaAI
            float num = 300f;
            SlotId val;
            //if (projectile.soundDelay == 0)
            //{
            //    projectile.soundDelay = -1;
            //    float[] array = projectile.localAI;
            //    val = SoundEngine.PlayTrackedSound(in SoundID.DD2_BookStaffTwisterLoop, base.Center);
            //    array[1] = ((SlotId)(ref val)).ToFloat();
            //}
            //ActiveSound activeSound = SoundEngine.GetActiveSound(SlotId.FromFloat(this.localAI[1]));
            //if (activeSound != null)
            //{
            //    activeSound.Position = base.Center;
            //    activeSound.Volume = 1f - Math.Max(this.ai[0] - (num - 15f), 0f) / 15f;
            //}
            //else
            //{
            //    float[] array2 = this.localAI;
            //    val = SlotId.Invalid;
            //    array2[1] = ((SlotId)(ref val)).ToFloat();
            //}
            if (projectile.localAI[0] >= 16f && projectile.ai[0] < num - 15f)
            {
                projectile.ai[0] = num - 15f;
            }
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= num)
            {
                projectile.Kill();
            }
            Vector2 top = projectile.Top;
            Vector2 bottom = projectile.Bottom;
            Vector2 vector = Vector2.Lerp(top, bottom, 0.5f);
            Vector2 vector2 = new Vector2(0f, bottom.Y - top.Y);
            vector2.X = vector2.Y * 0.2f;
            int num2 = 16;
            int num3 = 160;
            for (int i = 0; i < 1; i++)
            {
                Vector2 vector3 = new Vector2(projectile.Center.X - (float)(num2 / 2), projectile.position.Y + (float)projectile.height - (float)num3);
                if (Collision.SolidCollision(vector3, num2, num3) || Collision.WetCollision(vector3, num2, num3))
                {
                    if (projectile.velocity.Y > 0f)
                    {
                        projectile.velocity.Y = 0f;
                    }
                    if (projectile.velocity.Y > -4f)
                    {
                        projectile.velocity.Y -= 2f;
                    }
                    else
                    {
                        projectile.velocity.Y -= 4f;
                        projectile.localAI[0] += 2f;
                    }
                    if (projectile.velocity.Y < -16f)
                    {
                        projectile.velocity.Y = -16f;
                    }
                    continue;
                }
                projectile.localAI[0] -= 1f;
                if (projectile.localAI[0] < 0f)
                {
                    projectile.localAI[0] = 0f;
                }
                if (projectile.velocity.Y < 0f)
                {
                    projectile.velocity.Y = 0f;
                }
                if (projectile.velocity.Y < 4f)
                {
                    projectile.velocity.Y += 2f;
                }
                else
                {
                    projectile.velocity.Y += 4f;
                }
                if (projectile.velocity.Y > 16f)
                {
                    projectile.velocity.Y = 16f;
                }
            }

            //Trailing dust
            if (projectile.ai[0] < num - 30f) //&& false
            {
                for (int j = 0; j < 1; j++)
                {
                    float value = -1f;
                    float value2 = 0.9f;
                    float amount = Main.rand.NextFloat();
                    Vector2 vector4 = new Vector2(MathHelper.Lerp(0.1f, 1f, Main.rand.NextFloat()), MathHelper.Lerp(value, value2, amount));
                    vector4.X *= MathHelper.Lerp(2.2f, 0.6f, amount);
                    vector4.X *= -1f;
                    Vector2 vector5 = new Vector2(6f, 10f);
                    Vector2 vector6 = vector + vector2 * vector4 * 0.5f + vector5;
                    Dust dust = Main.dust[Dust.NewDust(vector6, 0, 0, 274)];
                    dust.position = vector6;
                    dust.fadeIn = 1.3f;
                    dust.scale = 0.87f * 0f;
                    dust.alpha = 211;
                    if (vector4.X > -1.2f)
                    {
                        dust.velocity.X = 1f + Main.rand.NextFloat();
                    }
                    dust.noGravity = true;
                    dust.velocity.Y = Main.rand.NextFloat() * -0.5f - 1.3f;
                    dust.velocity.X += projectile.velocity.X * 2.1f;
                    dust.noLight = true;


                    //Custom Dust
                    Color col = Main.rand.NextBool(4) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;


                    ///int sa = Dust.NewDust(vector6, 0, 0, ModContent.DustType<GlowPixel>(), newColor: col, Scale: 0.35f);
                    //int sa = Dust.NewDust(vector6, 0, 0, ModContent.DustType<GlowFlare>(), newColor: col, Scale: 0.35f);
                    //int sa = Dust.NewDust(vector6, 0, 0, ModContent.DustType<GlowPixelCross>(), newColor: col, Scale: 0.25f);

                    ///Dust d = Main.dust[sa];
                    ///d.position = vector6;
                    ///d.velocity.Y = Main.rand.NextFloat() * -0.5f - 1.3f;
                    ///d.velocity.X += projectile.velocity.X * 2.1f * 1f;

                    //d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5, postSlowPower: 0.9f, velToBeginShrink: 2f, fadePower: 0.9f, shouldFadeColor: false);

                    int timeBeforeFade = Main.rand.Next(8, 14);

                    GlowPixelBehavior gpb = new GlowPixelBehavior(TimeForFadeIn: 3, TimeBeforeFadeOut: timeBeforeFade, VelFadePower: 0.92f, ScaleFadePower: 1f, AlphaFadePower: 0.9f, ColorFadePower: 0.9f);
                    gpb.earlyVelFadePower = Main.rand.NextFloat(0.88f, 0.95f);

                    //d.customData = gpb;
                }
            }

            //Dust at bottom
            Vector2 vector7 = projectile.Bottom + new Vector2(-25f, -25f);
            for (int k = 0; k < 2; k++) //4
            {
                Dust dust2 = Dust.NewDustDirect(vector7, 50, 25, 31, projectile.velocity.X, -2f, 100);
                dust2.fadeIn = 1.1f;
                dust2.noGravity = true;
            }

            //Paper Gores 
            for (int l = 0; l < 1; l++)
            {
                if (Main.rand.Next(5) == 0)
                {
                    Gore gore = Gore.NewGoreDirect(projectile.GetSource_FromThis(), projectile.TopLeft + Main.rand.NextVector2Square(0f, 1f) * projectile.Size, new Vector2(projectile.velocity.X * 1.5f, (0f - Main.rand.NextFloat()) * 16f), Utils.SelectRandom<int>(Main.rand, 1007, 1008, 1008));
                    gore.timeLeft = 60;
                    gore.alpha = 50;
                    gore.velocity.X += projectile.velocity.X;
                }
            }
            for (int m = 0; m < 1; m++)
            {
                if (Main.rand.Next(7) == 0)
                {
                    Gore gore2 = Gore.NewGoreDirect(projectile.GetSource_FromThis(), projectile.TopLeft + Main.rand.NextVector2Square(0f, 1f) * projectile.Size, new Vector2(projectile.velocity.X * 1.5f, (0f - Main.rand.NextFloat()) * 16f), Utils.SelectRandom<int>(Main.rand, 1007, 1008, 1008));
                    gore2.timeLeft = 0;
                    gore2.alpha = 80;
                }
            }
            for (int n = 0; n < 1; n++)
            {
                if (Main.rand.Next(7) == 0)
                {
                    Gore gore3 = Gore.NewGoreDirect(projectile.GetSource_FromThis(), projectile.TopLeft + Main.rand.NextVector2Square(0f, 1f) * projectile.Size, new Vector2(projectile.velocity.X * 1.5f, (0f - Main.rand.NextFloat()) * 16f), Utils.SelectRandom<int>(Main.rand, 1007, 1008, 1008));
                    gore3.timeLeft = 0;
                    gore3.alpha = 80;
                }
            }
            #endregion

            return false;
        }


        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Color LCcopy = lightColor;
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawTornado(projectile, LCcopy, giveUp: false);
            });
            DrawTornado(projectile, LCcopy, giveUp: true);

            return false;
        }

        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();
        Effect myEffect = null;
        public void DrawTornado(Projectile projectile, Color lightCol, bool giveUp)
        {
            if (giveUp)
                return;

            //Fix strange additive spritebatch bleed through
            //Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;


            Texture2D TrailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/OuterLavaTrailUp").Value; //
            Texture2D TrailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value; //T_Random_59

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/TornadoShaderPosterized", AssetRequestMode.ImmediateLoad).Value;


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.02f;

            Color StripColor(float progress) => Color.Lerp(lightCol, Color.White, 0.5f) * overallAlpha * (1f - Utils.GetLerpValue(0.8f, 1f, progress, true)) * projectile.Opacity;

            float StripWidth(float progress)
            {
                return Easings.easeOutQuad(progress) * 60f;
            }


            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(trailPositions.ToArray(), trailRotations.ToArray(), StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            
            float sine1 = 0.5f + (float)Math.Cos(Main.timeForVisualEffects * 0.1f) * 0.5f;
            float sine2 = 0.35f + (float)Math.Sin(Main.timeForVisualEffects * 0.05f) * -0.1f;


            Color tan1 = new Color(237, 225, 204);
            Color between2 = Color.Lerp(Color.Tan, Color.White, 0.2f);

            //Everybody say thank you lucille karma!!! (fix required when pixelizing)
            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            myEffect.Parameters["WorldViewProjection"].SetValue(view * projectionMatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f * Math.Sign(projectile.velocity.X)); //0.02
            myEffect.Parameters["fadeWidth"].SetValue(0.35f); //0.02

            myEffect.Parameters["TrailTexture1"].SetValue(TrailTexture1);
            myEffect.Parameters["tex1FlowSpeed"].SetValue(new Vector2(0.3f, -2.5f));
            myEffect.Parameters["tex1Zoom"].SetValue(new Vector2(2f, 0.63f));
            myEffect.Parameters["tex1Color"].SetValue(tan1.ToVector3() * 1.15f * 1f);
            myEffect.Parameters["tex1ColorMirrorMult"].SetValue(0.6f); 



            myEffect.Parameters["TrailTexture2"].SetValue(TrailTexture2);
            myEffect.Parameters["tex2FlowSpeed"].SetValue(new Vector2(0.3f, -1.37f));
            myEffect.Parameters["tex2Zoom"].SetValue(new Vector2(2.0f, 0.3f));
            myEffect.Parameters["tex2Color"].SetValue(between2.ToVector3() * 0.65f * 1f);
            myEffect.Parameters["tex2ColorMirrorMult"].SetValue(0f);

            myEffect.Parameters["sineTimeSpeed"].SetValue(2.0f); 
            myEffect.Parameters["sineMult"].SetValue(1.0f); 
            myEffect.Parameters["startingCurveAmount"].SetValue(0.0f); 

            myEffect.Parameters["posterizationSteps"].SetValue(6.0f);
            myEffect.Parameters["hueShiftIntensity"].SetValue(0.0f);


            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
           
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }

    }

}
