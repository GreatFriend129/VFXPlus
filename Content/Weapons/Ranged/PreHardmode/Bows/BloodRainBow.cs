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

            return base.PreAI(projectile);
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

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                //After-Image
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    //Start End
                    Color col = Color.Lerp(Color.Red, Color.DarkRed, progress) * progress;

                    Vector2 size = new Vector2(1f, 0.3f * progress) * drawScale;

                    Main.EntitySpriteDraw(flare, AfterImagePos, null, col * progress * 2f,
                        previousRotations[i], TexOrigin, size, SpriteEffects.None);

                    //SIZE 2 IS REAL
                    Vector2 size2 = new Vector2(0.5f, 2.5f * progress) * drawScale;

                    Main.EntitySpriteDraw(flare2, AfterImagePos, null, Color.Red with { A = 0 } * 0.3f * progress,
                        previousRotations[i], flare2.Size() / 2f, size2, SpriteEffects.None);
                }
            });

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
}
