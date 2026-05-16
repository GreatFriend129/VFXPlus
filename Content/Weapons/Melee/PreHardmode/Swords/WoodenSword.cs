using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Swords
{

    public class WoodenSwordItemOverride : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.WoodenSword);
        }

        public override void SetDefaults(Item entity)
        {
            entity.shoot = ProjectileID.WoodenArrowFriendly;
            entity.shootsEveryUse = true;
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
            int trail = Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<BaseSwordProj>(), 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);

            Main.NewText(adjustedItemScale);

            //Always start with black probably
            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                Color.Lerp(Color.SaddleBrown, Color.SandyBrown, 0.33f).ToVector3(),
                Color.Lerp(Color.SaddleBrown, Color.SandyBrown, 0.66f).ToVector3(),
            };


            SwordProjInfo info = new SwordProjInfo(item.type, gradCols, 10f, 0f, 38f, 2f, 0.75f, 1f);
            (Main.projectile[trail].ModProjectile as BaseSwordProj).info = info;

            return false;
        }

    }

}
