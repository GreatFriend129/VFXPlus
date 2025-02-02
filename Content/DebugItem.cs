using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Microsoft.CodeAnalysis;
using Terraria.GameContent.Drawing;
using VFXPlus.Content.VFXTest;
using VFXPlus.Content.Weapons.Magic.Hardmode.Staves;
using VFXPlus.Content.Dusts;
using System.Runtime.Intrinsics.Arm;
using System.Linq;
using VFXPlus.Content.FeatheredFoe;
using VFXPlus.Content.Weapons.Magic.Hardmode.Misc;
using Microsoft.Build.Evaluation;
using VFXPlus.Common.Utilities;
using log4net.Core;
using System.Threading;
using Terraria.Utilities;
using VFXPlus.Content.Weapons.Magic.Hardmode.Tomes;
using VFXPLus.Common;
using Terraria.Utilities.Terraria.Utilities;


namespace VFXPlus.Content
{
    public class DebugItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 22;

            Item.width = 46;
            Item.height = 28;

            Item.useTime = Item.useAnimation = 7; 
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.knockBack = 0;
            Item.rare = ItemRarityID.Cyan;
            Item.shootSpeed = 10f;

            Item.shoot = ProjectileID.TopazBolt;

            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;

        }

        bool tick = false;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //int bA = Projectile.NewProjectile(null, Main.MouseWorld, velocity.SafeNormalize(Vector2.UnitX) * -9f, ProjectileID.VortexBeaterRocket, 10, 0, player.whoAmI);

            int num35 = Main.rand.Next(4, 8);
            for (int num36 = 0; num36 < num35; num36++)
            {
                int num37 = Dust.NewDust(Main.MouseWorld, 0, 0, 229, 0f, 0f, 100, default(Color), 0.8f);
                Main.dust[num37].velocity *= 1.6f;
                Main.dust[num37].velocity.Y -= 1f;
                Main.dust[num37].velocity += velocity;
                Main.dust[num37].noGravity = true;
            }


            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);

            Vector2 pos = player.Center + new Vector2(0f, 100f);
            //Impact
            for (int i = 0; i < 6 + Main.rand.Next(0, 4); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 13, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 7 + Main.rand.Next(0, 3); i++)
            {
                if (i > 4)
                {
                    Vector2 smvel = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                    Dust sm = Dust.NewDustPerfect(pos, ModContent.DustType<HighResSmoke>(), smvel, newColor: Color.OrangeRed * 1f, Scale: Main.rand.NextFloat(0.35f, 0.75f));
                    sm.customData = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                        overallAlpha: 0.5f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                }

                Color col = Main.rand.NextBool() ? between : Color.OrangeRed;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 5f);
                Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.75f, 1.25f) * 0.8f);
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(pos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);


            for (int i22 = 220; i22 < 3 + Main.rand.Next(0, 4); i22++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<PixelatedLineSpark>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 19),
                    newColor: Color.MediumAquamarine, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80

            }


            //(Main.projectile[b].ModProjectile as WindPulse).timeForPulse = 50;


            for (int i = 2220; i < 22 + Main.rand.Next(0, 2); i++) //4 //2,2
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) * 9f;

                Color rainbow = Main.hslToRgb((i * 0.05f + 0.5f) % 1f, 1f, 0.4f, 0) * 1f;



                Dust p = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<PixelatedLineSpark>(), vel,
                    newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1f);

                p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                    1f, 0.5f, shouldFadeColor: false);

                if (i % 1 == 0)
                {
                    Dust p2 = Dust.NewDustPerfect(Main.MouseWorld + vel * 4f, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.4f);
                    p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.05f);
                }

            }
            return false;

            int number_of_feathers = 6;
            for (int a1 = 2220; a1 < number_of_feathers; a1++)
            {
                float prog = (float)a1 / (float)number_of_feathers;

                int feather = Projectile.NewProjectile(null, Main.MouseWorld, new Vector2(13f, 0f).RotatedBy(MathHelper.TwoPi * prog), ModContent.ProjectileType<CurvingFeather>(), 1, 1);
                (Main.projectile[feather].ModProjectile as CurvingFeather).curveValue = -0.1f;

            }

            for (int i = 220; i < 5; i++) //4 //2,2
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);

                Color innerCol = Color.Lerp(Color.LightSkyBlue, Color.Tan, 0.5f);


                Dust p = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<WindLine>(), vel + -velocity * 2f,
                    newColor: innerCol, Scale: 1f);

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, 0.9f, 0.35f, true);
                wlb.randomVelRotatePower = 0.2f;

                p.customData = wlb;
            }

            /*
            CirclePulseBehavior cpb2 = new CirclePulseBehavior(10f, false, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(player.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(player.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.SkyBlue * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);
            */

            Vector2 posA = player.Center + new Vector2(0f, 0f);
            Vector2 posB = player.Center + new Vector2(0f, 90f);
            Vector2 posC = player.Center + new Vector2(0f, -90f);

            int ar = Projectile.NewProjectile(null, posA, new Vector2(0f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
            int br = Projectile.NewProjectile(null, posB, new Vector2(0f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
            int cr = Projectile.NewProjectile(null, posC, new Vector2(0f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);

            (Main.projectile[ar].ModProjectile as MadisonTornado).startDir = -1;// Main.rand.NextBool() ? 1 : -1;
            (Main.projectile[br].ModProjectile as MadisonTornado).startDir = -1;
            (Main.projectile[cr].ModProjectile as MadisonTornado).startDir = -1;


            /*
            for (int i = 0; i < 22 + Main.rand.Next(0, 2); i++) //4 //2,2
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);

                Color rainbow = Main.hslToRgb((i * 0.05f + 0.5f) % 1f, 1f, 0.6f, 0) * 1f;



                Dust p = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<PixelatedLineSpark>(), vel,
                    newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.7f);

                p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                    1f, 0.5f, shouldFadeColor: false);

                if (i % 1 == 0)
                {
                    Dust p2 = Dust.NewDustPerfect(Main.MouseWorld + vel * 4f, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.2f);
                    p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.05f);
                }

            }
            */
            //return false;



            return false;
            int[] orbitValues1 = { 20,  80, 140,
                                  40,  100, 160,
                                  60,  120, 180 };

            int[] orbitValues2 = { 20,  60, 40,
                                  100,  40, 100,
                                  60,  80, 80 };

            int[][] orbitValues = { orbitValues1, orbitValues2 };

            int numberOfFeahters = 9;
            for (int ab = 0; ab < 2; ab++)
            {
                for (int index = 1; index <= numberOfFeahters; index++)
                {
                    int orbfeather = Projectile.NewProjectile(null, player.Center, Vector2.Zero, ModContent.ProjectileType<OrbitingFeather>(), damage, 0, Main.myPlayer);

                    if (Main.projectile[orbfeather].ModProjectile is OrbitingFeather of)
                    {
                        of.timeToOrbit = 60 + (orbitValues[ab][index - 1] * 2) + (180 * ab * 2);  //60 * index;
                        of.orbitVector = new Vector2(355f - (100 * ab), 0f).RotatedBy(MathHelper.TwoPi * ((index - 1f) / numberOfFeahters));
                        of.orbitVal = 355f - (100 * ab);
                        of.rotSpeed = ab == 0 ? 1.85f : 1.5f;
                    }

                }
            }

            int barrier = Projectile.NewProjectile(null, player.Center + new Vector2(0f, -245f), Vector2.Zero, ModContent.ProjectileType<WindBarrier>(), 0, 0, Main.myPlayer);

            //int a21 = Projectile.NewProjectile(null, position, velocity * 0f, ModContent.ProjectileType<GoozmaPrismStudy>(), 2, 0, player.whoAmI);

            //Main.projectile[a21].direction = (Main.rand.Next(5) > 2 ? 1 : -1);
            //Main.projectile[a21].rotation = Main.rand.NextFloat(-2f, 2f);
            //Main.projectile[a21].ai[0] = -66;
            //Main.projectile[a21].ai[1] = 0;
            //Main.projectile[a21].ai[2] = Main.rand.NextFloat(-3f, 3f);


            //!!!!!!!int b = Projectile.NewProjectile(null, position, velocity.SafeNormalize(Vector2.UnitX) * 17f, ProjectileID.WaterBolt, 2, 0, player.whoAmI);



            //Projectile.NewProjectile(null, position, velocity, ProjectileID.ShadowFlame, 2, 0, player.whoAmI);

            //Projectile.NewProjectile(null, position, velocity.RotatedBy(2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);
            //Projectile.NewProjectile(null, position, velocity.RotatedBy(-2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);


            return false;
        }
        
    }
}