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

    public class CrystalBulletProjOverride : GlobalProjectile, IDrawAdditive
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
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value;
            trail1.trailPointLimit = 135 + trailRandomLengthOffset;
            trail1.trailWidth = (int)(12 * totalAlpha);
            trail1.trailMaxLength = 135 + trailRandomLengthOffset; //120

            trail1.shouldSmooth = false;
            trail1.trailColor = col * totalAlpha;


            trail1.trailTime = randomTimeOffset + (timer * 0.05f * randomTrailSpeed);
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
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
            totalScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 4f)) * 1f;

            Lighting.AddLight(projectile.Center, col.ToVector3() * (isBlue ? 0.45f : 0.75f));

            proj = projectile;
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

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;
                Texture2D orb = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/feather_circle128PMA").Value;

                Vector2 drawPos = projectile.Center - Main.screenPosition + (projectile.velocity.SafeNormalize(Vector2.UnitX) * -10) + new Vector2(0f, 0f);
                float drawRot = projectile.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = projectile.scale * (5f / 6f);

                Color spikeCol = isBlue ? Color.DodgerBlue : Color.DeepPink;
                Vector2 outSpikeScale = new Vector2(adjustedScale * 2.15f, adjustedScale * 1.5f * totalScale) * 0.5f;
                Main.EntitySpriteDraw(spike, drawPos + new Vector2(0f, 0f), null, spikeCol with { A = 0 } * 0.5f * totalAlpha, drawRot, drawOrigin, outSpikeScale, SpriteEffects.None);

                Color orbCol = isBlue ? Color.DeepSkyBlue : Color.DeepPink;
                Vector2 orbScale = new Vector2(1f, 0.3f * totalScale) * 0.7f * adjustedScale;
                Main.EntitySpriteDraw(orb, drawPos + new Vector2(0f, 0f), null, orbCol with { A = 0 } * 0.3f * totalAlpha, drawRot, orb.Size() / 2f, orbScale, SpriteEffects.None);

            });

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                //Need to not draw if projectile is false because otherwise it will draw wrong on the frame it is killed (due to pixelation system)
                if (projectile.active == false)
                    totalAlpha = 0f;

                Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;

                Vector2 drawPos = proj.Center - Main.screenPosition + (proj.velocity.SafeNormalize(Vector2.UnitX) * -12);
                float drawRot = proj.velocity.ToRotation();
                Vector2 drawOrigin = spike.Size() / 2f;

                //Vanilla has 1.2 scale for bullets, so normalize this to 1f
                float adjustedScale = proj.scale * (5f / 6f);
                Vector2 drawScale = new Vector2(adjustedScale * 2f, adjustedScale * totalScale * 0.9f) * 0.5f;

                Color thisCol = isBlue ? Color.DeepSkyBlue : Color.DeepPink;


                Main.spriteBatch.Draw(spike, drawPos, null, thisCol * 2f * totalAlpha, drawRot, drawOrigin, drawScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(spike, drawPos, null, Color.White * totalAlpha, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None, 0f);

            });

            return false;
        }

        //Need this for DrawAdditve to have projectile pos and stuff
        Projectile proj = null;
        public void DrawAdditive(SpriteBatch sb)
        {
            if (proj == null)
                return;

            Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Starlight").Value;

            Vector2 drawPos = proj.Center - Main.screenPosition + (proj.velocity.SafeNormalize(Vector2.UnitX) * -10) + new Vector2(0f, 0f);
            float drawRot = proj.velocity.ToRotation();
            Vector2 drawOrigin = spike.Size() / 2f;

            //Vanilla has 1.2 scale for bullets, so normalize this to 1f
            float adjustedScale = proj.scale * (5f / 6f);
            Vector2 drawScale = new Vector2(adjustedScale * 2f, adjustedScale * totalScale) * 0.5f;

            Color col = isBlue ? Color.SkyBlue : Color.HotPink;

            //sb.Draw(spike, drawPos, null, col * totalAlpha, drawRot, drawOrigin, drawScale, SpriteEffects.None, 0f);
            sb.Draw(spike, drawPos, null, Color.White * totalAlpha, drawRot, drawOrigin, drawScale * 0.5f, SpriteEffects.None, 0f);

            trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
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
