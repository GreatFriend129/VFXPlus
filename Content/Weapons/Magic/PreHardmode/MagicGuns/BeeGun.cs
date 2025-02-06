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
using System.Runtime.InteropServices;
using Terraria.GameContent;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class BeeGun : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BeeGun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BeeGunToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Dust
            for (int i = 0; i < 7 + Main.rand.Next(0, 4); i++)
            {
                if (Main.rand.NextBool())
                {
                    Color col = Color.Orange;

                    Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 38, ModContent.DustType<GlowPixelAlts>(),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.35f);

                    dp.noGravity = true;
                }
                else
                {
                    Color col = Color.Black;

                    Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 38, DustID.Bee,
                        velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.15f);

                    dp.noGravity = true;
                }

            }

            SoundEngine.PlaySound(SoundID.Item11, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_97") with { Volume = 0.15f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }

}
