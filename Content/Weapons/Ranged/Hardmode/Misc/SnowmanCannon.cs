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
using Terraria.Graphics;
using Terraria.Physics;
using VFXPlus.Content.Projectiles;
using VFXPLus.Common;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    public class SnowmanCannon : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SnowmanCannon);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicGunProjMiddle>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicGunProjMiddle held)
            {
                held.SetProjInfo(
                    GunID: ItemID.SnowmanCannon,
                    AnimTime: 14,
                    NormalXOffset: 19f,
                    DestXOffset: 0f,
                    YRecoilAmount: 0.2f,
                    HoldOffset: new Vector2(0f, 1f),
                    TipPos: new Vector2(40f, -1f),
                    StarPos: new Vector2(30f, -1f)
                    );

                held.timeToStartFade = 2;
                held.isShotgun = true;
            }

            //Explosion
            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(36f, -2f * dir).RotatedBy(velocity.ToRotation()) + new Vector2(0f, 1f); //4448

            for (int i = 0; i < 9; i++) //16
            {
                Color col1 = Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, 0.5f);

                float progress = (float)i / 8;
                Color col = Color.Lerp(Color.SkyBlue * 0.25f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.45f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(10, 24), 0.98f, 0.01f, 0.75f);

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 1f;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.SkyBlue, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.14f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 2 + Main.rand.Next(0, 2); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.SkyBlue, Scale: Main.rand.NextFloat(0.3f, 0.6f) * 1.5f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.9f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 5f;
            }

            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.25f;
            Color pulseColor = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.35f);

            Dust d2 = Dust.NewDustPerfect(position + velNormalized * 2f, ModContent.DustType<CirclePulse>(), velNormalized * 4f, newColor: pulseColor);
            CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            b2.drawLayer = "Dusts";
            d2.customData = b2;
            d2.scale = circlePulseSize * 0.15f;

            SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion with { Volume = 0.35f, PitchVariance = 0.08f, Pitch = 0.5f, MaxInstances = 1 }, player.Center);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Fire/FlareImpact") with { Volume = 0.12f, Pitch = -0.8f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            //SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/rescue_ranger_fire") with { Volume = .03f, Pitch = .65f, PitchVariance = .05f, MaxInstances = 1 };
            //SoundEngine.PlaySound(style2, player.Center);

            return true;
        }
    }
    public class SnowmanCannonShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            bool rocket12 = entity.type == ProjectileID.RocketSnowmanI || entity.type == ProjectileID.RocketSnowmanII;
            bool rocket34 = entity.type == ProjectileID.RocketSnowmanIII || entity.type == ProjectileID.RocketSnowmanIV;
            bool rocketNuke = entity.type == ProjectileID.MiniNukeSnowmanRocketI || entity.type == ProjectileID.MiniNukeSnowmanRocketII;
            bool rocketCluster = entity.type == ProjectileID.ClusterSnowmanRocketI || entity.type == ProjectileID.ClusterSnowmanRocketII;

            bool rocketLiquid = entity.type == ProjectileID.DrySnowmanRocket || entity.type == ProjectileID.WetSnowmanRocket
                || entity.type == ProjectileID.LavaSnowmanRocket || entity.type == ProjectileID.HoneySnowmanRocket;

            return lateInstantiation && (rocket12 || rocket34 || rocketNuke || rocketCluster || rocketLiquid);
        }


        //For some fucking reason, rockets projectiles persist for two frames after hitting and enemy or tileColliding
        //But thats makes the explosion feel weird as shit so we are just pretending that doesn't happen
        Vector2 storedPosition = Vector2.Zero;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 24;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            //Trailing Fire Dust
            if (timer % 2 == 0 && Main.rand.NextBool() && timer > 10)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.35f;

                Dust dp = Dust.NewDustPerfect(dustPos, ModContent.DustType<Snowflakes>(), dustVel, newColor: Color.White, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1.15f);
                dp.rotation = Main.rand.NextFloat(6.28f);
                SnowflakeBehavior sb = new SnowflakeBehavior(VelShrinkAmount: 0.93f, ScaleShrinkAmount: 0.91f, AlphaShrinkAmount: 0.92f, ColorIntensity: 1f);
                dp.customData = sb;
            }

            if ((timer + 1) % 3 == 0)
            {
                int num4 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.IceTorch, projectile.velocity.X * -0.4f, projectile.velocity.Y * -0.4f, 100, default(Color), 1.2f);
                Main.dust[num4].noGravity = true;
                Main.dust[num4].velocity.X *= 4f;
                Main.dust[num4].velocity.Y *= 4f;
                Main.dust[num4].velocity = -(Main.dust[num4].velocity + projectile.velocity) / 2f;
            }


            if (timer % 1 == 0 && timer > 5)
            {
                Color between = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 0.35f) * 0.5f;


                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f, 
                    newColor: between with { A = 55 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.4f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
                d.velocity += projectile.velocity * -0.1f;

                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.4f);
                d2.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f;
            }

            float fadeInTime = Math.Clamp((timer + 7f) / 17f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            timer++;

            #region vanillaAI (jumpscare warning)
            if (projectile.wet && (projectile.type == 799 || projectile.type == 800 || projectile.type == 801 || projectile.type == 810 || projectile.type == 906 || projectile.type == 784 || projectile.type == 785 || projectile.type == 786 || projectile.type == 805 || projectile.type == 903 || projectile.type == 787 || projectile.type == 788 || projectile.type == 789 || projectile.type == 806 || projectile.type == 904 || projectile.type == 790 || projectile.type == 791 || projectile.type == 792 || projectile.type == 807 || projectile.type == 905))
            {
                storedPosition = projectile.Center;

                projectile.timeLeft = 1;
            }
            if (projectile.type == 108 || projectile.type == 164 || projectile.type == 1002)
            {
                projectile.ai[0] += 1f;
                if (projectile.ai[0] > 3f)
                {
                    projectile.Kill();
                }
            }
            if (projectile.type == 102)
            {
                int num = (int)(projectile.Center.X / 16f);
                int num12 = (int)(projectile.Center.Y / 16f);
                if (WorldGen.InWorld(num, num12))
                {
                    Tile tile = Main.tile[num, num12];
                    if (tile != null && tile.HasTile && (TileID.Sets.Platforms[tile.TileType] || tile.TileType == 380))
                    {
                        projectile.Kill();
                        return false;
                    }
                }
            }
            if (projectile.type == 75)
            {
                if (projectile.localAI[0] == 0f)
                {
                    projectile.localAI[0] = 1f;
                    ///SoundEngine.PlaySound(66, (int)projectile.position.X, (int)projectile.position.Y);
                }
                for (int i = 0; i < 255; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && !Main.player[i].ghost && (projectile.Center - Main.player[i].Center).Length() < 40f)
                    {
                        projectile.Kill();
                        return false;
                    }
                }
            }
            bool flag = false;
            if (projectile.type == 37 || projectile.type == 397 || projectile.type == 470 || projectile.type == 519 || projectile.type == 773 || projectile.type == 911)
            {
                try
                {
                    int num23 = (int)(projectile.position.X / 16f) - 1;
                    int num24 = (int)((projectile.position.X + (float)projectile.width) / 16f) + 2;
                    int num25 = (int)(projectile.position.Y / 16f) - 1;
                    int num26 = (int)((projectile.position.Y + (float)projectile.height) / 16f) + 2;
                    if (num23 < 0)
                    {
                        num23 = 0;
                    }
                    if (num24 > Main.maxTilesX)
                    {
                        num24 = Main.maxTilesX;
                    }
                    if (num25 < 0)
                    {
                        num25 = 0;
                    }
                    if (num26 > Main.maxTilesY)
                    {
                        num26 = Main.maxTilesY;
                    }
                    Vector2 vector = default(Vector2);
                    for (int j = num23; j < num24; j++)
                    {
                        for (int k = num25; k < num26; k++)
                        {
                            if (Main.tile[j, k] == null || !Main.tile[j, k].HasTile || !Main.tileSolid[Main.tile[j, k].TileType] || Main.tileSolidTop[Main.tile[j, k].TileType])
                            {
                                continue;
                            }
                            vector.X = j * 16;
                            vector.Y = k * 16;
                            if (!(projectile.position.X + (float)projectile.width - 4f > vector.X) || !(projectile.position.X + 4f < vector.X + 16f) || !(projectile.position.Y + (float)projectile.height - 4f > vector.Y) || !(projectile.position.Y + 4f < vector.Y + 16f))
                            {
                                continue;
                            }
                            if (projectile.type == 911 && projectile.owner == Main.myPlayer && projectile.localAI[0] == 0f)
                            {
                                float num27 = 12f;
                                Vector2 value = vector + new Vector2(8f, 8f);
                                if (Vector2.Distance(projectile.Center, value) < num27)
                                {
                                    projectile.Center += projectile.velocity.SafeNormalize(Vector2.Zero) * -4f;
                                }
                                projectile.localAI[0] = 1f;
                                projectile.netUpdate = true;
                            }
                            projectile.velocity.X = 0f;
                            projectile.velocity.Y = -0.2f;
                            flag = true;
                        }
                    }
                }
                catch
                {
                }
            }
            if (flag && projectile.type == 911)
            {
                Point p = projectile.Center.ToTileCoordinates();
                if (WorldGen.SolidOrSlopedTile(Framing.GetTileSafely(p.X, p.Y)))
                {
                    Vector2 v = p.ToWorldCoordinates() - projectile.Center;
                    projectile.Center += v.SafeNormalize(Vector2.Zero) * -4f;
                }
            }
            if (flag && projectile.type == 773)
            {
                Player player = Main.player[projectile.owner];
                Vector2 v2 = projectile.DirectionTo(player.Center).SafeNormalize(Vector2.UnitX * player.direction);
                float num28 = projectile.rotation;
                float num29 = v2.ToRotation() + (float)Math.PI / 2f;
                projectile.rotation = projectile.rotation.AngleLerp(num29, 0.2f);
                projectile.rotation = projectile.rotation.AngleTowards(num29, 0.05f);
                Vector2 vector2 = (projectile.rotation - (float)Math.PI / 2f).ToRotationVector2();
                if (Main.rand.Next(3) == 0 && false)
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center + vector2 * 10f, 59, vector2 * 2f + Main.rand.NextVector2Circular(0.25f, 0.25f), 0, default(Color), 2f);
                    dust.noGravity = true;
                    if (Main.rand.Next(3) == 0 && false)
                    {
                        dust.velocity *= 1.5f;
                        dust.noGravity = false;
                        dust.scale /= 2f;
                    }
                }
                if (Main.rand.Next(3) == 0 && false)
                {
                    //Nothanks
                    //Point scarabBombDigDirectionSnap = projectile.GetScarabBombDigDirectionSnap8();
                    //Dust.NewDustPerfect(projectile.Center + vector2 * -10f, 59, scarabBombDigDirectionSnap.ToVector2() * 1.5f, 0, default(Color), 2f).noGravity = true;
                }
                if (Main.rand.Next(15) == 0 && false)
                {
                    Dust dust2 = Dust.NewDustPerfect(projectile.Center + vector2 * 10f, 88, vector2 * 3f + Main.rand.NextVector2Circular(0.25f, 0.25f), 0, default(Color), 2f);
                    dust2.noGravity = true;
                    if (Main.rand.Next(3) == 0)
                    {
                        dust2.velocity *= 1.5f;
                    }
                }
                bool flag2 = Main.rand.Next(30) == 0;
                if (num28 != projectile.rotation && Main.rand.Next(40) == 0)
                {
                    flag2 = true;
                }
                if (flag2 && false)
                {
                    float num2 = (float)Math.PI * 2f * Main.rand.NextFloat();
                    for (float num3 = 0f; num3 < 1f; num3 += 1f / 7f)
                    {
                        Vector2 spinningpoint = (num3 * ((float)Math.PI * 2f) + num2).ToRotationVector2();
                        spinningpoint *= new Vector2(1f, 0.3f);
                        spinningpoint = spinningpoint.RotatedBy(num29);
                        Dust dust5 = Dust.NewDustPerfect(projectile.Center + spinningpoint + vector2 * 8f, 59, vector2 * 3f + spinningpoint);
                        dust5.noGravity = true;
                        dust5.fadeIn = 1.6f;
                    }
                }
                if (++projectile.frameCounter >= 3)
                {
                    projectile.frameCounter = 0;
                    if (++projectile.frame >= 4)
                    {
                        projectile.frame = 0;
                    }
                }
            }
            if (projectile.type == 519)
            {
                projectile.localAI[1] += 1f;
                float num4 = 180f - projectile.localAI[1];
                if (num4 < 0f)
                {
                    num4 = 0f;
                }
                projectile.frameCounter++;
                if (num4 < 15f)
                {
                    projectile.frameCounter++;
                }
                if ((float)projectile.frameCounter >= (num4 / 10f + 6f) / 2f)
                {
                    projectile.frame++;
                    projectile.frameCounter = 0;
                    if (projectile.frame >= Main.projFrames[projectile.type])
                    {
                        projectile.frame = 0;
                    }
                }
            }
            if (projectile.type == 681 && projectile.localAI[1] == 0f)
            {
                projectile.localAI[1] = 1f;
            }
            int num5 = 6;
            if (projectile.type == 776 || projectile.type == 780 || projectile.type == 803 || projectile.type == 804)
            {
                num5 = 228;
            }
            else if (projectile.type == 784 || projectile.type == 805)
            {
                num5 = ((Main.rand.Next(3) == 0) ? 6 : Dust.dustWater());
            }
            else if (projectile.type == 787 || projectile.type == 806)
            {
                num5 = ((Main.rand.Next(3) == 0) ? 6 : 35);
            }
            else if (projectile.type == 790 || projectile.type == 807)
            {
                num5 = ((Main.rand.Next(3) == 0) ? 6 : 152);
            }
            if (projectile.type == 102)
            {
                if (projectile.velocity.Y > 10f)
                {
                    projectile.velocity.Y = 10f;
                }
                if (projectile.localAI[0] == 0f)
                {
                    projectile.localAI[0] = 1f;
                    SoundEngine.PlaySound(in SoundID.Item10, projectile.position);
                }
                projectile.frameCounter++;
                if (projectile.frameCounter > 3)
                {
                    projectile.frame++;
                    projectile.frameCounter = 0;
                }
                if (projectile.frame > 1)
                {
                    projectile.frame = 0;
                }
                if (projectile.velocity.Y == 0f)
                {
                    projectile.position.X += projectile.width / 2;
                    projectile.position.Y += projectile.height / 2;
                    projectile.width = 128;
                    projectile.height = 128;
                    projectile.position.X -= projectile.width / 2;
                    projectile.position.Y -= projectile.height / 2;
                    projectile.damage = 40;
                    projectile.knockBack = 8f;
                    projectile.timeLeft = 3;
                    projectile.netUpdate = true;
                }
            }
            if (projectile.type == 303 && projectile.timeLeft <= 3 && projectile.hostile)
            {
                projectile.position.X += projectile.width / 2;
                projectile.position.Y += projectile.height / 2;
                projectile.width = 128;
                projectile.height = 128;
                projectile.position.X -= projectile.width / 2;
                projectile.position.Y -= projectile.height / 2;
            }
            if (projectile.owner == Main.myPlayer && projectile.timeLeft <= 3)
            {
                projectile.PrepareBombToBlow();
            }
            else
            {
                if (projectile.type != 30 && projectile.type != 75 && projectile.type != 517 && projectile.type != 681 && projectile.type != 588 && projectile.type != 397 && projectile.type != 108 && projectile.type != 1002 && projectile.type != 133 && projectile.type != 134 && projectile.type != 135 && projectile.type != 136 && projectile.type != 137 && projectile.type != 138 && projectile.type != 139 && projectile.type != 140 && projectile.type != 141 && projectile.type != 142 && projectile.type != 143 && projectile.type != 144 && projectile.type != 164 && projectile.type != 303 && projectile.type != 338 && projectile.type != 339 && projectile.type != 340 && projectile.type != 341 && (projectile.type < 776 || projectile.type > 801) && (projectile.type < 803 || projectile.type > 810) && projectile.type != 862 && projectile.type != 863 && projectile.type != 930)
                {
                    projectile.damage = 0;
                }
                if (projectile.type == 338 || projectile.type == 339 || projectile.type == 340 || projectile.type == 341 || projectile.type == 803 || projectile.type == 804 || projectile.type == 808 || projectile.type == 809 || projectile.type == 810 || projectile.type == 805 || projectile.type == 806 || projectile.type == 807 || projectile.type == 930)
                {
                    projectile.localAI[1] += 1f;
                    if (projectile.localAI[1] > 6f)
                    {
                        projectile.alpha = 0;
                    }
                    else
                    {
                        projectile.alpha = (int)(255f - 42f * projectile.localAI[1]) + 100;
                        if (projectile.alpha > 255)
                        {
                            projectile.alpha = 255;
                        }
                    }
                    for (int l = 0; l < 2; l++)
                    {
                        float num6 = 0f;
                        float num7 = 0f;
                        if (l == 1)
                        {
                            num6 = projectile.velocity.X * 0.5f;
                            num7 = projectile.velocity.Y * 0.5f;
                        }
                        if (!(projectile.localAI[1] > 9f))
                        {
                            continue;
                        }
                        if (Main.rand.Next(2) == 0 && false)
                        {
                            int num8 = Dust.NewDust(new Vector2(projectile.position.X + 3f + num6, projectile.position.Y + 3f + num7) - projectile.velocity * 0.5f, projectile.width - 8, projectile.height - 8, num5, 0f, 0f, 100);
                            Main.dust[num8].scale *= 1.4f + (float)Main.rand.Next(10) * 0.1f;
                            Main.dust[num8].velocity *= 0.2f;
                            Main.dust[num8].noGravity = true;
                            if (Main.dust[num8].type == 152)
                            {
                                Main.dust[num8].scale *= 0.5f;
                                Main.dust[num8].velocity += projectile.velocity * 0.1f;
                            }
                            else if (Main.dust[num8].type == 35)
                            {
                                Main.dust[num8].scale *= 0.5f;
                                Main.dust[num8].velocity += projectile.velocity * 0.1f;
                            }
                            else if (Main.dust[num8].type == Dust.dustWater())
                            {
                                Main.dust[num8].scale *= 0.65f;
                                Main.dust[num8].velocity += projectile.velocity * 0.1f;
                            }
                            if (projectile.type == 808 || projectile.type == 809)
                            {
                                Dust dust3 = Main.dust[num8];
                                if (dust3.dustIndex != 6000)
                                {
                                    dust3 = Dust.NewDustPerfect(dust3.position, dust3.type, dust3.velocity, dust3.alpha, dust3.color, dust3.scale + 0.5f);
                                    dust3.velocity = Main.rand.NextVector2Circular(3f, 3f);
                                    dust3.noGravity = true;
                                }
                                if (dust3.dustIndex != 6000)
                                {
                                    dust3 = Dust.NewDustPerfect(dust3.position, dust3.type, dust3.velocity, dust3.alpha, dust3.color, dust3.scale + 0.5f);
                                    dust3.velocity = ((float)Math.PI * 2f * ((float)projectile.timeLeft / 20f)).ToRotationVector2() * 3f;
                                    dust3.noGravity = true;
                                }
                            }
                        }
                        if (Main.rand.Next(2) == 0 && false)
                        {
                            int num9 = Dust.NewDust(new Vector2(projectile.position.X + 3f + num6, projectile.position.Y + 3f + num7) - projectile.velocity * 0.5f, projectile.width - 8, projectile.height - 8, 31, 0f, 0f, 100, default(Color), 0.5f);
                            Main.dust[num9].fadeIn = 0.5f + (float)Main.rand.Next(5) * 0.1f;
                            Main.dust[num9].velocity *= 0.05f;
                        }
                    }
                    float x = projectile.position.X;
                    float y = projectile.position.Y;
                    float num10 = 600f;
                    if (projectile.type == 930)
                    {
                        num10 = 650f;
                    }
                    bool flag3 = false;
                    projectile.ai[0] += 1f;
                    if (projectile.ai[0] > 30f)
                    {
                        projectile.ai[0] = 30f;
                        for (int m = 0; m < 200; m++)
                        {
                            if (Main.npc[m].CanBeChasedBy(projectile))
                            {
                                float num11 = Main.npc[m].position.X + (float)(Main.npc[m].width / 2);
                                float num13 = Main.npc[m].position.Y + (float)(Main.npc[m].height / 2);
                                float num14 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num11) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num13);
                                if (num14 < num10 && Collision.CanHit(projectile.position, projectile.width, projectile.height, Main.npc[m].position, Main.npc[m].width, Main.npc[m].height))
                                {
                                    num10 = num14;
                                    x = num11;
                                    y = num13;
                                    flag3 = true;
                                }
                            }
                        }
                    }
                    if (!flag3)
                    {
                        x = projectile.position.X + (float)(projectile.width / 2) + projectile.velocity.X * 100f;
                        y = projectile.position.Y + (float)(projectile.height / 2) + projectile.velocity.Y * 100f;
                    }
                    float num15 = 16f;
                    if (projectile.type == 930)
                    {
                        num15 = 12f;
                    }
                    Vector2 value2 = (new Vector2(x, y) - projectile.Center).SafeNormalize(-Vector2.UnitY) * num15;
                    projectile.velocity = Vector2.Lerp(projectile.velocity, value2, 1f / 12f);
                }
                else if (projectile.type == 134 || projectile.type == 137 || projectile.type == 140 || projectile.type == 143 || projectile.type == 303 || projectile.type == 776 || projectile.type == 780 || projectile.type == 793 || projectile.type == 796 || projectile.type == 799 || projectile.type == 784 || projectile.type == 787 || projectile.type == 790)
                {
                    if (Math.Abs(projectile.velocity.X) >= 8f || Math.Abs(projectile.velocity.Y) >= 8f)
                    {
                        for (int n = 220; n < 2; n++) //!
                        {
                            float num16 = 0f;
                            float num17 = 0f;
                            if (n == 1)
                            {
                                num16 = projectile.velocity.X * 0.5f;
                                num17 = projectile.velocity.Y * 0.5f;
                            }
                            int num18 = Dust.NewDust(new Vector2(projectile.position.X + 3f + num16, projectile.position.Y + 3f + num17) - projectile.velocity * 0.5f, projectile.width - 8, projectile.height - 8, num5, 0f, 0f, 100);
                            Main.dust[num18].scale *= 2f + (float)Main.rand.Next(10) * 0.1f;
                            Main.dust[num18].velocity *= 0.2f;
                            Main.dust[num18].noGravity = true;
                            if (Main.dust[num18].type == 152)
                            {
                                Main.dust[num18].scale *= 0.5f;
                                Main.dust[num18].velocity += projectile.velocity * 0.1f;
                            }
                            else if (Main.dust[num18].type == 35)
                            {
                                Main.dust[num18].scale *= 0.5f;
                                Main.dust[num18].velocity += projectile.velocity * 0.1f;
                            }
                            else if (Main.dust[num18].type == Dust.dustWater())
                            {
                                Main.dust[num18].scale *= 0.65f;
                                Main.dust[num18].velocity += projectile.velocity * 0.1f;
                            }
                            if (projectile.type == 793 || projectile.type == 796)
                            {
                                Dust dust4 = Main.dust[num18];
                                if (dust4.dustIndex != 6000)
                                {
                                    dust4 = Dust.NewDustPerfect(dust4.position, dust4.type, dust4.velocity, dust4.alpha, dust4.color, dust4.scale);
                                    dust4.velocity = Main.rand.NextVector2Circular(3f, 3f);
                                    dust4.noGravity = true;
                                }
                                if (dust4.dustIndex != 6000)
                                {
                                    dust4 = Dust.NewDustPerfect(dust4.position, dust4.type, dust4.velocity, dust4.alpha, dust4.color, dust4.scale);
                                    dust4.velocity = ((float)Math.PI * 2f * ((float)projectile.timeLeft / 20f)).ToRotationVector2() * 3f;
                                    dust4.noGravity = true;
                                }
                            }
                            num18 = Dust.NewDust(new Vector2(projectile.position.X + 3f + num16, projectile.position.Y + 3f + num17) - projectile.velocity * 0.5f, projectile.width - 8, projectile.height - 8, 31, 0f, 0f, 100, default(Color), 0.5f);
                            Main.dust[num18].fadeIn = 1f + (float)Main.rand.Next(5) * 0.1f;
                            Main.dust[num18].velocity *= 0.05f;
                        }
                    }
                    if (Math.Abs(projectile.velocity.X) < 15f && Math.Abs(projectile.velocity.Y) < 15f)
                    {
                        projectile.velocity *= 1.1f;
                    }
                }
                else if (projectile.type == 133 || projectile.type == 136 || projectile.type == 139 || projectile.type == 142 || projectile.type == 777 || projectile.type == 781 || projectile.type == 794 || projectile.type == 797 || projectile.type == 800 || projectile.type == 785 || projectile.type == 788 || projectile.type == 791)
                {
                    //int num19 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100);
                    //Main.dust[num19].scale *= 1f + (float)Main.rand.Next(10) * 0.1f;
                    //Main.dust[num19].velocity *= 0.2f;
                    //Main.dust[num19].noGravity = true;
                }
                else if (projectile.type == 135 || projectile.type == 138 || projectile.type == 141 || projectile.type == 144 || projectile.type == 778 || projectile.type == 782 || projectile.type == 795 || projectile.type == 798 || projectile.type == 801 || projectile.type == 786 || projectile.type == 789 || projectile.type == 792)
                {
                    if ((double)projectile.velocity.X > -0.2 && (double)projectile.velocity.X < 0.2 && (double)projectile.velocity.Y > -0.2 && (double)projectile.velocity.Y < 0.2)
                    {
                        projectile.alpha += 2;
                        if (projectile.alpha > 200)
                        {
                            projectile.alpha = 200;
                        }
                    }
                    else
                    {
                        projectile.alpha = 0;
                        //int num20 = Dust.NewDust(new Vector2(projectile.position.X + 3f, projectile.position.Y + 3f) - projectile.velocity * 0.5f, projectile.width - 8, projectile.height - 8, 31, 0f, 0f, 100);
                        //Main.dust[num20].scale *= 1.6f + (float)Main.rand.Next(5) * 0.1f;
                        //Main.dust[num20].velocity *= 0.05f;
                        //Main.dust[num20].noGravity = true;
                    }
                }
                else if (projectile.type == 779 || projectile.type == 783 || projectile.type == 862 || projectile.type == 863)
                {
                    if (Main.rand.Next(25) == 0)
                    {
                        //Dust dust6 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 228, (0f - projectile.velocity.X) / 10f, (0f - projectile.velocity.Y) / 10f, 100);
                        //dust6.noGravity = true;
                        //dust6.velocity *= 0f;
                        //dust6.scale = 1.3f;
                    }
                    if (Main.rand.Next(5) == 0)
                    {
                        //Dust dust7 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, (0f - projectile.velocity.X) / 10f, (0f - projectile.velocity.Y) / 10f, 100);
                        //dust7.noGravity = true;
                        //dust7.velocity *= 0f;
                        //dust7.scale = 1.3f;
                    }
                    if (projectile.frameCounter == 0)
                    {
                        projectile.frameCounter = 1;
                        projectile.frame = Main.rand.Next(4);
                    }
                }
                else if (projectile.type != 30 && projectile.type != 517 && projectile.type != 681 && projectile.type != 397 && projectile.type != 519 && projectile.type != 588 && projectile.type != 779 && projectile.type != 783 && projectile.type != 862 && projectile.type != 863 && Main.rand.Next(2) == 0)
                {
                    //int num21 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100);
                    //Main.dust[num21].scale = 0.1f + (float)Main.rand.Next(5) * 0.1f;
                    //Main.dust[num21].fadeIn = 1.5f + (float)Main.rand.Next(5) * 0.1f;
                    //Main.dust[num21].noGravity = true;
                    //Main.dust[num21].position = projectile.Center + new Vector2(0f, -projectile.height / 2).RotatedBy(projectile.rotation) * 1.1f;
                    int num22 = 6;
                    if (projectile.type == 773)
                    {
                        num22 = 59;
                    }
                    if (projectile.type == 903)
                    {
                        num22 = Dust.dustWater();
                    }
                    if (projectile.type == 904)
                    {
                        num22 = 35;
                    }
                    if (projectile.type == 905)
                    {
                        num22 = 152;
                    }
                    if (projectile.type == 910 || projectile.type == 911)
                    {
                        num22 = 0;
                    }
                    //Dust dust8 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num22, 0f, 0f, 100);
                    //dust8.scale = 1f + (float)Main.rand.Next(5) * 0.1f;
                    //dust8.noGravity = true;
                    //dust8.position = projectile.Center + new Vector2(0f, -projectile.height / 2 - 6).RotatedBy(projectile.rotation) * 1.1f;
                }
                else if (projectile.type == 681)
                {
                    //Dust dust9 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100);
                    //dust9.scale = 1f + (float)Main.rand.Next(5) * 0.1f;
                    //dust9.noGravity = true;
                    //dust9.position = projectile.Center + new Vector2(6 * Math.Sign(projectile.velocity.X), -projectile.height / 2 - 6).RotatedBy(projectile.rotation) * 1.1f;
                }
            }
            projectile.ai[0] += 1f;
            if (projectile.type == 338 || projectile.type == 339 || projectile.type == 340 || projectile.type == 341 || projectile.type == 803 || projectile.type == 804 || projectile.type == 808 || projectile.type == 809 || projectile.type == 810 || projectile.type == 805 || projectile.type == 806 || projectile.type == 807 || projectile.type == 930)
            {
                if (projectile.velocity.X < 0f)
                {
                    projectile.spriteDirection = -1;
                    projectile.rotation = (float)Math.Atan2(0f - projectile.velocity.Y, 0f - projectile.velocity.X) - 1.57f;
                }
                else
                {
                    projectile.spriteDirection = 1;
                    projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
                }
            }
            else if (projectile.type == 134 || projectile.type == 137 || projectile.type == 140 || projectile.type == 143 || projectile.type == 303 || projectile.type == 776 || projectile.type == 780 || projectile.type == 793 || projectile.type == 796 || projectile.type == 799 || projectile.type == 784 || projectile.type == 787 || projectile.type == 790)
            {
                if (projectile.velocity != Vector2.Zero)
                {
                    projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
                }
            }
            else if (projectile.type == 135 || projectile.type == 138 || projectile.type == 141 || projectile.type == 144 || projectile.type == 778 || projectile.type == 782 || projectile.type == 795 || projectile.type == 798 || projectile.type == 801 || projectile.type == 786 || projectile.type == 789 || projectile.type == 792)
            {
                projectile.velocity.Y += 0.2f;
                projectile.velocity *= 0.97f;
                if ((double)projectile.velocity.X > -0.1 && (double)projectile.velocity.X < 0.1)
                {
                    projectile.velocity.X = 0f;
                }
                if ((double)projectile.velocity.Y > -0.1 && (double)projectile.velocity.Y < 0.1)
                {
                    projectile.velocity.Y = 0f;
                }
            }
            else if (projectile.type == 133 || projectile.type == 136 || projectile.type == 139 || projectile.type == 142 || projectile.type == 777 || projectile.type == 781 || projectile.type == 794 || projectile.type == 797 || projectile.type == 800 || projectile.type == 785 || projectile.type == 788 || projectile.type == 791)
            {
                if (projectile.ai[0] > 15f)
                {
                    if (projectile.velocity.Y == 0f)
                    {
                        projectile.velocity.X *= 0.95f;
                    }
                    projectile.velocity.Y += 0.2f;
                }
            }
            else if (((projectile.type == 30 || projectile.type == 397 || projectile.type == 517 || projectile.type == 681 || projectile.type == 588 || projectile.type == 779 || projectile.type == 783 || projectile.type == 862 || projectile.type == 863) && projectile.ai[0] > 10f) || (projectile.type != 30 && projectile.type != 397 && projectile.type != 517 && projectile.type != 588 && projectile.type != 779 && projectile.type != 783 && projectile.type != 862 && projectile.type != 863 && projectile.ai[0] > 5f))
            {
                projectile.ai[0] = 10f;
                if (projectile.velocity.Y == 0f && projectile.velocity.X != 0f)
                {
                    projectile.velocity.X *= 0.97f;
                    if (projectile.type == 29 || projectile.type == 470 || projectile.type == 637)
                    {
                        projectile.velocity.X *= 0.99f;
                    }
                    if ((double)projectile.velocity.X > -0.01 && (double)projectile.velocity.X < 0.01)
                    {
                        projectile.velocity.X = 0f;
                        projectile.netUpdate = true;
                    }
                }
                projectile.velocity.Y += 0.2f;
                if (projectile.type == 911)
                {
                    projectile.velocity.X = MathHelper.Clamp(projectile.velocity.X, -8f, 8f);
                    projectile.velocity.Y = MathHelper.Clamp(projectile.velocity.Y, -8f, 8f);
                }
            }
            if (projectile.type == 519)
            {
                projectile.rotation += projectile.velocity.X * 0.06f;
            }
            else if (projectile.type != 134 && projectile.type != 137 && projectile.type != 140 && projectile.type != 143 && projectile.type != 303 && (projectile.type < 338 || projectile.type > 341) && projectile.type != 776 && projectile.type != 780 && projectile.type != 793 && projectile.type != 796 && projectile.type != 799 && projectile.type != 784 && projectile.type != 787 && projectile.type != 790 && projectile.type != 803 && projectile.type != 804 && projectile.type != 808 && projectile.type != 809 && projectile.type != 810 && projectile.type != 805 && projectile.type != 806 && projectile.type != 807 && projectile.type != 930)
            {
                projectile.rotation += projectile.velocity.X * 0.1f;
            }
            #endregion

            if (projectile.timeLeft == 2)
                storedPosition = projectile.Center;

            return false;
        }


        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (projectile.timeLeft < 2)
                return false;
            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float scale = projectile.scale * overallScale;

            Color col = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 0.3f);
            Color col2 = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color trailCol = Color.Lerp(col, col2, 1f - progress) * progress;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(6f, 6f) * progress;

                Vector2 flareScale = new Vector2(1f, 0.65f * progress) * scale;

                Main.EntitySpriteDraw(flare, AfterImagePos, null, trailCol with { A = 0 } * progress * 0.9f,
                    previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, flareScale, 0);
            }

            //Bloomball
            Texture2D Ball = CommonTextures.FireBallBlur.Value;
            Vector2 ballOff1 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f + new Vector2(0f, 0f);
            Main.EntitySpriteDraw(Ball, ballOff1, null, Color.SkyBlue with { A = 0 } * 0.35f, projectile.rotation, Ball.Size() / 2f, new Vector2(0.8f, 0.8f) * projectile.scale * overallScale, SE);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.SkyBlue with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (projectile.type == ProjectileID.RocketSnowmanI || projectile.type == ProjectileID.RocketSnowmanII)
            {
                SnowmanExplosionBase(projectile, storedPosition);

                if (projectile.type == ProjectileID.RocketSnowmanII && projectile.owner == Main.myPlayer)
                    TileExplosion(projectile);

            }
            else if (projectile.type == ProjectileID.RocketSnowmanIII || projectile.type == ProjectileID.RocketSnowmanIV)
            {
                SnowmanExplosionRocketIII(projectile, storedPosition);

                if (projectile.type == ProjectileID.RocketSnowmanIV && projectile.owner == Main.myPlayer)
                    TileExplosion(projectile);
            }
            else if (projectile.type == ProjectileID.MiniNukeSnowmanRocketI || projectile.type == ProjectileID.MiniNukeSnowmanRocketII)
            {
                SnowmanExplosionMiniNuke(projectile, storedPosition);

                if (projectile.type == ProjectileID.MiniNukeSnowmanRocketII && projectile.owner == Main.myPlayer)
                    TileExplosion(projectile);
            }
            else if (projectile.type == ProjectileID.ClusterSnowmanRocketI || projectile.type == ProjectileID.ClusterSnowmanRocketII)
            {
                SnowmanExplosionBase(projectile, storedPosition);

                if (projectile.type == ProjectileID.ClusterSnowmanRocketII && projectile.owner == Main.myPlayer)
                    TileExplosion(projectile);

                #region clusterKill
                projectile.Resize(22, 22);
                if (projectile.type == 777 || projectile.type == 781)
                {
                    SoundEngine.PlaySound(in SoundID.Item62, projectile.position);
                }
                else
                {
                    SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
                }
                Color transparent8 = Color.Transparent;
                for (int num910 = 1110; num910 < 30; num910++)
                {
                    Dust dust325 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, transparent8, 1.5f);
                    Dust dust68 = dust325;
                    Dust dust334 = dust68;
                    dust334.velocity *= 1.4f;
                }
                for (int num911 = 1110; num911 < 40; num911++)
                {
                    Dust dust326 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 228, 0f, 0f, 100, transparent8, 3.5f);
                    dust326.noGravity = true;
                    Dust dust69 = dust326;
                    Dust dust334 = dust69;
                    dust334.velocity *= 7f;
                    dust326 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 228, 0f, 0f, 100, transparent8, 1.3f);
                    dust69 = dust326;
                    dust334 = dust69;
                    dust334.velocity *= 4f;
                    dust326.noGravity = true;
                }
                for (int num912 = 11110; num912 < 8; num912++)
                {
                    Dust dust327 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 226, 0f, 0f, 100, transparent8, 1.3f);
                    Dust dust70 = dust327;
                    Dust dust334 = dust70;
                    dust334.velocity *= 4f;
                    dust327.noGravity = true;
                }
                for (int num913 = 11112; num913 <= 2; num913++)
                {
                    for (int num914 = -1; num914 <= 1; num914 += 2)
                    {
                        for (int num915 = -1; num915 <= 1; num915 += 2)
                        {
                            Gore gore5 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, Main.rand.Next(61, 64));
                            Gore gore19 = gore5;
                            Gore gore64 = gore19;
                            gore64.velocity *= ((num913 == 1) ? 0.4f : 0.8f);
                            gore19 = gore5;
                            gore64 = gore19;
                            gore64.velocity += new Vector2(num914, num915);
                        }
                    }
                }
                if (projectile.owner == Main.myPlayer)
                {
                    int num916 = 779;
                    if (projectile.type == 780 || projectile.type == 781 || projectile.type == 782)
                    {
                        num916 = 783;
                    }
                    if (projectile.type == 803)
                    {
                        num916 = 862;
                    }
                    if (projectile.type == 804)
                    {
                        num916 = 863;
                    }
                    float num919 = Main.rand.NextFloat() * ((float)Math.PI * 2f);
                    for (float num920 = 0f; num920 < 1f; num920 += 1f / 6f)
                    {
                        float f5 = num919 + num920 * ((float)Math.PI * 2f);
                        Vector2 vector71 = f5.ToRotationVector2() * (4f + Main.rand.NextFloat() * 2f);
                        vector71 += Vector2.UnitY * -1f;
                        int num921 = Projectile.NewProjectile(projectile.GetSource_FromAI(), projectile.Center, vector71, num916, projectile.damage / 2, 0f, projectile.owner);
                        Projectile projectileA = Main.projectile[num921];
                        Projectile projectile2 = projectileA;
                        Projectile projectile3 = projectile2;
                        projectile3.timeLeft -= Main.rand.Next(30);
                    }
                }
                #endregion

            }
            else if (projectile.type == ProjectileID.DrySnowmanRocket)
            {
                SnowmanExplosionBase(projectile, storedPosition);

                #region vanilla
                projectile.Resize(22, 22);
                if (projectile.type == 800)
                {
                    //SoundEngine.PlaySound(in SoundID.Item62, projectile.position);
                }
                else
                {
                    //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
                }
                Color transparent6 = Color.Transparent;
                int num887 = 31;
                for (int num888 = 0; num888 < 30; num888++)
                {
                    //Dust dust316 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, transparent6, 1.5f);
                    //Dust dust58 = dust316;
                    //Dust dust334 = dust58;
                    //dust334.velocity *= 1.4f;
                }
                for (int num889 = 0; num889 < 80; num889++)
                {
                    //Dust dust318 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num887, 0f, 0f, 100, transparent6, 1.2f);
                    //Dust dust59 = dust318;
                    //Dust dust334 = dust59;
                    //dust334.velocity *= 7f;
                    //dust318 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num887, 0f, 0f, 100, transparent6, 0.3f);
                    //dust59 = dust318;
                    //dust334 = dust59;
                    //dust334.velocity *= 4f;
                }
                for (int num890 = 1; num890 <= 2; num890++)
                {
                    for (int num891 = -1; num891 <= 1; num891 += 2)
                    {
                        for (int num892 = -1; num892 <= 1; num892 += 2)
                        {
                            //Gore gore2 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, Main.rand.Next(61, 64));
                            //Gore gore16 = gore2;
                            //Gore gore64 = gore16;
                            //gore64.velocity *= ((num890 == 1) ? 0.4f : 0.8f);
                            //gore16 = gore2;
                            //gore64 = gore16;
                            //gore64.velocity += new Vector2(num891, num892);
                        }
                    }
                }
                if (Main.netMode != 1)
                {
                    Point pt5 = projectile.Center.ToTileCoordinates();
                    projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(pt5, 3.5f, DelegateMethods.SpreadDry);
                }
                #endregion
            }
            else if (projectile.type == ProjectileID.WetSnowmanRocket)
            {
                SnowmanExplosionBase(projectile, storedPosition);

                #region vanillaWet
                projectile.Resize(22, 22);
                if (projectile.type == 785)
                {
                    //SoundEngine.PlaySound(in SoundID.Item62, projectile.position);
                }
                else
                {
                    //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
                }
                Color transparent3 = Color.Transparent;
                int num867 = Dust.dustWater();
                for (int num868 = 0; num868 < 30; num868++)
                {
                    Dust dust310 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, transparent3, 1.5f);
                    Dust dust51 = dust310;
                    Dust dust334 = dust51;
                    dust334.velocity *= 1.4f;
                }
                for (int num869 = 0; num869 < 80; num869++)
                {
                    Dust dust311 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num867, 0f, 0f, 100, transparent3, 2.2f);
                    dust311.noGravity = true;
                    dust311.velocity.Y -= 1.2f;
                    Dust dust52 = dust311;
                    Dust dust334 = dust52;
                    dust334.velocity *= 7f;
                    dust311 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num867, 0f, 0f, 100, transparent3, 1.3f);
                    dust311.velocity.Y -= 1.2f;
                    dust52 = dust311;
                    dust334 = dust52;
                    dust334.velocity *= 4f;
                }
                for (int num870 = 1; num870 <= 2; num870++)
                {
                    for (int num871 = -1; num871 <= 1; num871 += 2)
                    {
                        for (int num872 = -1; num872 <= 1; num872 += 2)
                        {
                            Gore gore61 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, Main.rand.Next(61, 64));
                            Gore gore13 = gore61;
                            Gore gore64 = gore13;
                            gore64.velocity *= ((num870 == 1) ? 0.4f : 0.8f);
                            gore13 = gore61;
                            gore64 = gore13;
                            gore64.velocity += new Vector2(num871, num872);
                        }
                    }
                }
                if (Main.netMode != 1)
                {
                    Point pt2 = projectile.Center.ToTileCoordinates();
                    projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(pt2, 3f, DelegateMethods.SpreadWater);
                }
                #endregion
            }
            else if (projectile.type == ProjectileID.LavaSnowmanRocket)
            {
                SnowmanExplosionBase(projectile, storedPosition);

                #region vanillaLava
                projectile.Resize(22, 22);
                if (projectile.type == 788)
                {
                    //SoundEngine.PlaySound(in SoundID.Item62, projectile.position);
                }
                else
                {
                    //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
                }
                Color transparent4 = Color.Transparent;
                int num874 = 35;
                for (int num875 = 0; num875 < 30; num875++)
                {
                    Dust dust312 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, transparent4, 1.5f);
                    Dust dust53 = dust312;
                    Dust dust334 = dust53;
                    dust334.velocity *= 1.4f;
                }
                for (int num876 = 0; num876 < 80; num876++)
                {
                    Dust dust313 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num874, 0f, 0f, 100, transparent4, 1.2f);
                    Dust dust55 = dust313;
                    Dust dust334 = dust55;
                    dust334.velocity *= 7f;
                    dust313 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num874, 0f, 0f, 100, transparent4, 0.3f);
                    dust55 = dust313;
                    dust334 = dust55;
                    dust334.velocity *= 4f;
                }
                for (int num877 = 1; num877 <= 2; num877++)
                {
                    for (int num878 = -1; num878 <= 1; num878 += 2)
                    {
                        for (int num879 = -1; num879 <= 1; num879 += 2)
                        {
                            Gore gore62 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, Main.rand.Next(61, 64));
                            Gore gore14 = gore62;
                            Gore gore64 = gore14;
                            gore64.velocity *= ((num877 == 1) ? 0.4f : 0.8f);
                            gore14 = gore62;
                            gore64 = gore14;
                            gore64.velocity += new Vector2(num878, num879);
                        }
                    }
                }
                if (Main.netMode != 1)
                {
                    Point pt3 = projectile.Center.ToTileCoordinates();
                    projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(pt3, 3f, DelegateMethods.SpreadLava);
                }
                #endregion
            }
            else if (projectile.type == ProjectileID.HoneySnowmanRocket)
            {
                SnowmanExplosionBase(projectile, storedPosition);

                #region vanillaHoney
                projectile.Resize(22, 22);
                if (projectile.type == 791)
                {
                    //SoundEngine.PlaySound(in SoundID.Item62, projectile.position);
                }
                else
                {
                    //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
                }
                Color transparent5 = Color.Transparent;
                int num880 = 152;
                for (int num881 = 0; num881 < 30; num881++)
                {
                    Dust dust314 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, transparent5, 1.5f);
                    Dust dust56 = dust314;
                    Dust dust334 = dust56;
                    dust334.velocity *= 1.4f;
                }
                for (int num882 = 0; num882 < 80; num882++)
                {
                    Dust dust315 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num880, 0f, 0f, 100, transparent5, 2.2f);
                    Dust dust57 = dust315;
                    Dust dust334 = dust57;
                    dust334.velocity *= 7f;
                    dust315 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, num880, 0f, 0f, 100, transparent5, 1.3f);
                    dust57 = dust315;
                    dust334 = dust57;
                    dust334.velocity *= 4f;
                }
                for (int num883 = 1; num883 <= 2; num883++)
                {
                    for (int num885 = -1; num885 <= 1; num885 += 2)
                    {
                        for (int num886 = -1; num886 <= 1; num886 += 2)
                        {
                            Gore gore63 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, Main.rand.Next(61, 64));
                            Gore gore15 = gore63;
                            Gore gore64 = gore15;
                            gore64.velocity *= ((num883 == 1) ? 0.4f : 0.8f);
                            gore15 = gore63;
                            gore64 = gore15;
                            gore64.velocity += new Vector2(num885, num886);
                        }
                    }
                }
                if (Main.netMode != 1)
                {
                    Point pt4 = projectile.Center.ToTileCoordinates();
                    projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(pt4, 3f, DelegateMethods.SpreadHoney);
                }
                #endregion
            }

            SoundEngine.PlaySound(SoundID.DeerclopsIceAttack with { Volume = 0.06f, Pitch = 0.05f, PitchVariance = 0.15f, MaxInstances = -1 }, projectile.Center);

            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.5f, Pitch = -0.3f, PitchVariance = 0.1f, MaxInstances = -1 }, projectile.Center);

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_45") with { Volume = 0.55f, Pitch = -.75f, MaxInstances = -1 };
            SoundEngine.PlaySound(style3, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .23f, Pitch = .7f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            return false;
        }

        public void SnowmanExplosionBase(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.SkyBlue : Color.LightSkyBlue;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 10; i++) //16
            {
                float progress = (float)i / 10;
                Color col = Color.Lerp(Color.LightSkyBlue with { A = 0 }, Color.LightSkyBlue with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2.45f) * 2.45f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col with { A = 0 } * 0.3f, Scale: Main.rand.NextFloat(0.9f, 1.35f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.LightSkyBlue, Scale: 0.23f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 6 + Main.rand.Next(0, 6); i++) //2 //0,3
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(2.5f, 2.5f) * Main.rand.NextFloat(1f, 4f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<Snowflakes>(), vel, newColor: Color.White, Scale: Main.rand.NextFloat(0.55f, 0.8f) * 1.5f);
                dp.rotation = Main.rand.NextFloat(6.28f);

                float velFade = Main.rand.NextFloat(0.89f, 0.93f);
                SnowflakeBehavior sb = new SnowflakeBehavior(VelShrinkAmount: velFade, ScaleShrinkAmount: 0.91f, AlphaShrinkAmount: 0.92f, ColorIntensity: 1f);
                dp.customData = sb;
            }

            for (int i = 0; i < 8 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 0.85f);

                Vector2 smvel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f));

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.5f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.LightSkyBlue * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public void SnowmanExplosionRocketIII(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 4 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.SkyBlue : Color.LightSkyBlue;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 7f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.5f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 16; i++) //16
            {
                float progress = (float)i / 16;
                Color col = Color.Lerp(Color.LightSkyBlue with { A = 0 }, Color.LightSkyBlue with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.25f, 4f) * 2.5f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col with { A = 0 } * 0.3f, Scale: Main.rand.NextFloat(1.1f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.LightSkyBlue, Scale: 0.3f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 9 + Main.rand.Next(0, 6); i++) //2 //0,3
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(1.25f, 4.5f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<Snowflakes>(), vel, newColor: Color.White, Scale: Main.rand.NextFloat(0.6f, 0.8f) * 1.5f);
                dp.rotation = Main.rand.NextFloat(6.28f);

                float velFade = Main.rand.NextFloat(0.89f, 0.93f);
                SnowflakeBehavior sb = new SnowflakeBehavior(VelShrinkAmount: velFade, ScaleShrinkAmount: 0.91f, AlphaShrinkAmount: 0.92f, ColorIntensity: 1f);
                dp.customData = sb;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 0.85f);

                Vector2 smvel = Main.rand.NextVector2CircularEdge(1.75f, 1.75f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f) * 1.25f);

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.75f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.LightSkyBlue * 0.25f);
            d1.scale = 0.07f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 4f;
        }

        public void SnowmanExplosionMiniNuke(Projectile projectile, Vector2 pos)
        {
            for (int i = 0; i < 4 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.SkyBlue : Color.LightSkyBlue;
                Dust sa = Dust.NewDustPerfect(pos, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 7f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.5f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 20; i++) //16
            {
                float progress = (float)i / 20;
                Color col = Color.Lerp(Color.LightSkyBlue with { A = 0 }, Color.LightSkyBlue with { A = 0 }, progress);

                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.25f, 4f) * 2.5f;

                Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MediumSmoke>(), Velocity: vel,
                    newColor: col with { A = 0 } * 0.3f, Scale: Main.rand.NextFloat(1.1f, 1.5f));
                d.customData = new MediumSmokeBehavior(Main.rand.Next(6, 21), 0.93f, 0.01f, 1f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow2 = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.LightSkyBlue, Scale: 0.33f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 14,
                overallAlpha: 0.3f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 13 + Main.rand.Next(0, 6); i++) //2 //0,3
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3.5f, 3.5f) * Main.rand.NextFloat(1.25f, 4.5f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<Snowflakes>(), vel, newColor: Color.White, Scale: Main.rand.NextFloat(0.6f, 0.8f) * 1.5f);
                dp.rotation = Main.rand.NextFloat(6.28f);

                float velFade = Main.rand.NextFloat(0.89f, 0.93f);
                SnowflakeBehavior sb = new SnowflakeBehavior(VelShrinkAmount: velFade, ScaleShrinkAmount: 0.91f, AlphaShrinkAmount: 0.92f, ColorIntensity: 1f);
                dp.customData = sb;
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 3); i++)
            {
                Color col = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 0.85f);

                Vector2 smvel = Main.rand.NextVector2CircularEdge(1.75f, 1.75f) * Main.rand.NextFloat(1f, 2f);
                Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: col, Scale: Main.rand.NextFloat(0.6f, 0.9f) * 1.5f);

                HighResSmokeBehavior b = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1.15f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                b.isPixelated = true;
                sm.customData = b;
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.85f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.LightSkyBlue * 0.25f);
            d1.scale = 0.085f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            float distanceToPlayer = (pos - Main.player[projectile.owner].Center).Length();

            if (distanceToPlayer < 1400)
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = (1f - (distanceToPlayer / 1500f)) * 5f;
        }


        public void TileExplosion(Projectile projectile)
        {
            if (projectile.owner == Main.myPlayer)
            {
                if (projectile.type == 28 || projectile.type == 29 || projectile.type == 37 || projectile.type == 108 || projectile.type == 136 || projectile.type == 137 || projectile.type == 138 || projectile.type == 142 || projectile.type == 143 || projectile.type == 144 || projectile.type == 339 || projectile.type == 341 || projectile.type == 470 || projectile.type == 516 || projectile.type == 519 || projectile.type == 637 || projectile.type == 716 || projectile.type == 718 || projectile.type == 780 || projectile.type == 781 || projectile.type == 782 || projectile.type == 804 || projectile.type == 783 || projectile.type == 863 || projectile.type == 796 || projectile.type == 797 || projectile.type == 798 || projectile.type == 809 || (projectile.type == 102 && Main.getGoodWorld && !Main.remixWorld))
                {
                    int num16 = 3;
                    if (projectile.type == 102)
                    {
                        num16 = 4;
                    }
                    if (projectile.type == 28 || projectile.type == 37 || projectile.type == 516 || projectile.type == 519)
                    {
                        num16 = 4;
                    }
                    if (projectile.type == 29 || projectile.type == 470 || projectile.type == 637 || projectile.type == 796 || projectile.type == 797 || projectile.type == 798 || projectile.type == 809)
                    {
                        num16 = 7;
                    }
                    if (projectile.type == 142 || projectile.type == 143 || projectile.type == 144 || projectile.type == 341)
                    {
                        num16 = 5;
                    }
                    if (projectile.type == 716 || projectile.type == 780 || projectile.type == 781 || projectile.type == 782 || projectile.type == 804 || projectile.type == 783 || projectile.type == 863)
                    {
                        num16 = 3;
                    }
                    if (projectile.type == 718)
                    {
                        num16 = 5;
                    }
                    if (projectile.type == 108)
                    {
                        num16 = 10;
                    }
                    if (projectile.type == 1002)
                    {
                        num16 = 10;
                    }
                    Vector2 center3 = projectile.Center;
                    if (projectile.type == 716 || projectile.type == 718 || projectile.type == 773)
                    {
                        center3 = projectile.Center;
                    }
                    int num17 = num16;
                    int num18 = num16;
                    int num19 = (int)(center3.X / 16f - (float)num17);
                    int num20 = (int)(center3.X / 16f + (float)num17);
                    int num21 = (int)(center3.Y / 16f - (float)num18);
                    int num22 = (int)(center3.Y / 16f + (float)num18);
                    if (num19 < 0)
                    {
                        num19 = 0;
                    }
                    if (num20 > Main.maxTilesX)
                    {
                        num20 = Main.maxTilesX;
                    }
                    if (num21 < 0)
                    {
                        num21 = 0;
                    }
                    if (num22 > Main.maxTilesY)
                    {
                        num22 = Main.maxTilesY;
                    }
                    bool wallSplode2 = projectile.ShouldWallExplode(center3, num16, num19, num20, num21, num22);
                    projectile.ExplodeTiles(center3, num16, num19, num20, num21, num22, wallSplode2);
                }
                if (Main.netMode != 0)
                {
                    NetMessage.SendData(29, -1, -1, null, projectile.identity, projectile.owner);
                }
                if (!projectile.noDropItem)
                {
                    int num23 = -1;
                    if (projectile.type >= 736 && projectile.type <= 738)
                    {
                        SoundEngine.PlaySound(in SoundID.Item127, projectile.position);
                        for (int num24 = 0; num24 < 3; num24++)
                        {
                            Dust.NewDust(projectile.position, 16, 16, projectile.type - 736 + 275);
                        }
                        int num26 = (int)(projectile.Center.X / 16f);
                        int num27 = (int)(projectile.Center.Y / 16f) + 1;
                        if (Main.myPlayer == projectile.owner && Main.tile[num26, num27].HasTile && TileID.Sets.CrackedBricks[Main.tile[num26, num27].TileType] && Main.rand.Next(2) == 0)
                        {
                            WorldGen.KillTile(num26, num27);
                            if (Main.netMode != 0)
                            {
                                NetMessage.SendData(17, -1, -1, null, 20, num26, num27);
                            }
                        }
                    }
                    else if (projectile.aiStyle == 10)
                    {
                        int num28 = (int)(projectile.position.X + (float)(projectile.width / 2)) / 16;
                        int num29 = (int)(projectile.position.Y + (float)(projectile.height / 2)) / 16;
                        int num30 = 0;
                        int num31 = 2;
                        ProjectileID.Sets.FallingBlockTileItemInfo data = ProjectileID.Sets.FallingBlockTileItem[projectile.type];
                        if (data != null)
                        {
                            num30 = data.TileType;
                            num31 = data.ItemType;
                        }
                        if (projectile.type == 31 && projectile.ai[0] == 2f)
                        {
                            num31 = 0;
                        }
                        if (projectile.type == 109)
                        {
                            int num32 = Player.FindClosest(projectile.position, projectile.width, projectile.height);
                            if ((double)(projectile.Center - Main.player[num32].Center).Length() > (double)Main.LogicCheckScreenWidth * 0.75)
                            {
                                num30 = -1;
                                num31 = 593;
                            }
                        }
                        if (Main.tile[num28, num29].HasTile && Main.tile[num28, num29].IsHalfBlock && projectile.velocity.Y > 0f && Math.Abs(projectile.velocity.Y) > Math.Abs(projectile.velocity.X))
                        {
                            num29--;
                        }
                        if (!Main.tile[num28, num29].HasTile && num30 >= 0)
                        {
                            bool flag5 = false;
                            bool flag6 = false;
                            if (num29 < Main.maxTilesY - 2)
                            {
                                Tile tile2 = Main.tile[num28, num29 + 1];
                                if (tile2 != null && tile2.HasTile)
                                {
                                    if (tile2.HasTile && tile2.TileType == 314)
                                    {
                                        flag6 = true;
                                    }
                                    if (tile2.HasTile && WorldGen.BlockBelowMakesSandFall(num28, num29))
                                    {
                                        flag6 = true;
                                    }
                                }
                            }
                            if (!flag6)
                            {
                                flag5 = WorldGen.PlaceTile(num28, num29, num30, mute: false, forced: true);
                            }
                            if (!flag6 && Main.tile[num28, num29].HasTile && Main.tile[num28, num29].TileType == num30)
                            {
                                if (Main.tile[num28, num29 + 1].IsHalfBlock || Main.tile[num28, num29 + 1].Slope != 0)
                                {
                                    WorldGen.SlopeTile(num28, num29 + 1);
                                    if (Main.netMode != 0)
                                    {
                                        NetMessage.SendData(17, -1, -1, null, 14, num28, num29 + 1);
                                    }
                                }
                                if (Main.netMode != 0)
                                {
                                    NetMessage.SendData(17, -1, -1, null, 1, num28, num29, num30);
                                }
                            }
                            else if (!flag5 && num31 > 0)
                            {
                                num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, num31);
                            }
                        }
                        else if (num31 > 0)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, num31);
                        }
                    }
                    if (projectile.type == 171)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 985);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 965, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 475)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3005);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 2996, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 505)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3079);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3077, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 506)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3080);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3078, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 12 && projectile.damage > 500 && !Main.remixWorld)
                    {
                        num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 75);
                    }
                    if (projectile.type == 155)
                    {
                        num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 859);
                    }
                    if (projectile.type == 861)
                    {
                        num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 4743);
                    }
                    if (Main.netMode == 1 && num23 >= 0)
                    {
                        NetMessage.SendData(21, -1, -1, null, num23, 1f);
                    }
                }


            }
        }


    }

    public class SnowmanClusterShardsOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ClusterSnowmanFragmentsI || entity.type == ProjectileID.ClusterSnowmanFragmentsII);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 12f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

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

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.3f,
                    previousRotations[i], TexOrigin, size * overallScale, SpriteEffects.None);

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * 0.75f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.SkyBlue * 0.85f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.DeepSkyBlue with { A = 0 } * 0.5f, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1.5f, 3f);

                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: Color.SkyBlue, Scale: Main.rand.NextFloat(0.25f, 0.35f));
                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.92f, shouldFadeColor: false);
            }

            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++) //2 //0,3
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) * Main.rand.NextFloat(1f, 2f);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<Snowflakes>(), vel, newColor: Color.White, Scale: Main.rand.NextFloat(0.35f, 0.55f));
                dp.rotation = Main.rand.NextFloat(6.28f);

                float velFade = Main.rand.NextFloat(0.89f, 0.93f);
                SnowflakeBehavior sb = new SnowflakeBehavior(VelShrinkAmount: velFade, ScaleShrinkAmount: 0.91f, AlphaShrinkAmount: 0.92f, ColorIntensity: 1f);
                dp.customData = sb;
            }

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .10f, Pitch = .55f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            #region tileExplosion
            if (projectile.owner == Main.myPlayer)
            {
                if (projectile.type == 28 || projectile.type == 29 || projectile.type == 37 || projectile.type == 108 || projectile.type == 136 || projectile.type == 137 || projectile.type == 138 || projectile.type == 142 || projectile.type == 143 || projectile.type == 144 || projectile.type == 339 || projectile.type == 341 || projectile.type == 470 || projectile.type == 516 || projectile.type == 519 || projectile.type == 637 || projectile.type == 716 || projectile.type == 718 || projectile.type == 780 || projectile.type == 781 || projectile.type == 782 || projectile.type == 804 || projectile.type == 783 || projectile.type == 863 || projectile.type == 796 || projectile.type == 797 || projectile.type == 798 || projectile.type == 809 || (projectile.type == 102 && Main.getGoodWorld && !Main.remixWorld))
                {
                    int num16 = 3;
                    if (projectile.type == 102)
                    {
                        num16 = 4;
                    }
                    if (projectile.type == 28 || projectile.type == 37 || projectile.type == 516 || projectile.type == 519)
                    {
                        num16 = 4;
                    }
                    if (projectile.type == 29 || projectile.type == 470 || projectile.type == 637 || projectile.type == 796 || projectile.type == 797 || projectile.type == 798 || projectile.type == 809)
                    {
                        num16 = 7;
                    }
                    if (projectile.type == 142 || projectile.type == 143 || projectile.type == 144 || projectile.type == 341)
                    {
                        num16 = 5;
                    }
                    if (projectile.type == 716 || projectile.type == 780 || projectile.type == 781 || projectile.type == 782 || projectile.type == 804 || projectile.type == 783 || projectile.type == 863)
                    {
                        num16 = 3;
                    }
                    if (projectile.type == 718)
                    {
                        num16 = 5;
                    }
                    if (projectile.type == 108)
                    {
                        num16 = 10;
                    }
                    if (projectile.type == 1002)
                    {
                        num16 = 10;
                    }
                    Vector2 center3 = projectile.Center;
                    if (projectile.type == 716 || projectile.type == 718 || projectile.type == 773)
                    {
                        center3 = projectile.Center;
                    }
                    int num17 = num16;
                    int num18 = num16;
                    int num19 = (int)(center3.X / 16f - (float)num17);
                    int num20 = (int)(center3.X / 16f + (float)num17);
                    int num21 = (int)(center3.Y / 16f - (float)num18);
                    int num22 = (int)(center3.Y / 16f + (float)num18);
                    if (num19 < 0)
                    {
                        num19 = 0;
                    }
                    if (num20 > Main.maxTilesX)
                    {
                        num20 = Main.maxTilesX;
                    }
                    if (num21 < 0)
                    {
                        num21 = 0;
                    }
                    if (num22 > Main.maxTilesY)
                    {
                        num22 = Main.maxTilesY;
                    }
                    bool wallSplode2 = projectile.ShouldWallExplode(center3, num16, num19, num20, num21, num22);
                    projectile.ExplodeTiles(center3, num16, num19, num20, num21, num22, wallSplode2);
                }
                if (Main.netMode != 0)
                {
                    NetMessage.SendData(29, -1, -1, null, projectile.identity, projectile.owner);
                }
                if (!projectile.noDropItem)
                {
                    int num23 = -1;
                    if (projectile.type >= 736 && projectile.type <= 738)
                    {
                        SoundEngine.PlaySound(in SoundID.Item127, projectile.position);
                        for (int num24 = 0; num24 < 3; num24++)
                        {
                            Dust.NewDust(projectile.position, 16, 16, projectile.type - 736 + 275);
                        }
                        int num26 = (int)(projectile.Center.X / 16f);
                        int num27 = (int)(projectile.Center.Y / 16f) + 1;
                        if (Main.myPlayer == projectile.owner && Main.tile[num26, num27].HasTile && TileID.Sets.CrackedBricks[Main.tile[num26, num27].TileType] && Main.rand.Next(2) == 0)
                        {
                            WorldGen.KillTile(num26, num27);
                            if (Main.netMode != 0)
                            {
                                NetMessage.SendData(17, -1, -1, null, 20, num26, num27);
                            }
                        }
                    }
                    else if (projectile.aiStyle == 10)
                    {
                        int num28 = (int)(projectile.position.X + (float)(projectile.width / 2)) / 16;
                        int num29 = (int)(projectile.position.Y + (float)(projectile.height / 2)) / 16;
                        int num30 = 0;
                        int num31 = 2;
                        ProjectileID.Sets.FallingBlockTileItemInfo data = ProjectileID.Sets.FallingBlockTileItem[projectile.type];
                        if (data != null)
                        {
                            num30 = data.TileType;
                            num31 = data.ItemType;
                        }
                        if (projectile.type == 31 && projectile.ai[0] == 2f)
                        {
                            num31 = 0;
                        }
                        if (projectile.type == 109)
                        {
                            int num32 = Player.FindClosest(projectile.position, projectile.width, projectile.height);
                            if ((double)(projectile.Center - Main.player[num32].Center).Length() > (double)Main.LogicCheckScreenWidth * 0.75)
                            {
                                num30 = -1;
                                num31 = 593;
                            }
                        }
                        if (Main.tile[num28, num29].HasTile && Main.tile[num28, num29].IsHalfBlock && projectile.velocity.Y > 0f && Math.Abs(projectile.velocity.Y) > Math.Abs(projectile.velocity.X))
                        {
                            num29--;
                        }
                        if (!Main.tile[num28, num29].HasTile && num30 >= 0)
                        {
                            bool flag5 = false;
                            bool flag6 = false;
                            if (num29 < Main.maxTilesY - 2)
                            {
                                Tile tile2 = Main.tile[num28, num29 + 1];
                                if (tile2 != null && tile2.HasTile)
                                {
                                    if (tile2.HasTile && tile2.TileType == 314)
                                    {
                                        flag6 = true;
                                    }
                                    if (tile2.HasTile && WorldGen.BlockBelowMakesSandFall(num28, num29))
                                    {
                                        flag6 = true;
                                    }
                                }
                            }
                            if (!flag6)
                            {
                                flag5 = WorldGen.PlaceTile(num28, num29, num30, mute: false, forced: true);
                            }
                            if (!flag6 && Main.tile[num28, num29].HasTile && Main.tile[num28, num29].TileType == num30)
                            {
                                if (Main.tile[num28, num29 + 1].IsHalfBlock || Main.tile[num28, num29 + 1].Slope != 0)
                                {
                                    WorldGen.SlopeTile(num28, num29 + 1);
                                    if (Main.netMode != 0)
                                    {
                                        NetMessage.SendData(17, -1, -1, null, 14, num28, num29 + 1);
                                    }
                                }
                                if (Main.netMode != 0)
                                {
                                    NetMessage.SendData(17, -1, -1, null, 1, num28, num29, num30);
                                }
                            }
                            else if (!flag5 && num31 > 0)
                            {
                                num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, num31);
                            }
                        }
                        else if (num31 > 0)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, num31);
                        }
                    }
                    if (projectile.type == 171)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 985);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 965, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 475)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3005);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 2996, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 505)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3079);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3077, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 506)
                    {
                        if (projectile.ai[1] == 0f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3080);
                            Main.item[num23].noGrabDelay = 0;
                        }
                        else if (projectile.ai[1] < 10f)
                        {
                            num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 3078, (int)(10f - projectile.ai[1]));
                            Main.item[num23].noGrabDelay = 0;
                        }
                    }
                    if (projectile.type == 12 && projectile.damage > 500 && !Main.remixWorld)
                    {
                        num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 75);
                    }
                    if (projectile.type == 155)
                    {
                        num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 859);
                    }
                    if (projectile.type == 861)
                    {
                        num23 = Item.NewItem(projectile.GetSource_FromThis(), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, 4743);
                    }
                    if (Main.netMode == 1 && num23 >= 0)
                    {
                        NetMessage.SendData(21, -1, -1, null, num23, 1f);
                    }
                }


            }
            #endregion

            #region vanillaKill
            projectile.Resize(22, 22);
            //SoundEngine.PlaySound(in SoundID.Item62, projectile.position);
            Color transparent7 = Color.Transparent;
            for (int num904 = 1110; num904 < 15; num904++)
            {
                Dust dust322 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 31, 0f, 0f, 100, transparent7, 0.8f);
                dust322.fadeIn = 0f;
                Dust dust64 = dust322;
                Dust dust334 = dust64;
                dust334.velocity *= 0.5f;
            }
            for (int num905 = 1110; num905 < 5; num905++)
            {
                Dust dust323 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 228, 0f, 0f, 100, transparent7, 2.5f);
                dust323.noGravity = true;
                Dust dust65 = dust323;
                Dust dust334 = dust65;
                dust334.velocity *= 2.5f;
                dust323 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 228, 0f, 0f, 100, transparent7, 1.1f);
                dust65 = dust323;
                dust334 = dust65;
                dust334.velocity *= 2f;
                dust323.noGravity = true;
            }
            for (int num907 = 1110; num907 < 3; num907++)
            {
                Dust dust324 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 226, 0f, 0f, 100, transparent7, 1.1f);
                Dust dust67 = dust324;
                Dust dust334 = dust67;
                dust334.velocity *= 2f;
                dust324.noGravity = true;
            }
            for (int num908 = -1 + 1111; num908 <= 1; num908 += 2)
            {
                for (int num909 = -1; num909 <= 1; num909 += 2)
                {
                    if (Main.rand.Next(5) == 0)
                    {
                        Gore gore4 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, Main.rand.Next(61, 64));
                        Gore gore18 = gore4;
                        Gore gore64 = gore18;
                        gore64.velocity *= 0.2f;
                        gore18 = gore4;
                        gore64 = gore18;
                        gore64.scale *= 0.65f;
                        gore18 = gore4;
                        gore64 = gore18;
                        gore64.velocity += new Vector2(num908, num909) * 0.5f;
                    }
                }
            }
            #endregion
            return false;
        }
    }

}
