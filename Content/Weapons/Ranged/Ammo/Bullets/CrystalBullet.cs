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
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Interfaces;
using Terraria.GameContent.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Bullets
{

    public class CrystalBulletProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalBullet);
        }

        public bool isBlue;

        float randomTrailSpeed = 1f;
        float randomTimeOffset = 0;
        int trailRandomLengthOffset = 0;
        BaseTrailInfo trail1 = new BaseTrailInfo();
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //We want each bullet to feel a little different, so we randomize the length of the trail a little bit and offset the trail time
            //Without this, bullets can often feel very weird when fired at the same (shotguns)
            if (timer == 0)
            {
                randomTimeOffset = Main.rand.NextFloat(0f, 10f);
                trailRandomLengthOffset = Main.rand.Next(0, 35);
                randomTrailSpeed = Main.rand.NextFloat(0.85f, 1.15f);

                isBlue = Main.player[projectile.owner].GetModPlayer<CrystalBulletPlayer>().makeBlue;
                Main.player[projectile.owner].GetModPlayer<CrystalBulletPlayer>().makeBlue = !isBlue;
            }

            Color col = isBlue ? Color.DeepSkyBlue : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

            //Trail1 Info Dump
            trail1.trailPointLimit = 135 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(12 * totalAlpha);
            trail1.trailMaxLength = 135 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;

            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 5f;
            trail1.TrailLogic();



            int trailCount = 14; //34
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer > 5 && timer % 3 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            //Quickly fade in
            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.25f, 0.08f), 0f, 1f); //1.15

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 4) / timeForPopInAnim, 0f, 1f);
            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2f)) * 1f;

            Lighting.AddLight(projectile.Center, col.ToVector3() * (isBlue ? 0.45f : 0.75f));

            timer++;
            return base.PreAI(projectile);
        }

        float totalScale = 0f;
        float totalAlpha = 0f;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Dont draw on frame one
            if (timer == 0)
                return false;

            DrawTrail(projectile, false);


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawBody(projectile, true);
            });
            DrawBody(projectile, false);

            return false;
        }

        public void DrawBody(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;


            Color darkest = isBlue ? Color.DodgerBlue : Color.DeepPink;
            Color middle = isBlue ? Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.5f) : Color.DeepPink;
            Color brightest = isBlue ? Color.DeepSkyBlue : Color.HotPink;


            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
            Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10f);
            drawPos += new Vector2(0f, 0f);

            float drawRot = projectile.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = projectile.scale * (5f / 6f);

            Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale * 1f) * 0.5f;

            Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, darkest with { A = 140 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

            Vector2 orbScale = new Vector2(1f, 0.25f * totalScale * 1f) * 0.7f * adjustedScale; //0.3
            Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, middle with { A = 140 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);


            Texture2D spike2 = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/StarlightLessGlow").Value;

            Vector2 drawScale2 = new Vector2(adjustedScale * 2f, adjustedScale * totalScale * 1f) * 0.45f;

            drawPos += new Vector2(0f, 0f);
            Main.spriteBatch.Draw(spike2, drawPos, null, brightest with { A = 90 } * totalAlpha, drawRot, drawOrigin, drawScale2, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(spike2, drawPos, null, Color.White with { A = 90 } * totalAlpha * 0.75f, drawRot, drawOrigin, drawScale2 * 0.6f, SpriteEffects.None, 0f);
        }

        Effect trailEffect = null;
        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;
            Main.pixelShader.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //isBlue = true;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/BasicGlowTrail", AssetRequestMode.ImmediateLoad).Value;


            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine3").Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = trail1.trailPositions.ToArray();
            float[] rot_arr = trail1.trailRotations.ToArray();

            Color trailDark = isBlue ? Color.DodgerBlue : Color.DeepPink;
            Color trailLight = isBlue ? Color.Lerp(Color.DeepSkyBlue, Color.DodgerBlue, 0f) : Color.HotPink;
            Color StripColor(float progress) => Color.Lerp(trailDark, trailLight, progress) with { A = 215 } * Easings.easeOutQuad(progress) * (isBlue ? 0.4f : 0.5f);

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.18f;

                float toReturn = Easings.easeOutSine(progress);

                return 12f * totalScale;// * toReturn * totalScale; 
            }

            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.05f);

            trailEffect.Parameters["bodyIntensity"].SetValue(1.25f);
            trailEffect.Parameters["bodyPower"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(0f);

            trailEffect.Parameters["whiteGlowSize"].SetValue(0.5f);
            trailEffect.Parameters["whiteGlowPower"].SetValue(2f);
            trailEffect.Parameters["whiteGlowIntensity"].SetValue(0f);


            trailEffect.Parameters["TrailTexture1"].SetValue(trailTexture1);
            trailEffect.Parameters["tex1reps"].SetValue(3f);
            trailEffect.Parameters["tex1Intensity"].SetValue(isBlue ? 1f : 1f); //2f
            trailEffect.Parameters["tex1YStretch"].SetValue(1.25f); //2f

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(2.5f);
            trailEffect.Parameters["tex2YStretch"].SetValue(0.75f); //2f

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {            
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Volume = 0.5f, Pitch = -.7f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            int soundVariant1 = Main.rand.Next(3);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_" + soundVariant1) with { Volume = 0.2f, Pitch = .1f, PitchVariance = .25f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);


            Color col = isBlue ? Color.DeepSkyBlue : Color.DeepPink;


            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            //Particles on trail
            int count = trail1.trailPositions.Count;
            for (int i = (int)(count * 0.3f); i < count; i += 5)
            {
                if (Main.rand.NextBool())
                {
                    
                    Vector2 pos = trail1.trailPositions[i];
                    Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);

                    //new Color(255, 120, 40)
                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelAlts>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.4f);
                    d.alpha = 2;
                    d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.35f;
                    d.velocity *= 0.35f;
                }
            }

            for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: col * 1f, Scale: Main.rand.NextFloat(0.15f, 0.35f));
            }

            #region vanilla Kill
            ///SoundEngine.PlaySound(0, (int)base.position.X, (int)base.position.Y);
            for (int num581 = 11110; num581 < 5; num581++)
            {
                int num582 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 68);
                Main.dust[num582].noGravity = true;
                Dust dust195 = Main.dust[num582];
                Dust dust334 = dust195;
                dust334.velocity *= 1.5f;
                dust195 = Main.dust[num582];
                dust334 = dust195;
                dust334.scale *= 0.9f;
            }
            if (projectile.type == 89 && projectile.owner == Main.myPlayer)
            {
                for (int num583 = 0; num583 < 2; num583++)
                {
                    float num584 = (0f - projectile.velocity.X) * (float)Main.rand.Next(40, 70) * 0.01f + (float)Main.rand.Next(-20, 21) * 0.4f;
                    float num587 = (0f - projectile.velocity.Y) * (float)Main.rand.Next(40, 70) * 0.01f + (float)Main.rand.Next(-20, 21) * 0.4f;
                    int a = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + num584, projectile.position.Y + num587, num584, num587, 90, (int)((double)projectile.damage * 0.5), 0f, projectile.owner);

                    //! I can't image ai2 would be used for anything important
                    Main.projectile[a].ai[2] = isBlue ? 1 : -1;
                }
            }
            #endregion

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    
    public class CrystalBulletShardOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;


        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalShard);
        }

        public bool isBlue = false;

        float overallScale = 0f;
        float overallAlpha = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                isBlue = projectile.ai[2] == 1;


            Color col = isBlue ? Color.DeepSkyBlue : Color.DeepPink;

            //Always release a dust every 2 frames if velocity is above threshold
            bool aboveThresh = projectile.velocity.Length() > 10f;
            bool shouldSpawnDust = (aboveThresh || Main.rand.NextBool(3));

            //Dust
            if (timer % 2 == 0 && timer > 5) 
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowPixelCross>(), newColor: col, Scale: Main.rand.NextFloat(0.15f, 0.25f) * projectile.scale);

                if (aboveThresh)
                    Main.dust[d].velocity *= 1.25f;
                Main.dust[d].velocity += projectile.velocity * 0.43f;
                Main.dust[d].velocity *= 0.4f;

                Main.dust[d].customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.25f, timeBeforeSlow: 8, preSlowPower: 0.97f, postSlowPower: 0.88f,
                    velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: true);
            }


            float timeForScaleIn = Math.Clamp((timer + 12) / 24f, 0f, 1f); //40
            overallScale = Easings.easeInOutBack(timeForScaleIn, 0f, 2f);

            if (projectile.scale > 0.9f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);
            else
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.25f, 0.07f), 0f, 1f);

            //Lighting.AddLight(projectile.Center, col.ToVector3() * projectile.scale * 0.5f);

            timer++;

            return true;
        }


        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Color colToUse = isBlue ? Color.DeepSkyBlue : Color.DeepPink;
            colToUse *= overallAlpha;

            //Utils.DrawBorderString(Main.spriteBatch, "" + overallAlpha, projectile.Center - Main.screenPosition, Color.White);

            Texture2D GlowTex = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Tomes/CrystalShardWhiteGlow").Value;
            Texture2D SoftGlow = Mod.Assets.Request<Texture2D>("Assets/Orbs/SoftGlow64").Value;

            Texture2D BlueTex = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Tomes/CrystalShardBlue").Value;
            Texture2D PinkTex = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Tomes/CrystalShardPink").Value;

            Texture2D TexToUse = isBlue ? BlueTex : PinkTex;

            Vector2 DrawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = TexToUse.Size() / 2f;

            float totalScale = projectile.scale * overallScale;

            //Border
            for (int i = 0; i < 4; i++) //4
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);

                Main.EntitySpriteDraw(TexToUse, DrawPos + offset, null, Color.White with { A = 0 } * 2f * overallAlpha, projectile.rotation, TexOrigin, totalScale, SpriteEffects.None);

            }

            //Under Glows
            Main.EntitySpriteDraw(SoftGlow, DrawPos, null, colToUse with { A = 0 } * 0.15f, projectile.rotation, SoftGlow.Size() / 2f, totalScale * 0.45f, SpriteEffects.None);

            Main.EntitySpriteDraw(GlowTex, DrawPos, null, colToUse with { A = 0 }, projectile.rotation, GlowTex.Size() / 2f, totalScale, SpriteEffects.None);
            Main.EntitySpriteDraw(GlowTex, DrawPos, null, colToUse with { A = 0 }, projectile.rotation, GlowTex.Size() / 2f, totalScale * 0.5f, SpriteEffects.None);


            //Main Tex
            Main.EntitySpriteDraw(TexToUse, DrawPos, null, lightColor * overallAlpha, projectile.rotation, TexOrigin, totalScale, SpriteEffects.None);

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            
            return base.PreKill(projectile, timeLeft);
        }

    }
    

    //Literally just so that the crystal bullets alternate color
    public class CrystalBulletPlayer : ModPlayer
    {
        public bool makeBlue = false;
    }
}
