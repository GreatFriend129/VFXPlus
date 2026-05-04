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
using static tModPorter.ProgressUpdate;
using System.Runtime.Intrinsics.Arm;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Bows
{
    
    public class TheBeesKnees : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BeesKnees);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 6 + Main.rand.Next(0, 4); i++)
            {
                if (Main.rand.NextBool())
                {
                    Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);

                    Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 15, ModContent.DustType<GlowPixelAlts>(),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 5),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.35f);

                    dp.noGravity = true;
                }
                else
                {
                    Color col = Color.Black;

                    Dust dp = Dust.NewDustPerfect(position + velocity.SafeNormalize(Vector2.UnitX) * 15, DustID.Bee,
                        velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.Next(2, 6),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.25f);

                    dp.noGravity = true;
                }

            }
            return true;

        }

    }

    public class BeesKneesShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BeeArrow);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 10;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 9f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.08f), 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width / 2f, vanillaTex.Height / 8f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = between * progress * overallAlpha;

                float size2 = Easings.easeOutCubic(progress) * projectile.scale * overallScale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.3f,
                        previousRotations[i], TexOrigin, size2, SpriteEffects.None);

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    between with { A = 0 } * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * 1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 6 + Main.rand.Next(0, 4); i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f);
                if (Main.rand.NextBool())
                {
                    Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);

                    Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel * Main.rand.NextFloat(1.5f, 2f),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.4f);

                    dp.noGravity = true;
                }
                else
                {
                    Color col = Color.Black;

                    Dust dp = Dust.NewDustPerfect(projectile.Center, DustID.Bee, vel * Main.rand.NextFloat(1.5f, 2.5f),
                        newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.3f);

                    dp.noGravity = true;
                }

            }

            return true;
        }

    }

}
