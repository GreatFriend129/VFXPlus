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


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class WoodenBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.WoodenBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int count = 2 + Main.rand.Next(0, 2);
            for (int i = 0; i < count; i++)
            {
                float prog = (float)i / (float)count;
                
                Vector2 posOffset = new Vector2(10f, Main.rand.NextFloat(-2f, 2f)).RotatedBy(velocity.ToRotation());

                Vector2 vel = velocity.RotateRandom(0.55f).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1.5f, 5f);


                float newScale = 0.25f + (prog * 0.15f);

                Color brown = new Color(160, 106, 70);

                Dust d = Dust.NewDustPerfect(position + posOffset, DustID.WoodFurniture, vel, newColor: Color.White with { A = 0 } * 0.5f, Scale: 0.85f);
                d.noGravity = true;
            }
            return true;
        }

    }
}
