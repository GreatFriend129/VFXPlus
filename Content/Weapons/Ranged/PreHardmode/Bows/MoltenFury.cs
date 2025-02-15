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
    
    public class MoltenFury : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.MoltenFury);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 5 + Main.rand.Next(0, 4); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 15, DustID.Torch,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.8f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.MoltenFury;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 1.5f;
            player.GetModPlayer<HeldBowPlayer>().underGlowColor = Color.OrangeRed;

            return true;

        }
        public override Vector2? HoldoutOffset(int type)
        {
            return new Vector2(5f, 0f);
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);


    }

    
}
