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
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Weapons.Ranged.Hardmode.Bows;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Bows
{
    
    public class BloodRainBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BloodRainBow);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            return true;
        }
    }

    public class BloodRainBowShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BloodArrow);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                Vector2 portalVel = projectile.velocity.SafeNormalize(Vector2.UnitX) * 3f;
                int portal = Projectile.NewProjectile(null, projectile.Center, portalVel, ModContent.ProjectileType<BloodRainBowVFX>(), 0, 0, Main.myPlayer);
                Main.projectile[portal].rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            
            //VERY IMPORTANT
            projectile.hide = false;

            int trailCount = 24;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 3 == 0)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f).RotatedBy(projectile.velocity.ToRotation());
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.Red, Scale: Main.rand.NextFloat(0.55f, 0.8f) * 0.8f);
                d.velocity += projectile.velocity * 0.5f;

                d.customData = new GlowFlareBehavior(GlowThreshold: 0.6f, GlowPower: 2.5f, TotalBoost: 1f);
            }

            float fadeInTime = Math.Clamp((timer + 12f) / 55f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.25f);

            timer++;

            if (timer % 1 == 0)
            {
                Vector2 dustPos = projectile.Center;
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Blood, dustVel, Scale: 1f);
                d.velocity += projectile.velocity * 0.1f;

                if (Main.rand.NextBool())
                {
                    Vector2 dustVel2 = Main.rand.NextVector2Circular(0.75f, 0.75f);
                    Dust d2 = Dust.NewDustPerfect(dustPos, DustID.Blood, dustVel, Scale: 0.75f);
                    d2.velocity += projectile.velocity * 0.1f;
                }
            }

            #region vanillaAI
            projectile.ai[0] += 1f;

            if (projectile.ai[0] >= 15f)
            {
                projectile.ai[0] = 15f;

                projectile.velocity.Y += 0.1f;
            }

            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            Lighting.AddLight(projectile.Center, 0.3f, 0.05f, 0.05f);
            #endregion

            return false;
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D flare = CommonTextures.Flare.Value;
            Texture2D flare2 = CommonTextures.AnotherLineGlow.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = flare.Size() / 2f;

            float drawScale = projectile.scale * overallScale;
            Color darkerRed = new Color(103, 0, 0);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {

                //After-Image
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    //Start End
                    Color col = Color.Lerp(Color.DarkRed, darkerRed, progress) * progress;

                    Vector2 size = new Vector2(1f, 0.3f * progress) * drawScale;

                    Main.EntitySpriteDraw(flare, AfterImagePos, null, col * progress * 2f,
                        previousRotations[i], TexOrigin, size, SpriteEffects.None);

                    //SIZE 2 IS REAL
                    Vector2 size2 = new Vector2(0.7f, 3f * progress) * drawScale;

                    Main.EntitySpriteDraw(flare2, AfterImagePos, null, Color.Red with { A = 0 } * 0.11f * progress,
                        previousRotations[i], flare2.Size() / 2f, size2, SpriteEffects.None);
                }

            });

            Main.EntitySpriteDraw(flare, projectile.Center - Main.screenPosition, null, Color.Red * 1f, projectile.velocity.ToRotation(), TexOrigin, new Vector2(1f, 0.75f), SpriteEffects.None);
            Main.EntitySpriteDraw(flare, projectile.Center - Main.screenPosition, null, new Color(255, 175, 175) with { A = 0 } * 0f, projectile.velocity.ToRotation(), TexOrigin, 0.55f, SpriteEffects.None);

            return false;
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 9 + Main.rand.Next(0, 5); i++)
            {

                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f).RotatedBy(projectile.velocity.ToRotation());
                Dust d = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, projectile.height / 2f), ModContent.DustType<GlowFlare>(), vel, newColor: Color.Red, Scale: Main.rand.NextFloat(0.45f, 0.8f) * 0.8f);
                d.velocity += new Vector2(0f, -1f);

                d.customData = new GlowFlareBehavior(GlowThreshold: 0.6f, GlowPower: 2.5f, TotalBoost: 1f);
            }

            return true;
        }
    }

    public class BloodRainBowVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            //Make sure to draw projectile even if its position is off screen
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        //Safety Checks
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2400;
        }

        int timer = 0;
        float true_width = 1f;
        float true_alpha = 1f;

        public override void AI()
        {
            if (timer > 2)
                true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.08f), 0, 1f);

            if (timer == 100 || true_width <= 0.05f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawPortal(false);
            });
            DrawPortal(true);

            return false;
        }

        public void DrawPortal(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //Portal at first node
            Vector2 portalPos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;

            float easedScale = true_width;
            Vector2 v2Scale = new Vector2(1f * easedScale, 0.25f + (easedScale * 0.75f)) * Projectile.scale * 1.25f;


            Main.EntitySpriteDraw(portal, portalPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.DarkRed with { A = 0 } * 0.5f, rot, portal.Size() / 2f, v2Scale * 1.25f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, portalPos, null, Color.Red with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, portalPos, null, Color.White with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale * 0.5f, SpriteEffects.None);

        }
    }

}
