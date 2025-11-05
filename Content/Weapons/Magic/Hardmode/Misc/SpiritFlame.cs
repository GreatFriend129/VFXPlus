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
using VFXPlus.Content.Particles;
using VFXPlus.Content.QueenBee;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    public class SpiritFlameShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SpiritFlame) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SpiritFlameToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            float adjustedRot = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            projectile.rotation = projectile.velocity.Length() > 0 ? adjustedRot : 0f;
            
            int trailCount = 9;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer == 0)
            {
                Color littleLessPurple = new Color(121, 7, 179);

                //little offset because proj center is not center of fireball visually
                Vector2 dustSpawnPos = projectile.Center;// + new Vector2(0f, 7f * projectile.scale);

                Dust d1 = Dust.NewDustPerfect(dustSpawnPos, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: littleLessPurple, Scale: 1f);
                d1.rotation = 0f;
                d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

                Dust d2 = Dust.NewDustPerfect(dustSpawnPos, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: littleLessPurple, Scale: 1f);
                d2.rotation = MathHelper.PiOver4;
                d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

                for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                    Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: littleLessPurple, Scale: Main.rand.NextFloat(0.75f, 1.25f) * 0.4f);
                    d.fadeIn = Main.rand.Next(0, 4);
                    d.alpha = Main.rand.Next(0, 2);
                    d.noLight = false;
                }

                for (int i = 220; i < 7; i++)
                {
                    float velMult = Main.rand.NextFloat(2f, 6f);
                    Vector2 randomStart = Main.rand.NextVector2CircularEdge(2f, 2f);

                    FireParticle fire = new FireParticle(projectile.Center, randomStart * velMult * 0.35f, 1f, Color.Purple, colorMult: 2f, bloomAlpha: 1.5f);
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }

            if (timer % 5 == 0 && Main.rand.NextBool())
            {

                Vector2 vel = Main.rand.NextVector2Circular(2.75f, 2.75f) + projectile.velocity * 0.25f;

                Color purp = new Color(61, 2, 92); //42 2 82

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: purp * 3f, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity += projectile.velocity * 0.2f;

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(shouldFadeColor: false);

            }

            if (timer % 2 == 0 && Main.rand.NextBool(2) && projectile.velocity.Length() > 0)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) - projectile.velocity * 0.25f;

                FireParticle fire = new FireParticle(projectile.Center, vel, 0.45f, new Color(121, 7, 179), colorMult: 1f, bloomAlpha: 1.5f, AlphaFade: 0.94f + Main.rand.NextFloat(-0.02f, 0.02f), VelFade: 0.9f, RotPower: 0.1f);
                ShaderParticleHandler.SpawnParticle(fire);
            }

            timer++;

            inFadePower = Math.Clamp(MathHelper.Lerp(inFadePower, 1.35f, 0.1f), 0f, 1f);

            return base.PreAI(projectile);
        }

        float inFadePower = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(projectile, false);
            });

            Color purp = new Color(121, 7, 179) * inFadePower;
            Color purp2 = new Color(61, 2, 92);

            Texture2D SoftGlow = CommonTextures.SoftGlow64.Value;
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;


            Vector2 posOffset = new Vector2(0f, -6f * projectile.scale).RotatedBy(projectile.rotation);
            Vector2 drawPos = projectile.Center - Main.screenPosition + posOffset;
            
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            float easeVal = Easings.easeInOutBack(inFadePower, 0f, 10f);
            Vector2 vec2Scale = new Vector2(easeVal, 1f);

            //Border
            for (int i = 0; i < 8; i++)
            {
                float opacitySquared = inFadePower * inFadePower;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    purp2 with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, vec2Scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * inFadePower, projectile.rotation, TexOrigin, vec2Scale * projectile.scale, SpriteEffects.None);
            return false;

        }

        public void DrawAfterImage(Projectile projectile, bool returnImmediately)
        {
            Color purple = new Color(61, 2, 92);
            Color purple2 = new Color(61, 2, 92);
            Color purple3 = new Color(121, 7, 179);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;


            Vector2 posOffset = new Vector2(0f, -6f * projectile.scale).RotatedBy(projectile.rotation);
            Vector2 drawPos = projectile.Center - Main.screenPosition + posOffset;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            float easeVal = Easings.easeInOutBack(inFadePower, 0f, 10f);
            Vector2 vec2Scale = new Vector2(easeVal, 1f);

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = (purple2 * 1.5f) * progress * inFadePower;

                float size2 = (0.25f + (progress * 0.75f)) * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + posOffset;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 1f, //0.5f
                        previousRotations[i], TexOrigin, vec2Scale * size2, SpriteEffects.None);
            }



            //Orb
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color[] cols = { purple3 * 0.75f, Color.Purple * 0.525f, purple * 0.375f };
            float[] scales = { 1.15f, 1.6f, 2.5f };

            float orbRot = projectile.rotation;
            float orbAlpha = 0.65f * inFadePower;
            Vector2 orbOrigin = orb.Size() / 2f;
            Vector2 orbScale = new Vector2(0.55f * easeVal, 0.85f) * projectile.scale * 0.35f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[2] * sineScale2, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //return true;
            if (projectile.ai[0] < 0f)
                return false;

            #region CerobaImpact
            Color littleLessPurple = new Color(121, 7, 179);

            Color newPurple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);

            Color purp1 = newPurple;
            Color purp2 = Color.Purple;

            //Impact
            for (int i = 220; i < 6 + Main.rand.Next(0, 4); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: littleLessPurple * 3f, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 13, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 7 + Main.rand.Next(0, 3); i++)
            {
                Color col = Main.rand.NextBool() ? newPurple * 2f : newPurple;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.75f, 1.25f) * 1f);
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;
            }
            

            for (int i = 0; i < 14; i++)
            {
                float prog = (float)i / 17f;


                Vector2 veloF = Main.rand.NextVector2CircularEdge(4f, 4f) * Easings.easeOutQuad(prog) * 1.5f;

                FireParticle fire = new FireParticle(projectile.Center, veloF, 1f * Main.rand.NextFloat(1f, 1.5f), new Color(121, 7, 179), colorMult: 1f, bloomAlpha: 1.5f, 
                    AlphaFade: 0.94f + Main.rand.NextFloat(-0.02f, 0.02f), VelFade: 0.9f, RotPower: 0.35f);
                ShaderParticleHandler.SpawnParticle(fire);

            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: littleLessPurple * 2f, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);

            //Sound
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_14") with { Pitch = .3f, MaxInstances = -1, PitchVariance = 0.2f, Volume = 0.3f };
            SoundEngine.PlaySound(style, projectile.Center);

            string randomSound = Main.rand.NextBool() ? "2" : "1";

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Custom/dd2_flameburst_tower_shot_" + randomSound) with { Pitch = .25f, PitchVariance = .32f, MaxInstances = -1, Volume = 0.35f };
            SoundEngine.PlaySound(style4, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_62") with { Volume = .23f, Pitch = .51f, PitchVariance = .27f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            #endregion

            #region vanillaKillStuff
            if (projectile.ai[0] >= 0f)
            {
                int num143 = 80;
                projectile.position = projectile.Center;
                projectile.width = (projectile.height = num143);
                projectile.Center = projectile.position;
                projectile.Damage();
                SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
                int num144 = 15;
            }

            #endregion

            return false;
        }
    }

}
