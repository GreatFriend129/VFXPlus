#region Using directives

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using Terraria.DataStructures;

#endregion

namespace VFXPlus.Common
{
    internal static class UseStyleHelper
    {
        public static void BasicBowUseStyle(Player player)
        {
            float rot = player.itemRotation - MathHelper.PiOver2 * player.direction;
            float prog = 1f - player.itemTime / (float)player.itemTimeMax;

            prog = Easings.easeInQuad(prog);
            if (prog < 0.15)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, rot);
            else if (prog < 0.25f)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot);
            else if (prog < 0.5)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rot);
            else if (prog < 0.75f)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, rot);
            else if (prog < 1f)
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, rot);


            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rot);
        }

    }

    public class HeldBowPlayer : ModPlayer
    {
        public int arrowType = 1;
        public int bowType = 1;

        public Vector2 holdOffset = Vector2.Zero;


        public float arrowOffset = 0f;
        public float arrowPullAmount = 0f;

        public Color underGlowColor = Color.White;
        public float underGlowPower = 0f;
        private class HeldBowDrawLayer : PlayerDrawLayer
        {
            public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
            {
                int bowID = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().bowType;
                return drawInfo.drawPlayer.HeldItem?.type == bowID && drawInfo.drawPlayer.controlUseItem;
            }

            public override Position GetDefaultPosition()
            {
                return new AfterParent(PlayerDrawLayers.HeldItem);
            }
            protected override void Draw(ref PlayerDrawSet drawInfo)
            {
                if (drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().bowType == ItemID.PulseBow)
                {
                    DrawPulseBowArrow(drawInfo);
                    return;
                }

                int dir = drawInfo.drawPlayer.direction;

                float rot = drawInfo.drawPlayer.itemRotation + MathHelper.PiOver2 * dir;

                //HoldoutOffset
                float xOff = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().holdOffset.X;
                float yOff = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().holdOffset.Y;
                Vector2 off = new Vector2(xOff * dir, yOff);

                //AlphaScale
                float prog = Math.Clamp(drawInfo.drawPlayer.itemTime / (float)drawInfo.drawPlayer.itemTimeMax, 0f, 1f);
                float alpha = Easings.easeInOutQuad(1f - prog);
                Vector2 scale = new Vector2(0.35f + (0.65f * alpha), 1f);

                //Arrow Pos
                float arrowProg = 1f - Easings.easeInQuad(1f - prog);

                float arrowOff = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().arrowOffset;
                float arrowPull = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().arrowPullAmount;

                Vector2 drawPos = drawInfo.drawPlayer.MountedCenter - Main.screenPosition + new Vector2(0f, arrowOff - (arrowPull * arrowProg)).RotatedBy(rot);
                drawPos.Y += drawInfo.drawPlayer.gfxOffY;

                //Arrow Texture
                int type = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().arrowType;
                Texture2D arrow = TextureAssets.Projectile[type].Value;

                //UnderGlow
                for (int i = 0; i < 3; i++)
                {
                    float underGlow = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().underGlowPower;
                    Color underCol = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().underGlowColor;
                    Vector2 randOff = Main.rand.NextVector2Circular(2f, 2f);
                    drawInfo.DrawDataCache.Add(new DrawData(arrow, drawPos + off + randOff, null, underCol with { A = 0 } * alpha * underGlow, rot, arrow.Size() / 2f, scale * 1.05f, SpriteEffects.None, 0));
                }

                drawInfo.DrawDataCache.Add(new DrawData(arrow, drawPos + off, null, Color.White * alpha, rot, arrow.Size() / 2f, scale, SpriteEffects.None, 0));

            }

            public void DrawPulseBowArrow(PlayerDrawSet drawInfo)
            {
                int dir = drawInfo.drawPlayer.direction;

                float rot = drawInfo.drawPlayer.itemRotation;// + MathHelper.PiOver2 * dir;

                //HoldoutOffset
                float xOff = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().holdOffset.X;
                float yOff = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().holdOffset.Y;
                Vector2 off = new Vector2(xOff * dir, yOff);

                //AlphaScale
                float prog = Math.Clamp(drawInfo.drawPlayer.itemTime / (float)drawInfo.drawPlayer.itemTimeMax, 0f, 1f);
                float alpha = Easings.easeInOutQuad(1f - prog);
                Vector2 scale = new Vector2(1f, 0.35f + (0.65f * alpha));

                //Arrow Pos
                float arrowProg = 1f - Easings.easeInQuad(1f - prog);

                float arrowOff = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().arrowOffset;
                float arrowPull = drawInfo.drawPlayer.GetModPlayer<HeldBowPlayer>().arrowPullAmount;

                Vector2 drawPos = drawInfo.drawPlayer.MountedCenter - Main.screenPosition + new Vector2(arrowOff - (arrowPull * arrowProg), 0f).RotatedBy(rot);
                drawPos.Y += drawInfo.drawPlayer.gfxOffY;

                //Arrow Texture
                Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

                drawInfo.DrawDataCache.Add(new DrawData(flare, drawPos + off, null, Color.DeepSkyBlue with { A = 0 } * alpha, rot, flare.Size() / 2f, scale, SpriteEffects.None, 0));
                drawInfo.DrawDataCache.Add(new DrawData(flare, drawPos + off, null, Color.White with { A = 0 } * alpha, rot, flare.Size() / 2f, scale, SpriteEffects.None, 0)); 
            }

        }
    }
}
