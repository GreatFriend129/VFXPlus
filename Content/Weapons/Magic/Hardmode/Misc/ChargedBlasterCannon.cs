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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class ChargedBlasterCannon : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ChargedBlasterCannon);
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
            return lateInstantiation && (entity.type == ProjectileID.ChargedBlasterCannon);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 6;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

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
                if ((!flag9 && projectile.ai[1] >= (float)num14) || (flag9 && projectile.ai[0] % 1f == 0f)) //VANILLA DIFFERENCE BECAUSE THIS SHIT SUCKS ASS
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
                        Vector2 vector4 = vector3 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - (float)(num13 * 2));
                        int num18 = Dust.NewDust(vector4 - Vector2.One * 8f, 16, 16, num16, projectile.velocity.X / 2f, projectile.velocity.Y / 2f);
                        Main.dust[num18].velocity = Vector2.Normalize(vector3 - vector4) * 1.5f * (10f - (float)num13 * 2f) / 10f;
                        Main.dust[num18].noGravity = true;
                        Main.dust[num18].scale = num17;
                        Main.dust[num18].customData = player;
                    }
                }
                if (flag9)
                {
                    Vector2 spinningpoint5 = Vector2.UnitX * 14f;
                    spinningpoint5 = spinningpoint5.RotatedBy(projectile.rotation - (float)Math.PI / 2f);
                    Vector2 vector5 = projectile.Center + spinningpoint5;
                    for (int l = 0; l < 2; l++)
                    {
                        int num19 = 226;
                        float num20 = 0.35f;
                        if (l % 2 == 1)
                        {
                            num19 = 226;
                            num20 = 0.45f;
                        }
                        float num21 = Main.rand.NextFloatDirection();
                        Vector2 vector6 = vector5 + (projectile.rotation + num21 * ((float)Math.PI / 4f) * 0.8f - (float)Math.PI / 2f).ToRotationVector2() * 6f;
                        int num22 = 24;
                        int num24 = Dust.NewDust(vector6 - Vector2.One * (num22 / 2), num22, num22, num19, projectile.velocity.X / 2f, projectile.velocity.Y / 2f);
                        Main.dust[num24].velocity = (vector6 - vector5).SafeNormalize(Vector2.Zero) * MathHelper.Lerp(1.5f, 9f, Utils.GetLerpValue(1f, 0f, Math.Abs(num21), clamped: true));
                        Main.dust[num24].noGravity = true;
                        Main.dust[num24].scale = num20;
                        Main.dust[num24].customData = player;
                        Main.dust[num24].fadeIn = 0.5f;
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
                                SoundStyle style = new SoundStyle("Terraria/Sounds/Item_75") with { Volume = 0.75f, Pitch = 0f, PitchVariance = .05f, MaxInstances = -1}; 
                                SoundEngine.PlaySound(style, projectile.Center);

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
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //return true;
            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //Border
            for (int i = 0; i < 6; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.SkyBlue with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.07f, SpriteEffects.None);
            }

            for (int i = 20; i < 8; i++)
            {
                float dist = 2f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.01f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle, 
                    Color.SkyBlue with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            //We NEED to return true or else the laser wont draw for some reason

            return true;
            return false;

        }
    }

    public class ChargedBlasterOrbShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ChargedBlasterOrb);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //Big Shot
            if (timer == 0 && projectile.ai[1] >= 1)
            {
                float circlePulseSize = 0.35f;
                Color inBetweenCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.25f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.4f * projectile.scale, newColor: inBetweenCol * 0.25f);
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

            //Little Shot
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


            int trailCount = (int)(22f * projectile.scale); //8
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 3 == 0 && Main.rand.NextBool() && timer > 5 && false)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1.5f, 1.5f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), dustVel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.4f, 0.6f) * 3f * projectile.scale);
                da.velocity += projectile.velocity.RotatedByRandom(0.25f) * 0.4f;

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.91f, FadeVelPower: 0.92f, Pixelize: true, XScale: 0.8f, YScale: 0.5f, WhiteLayerPower: 0.85f);
                esb.randomVelRotatePower = 0.15f;
                da.customData = esb;

            }


            float timeForPopInAnim = 35;
            float animProgress = Math.Clamp((timer + 8) / timeForPopInAnim, 0f, 1f);
            //overallScale = MathHelper.Lerp(0f, 1f, Easings.easeOutCubic(animProgress));
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
                    int num192 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 226, projectile.velocity.X);
                    Main.dust[num192].position -= Vector2.One * 3f;
                    Main.dust[num192].scale = 0.5f;
                    Main.dust[num192].noGravity = true;
                    Main.dust[num192].velocity = projectile.velocity.RotatedByRandom(0.1f) / 2.5f;
                    Main.dust[num192].alpha = 255 - (int)(255f * projectile.scale);
                }
            }
            #endregion
            return false;
        }

        float overallAlpha = 0f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawAfterImage(projectile, false);
            });

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 baseVec2Scale = new Vector2(1f, overallScale) * projectile.scale;

            Texture2D SoftGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;
            Vector2 vec2Scale = new Vector2(1f, 0.85f * overallScale) * projectile.scale * overallScale;
            Color inBetween = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 1f) * 0.5f;
            Main.EntitySpriteDraw(SoftGlow, drawPos, null, inBetween with { A = 0 } * 0.15f * overallAlpha, projectile.rotation, SoftGlow.Size() / 2f, vec2Scale * 2.35f, se);
            Main.EntitySpriteDraw(SoftGlow, drawPos, null, Color.SkyBlue with { A = 0 } * 0.2f * overallAlpha, projectile.rotation, SoftGlow.Size() / 2f, vec2Scale * 1.75f, se);


            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3f, 3f), sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * 1f * overallAlpha, projectile.rotation, TexOrigin, baseVec2Scale * 1.05f, se);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * overallAlpha, projectile.rotation, TexOrigin, baseVec2Scale, se);
            return false;

        }

        public void DrawAfterImage(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 baseVec2Scale = new Vector2(1f, overallScale) * projectile.scale;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Vector2 sizeSmall = baseVec2Scale * (0.5f + (progress * 0.5f));
                    Vector2 sizeBig = baseVec2Scale * (0.34f + (progress * 0.66f));

                    Color col = Color.Lerp(Color.Black, Color.White, progress) * Easings.easeInSine(progress) * overallAlpha;
                    Color col2 = Color.Lerp(Color.White, Color.White, progress) * Easings.easeInSine(progress) * overallAlpha;


                    Main.EntitySpriteDraw(vanillaTex, previousPostions[i] - Main.screenPosition, sourceRectangle, col with { A = 0 } * 0.3f,
                            previousRotations[i], TexOrigin, sizeBig, se);


                    Vector2 randOffset = Main.rand.NextVector2Circular(5f, 5f);

                    Vector2 vec2Scaleae = new Vector2(1.15f, 0.5f * progress) * sizeSmall;
                    Main.EntitySpriteDraw(vanillaTex, previousPostions[i] - Main.screenPosition + randOffset, sourceRectangle, col2 with { A = 0 } * 0.65f, //55
                        previousRotations[i], TexOrigin, vec2Scaleae, se);
                }

            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

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
            for (int num296 = 0; num296 < num293; num296++)
            {
                int num298 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                Main.dust[num298].position = new Vector2(projectile.width / 2, 0f).RotatedBy(6.2831854820251465 * Main.rand.NextDouble()) * (float)Main.rand.NextDouble() + projectile.Center;
            }
            for (int num299 = 0; num299 < num294; num299++)
            {
                int num300 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 226, 0f, 0f, 0, default(Color), 1.5f);
                Main.dust[num300].position = new Vector2(projectile.width / 2, 0f).RotatedBy(6.2831854820251465 * Main.rand.NextDouble()) * (float)Main.rand.NextDouble() + projectile.Center;
                Main.dust[num300].noGravity = true;
                Dust dust24 = Main.dust[num300];
                Dust dust334 = dust24;
                dust334.velocity *= 1f;
            }
            for (int num301 = 0; num301 < num295; num301++)
            {
                int num302 = Gore.NewGore(null, projectile.position + new Vector2((float)(projectile.width * Main.rand.Next(100)) / 100f, (float)(projectile.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64));
                Gore gore9 = Main.gore[num302];
                Gore gore64 = gore9;
                gore64.velocity *= 0.3f;
                Main.gore[num302].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                Main.gore[num302].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
            }

            #endregion

            for (int i = 220; i < 9 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.Gold * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);
            }

            return false;
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }


    public class ChargedBlasterCannonLaserOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ChargedBlasterLaser);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            

            timer++;

            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return true;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = (0.75f + (progress * 0.25f)) * projectile.scale;

                    Color col = Color.LightGoldenrodYellow * progress * projectile.Opacity;
                    // Color.Lerp(Color.Gold, Color.LightGoldenrodYellow, progress) * progress * projectile.Opacity;

                    float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f, //0.5f
                            previousRotations[i], TexOrigin, size2, SpriteEffects.None);

                }

            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.Gold with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);
            return false;

        }
    }

}
