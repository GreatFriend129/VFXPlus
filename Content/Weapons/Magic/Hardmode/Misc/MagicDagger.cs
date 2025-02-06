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
            int trailCount = 6;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 5 == 0 && Main.rand.NextBool())
            {
                
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.Goldenrod, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity += projectile.velocity * 0.2f;
            }


            timer++;

            return base.PreAI(projectile);
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.LightGoldenrodYellow * progress * projectile.Opacity;

                    float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f,
                            previousRotations[i], TexOrigin, size2, SpriteEffects.None);

                }

            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.Gold with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);
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

            SoundStyle tile_hit = new SoundStyle("Terraria/Sounds/Dig_" + soundVariant2) with { Volume = 1f, Pitch = 0f, PitchVariance = 0f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(tile_hit, projectile.Center);

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
