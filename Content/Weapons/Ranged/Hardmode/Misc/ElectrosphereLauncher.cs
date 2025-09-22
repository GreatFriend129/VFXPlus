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
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest.Aero;
using ReLogic.Utilities;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    public class ElectrosphereLauncher : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ElectrosphereLauncher);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.ElectrosphereLauncher,
                    AnimTime: 15,
                    NormalXOffset: 16f,
                    DestXOffset: 3f,
                    YRecoilAmount: 0.08f,
                    HoldOffset: new Vector2(0f, 2.5f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            return true;
        }
    }
    public class ElectrosphereRocketOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ElectrosphereMissile);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 22; //24
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 1 == 0 && Main.rand.NextBool() && timer > 5)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), dustVel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.2f, 0.5f) * 2f);
                da.velocity -= projectile.velocity.RotatedByRandom(0.25f) * 0.5f;

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.93f, FadeScalePower: 0.98f, FadeVelPower: 0.93f, Pixelize: true, XScale: 1f, YScale: 0.45f);
                esb.randomVelRotatePower = 0.15f;
                da.customData = esb;

            }

            float fadeInTime = Math.Clamp((timer + 6f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            timer++;

            #region vanillaAI
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2f;

            projectile.frame = 0;
            if (projectile.alpha != 0)
            {
                projectile.localAI[0] += 1f;
                if (projectile.localAI[0] >= 4f)
                {
                    projectile.alpha -= 90;
                    if (projectile.alpha < 0)
                    {
                        projectile.alpha = 0;
                        projectile.localAI[0] = 2f;
                    }
                }
            }
            if (Vector2.Distance(projectile.Center, new Vector2(projectile.ai[0], projectile.ai[1]) * 16f + Vector2.One * 8f) <= 16f)
            {
                projectile.Kill();
                return false;
            }
            if (projectile.alpha == 0)
            {
                projectile.localAI[1] += 1f;
                if (projectile.localAI[1] >= 120f)
                {
                    projectile.Kill();
                    return false;
                }
                Lighting.AddLight((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16, 0.3f, 0.45f, 0.8f);
                projectile.localAI[0] += 1f;
                if (projectile.localAI[0] == 3f)
                {
                    projectile.localAI[0] = 0f;
                    for (int num193 = 2220; num193 < 8; num193++)
                    {
                        Vector2 spinningpoint3 = Vector2.UnitX * -8f;
                        spinningpoint3 += -Vector2.UnitY.RotatedBy((float)num193 * (float)Math.PI / 4f) * new Vector2(2f, 4f);
                        spinningpoint3 = spinningpoint3.RotatedBy(projectile.rotation - (float)Math.PI / 2f);
                        int num194 = Dust.NewDust(projectile.Center, 0, 0, 135);
                        Main.dust[num194].scale = 1.5f;
                        Main.dust[num194].noGravity = true;
                        Main.dust[num194].position = projectile.Center + spinningpoint3;
                        Main.dust[num194].velocity = projectile.velocity * 0.66f;
                    }
                }
            }
            #endregion
            return false;
        }


        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {   
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            float scale = projectile.scale * overallScale;


            //Trail
            Color col = Color.Lerp(Color.LightSkyBlue, Color.SkyBlue, 1f);
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color trailCol = Color.Lerp(col, Color.DeepSkyBlue, 1f - progress) * progress;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(10f, 10f) * progress;
                Vector2 AfterImagePos2 = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(4f, 4f) * progress;

                Vector2 flareScale = new Vector2(1f, 0.45f * progress) * scale;

                Main.EntitySpriteDraw(flare, AfterImagePos, null, trailCol with { A = 0 } * progress * 0.8f,
                    previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, flareScale, 0);

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos2, sourceRectangle, trailCol with { A = 0 } * progress * 0.65f, 
                    previousRotations[i], TexOrigin, scale * progress * progress, 0);
            }

            //Bloomball
            Texture2D Ball = CommonTextures.FireBallBlur.Value;

            Vector2 ballOff1 = drawPos + projectile.velocity.SafeNormalize(Vector2.UnitX) * -15f + new Vector2(0f, 0f);

            Main.EntitySpriteDraw(Ball, ballOff1, null, Color.SkyBlue with { A = 0 } * 0.35f, projectile.rotation, Ball.Size() / 2f, new Vector2(0.7f, 0.7f) * scale, SE);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(3f, 3f), sourceRectangle,
                    Color.SkyBlue with { A = 0 } * 1f, projectile.rotation, TexOrigin, scale * 1.1f, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, scale, SE);


            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;
        }
    }

    public class ElectrosphereOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Electrosphere);
        }

        int dir = 1;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            if (timer % 1 == 0 && Main.rand.NextBool() && timer > 5)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), dustVel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.2f, 0.5f) * 2f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.93f, FadeScalePower: 0.98f, FadeVelPower: 0.93f, Pixelize: true, XScale: 1f, YScale: 0.45f);
                esb.randomVelRotatePower = 0.1f;
                da.customData = esb;

            }

            float fadeInTime = Math.Clamp((timer + 18f) / 45f, 0f, 1f);

            if (projectile.timeLeft < 15)
                overallScale = Math.Clamp(MathHelper.Lerp(overallScale, -0.5f, 0.07f), 0f, 1f);
            else
                overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;

            #region vanillaAI
            ActiveSound activeSound = null;
            if (SoundEngine.TryGetActiveSound(SlotId.FromFloat(projectile.localAI[0]), out var idleSoundOut))
            {
                activeSound = idleSoundOut;
            }

            //ActiveSound activeSound = SoundEngine.TryGetActiveSound(SlotId.FromFloat(projectile.localAI[0]), out var soundOut);
            SlotId invalid;
            if (activeSound != null)
            {
                if (activeSound.Volume == 0f)
                {
                    activeSound.Stop();
                    float[] array5 = projectile.localAI;
                    invalid = SlotId.Invalid;
                    array5[0] = ((SlotId)(invalid)).ToFloat();
                }
                activeSound.Volume = Math.Max(0f, activeSound.Volume - 0.05f);
            }
            else
            {
                float[] array6 = projectile.localAI;
                invalid = SlotId.Invalid;
                array6[0] = ((SlotId)(invalid)).ToFloat();
            }
            if (projectile.ai[1] == 1f)
            {
                projectile.friendly = false;
                if (projectile.alpha < 255)
                {
                    projectile.alpha += 51;
                }
                if (projectile.alpha >= 255)
                {
                    projectile.alpha = 255;
                    projectile.Kill();
                    return false;
                }
            }
            else
            {
                if (projectile.alpha > 0)
                {
                    projectile.alpha -= 50;
                }
                if (projectile.alpha < 0)
                {
                    projectile.alpha = 0;
                }
            }
            float num673 = 30f;
            float num674 = num673 * 4f;
            projectile.ai[0]++;
            if (projectile.ai[0] > num674)
            {
                projectile.ai[0] = 0f;
            }
            Vector2 vector128 = -Vector2.UnitY.RotatedBy((float)Math.PI * 2f * projectile.ai[0] / num673);
            float val = 0.75f + vector128.Y * 0.25f;
            float val2 = 0.8f - vector128.Y * 0.2f;
            float num675 = Math.Max(val, val2);
            projectile.position += new Vector2(projectile.width, projectile.height) / 2f;
            projectile.width = (projectile.height = (int)(80f * num675));
            projectile.position -= new Vector2(projectile.width, projectile.height) / 2f;
            projectile.frameCounter++;
            if (projectile.frameCounter >= 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame >= 4)
                {
                    projectile.frame = 0;
                }
            }
            for (int num676 = 220; num676 < 1; num676++)
            {
                float num678 = 55f * num675;
                float num679 = 11f * num675;
                float num680 = 0.5f;
                int num681 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 226, 0f, 0f, 100, default(Color), 0.5f);
                Main.dust[num681].noGravity = true;
                Dust dust109 = Main.dust[num681];
                Dust dust212 = dust109;
                dust212.velocity *= 2f;
                Main.dust[num681].position = ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (num679 + num680 * (float)Main.rand.NextDouble() * num678) + projectile.Center;
                Main.dust[num681].velocity = Main.dust[num681].velocity / 2f + Vector2.Normalize(Main.dust[num681].position - projectile.Center);
                if (Main.rand.Next(2) == 0)
                {
                    num681 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 226, 0f, 0f, 100, default(Color), 0.9f);
                    Main.dust[num681].noGravity = true;
                    dust109 = Main.dust[num681];
                    dust212 = dust109;
                    dust212.velocity *= 1.2f;
                    Main.dust[num681].position = ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (num679 + num680 * (float)Main.rand.NextDouble() * num678) + projectile.Center;
                    Main.dust[num681].velocity = Main.dust[num681].velocity / 2f + Vector2.Normalize(Main.dust[num681].position - projectile.Center);
                }
                if (Main.rand.Next(4) == 0)
                {
                    num681 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 226, 0f, 0f, 100, default(Color), 0.7f);
                    Main.dust[num681].noGravity = true;
                    dust109 = Main.dust[num681];
                    dust212 = dust109;
                    dust212.velocity *= 1.2f;
                    Main.dust[num681].position = ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (num679 + num680 * (float)Main.rand.NextDouble() * num678) + projectile.Center;
                    Main.dust[num681].velocity = Main.dust[num681].velocity / 2f + Vector2.Normalize(Main.dust[num681].position - projectile.Center);
                }
            }

            #endregion
            return false;
        }


        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        Effect myEffect = null;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {

            #region vanillaDraw
            SpriteEffects dirSE = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
                dirSE = SpriteEffects.FlipHorizontally;

            Texture2D value173 = TextureAssets.Projectile[projectile.type].Value;
            float num284 = 30f;
            float num285 = num284 * 4f;
            float num286 = (float)Math.PI * 2f * projectile.ai[0] / num284;
            float num287 = (float)Math.PI * 2f * projectile.ai[0] / num285;
            Vector2 vector156 = -Vector2.UnitY.RotatedBy(num286);
            float scale18 = 0.75f + vector156.Y * 0.25f;
            float scale19 = 0.8f - vector156.Y * 0.2f;
            int num288 = value173.Height / Main.projFrames[projectile.type];
            int y20 = num288 * projectile.frame;
            Vector2 position18 = projectile.position + new Vector2(projectile.width, projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;

            Main.EntitySpriteDraw(value173, position18, new Rectangle(0, y20, value173.Width, num288), projectile.GetAlpha(lightColor) with { A = 0 } * 0.65f, projectile.rotation + num287,
                new Vector2((float)value173.Width / 2f, (float)num288 / 2f), scale18 * overallScale, dirSE);

            Main.EntitySpriteDraw(value173, position18, new Rectangle(0, y20, value173.Width, num288), projectile.GetAlpha(lightColor) with { A = 0 } * 0.65f, projectile.rotation + ((float)Math.PI * 2f - num287),
                new Vector2((float)value173.Width / 2f, (float)num288 / 2f), scale19 * overallScale, dirSE);

            #endregion

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("Dusts", () =>
            {
                DrawAura(projectile, false);
            });
            DrawAura(projectile, true);

            return false;
        }

        public void DrawAura(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle").Value;
            Texture2D OrbPMA = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);

            #region DrawElectric
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;


            float orbRot = timer * 0.08f * dir * 0f;
            float orbScale = 0.31f * overallScale * projectile.scale * 1f;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/foam_mask_bloom").Value); //foam_mask_bloom
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/ThunderGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value);
            myEffect.Parameters["flowSpeed"].SetValue(1f);
            myEffect.Parameters["distortStrength"].SetValue(0.1f); //0.1
            myEffect.Parameters["uTime"].SetValue(timer * 0.015f);

            myEffect.Parameters["vignetteSize"].SetValue(0.15f);
            myEffect.Parameters["vignetteBlend"].SetValue(1f);
            myEffect.Parameters["colorIntensity"].SetValue(0.9f);

            Main.spriteBatch.Draw(OrbPMA, drawPos, null, Color.DeepSkyBlue with { A = 0 } * 0.2f, orbRot, OrbPMA.Size() / 2, orbScale * 5f, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Border, drawPos, null, Color.DeepSkyBlue with { A = 0 } * 0.25f, orbRot, Border.Size() / 2, orbScale * 0.65f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, default, myEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(Orb, drawPos, null, Color.White, orbRot, Orb.Size() / 2, orbScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Orb, drawPos, null, Color.White, orbRot, Orb.Size() / 2, orbScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.AlphaBlend, Main.DefaultSamplerState, default, default, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            #endregion
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;
        }
    }
}
