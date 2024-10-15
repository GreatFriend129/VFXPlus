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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class UnholyTrident : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.UnholyTrident);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 4 + Main.rand.Next(2); i++)
            {
                /*
                Vector2 v = Main.rand.NextVector2Unit();
                Dust sa = Dust.NewDustPerfect(player.Center + velocity.SafeNormalize(Vector2.UnitX) * 50, ModContent.DustType<GlowPixelCross>(), 
                    velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1f) * Main.rand.NextFloat(2f, 4f), 0,
                    Color.Purple, Main.rand.NextFloat(0.35f, 0.5f));

                sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: false);
                */

                Dust sa = Dust.NewDustPerfect(player.Center + velocity.SafeNormalize(Vector2.UnitX) * 35, ModContent.DustType<MuraLineBasic>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1.8f) * Main.rand.NextFloat(2f, 4f), 255,
                    new Color(42, 2, 82), Main.rand.NextFloat(0.35f, 0.5f) * 0.5f);


            }


            return true;
        }

    }
    public class UnholyTridentShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.UnholyTridentFriendly);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 8; ///12
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

                p.velocity += projectile.velocity * 0.2f;
            }

            if (timer % 2 == 0)
            {
                Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelAlts>(), Velocity: initVel, newColor: Color.White/*new Color(42, 2, 82)*/, Scale: Main.rand.NextFloat(0.85f, 1.15f));

                a.velocity *= 0.25f;
                a.velocity += projectile.velocity * 0.5f;
            }



            /*
            if (timer % 4 == 0 && Main.rand.NextBool() && false)
            {
                Dust grass = Dust.NewDustPerfect(projectile.Center, DustID.JungleGrass, Main.rand.NextVector2Circular(2, 2), 0, Scale: 0.9f);
                grass.velocity += projectile.velocity;
                grass.noGravity = true;
                grass.alpha = 50;
            }
            */

            float fadeInTime = Math.Clamp((timer + 5f) / 15f, 0f, 1f);
            fadeInAlpha = Easings.easeInCirc(fadeInTime);

            timer++;

            return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Utils.DrawBorderString(Main.spriteBatch, "" + fadeInAlpha, new Vector2(0f, -120f) + projectile.Center - Main.screenPosition, Color.White);
            
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

                    Color col = Color.White * progress;
                    // Color.Lerp(Color.Gold, Color.LightGoldenrodYellow, progress) * progress * projectile.Opacity;

                    float size2 = (1f + (progress * 0.25f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f * progress, //0.5f
                            previousRotations[i], TexOrigin, size2 * fadeInAlpha, SpriteEffects.None);


                    if (i > 1)
                    {
                        float middleProg = (float)(i - 1) / previousPostions.Count;
                        Vector2 vec2Scale = new Vector2(0.5f, 0.5f) * size * fadeInAlpha;
                        Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.White with { A = 0 } * 0.85f * middleProg,
                            previousRotations[i], TexOrigin, vec2Scale, SpriteEffects.None);
                    }

                }



            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.Purple with { A = 0 } * 0.75f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * fadeInAlpha, SpriteEffects.None);
            }


            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * fadeInAlpha, SpriteEffects.None);
            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
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
