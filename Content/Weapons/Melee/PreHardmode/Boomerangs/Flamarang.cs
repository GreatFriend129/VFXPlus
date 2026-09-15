using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Particles;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Boomerangs
{
    public class FlamarangProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Flamarang);
        }

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 6; //10
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (timer % 1 == 0 && timer > 3)
            {
                Color fireRed = Color.Lerp(Color.OrangeRed, Color.Red, 0.05f);//0.15
                //fireRed = Color.Lerp(Color.OrangeRed, Color.Orange, 0f);

                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f);
                    FireParticleAlpha fire = new FireParticleAlpha(projectile.Center + projectile.velocity, vel, 1f, fireRed, colorMult: 1.5f, bloomAlpha: 3f, AlphaFade: 0.9f);
                    fire.bloomColor = fireRed with { A = 150 };
                    fire.scaleFadePower = 1.08f;
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }

            }

            if (timer % 1 == 0 && timer > 3 && false)
            {
                Color fireRed = Color.Lerp(Color.OrangeRed, Color.Red, 0.05f);//0.15
                //fireRed = Color.Lerp(Color.OrangeRed, Color.Orange, 0f);

                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f);
                FireParticle fire = new FireParticle(projectile.Center + new Vector2(0f, -100), vel * 1.25f, 1.15f, fireRed, colorMult: 1.5f, bloomAlpha: 1f, AlphaFade: 0.9f);
                fire.scaleFadePower = 1.08f;
                fire.renderLayer = RenderLayer.UnderProjectiles;
                ShaderParticleHandler.SpawnParticle(fire);
            }

            if (timer % 1 == 0 && timer > 3 && false)
            {
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f);
                FireParticleAlpha fire = new FireParticleAlpha(projectile.Center + new Vector2(0f, -100f), vel, 1.15f, Color.Lerp(Color.DodgerBlue, Color.Blue, 0.15f), colorMult: 1f, bloomAlpha: 1f, AlphaFade: 0.9f);
                fire.scaleFadePower = 1.08f;
                fire.renderLayer = RenderLayer.Dusts;
                ShaderParticleHandler.SpawnParticle(fire);
            }


            float fadeInTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            overallScale = Easings.easeInOutHarsh(fadeInTime);


            //float fadeInAlphaTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            //overallAlpha = Easings.easeInOutHarsh(fadeInTime);

            //visualRotation = projectile.rotation;
            visualRotation += 0.45f * projectile.direction * overallScale;

            timer++;

            #region vanillaAI
            if (projectile.soundDelay == 0 && projectile.type != ProjectileID.Anchor)
            {
                projectile.soundDelay = 8;
                SoundEngine.PlaySound(in SoundID.Item7, projectile.position);
            }
            if (projectile.type == ProjectileID.Flamarang && false)
            {
                for (int num300 = 0; num300 < 2; num300++)
                {
                    int num311 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Torch, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 100, default(Color), 2f);
                    Main.dust[num311].noGravity = true;
                    Main.dust[num311].velocity.X *= 0.3f;
                    Main.dust[num311].velocity.Y *= 0.3f;
                }
            }
            else if (projectile.type == ProjectileID.Trimarang)
            {
                if (Main.rand.Next(3) == 0)
                {
                    switch (Main.rand.Next(3))
                    {
                        default:
                            {
                                int num1076 = Main.rand.Next(3);
                                Dust dust170 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, num1076 switch
                                {
                                    1 => 57,
                                    2 => 58,
                                    _ => 15,
                                }, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f)];
                                Dust dust27 = dust170;
                                Dust dust212 = dust27;
                                dust212.velocity *= 0.5f;
                                break;
                            }
                        case 1:
                            {
                                Dust dust172 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.RainbowMk2, projectile.velocity.X, projectile.velocity.Y, 50, new Color(50, 50, 200), 1.1f)];
                                dust172.fadeIn = 0.1f;
                                dust172.velocity = projectile.velocity * 0.5f;
                                dust172.noGravity = true;
                                break;
                            }
                        case 2:
                            {
                                Dust dust171 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Snow, projectile.velocity.X * 0.15f, projectile.velocity.Y * 0.15f, 0, default(Color), 1.1f)];
                                dust171.noGravity = true;
                                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.MagicMirror, projectile.velocity.X * 0.05f, projectile.velocity.Y * 0.05f, 150, default(Color), 0.6f);
                                break;
                            }
                    }
                }
            }
            else if (projectile.type == ProjectileID.Shroomerang)
            {
                if (Main.rand.Next(3) == 0)
                {
                    int num332 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.FungiHit, projectile.velocity.X, projectile.velocity.Y, 50);
                    Dust dust28 = Main.dust[num332];
                    Dust dust212 = dust28;
                    dust212.velocity *= 0.5f;
                    Main.dust[num332].noGravity = true;
                }
            }
            else if (projectile.type == ProjectileID.ThornChakram)
            {
                if (Main.rand.Next(1) == 0)
                {
                    int num343 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.JunglePlants, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 0, default(Color), 1.4f);
                    Main.dust[num343].noGravity = true;
                }
            }
            else if (projectile.type == ProjectileID.BloodyMachete)
            {
                if (Main.rand.Next(3) == 0)
                {
                    int num354 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Blood, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 0, default(Color), 1.1f);
                    if (Main.rand.Next(2) == 0)
                    {
                        Main.dust[num354].scale = 0.9f;
                        Dust dust29 = Main.dust[num354];
                        Dust dust212 = dust29;
                        dust212.velocity *= 0.2f;
                    }
                    else
                    {
                        Main.dust[num354].noGravity = true;
                    }
                }
            }
            else if (projectile.type == ProjectileID.EnchantedBoomerang)
            {
                if (Main.rand.Next(5) == 0)
                {
                    int num1077 = Main.rand.Next(3);
                    Dust.NewDust(projectile.position, projectile.width, projectile.height, num1077 switch
                    {
                        0 => 15,
                        1 => 57,
                        _ => 58,
                    }, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 150, default(Color), 0.7f);
                }
            }
            else if (projectile.type == ProjectileID.IceBoomerang && Main.rand.Next(1) == 0)
            {
                int num375 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Snow, projectile.velocity.X * 0.15f, projectile.velocity.Y * 0.15f, 0, default(Color), 1.1f);
                Main.dust[num375].noGravity = true;
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.MagicMirror, projectile.velocity.X * 0.05f, projectile.velocity.Y * 0.05f, 150, default(Color), 0.6f);
            }
            if (projectile.ai[0] == 0f)
            {
                bool flag = true;
                int num386 = projectile.type;
                if (num386 == 866)
                {
                    flag = false;
                }
                if (flag)
                {
                    projectile.ai[1] += 1f;
                }
                if (projectile.type == ProjectileID.LightDisc && projectile.ai[1] >= 45f)
                {
                    projectile.ai[0] = 1f;
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }
                if (projectile.type == ProjectileID.BloodyMachete || projectile.type == ProjectileID.Anchor)
                {
                    if (projectile.ai[1] >= 10f)
                    {
                        projectile.velocity.Y += 0.5f;
                        if (projectile.type == ProjectileID.Anchor && projectile.velocity.Y < 0f)
                        {
                            projectile.velocity.Y += 0.35f;
                        }
                        projectile.velocity.X *= 0.95f;
                        if (projectile.velocity.Y > 16f)
                        {
                            projectile.velocity.Y = 16f;
                        }
                        if (projectile.type == ProjectileID.Anchor && Vector2.Distance(projectile.Center, Main.player[projectile.owner].Center) > 800f)
                        {
                            projectile.ai[0] = 1f;
                            projectile.netUpdate = true;
                        }
                    }
                }
                else if (projectile.type == ProjectileID.PossessedHatchet)
                {
                    if (Main.rand.Next(2) == 0)
                    {
                        int num399 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Enchanted_Gold, 0f, 0f, 255, default(Color), 0.75f);
                        Dust dust30 = Main.dust[num399];
                        Dust dust212 = dust30;
                        dust212.velocity *= 0.1f;
                        Main.dust[num399].noGravity = true;
                    }
                    if (projectile.velocity.X > 0f)
                    {
                        projectile.spriteDirection = 1;
                    }
                    else if (projectile.velocity.X < 0f)
                    {
                        projectile.spriteDirection = -1;
                    }
                    float num411 = projectile.position.X;
                    float num422 = projectile.position.Y;
                    float num433 = 800f;
                    bool flag12 = false;
                    if (projectile.ai[1] > 10f && projectile.ai[1] < 360f)
                    {
                        for (int num444 = 0; num444 < 200; num444++)
                        {
                            if (Main.npc[num444].CanBeChasedBy(projectile))
                            {
                                float num455 = Main.npc[num444].position.X + (float)(Main.npc[num444].width / 2);
                                float num466 = Main.npc[num444].position.Y + (float)(Main.npc[num444].height / 2);
                                float num477 = projectile.Distance(Main.npc[num444].Center);
                                if (num477 < num433 && Collision.CanHit(new Vector2(projectile.position.X + (float)(projectile.width / 2), projectile.position.Y + (float)(projectile.height / 2)), 1, 1, Main.npc[num444].position, Main.npc[num444].width, Main.npc[num444].height))
                                {
                                    num433 = num477;
                                    num411 = num455;
                                    num422 = num466;
                                    flag12 = true;
                                }
                            }
                        }
                    }
                    if (!flag12)
                    {
                        num411 = projectile.position.X + (float)(projectile.width / 2) + projectile.velocity.X * 100f;
                        num422 = projectile.position.Y + (float)(projectile.height / 2) + projectile.velocity.Y * 100f;
                        if (projectile.ai[1] >= 30f)
                        {
                            projectile.ai[0] = 1f;
                            projectile.ai[1] = 0f;
                            projectile.netUpdate = true;
                        }
                    }
                    float num488 = 12f;
                    float num499 = 0.25f;
                    Vector2 vector101 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                    float num510 = num411 - vector101.X;
                    float num522 = num422 - vector101.Y;
                    float num533 = (float)Math.Sqrt(num510 * num510 + num522 * num522);
                    float num544 = num533;
                    num533 = num488 / num533;
                    num510 *= num533;
                    num522 *= num533;
                    if (projectile.velocity.X < num510)
                    {
                        projectile.velocity.X += num499;
                        if (projectile.velocity.X < 0f && num510 > 0f)
                        {
                            projectile.velocity.X += num499 * 2f;
                        }
                    }
                    else if (projectile.velocity.X > num510)
                    {
                        projectile.velocity.X -= num499;
                        if (projectile.velocity.X > 0f && num510 < 0f)
                        {
                            projectile.velocity.X -= num499 * 2f;
                        }
                    }
                    if (projectile.velocity.Y < num522)
                    {
                        projectile.velocity.Y += num499;
                        if (projectile.velocity.Y < 0f && num522 > 0f)
                        {
                            projectile.velocity.Y += num499 * 2f;
                        }
                    }
                    else if (projectile.velocity.Y > num522)
                    {
                        projectile.velocity.Y -= num499;
                        if (projectile.velocity.Y > 0f && num522 < 0f)
                        {
                            projectile.velocity.Y -= num499 * 2f;
                        }
                    }
                }
                else if (projectile.type == ProjectileID.BouncingShield)
                {
                    if (projectile.owner == Main.myPlayer && projectile.damage > 0)
                    {
                        float num555 = projectile.ai[1];
                        if (projectile.localAI[0] >= 10f && projectile.localAI[0] <= 360f)
                        {
                            int num566 = projectile.FindTargetWithLineOfSight();
                            projectile.ai[1] = num566;
                        }
                        else
                        {
                            projectile.ai[1] = -1f;
                        }
                        if (projectile.ai[1] != num555)
                        {
                            projectile.netUpdate = true;
                        }
                    }
                    projectile.localAI[0] += 1f;
                    int num577 = (int)projectile.ai[1];
                    Vector2 vector112;
                    if (Main.npc.IndexInRange(num577) && Main.npc[num577].CanBeChasedBy(projectile))
                    {
                        vector112 = Main.npc[num577].Center;
                    }
                    else
                    {
                        vector112 = projectile.Center + projectile.velocity * 100f;
                        int num588 = 30;
                        if (projectile.owner != Main.myPlayer)
                        {
                            num588 = 60;
                        }
                        if (projectile.localAI[0] >= (float)num588)
                        {
                            projectile.ai[0] = 1f;
                            projectile.ai[1] = 0f;
                            projectile.netUpdate = true;
                        }
                    }
                    float num599 = 12f;
                    float num610 = 0.25f;
                    Vector2 vector123 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                    float num621 = vector112.X - vector123.X;
                    float num633 = vector112.Y - vector123.Y;
                    float num644 = (float)Math.Sqrt(num621 * num621 + num633 * num633);
                    float num655 = num644;
                    num644 = num599 / num644;
                    num621 *= num644;
                    num633 *= num644;
                    if (projectile.velocity.X < num621)
                    {
                        projectile.velocity.X += num610;
                        if (projectile.velocity.X < 0f && num621 > 0f)
                        {
                            projectile.velocity.X += num610 * 2f;
                        }
                    }
                    else if (projectile.velocity.X > num621)
                    {
                        projectile.velocity.X -= num610;
                        if (projectile.velocity.X > 0f && num621 < 0f)
                        {
                            projectile.velocity.X -= num610 * 2f;
                        }
                    }
                    if (projectile.velocity.Y < num633)
                    {
                        projectile.velocity.Y += num610;
                        if (projectile.velocity.Y < 0f && num633 > 0f)
                        {
                            projectile.velocity.Y += num610 * 2f;
                        }
                    }
                    else if (projectile.velocity.Y > num633)
                    {
                        projectile.velocity.Y -= num610;
                        if (projectile.velocity.Y > 0f && num633 < 0f)
                        {
                            projectile.velocity.Y -= num610 * 2f;
                        }
                    }
                }
                else if (projectile.type == ProjectileID.PaladinsHammerFriendly)
                {
                    if (projectile.ai[1] >= 20f)
                    {
                        projectile.ai[0] = 1f;
                        projectile.ai[1] = 0f;
                        projectile.velocity = Vector2.Zero;
                        projectile.netUpdate = true;
                    }
                }
                else if (projectile.ai[1] >= 30f)
                {
                    projectile.ai[0] = 1f;
                    projectile.ai[1] = 0f;
                    projectile.netUpdate = true;
                }
            }
            else
            {
                projectile.tileCollide = false;
                float num666 = 9f;
                float num677 = 0.4f;
                if (projectile.type == ProjectileID.Trimarang)
                {
                    num666 = 9.5f;
                }
                if (projectile.type == ProjectileID.Flamarang)
                {
                    num666 = 20f;
                    num677 = 1.5f;
                }
                else if (projectile.type == ProjectileID.ThornChakram)
                {
                    num666 = 18f;
                    num677 = 1.2f;
                }
                else if (projectile.type == ProjectileID.PossessedHatchet)
                {
                    num666 = 16f;
                    num677 = 1.2f;
                }
                else if (projectile.type == ProjectileID.BouncingShield)
                {
                    num666 = 16f;
                    num677 = 1.2f;
                }
                else if (projectile.type == ProjectileID.LightDisc)
                {
                    num666 = 16f;
                    num677 = 1.2f;
                }
                else if (projectile.type == ProjectileID.Bananarang)
                {
                    num666 = 20f;
                    num677 = 1.5f;
                }
                else if (projectile.type == ProjectileID.FruitcakeChakram)
                {
                    num666 = 12f;
                    num677 = 0.6f;
                }
                else if (projectile.type == ProjectileID.PaladinsHammerFriendly)
                {
                    num666 = 15f;
                    num677 = 3f;
                }
                else if (projectile.type == ProjectileID.BloodyMachete)
                {
                    num666 = 15f;
                    num677 = 3f;
                }
                else if (projectile.type == ProjectileID.Anchor)
                {
                    num666 = 16f;
                    num677 = 4f;
                }
                Vector2 vector134 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                float num688 = Main.player[projectile.owner].position.X + (float)(Main.player[projectile.owner].width / 2) - vector134.X;
                float num699 = Main.player[projectile.owner].position.Y + (float)(Main.player[projectile.owner].height / 2) - vector134.Y;
                float num710 = (float)Math.Sqrt(num688 * num688 + num699 * num699);
                if (num710 > 3000f)
                {
                    projectile.Kill();
                }
                num710 = num666 / num710;
                num688 *= num710;
                num699 *= num710;
                if (projectile.type == ProjectileID.Anchor)
                {
                    Vector2 vector145 = new Vector2(num688, num699) - projectile.velocity;
                    if (vector145 != Vector2.Zero)
                    {
                        Vector2 vector156 = vector145;
                        vector156.Normalize();
                        projectile.velocity += vector156 * Math.Min(num677, vector145.Length());
                    }
                }
                else
                {
                    if (projectile.velocity.X < num688)
                    {
                        projectile.velocity.X += num677;
                        if (projectile.velocity.X < 0f && num688 > 0f)
                        {
                            projectile.velocity.X += num677;
                        }
                    }
                    else if (projectile.velocity.X > num688)
                    {
                        projectile.velocity.X -= num677;
                        if (projectile.velocity.X > 0f && num688 < 0f)
                        {
                            projectile.velocity.X -= num677;
                        }
                    }
                    if (projectile.velocity.Y < num699)
                    {
                        projectile.velocity.Y += num677;
                        if (projectile.velocity.Y < 0f && num699 > 0f)
                        {
                            projectile.velocity.Y += num677;
                        }
                    }
                    else if (projectile.velocity.Y > num699)
                    {
                        projectile.velocity.Y -= num677;
                        if (projectile.velocity.Y > 0f && num699 < 0f)
                        {
                            projectile.velocity.Y -= num677;
                        }
                    }
                }
                if (Main.myPlayer == projectile.owner)
                {
                    Rectangle rectangle = new Rectangle((int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height);
                    Rectangle value = new Rectangle((int)Main.player[projectile.owner].position.X, (int)Main.player[projectile.owner].position.Y, Main.player[projectile.owner].width, Main.player[projectile.owner].height);
                    if (rectangle.Intersects(value))
                    {
                        projectile.Kill();
                    }
                }
            }
            if (projectile.type == ProjectileID.LightDisc)
            {
                projectile.rotation += 0.3f * (float)projectile.direction;
            }
            else if (projectile.type == ProjectileID.BouncingShield)
            {
                projectile.rotation = projectile.velocity.ToRotation();
                if (Main.rand.Next(2) == 0)
                {
                    int num721 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.BubbleBurst_White);
                    Dust dust32 = Main.dust[num721];
                    Dust dust212 = dust32;
                    dust212.velocity *= 0.1f;
                    Main.dust[num721].noGravity = true;
                }
            }
            else if (projectile.type == ProjectileID.Anchor)
            {
                if (projectile.ai[0] == 0f)
                {
                    Vector2 v = projectile.velocity;
                    v = v.SafeNormalize(Vector2.Zero);
                    projectile.rotation = (float)Math.Atan2(v.Y, v.X) + 1.57f;
                }
                else
                {
                    Vector2 v2 = projectile.Center - Main.player[projectile.owner].Center;
                    v2 = v2.SafeNormalize(Vector2.Zero);
                    projectile.rotation = (float)Math.Atan2(v2.Y, v2.X) + 1.57f;
                }
            }
            else if (projectile.type == ProjectileID.PaladinsHammerFriendly)
            {
                if (projectile.ai[0] == 0f)
                {
                    projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 4f;
                    if (Main.rand.Next(2) == 0)
                    {
                        int num732 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Enchanted_Gold, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 200, default(Color), 1.2f);
                        Dust dust36 = Main.dust[num732];
                        Dust dust212 = dust36;
                        dust212.velocity += projectile.velocity * 0.3f;
                        dust36 = Main.dust[num732];
                        dust212 = dust36;
                        dust212.velocity *= 0.2f;
                        Main.dust[num732].noGravity = true;
                    }
                    if (Main.rand.Next(3) == 0)
                    {
                        int num744 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.TintableDustLighted, 0f, 0f, 254, default(Color), 0.3f);
                        Dust dust37 = Main.dust[num744];
                        Dust dust212 = dust37;
                        dust212.velocity += projectile.velocity * 0.5f;
                        dust37 = Main.dust[num744];
                        dust212 = dust37;
                        dust212.velocity *= 0.5f;
                        Main.dust[num744].noGravity = true;
                    }
                }
                else
                {
                    projectile.rotation += 0.4f * (float)projectile.direction;
                }
            }
            else
            {
                projectile.rotation += 0.4f * (float)projectile.direction;
            }
            #endregion

            return false;
        }

        float visualRotation = 0f;
        float overallAlpha = 1f;
        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawSpinningUnder(projectile, false); 
            });
            DrawSpinningUnder(projectile, true);


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            for (int i = 220; i < 10; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null, Color.OrangeRed with { A = 0 }, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);
            }


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);

            return false;

        }

        public void DrawSpinningUnder(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.OrangeRed with { A = 150 } * 0.25f, 0f, Orb.Size() / 2f, 0.5f * projectile.scale * overallScale, SpriteEffects.None);


            Texture2D trailTex = CommonTextures.Flare.Value;
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 spikeScale = new Vector2(0.5f, 1f * progress);

                //float xScale = 0.15f + (0.45f * Utils.GetLerpValue(2f, 5f, projectile.velocity.Length(), true));
                //Vector2 vec2Scale = new Vector2(xScale * 1.5f, 1f * Easings.easeInSine(progress)) * projectile.scale * 0.75f;

                Color spikeCol = Color.Lerp(Color.Orange, Color.OrangeRed, 0.65f);

                Main.EntitySpriteDraw(trailTex, previousPositions[i] - Main.screenPosition, null, spikeCol with { A = 100 } * 1f * overallAlpha,
                        previousRotations[i], trailTex.Size() / 2f, projectile.scale * overallScale * spikeScale, SpriteEffects.None);
            }


            //Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/twirl_03").Value; //twirl_03 0.6f //A |0.75f time|
            //float ringRot = (float)Main.timeForVisualEffects * 0.6f * projectile.direction;
            //float ringScale = 0.08f * projectile.scale * overallScale; //125

            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingA").Value; //twirl_03 0.6f //A |0.75f time|

            float ringRot = (float)Main.timeForVisualEffects * 0.6f * projectile.direction;

            float ringScale = 0.09f * projectile.scale * overallScale; //125
            float ringAlpha = overallAlpha * 1f;


            Color colA = Color.Lerp(Color.OrangeRed, Color.Red, 0f);
            Color colB = Color.Lerp(Color.OrangeRed, Color.Orange, 1f);

            //Main.EntitySpriteDraw(RingTex, drawPos, null, colB with { A = 150 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
            //M/ain.EntitySpriteDraw(RingTex, drawPos, null, colA with { A = 150 } * ringAlpha, ringRot + MathHelper.PiOver4, RingTex.Size() / 2f, ringScale * 1.1f, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
