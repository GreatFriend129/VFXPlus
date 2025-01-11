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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class TomeOfInfiniteWisdom : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BookStaff);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int dustCount = player.altFunctionUse == 2 ? 12 : 3;

            for (int i = 0; i < dustCount + Main.rand.Next(2); i++)
            {
                Vector2 posOff = Main.rand.NextVector2Circular(1.5f, 1.5f);

                if (player.altFunctionUse == 2)
                {
                    Color col = Main.rand.NextBool(4) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;

                    Vector2 velOff = Main.rand.NextVector2Circular(1f, 3.5f).RotatedBy(velocity.ToRotation());

                    Dust sa = Dust.NewDustPerfect(position + posOff + velocity.SafeNormalize(Vector2.UnitX) * 44f, ModContent.DustType<GlowPixelCross>(),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.9f) * Main.rand.NextFloat(0.75f, 3f) + velOff, 0,
                        col * 0.5f, Main.rand.NextFloat(0.35f, 0.5f) * 0.7f);

                    sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: false);
                }
                else
                {
                    Color col = Main.rand.NextBool(2) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;
                    Vector2 v = Main.rand.NextVector2Circular(1.5f, 1.5f);

                    Dust sa = Dust.NewDustPerfect(position + v + velocity.SafeNormalize(Vector2.UnitX) * 44f, ModContent.DustType<GlowPixelCross>(),
                        velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.9f) * Main.rand.NextFloat(1f, 4f), 0,
                        col * 1f, Main.rand.NextFloat(0.35f, 0.5f) * 0.65f);

                    sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: false);
                }


            }

            //Main.NewText(player.altFunctionUse);

            return true;
        }

    }
    public class ToIWShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BookStaffShot);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //The vanilla projectile starts at 255 alpha
            //It randomizes the frame if alpha = 255 
            //It creates dust and and sets alpha to 0 if alpha == 255

            //We dont want it to create the dust, so we will set alpha to 0 and randomize the frame ourselves
            projectile.alpha = 0;
            if (timer == 0)
            {
                projectile.frame = Main.rand.Next(2) * 4;
            }

            int trailCount = 14; //8
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 3 == 0 && Main.rand.NextBool() && timer > 8)
            {
                Color col = Main.rand.NextBool(2) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(2f, 4f) * -1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.15f, 0.2f) * 1.5f);
            }

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
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawAE(projectile);
            });

            //Border
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * 0.7f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * fadeInAlpha, SpriteEffects.None);
            }


            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * fadeInAlpha, SpriteEffects.None);

            return false;
        }

        public void DrawAE(Projectile projectile)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                Texture2D trailLine = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;


                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.White * progress;

                    float size2 = (0.5f + (progress * 0.5f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.35f * progress, //0.5f
                            previousRotations[i], TexOrigin, size2 * fadeInAlpha, SpriteEffects.None);


                    Color innerCol = Color.Lerp(Color.LightSkyBlue, Color.Tan, 1f);

                    Vector2 vec2Scale = new Vector2(0.55f, 0.35f * progress) * projectile.scale;
                    Main.EntitySpriteDraw(trailLine, AfterImagePos, null, innerCol with { A = 0 } * 0.35f * progress,
                        projectile.velocity.ToRotation(), trailLine.Size() / 2f, vec2Scale, SpriteEffects.None);

                }
            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 5 + Main.rand.Next(1, 4); i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) : Color.Tan;

                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: col * 1f, Scale: Main.rand.NextFloat(0.35f, 0.45f) * projectile.scale);
                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(velToBeginShrink: 2f, shouldFadeColor: false, fadePower: 0.9f);
            }

            //Vanilla code but without dust (still has gore):
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            SoundEngine.PlaySound(in SoundID.Item10, projectile.position);
            int num693 = Main.rand.Next(6, 12);
            for (int num694 = 220; num694 < num693; num694++)
            {
                int num695 = Dust.NewDust(projectile.Center, 0, 0, 15, 0f, 0f, 100);
                Dust dust236 = Main.dust[num695];
                Dust dust3 = dust236;
                dust3.velocity *= 1.6f;
                Main.dust[num695].velocity.Y -= 1f;
                dust236 = Main.dust[num695];
                dust3 = dust236;
                dust3.velocity += -projectile.velocity * (Main.rand.NextFloat() * 2f - 1f) * 0.5f;
                Main.dust[num695].scale = 1f;
                Main.dust[num695].fadeIn = 1.5f;
                Main.dust[num695].noGravity = true;
                Main.dust[num695].color = new Color(255, 255, 255, 0) * 0.3f;
                dust236 = Main.dust[num695];
                dust3 = dust236;
                dust3.velocity *= 0.7f;
                dust236 = Main.dust[num695];
                dust3 = dust236;
                dust3.position += Main.dust[num695].velocity * 5f;
            }
            for (int num696 = 0; num696 < 3; num696++)
            {
                Gore gore33 = Gore.NewGoreDirect(null, projectile.position, Vector2.Zero, 1008, 1f + Main.rand.NextFloatDirection() * 0.2f);
                Gore gore34 = gore33;
                Gore gore3 = gore34;
                gore3.velocity *= 4f;
            }

            return false;
        }

    }

    public class ToIWTornadoOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.DD2ApprenticeStorm);
        }

        float glowAlpha = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            glowAlpha = Math.Clamp(glowAlpha + 0.03f, 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {

            return true;
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            Texture2D bloom = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;

            Main.EntitySpriteDraw(bloom, projectile.Center - Main.screenPosition, null, Color.Tan with { A = 0 } * 0.2f * glowAlpha, projectile.rotation, bloom.Size() / 2f, 5f * new Vector2(0.65f, 1.5f), 0, 0);

        }

    }

}
