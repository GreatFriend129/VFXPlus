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
using System.Threading;
using VFXPlus.Content.VFXTest;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.Gores;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Guns
{
    public class VenusMagnum : GlobalItem 
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.VenusMagnum);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicGunProjMiddle>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicGunProjMiddle held)
            {
                held.SetProjInfo(
                    GunID: ItemID.VenusMagnum,
                    AnimTime: 14,
                    NormalXOffset: 20f,
                    DestXOffset: 14f,
                    YRecoilAmount: 0.12f,
                    HoldOffset: new Vector2(0f, -4f),
                    TipPos: new Vector2(30f, -5f),
                    StarPos: new Vector2(24f, -5f)
                    );
            }
            Main.projectile[gun].scale = 0.85f;

            //Explosion
            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(40f, -6f * dir).RotatedBy(velocity.ToRotation());

            //Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 50f;
            for (int i = 0; i < 5; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);

                float progress = (float)i / 4;
                Color col = Color.Lerp(Color.DarkGreen * 0.5f, Color.LawnGreen with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.85f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.35f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.75f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 0.5f;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.ForestGreen, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.1f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 1 + Main.rand.Next(0, 2); i++)
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f);


                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.ForestGreen, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1.5f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.9f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 2f;
            }

            //Bullet Casing
            Gore.NewGore(source, position + velocity, new Vector2(velocity.X * -0.25f, -0.75f), ModContent.GoreType<GreenCasing>());

            //Sound
            
            //GGator
            
            float volumeMult = 0.6f; //.5

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_38") with { Volume = .15f * volumeMult, Pitch = 0.5f, PitchVariance = 0.15f }; //1f P
            SoundEngine.PlaySound(style4, position);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Gun/GunShotC") with { Volume = 0.15f * volumeMult, Pitch = 0.2f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style5 = new SoundStyle("VFXPlus/Sounds/Effects/Gun/SingleShot") with { Volume = .45f * volumeMult, Pitch = -0.25f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style5, position);

            return true;
        }
    }

}
