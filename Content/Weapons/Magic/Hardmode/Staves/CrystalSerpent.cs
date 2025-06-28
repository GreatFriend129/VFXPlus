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
using VFXPlus.Common.Drawing;
using Terraria.Graphics;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    public class CrystalSerpentShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalPulse) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.CrystalSerpentToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 20; //40 | 15
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            bool addInBetween = false;
            if (addInBetween)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }

            Color inBetween = Color.Lerp(Color.HotPink, Color.DeepPink, 0.5f);

            Vector2 dustPo2s = projectile.Center + Main.rand.NextVector2Circular(3f, 3f);
            Dust star = Dust.NewDustPerfect(dustPo2s, ModContent.DustType<GlowPixelCross>(), projectile.velocity * 0.2f, newColor: inBetween, Scale: Main.rand.NextFloat(0.2f, 0.31f));
            star.alpha = 2;
            star.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.5f, shouldFadeColor: false, velToBeginShrink: 0.65f);

            Vector2 dustPo2s2 = projectile.Center + Main.rand.NextVector2Circular(3f, 3f);
            Dust star2 = Dust.NewDustPerfect(dustPo2s2 + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelCross>(), projectile.velocity * 0.2f, newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.2f, 0.31f));
            star2.alpha = 2;
            star2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.5f, shouldFadeColor: false, velToBeginShrink: 0.65f);

            if (timer % 1 == 0 && timer > 3)
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.3f, 0.4f) * 2f);
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
            }

            fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, 1.25f, 0.04f), 0f, 1f);


            #region vanillaDust
            int a = false ? 0 : 100;
            for (int num253 = a; num253 < 3; num253++)
            {
                int num254 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 254, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1.2f);
                Main.dust[num254].position = (Main.dust[num254].position + projectile.Center) / 2f;
                Main.dust[num254].position += new Vector2(0f, -150f);
                Main.dust[num254].noGravity = true;
                Dust dust72 = Main.dust[num254];
                Dust dust3 = dust72;
                dust3.velocity *= 0.5f;
            }
            for (int num255 = 0 + a; num255 < 2; num255++)
            {
                int num256 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 255, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 0.4f);
                switch (num255)
                {
                    case 0:
                        Main.dust[num256].position = (Main.dust[num256].position + projectile.Center * 5f) / 6f;
                        break;
                    case 1:
                        Main.dust[num256].position = (Main.dust[num256].position + (projectile.Center + projectile.velocity / 2f) * 5f) / 6f;
                        break;
                }
                Main.dust[num256].position += new Vector2(0f, -150f);

                Dust dust73 = Main.dust[num256];
                Dust dust3 = dust73;
                dust3.velocity *= 0.1f;
                Main.dust[num256].noGravity = true;
                Main.dust[num256].fadeIn = 1f;
            }
            #endregion
            timer++;
            return false;
        }

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                Draw(projectile);
            });

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, -0f);
            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);


            float drawRot = projectile.velocity.ToRotation();
            Vector2 scale = new Vector2(1.15f, 1f) * projectile.scale * 0.45f;


            Color col1 = Color.DeepPink;
            Color col2 = Color.HotPink;

            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, col2 with { A = 0 }, drawRot, TexOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, col1 with { A = 0 }, drawRot, TexOrigin, scale * 0.55f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 }, drawRot, TexOrigin, scale * 0.3f, SpriteEffects.None, 0f);

            return false;
        }

        public void Draw(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(2f, 2f);

                float startScale = projectile.scale + sineScale;

                Color col = Color.Lerp(Color.HotPink * 1.25f, Color.DeepPink, 1f);

                float easedFadeValue = progress * progress;


                Vector2 fireBallScale = new Vector2(1f, 1f) * 0.5f;
                Vector2 fireBallScale2 = new Vector2(0.5f, 1f) * 0.5f;

                Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.8f * easedFadeValue,
                    previousRotations[i] + MathHelper.PiOver2, line.Size() / 2f, fireBallScale * startScale, SpriteEffects.None);

                Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.65f * easedFadeValue,
                    previousRotations[i] + MathHelper.PiOver2, line.Size() / 2f, fireBallScale2 * startScale, SpriteEffects.None);

            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 8 + Main.rand.Next(0, 9); i++)
            {

                float velMult = Main.rand.NextFloat(1.5f, 6.5f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0,
                    newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.35f, 0.95f));

                if (dust.scale > 0.8f)
                    dust.velocity *= 0.5f;

                dust.scale *= 2f;
                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }

            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepPink, Scale: 0.2f);
            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.91f, sizeChangeSpeed: 0.92f, timeToKill: 150,
                overallAlpha: 0.2f, DrawWhiteCore: false, 1f, 1f);

            //Vanilla OnKillStuff (minus dust)
            SoundEngine.PlaySound(in SoundID.Item110, projectile.Center);
            if (Main.myPlayer == projectile.owner)
            {
                int num332 = Main.rand.Next(3, 6);
                for (int num333 = 0; num333 < num332; num333++)
                {
                    Vector2 vector40 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                    while (vector40.X == 0f && vector40.Y == 0f)
                    {
                        vector40 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                    }
                    vector40.Normalize();
                    vector40 *= (float)Main.rand.Next(70, 101) * 0.1f;
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.oldPosition.X + (float)(projectile.width / 2), projectile.oldPosition.Y + (float)(projectile.height / 2), vector40.X, vector40.Y, 522, (int)((double)projectile.damage * 0.8), projectile.knockBack * 0.8f, projectile.owner);
                }
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }
    }

    public class CrystalSerpentShardOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalPulse2) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.CrystalSerpentToggle;
        }

        float drawAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 20; //20
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            
            timer++;

            #region vanillaCode
            projectile.ai[1] += 1f;
            float num257 = (60f - projectile.ai[1]) / 60f;
            if (projectile.ai[1] > 40f)
            {
                projectile.Kill();
            }
            projectile.velocity.Y += 0.2f;
            if (projectile.velocity.Y > 18f)
            {
                projectile.velocity.Y = 18f;
            }
            projectile.velocity.X *= 0.98f;
            for (int num258 = 220; num258 < 2; num258++)
            {
                int num259 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 254, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1.1f);
                Main.dust[num259].position = (Main.dust[num259].position + projectile.Center) / 2f;
                Main.dust[num259].noGravity = true;
                Dust dust74 = Main.dust[num259];
                Dust dust3 = dust74;
                dust3.velocity *= 0.3f;
                dust74 = Main.dust[num259];
                dust3 = dust74;
                dust3.scale *= num257;
            }
            for (int num260 = 220; num260 < 1; num260++)
            {
                int num261 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 255, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 0.6f);
                Main.dust[num261].position = (Main.dust[num261].position + projectile.Center * 5f) / 6f;
                Dust dust75 = Main.dust[num261];
                Dust dust3 = dust75;
                dust3.velocity *= 0.1f;
                Main.dust[num261].noGravity = true;
                Main.dust[num261].fadeIn = 0.9f * num257;
                dust75 = Main.dust[num261];
                dust3 = dust75;
                dust3.scale *= num257;
            }


            #endregion

            return false;
        }



        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawPixelTrail(projectile);
            });

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);


            float drawRot = projectile.velocity.ToRotation();
            Vector2 scale = new Vector2(0.75f, 1.45f) * projectile.scale * 0.85f;

            Color col1 = Color.DeepPink;
            Color col2 = Color.HotPink;

            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, col2 with { A = 0 }, drawRot, TexOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, col1 with { A = 0 }, drawRot, TexOrigin, scale * 0.55f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 }, drawRot, TexOrigin, scale * 0.3f, SpriteEffects.None, 0f);

            return false;
        }

        public void DrawPixelTrail(Projectile projectile)
        {
            Texture2D AfterImage = CommonTextures.Flare.Value;

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                if (progress != 1)
                {
                    Color col = Color.Lerp(Color.DeepPink, Color.HotPink, Easings.easeInCirc(progress));

                    float size2 = Easings.easeInSine(progress) * projectile.scale * 1f;

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f * progress, 3f);

                    Vector2 newVec2 = new Vector2(1f, 0.5f) * size2;
                    Vector2 newVec22 = new Vector2(1f, 0.13f) * size2;

                    Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.Black * 0.7f * progress,
                        previousRotations[i], AfterImage.Size() / 2f, newVec2 * 0.6f, SpriteEffects.None);

                    Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, col with { A = 0 } * 1f,
                           previousRotations[i], AfterImage.Size() / 2f, newVec2 * 1f, SpriteEffects.None);

                    Main.EntitySpriteDraw(AfterImage, AfterImagePos, null, Color.White with { A = 0 } * 0.75f * progress,
                           previousRotations[i], AfterImage.Size() / 2f, newVec22 * 1f, SpriteEffects.None);
                }

            }

            //Glorb
            #region drawGlorb
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Color orbCol2 = Color.Lerp(Color.HotPink, Color.DeepPink, 0.4f) * 0.375f * drawAlpha;
            Color orbCol3 = Color.Lerp(Color.HotPink, Color.DeepPink, 0.95f) * 0.525f * drawAlpha;

            float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.5f) * 0.2f;
            float sineScale2 = MathF.Cos((float)Main.timeForVisualEffects * 0.22f + 0.32578f) * 0.15f;

            float scale2 = 1.8f + sineScale2;
            float scale3 = 2.4f + sineScale + (0.5f + 1f);

            //Main.EntitySpriteDraw(Glow, drawPos, null, Color.Black * 0.3f * drawAlpha, projectile.velocity.ToRotation(), Glow.Size() / 2f, finalDrawScale * 0.3f * drawAlpha, SpriteEffects.None);

            //Main.EntitySpriteDraw(Glow, drawPos, null, orbCol2 with { A = 0 } * 0.75f, projectile.velocity.ToRotation(), Glow.Size() / 2f, finalDrawScale * scale2 * 0.25f * drawAlpha, SpriteEffects.None);
            //Main.EntitySpriteDraw(Glow, drawPos, null, orbCol3 with { A = 0 } * 0.75f, projectile.velocity.ToRotation(), Glow.Size() / 2f, finalDrawScale * scale3 * 0.17f * drawAlpha, SpriteEffects.None);


            #endregion
        }
        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundEngine.PlaySound(in SoundID.Item118, projectile.Center);

            for (int i = 0; i < Main.rand.Next(4, 9); i++)
            {
                float velMult = Main.rand.NextFloat(1.5f, 3f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.55f, 1f));

                if (dust.scale > 0.9f)
                    dust.velocity *= 0.5f;

                dust.scale *= 1.3f;

                dust.fadeIn = Main.rand.NextFloat(0.25f, 0.9f);
                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }

            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepPink, Scale: 0.12f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
                overallAlpha: 0.15f, DrawWhiteCore: false, 1f, 1f);

            return false;
        }
    }

}
