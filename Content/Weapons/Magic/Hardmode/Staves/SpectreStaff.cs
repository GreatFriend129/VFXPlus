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
using Terraria.GameContent.Drawing;
using VFXPlus.Common.Drawing;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Runtime.InteropServices.JavaScript;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    public class SpectreStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LostSoulFriendly) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SpecterStaffToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 30; //40
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer % 2 == 0 && timer > 20)
            {
                Color col = Main.rand.NextBool() ? Color.White : Color.SkyBlue;

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelFast>(), Alpha: 50, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.35f));
                d.position -= projectile.velocity;

                Vector2 dustVel = (projectile.velocity * Main.rand.NextFloat(0.85f, 1.15f) * -0.5f).RotateRandom(0.3f);
                d.velocity = dustVel;

                d.fadeIn = 50;
            }

            float timeForPopInAnim = 50;
            float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f);

            //overallAlpha = 0.2f + MathHelper.Lerp(0f, 0.8f, Easings.easeInOutBack(animProgress, 1f, 1f));

            starPower = Math.Clamp(MathHelper.Lerp(starPower, 1.25f, 0.02f), 0f, 1f);
            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.03f), 0f, 1f);

            Lighting.AddLight(projectile.Center, Color.LightSkyBlue.ToVector3() * 0.6f);

            if (timer % 12 == 0)
                projectile.frameCounter++;


            #region vanillaCode Minus Dust

            float num408 = projectile.Center.X;
            float num409 = projectile.Center.Y;
            float num412 = 400f;
            bool flag14 = false;
            int num413 = 0;
            if (true)
            {
                for (int num414 = 0; num414 < 200; num414++)
                {
                    if (Main.npc[num414].CanBeChasedBy(this) && projectile.Distance(Main.npc[num414].Center) < num412 && Collision.CanHit(projectile.Center, 1, 1, Main.npc[num414].Center, 1, 1))
                    {
                        float num415 = Main.npc[num414].position.X + (float)(Main.npc[num414].width / 2);
                        float num416 = Main.npc[num414].position.Y + (float)(Main.npc[num414].height / 2);
                        float num417 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num415) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num416);
                        if (num417 < num412)
                        {
                            num412 = num417;
                            num408 = num415;
                            num409 = num416;
                            flag14 = true;
                            num413 = num414;
                        }
                    }
                }
            }
            
            if (flag14)
            {
                float num423 = 6f;

                Vector2 vector107 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                float num424 = num408 - vector107.X;
                float num425 = num409 - vector107.Y;
                float num426 = (float)Math.Sqrt(num424 * num424 + num425 * num425);
                float num427 = num426;
                num426 = num423 / num426;
                num424 *= num426;
                num425 *= num426;

                projectile.velocity.X = (projectile.velocity.X * 20f + num424) / 21f;
                projectile.velocity.Y = (projectile.velocity.Y * 20f + num425) / 21f;


            }
            #endregion
            
            timer++;
            return false;

        }

        float drawScale = 0.65f;

        float starPower = 0f;
        public float overallAlpha = 0f;
        Effect myEffect = null;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                Trail(projectile, true);
                NewTrail(projectile, false);
            });

            Trail(projectile, true);
            NewTrail(projectile, true);

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Star
            if (starPower < 1)
            {
                Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;

                float dir = projectile.velocity.X > 0 ? 1 : -1;

                float starRotation = MathHelper.Lerp(0f, MathHelper.Pi * 2f * dir, Easings.easeInOutQuad(starPower)) + projectile.rotation;
                float starScale = Easings.easeOutQuint(1f - starPower) * projectile.scale * 1.35f;

                Vector2 starScaleVec2 = new Vector2(1f, 0.7f) * starScale * overallAlpha;
                

                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScaleVec2, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScaleVec2 * 0.65f, SpriteEffects.None);

                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScaleVec2, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScaleVec2 * 0.65f, SpriteEffects.None);
            }


            Texture2D Ghost = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Staves/SpectreAssets/DungeonSpirit").Value;
            Texture2D GhostBorder = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Staves/SpectreAssets/DungeonSpiritBorder").Value;

            int currentFrame = projectile.frameCounter % 3;
            int frameHeight = Ghost.Height / 3;

            Rectangle sourceRectangle = new Rectangle(0, frameHeight * currentFrame, Ghost.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            float sinScale = 1f + (float)(Math.Sin((randomSineOffset + 20f) + Main.timeForVisualEffects * 0.15f) * 0.07f);

            Vector2 mainVec2Scale = new Vector2(1f, Easings.easeOutSine(overallAlpha)) * sinScale;

            Main.EntitySpriteDraw(GhostBorder, drawPos, sourceRectangle, Color.White with { A = 0 } * overallAlpha * 0.5f, projectile.velocity.ToRotation(), origin, projectile.scale * drawScale * 1.1f * mainVec2Scale, SpriteEffects.None);

            Main.EntitySpriteDraw(Ghost, drawPos, sourceRectangle, Color.White * overallAlpha, projectile.velocity.ToRotation(), origin, projectile.scale * drawScale * 1.1f * mainVec2Scale, SpriteEffects.None);

            return false;
        }

        public void Trail(Projectile projectile, bool returnImmediately)
        {
            if (returnImmediately)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Star
            if (starPower < 1)
            {
                Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;

                float dir = projectile.velocity.X > 0 ? 1 : -1;

                float starRotation = MathHelper.Lerp(0f, MathHelper.Pi * 2f * dir, Easings.easeInOutQuad(starPower)) + projectile.rotation;
                float starScale = Easings.easeOutQuint(1f - starPower) * projectile.scale * 1.25f;

                Vector2 starScaleVec2 = new Vector2(1f, 0.5f) * starScale;


                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScaleVec2, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScaleVec2 * 0.65f, SpriteEffects.None);

                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScaleVec2, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScaleVec2 * 0.65f, SpriteEffects.None);
            }

            //Orb
            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;
            Vector2 vec2Scale = new Vector2(0.85f, 0.55f) * overallAlpha;

            //Main.EntitySpriteDraw(orb, drawPos, null, Color.White with { A = 0 } * 0.1f, projectile.velocity.ToRotation(), orb.Size() / 2f, 2f * overallAlpha, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, Color.LightSkyBlue with { A = 0 } * 1f, projectile.velocity.ToRotation(), orb.Size() / 2f, vec2Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, Color.White with { A = 0 } * 1f, projectile.velocity.ToRotation(), orb.Size() / 2f, vec2Scale * 0.5f, SpriteEffects.None);


            Texture2D Ghost = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Staves/SpectreAssets/lostsoul").Value;

            Vector2 mainVec2Scale = new Vector2(1f, overallAlpha);

            //AfterImage
            for (int i = 0; i < previousRotations.Count; i += 2)
            {
                float progress = (float)i / previousRotations.Count;

                float size = (0.5f + (progress * 0.5f)) * projectile.scale;

                Color betweenBlue = Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, 0.5f);

                Color col = Color.Lerp(Color.DeepSkyBlue, betweenBlue, progress) * progress;

                float size2 = size;
                Main.EntitySpriteDraw(Ghost, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.5f * Easings.easeOutQuad(overallAlpha),
                        previousRotations[i], Ghost.Size() / 2f, mainVec2Scale * size2 * drawScale, SpriteEffects.None);

                Vector2 vec2ScaleLine = new Vector2(1.5f, 0.3f * overallAlpha) * size;

                Main.EntitySpriteDraw(Ghost, previousPositions[i] - Main.screenPosition, null, col with { A = 0 } * 0.35f * overallAlpha,
                        previousRotations[i], Ghost.Size() / 2f, vec2ScaleLine * drawScale, SpriteEffects.None);
            }
        }

        float randomSineOffset = Main.rand.NextFloat(0f, 100f);
        public void NewTrail(Projectile projectile, bool returnImmediately)
        {
            if (returnImmediately)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Orb
            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;
            Vector2 vec2Scale = new Vector2(0.85f * Easings.easeOutSine(overallAlpha), 0.55f) * overallAlpha * 1.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, Color.LightSkyBlue with { A = 0 } * 1f, projectile.velocity.ToRotation(), orb.Size() / 2f, vec2Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, Color.White with { A = 0 } * 1f, projectile.velocity.ToRotation(), orb.Size() / 2f, vec2Scale * 0.5f, SpriteEffects.None);


            Texture2D Spike = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            float sinWidth = 1f + (float)(Math.Sin(randomSineOffset + Main.timeForVisualEffects * 0.25f) * 0.15f);

            for (int i = 0; i < previousPositions.Count; i++)
            {
                float scale = (float)i / previousPositions.Count;
                Vector2 vec2ScaleTrail = new Vector2(scale * 0.5f * Easings.easeOutSine(overallAlpha), Easings.easeOutQuad(scale) * 0.45f * sinWidth) * projectile.scale;

                Vector2 drawPosAI = previousPositions[i] - Main.screenPosition;

                Color betweenBlue = Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, 0.75f);
                Color col = Color.Lerp(Color.DeepSkyBlue, betweenBlue, scale) * scale * overallAlpha;

                Main.spriteBatch.Draw(Spike, drawPosAI, null, col with { A = 0 } * 0.75f, previousRotations[i], Spike.Size() / 2f, vec2ScaleTrail, SpriteEffects.None, 0f);
            }
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < previousPositions.Count; i++)
            {
                Vector2 pos = previousPositions[i];
                Vector2 velRot = previousRotations[i].ToRotationVector2();

                if (i % 2 == 0)
                {
                    Color col = Main.rand.NextBool() ? Color.White : Color.SkyBlue;

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelFast>(), Alpha: 100, newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f));

                    Vector2 dustVel = (velRot * Main.rand.NextFloat(1f, 4.1f) * -0.5f).RotateRandom(0.3f);
                    d.velocity = dustVel + Main.rand.NextVector2Circular(3f, 3f);
                }

            }
            return base.PreKill(projectile, timeLeft);
        }
    }
}
