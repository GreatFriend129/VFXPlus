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
using VFXPlus.Content.Projectiles;
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class ZapinatorItemOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && ((item.type == ItemID.ZapinatorGray) || (item.type == ItemID.ZapinatorOrange)) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ZapinatorToggle;
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
                    GunID: item.type,
                    AnimTime: 22,
                    NormalXOffset: 20f,
                    DestXOffset: 1f,
                    YRecoilAmount: 0.2f,
                    HoldOffset: new Vector2(0f, 3f)
                    );

                //held.compositeArmAlwaysFull = true;
                held.timeToStartFade = 3;
            }


            //Dust
            for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++)
            {
                Vector2 pos = position + velocity.SafeNormalize(Vector2.UnitX) * 26f;
                Vector2 vel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(3.2f, 8.5f);

                Dust dp = Dust.NewDustPerfect(pos + vel, ModContent.DustType<MuraLineBasic>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.6f);
                dp.alpha = 10 + Main.rand.Next(-2, 5);

                dp.customData = new MuraLineBehavior(new Vector2(0.65f, 1f), VelFadeSpeed: Main.rand.NextFloat(0.92f, 0.96f));
            }

            for (int i = 0; i < 4 + Main.rand.Next(0, 3); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 30f, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.25f, 0.3f) * 1.15f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.93f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 6f;
            }


            Vector2 offsetPos = position + velocity.SafeNormalize(Vector2.UnitX) * 15f;

            Vector2 ringVel = velocity.SafeNormalize(Vector2.UnitX) * 2f; //2.5
            Dust d = Dust.NewDustPerfect(offsetPos, ModContent.DustType<CirclePulse>(), ringVel, newColor: Color.DodgerBlue * 0.75f);
            d.scale = 0.025f;
            CirclePulseBehavior b = new CirclePulseBehavior(0.05f, true, 2, 0.2f, 0.35f);
            b.drawLayer = "OverPlayers";
            d.customData = b;

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
            if (timer > 2 && timer % 7 == 0 && Main.rand.NextBool(1))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixel>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 1f);
                d.velocity += projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }


            if (timer % 7 == 0 && Main.rand.NextBool(2) && timer > 2)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                dustVel += projectile.velocity * 0.55f;

                float dustScale = Main.rand.NextFloat(0.5f, 0.6f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: Color.DodgerBlue, Scale: dustScale);
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, postSlowPower: 0.89f, velToBeginShrink: 10f, fadePower: 0.89f, shouldFadeColor: false);
                d.rotation = Main.rand.NextFloat(6.28f);
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


            float sineScale = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.30f) * 0.08f;
            float sineScale2 = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.18f) * 0.04f;

            Vector2 lineScale = new Vector2(projectile.scale, projectile.scale * sineScale2) * overallScale * sineScale;
            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.35f, drawRot, Tex2Origin, lineScale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color1 * 0.75f, drawRot, Tex2Origin, lineScale * 0.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color2 * 0.9f, drawRot, Tex2Origin, lineScale * 0.1f, SpriteEffects.None, 0f);

            Vector2 InnerLineScale = new Vector2(2.1f, 0.4f * Easings.easeInCirc(projectile.Opacity) * sineScale) * projectile.scale * sineScale2;
            Main.spriteBatch.Draw(Tex, drawPos, null, color3 * 0.9f, drawRot, TexOrigin, InnerLineScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White with { A = 0 } * 0.7f, drawRot, TexOrigin, InnerLineScale * 0.4f, SpriteEffects.None, 0f);

            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 4f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.65f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            for (int h = 0; h < 3 + Main.rand.Next(0, 2); h++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-2f, 2f)) * Main.rand.NextFloat(1f, 3f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixel>(), dustVel, newColor: Color.DodgerBlue, Alpha: 70);
            }


            base.OnKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Electric hit sound
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/NPC_Hit_53") with { Volume = .08f, Pitch = 0.95f, PitchVariance = .25f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            //Hit dust
            if (projectile.penetrate > 1)
            {
                for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
                {
                    Vector2 vel = -projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.NextFloat(2f, 10f);

                    Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineBasic>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.35f, 0.65f) * 0.65f);
                    dp.alpha = 10 + Main.rand.Next(-2, 5);

                    dp.customData = new MuraLineBehavior(new Vector2(1f, 1f), VelFadeSpeed: Main.rand.NextFloat(0.94f, 0.97f));
                }

                for (int i = 0; i < 2 + Main.rand.Next(0, 3); i++)
                {
                    Vector2 vel = -projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1.5f, 1.5f)) * Main.rand.NextFloat(0.5f, 4f);

                    Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixel>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.5f, 0.75f));
                    p.alpha = 2;
                    //p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, fadePower: 0.93f, shouldFadeColor: false);
                }
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
