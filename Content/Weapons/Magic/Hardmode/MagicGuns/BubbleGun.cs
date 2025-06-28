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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class BubbleGun : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BubbleGun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BubbleGunToggle;
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            for (int k = 0; k < 7; k++)
            {
                Vector2 normalizedVel = velocity.SafeNormalize(Vector2.UnitX);

                Vector2 offset = new Vector2(0f, velocity.X > 0 ? -5 : 5).RotatedBy(velocity.ToRotation());
                Vector2 adjustedSpawnPos = (position + normalizedVel * 37f) + offset;

                Vector2 dustVel = velocity.RotateRandom(1f) * Main.rand.NextFloat(0.75f, 1.25f);
                Dust d = Dust.NewDustPerfect(adjustedSpawnPos, DustID.FishronWings, dustVel * 0.4f, 
                    newColor: Color.White, Scale: Main.rand.NextFloat(0.25f, 0.4f) * 3f);

                d.noGravity = true;
            }


            return true;
        }

    }
    public class BubbleGunShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Bubble) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BubbleGunToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                initialVel = projectile.velocity;
                projectile.rotation = projectile.velocity.ToRotation();
            }

            if (projectile.velocity.X > 0)
                projectile.rotation += projectile.velocity.Length() * 0.02f;
            else
                projectile.rotation -= projectile.velocity.Length() * 0.02f;

            if (timer > 1)
                trueAlpha = Math.Clamp(MathHelper.Lerp(trueAlpha, 1.5f, 0.12f), 0f, 1f);

            float progress = Math.Clamp((timer + 5) / 20f, 0f, 1f); //timer / 50
            trueScale = 0.2f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(progress, 0f, 2f)) * 0.8f;

            timer++;
            return base.PreAI(projectile);
        }

        float trueAlpha = 0f;
        float trueScale = 0f;
        Vector2 initialVel = Vector2.Zero;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Bloomball
            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            Main.EntitySpriteDraw(Ball, drawPos + new Vector2(0f, 0f), null, Color.Aqua with { A = 0 } * 0.12f * trueAlpha, projectile.rotation, Ball.Size() / 2f, projectile.scale * 0.4f * trueScale, 0);


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * 1f * trueAlpha, projectile.rotation, TexOrigin, projectile.scale * trueScale, se);
            float glowscale = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 6f) / 5f + 1f) * 1.15f;
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.DarkSeaGreen with { A = 0 } * 0.07f * trueAlpha, projectile.rotation, TexOrigin, projectile.scale * glowscale * trueScale, se);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 5 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: Color.LightSeaGreen * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.3f) * projectile.scale);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(shouldFadeColor: false, fadePower: 0.92f);
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.05f * projectile.scale, true, 1, 0.4f, 0.4f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Aquamarine * 0.25f);
            d1.customData = cpb2;
            d1.velocity = projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.01f;

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Aquamarine * 0.25f);
            d2.customData = cpb2;
            d2.velocity = projectile.velocity.SafeNormalize(Vector2.UnitX) * -0.01f;

            d1.scale = 0.1f;
            d2.scale = 0.1f;

            return base.PreKill(projectile, timeLeft);
        }
    }

}
