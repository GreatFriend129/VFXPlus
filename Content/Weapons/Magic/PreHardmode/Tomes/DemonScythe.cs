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


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Tomes
{
    
    public class DemonScythe : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.DemonScythe) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.DemonScytheToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle stylees = new SoundStyle("Terraria/Sounds/Item_117") with { Pitch = .45f, PitchVariance = .25f, Volume = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(stylees, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Custom/dd2_book_staff_cast_0") with { Volume = 0.3f, Pitch = -0.25f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_8") with { Volume = 0.85f, PitchVariance = 0.1f, Pitch = .05f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class DemonScytheShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.DemonScythe) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.DemonScytheToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            if (timer % 2 == 0 && Main.rand.NextBool())
            {
                Vector2 offset2 = projectile.rotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10, 10);
                Vector2 vel2 = projectile.rotation.ToRotationVector2().RotatedByRandom(0.15f) * Main.rand.NextFloat(2, 7);

                Dust d2 = Dust.NewDustPerfect(projectile.Center + offset2, ModContent.DustType<GlowPixelCross>(), vel2, newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.25f, 1.5f) * 0.25f);
                d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 3, postSlowPower: 0.9f, velToBeginShrink: 1.75f, fadePower: 0.93f, shouldFadeColor: false);

                d2.velocity += projectile.velocity * 0.25f;
                d2.velocity *= 0.5f;
            }

            float progress = Math.Clamp(timer / 50f, 0f, 1f);
            fadeInAlpha = MathHelper.Lerp(0f, 1f, progress);

            timer++;


            #region vanillaAI
            projectile.rotation += (float)projectile.direction * 0.8f;
            projectile.ai[0] += 1f;
            if (!(projectile.ai[0] < 30f))
            {
                if (projectile.ai[0] < 100f)
                {
                    projectile.velocity *= 1.06f;
                }
                else
                {
                    projectile.ai[0] = 200f;
                }
            }
            for (int num262 = 0; num262 < 2; num262++)
            {
                int num263 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Shadowflame, 0f, 0f, 100);
                Main.dust[num263].noGravity = true;
                Main.dust[num263].velocity += projectile.velocity * 0.25f;

            }
            #endregion

            return false;

        }

        float fadeInAlpha = 0f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D pixelSwirl = CommonTextures.PixelSwirl.Value;
            Texture2D star = CommonTextures.RainbowRod.Value;


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            Color newPurple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);


            //Pixel Swirl
            Main.EntitySpriteDraw(pixelSwirl, drawPos, null, darkPurple with { A = 0 } * fadeInAlpha * 1.5f, projectile.rotation, pixelSwirl.Size() / 2f, projectile.scale * 0.75f, SpriteEffects.None);

            //Main tex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);


            //Star
            Vector2 starPos = drawPos;

            float starScale = MathHelper.Lerp(1.5f, 0f, Easings.easeOutQuad(fadeInAlpha)) * projectile.scale;

            Main.EntitySpriteDraw(star, starPos, null, darkPurple with { A = 0 } * 2f * (1f - fadeInAlpha), projectile.rotation, star.Size() / 2f, starScale * 1.4f, SpriteEffects.None);
            Main.EntitySpriteDraw(star, starPos, null, Color.MediumPurple with { A = 0 } * 1.25f * (1f - fadeInAlpha), projectile.rotation, star.Size() / 2f, starScale * 0.65f, SpriteEffects.None);

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = MathHelper.Lerp(60f, 4f, fadeInAlpha);
                float scale = MathHelper.Lerp(2.2f, 1.1f, Easings.easeInBack(fadeInAlpha)); //2.25
                float alpha = Easings.easeInCubic(fadeInAlpha);

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);

                Main.EntitySpriteDraw(vanillaTex, drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.03f * projectile.direction), sourceRectangle,
                    newPurple with { A = 0 } * alpha * 1.25f, projectile.rotation, TexOrigin, projectile.scale * scale, SpriteEffects.None);
            }


            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_43") with { Volume = .25f, Pitch = -.32f, PitchVariance = .15f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_60") with { Volume = .15f, Pitch = .15f, PitchVariance = .12f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style2, projectile.Center);

            SoundStyle styleNormal = new SoundStyle("Terraria/Sounds/Item_10") with { Volume = 0.8f, Pitch = .15f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(styleNormal, projectile.Center);


            for (int i = 0; i < 30 + Main.rand.Next(1, 7); i++)
            {
                int a = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Shadowflame, Alpha: 100, Scale: projectile.scale);

                Main.dust[a].velocity += projectile.velocity * 0.25f;
                Main.dust[a].noGravity = true;
            }

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            for (int i = 0; i < 7 + Main.rand.Next(1, 7); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Vector2 spawnOffset = Main.rand.NextVector2Circular(15f,15f);

                Dust p = Dust.NewDustPerfect(projectile.Center + spawnOffset, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.75f, 1.05f),
                    newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.15f, 0.45f) * projectile.scale);
                p.velocity += oldVelocity * Main.rand.NextFloat(0.25f, 0.51f);
            }

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
