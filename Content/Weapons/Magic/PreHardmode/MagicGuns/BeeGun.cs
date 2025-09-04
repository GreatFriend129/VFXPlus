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
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class BeeGunOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BeeGun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BeeGunToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.BeeGun,
                    AnimTime: 18,
                    NormalXOffset: 22f,
                    DestXOffset: 10f,
                    YRecoilAmount: 0.1f,
                    HoldOffset: new Vector2(0f, 0f)
                    );

                //held.compositeArmAlwaysFull = true;
                held.timeToStartFade = 3;
            }

            //Bee gun is drawn at smaller in vanilla too b/c its fucking massive otherwise
            Main.projectile[gun].scale = 0.8f;


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

            //Smoke Dust
            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(32f, -3f * dir).RotatedBy(velocity.ToRotation());
            for (int i = 0; i < 11; i++) //16
            {
                float prog = (float)i / 11f;

                float proggg = Main.rand.NextFloat();
                Color col = Color.Lerp(Color.Orange with { A = 0 } * 0.7f, Color.Black * 0.45f, 1f - prog);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 1f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.4f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.75f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 0.85f;
            }

            //Sound isn't pitched much and uncommon so we don't need to make a copy of the sound
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_97") with { Volume = 0.1f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }

}
