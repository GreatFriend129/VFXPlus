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
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest.Aero;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    
    public class DartPistolOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.DartPistol);
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.DartPistol,
                    AnimTime: 20,
                    NormalXOffset: 23f,
                    DestXOffset: 2f,
                    YRecoilAmount: 0.2f,
                    HoldOffset: new Vector2(0f, 0f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            return true;
        }

    }

}
