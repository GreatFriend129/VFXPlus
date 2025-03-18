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
using static tModPorter.ProgressUpdate;
using System.Runtime.Intrinsics.Arm;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Bows
{
    
    public class DemonBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.DemonBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.DemonBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 2f;
            player.GetModPlayer<HeldBowPlayer>().underGlowColor = new Color(42, 2, 82);

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }

    
}
