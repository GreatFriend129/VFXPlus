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
    
    public class CopperBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.CopperBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Copper,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.White * 0.4f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.45f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.CopperBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }

    public class TinBow : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.TinBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Tin,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.White with { A = 0 } * 0.15f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.45f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.TinBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }

    public class IronBow : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.IronBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Iron,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.White with { A = 0 } * 0.15f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.45f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.IronBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }

    public class LeadBow : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.LeadBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Lead,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.White with { A = 0 } * 0.15f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.45f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.LeadBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }
    public class SilverBow : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SilverBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Silver,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.White with { A = 0 } * 0.15f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.45f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.SilverBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }
    public class TungstenBow : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.TungstenBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Tungsten,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.White with { A = 0 } * 0.15f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.45f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.TungstenBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }

    public class GoldBow : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.GoldBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Gold,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 5),
                    newColor: Color.White with { A = 0 }, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.4f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.GoldBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }

    public class PlatinumBow : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.PlatinumBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++)
            {
                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 10, DustID.Platinum,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 5),
                    newColor: Color.White with { A = 0 }, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.4f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.PlatinumBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }
}
