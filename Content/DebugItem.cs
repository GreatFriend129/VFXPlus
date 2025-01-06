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
            int b = Projectile.NewProjectile(null, Main.MouseWorld, velocity.SafeNormalize(Vector2.UnitX) * 0f, ModContent.ProjectileType<WindPulse>(), 0, 0, player.whoAmI);

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

            ///player.GetModPlayer<ScreenShakePlayer>().ScreenShakePower = 20;

            return false;
            for (int i = 220; i < 22; i++)
            {
                float prog = (float)i / 22f;

                Color between = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
                Color col = Color.Lerp(Color.YellowGreen, Color.Black, 1f - prog);

                Dust d = Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 3.4f) * 1.65f, 
                    newColor: col with { A = 0 } * prog, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.75f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 1f); //12 28

                d.velocity += velocity * 0.67f * (prog);
            }


            FastRandom r = new(player.name.GetHashCode());

            float min = 0.4f;
            float max = 1.2f;

            for (int i = 0; i < 5; i++)
            {
                float val = min + r.NextFloat() * (max - min);
                //Main.NewText(val);
            }
            //Main.NewText("-------------------" + player.name.GetHashCode());

            //Main.NewText(player.name);

            Vector2 posA = Main.MouseWorld + new Vector2(0f, 0f);
            Vector2 posB = player.Center + new Vector2(0f, 90f);
            Vector2 posC = player.Center + new Vector2(0f, -90f);

            int ar = Projectile.NewProjectile(null, posA, new Vector2(10f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
            //int br = Projectile.NewProjectile(null, posB, new Vector2(-10f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);
            //int cr = Projectile.NewProjectile(null, posC, new Vector2(-10f, 0f), ModContent.ProjectileType<MadisonTornado>(), 10, 0, Main.myPlayer);


            (Main.projectile[ar].ModProjectile as MadisonTornado).startDir = -1;// Main.rand.NextBool() ? 1 : -1;
            //(Main.projectile[br].ModProjectile as MadisonTornado).startDir = -1;
            //(Main.projectile[cr].ModProjectile as MadisonTornado).startDir = -1;


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