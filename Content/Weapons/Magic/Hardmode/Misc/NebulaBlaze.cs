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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VFXPlus.Common.Drawing;
using static Terraria.ModLoader.PlayerDrawLayer;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class NebulaBlaze : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.NebulaBlaze) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NebulaBlazeToggleToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }
    public class NebulaBlaze1ShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaBlaze1) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NebulaBlazeToggleToggle;
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();
        BaseTrailInfo trail2 = new BaseTrailInfo();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            if (timer == 0)
            {
                Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.35f, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.55f, newColor: Color.HotPink * 0.6f, Scale: 0.01f);
                CirclePulseBehavior b2 = new CirclePulseBehavior(0.65f * 1f, true, 3, 0.2f, 0.4f);
                b2.drawLayer = "OverPlayers";
                d2.customData = b2;

                Dust d2A = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.7f, newColor: Color.HotPink * 0.7f, Scale: 0.01f);
                CirclePulseBehavior b2A = new CirclePulseBehavior(0.65f * 1f, true, 2, 0.15f, 0.3f);
                b2A.drawLayer = "OverPlayers";
                d2A.customData = b2A;
            }

            int trailCount = 22; //16
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 4 == 0 && Main.rand.NextBool(2))
            {
                
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.2f, 0.25f) * 1.5f);

                p.velocity += projectile.velocity * -0.5f;
            }

            if (timer % 1 == 0 && timer > 3 && Main.rand.NextBool())
            {
                int d = Dust.NewDust(projectile.Center - new Vector2(7, 7), 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.3f, 0.4f) * 2f);
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
            }

            #region Trail
            Color thisPink = Color.Lerp(Color.HotPink, Color.DeepPink, 0.08f) * overallAlpha;

            int trueTrailWidth = (int)(27f * overallScale); //20

            if (trueTrailWidth < 3)
                trueTrailWidth = 0;

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/EvenThinnerGlowLine").Value; //EvenThinnerGlowLine == Really clean
            trail1.trailPointLimit = 90;
            trail1.trailWidth = trueTrailWidth;
            trail1.trailMaxLength = 250;
            trail1.timesToDraw = 2;
            trail1.shouldSmooth = false;
            trail1.pinchHead = true;
            trail1.useEffectMatrix = true;

            trail1.trailColor = thisPink;

            float OffsetAmount = 20f * MathF.Sin((float)timer / 30 * projectile.extraUpdates);
            Vector2 offsetPosition = new Vector2(0, OffsetAmount).RotatedBy(projectile.velocity.ToRotation());

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = offsetPosition + projectile.Center + projectile.velocity;//Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5,5);
            trail1.TrailLogic();


            //Trail2 Info Dump
            trail2.trailTexture = trail1.trailTexture;
            trail2.trailPointLimit = 90;
            trail2.trailWidth = trueTrailWidth;
            trail2.trailMaxLength = 250;
            trail2.timesToDraw = 2;
            trail2.shouldSmooth = false;
            trail2.pinchHead = true;
            trail2.useEffectMatrix = true;

            float OffsetAmount2 = 20f * MathF.Sin((float)timer / 30 * projectile.extraUpdates);
            Vector2 offsetPosition2 = new Vector2(0, -OffsetAmount2).RotatedBy(projectile.velocity.ToRotation());

            trail2.trailColor = thisPink;

            trail2.trailRot = projectile.velocity.ToRotation();
            trail2.trailPos = offsetPosition2 + projectile.Center + projectile.velocity;//Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5,5);
            trail2.TrailLogic();

            #endregion

            float timeForPopInAnim = 60;
            float animProgress = Math.Clamp((timer + 25) / timeForPopInAnim, 0f, 1f); //15 60
            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 3.25f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.04f), 0f, 1f);

            timer++;

            #region vanillaAI without dust
            float num28 = 5f;
            float num29 = 250f;
            float num30 = 6f;
            Vector2 vector9 = new Vector2(8f, 10f);
            float num31 = 1.2f;
            Vector3 rgb2 = new Vector3(0.7f, 0.1f, 0.5f);
            int num32 = 4 * projectile.MaxUpdates;
            int num33 = Utils.SelectRandom<int>(Main.rand, 242, 73, 72, 71, 255);
            int num34 = 255;
            if (projectile.ai[1] == 0f)
            {
                projectile.ai[1] = 1f;
                projectile.localAI[0] = -Main.rand.Next(48);
                SoundEngine.PlaySound(in SoundID.Item34, projectile.position);
            }
            else if (projectile.ai[1] == 1f && projectile.owner == Main.myPlayer)
            {
                int num35 = -1;
                float num36 = num29;
                for (int num37 = 0; num37 < 200; num37++)
                {
                    if (Main.npc[num37].active && Main.npc[num37].CanBeChasedBy(this))
                    {
                        Vector2 center3 = Main.npc[num37].Center;
                        float num38 = Vector2.Distance(center3, projectile.Center);
                        if (num38 < num36 && num35 == -1 && Collision.CanHitLine(projectile.Center, 1, 1, center3, 1, 1))
                        {
                            num36 = num38;
                            num35 = num37;
                        }
                    }
                }
                if (num36 < 20f)
                {
                    projectile.Kill();
                    return false;
                }
                if (num35 != -1)
                {
                    projectile.ai[1] = num28 + 1f;
                    projectile.ai[0] = num35;
                    projectile.netUpdate = true;
                }
            }
            else if (projectile.ai[1] > num28)
            {
                projectile.ai[1] += 1f;
                int num39 = (int)projectile.ai[0];
                if (!Main.npc[num39].active || !Main.npc[num39].CanBeChasedBy(this))
                {
                    projectile.ai[1] = 1f;
                    projectile.ai[0] = 0f;
                    projectile.netUpdate = true;
                }
                else
                {
                    projectile.velocity.ToRotation();
                    Vector2 vector10 = Main.npc[num39].Center - projectile.Center;
                    if (vector10.Length() < 20f)
                    {
                        projectile.Kill();
                        return false;
                    }
                    if (vector10 != Vector2.Zero)
                    {
                        vector10.Normalize();
                        vector10 *= num30;
                    }
                    float num40 = 30f;
                    projectile.velocity = (projectile.velocity * (num40 - 1f) + vector10) / num40;
                }
            }
            if (projectile.ai[1] >= 1f && projectile.ai[1] < num28)
            {
                projectile.ai[1] += 1f;
                if (projectile.ai[1] == num28)
                {
                    projectile.ai[1] = 1f;
                }
            }
            projectile.alpha -= 40;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            projectile.spriteDirection = projectile.direction;
            projectile.frameCounter++;
            if (projectile.frameCounter >= num32)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
                if (projectile.frame >= 4)
                {
                    projectile.frame = 0;
                }
            }
            Lighting.AddLight(projectile.Center, rgb2);
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.localAI[0] += 1f;
            if (projectile.localAI[0] == 48f)
            {
                projectile.localAI[0] = 0f;
            }
            else if (projectile.alpha == 0 && false)
            {
                for (int num41 = 0; num41 < 2; num41++)
                {
                    Vector2 vector11 = Vector2.UnitX * -30f;
                    vector11 = -Vector2.UnitY.RotatedBy(projectile.localAI[0] * ((float)Math.PI / 24f) + (float)num41 * (float)Math.PI) * vector9 - projectile.rotation.ToRotationVector2() * 10f;
                    int num42 = Dust.NewDust(projectile.Center, 0, 0, num34, 0f, 0f, 160);
                    Main.dust[num42].scale = num31;
                    Main.dust[num42].noGravity = true;
                    Main.dust[num42].position = projectile.Center + vector11 + projectile.velocity * 2f;
                    Main.dust[num42].velocity = Vector2.Normalize(projectile.Center + projectile.velocity * 2f * 8f - Main.dust[num42].position) * 2f + projectile.velocity * 2f;
                }
            }

            #endregion
            return false;
        }


        float overallAlpha = 0f;
        float overallScale = 0f; 
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            trail1.gradientTime = (float)Main.timeForVisualEffects * 0.02f;
            trail1.trailTime = (float)Main.timeForVisualEffects * 0.03f;

            trail2.gradientTime = (float)Main.timeForVisualEffects * 0.02f;
            trail2.trailTime = (float)Main.timeForVisualEffects * 0.03f;


            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
                trail2.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
            });




            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float drawScale = projectile.scale * overallScale;
            float rot = projectile.velocity.ToRotation();

            //Line
            if (timer > 10 && overallAlpha != 1f)
            {
                Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;

                float lineAlpha = Easings.easeOutCubic(1f - overallAlpha) * 1f;

                Main.EntitySpriteDraw(line, drawPos, null, Color.White with { A = 0 } * lineAlpha, rot, line.Size() / 2f, 1f * new Vector2(1.75f, 0.5f) * projectile.scale, SpriteEffects.None);
                Main.EntitySpriteDraw(line, drawPos, null, Color.HotPink with { A = 0 } * lineAlpha, rot, line.Size() / 2f, 1.5f * new Vector2(1.75f, 0.5f) * projectile.scale, SpriteEffects.None);
            }


            Texture2D orb = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;

            Color col1 = Color.White * 0.55f;
            Color col2 = Color.HotPink * 0.525f * 0.75f;
            Color col3 = Color.DeepPink * 0.375f * 0.5f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            float scale = 1.15f * drawScale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            //Main.EntitySpriteDraw(orb, drawPos, null, col1 with { A = 0 }, rot, orb.Size() / 2f, scale1 * scale * new Vector2(1f, 0.8f * projectile.scale), SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, col2 with { A = 0 } * overallAlpha, rot, orb.Size() / 2f, scale2 * scale * sineScale1 * new Vector2(1f, 0.8f), SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, col3 with { A = 0 } * overallAlpha, rot, orb.Size() / 2f, scale3 * scale * sineScale2 * new Vector2(1f, 0.8f), SpriteEffects.None);



            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects se = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            
            //After-Image
            if (previousRotations != null && previousPositions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = progress * drawScale;// (0.75f + (progress * 0.25f)) * projectile.scale;

                    Color col = Color.Pink * progress * projectile.Opacity * overallAlpha;

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.35f, //0.5f
                            previousRotations[i], TexOrigin, size, se);

                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity * overallAlpha;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle, 
                    Color.Pink with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, 1.2f * drawScale, se);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * projectile.Opacity * overallAlpha, projectile.rotation, TexOrigin, drawScale, se);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 } * 0.35f * projectile.Opacity * overallAlpha, projectile.rotation, TexOrigin, drawScale, se);

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color between = Color.Lerp(Color.HotPink, Color.DeepPink, 0.5f);

            //TrailDust
            int i1 = 0;
            foreach (Vector2 pos in trail1.trailPositions)
            {
                i1++;
                if (i1 % 6 == 0 && i1 > 11)
                {
                    int dust2 = Dust.NewDust(pos, 1, 1, ModContent.DustType<GlowPixelAlts>(), Scale: 0.25f + Main.rand.NextFloat(-0.1f, 0.1f), newColor: Color.HotPink);
                    Main.dust[dust2].velocity *= 0.45f;
                    Main.dust[dust2].velocity += projectile.velocity * 0.25f;
                    Main.dust[dust2].alpha = 2;
                }
            }
            int i2 = 0;
            foreach (Vector2 pos in trail2.trailPositions)
            {
                i2++;
                if (i2 % 6 == 0 && i2 > 11)
                {
                    int dust2 = Dust.NewDust(pos, 1, 1, ModContent.DustType<GlowPixelAlts>(), Scale: 0.25f + Main.rand.NextFloat(-0.1f, 0.1f), newColor: Color.HotPink);
                    Main.dust[dust2].velocity *= 0.45f;
                    Main.dust[dust2].velocity += projectile.velocity * 0.25f;
                    Main.dust[dust2].alpha = 2;
                }
            }

            for (int i = 0; i < 11 + Main.rand.Next(0, 6); i++)
            {

                float velMult = Main.rand.NextFloat(1.5f, 6.5f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0,
                    newColor: between, Scale: Main.rand.NextFloat(0.35f, 0.55f));

                dust.velocity += projectile.velocity * 1f;

                dust.velocity *= 0.9f;

                dust.scale *= 1.75f;

                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }

            for (int i = 0; i < 9 + Main.rand.Next(2); i++)
            {
                Color col = Main.rand.NextBool(3) ? Color.DeepPink : Color.HotPink;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 7f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.8f, 1.25f));
                d.velocity += projectile.velocity * 0.9f;

                d.velocity *= 0.7f;


                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

            }

            for (int i = 0; i < 7 + Main.rand.Next(0, 5); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: between, Scale: Main.rand.NextFloat(0.65f, 0.75f));

                dust.velocity += projectile.velocity * 0.2f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            Dust softGlow = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepPink, Scale: 0.25f);
            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.91f, sizeChangeSpeed: 0.92f, timeToKill: 150,
                overallAlpha: 0.21f, DrawWhiteCore: true, 1f, 1f);

            //SoftGlowDustBehavior sgbt = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.87f, sizeChangeSpeed: 0.80f, timeToKill: 150,
                //overallAlpha: 0.27f, DrawWhiteCore: true, 1f, 1f);
            
            //Dust softGlow2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity + new Vector2(100f, 0f), ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepPink, Scale: 0.35f);
            //softGlow2.customData = sgbt;
            //Dust softGlow3 = Dust.NewDustPerfect(projectile.Center + projectile.velocity + new Vector2(100f, 0f), ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.HotPink, Scale: 0.25f);
            //softGlow3.customData = sgbt;
            //Dust softGlow4 = Dust.NewDustPerfect(projectile.Center + projectile.velocity + new Vector2(100f, 0f), ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.LightPink, Scale: 0.15f);
            //softGlow4.customData = sgbt;


            #region vanillaKill
            int num121 = Utils.SelectRandom<int>(Main.rand, 242, 73, 72, 71, 255);
            int num122 = 255;
            int num123 = 255;
            int num124 = 50;
            float num125 = 1.7f;
            float num126 = 0.8f;
            float num127 = 2f;
            Vector2 vector25 = (projectile.rotation - (float)Math.PI / 2f).ToRotationVector2();
            Vector2 vector26 = vector25 * projectile.velocity.Length() * projectile.MaxUpdates;
            if (projectile.type == 635)
            {
                num122 = 88;
                num123 = 88;
                num121 = Utils.SelectRandom<int>(Main.rand, 242, 59, 88);
                num125 = 3.7f;
                num126 = 1.5f;
                num127 = 2.2f;
                vector26 *= 0.5f;
            }
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = num124);
            projectile.Center = projectile.position;
            projectile.maxPenetrate = -1;
            projectile.penetrate = -1;
            projectile.Damage();
            for (int num128 = 220; num128 < 40; num128++)
            {
                num121 = Utils.SelectRandom<int>(Main.rand, 242, 73, 72, 71, 255);
                if (projectile.type == 635)
                {
                    num121 = Utils.SelectRandom<int>(Main.rand, 242, 59, 88);
                }
                int num129 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num121, 0f, 0f, 200, default(Color), num125);
                Main.dust[num129].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
                Main.dust[num129].noGravity = true;
                Dust dust71 = Main.dust[num129];
                Dust dust3 = dust71;
                dust3.velocity *= 3f;
                dust71 = Main.dust[num129];
                dust3 = dust71;
                dust3.velocity += vector26 * Main.rand.NextFloat();
                num129 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num122, 0f, 0f, 100, default(Color), num126);
                Main.dust[num129].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
                dust71 = Main.dust[num129];
                dust3 = dust71;
                dust3.velocity *= 2f;
                Main.dust[num129].noGravity = true;
                Main.dust[num129].fadeIn = 1f;
                Main.dust[num129].color = Color.Crimson * 0.5f;
                dust71 = Main.dust[num129];
                dust3 = dust71;
                dust3.velocity += vector26 * Main.rand.NextFloat();
            }
            for (int num130 = 220; num130 < 20; num130++)
            {
                int num131 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num123, 0f, 0f, 0, default(Color), num127);
                Main.dust[num131].position = projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(projectile.velocity.ToRotation()) * projectile.width / 3f;
                Main.dust[num131].noGravity = true;
                Dust dust72 = Main.dust[num131];
                Dust dust3 = dust72;
                dust3.velocity *= 0.5f;
                dust72 = Main.dust[num131];
                dust3 = dust72;
                dust3.velocity += vector26 * (0.6f + 0.6f * Main.rand.NextFloat());
            }

            #endregion
            return false;
            return base.PreKill(projectile, timeLeft);
        }

    }

    public class NebulaBlaze2ShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NebulaBlaze2) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NebulaBlazeToggleToggle;
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();
        BaseTrailInfo trail2 = new BaseTrailInfo();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Color betweenBlue = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);
            Color betweenBlue2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.25f);

            if (timer == 0)
            {
                Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.35f, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.55f, newColor: betweenBlue2 * 0.6f, Scale: 0.01f);
                CirclePulseBehavior b2 = new CirclePulseBehavior(0.75f * 1f, true, 3, 0.25f, 0.45f);
                b2.drawLayer = "OverPlayers";
                d2.customData = b2;

                Dust d2A = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.7f, newColor: betweenBlue2 * 0.7f, Scale: 0.01f);
                CirclePulseBehavior b2A = new CirclePulseBehavior(0.75f * 1f, true, 2, 0.2f, 0.35f);
                b2A.drawLayer = "OverPlayers";
                d2A.customData = b2A;
            }

            int trailCount = 22;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer % 4 == 0 && Main.rand.NextBool(2))
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: betweenBlue, Scale: Main.rand.NextFloat(0.2f, 0.25f) * 1.5f);

                p.velocity += projectile.velocity * -0.5f;
            }

            if (timer % 1 == 0 && timer > 3 && Main.rand.NextBool())
            {
                int d = Dust.NewDust(projectile.Center - new Vector2(7, 7), 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: betweenBlue, Scale: Main.rand.NextFloat(0.3f, 0.4f) * 2f);
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
            }

            #region Trail
            Color thisBlue = betweenBlue2 * overallAlpha;// Color.Lerp(Color.HotPink, Color.DeepPink, 0f) * overallAlpha;

            int trueTrailWidth = (int)(35f * overallScale); //20

            if (trueTrailWidth < 3)
                trueTrailWidth = 0;

            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value;
            trail1.trailPointLimit = 85;
            trail1.trailWidth = trueTrailWidth;
            trail1.trailMaxLength = 250;
            trail1.timesToDraw = 2;
            trail1.shouldSmooth = false;
            trail1.pinchHead = true;
            trail1.useEffectMatrix = true;

            trail1.trailColor = thisBlue;

            float OffsetAmount = 25f * MathF.Sin((float)timer / 35 * projectile.extraUpdates);
            Vector2 offsetPosition = new Vector2(0, OffsetAmount).RotatedBy(projectile.velocity.ToRotation());

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = offsetPosition + projectile.Center + projectile.velocity;//Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5,5);
            trail1.TrailLogic();


            //Trail2 Info Dump
            trail2.trailTexture = trail1.trailTexture;
            trail2.trailPointLimit = 85;
            trail2.trailWidth = trueTrailWidth;
            trail2.trailMaxLength = 250;
            trail2.timesToDraw = 2;
            trail2.shouldSmooth = false;
            trail2.pinchHead = true;
            trail2.useEffectMatrix = true;

            float OffsetAmount2 = 25f * MathF.Sin((float)timer / 35 * projectile.extraUpdates);
            Vector2 offsetPosition2 = new Vector2(0, -OffsetAmount2).RotatedBy(projectile.velocity.ToRotation());

            trail2.trailColor = thisBlue;

            trail2.trailRot = projectile.velocity.ToRotation();
            trail2.trailPos = offsetPosition2 + projectile.Center + projectile.velocity;//Projectile.Center + Projectile.velocity + Main.rand.NextVector2Circular(5,5);
            trail2.TrailLogic();

            #endregion

            float timeForPopInAnim = 60;
            float animProgress = Math.Clamp((timer + 25) / timeForPopInAnim, 0f, 1f); //15 60
            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 3.25f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.04f), 0f, 1f);


            timer++;
            #region vanillaWithoutDust
            float num28 = 5f;
            float num29 = 250f;
            float num30 = 6f;
            Vector2 vector9 = new Vector2(8f, 10f);
            float num31 = 1.2f;
            Vector3 rgb2 = new Vector3(0.7f, 0.1f, 0.5f);
            int num32 = 4 * projectile.MaxUpdates;
            int num33 = Utils.SelectRandom<int>(Main.rand, 242, 73, 72, 71, 255);
            int num34 = 255;
            if (projectile.type == 635)
            {
                vector9 = new Vector2(10f, 20f);
                num31 = 1f;
                num29 = 500f;
                num34 = 88;
                num32 = 3 * projectile.MaxUpdates;
                rgb2 = new Vector3(0.4f, 0.6f, 0.9f);
                num33 = Utils.SelectRandom<int>(Main.rand, 242, 59, 88);
            }
            if (projectile.ai[1] == 0f)
            {
                projectile.ai[1] = 1f;
                projectile.localAI[0] = -Main.rand.Next(48);
                SoundEngine.PlaySound(in SoundID.Item34, projectile.position);
            }
            else if (projectile.ai[1] == 1f && projectile.owner == Main.myPlayer)
            {
                int num35 = -1;
                float num36 = num29;
                for (int num37 = 0; num37 < 200; num37++)
                {
                    if (Main.npc[num37].active && Main.npc[num37].CanBeChasedBy(this))
                    {
                        Vector2 center3 = Main.npc[num37].Center;
                        float num38 = Vector2.Distance(center3, projectile.Center);
                        if (num38 < num36 && num35 == -1 && Collision.CanHitLine(projectile.Center, 1, 1, center3, 1, 1))
                        {
                            num36 = num38;
                            num35 = num37;
                        }
                    }
                }
                if (num36 < 20f)
                {
                    projectile.Kill();
                    return false;
                }
                if (num35 != -1)
                {
                    projectile.ai[1] = num28 + 1f;
                    projectile.ai[0] = num35;
                    projectile.netUpdate = true;
                }
            }
            else if (projectile.ai[1] > num28)
            {
                projectile.ai[1] += 1f;
                int num39 = (int)projectile.ai[0];
                if (!Main.npc[num39].active || !Main.npc[num39].CanBeChasedBy(this))
                {
                    projectile.ai[1] = 1f;
                    projectile.ai[0] = 0f;
                    projectile.netUpdate = true;
                }
                else
                {
                    projectile.velocity.ToRotation();
                    Vector2 vector10 = Main.npc[num39].Center - projectile.Center;
                    if (vector10.Length() < 20f)
                    {
                        projectile.Kill();
                        return false;
                    }
                    if (vector10 != Vector2.Zero)
                    {
                        vector10.Normalize();
                        vector10 *= num30;
                    }
                    float num40 = 30f;
                    projectile.velocity = (projectile.velocity * (num40 - 1f) + vector10) / num40;
                }
            }
            if (projectile.ai[1] >= 1f && projectile.ai[1] < num28)
            {
                projectile.ai[1] += 1f;
                if (projectile.ai[1] == num28)
                {
                    projectile.ai[1] = 1f;
                }
            }
            projectile.alpha -= 40;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            projectile.spriteDirection = projectile.direction;
            projectile.frameCounter++;
            if (projectile.frameCounter >= num32)
            {
                projectile.frame++;
                projectile.frameCounter = 0;
                if (projectile.frame >= 4)
                {
                    projectile.frame = 0;
                }
            }
            Lighting.AddLight(projectile.Center, rgb2);
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.localAI[0] += 1f;
            if (projectile.localAI[0] == 48f)
            {
                projectile.localAI[0] = 0f;
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
            trail1.gradientTime = (float)Main.timeForVisualEffects * 0.02f;
            trail1.trailTime = (float)Main.timeForVisualEffects * 0.03f;

            trail2.gradientTime = (float)Main.timeForVisualEffects * 0.02f;
            trail2.trailTime = (float)Main.timeForVisualEffects * 0.03f;

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
                trail2.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
            });

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float drawScale = projectile.scale * overallScale;
            float rot = projectile.velocity.ToRotation();

            //Line
            if (timer > 10 && overallAlpha != 1f)
            {
                Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;

                float lineAlpha = Easings.easeOutCubic(1f - overallAlpha) * 1f;

                Main.EntitySpriteDraw(line, drawPos, null, Color.White with { A = 0 } * lineAlpha, rot, line.Size() / 2f, 1f * new Vector2(1.75f, 0.5f) * projectile.scale, SpriteEffects.None);
                Main.EntitySpriteDraw(line, drawPos, null, Color.SkyBlue with { A = 0 } * lineAlpha, rot, line.Size() / 2f, 1.5f * new Vector2(1.75f, 0.5f) * projectile.scale, SpriteEffects.None);
            }



            Texture2D orb = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;

            Color col2 = Color.SkyBlue * 0.525f * 0.75f;
            Color col3 = Color.DeepSkyBlue * 0.375f * 0.5f;

            float scale2 = 1.6f;
            float scale3 = 2.5f;
            float scale = 1.15f * drawScale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, col2 with { A = 0 } * overallAlpha, rot, orb.Size() / 2f, scale2 * scale * sineScale1 * new Vector2(1f, 0.8f), SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, col3 with { A = 0 } * overallAlpha, rot, orb.Size() / 2f, scale3 * scale * sineScale2 * new Vector2(1f, 0.8f), SpriteEffects.None);



            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects se = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = progress * drawScale;// (0.75f + (progress * 0.25f)) * projectile.scale;

                    Color col = Color.SkyBlue * progress * projectile.Opacity * overallAlpha;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.35f, //0.5f
                            previousRotations[i], TexOrigin, size, se);

                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Color betweenBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

                float opacitySquared = projectile.Opacity * projectile.Opacity * overallAlpha;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), sourceRectangle,
                    betweenBlue with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, 1.2f * drawScale, se);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White * projectile.Opacity * overallAlpha * 0.85f, projectile.rotation, TexOrigin, drawScale, se);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.LightSkyBlue with { A = 0 } * 0.75f * projectile.Opacity * overallAlpha, projectile.rotation, TexOrigin, drawScale, se);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color between = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);

            //TrailDust
            int i1 = 0;
            foreach (Vector2 pos in trail1.trailPositions)
            {
                i1++;
                if (i1 % 6 == 0 && i1 > 11)
                {
                    int dust2 = Dust.NewDust(pos, 1, 1, ModContent.DustType<GlowPixelAlts>(), Scale: 0.25f + Main.rand.NextFloat(-0.1f, 0.1f), newColor: Color.SkyBlue);
                    Main.dust[dust2].velocity *= 0.45f;
                    Main.dust[dust2].velocity += projectile.velocity * 0.25f;
                    Main.dust[dust2].alpha = 2;
                }
            }
            int i2 = 0;
            foreach (Vector2 pos in trail2.trailPositions)
            {
                i2++;
                if (i2 % 6 == 0 && i2 > 11)
                {
                    int dust2 = Dust.NewDust(pos, 1, 1, ModContent.DustType<GlowPixelAlts>(), Scale: 0.25f + Main.rand.NextFloat(-0.1f, 0.1f), newColor: Color.SkyBlue);
                    Main.dust[dust2].velocity *= 0.45f;
                    Main.dust[dust2].velocity += projectile.velocity * 0.25f;
                    Main.dust[dust2].alpha = 2;
                }
            }

            for (int i = 0; i < 11 + Main.rand.Next(0, 6); i++)
            {

                float velMult = Main.rand.NextFloat(1.5f, 6.5f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0,
                    newColor: between, Scale: Main.rand.NextFloat(0.35f, 0.55f));

                dust.velocity += projectile.velocity * 1f;

                dust.velocity *= 0.9f;

                dust.scale *= 1.75f;

                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }

            for (int i = 0; i < 9 + Main.rand.Next(2); i++)
            {
                Color col = Main.rand.NextBool(3) ? Color.DeepSkyBlue : Color.SkyBlue;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 7f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.8f, 1.25f));
                d.velocity += projectile.velocity * 0.9f;

                d.velocity *= 0.7f;


                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;
            }

            for (int i = 0; i < 7 + Main.rand.Next(0, 5); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: between, Scale: Main.rand.NextFloat(0.65f, 0.75f));

                dust.velocity += projectile.velocity * 0.2f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            Dust softGlow = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepSkyBlue, Scale: 0.25f);
            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.91f, sizeChangeSpeed: 0.92f, timeToKill: 150,
                overallAlpha: 0.18f, DrawWhiteCore: true, 1f, 1f);

            #region vanillaKill
            int num121 = Utils.SelectRandom<int>(Main.rand, 242, 73, 72, 71, 255);
            int num122 = 255;
            int num123 = 255;
            int num124 = 50;
            float num125 = 1.7f;
            float num126 = 0.8f;
            float num127 = 2f;
            Vector2 vector25 = (projectile.rotation - (float)Math.PI / 2f).ToRotationVector2();
            Vector2 vector26 = vector25 * projectile.velocity.Length() * projectile.MaxUpdates;
            if (projectile.type == 635)
            {
                num122 = 88;
                num123 = 88;
                num121 = Utils.SelectRandom<int>(Main.rand, 242, 59, 88);
                num125 = 3.7f;
                num126 = 1.5f;
                num127 = 2.2f;
                vector26 *= 0.5f;
            }
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = num124);
            projectile.Center = projectile.position;
            projectile.maxPenetrate = -1;
            projectile.penetrate = -1;
            projectile.Damage();
            for (int num128 = 220; num128 < 40; num128++)
            {
                num121 = Utils.SelectRandom<int>(Main.rand, 242, 73, 72, 71, 255);
                if (projectile.type == 635)
                {
                    num121 = Utils.SelectRandom<int>(Main.rand, 242, 59, 88);
                }
                int num129 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num121, 0f, 0f, 200, default(Color), num125);
                Main.dust[num129].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
                Main.dust[num129].noGravity = true;
                Dust dust71 = Main.dust[num129];
                Dust dust3 = dust71;
                dust3.velocity *= 3f;
                dust71 = Main.dust[num129];
                dust3 = dust71;
                dust3.velocity += vector26 * Main.rand.NextFloat();
                num129 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num122, 0f, 0f, 100, default(Color), num126);
                Main.dust[num129].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * projectile.width / 2f;
                dust71 = Main.dust[num129];
                dust3 = dust71;
                dust3.velocity *= 2f;
                Main.dust[num129].noGravity = true;
                Main.dust[num129].fadeIn = 1f;
                Main.dust[num129].color = Color.Crimson * 0.5f;
                dust71 = Main.dust[num129];
                dust3 = dust71;
                dust3.velocity += vector26 * Main.rand.NextFloat();
            }
            for (int num130 = 220; num130 < 20; num130++)
            {
                int num131 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, num123, 0f, 0f, 0, default(Color), num127);
                Main.dust[num131].position = projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(projectile.velocity.ToRotation()) * projectile.width / 3f;
                Main.dust[num131].noGravity = true;
                Dust dust72 = Main.dust[num131];
                Dust dust3 = dust72;
                dust3.velocity *= 0.5f;
                dust72 = Main.dust[num131];
                dust3 = dust72;
                dust3.velocity += vector26 * (0.6f + 0.6f * Main.rand.NextFloat());
            }

            #endregion
            return false;
            
            #region storedDUstthing
            /*
            for (int i = 0; i < 12; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: between, Scale: Main.rand.NextFloat(0.35f, 0.65f));
                dust.velocity += projectile.velocity * 0.25f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 15; i++)
            {
                Color col = Main.rand.NextBool() ? Color.DeepPink : between;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 7f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.75f, 1.5f));
                d.velocity += projectile.velocity * 0.9f;

                d.velocity *= 0.7f;


                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

            }


            //Smoke
            for (int i = 0; i < 22; i++)
            {
                float prog = (float)i / 22f;

                Color col = Color.Lerp(Color.HotPink, Color.Black, 1f - prog);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 3.4f) * 1.65f,
                    newColor: col with { A = 0 } * prog * 0.5f, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 1.25f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(5, 22), 0.95f, 0.01f, 0.35f); //12 28

                d.velocity += projectile.velocity * 1.5f * (1f - prog);
            }

            for (int m = 220; m < 10; m++)
            {
                float range = m > 3 ? 0.3f : 0.6f;

                float xScaleMinus = Main.rand.NextFloat(0.3f, 1.6f);

                Vector2 normalVel = projectile.velocity.SafeNormalize(Vector2.UnitX);

                Vector2 dustPos = projectile.Center + normalVel * 7f;

                Dust d = Dust.NewDustPerfect(dustPos, ModContent.DustType<MuraLineDust>(), normalVel.RotatedBy(Main.rand.NextFloat(-1 * range, range)) * Main.rand.NextFloat(4f, 14f),
                    newColor: Color.HotPink, Scale: 0.25f);

                d.customData = new MuraLineBehavior(new Vector2(2f - xScaleMinus, 2.5f), VelFadeSpeed: 0.92f);
                //d.customData = new MuraLineBehavior(new Vector2(1f, 1f));

            }

            for (int i = 220; i < 12; i++)
            {
                float velVal = Main.rand.NextFloat(4f, 8f) * 1.5f;
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(),
                        projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.4f, 0.41f)) * velVal, newColor: between, Scale: 1f);

                //d.customData = new GlowFlareBehavior(0.4f, 2.5f);
            }
            */
            #endregion

        }

    }

}
