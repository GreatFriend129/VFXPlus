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
        public override void SetDefaults(Item entity)
        {
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 8 + Main.rand.Next(0, 5); i++)
            {
                Color col = Color.Gray;

                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 15, DustID.Shadowflame,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: Color.Silver, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.75f);

                dp.noGravity = true;
            }

            player.GetModPlayer<HeldBowPlayer>().arrowType = type;

            return true;

        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            //Have composite arms start at max dist and quickly pull back

            float rot = player.itemRotation - MathHelper.PiOver2 * player.direction;

            int pullBackTime = 10;

            float prog = 1f - player.itemTime / (float)player.itemTimeMax;

            prog = Easings.easeInQuad(prog);
            if (prog < 0.25f)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot);
            else if (prog < 0.5)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rot);
            else if (prog < 0.75f)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, rot);
            else if (prog < 1f)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, rot);


            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rot);

            //player.
        }

        public override Vector2? HoldoutOffset(int type)
        {
            return base.HoldoutOffset(type);
        }

    }

    public class HeldBowPlayer : ModPlayer
    {
        public int arrowType = 1;

        public float justShotPower = 0f;
        private class HeldBowDrawLayer : PlayerDrawLayer
        {
            public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
            {
                return drawInfo.drawPlayer.HeldItem?.type == ItemID.DemonBow && drawInfo.drawPlayer.controlUseItem;
            }

            public override Position GetDefaultPosition()
            {
                return new AfterParent(PlayerDrawLayers.HeldItem);
            }
            protected override void Draw(ref PlayerDrawSet drawInfo)
            {
                int dir = drawInfo.drawPlayer.direction;

                Vector2 off = new Vector2(-2f * dir, 0f);

                int type = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().arrowType;
                float rot = drawInfo.drawPlayer.itemRotation + MathHelper.PiOver2 * dir;

                Texture2D arrow = TextureAssets.Projectile[type].Value;

                float prog = Math.Clamp((drawInfo.drawPlayer.itemTime / (float)drawInfo.drawPlayer.itemTimeMax) * 0.7f, 0f, 1f);
                float alpha = Easings.easeInOutQuad(1f - prog);
                Vector2 scale = new Vector2(0.35f + (0.65f * alpha), 1f);

                Vector2 drawPos = drawInfo.drawPlayer.MountedCenter - Main.screenPosition + new Vector2(0f, -15f - (15f * (1f - alpha))).RotatedBy(rot);
                drawPos.Y += drawInfo.drawPlayer.gfxOffY;


                drawInfo.DrawDataCache.Add(new DrawData(arrow, drawPos + off, null, Color.White * alpha, rot, arrow.Size() / 2f, scale, SpriteEffects.None, 0));

            }
        }
    }
}
