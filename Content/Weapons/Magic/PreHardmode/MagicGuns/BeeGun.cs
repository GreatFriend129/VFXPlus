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
            return lateInstatiation && (item.type == ItemID.BeeGun);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.scale = 1f;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Dust
            for (int i = 0; i < 10; i++)
            {
                Color col = Color.Yellow;

                Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 38, DustID.Bee,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                    newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f));

                dp.noGravity = true;

                ///dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    //0.95f, 0.75f);

            }

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_11") with { Volume = .35f, Pitch = 1f, PitchVariance = .15f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style2, player.Center);
            return true;
        }

    }
    public class BeeGunShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Bee);
        }

        public override void AI(Projectile projectile)
        {
            projectile.localAI[1] = Math.Clamp(MathHelper.Lerp(projectile.localAI[1], 1.25f, 0.04f), 0f, 1f);
            base.AI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            float drawRot = projectile.rotation;
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Border
            for (int i = 0; i < 6; i++)
            {
                Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, Color.White with { A = 0 } * 0.75f * projectile.localAI[1], drawRot, TexOrigin, projectile.scale, SE, 0f); //0.3
            }

            Main.spriteBatch.Draw(vanillaTex, drawPos, sourceRectangle, Color.White * projectile.localAI[1], drawRot, TexOrigin, projectile.scale, SE, 0f); //0.3

            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            base.OnKill(projectile, timeLeft);
        }

    }

    public class HivePackBeeOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.GiantBee);
        }

        public override void AI(Projectile projectile)
        {
            projectile.localAI[1] = Math.Clamp(MathHelper.Lerp(projectile.localAI[1], 1.25f, 0.07f), 0f, 1f);
            base.AI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            float drawRot = projectile.rotation;
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Border
            for (int i = 0; i < 6; i++)
            {
                Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle, Color.White with { A = 0 } * 0.75f * projectile.localAI[1], drawRot, TexOrigin, projectile.scale, SE, 0f); //0.3
            }

            Main.spriteBatch.Draw(vanillaTex, drawPos, sourceRectangle, Color.White * projectile.localAI[1], drawRot, TexOrigin, projectile.scale, SE, 0f); //0.3

            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            base.OnKill(projectile, timeLeft);
        }

    }

}
