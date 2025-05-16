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
using rail;
using VFXPlus.Content.Projectiles;
using VFXPLus.Common;
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class MolotovCocktailItemOverride : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.MolotovCocktail);
        }

        public override void SetDefaults(Item entity)
        {
            entity.useStyle = ItemUseStyleID.Swing;
            entity.noUseGraphic = true;

            entity.UseSound = SoundID.Item1 with { Volume = 0f, MaxInstances = -1 };

            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.DD2_GoblinBomberThrow with { Volume = 0.8f, Pitch = 0.35f }, player.Center);

            SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Volume = 0.2f, Pitch = 0.2f }, player.Center);


            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_106") with { Volume = .4f, Pitch = -.15f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, player.Center);

            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }

    public class MolotovCocktailBottleOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MolotovCocktail);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 16;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);

            overallScale = 0.34f + MathHelper.Lerp(0f, 0.66f, Easings.easeInOutBack(animProgress, 1f, 3f));

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;
                float size = projectile.scale * overallScale;

                Color col = Color.Orange * progress * projectile.Opacity;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.25f,
                        previousRotations[i], TexOrigin, size, SE);

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 1.5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.White with { A = 0 }, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int num987 = 0;

            //SoundEngine.PlaySound(in SoundID.Item100, projectile.position);
            for (int num38 = 0; num38 < num987; num38++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3.5f, 8f);

                Dust d = Dust.NewDustPerfect(projectile.Center, 6, vel, Alpha: 200, Scale: 2.5f);

                d.noLight = true;
                d.noGravity = true;
            }

            #region Explosion
            for (int i = 0; i < 2 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1.25f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 13; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f);

                float progress = (float)i / 12;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f, 3.5f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.75f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(11, 25), 0.95f, 0.01f, 1f); //7 21

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 7 + Main.rand.Next(0, 3); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.1f);
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 13, postSlowPower: 0.92f,
                    velToBeginShrink: 4f, fadePower: 0.91f, shouldFadeColor: false);
            }

            #endregion

            SoundEngine.PlaySound(SoundID.Item107 with { Volume = 0.25f, Pitch = -0.3f, PitchVariance = 0.05f }, projectile.Center);
            SoundEngine.PlaySound(SoundID.Shatter with { Volume = 1f, Pitch = 0f, PitchVariance = 0.1f, MaxInstances = -1 }, projectile.Center);

            #region vanillaAI
            Vector2 vector42 = new Vector2(20f, 20f);
            for (int num401 = 220; num401 < 5; num401++)
            {
                Dust.NewDust(projectile.Center - vector42 / 2f, (int)vector42.X, (int)vector42.Y, 12, 0f, 0f, 0, Color.Red);
            }
            for (int num402 = 220; num402 < 10; num402++)
            {
                int num403 = Dust.NewDust(projectile.Center - vector42 / 2f, (int)vector42.X, (int)vector42.Y, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust250 = Main.dust[num403];
                Dust dust334 = dust250;
                dust334.velocity *= 1.4f;
            }
            for (int num404 = 220; num404 < 20; num404++)
            {
                int num405 = Dust.NewDust(projectile.Center - vector42 / 2f, (int)vector42.X, (int)vector42.Y, 6, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[num405].noGravity = true;
                Dust dust251 = Main.dust[num405];
                Dust dust334 = dust251;
                dust334.velocity *= 5f;
                num405 = Dust.NewDust(projectile.Center - vector42 / 2f, (int)vector42.X, (int)vector42.Y, 6, 0f, 0f, 100, default(Color), 1.5f);
                dust251 = Main.dust[num405];
                dust334 = dust251;
                dust334.velocity *= 3f;
            }
            if (Main.myPlayer == projectile.owner)
            {
                for (int num406 = 0; num406 < 6; num406++)
                {
                    float num407 = (0f - projectile.velocity.X) * (float)Main.rand.Next(20, 50) * 0.01f + (float)Main.rand.Next(-20, 21) * 0.4f;
                    float num409 = (0f - Math.Abs(projectile.velocity.Y)) * (float)Main.rand.Next(30, 50) * 0.01f + (float)Main.rand.Next(-20, 5) * 0.4f;
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center.X + num407, projectile.Center.Y + num409, num407, num409, 400 + Main.rand.Next(3), (int)((double)projectile.damage * 0.5), 0f, projectile.owner);
                }
            }
            #endregion

            return false;
        }

    }

    public class MolotovCocktailFireOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MolotovFire || entity.type == ProjectileID.MolotovFire2 || entity.type == ProjectileID.MolotovFire3);
        }

        float randomSineTime = Main.rand.NextFloat(0f, 10f);
        float spinInRandomTime = Main.rand.Next(-5, 5);
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            float rot = projectile.rotation;


            for (int i = 0; i < 1; i++)
            {
                Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.6f);

                Vector2 dustPos = projectile.Center + new Vector2(0f, 7f + projectile.velocity.Y).RotatedBy(rot);

                dustPos += projectile.velocity * 1.5f;

                Vector2 vel = new Vector2(0f, -3f).RotatedBy(rot) * overallScale;

                Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<MediumSmoke>(), Velocity: vel, newColor: between with { A = 0 } * 1f,
                    Scale: Main.rand.NextFloat(0.9f, 1.25f) * 0.4f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                d.rotation = Main.rand.NextFloat(6.28f);

                //Dust d2 = Dust.NewDustPerfect(dustPos, ModContent.DustType<MediumSmoke>(), Velocity: vel, 
                //    newColor: between with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.4f);
                //d2.rotation = Main.rand.NextFloat(6.28f);
                //d2.velocity += projectile.velocity * -0.1f;
            }

            if (timer % 2 == 0)
            {
                Vector2 pos = projectile.Center + new Vector2(0f, 6f).RotatedBy(rot);

                Vector2 dustVel = new Vector2(0f, Main.rand.NextFloat(-3f, -1f)).RotatedByRandom(0.35f) * overallScale;

                Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.65f + Main.rand.NextFloat(-0.15f, 0.15f));

                Dust d2 = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelCross>(), dustVel.RotatedBy(rot), 
                    newColor: between, Scale: Main.rand.NextFloat(0.25f, 1.5f) * 0.25f);
                d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 3, postSlowPower: 0.92f, velToBeginShrink: 1.5f, fadePower: 0.93f, shouldFadeColor: false);

                d2.noLight = false;
            }

            if (timer % 2 == 0 && Main.rand.NextBool())
            {
                int num151 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch, 0f, 0f, 100);
                Main.dust[num151].position.X -= 2f;
                Main.dust[num151].position.Y += 2f;
                Dust dust74 = Main.dust[num151];
                Dust dust212 = dust74;
                dust212.scale += (float)Main.rand.Next(50) * 0.01f;
                Main.dust[num151].noGravity = true;
                Main.dust[num151].velocity.Y -= 2f;
            }

            float fadeInTime = Math.Clamp((float)timer / 25f, 0f, 1f);
            overallScale = Easings.easeOutQuad(fadeInTime);


            float timeForSpinIn = Math.Clamp(timer / (42f + spinInRandomTime), 0f, 1f);
            spinInPower = Easings.easeOutCirc(timeForSpinIn);

            timer++;

            #region vanillaAI

            projectile.ai[0] += 1f;
            if (projectile.ai[0] > 5f)
            {
                projectile.ai[0] = 5f;
                if (projectile.velocity.Y == 0f && projectile.velocity.X != 0f)
                {
                    projectile.velocity.X *= 0.97f;
                    if ((double)projectile.velocity.X > -0.01 && (double)projectile.velocity.X < 0.01)
                    {
                        projectile.velocity.X = 0f;
                        projectile.netUpdate = true;
                    }
                }
                projectile.velocity.Y += 0.2f;
            }
            projectile.rotation += projectile.velocity.X * 0.15f;

            if (projectile.wet)
            {
                projectile.Kill();
            }
            if (projectile.ai[1] == 0f && projectile.type >= 326 && projectile.type <= 328)
            {
                projectile.ai[1] = 1f;
                SoundEngine.PlaySound(in SoundID.Item13, projectile.position);
            }
            //int num154 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100);
            //Main.dust[num154].position.X -= 2f;
            //Main.dust[num154].position.Y += 2f;
            //Dust dust78 = Main.dust[num154];
            //Dust dust212 = dust78;
            //dust212.scale += (float)Main.rand.Next(50) * 0.01f;
            //Main.dust[num154].noGravity = true;
            //Main.dust[num154].velocity.Y -= 2f;
            if (Main.rand.Next(2) == 0)
            {
                //int num155 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 6, 0f, 0f, 100);
                //Main.dust[num155].position.X -= 2f;
                //Main.dust[num155].position.Y += 2f;
                //dust78 = Main.dust[num155];
                //dust212 = dust78;
                //dust212.scale += 0.3f + (float)Main.rand.Next(50) * 0.01f;
                //Main.dust[num155].noGravity = true;
                //dust78 = Main.dust[num155];
                //dust212 = dust78;
                //dust212.velocity *= 0.1f;
            }
            if ((double)projectile.velocity.Y < 0.25 && (double)projectile.velocity.Y > 0.15)
            {
                projectile.velocity.X *= 0.8f;
            }
            projectile.rotation = (0f - projectile.velocity.X) * 0.075f;
			if (projectile.velocity.Y > 16f)
			{
                projectile.velocity.Y = 16f;
			}
            #endregion

            return false;
        }

        float spinInPower = 0f;
        float overallAlpha = 1f;
        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 5f).RotatedBy(projectile.rotation);

            float spinInBonusRot = MathHelper.Lerp(18f * projectile.direction, 0f, spinInPower);
            float drawRot = projectile.rotation - spinInBonusRot;

            Texture2D AreYouAStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;
            Vector2 origin = AreYouAStar.Size() / 2f;

            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);


            float ySinVal = (float)Math.Sin(randomSineTime + Main.timeForVisualEffects * 0.34f) * 0.1f;
            float xSinVal = (float)Math.Cos(randomSineTime + Main.timeForVisualEffects * 0.22f) * 0.1f;

            Vector2 vec2x = new Vector2(0.85f + xSinVal, 0.9f + ySinVal) * projectile.scale * overallScale * 0.4f;

            //re-name these 
            //Vector2 newScale = new Vector2(1.5f, 1f * true_width) * 0.5f; //sword
            //Vector2 newScale2 = new Vector2(0.75f, (1.3f + ySinVal) * true_width) * (0.5f + xSinVal); //spiky
            //Vector2 newScale3 = new Vector2(0.25f * true_width, 0.25f); //Hilt

            Main.EntitySpriteDraw(AreYouAStar, drawPos, null, between with { A = 0 }, drawRot, origin, vec2x, SpriteEffects.None);
            Main.EntitySpriteDraw(AreYouAStar, drawPos, null, Color.White with { A = 0 } * 1f, drawRot, origin, vec2x * 0.5f, SpriteEffects.None);

            Main.EntitySpriteDraw(AreYouAStar, drawPos, null, between with { A = 0 }, drawRot + MathHelper.PiOver2, origin, vec2x, SpriteEffects.None);
            Main.EntitySpriteDraw(AreYouAStar, drawPos, null, Color.White with { A = 0 } * 1f, drawRot + MathHelper.PiOver2, origin, vec2x * 0.5f, SpriteEffects.None);
            return false;
        }

    }
}
