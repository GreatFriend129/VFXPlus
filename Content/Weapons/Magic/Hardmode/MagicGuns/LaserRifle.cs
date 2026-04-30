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
using System.Threading;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class LaserRifleItemOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.LaserRifle) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LaserRifleToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = .12f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Research_1") with { Pitch = .85f, PitchVariance = .2f, Volume = 0.25f };
            SoundEngine.PlaySound(style2, player.Center);

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.LaserRifle,
                    AnimTime: 18,
                    NormalXOffset: 20f,
                    DestXOffset: 10f,
                    YRecoilAmount: 0.05f,
                    HoldOffset: new Vector2(0f, 2f)
                    );

                //held.compositeArmAlwaysFull = true;
                held.timeToStartFade = 3;
            }


            //Dust
            Color between = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);
            for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++) //2 //0,3
            {
                Vector2 pos = position + velocity.SafeNormalize(Vector2.UnitX) * 30f;

                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<LineSpark>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(6f, 22f),
                    newColor: between * 1f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80

            }

            for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
            {
                Color col1 = Color.Lerp(Color.DeepPink, Color.HotPink, 0.65f);

                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 30f, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.25f, 0.3f) * 1.15f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.93f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 6f;
            }


            Vector2 offsetPos = position + velocity.SafeNormalize(Vector2.UnitX) * 33f;

            Color ringCol = Color.Lerp(Color.HotPink, Color.Purple, 0.5f);

            Vector2 vel = velocity.SafeNormalize(Vector2.UnitX) * 1f; //2.5
            Dust d = Dust.NewDustPerfect(offsetPos, ModContent.DustType<CirclePulse>(), vel, newColor: ringCol * 0.75f);
            d.scale = 0.025f;
            CirclePulseBehavior b = new CirclePulseBehavior(0.05f, true, 2, 0.2f, 0.35f);
            b.drawLayer = "OverPlayers";
            d.customData = b;

            return true;
        }

    }
    public class LaserRifleShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PurpleLaser) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LaserRifleToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer > 2 && timer % 5 == 0 && Main.rand.NextBool(4))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.Lerp(Color.HotPink, Color.DeepPink, 0.25f), Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.45f);
                d.velocity += projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            if (timer % 3 == 0 && Main.rand.NextBool(5) && timer > 2)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                dustVel += projectile.velocity * 0.55f;

                Color dustCol = Color.Lerp(Color.Purple, Color.DeepPink, 0.75f);
                float dustScale = Main.rand.NextFloat(0.5f, 0.6f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCol, Scale: dustScale);
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 0, postSlowPower: 0.89f, velToBeginShrink: 10f, fadePower: 0.92f, shouldFadeColor: false);
                d.rotation = Main.rand.NextFloat(6.28f);
            }

            float timeForPopInAnim = 80;
            float animProgress = Math.Clamp((timer + 40) / timeForPopInAnim, 0f, 1f);
            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2f)) * 1f;

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


            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/FlareLessGlow").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Flare/CyverLaserPMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -46);
            float drawRot = projectile.rotation - MathHelper.PiOver2;

            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 Tex2Origin = Tex2.Size() / 2f;

            float opacity = 1f;// Easings.easeInCirc(projectile.Opacity);

            Color color1 = Color.DeepPink with { A = 50 } * opacity;
            Color color2 = Color.White with { A = 50 } * opacity;
            Color color3 = Color.HotPink with { A = 50 } * opacity;

            Vector2 lineScale = new Vector2(projectile.scale, projectile.scale * 1.15f) * overallScale;
            Vector2 InnerLineScale = new Vector2(2f, 0.4f * opacity) * projectile.scale * overallScale;


            Main.spriteBatch.Draw(Tex2, drawPos + drawRot.ToRotationVector2() * -25f, null, Color.DeepPink * 0.35f, drawRot, Tex2Origin, lineScale * 0.3f, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex2, drawPos + drawRot.ToRotationVector2() * -15f, null, color1 * 0.35f, drawRot, Tex2Origin, lineScale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, color2 * 0.6f, drawRot, Tex2Origin, new Vector2(lineScale.X * 0.12f, lineScale.Y * 0.04f), SpriteEffects.None, 0f);


            Main.spriteBatch.Draw(Tex, drawPos + new Vector2(0f, 0f), null, color3 * 0.7f, drawRot, TexOrigin, InnerLineScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + new Vector2(0f, 0f), null, Color.White with { A = 50 } * 0.6f, drawRot, TexOrigin, new Vector2(InnerLineScale.X * 0.45f, InnerLineScale.Y * 0.5f), SpriteEffects.None, 0f);


            /*
            Texture2D Tex = CommonTextures.Flare.Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Assets/Flare/CyverLaserPMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -36);
            float drawRot = projectile.rotation - MathHelper.PiOver2;

            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 Tex2Origin = Tex2.Size() / 2f;

            Color pinkToUse = Color.Lerp(Color.Purple, Color.DeepPink, 0.1f);

            Vector2 lineScale = new Vector2(projectile.scale, projectile.scale) * overallScale * projectile.scale;

            Main.spriteBatch.Draw(Tex2, drawPos, null, pinkToUse with { A = 0 } * 0.5f, drawRot, Tex2Origin, lineScale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, pinkToUse with { A = 0 }, drawRot, Tex2Origin, lineScale * 0.15f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex2, drawPos, null, Color.White with { A = 0 } * 0.7f, drawRot, Tex2Origin, lineScale * 0.1f, SpriteEffects.None, 0f);

            Vector2 InnerLineScale = new Vector2(2f, 0.35f * projectile.Opacity) * projectile.scale * overallScale;
            Main.spriteBatch.Draw(Tex, drawPos, null, Color.HotPink with { A = 0 } * 0.7f, drawRot, TexOrigin, InnerLineScale, SpriteEffects.None, 0f); //0.3
            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White with { A = 0 } * 0.7f, drawRot, TexOrigin, InnerLineScale * 0.3f, SpriteEffects.None, 0f); //0.3
            */
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color pinkToUse = Color.Lerp(Color.Purple, Color.DeepPink, 0.1f);

            for (int i = 0; i < 4 + Main.rand.Next(1, 3); i++)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-2f, 2f)) * Main.rand.Next(1, 3),
                    newColor: pinkToUse, Scale: Main.rand.NextFloat(0.3f, 0.5f));
            }

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //Electric hit sound
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/NPC_Hit_53") with { Volume = .06f, Pitch = 0.95f, PitchVariance = .25f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            //Hit dust
            for (int i = 0; i < 4 + Main.rand.Next(1, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LineSpark>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.NextFloat(5f, 20f),
                    newColor: Color.DeepPink * 1.5f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.55f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 5 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80
            }

            Color between = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);
            for (int i = 220; i < 2 + Main.rand.Next(1, 3); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LineSpark>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.NextFloat(5f, 20f),
                    newColor: between * 1f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80

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
