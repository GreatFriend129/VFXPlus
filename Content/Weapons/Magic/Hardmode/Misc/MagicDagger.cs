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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class MagicDaggerShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MagicDagger) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.MagicDaggerToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 6; //6
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
                    newColor: Color.Goldenrod, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity += projectile.velocity * 0.2f;
            }

            float fadeInTime = Math.Clamp((timer + 10f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.15f);

            timer++;

            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Orb
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Color[] cols = { Color.Yellow * 0.75f, Color.Gold * 0.525f, Color.Orange * 0.375f };
            float[] scales = { 0.85f, 1.6f, 2.5f };

            float orbRot = projectile.rotation + MathHelper.PiOver2;
            float orbAlpha = 0.1f;
            Vector2 orbScale = new Vector2(0.15f, 0.1f) * overallScale * 1.25f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[2] * sineScale2, SpriteEffects.None);



            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.LightGoldenrodYellow * progress * projectile.Opacity;

                float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f,
                        previousRotations[i], TexOrigin, size2 * overallScale, SpriteEffects.None); //0.5f

            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.Gold with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);
            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 9 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.Gold * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);
            }

            int soundVariant1 = Main.rand.Next(3);
            int soundVariant2 = Main.rand.Next(3);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_" + soundVariant1) with { Volume = 0.2f, Pitch = .05f, PitchVariance = .25f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style, projectile.Center);

            SoundEngine.PlaySound(SoundID.Dig, projectile.Center);

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }
    }

}
