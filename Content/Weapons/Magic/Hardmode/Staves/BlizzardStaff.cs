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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Reflection.Metadata;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{

    public class BlizzardStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Blizzard) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BlizzardStaffToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 11;

            if (timer % 1 == 0)
            {
                previousRotations.Add(projectile.rotation);
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);

                if (timer % 4 == 0 && Main.rand.NextBool())
                {
                    Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.45f);

                    Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: col * 1f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * 1.35f);
                    d.velocity += projectile.velocity * 0.2f;
                }

                if (timer % 2 == 0 && false)
                {
                    Vector2 vel2 = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.25f);

                    Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel2, newColor: col2 * 1f, Scale: Main.rand.NextFloat(0.25f, 0.5f));
                    d2.customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.12f); //0.5f
                    d2.velocity += projectile.velocity * 0.3f;
                }


            }

            if (timer < 7)
            {
                Vector2 vel2 = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.25f);

                Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel2, newColor: col2 * 0.75f, Scale: Main.rand.NextFloat(0.3f, 0.65f));
                d2.velocity += projectile.velocity.RotatedByRandom(0.75f) * 0.7f;

                HighResSmokeBehavior hrsb = new HighResSmokeBehavior();
                hrsb.velSlowAmount = 0.89f;
                hrsb.overallAlpha = 0.7f;
                d2.customData = hrsb;
            }

            Lighting.AddLight(projectile.Center, Color.SkyBlue.ToVector3() * 0.7f);

            //float timeForPopInAnim = 30;
            //float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            //drawScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1f));

            timer++;
            return true;

            #region vanillaCode
            if (projectile.position.Y > Main.player[projectile.owner].position.Y - 300f)
            {
                projectile.tileCollide = true;
            }
            if ((double)projectile.position.Y < Main.worldSurface * 16.0)
            {
                projectile.tileCollide = true;
            }
            projectile.frame = (int)projectile.ai[1];
            if (Main.rand.Next(2) == 0 && false)
            {
                int num116 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 197);
                Main.dust[num116].velocity *= 0.5f;
                Main.dust[num116].noGravity = true;
            }
            #endregion

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            return false;
            return base.PreAI(projectile);
        }


        float drawScale = 1f;
        List<float> previousRotations = new List<float>();
        List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;
                float size = (0.75f + (progress * 0.25f)) * projectile.scale;

                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, progress) * progress * projectile.Opacity * 0.5f;

                //float size2 = (1f + (progress * 0.25f)) * projectile.scale;
                float size2 = 0.8f + (progress * 0.2f) * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.35f,
                        previousRotations[i], TexOrigin, size2, SpriteEffects.None);

                if (i > 5) //2
                {
                    Vector2 vec2Scale = new Vector2(0.2f, 1.5f) * size;
                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.LightSkyBlue with { A = 0 } * 0.25f * progress * 0.5f,
                        previousRotations[i], TexOrigin, vec2Scale, SpriteEffects.None);
                }

            }

            for (int i = 0; i < 4; i++)
            {
                float dist = 4f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                float opacitySquared = projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.SkyBlue with { A = 0 } * 0.25f * opacitySquared, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SpriteEffects.None);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 randOff = Main.rand.NextVector2CircularEdge(1f, 1f);
                
                Main.EntitySpriteDraw(vanillaTex, drawPos + randOff, sourceRectangle, Color.LightSkyBlue with { A = 0 } * 0.85f, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * 1f, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 } * 0.2f, projectile.rotation, TexOrigin, projectile.scale * drawScale, SpriteEffects.None);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/deerclops_ice_attack_1") with { Volume = .05f, Pitch = 0.9f, PitchVariance = 0.3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .27f, Pitch = .7f, PitchVariance = 0.2f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            //Option B:

            //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ice_hit") with { Volume = 0.1f, Pitch = 0.15f, PitchVariance = .5f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style, projectile.Center);

            //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_107") with { Volume = .1f, Pitch = .55f, PitchVariance = 0.8f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style2, projectile.Center);

            //SoundStyle style4 = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_death_1") with { Volume = 0.05f, Pitch = .85f, PitchVariance = 0.45f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style4, projectile.Center);

            for (int i = 0; i < 9 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);

                p.velocity += projectile.velocity * 0.15f;
            }

            for (int i = 0; i < 3; i++)
            {
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);

                Vector2 vel = Main.rand.NextVector2Circular(1.75f, 1.75f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel, newColor: col2, Scale: Main.rand.NextFloat(0.25f, 0.5f));
                d.customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.65f);
            }

            return false;
            //return base.PreKill(projectile, timeLeft);
        }

    }
}
