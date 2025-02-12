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
using Terraria.Graphics;
using Terraria.Physics;
using AerovelenceMod.Content.Items.Weapons.AreaPistols.ErinGun;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    public class ToxikarpShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ToxicBubble);
        }

        float randomSineOffsetTime = 0f;
        float randomPulseOffsetTime = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            randomPulseOffsetTime = Main.rand.NextFloat(2f);
            randomSineOffsetTime = Main.rand.NextFloat(2f);


            float fadeInTime = Math.Clamp((timer + 6f) / 25f, 0f, 1f);
            totalScale = Easings.easeInOutHarsh(fadeInTime);


            int trailCount = 24;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            //totalScale = 1f;
            timer++;
            return true;
        }


        float drawRot = 0f;
        float totalAlpha = 0f;
        float totalScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float scale = projectile.scale * totalScale;

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.GreenYellow * progress * progress;

                float size1 = (0.25f + (progress * 0.75f)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.1f,
                    previousRotations[i], TexOrigin, size1 * scale, 0);
            }

            //Bloomball
            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.White * 0.35f, projectile.rotation, TexOrigin, scale, 0);



            float glowscale = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 6f + randomSineOffsetTime) / 5f + 1f) * 1.25f;
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.LawnGreen with { A = 0 } * 0.1f, projectile.rotation, TexOrigin, scale * glowscale, 0);

            float sineScale = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.15f + randomPulseOffsetTime) * 0.1f;

            Main.EntitySpriteDraw(Ball, drawPos + new Vector2(0f, 0f), null, Color.LawnGreen with { A = 0 } * 0.1f, projectile.rotation, Ball.Size() / 2f, scale * 0.5f * sineScale, 0);


            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.08f * projectile.scale, true, 2, 0.4f, 0.4f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.GreenYellow * 0.25f);
            d1.customData = cpb2;
            d1.velocity = projectile.velocity.SafeNormalize(Vector2.UnitX) * 0.01f;

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.GreenYellow * 0.25f);
            d2.customData = cpb2;
            d2.velocity = projectile.velocity.SafeNormalize(Vector2.UnitX) * -0.01f;

            d1.scale = 0.12f;
            d2.scale = 0.12f;

            for (int i = 0; i < 4 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: Color.GreenYellow * 0.5f, Scale: Main.rand.NextFloat(0.2f, 0.35f) * projectile.scale);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(shouldFadeColor: false, fadePower: 0.92f);
            }

            return true;
        }
    }

}
