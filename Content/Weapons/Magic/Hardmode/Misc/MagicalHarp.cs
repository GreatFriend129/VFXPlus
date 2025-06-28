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
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class MagicalHarpShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            bool a = entity.type == ProjectileID.QuarterNote;
            bool b = entity.type == ProjectileID.EighthNote;
            bool c = entity.type == ProjectileID.TiedEighthNote;

            return lateInstantiation && (a || b || c) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.MagicHarpToggle;
        }

        float fadeInAlpha = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 25;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 5 == 0 && Main.rand.NextBool())
            {   
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.MediumPurple, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity -= projectile.velocity * 0.5f;
            }

            float progress = Math.Clamp((timer + 9) / 45f, 0f, 1f);
            fadeInAlpha = MathHelper.Lerp(0f, 1f, progress);

            timer++;
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float totalAlpha = Easings.easeOutBack(fadeInAlpha);
            float totalScale = Easings.easeInOutBack(fadeInAlpha, 0f, 2.5f);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //Orb
            Color purp = new Color(105, 63, 191);

            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color col1 = new Color(105, 63, 191) * 1f;
            Color col2 = new Color(77, 43, 130) * 1f;
            Color col3 = new Color(52, 21, 101) * 1f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            float scale = 0.25f * totalScale;

            Main.EntitySpriteDraw(orb, drawPos, null, col1 with { A = 0 } * totalAlpha * 0.3f, 0f, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, col2 with { A = 0 } * totalAlpha * 0.3f, 0f, orb.Size() / 2f, scale2 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, col3 with { A = 0 } * totalAlpha * 0.3f, 0f, orb.Size() / 2f, scale3 * scale, SpriteEffects.None);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.Lerp(Color.Purple, Color.MediumPurple, progress) * progress * totalAlpha;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 2f, //0.5f
                        previousRotations[i], TexOrigin, progress * progress * totalScale, SpriteEffects.None);

            }

            //Border
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.White with { A = 0 } * 1.5f * totalAlpha, projectile.rotation, TexOrigin, projectile.scale * 1.1f * totalScale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * totalAlpha, projectile.rotation, TexOrigin, projectile.scale * totalScale, SpriteEffects.None);
            return false;

        }
    }
}
