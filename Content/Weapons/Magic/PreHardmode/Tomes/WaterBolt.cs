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
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Tomes
{
    public class WaterBoltShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WaterBolt) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.WaterboltToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            if (timer % 2 == 0)
            {
                int trailCount = 10; //30
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }

            if (timer % 2 == 0 && timer > 3 && Main.rand.NextBool(2))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: Color.DodgerBlue * 0.75f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.75f);
                da.velocity += projectile.velocity.RotatedByRandom(0.2f) * 0.65f;
                da.alpha = 12;
            }

            if (timer % 3 == 0 && Main.rand.NextBool(5) && timer > 3)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DodgerBlue, Scale: 0.6f);
                de.customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);


                Dust dust57 = de;
                Dust dust212 = dust57;
                dust212.velocity *= 0.45f;
                dust57 = de;
                dust212 = dust57;
                dust212.velocity += projectile.velocity * 0.5f;
            }


            Lighting.AddLight(projectile.Center, Color.DodgerBlue.ToVector3() * 0.85f);


            /*
            if (timer % 4 == 0 && Main.rand.NextBool() && timer > 8)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, -100f), ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.DodgerBlue * 2f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.25f);

                p.velocity -= projectile.velocity * 1f;
            }
            */


            fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, 1.25f, 0.04f), 0f, 1f);

            timer++;
            return false;
        }

        float fadeInAlpha = 0f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                TrailDraw(projectile);
            });

            return false;
        }

        public void TrailDraw(Projectile projectile)
        {
            Texture2D line = CommonTextures.Flare.Value;

            //After-Image
            if (previousRotations != null && previousPositions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;


                    float offsetIntensity = (1.5f * (1f - progress)) + 4.5f;

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(offsetIntensity, offsetIntensity); //3f

                    float startScale = 1f + sineScale;

                    Color col = Color.Lerp(Color.DodgerBlue, Color.Blue, 1f - progress);

                    float easedFadeValue = progress * progress;


                    Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress); //
                    Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress); //0.1f 0.2f

                    //Black
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.2f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * projectile.scale, SpriteEffects.None);

                    //Main
                    Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 1f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);

                    //White
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

                }

                Main.EntitySpriteDraw(line, projectile.Center - Main.screenPosition, null, Color.DodgerBlue with { A = 0 } * 1f,
                    projectile.velocity.ToRotation(), line.Size() / 2f, new Vector2(1.2f, 0.7f) * 0.75f, SpriteEffects.None);

                Main.EntitySpriteDraw(line, projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * 0.85f,
                    projectile.velocity.ToRotation(), line.Size() / 2f, new Vector2(1.25f, 0.7f) * 0.5f, SpriteEffects.None);
            }

            return;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 6 + Main.rand.Next(3, 8); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 2.5f).RotatedBy(projectile.velocity.ToRotation());
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.35f, 0.8f));
                d.customData = new GlowFlareBehavior(GlowThreshold: 0.45f, GlowPower: 2.5f, TotalBoost: 0.95f);
            }

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ENV_water_splash_01") with { Volume = 0.15f, Pitch = .5f, PitchVariance = 0.25f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.45f, 0.75f));
                d.customData = new GlowFlareBehavior(GlowThreshold: 0.45f, GlowPower: 2.5f, TotalBoost: 0.95f);

            }

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: Color.DodgerBlue, Scale: 0.8f);
            d1.rotation = 0f + oldVelocity.ToRotation();
            d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: Color.DodgerBlue, Scale: 0.8f);
            d2.rotation = MathHelper.PiOver4 + oldVelocity.ToRotation();
            d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
