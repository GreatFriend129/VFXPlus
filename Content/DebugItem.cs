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