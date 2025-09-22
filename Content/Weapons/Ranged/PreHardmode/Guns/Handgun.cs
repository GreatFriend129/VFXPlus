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
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.Gores;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Guns
{
    public class Handgun : GlobalItem 
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Handgun);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicGunProjLong>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicGunProjLong held)
            {
                held.SetProjInfo(
                    GunID: ItemID.Handgun,
                    AnimTime: 14,
                    NormalXOffset: 17f,
                    DestXOffset: 9f,
                    YRecoilAmount: 0.18f,
                    HoldOffset: new Vector2(-3f, 3f),//4
                    TipPos: new Vector2(22, -8f),
                    StarPos: new Vector2(20, -8f)
                    );
            }
            Main.projectile[gun].scale = 0.9f;

            //Explosion
            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(37f, -4f * dir).RotatedBy(velocity.ToRotation()) + new Vector2(0f, 0f);

            for (int i = 0; i < 6; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);

                float progress = (float)i / 5;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.85f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.25f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.75f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 0.5f;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.1f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 2 + Main.rand.Next(0, 2); i++)
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f);


                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1.5f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.9f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 2f;
            }

            //Sound
            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_38") with { Volume = .25f, Pitch = 0.5f, PitchVariance = 0.15f }; //1f P
            SoundEngine.PlaySound(style4, position);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Gun/SniperShotA") with { Volume = .01f, Pitch = -1f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, position);

            SoundStyle style5 = new SoundStyle("VFXPlus/Sounds/Effects/Gun/SingleShot") with { Volume = .45f, Pitch = -0.25f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style5, position);

            //Bullet Casing
            Gore.NewGore(source, position + velocity, new Vector2(velocity.X * -0.25f, -0.75f), ModContent.GoreType<BulletCasing>());

            return true;
        }
    }

}
