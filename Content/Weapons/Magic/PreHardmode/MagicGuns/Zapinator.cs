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
using System.Threading;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class Zapinator : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && ((item.type == ItemID.ZapinatorGray) || (item.type == ItemID.ZapinatorOrange)) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ZapinatorToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.scale = 1f;
            base.SetDefaults(entity); 
        }

        public override Vector2? HoldoutOffset(int type)
        {
            return new Vector2(-2f, 1f);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Dust
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Vector2 spawnOffset = new Vector2(0f, velocity.X > 0 ? -5f : 5).RotatedBy(velocity.ToRotation());
                Vector2 spawnPos = position + (velocity.SafeNormalize(Vector2.UnitX) * 40f) + spawnOffset;

                Dust dp = Dust.NewDustPerfect(spawnPos, ModContent.DustType<MuraLineBasic>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(1.5f, 6f),
                    newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.4f, 0.65f) * 0.3f);

                dp.alpha = 10;
            }

            return true;
        }

    }
    public class ZapinatorShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ZapinatorLaser) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ZapinatorToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            bool checkA = (timer % 4 == 0 && Main.rand.NextBool(1));

            if (checkA && timer != 0)
            {
                float scale = Main.rand.NextFloat(0.25f, 0.5f);
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<GlowPixel>(), newColor: Color.DodgerBlue, Scale: scale);
                Main.dust[a].velocity *= 0.75f;
                Main.dust[a].velocity += projectile.velocity * 0.35f;
            }

            float timeForPopInAnim = 60;
            float animProgress = Math.Clamp((timer + 20) / timeForPopInAnim, 0f, 1f);
            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2.5f)) * 1f;

            timer++;
            return true;
        }

        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D Tex = CommonTextures.Flare.Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Flare/CyverLaserPMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -30);
            float drawRot = projectile.rotation - MathHelper.PiOver2;

            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 Tex2Origin = Tex2.Size() / 2f;

            float opacity = 1f;

            Color color1 = Color.DodgerBlue with { A = 0 } * opacity;
            Color color2 = Color.White with { A = 0 } * opacity;
            Color color3 = Color.DeepSkyBlue with { A = 0 } * opacity;


            //Main.spriteBatch.Draw(Tex, drawPos, null, Color.Black * 0.7f, drawRot, TexOrigin, new Vector2(2.1f, 0.4f * Easings.easeInCirc(projectile.Opacity)) * projectile.scale * 0.9f, SpriteEffects.None, 0f);

            Vector2 lineScale = new Vector2(projectile.scale, projectile.scale) * overallScale;
            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.35f, drawRot, Tex2Origin, lineScale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.75f, drawRot, Tex2Origin, lineScale * 0.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color2 * 0.9f, drawRot, Tex2Origin, lineScale * 0.1f, SpriteEffects.None, 0f);

            Vector2 InnerLineScale = new Vector2(2.1f, 0.4f * Easings.easeInCirc(projectile.Opacity)) * projectile.scale;
            Main.spriteBatch.Draw(Tex, drawPos, null, color3 * 0.9f, drawRot, TexOrigin, InnerLineScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White with { A = 0 } * 0.7f, drawRot, TexOrigin, InnerLineScale * 0.4f, SpriteEffects.None, 0f);

            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++)
            {
                Dust p2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<GlowPixel>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-3f, 3f)) * Main.rand.NextFloat(0.1f, 1.75f),
                    newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.65f, 1f));
            }

            base.OnKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Electric hit sound
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/NPC_Hit_53") with { Volume = .08f, Pitch = 0.95f, PitchVariance = .25f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            //Hit dust
            for (int i = 0; i < 5 + Main.rand.Next(1, 6); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineBasic>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.Next(5, 15),
                    newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.55f);

                dp.velocity *= 0.5f;
                dp.alpha = 20;
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_40") with { Pitch = -.7f, PitchVariance = .25f, MaxInstances = 1, Volume = 0.35f };
            SoundEngine.PlaySound(style, projectile.Center);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
