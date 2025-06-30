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
using ReLogic.Utilities;
using VFXPlus.Common.Drawing;
using rail;
using Terraria.Graphics;
using System.Runtime.Intrinsics.X86;
using System.Net;
using static Terraria.ModLoader.PlayerDrawLayer;
using VFXPlus.Content.Weapons.Ranged.PreHardmode.Bows;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class ChargedBlasterCannon : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ChargedBlasterCannon) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ChargedBlasterCannonToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }

    public class ChargedBlasterCannonHeldProjectileOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ChargedBlasterCannon) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ChargedBlasterCannonToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //If Firing Laser
            if (projectile.ai[0] > 180)
            {
                if (timer % 1 == 0)
                {
                    Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(3, 9) * 1.75f;

                    Dust dp = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<ElectricSparkGlow>(), dustVel, 
                        newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.6f);

                    ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.94f, FadeVelPower: 0.89f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91
                    dp.customData = esb;
                }
            }


            timer++;

            #region vanilla

            Player player = Main.player[projectile.owner];
            float num = (float)Math.PI / 2f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            int num12 = 2;
            float num23 = 0f;
            if (projectile.type == 460)
            {
                projectile.ai[0] += 1f;
                int num13 = 0;
                if (projectile.ai[0] >= 80f)
                {
                    num13++;
                }
                if (projectile.ai[0] >= 180f)
                {
                    num13++;
                }
                bool flag8 = false;
                _ = projectile.ai[0];
                if (projectile.ai[0] == 80f || projectile.ai[0] == 180f || (projectile.ai[0] > 180f && projectile.ai[0] % 20f == 0f))
                {
                    flag8 = true;
                }
                bool flag9 = projectile.ai[0] >= 180f;
                int num14 = 5;
                if (!flag9)
                {
                    projectile.ai[1] += 1f;
                }
                bool flag10 = false;
                if (projectile.ai[0] == 1f)
                {
                    flag10 = true;
                }
                if (flag9 && projectile.ai[0] % 20f == 0f)
                {
                    flag10 = true;
                }
                if ((!flag9 && projectile.ai[1] >= (float)num14) || (flag9 && projectile.ai[0] % 1f == 0f)) //VANILLA DIFFERENCE BECAUSE THIS SHIT SUCKS ASS %5
                {
                    if (!flag9)
                    {
                        projectile.ai[1] = 0f;
                    }
                    flag10 = true;
                    float num15 = player.inventory[player.selectedItem].shootSpeed * projectile.scale;
                    Vector2 vector39 = vector;
                    Vector2 value7 = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector39;
                    if (player.gravDir == -1f)
                    {
                        value7.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector39.Y;
                    }
                    Vector2 vector2 = Vector2.Normalize(value7);
                    if (float.IsNaN(vector2.X) || float.IsNaN(vector2.Y))
                    {
                        vector2 = -Vector2.UnitY;
                    }
                    vector2 *= num15;
                    if (vector2.X != projectile.velocity.X || vector2.Y != projectile.velocity.Y)
                    {
                        projectile.netUpdate = true;
                    }
                    projectile.velocity = vector2;
                }
                if (projectile.soundDelay <= 0 && !flag9)
                {
                    projectile.soundDelay = num14 - num13;
                    projectile.soundDelay *= 2;
                    if (projectile.ai[0] != 1f)
                    {
                        SoundEngine.PlaySound(in SoundID.Item15, projectile.position);
                    }
                }
                if (projectile.ai[0] > 10f && !flag9)
                {
                    Vector2 spinningpoint4 = Vector2.UnitX * 18f;
                    spinningpoint4 = spinningpoint4.RotatedBy(projectile.rotation - (float)Math.PI / 2f);
                    Vector2 vector3 = projectile.Center + spinningpoint4;
                    for (int k = 0; k < num13 + 1; k++)
                    {
                        int num16 = 226;
                        float num17 = 0.4f;
                        if (k % 2 == 1)
                        {
                            num16 = 226;
                            num17 = 0.65f;
                        }
                        Vector2 vector4 = vector3 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (14f - (float)(num13 * 2)); //changed from 12 to 16
                        int num18 = Dust.NewDust(vector4 - Vector2.One * 8f, 16, 16, num16, projectile.velocity.X / 2f, projectile.velocity.Y / 2f);
                        Main.dust[num18].velocity = Vector2.Normalize(vector3 - vector4) * 1.5f * (10f - (float)num13 * 2f) / 10f;
                        Main.dust[num18].noGravity = true;
                        Main.dust[num18].scale = num17 * 0.55f; //0.5f is my change
                        Main.dust[num18].customData = player;
                    }
                }
                if (flag9)
                {
                    Vector2 spinningpoint5 = Vector2.UnitX * 14f;
                    spinningpoint5 = spinningpoint5.RotatedBy(projectile.rotation - (float)Math.PI / 2f);
                    Vector2 vector5 = projectile.Center + spinningpoint5;
                    //This is dust while firing laser
                    for (int l = 0; l < 2; l++) // vanilla is l < 2
                    {
                        int num19 = 226;
                        float num20 = 0.35f;
                        if (l % 2 == 1)
                        {
                            num19 = 226;
                            num20 = 0.45f;
                        }
                        //float num21 = Main.rand.NextFloatDirection();
                        //Vector2 vector6 = vector5 + (projectile.rotation + num21 * ((float)Math.PI / 4f) * 0.8f - (float)Math.PI / 2f).ToRotationVector2() * 6f;
                        //int num22 = 24;
                        //int num24 = Dust.NewDust(vector6 - Vector2.One * (num22 / 2), num22, num22, num19, projectile.velocity.X / 2f, projectile.velocity.Y / 2f);
                        //Main.dust[num24].velocity = (vector6 - vector5).SafeNormalize(Vector2.Zero) * MathHelper.Lerp(1.5f, 9f, Utils.GetLerpValue(1f, 0f, Math.Abs(num21), clamped: true));
                        //Main.dust[num24].noGravity = true;
                        //Main.dust[num24].scale = num20;
                        //Main.dust[num24].customData = player;
                        //Main.dust[num24].fadeIn = 0.5f;
                    }
                }
                if (flag10 && Main.myPlayer == projectile.owner)
                {
                    bool flag11 = false;
                    flag11 = !flag8 || player.CheckMana(player.inventory[player.selectedItem], -1, pay: true);
                    if (player.channel && flag11 && !player.noItems && !player.CCed)
                    {
                        if (projectile.ai[0] == 180f)
                        {
                            Vector2 center = projectile.Center;
                            Vector2 vector7 = Vector2.Normalize(projectile.velocity);
                            if (float.IsNaN(vector7.X) || float.IsNaN(vector7.Y))
                            {
                                vector7 = -Vector2.UnitY;
                            }
                            int num25 = (int)((float)projectile.damage * 1.5f);
                            int num26 = Projectile.NewProjectile(projectile.GetSource_FromThis(), center.X, center.Y, vector7.X, vector7.Y, 461, num25, projectile.knockBack, projectile.owner, 0f, projectile.whoAmI);
                            projectile.ai[1] = num26;
                            projectile.netUpdate = true;


                            // 
                            Vector2 pulsePos = projectile.Center + new Vector2(0f, -14f).RotatedBy(projectile.rotation) + new Vector2(0f, Main.player[projectile.owner].gfxOffY);

                            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.10f, true, 2, 0.8f, 0.8f);
                            cpb2.drawLayer = "Dusts";

                            Dust d1 = Dust.NewDustPerfect(pulsePos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.DeepSkyBlue * 1f);
                            d1.scale = 0.06f;
                            d1.customData = cpb2;
                            d1.velocity = new Vector2(-0.01f, 0f);

                            Dust d2 = Dust.NewDustPerfect(pulsePos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 1f);
                            d2.scale = 0.06f;
                            d2.customData = cpb2;
                            d2.velocity = new Vector2(0.01f, 0f);


                            for (int i = 0; i < 4 + Main.rand.Next(0, 6); i++) //2 //0,3
                            {
                                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 10f) * 0.75f;

                                Dust dp = Dust.NewDustPerfect(pulsePos, ModContent.DustType<ElectricSparkGlow>(), vel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.5f);

                                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91
                                dp.customData = esb;
                            }

                            for (int i = 0; i < 6; i++)
                            {
                                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 7f) * 0.75f;
                                Color middleBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f + Main.rand.NextFloat(-0.15f, 0.15f));

                                Dust gd = Dust.NewDustPerfect(pulsePos, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.15f, 0.4f));
                                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                                    preSlowPower: 0.94f, postSlowPower: 0.90f, velToBeginShrink: 1f, fadePower: 0.92f, shouldFadeColor: false);
                            }

                        }
                        else if (flag9)
                        {
                            Projectile projectileA = Main.projectile[(int)projectile.ai[1]];
                            if (!projectileA.active || projectileA.type != 461)
                            {
                                projectile.Kill();
                                return false;
                            }
                        }
                        else
                        {
                            bool flag12 = false;
                            if (projectile.ai[0] == 1f)
                            {
                                flag12 = true;
                            }
                            if (projectile.ai[0] <= 50f && projectile.ai[0] % 10f == 0f)
                            {
                                flag12 = true;
                            }
                            if (projectile.ai[0] >= 80f && projectile.ai[0] < 180f && projectile.ai[0] % 30f == 0f)
                            {
                                flag12 = true;
                            }
                            if (flag12)
                            {

                                Vector2 vector8 = vector;
                                int num27 = 459;
                                float num28 = 10f;
                                vector8 = projectile.Center;
                                Vector2 vector9 = Vector2.Normalize(projectile.velocity) * num28;
                                if (float.IsNaN(vector9.X) || float.IsNaN(vector9.Y))
                                {
                                    vector9 = -Vector2.UnitY;
                                }
                                float num29 = 0.7f + (float)num13 * 0.3f;
                                int num30 = ((num29 < 1f) ? projectile.damage : ((int)((float)projectile.damage * 2.5f)));

                                if (num29 < 1f) //Small shot
                                {
                                    SoundStyle style = new SoundStyle("Terraria/Sounds/Item_75") with { Volume = 0.5f, Pitch = -0.05f, PitchVariance = .05f, MaxInstances = -1}; 
                                    SoundEngine.PlaySound(style, projectile.Center);

                                    SoundStyle style4 = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = 0.15f, Pitch = .35f, PitchVariance = 0.1f, MaxInstances = -1 }; 
                                    SoundEngine.PlaySound(style4, projectile.Center);
                                }
                                else //Big shot
                                {
                                    SoundStyle style = new SoundStyle("Terraria/Sounds/Item_75") with { Volume = 0.35f, Pitch = -0.05f, PitchVariance = .05f, MaxInstances = -1 };
                                    SoundEngine.PlaySound(style, projectile.Center);

                                    SoundStyle style32 = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = 0.2f, Pitch = 0.25f, MaxInstances = -1, PitchVariance = 0.1f };
                                    SoundEngine.PlaySound(style32, projectile.Center);

                                    SoundStyle style23 = new SoundStyle("Terraria/Sounds/Custom/dd2_sky_dragons_fury_shot_0") with { Pitch = 0.15f, PitchVariance = 0.4f, Volume = 0.3f };
                                    SoundEngine.PlaySound(style23, projectile.Center);

                                    SoundStyle stylec = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_67") with { Pitch = 0f, Volume = 0.55f, PitchVariance = 0.1f, MaxInstances = -1 }; //1f
                                    SoundEngine.PlaySound(stylec, player.Center);
                                }
                                Projectile.NewProjectile(projectile.GetSource_FromThis(), vector8.X, vector8.Y, vector9.X, vector9.Y, num27, num30, projectile.knockBack, projectile.owner, 0f, num29);
                            }
                        }
                    }
                    else
                    {
                        projectile.Kill();
                    }
                }
            }

            projectile.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            projectile.timeLeft = 2;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.SetDummyItemTime(num12);
            player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(projectile.velocity.Y * (float)projectile.direction, projectile.velocity.X * (float)projectile.direction) + num23);
            if (projectile.type == 460)
            {
                Vector2 vector32 = Main.OffsetsPlayerOnhand[player.bodyFrame.Y / 56] * 2f;
                if (player.direction != 1)
                {
                    vector32.X = (float)player.bodyFrame.Width - vector32.X;
                }
                if (player.gravDir != 1f)
                {
                    vector32.Y = (float)player.bodyFrame.Height - vector32.Y;
                }
                vector32 -= new Vector2(player.bodyFrame.Width - player.width, player.bodyFrame.Height - 42) / 2f;
                projectile.Center = player.RotatedRelativePoint(player.MountedCenter - new Vector2(20f, 42f) / 2f + vector32, reverseRotation: false, addGfxOffY: false) - projectile.velocity;
            }

            #endregion

            return false;
        }


        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //Border
            for (int i = 0; i < 6; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.SkyBlue with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.07f, SpriteEffects.None);
            }

            //We NEED to return true or else the laser wont draw for some reason

            return true;
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            //Only draws if we are not doing the laser
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                if (projectile.ai[0] < 180)
                {
                    Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[projectile.owner].gfxOffY);

                    Vector2 posOffset = new Vector2(0f, -16f).RotatedBy(projectile.rotation);
                    drawPos += posOffset;

                    //Draw Orb
                    Texture2D Orb = CommonTextures.feather_circle128PMA.Value;

                    float progress = Utils.GetLerpValue(0f, 180f, projectile.ai[0], true);

                    Color col1 = Color.White * 0.75f;
                    Color col2 = Color.SkyBlue * 0.525f;
                    Color col3 = Color.DeepSkyBlue * 0.375f;

                    float scale1 = 0.85f;
                    float scale2 = 1.6f;
                    float scale3 = 2.5f;
                    float scale = projectile.scale * 0.25f * Easings.easeInOutQuad(progress);

                    float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.2f;
                    float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.12f;

                    Main.EntitySpriteDraw(Orb, drawPos, null, col1 with { A = 0 }, projectile.rotation, Orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
                    Main.EntitySpriteDraw(Orb, drawPos, null, col2 with { A = 0 } * Easings.easeOutCubic(progress), projectile.rotation, Orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
                    Main.EntitySpriteDraw(Orb, drawPos, null, col3 with { A = 0 } * Easings.easeOutSine(progress), projectile.rotation, Orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);
                }
            });
            
            base.PostDraw(projectile, lightColor);
        }

    }

    public class ChargedBlasterOrbShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ChargedBlasterOrb) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ChargedBlasterCannonToggle;
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //Big Shot just spawn FX
            if (timer == 0 && projectile.ai[1] >= 1)
            {
                float circlePulseSize = 0.35f;
                Color inBetweenCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.35f * projectile.scale, newColor: inBetweenCol * 0.5f);
                CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize * 1.5f, true, 3, 0.2f, 0.4f);
                b2.drawLayer = "Dusts";
                d2.customData = b2;

                for (int i = 0; i < 7 + Main.rand.Next(0, 4); i++) //2 //0,3
                {
                    Dust dp = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<ElectricSparkGlow>(),
                        projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.45f, 0.45f)) * Main.rand.Next(3, 9) * 1.5f,
                        newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.55f);

                    ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.91f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91

                    if (i < 4)
                        esb.randomVelRotatePower = 0.55f; //1f
                    dp.customData = esb;
                }
            }

            //Little Shot just spawn FX
            if (timer == 0 && projectile.ai[1] < 1f)
            {

                Color col = Color.DeepSkyBlue;
                for (int i = 0; i < 2 + Main.rand.Next(0, 4); i++) //2 //0,3
                {
                    Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(),
                        projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(3, 9) * 1.3f,
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.45f);

                    ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.91f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91

                    if (i < 31)
                        esb.randomVelRotatePower = 0.35f; //1f
                    dp.customData = esb;
                }
            }

            #region trail
            int trailCount = (int)(25f * projectile.scale); //8
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            int trueTrailWidth = (int)(35f * overallScale * projectile.scale * 2f); //20

            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value; 
            trail1.trailPointLimit = 90;
            trail1.trailWidth = trueTrailWidth;
            trail1.trailMaxLength = 100 * projectile.scale; //65
            trail1.timesToDraw = 2;
            trail1.shouldSmooth = false;
            trail1.pinchHead = false;
            trail1.pinchTail = false;
            trail1.useEffectMatrix = true;

            trail1.trailColor = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.65f);

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            #endregion

            int mod = projectile.scale >= 1f ? 3 : 5;
            if (timer % mod == 0 && Main.rand.NextBool() && timer > 5)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), dustVel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.4f, 0.6f) * 4f * projectile.scale);
                da.velocity += projectile.velocity.RotatedByRandom(0.1f) * 0.4f;

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.91f, FadeVelPower: 0.92f, Pixelize: true, XScale: 0.8f, YScale: 0.5f, WhiteLayerPower: 0.85f);
                esb.randomVelRotatePower = 0.15f;
                da.customData = esb;
            }

            float timeForPopInAnim = 35;
            float animProgress = Math.Clamp((timer + 8) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.15f + MathHelper.Lerp(0f, 0.85f, Easings.easeInOutBack(animProgress, 0f, 2f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.05f), 0f, 1f);

            timer++;

            #region vanillaAI
            projectile.rotation = projectile.velocity.ToRotation();
            if (projectile.direction == -1)
            {
                projectile.rotation += (float)Math.PI;
            }

            projectile.alpha -= 30;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            projectile.spriteDirection = projectile.direction;
            projectile.frameCounter++;
            if (projectile.frameCounter >= 3)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
                if (projectile.frame >= 3)
                {
                    projectile.frame = 0;
                }
            }
            projectile.position = projectile.Center;
            projectile.scale = projectile.ai[1];
            projectile.width = (projectile.height = (int)(22f * projectile.scale));
            projectile.Center = projectile.position;
            Lighting.AddLight((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16, 0.4f, 0.85f, 0.9f);
            int num190 = 0;
            if ((double)projectile.scale < 0.85)
            {
                num190 = ((Main.rand.Next(3) == 0) ? 1 : 0);
            }
            else
            {
                num190 = 1;
                projectile.penetrate = -1;
                projectile.maxPenetrate = -1;
            }
            for (int num191 = 0; num191 < num190; num191++)
            {
                if (Main.rand.NextBool())
                {
                    //int num192 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 226, projectile.velocity.X);
                    //Main.dust[num192].position -= Vector2.One * 3f;
                    //Main.dust[num192].scale = 0.5f;
                    //Main.dust[num192].noGravity = true;
                    //Main.dust[num192].velocity = projectile.velocity.RotatedByRandom(0.1f) / 2.5f;
                    //Main.dust[num192].alpha = 255 - (int)(255f * projectile.scale);
                }
            }
            #endregion
            return false;
        }

        float overallAlpha = 0f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            trail1.trailTime = (float)Main.timeForVisualEffects * 0.06f;

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
            });

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(projectile, false);
            });
            DrawAfterImage(projectile, true);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 baseVec2Scale = new Vector2(1f, overallScale) * projectile.scale;

            Texture2D SoftGlow = CommonTextures.feather_circle128PMA.Value;

            #region Orb
            Color col1 = Color.LightSkyBlue * 0.75f; //Gold
            Color col2 = Color.SkyBlue * 0.525f;
            Color col3 = Color.DeepSkyBlue * 0.375f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            Vector2 scale = new Vector2(0.75f, overallScale * 0.7f) * projectile.scale * 0.55f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(SoftGlow, drawPos, null, col1 with { A = 0 } * overallAlpha * 0.5f, projectile.rotation, SoftGlow.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(SoftGlow, drawPos, null, col2 with { A = 0 } * overallAlpha * 0.5f, projectile.rotation, SoftGlow.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(SoftGlow, drawPos, null, col3 with { A = 0 } * overallAlpha * 0.5f, projectile.rotation, SoftGlow.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);
            #endregion

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3f, 3f), sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * 1f * overallAlpha, projectile.rotation, TexOrigin, baseVec2Scale * 1.05f, se);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * overallAlpha * 0.9f, projectile.rotation, TexOrigin, baseVec2Scale, se);
            return false;

        }

        public void DrawAfterImage(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D line = CommonTextures.SoulSpike.Value;
            Color between2 = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / (float)previousPositions.Count;

                Vector2 offset1 = Vector2.Zero;

                offset1 += Main.rand.NextVector2Circular(11f * projectile.scale, 11f * projectile.scale) * overallScale * projectile.scale;


                Vector2 flarePos = previousPositions[i] - Main.screenPosition;

                Color col = Color.Lerp(Color.DeepSkyBlue * 1.5f, between2, Easings.easeOutCubic(progress)) * overallAlpha;

                Vector2 lineScale = new Vector2(1f, 1f) * progress * overallScale * projectile.scale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, col with { A = 0 } * 0.45f * progress,
                    previousRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Vector2 innerScale = new Vector2(1f, 1f * 0.1f) * progress * overallScale * projectile.scale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, Color.White with { A = 0 } * 0.6f * progress * overallAlpha,
                    previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //Sounds
            if (projectile.scale < 1)
            {
                int soundVar = Main.rand.Next(0, 3);
                SoundStyle style5 = new SoundStyle("Terraria/Sounds/Custom/dd2_lightning_bug_zap_" + soundVar) with { Volume = 0.3f, Pitch = 0.51f, PitchVariance = 0.15f, MaxInstances = -1 };
                SoundEngine.PlaySound(style5, projectile.Center);

                SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_40") with { Volume = 0.08f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = -1 };
                SoundEngine.PlaySound(style2, projectile.Center);
            }
            else
            {
                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Thunder/ElectricExplode") with { Volume = 0.03f, Pitch = 0.35f, PitchVariance = 0.15f, MaxInstances = -1, };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style3 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_106") with { Volume = .15f, Pitch = .4f, PitchVariance = 0.05f, MaxInstances = -1 };
                SoundEngine.PlaySound(style3, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Thunder_0") with { Volume = .08f, Pitch = 1f, PitchVariance = .12f, MaxInstances = -1, };
                SoundEngine.PlaySound(style2, projectile.Center);
            }

            #region vanillaAI
            int num293 = 3;
            int num294 = 10;
            int num295 = 0;
            if (projectile.scale >= 1f)
            {
                projectile.position = projectile.Center;
                projectile.width = (projectile.height = 144);
                projectile.Center = projectile.position;
                num293 = 7;
                num294 = 30;
                num295 = 2;
                projectile.Damage();
            }
            for (int num296 = 11110; num296 < num293; num296++)
            {
                int num298 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                Main.dust[num298].position = new Vector2(projectile.width / 2, 0f).RotatedBy(6.2831854820251465 * Main.rand.NextDouble()) * (float)Main.rand.NextDouble() + projectile.Center;
            }
            for (int num299 = 211220; num299 < num294; num299++)
            {
                int num300 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 226, 0f, 0f, 0, default(Color), 1.5f);
                Main.dust[num300].position = new Vector2(projectile.width / 2, 0f).RotatedBy(6.2831854820251465 * Main.rand.NextDouble()) * (float)Main.rand.NextDouble() + projectile.Center;
                Main.dust[num300].noGravity = true;
                Dust dust24 = Main.dust[num300];
                Dust dust334 = dust24;
                dust334.velocity *= 1f;
            }
            for (int num301 = 2121220; num301 < num295; num301++)
            {
                int num302 = Gore.NewGore(null, projectile.position + new Vector2((float)(projectile.width * Main.rand.Next(100)) / 100f, (float)(projectile.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64));
                Gore gore9 = Main.gore[num302];
                Gore gore64 = gore9;
                gore64.velocity *= 0.3f;
                Main.gore[num302].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                Main.gore[num302].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
            }

            #endregion

            //KillFX
            Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

            if (projectile.scale >= 1f)
            {
                CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.55f, true, 2, 0.8f, 0.8f);

                Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: between * 0.25f);
                d1.customData = cpb2;
                d1.velocity = new Vector2(-0.01f, 0f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: between * 0.25f);
                d2.customData = cpb2;
                d2.velocity = new Vector2(0.01f, 0f);
            }

            float sparkVelMult = projectile.scale >= 1f ? 1f : 0.5f;
            int sparkCount = projectile.scale >= 1f ? 13 : 5;
            for (int i = 0; i < sparkCount + Main.rand.Next(0, 6); i++) //2 //0,3
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 10f) * sparkVelMult;

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), vel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.5f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91

                if (i < 8)
                    esb.randomVelRotatePower = 1f;
                dp.customData = esb;
            }

            float crossVelMult = projectile.scale >= 1f ? 1f : 0.5f;
            int crossCount = projectile.scale >= 1f ? 8 : 5;
            for (int i = 0; i < crossCount; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 7f) * crossVelMult;
                Color middleBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f + Main.rand.NextFloat(-0.15f, 0.15f));

                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.15f, 0.4f));
                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.90f, velToBeginShrink: 1f, fadePower: 0.92f, shouldFadeColor: false);
            }


            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Only do burst on hit if we are the big shot
            if (projectile.scale >= 1f)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 7f) * 0.5f;
                    Color middleBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f + Main.rand.NextFloat(-0.15f, 0.15f));

                    Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.15f, 0.4f));
                    gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                        preSlowPower: 0.94f, postSlowPower: 0.90f, velToBeginShrink: 1f, fadePower: 0.92f, shouldFadeColor: false);
                }

                for (int i = 0; i < 4 + Main.rand.Next(0, 4); i++) 
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 10f) * 0.45f;

                    Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), vel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.5f);

                    ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91

                    if (i < 8)
                        esb.randomVelRotatePower = 1f;
                    dp.customData = esb;
                }
            }


            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }

    public class ChargedBlasterCannonLaserOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ChargedBlasterLaser) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ChargedBlasterCannonToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                SoundStyle style32 = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = 0.35f, Pitch = 0f, MaxInstances = -1, PitchVariance = 0.1f };
                SoundEngine.PlaySound(style32, projectile.Center);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Tech/AnnihilatorShot") with { Volume = 0.1f, Pitch = 0.15f, MaxInstances = -1 };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle styla = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_122") with { Pitch = 0.75f, PitchVariance = 0.11f, Volume = 0.3f, MaxInstances = -1 };
                SoundEngine.PlaySound(styla, projectile.Center);

                SoundStyle styleb = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item125Trim") with { Volume = 0.5f, Pitch = 0.75f, PitchVariance = 0.11f, MaxInstances = -1 };
                SoundEngine.PlaySound(styleb, projectile.Center);
            }    

            laserAlpha = Math.Clamp(MathHelper.Lerp(laserAlpha, 1.5f, 0.1f), 0f, 1f);

            float timeForPopInAnim = 32;
            float animProgress = Math.Clamp((timer + 12) / timeForPopInAnim, 0f, 1f);
            laserWidth = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;


            if (timer % 2 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float percent = Main.rand.NextFloat(0f, 0.9f);

                    Vector2 pos = Vector2.Lerp(projectile.Center, projectile.Center + projectile.velocity * (projectile.localAI[1] - 8f), percent);

                    //Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.05f) * Main.rand.NextFloat(2, 7);


                    //Dust d = Dust.NewDustPerfect(pos + off, ModContent.DustType<LineSpark>(), vel * 2f, newColor: Color.DeepSkyBlue * 1f, Scale: Main.rand.NextFloat(0.4f, 0.8f) * 0.5f);
                    //d.noLight = false;
                    //d.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.93f, postShrinkPower: 0.89f, timeToStartShrink: 8, killEarlyTime: 100, XScale: 0.5f, YScale: 0.25f, shouldFadeColor: false);
                    
                    Vector2 dir = projectile.velocity.SafeNormalize(Vector2.UnitX);

                    Vector2 offset = dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10, 10);
                    Vector2 vel = dir.RotatedByRandom(0.05f) * Main.rand.NextFloat(4, 9);

                    Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowPixelCross>(), vel * 1f, newColor: Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f), 
                        Scale: Main.rand.NextFloat(0.5f, 1f) * 0.25f);
                    d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, postSlowPower: 0.89f, velToBeginShrink: 3f, fadePower: 0.89f, shouldFadeColor: false);
                }

            }

            timer++;

            #region vanillaAI
            Vector2? vector133 = null;
            if (projectile.velocity.HasNaNs() || projectile.velocity == Vector2.Zero)
            {
                projectile.velocity = -Vector2.UnitY;
            }

            if (projectile.type == 461 && Main.projectile[(int)projectile.ai[1]].active && Main.projectile[(int)projectile.ai[1]].type == 460)
            {
                Vector2 vector138 = Vector2.Normalize(Main.projectile[(int)projectile.ai[1]].velocity);
                projectile.position = Main.projectile[(int)projectile.ai[1]].Center + vector138 * 16f - new Vector2(projectile.width, projectile.height) / 2f + new Vector2(0f, 0f - Main.projectile[(int)projectile.ai[1]].gfxOffY);
                projectile.velocity = Vector2.Normalize(Main.projectile[(int)projectile.ai[1]].velocity);
            }
            else
            {
                projectile.Kill();
                return false;
            }

            if (projectile.velocity.HasNaNs() || projectile.velocity == Vector2.Zero)
            {
                projectile.velocity = -Vector2.UnitY;
            }
            if (projectile.type == 461)
            {
                projectile.ai[0]++;
                if (projectile.ai[0] >= 300f)
                {
                    projectile.Kill();
                    return false;
                }
                projectile.scale = (float)Math.Sin(projectile.ai[0] * (float)Math.PI / 300f) * 10f;
                if (projectile.scale > 1f)
                {
                    projectile.scale = 1f;
                }
            }
            float num761 = projectile.velocity.ToRotation();

            projectile.rotation = num761 - (float)Math.PI / 2f;
            projectile.velocity = num761.ToRotationVector2();
            float num762 = 0f;
            float num763 = 0f;
            Vector2 samplingPoint = projectile.Center;
            if (vector133.HasValue)
            {
                samplingPoint = vector133.Value;
            }
            if (projectile.type == 461)
            {
                num762 = 2f;
                num763 = 0f;
            }

            float[] array2 = new float[(int)num762];
            Collision.LaserScan(samplingPoint, projectile.velocity, num763 * projectile.scale, 2400f, array2);
            float num764 = 0f;
            for (int num765 = 0; num765 < array2.Length; num765++)
            {
                num764 += array2[num765];
            }
            num764 /= num762;
            float amount = 0.5f;
            projectile.localAI[1] = MathHelper.Lerp(projectile.localAI[1], num764, amount);


            if (projectile.type == 461)
            {
                Vector2 vector148 = projectile.Center + projectile.velocity * (projectile.localAI[1] - 8f);
                for (int num778 = 0; num778 < 2; num778++)
                {
                    //float num779 = projectile.velocity.ToRotation() + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
                    //float num780 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                    //Vector2 vector149 = new Vector2((float)Math.Cos(num779) * num780, (float)Math.Sin(num779) * num780);
                    //int num781 = Dust.NewDust(vector148, 0, 0, 226, vector149.X, vector149.Y);
                    //Main.dust[num781].noGravity = true;
                    //Main.dust[num781].scale = 1.2f;
                }
                if (Main.rand.Next(5) == 0)
                {
                    //Vector2 vector150 = projectile.velocity.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * projectile.width;
                    //int num782 = Dust.NewDust(vector148 + vector150 - Vector2.One * 4f, 8, 8, 31, 0f, 0f, 100, default(Color), 1.5f);
                    //Dust dust89 = Main.dust[num782];
                    //Dust dust212 = dust89;
                    //dust212.velocity *= 0.5f;
                    //Main.dust[num782].velocity.Y = 0f - Math.Abs(Main.dust[num782].velocity.Y);
                }
                DelegateMethods.v3_1 = new Vector3(0.4f, 0.85f, 0.9f);
                Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * projectile.localAI[1], (float)projectile.width * projectile.scale, DelegateMethods.CastLight);
            }
            #endregion

            Vector2 endPoint = projectile.Center + projectile.velocity * (projectile.localAI[1] - 8f);

            for (int i = 0; i < 1; i++) //2 //0,3
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 10f) * 0.5f;

                Dust dp = Dust.NewDustPerfect(endPoint, ModContent.DustType<ElectricSparkGlow>(), vel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.5f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.94f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91
                dp.customData = esb;
            }

            for (int i = 0; i < 1; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.5f, 7f) * 0.5f;
                Color middleBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f + Main.rand.NextFloat(-0.15f, 0.15f));

                Dust gd = Dust.NewDustPerfect(endPoint, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.15f, 0.4f));
                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.90f, velToBeginShrink: 2f, fadePower: 0.9f, shouldFadeColor: false);
            }

            return false;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawVertexTrail(projectile, false); 
                DrawEndpoints(projectile, false);

            });

            DrawVertexTrail(projectile, true);
            DrawEndpoints(projectile, true);

            return false;
        }

        float laserAlpha = 0f;
        float laserWidth = 2f;
        Effect myEffect = null;
        public void DrawVertexTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            #region shaderPrep
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture = CommonTextures.s06sBloom.Value; //EnergyTex 700 | s06sBloom
            Texture2D trailTexture2 = CommonTextures.ThinGlowLine.Value;

            Vector2 startPoint = projectile.Center + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Vector2 endPoint = startPoint + projectile.velocity * (projectile.localAI[1] - 8f);

            Vector2[] pos_arr = { endPoint, startPoint };
            float[] rot_arr = { projectile.velocity.ToRotation(), projectile.velocity.ToRotation() };

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.05f;

            Color StripColor(float progress) => Color.White * laserAlpha;
            float StripWidth1(float progress) => 15f * laserWidth * sineWidthMult;
            float StripWidth2(float progress) => 20f * laserWidth * sineWidthMult;

            //^ Doing Easings.easeOutQuad(progress) * Easings.easeInQuad(progress) gives a really nice zigzag patter (or do 1f - EaseIn)

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth1, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.03f);

            float dist = (startPoint - endPoint).Length();
            float repValue = dist / 700f;
            myEffect.Parameters["reps"].SetValue(repValue);

            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f * laserAlpha);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();


            Color inBetween = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.35f);

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["ColorOne"].SetValue(inBetween.ToVector3() * 5f * laserAlpha);
            myEffect.Parameters["glowThreshold"].SetValue(0.8f);
            myEffect.Parameters["glowIntensity"].SetValue(1.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.DeepSkyBlue.ToVector3() * 10f * laserAlpha);
            myEffect.Parameters["glowThreshold"].SetValue(0.55f);
            myEffect.Parameters["glowIntensity"].SetValue(2.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            #endregion
        }

        public void DrawEndpoints(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 startPoint = projectile.Center + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Vector2 endPoint = startPoint + projectile.velocity * (projectile.localAI[1] - 8f);
            float rot = projectile.velocity.ToRotation();

            Texture2D portal = CommonTextures.RainbowRod.Value;
            Texture2D bloom = CommonTextures.feather_circle128PMA.Value;

            float sinScale = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.03f) * 0.2f;
            float lerpXVal = MathHelper.Lerp(0.4f, 1f, laserWidth - 1f);
            float lerpYVal = MathHelper.Lerp(1f, 1.5f, laserWidth - 1f);
            Vector2 v2Scale = new Vector2(lerpXVal, lerpYVal) * sinScale * 0.6f;


            Vector2 drawPosStart = startPoint - Main.screenPosition;
            Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);

            //Source Star
            Main.EntitySpriteDraw(portal, drawPosStart, null, Color.Black * 0.3f, rot, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);

            Main.EntitySpriteDraw(bloom, drawPosStart, null, col with { A = 0 } * 0.4f, rot, bloom.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(2f, 2f), null, col with { A = 0 } * laserAlpha, rot, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(3f, 3f), null, col with { A = 0 } * laserAlpha, rot, portal.Size() / 2, v2Scale * 1.75f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(5f, 5f), null, Color.White with { A = 0 } * laserAlpha, rot, portal.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            //Destination Star
            drawPosStart = endPoint - Main.screenPosition;
            Vector2 v2Scale2 = new Vector2(1f, 1f) * sinScale * 0.45f;

            Main.EntitySpriteDraw(portal, drawPosStart, null, Color.Black * 0.3f, rot, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);

            Main.EntitySpriteDraw(bloom, drawPosStart, null, col with { A = 0 } * 0.4f, rot, bloom.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(2f, 2f), null, col with { A = 0 } * laserAlpha, rot, portal.Size() / 2, v2Scale2 * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(3f, 3f), null, col with { A = 0 } * laserAlpha, rot, portal.Size() / 2, v2Scale2 * 1.75f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(5f, 5f), null, Color.White with { A = 0 } * laserAlpha, rot, portal.Size() / 2, v2Scale2 * 1f, SpriteEffects.None);
        }

    }

}
