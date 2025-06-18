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
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class SpaceGun : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SpaceGun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SpaceGunToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //TODO: Adjust HoldoutOffset to better match having 1f item scale instead of 0.8f
            entity.scale = 1f;
            base.SetDefaults(entity); 
        }

        public override Vector2? HoldoutOffset(int type)
        {
            return new Vector2(-2f, 0f);

            return base.HoldoutOffset(type);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            Color betweenGreen = Color.Lerp(Color.LawnGreen, Color.Green, 0.75f);
            //Dust
            for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++) //2 //0,3
            {
                Vector2 pos = position + velocity.SafeNormalize(Vector2.UnitX) * 30f;
                Vector2 vel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(2f, 8.5f);

                Dust dp = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MuraLineBasic>(), vel, newColor: betweenGreen, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.6f);
                dp.alpha = 10 + Main.rand.Next(-2, 5);

                dp.customData = new MuraLineBehavior(new Vector2(1f, 1f), VelFadeSpeed: Main.rand.NextFloat(0.94f, 0.97f));
            }

            return true;
        }

    }
    public class SpaceGunShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.GreenLaser) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SpaceGunToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer > 2 && timer % 5 == 0 && Main.rand.NextBool())
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.Green, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.45f);
                d.alpha = 2;
                d.velocity += projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            float timeForPopInAnim = 75;
            float animProgress = Math.Clamp((timer + 25) / timeForPopInAnim, 0f, 1f);
            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2.5f)) * 1f;

            timer++;
            return base.PreAI(projectile);
        }

        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                Draw(projectile, true);
            });
            Draw(projectile, false);

            return false;

        }

        public void Draw(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D Tex = CommonTextures.Flare.Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Flare/CyverLaserPMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -30);
            float drawRot = projectile.rotation - MathHelper.PiOver2;

            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 Tex2Origin = Tex2.Size() / 2f;

            float opacity = 1f;// Easings.easeInCirc(projectile.Opacity);

            Color color1 = Color.Green with { A = 0 } * opacity;
            Color color2 = Color.White with { A = 0 } * opacity;
            Color color3 = Color.LawnGreen with { A = 0 } * opacity;

            Vector2 lineScale = new Vector2(projectile.scale * 0.75f, projectile.scale) * overallScale;

            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.35f, drawRot, Tex2Origin, lineScale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.75f, drawRot, Tex2Origin, lineScale * 0.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color2 * 0.7f, drawRot, Tex2Origin, lineScale * 0.1f, SpriteEffects.None, 0f);

            Vector2 InnerLineScale = new Vector2(1.65f, 0.3f * opacity) * projectile.scale * overallScale;

            Main.spriteBatch.Draw(Tex, drawPos, null, color3 * 0.7f, drawRot, TexOrigin, InnerLineScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White with { A = 0 } * 0.7f, drawRot, TexOrigin, InnerLineScale * 0.4f, SpriteEffects.None, 0f);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-2f, 2f)) * Main.rand.NextFloat(0.5f, 2.2f),
                    newColor: Color.Green, Scale: Main.rand.NextFloat(0.3f, 0.5f));
            }
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Electric hit sound
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/NPC_Hit_53") with { Volume = .06f, Pitch = 0.95f, PitchVariance = .25f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            //Hit dust
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Vector2 vel = -projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.NextFloat(2f, 10f);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineBasic>(), vel, newColor: Color.LimeGreen, Scale: Main.rand.NextFloat(0.35f, 0.65f) * 0.65f);
                dp.alpha = 10 + Main.rand.Next(-2, 5);
            }

            for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++)
            {
                Vector2 vel = -projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1.5f, 1.5f)) * Main.rand.NextFloat(0.75f, 4f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: Color.LimeGreen, Scale: Main.rand.NextFloat(0.15f, 0.35f));

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, fadePower: 0.93f, shouldFadeColor: false);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + oldVelocity, oldVelocity, projectile.width, projectile.height);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_40") with { Pitch = -.7f, PitchVariance = .25f, MaxInstances = 1, Volume = 0.35f };
            SoundEngine.PlaySound(style, projectile.Center);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
