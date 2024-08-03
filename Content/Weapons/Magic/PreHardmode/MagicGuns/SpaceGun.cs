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


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class SpaceGun : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SpaceGun);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };

            //TODO: Adjust HoldoutOffset to better match having 1f item scale instead of 0.8f

            entity.scale = 1f;
            //entity.scale = 0.8f;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = .12f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            //SoundEngine.PlaySound(style, player.Center);

            //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Research_1") with { Pitch = .85f, PitchVariance = .2f, Volume = 0.25f };
            //SoundEngine.PlaySound(style2, player.Center);


            //Dust
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<LineSpark>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 15),
                    newColor: Color.Green, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    0.95f, 0.75f); //80

            }

            return true;
        }

    }
    public class SpaceGunShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.GreenLaser);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Flare/CyverLaserPMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -30);
            float drawRot = projectile.rotation - MathHelper.PiOver2;

            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 Tex2Origin = Tex2.Size() / 2f;
             
            float opacity = Easings.easeInCirc(projectile.Opacity);

            Color color1 = Color.Green with { A = 0 } * opacity;
            Color color2 = Color.White with { A = 0 } * opacity;
            Color color3 = Color.LightGreen with { A = 0 } * opacity;

            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.35f, drawRot, Tex2Origin, projectile.scale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.75f, drawRot, Tex2Origin, projectile.scale * 0.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color2 * 0.7f, drawRot, Tex2Origin, projectile.scale * 0.1f, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex, drawPos, null, color3 * 0.7f, drawRot, TexOrigin, new Vector2(1.75f, 0.35f * Easings.easeInCirc(projectile.Opacity)) * projectile.scale, SpriteEffects.None, 0f);

            //Main.spriteBatch.Draw(Tex, drawPos, null, Color.LimeGreen with { A = 0 } * 0.7f, drawRot, TexOrigin, new Vector2(2f, 0.35f * Easings.easeInCirc(projectile.Opacity)) * projectile.scale, SpriteEffects.None, 0f); //0.3

            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-2f, 2f)) * Main.rand.NextFloat(0.5f, 2.2f),
                    newColor: Color.Green, Scale: Main.rand.NextFloat(0.3f, 0.5f));
            }

            base.OnKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Electric hit sound
            SoundStyle style = new SoundStyle("Terraria/Sounds/NPC_Hit_53") with { Volume = .06f, Pitch = 0.95f, PitchVariance = .25f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            //Hit dust
            for (int i = 0; i < 3 + Main.rand.Next(1, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LineSpark>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.Next(5, 15),
                    newColor: Color.Green, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.55f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 5 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Pitch = -.7f, PitchVariance = .25f, MaxInstances = 1, Volume = 0.35f };
            SoundEngine.PlaySound(style, projectile.Center);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
