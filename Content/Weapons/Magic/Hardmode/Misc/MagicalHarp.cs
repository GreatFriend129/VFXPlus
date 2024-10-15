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
    
    public class MagicalHarp : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.MagicalHarp);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {



            return true;
        }

    }
    public class MagicalHarpShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            bool a = entity.type == ProjectileID.QuarterNote;
            bool b = entity.type == ProjectileID.EighthNote;
            bool c = entity.type == ProjectileID.TiedEighthNote;

            return lateInstantiation && (a || b || c);
        }

        float fadeInAlpha = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 25; //22
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
                    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity -= projectile.velocity * 0.5f;
            }

            float progress = Math.Clamp((timer + 10) / 50f, 0f, 1f);
            fadeInAlpha = MathHelper.Lerp(0f, 1f, progress);

            /*
            if (timer % 4 == 0 && Main.rand.NextBool() && false)
            {
                Dust grass = Dust.NewDustPerfect(projectile.Center, DustID.JungleGrass, Main.rand.NextVector2Circular(2, 2), 0, Scale: 0.9f);
                grass.velocity += projectile.velocity;
                grass.noGravity = true;
                grass.alpha = 50;
            }
            */

            timer++;

            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            float totalAlpha = Easings.easeOutBack(fadeInAlpha);
            float totalScale = Easings.easeInOutBack(fadeInAlpha, 0f, 2.5f);

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
                    float size = (0.75f + (progress * 0.25f)) * projectile.scale;

                    Color col = Color.Lerp(Color.Purple, Color.MediumPurple, progress) * progress * totalAlpha;// Color.LightGoldenrodYellow * progress * projectile.Opacity;
                    // Color.Lerp(Color.Gold, Color.LightGoldenrodYellow, progress) * progress * projectile.Opacity;

                    float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 2f, //0.5f
                            previousRotations[i], TexOrigin, progress * progress * totalScale, SpriteEffects.None);

                }

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

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;
            
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
            //return base.PreKill(projectile, timeLeft);
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
