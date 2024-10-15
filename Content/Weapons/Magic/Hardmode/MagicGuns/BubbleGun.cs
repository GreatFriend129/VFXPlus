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
            return lateInstatiation && (item.type == ItemID.BubbleGun);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/JuniorShot") with { Volume = .18f, Pitch = .4f, PitchVariance = .25f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, position);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_154") with { Volume = 0.5f, Pitch = .55f, PitchVariance = .25f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style2, position);

            for (int k = 0; k < 7; k++)
            {
                Vector2 normalizedVel = velocity.SafeNormalize(Vector2.UnitX);

                Vector2 offset = new Vector2(0f, velocity.X > 0 ? -5 : 5).RotatedBy(velocity.ToRotation());
                Vector2 adjustedSpawnPos = (position + normalizedVel * 37f) + offset;

                Vector2 dustVel = velocity.RotateRandom(1f) * Main.rand.NextFloat(0.75f, 1.25f);
                Dust d = Dust.NewDustPerfect(adjustedSpawnPos, DustID.FishronWings, dustVel * 0.4f, 
                    newColor: Color.White, Scale: Main.rand.NextFloat(0.25f, 0.4f) * 3f);

                d.noGravity = true;

                //d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(colorFadePower: 0.88f);
            }


            return true;
        }

    }
    public class BubbleGunShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Bubble);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                initialVel = projectile.velocity;

            projectile.rotation = projectile.velocity.ToRotation();

            timer++;
            return base.PreAI(projectile);
        }

        Vector2 initialVel = Vector2.Zero;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects se = projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            float easeProgress = projectile.velocity.Length() / initialVel.Length();
            float adjustedEaseProgress = Math.Clamp(easeProgress * 0.65f, 0f, 1f);

            float easeA = Math.Clamp(projectile.velocity.Length() * 0.15f, 0f, 2f); //2f to 1f
            float easeB = Math.Clamp(projectile.velocity.Length() * 0.05f, 0f, 0.8f); //0.8 to 0


            float xScale = 1f + (easeA * Easings.easeInOutQuad(adjustedEaseProgress) * 1.75f);
            float yScale = 1f - (easeB * Easings.easeInOutQuad(adjustedEaseProgress) * 2.1f);

            //Utils.DrawBorderString(Main.spriteBatch, "" + adjustedEaseProgress, projectile.Center - Main.screenPosition, Color.White);

            //Bubble is larger at start
            float extraScale = MathHelper.Lerp(1.5f, 1f, Easings.easeOutCirc(1f - easeProgress));
            Vector2 bubbleScale = new Vector2(xScale, yScale) * projectile.scale * extraScale;

            //UnderGlow (starts bigger than bubble)
            float glowScale = MathHelper.Lerp(2f, 1f, Easings.easeInOutCubic(1f - easeProgress));
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.DeepSkyBlue with { A = 0 } * projectile.Opacity * 0.2f * (1f - easeProgress), projectile.rotation, TexOrigin, bubbleScale * glowScale, se);

            //Normal Bubble
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, bubbleScale, se);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 5 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3, 3);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: Color.LightSeaGreen * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.3f) * projectile.scale);
            }

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
