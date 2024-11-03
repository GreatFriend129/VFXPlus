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
            int a = Projectile.NewProjectile(null, position, velocity * 1.25f, ModContent.ProjectileType<ThunderBall>(), 2, 0, player.whoAmI);
            SoundStyle styleb = new SoundStyle("AerovelenceMod/Sounds/Effects/Item125Trim") with { Volume = .45f, Pitch = 1f, PitchVariance = .11f, MaxInstances = -1 };
            SoundEngine.PlaySound(styleb, player.Center);

            SoundStyle styla = new SoundStyle("Terraria/Sounds/Item_122") with { Pitch = 1f, Volume = 0.9f, PitchVariance = 0.11f };
            SoundEngine.PlaySound(styla, player.Center);

            //Projectile.NewProjectile(null, position, velocity, ProjectileID.ShadowFlame, 2, 0, player.whoAmI);

            //Projectile.NewProjectile(null, position, velocity.RotatedBy(2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);
            //Projectile.NewProjectile(null, position, velocity.RotatedBy(-2f), ProjectileID.MagicDagger, 2, 0, player.whoAmI);


            return false;

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_43") with { Volume = 0.8f, Pitch = .25f, PitchVariance = 0.05f };
            SoundEngine.PlaySound(style4, player.Center);

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_etherian_portal_dryad_touch") with { Volume = .3f, Pitch = 1f, PitchVariance = .15f, MaxInstances = -1, };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_20") with { Volume = 0.65f, Pitch = .45f, PitchVariance = 0.1f };
            SoundEngine.PlaySound(style2, player.Center);

            Projectile.NewProjectile(null, position, velocity.RotatedBy(-0.15), ProjectileID.TopazBolt, 2, 0, player.whoAmI);
            Projectile.NewProjectile(null, position, velocity.RotatedBy(-0.1), ProjectileID.EmeraldBolt, 2, 0, player.whoAmI);
            Projectile.NewProjectile(null, position, velocity.RotatedBy(-0.05), ProjectileID.SapphireBolt, 2, 0, player.whoAmI);

            Projectile.NewProjectile(null, position, velocity.RotatedBy(0), ProjectileID.DiamondBolt, 2, 0, player.whoAmI);

            Projectile.NewProjectile(null, position, velocity.RotatedBy(0.05), ProjectileID.RubyBolt, 2, 0, player.whoAmI);
            Projectile.NewProjectile(null, position, velocity.RotatedBy(0.1), ProjectileID.AmethystBolt, 2, 0, player.whoAmI);
            Projectile.NewProjectile(null, position, velocity.RotatedBy(0.15), ProjectileID.AmberBolt, 2, 0, player.whoAmI);

            return false;
        }
        
    }
}