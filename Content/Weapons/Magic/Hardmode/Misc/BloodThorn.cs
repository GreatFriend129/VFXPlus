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
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class BloodThorn : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SharpTears) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BloodThornToggle;
        }

        public override void SetDefaults(Item entity)
        {
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dustStartPos = player.MountedCenter + new Vector2(13f * player.direction, -18f);
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
            return lateInstantiation && (entity.type == ProjectileID.SharpTears) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BloodThornToggle;
        }

        public override bool PreAI(Projectile projectile)
        {

            Lighting.AddLight(projectile.Center, Color.Red.ToVector3() * 1.25f);

            return base.PreAI(projectile);
        }


        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + projectile.rotation.ToRotationVector2() * 84f * projectile.scale;
            Rectangle sourceRectangle = vanillaTex.Frame(1, 6, frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color[] cols = { Color.DarkRed * 0.75f, Color.DarkRed * 0.525f, Color.DarkRed * 0.375f };
            float[] scales = { 1.15f, 1.6f, 2.5f };

            float orbRot = projectile.rotation + MathHelper.PiOver2;
            float orbAlpha = 0.2f;
            Vector2 orbScale = new Vector2(0.4f, 1.25f) * 1f * projectile.scale;
            Vector2 orbOrigin = orb.Size() / 2f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[2] * sineScale2, SpriteEffects.None);



            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 2.5f + (MathF.Sin((float)Main.timeForVisualEffects * 0.11f) * 0.5f); //3f

                float colorProg = i * 0.25f;
                Color col = Color.Lerp(Color.DarkRed, Color.Crimson, colorProg);

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.2f * projectile.direction);

                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle, 
                    col with { A = 0 } * 0.4f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1f, SpriteEffects.None);
            }

            //ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            //{
            for (int num163 = 220; num163 < 4; num163++)
            {
                Vector2 offset = projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)num163) * 4f;

                Main.EntitySpriteDraw(vanillaTex, drawPos + offset + Main.rand.NextVector2Circular(1f, 1f), sourceRectangle,
                    Color.Red with { A = 0 } * projectile.Opacity * 0.2f, projectile.rotation, TexOrigin, projectile.scale, 0f);
            }
            //});

            return true;

        }
    }

}
