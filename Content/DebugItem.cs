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
using System.Net;
using VFXPlus.Content.Weapons.Ranged.Ammo.Arrows;
using VFXPlus.Content.VFXTest.Aero;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Bows;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Misc;
using VFXPlus.Common;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.QueenBee;
using VFXPlus.Content.VFXTest.Aero.Oblivion;
using VFXPlus.Content.VFXTest.Aero.Sword;
using static log4net.Appender.ColoredConsoleAppender;
using System.Xml.Linq;


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
        public override bool AltFunctionUse(Player player) => true;
        
        
        bool tick = false;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 pos = Main.MouseWorld;
                NPC.NewNPC(null, (int)pos.X, (int)pos.Y, ModContent.NPCType<FeatheredFoeBoss>());
                return false;
            }

            for (int iaa = -3; iaa < -224; iaa++)
            {
                Vector2 vel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(iaa * MathHelper.PiOver4 * 1.25f) * -10f;
                float curvePower = iaa * 0.009f;

                int curveFeather = Projectile.NewProjectile(null, Main.MouseWorld, vel, ModContent.ProjectileType<CurvingFeather>(), 1, 0, Main.myPlayer);

                if (Main.projectile[curveFeather].ModProjectile is CurvingFeather cf)
                {
                    cf.curveValue = curvePower;
                }

            }


            //0->0.5 | 0.5->1
            int numberOfCols = 3;
            int numberOfLerps = numberOfCols - 1;
            Color[] cols = { Color.White, Color.Black, Color.Red };
            float[] interpPowers = { 2f, 1f };
            float valToTest = 0.51f;

            float power = 2;


            int gradientStartingIndex = (int)(valToTest * (cols.Length - 1));
            float currentColorInterpolant = valToTest * (cols.Length - 1) % 1f;
            Color gradientSubdivisionA = cols[gradientStartingIndex];
            Color gradientSubdivisionB = cols[(gradientStartingIndex + 1) % (cols.Length - 1)];

            //currentColorInterpolant = MathF.Pow(currentColorInterpolant, interpPowers[]);

            Color toUse = Color.Lerp(gradientSubdivisionA, gradientSubdivisionB, currentColorInterpolant);

            Main.NewText(interpPowers[gradientStartingIndex]);

            //int colIndex = (int)Math.Floor(valToTest * numberOfLerps);
            //float colPercent = (valToTest * numberOfLerps) - colIndex;

            //Main.NewText("ColIndex: " + colIndex);
            //Main.NewText("ColPercent: " + colPercent);



            //Color toUse = Color.Lerp(cols[colIndex], cols[colIndex + 1], colPercent);

            Dust.NewDustPerfect(Main.MouseWorld, DustID.PortalBoltTrail, newColor: toUse);


            /*
            int numberOfCols = 4;
            int numberOfLerps = numberOfCols - 1;
            Color[] cols = { Color.Green, Color.White, Color.Black, Color.Blue };

            float valToTest = 0.5f;

            int colIndex = (int)Math.Floor(valToTest * numberOfLerps);

            float colPercent = (valToTest * numberOfLerps) - colIndex;
            */








            return false;


            //int windFX2 = Projectile.NewProjectile(null, player.Center, velocity.SafeNormalize(Vector2.UnitX) * 0f, ModContent.ProjectileType<WindPulseTest2>(), 0, 0, Main.myPlayer);

            //Vector2 velAAA = new Vector2(8f, 0f);
            //int are = Projectile.NewProjectile(null, Main.MouseWorld, new Vector2(8f, 2f) * 0.5f, ModContent.ProjectileType<FFWindOrb2>(), 10, 0, player.whoAmI);
            //(Main.projectile[are].ModProjectile as FFWindOrb2).startDir = -1;

            Color purple3 = new Color(121, 7, 179);
            Color betweenGold = Color.Lerp(Color.Gold, Color.OrangeRed, 0.2f);

            for (int i = 220; i < 13; i++)
            {
                float prog = (float)i / 13f;

                float proggg = Main.rand.NextFloat();
                Color col = Color.Lerp(betweenGold with { A = 0 }, Color.Purple * 1.25f, Easings.easeOutQuad(1f - prog));

                Dust d = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 3f) * 2.5f,
                    newColor: col * Easings.easeOutCubic(prog), Scale: Main.rand.NextFloat(1.15f, 1.5f) * 1f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(15, 25), 0.93f, 0.01f, 0.9f); //12 28

            }

            for (int i = 220; i < 7 + Main.rand.Next(0, 6); i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.Purple * 1f : Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);


                float velMult = Main.rand.NextFloat(1f, 6f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult);
                Dust dust = Dust.NewDustPerfect(Main.MouseWorld + randomStart * 5f, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0,
                    newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));

                dust.scale *= 1.75f;

                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 0.95f);
            }

            int[] orbitValues1 = { 25, 100, 175, //20 80 140 | 40 10 160 | 60 120 180
                                  50,  125, 200,
                                  75,  150, 225 };

            int[] orbitValues2 = { 30, 86, 142, //20 60 40 | 100 40 100 | 60 80 80
                                  38,  94, 150,
                                  46,  102, 158 };

            int[][] orbitValues = { orbitValues1, orbitValues2 };

            int numberOfFeahters = 9;
            for (int ab = 0; ab < 2; ab++)
            {
                for (int index = 1; index <= numberOfFeahters; index++)
                {
                    int orbfeather = Projectile.NewProjectile(null, player.Center, Vector2.Zero, ModContent.ProjectileType<OrbitingFeather>(), damage, 0, Main.myPlayer);

                    if (Main.projectile[orbfeather].ModProjectile is OrbitingFeather of)
                    {
                        of.timeToOrbit = 60 + (orbitValues[ab][index - 1] * 1) + (225 * ab);  //60 * index;
                        of.orbitVector = new Vector2(405f - (85 * ab), 0f).RotatedBy(MathHelper.TwoPi * ((index - 1f) / numberOfFeahters));
                        of.orbitVal = 300f - (100 * ab);
                        of.rotSpeed = ab == 0 ? 1.85f : 1.5f;
                    }

                }
            }

            int barrier = Projectile.NewProjectile(null, player.Center + new Vector2(0f, -255f), Vector2.Zero, ModContent.ProjectileType<WindBarrier>(), 0, 0, Main.myPlayer);
            (Main.projectile[barrier].ModProjectile as WindBarrier).center = player.Center;

            return false;
            
            //return false;
            Vector2 posAA = player.Center + new Vector2(-450f, 600f);
            Vector2 posBB = player.Center + new Vector2(-450f, -600f);
            Vector2 posCC = player.Center + new Vector2(-450f, 0f);

            Vector2 velAA = new Vector2(8f, 0f);
            int bA2 = Projectile.NewProjectile(null, posAA, velAA, ModContent.ProjectileType<FFWindOrb>(), 10, 0, player.whoAmI);
            int bB2 = Projectile.NewProjectile(null, posBB, velAA, ModContent.ProjectileType<FFWindOrb>(), 10, 0, player.whoAmI);
            int bC2 = Projectile.NewProjectile(null, posCC, velAA, ModContent.ProjectileType<FFWindOrb>(), 10, 0, player.whoAmI);

            (Main.projectile[bA2].ModProjectile as FFWindOrb).startDir = -1;

            return false;

            
            //

            for (int i = 220; i < 22 + Main.rand.Next(0, 2); i++) //4 //2,2
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
            //return false;

            int number_of_feathers = 6;
            for (int a1 = 2220; a1 < number_of_feathers; a1++)
            {
                float prog = (float)a1 / (float)number_of_feathers;

                int feather = Projectile.NewProjectile(null, Main.MouseWorld, new Vector2(13f, 0f).RotatedBy(MathHelper.TwoPi * prog), ModContent.ProjectileType<CurvingFeather>(), 1, 1);
                (Main.projectile[feather].ModProjectile as CurvingFeather).curveValue = -0.1f;

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

            (Main.projectile[ar].ModProjectile as MadisonTornado).startDir = -1;
            (Main.projectile[br].ModProjectile as MadisonTornado).startDir = 1;
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