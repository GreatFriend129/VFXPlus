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
using Terraria.GameContent.UI.BigProgressBar;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class BloodThorn : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SharpTears);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dustStartPos = player.Center + new Vector2(13f * player.direction, -18f);
            for (int i = 0; i < 5 + Main.rand.Next(1, 5); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(dustStartPos, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.5f, .85f) * 2f,
                    newColor: Color.Red * 0.5f, Scale: Main.rand.NextFloat(0.25f, 0.45f) * 2f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 0, preSlowPower: 0.96f, postSlowPower: 0.88f, 
                    velToBeginShrink: 3f, fadePower: 0.89f, shouldFadeColor: false);
            }

            return true;
        }

    }
    public class BloodThornShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SharpTears);
        }

        public override bool PreAI(Projectile projectile)
        {
            return base.PreAI(projectile);
        }


        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + projectile.rotation.ToRotationVector2() * 84f * projectile.scale;
            Rectangle sourceRectangle = vanillaTex.Frame(1, 6, frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 3f; //3f

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.2f * projectile.direction);

                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle, 
                    Color.Crimson with { A = 0 } * 0.4f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1f, SpriteEffects.None);
            }

            //Main.EntitySpriteDraw(vanillaTex, drawPos + new Vector2(0f, 0f), sourceRectangle, Color.Black * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale * 1f, SpriteEffects.None);
            
            return true;

        }
    }

}
