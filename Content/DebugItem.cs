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
            //int a21 = Projectile.NewProjectile(null, position, velocity * 0f, ModContent.ProjectileType<GoozmaPrismStudy>(), 2, 0, player.whoAmI);

            //Main.projectile[a21].direction = (Main.rand.Next(5) > 2 ? 1 : -1);
            //Main.projectile[a21].rotation = Main.rand.NextFloat(-2f, 2f);
            //Main.projectile[a21].ai[0] = -66;
            //Main.projectile[a21].ai[1] = 0;
            //Main.projectile[a21].ai[2] = Main.rand.NextFloat(-3f, 3f);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_0") with { Volume = .0f, Pitch = .6f, PitchVariance = 0.5f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);
            
            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_107") with { Volume = .3f, Pitch = .8f, PitchVariance = 0.3f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            for (int i = 20; i < 15; i++)
            {
                int a = Dust.NewDust(Main.MouseWorld, 20, 20, ModContent.DustType<ElectricSparkGlow>(), 0f, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.75f, 1f) * 1.15f);
                Main.dust[a].velocity = Main.rand.NextVector2CircularEdge(9f, 9f) * Main.rand.NextFloat(0.5f, 1.5f);
                Main.dust[a].velocity += velocity;

                //Dust dp = Dust.NewDustPerfect(position, ModContent.DustType<ElectricSparkGlow>(), velocity.RotatedBy(Main.rand.NextFloat(-0.75f, 0.75f)),
                    //newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.5f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 1f, FadeVelPower: 0.91f, Pixelize: true, XScale: 1f, YScale: 0.75f);
                esb.randomVelRotatePower = 0.3f; //5f
                //esb.killEarlyTime = 15;
                Main.dust[a].customData = esb;
            }


            //!!!!!!!int b = Projectile.NewProjectile(null, position, velocity.SafeNormalize(Vector2.UnitX) * 17f, ProjectileID.WaterBolt, 2, 0, player.whoAmI);
            //int b = Projectile.NewProjectile(null, position, velocity.SafeNormalize(Vector2.UnitX) * 17f, ProjectileID.WaterBolt, 2, 0, player.whoAmI);
            //(Main.projectile[a].ModProjectile as SolsearBombExplosion).size = 0.75f * 1f;

            //Dust d = Dust.NewDustPerfect(position, ModContent.DustType<CirclePulse>(), velocity * 0.3f, newColor: Color.DodgerBlue);
            //CirclePulseBehavior cpb = new CirclePulseBehavior(0.35f, true, 2, 0.4f, 0.8f);
            //cpb.drawBlackUnder = true;
            //cpb.blackUnderPower = 1f;
            //d.customData = cpb;


            //Dust d2 = Dust.NewDustPerfect(position, ModContent.DustType<CirclePulse>(), velocity * -0.3f, newColor: Color.HotPink);
            //CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.35f, true, 2, 0.8f, 0.8f);
            //cpb2.drawBlackUnder = false;
            //cpb2.blackUnderPower = 1f;
            //d2.customData = cpb2;


            //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_hurt_1") with { Volume = 1f, Pitch = .8f, PitchVariance = 0.1f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style, player.Center);

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_hurt_1") with { Pitch = .4f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style, player.Center);


            //SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/lightning_flash_01") with { Pitch = 1f, PitchVariance = 0.2f, Volume = 0.4f };
            //SoundEngine.PlaySound(style2, player.Center);

            //SoundStyle styleb = new SoundStyle("AerovelenceMod/Sounds/Effects/Item125Trim") with { Volume = .45f, Pitch = 1f, PitchVariance = .11f, MaxInstances = -1 };
            //SoundEngine.PlaySound(styleb, player.Center);

            //SoundStyle styla = new SoundStyle("Terraria/Sounds/Item_122") with { Pitch = 1f, Volume = 0.9f, PitchVariance = 0.11f };
            //SoundEngine.PlaySound(styla, player.Center);

            //Projectile.NewProjectile(null, position, velocity, ProjectileID.ShadowFlame, 2, 0, player.whoAmI);

            //Projectile.NewProjectile(null, position, velocity.RotatedBy(2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);
            //Projectile.NewProjectile(null, position, velocity.RotatedBy(-2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);


            return false;
        }
        
    }
}