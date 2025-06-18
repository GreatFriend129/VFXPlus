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


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Misc
{
    public class WeatherPainShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WeatherPainShot) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.WeatherPain;
        }


        float overallScale = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 5;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f);

            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress));

            timer++;
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue * 0.1f, progress) * progress * projectile.Opacity;

                float size2 = (1f + (progress * 0.25f)) * projectile.scale * overallScale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f,
                        previousRotations[i], TexOrigin, size2, SpriteEffects.None);
            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float sineMult = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.11f) * 0.033f;

                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.SkyBlue with { A = 0 } * 0.5f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale * sineMult, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);
            return false;

        }
    }

}
