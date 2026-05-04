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
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class OreRepeaters : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (
                item.type == ItemID.CobaltRepeater ||
                item.type == ItemID.MythrilRepeater ||
                item.type == ItemID.AdamantiteRepeater ||
                item.type == ItemID.PalladiumRepeater ||
                item.type == ItemID.OrichalcumRepeater ||
                item.type == ItemID.TitaniumRepeater
                );
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: item.type,
                    AnimTime: 18,
                    NormalXOffset: 16f,
                    DestXOffset: 2f,
                    YRecoilAmount: 0.12f,
                    HoldOffset: new Vector2(0f, 0f)
                    );
            }

            int randSound = Main.rand.Next(0, 2); //1

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_ballista_tower_shot_" + randSound) with { Volume = 0.1f, Pitch = .3f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_javelin_throwers_attack_1") with { Pitch = .65f, PitchVariance = 0.15f, Volume = 0.5f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_5") with { Volume = 1f, Pitch = -.55f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            return true;
        }

    }

    public class HallowedRepeaterOverride : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.HallowedRepeater);
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: item.type,
                    AnimTime: 18,
                    NormalXOffset: 16f,
                    DestXOffset: 2f,
                    YRecoilAmount: 0.12f,
                    HoldOffset: new Vector2(0f, 0f)
                    );
            }

            int randSound = Main.rand.Next(0, 2); //1

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_ballista_tower_shot_" + randSound) with { Volume = 0.1f, Pitch = .3f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_javelin_throwers_attack_1") with { Pitch = .65f, PitchVariance = 0.15f, Volume = 0.5f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_5") with { Volume = 1f, Pitch = -.55f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            Color color = Color.Gold;// Color.Lerp(Color.DeepSkyBlue, Color.DodgerBlue, 0.75f);
            Vector2 dustPos = position + velocity.SafeNormalize(Vector2.UnitX) * 27;
            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = velocity.SafeNormalize(Vector2.UnitX).RotateRandom(0.5f) * Main.rand.NextFloat(1f, 4f);
                Dust p = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: color, Scale: Main.rand.NextFloat(0.35f, 0.55f) * 1f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.89f, shouldFadeColor: false);
            }

            return true;
        }

    }

    public class ChloroShotbowOverride : GlobalItem
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ChlorophyteShotbow);
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: item.type,
                    AnimTime: 18,
                    NormalXOffset: 16f,
                    DestXOffset: 2f,
                    YRecoilAmount: 0.15f,
                    HoldOffset: new Vector2(0f, 0f)
                    );
            }

            int randSound = Main.rand.Next(0, 2); //1

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_ballista_tower_shot_" + randSound) with { Volume = 0.1f, Pitch = .3f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_javelin_throwers_attack_1") with { Pitch = .65f, PitchVariance = 0.15f, Volume = 0.5f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_5") with { Volume = 1f, Pitch = -.55f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            Color color = Color.Green;// Color.Lerp(Color.DeepSkyBlue, Color.DodgerBlue, 0.75f);
            Vector2 dustPos = position + velocity.SafeNormalize(Vector2.UnitX) * 27;
            dustPos += new Vector2(0f, 1f).RotatedBy(velocity.ToRotation()) * Main.rand.NextFloat(-6f, 6f);
            for (int i = 0; i < 5 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = velocity.SafeNormalize(Vector2.UnitX).RotateRandom(0.5f) * Main.rand.NextFloat(1f, 4f);
                Dust p = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: color, Scale: Main.rand.NextFloat(0.35f, 0.55f) * 1f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.89f, shouldFadeColor: false);
            }

            return true;
        }

    }

}
