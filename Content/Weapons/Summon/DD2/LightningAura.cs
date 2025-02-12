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
using VFXPlus.Common.Drawing;
using rail;
using Terraria.Physics;


namespace VFXPlus.Content.Weapons.Summon.DD2
{
    public class LightningAuraT1Override : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.DD2LightningAuraT1);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            

            

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            DrawLightning(projectile);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, -70f);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            for (int i = 20; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f) * 0f;
                Main.EntitySpriteDraw(vanillaTex, offset + drawPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)i) * 2f, sourceRectangle, Color.SkyBlue with { A = 0 } * 1f, projectile.rotation, TexOrigin,
                    projectile.scale * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);


            //Centerpiece
            Texture2D center = TextureAssets.Extra[86].Value;
            Vector2 centerPos = projectile.Center - Main.screenPosition + new Vector2(0f, -3f);


            for (int i = 20; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f) * 0f;
                Main.EntitySpriteDraw(center, centerPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)i) * 2f, null, Color.SkyBlue with { A = 0 } * 1f, projectile.rotation, center.Size() / 2f,
                    projectile.scale * overallScale, SE);
            }

            Main.EntitySpriteDraw(center, centerPos, null, lightColor, projectile.rotation, center.Size() / 2f, projectile.scale * overallScale, SE);

            return false;
        }

        public void DrawLightning(Projectile projectile)
        {
            return;

        }


    }

}
