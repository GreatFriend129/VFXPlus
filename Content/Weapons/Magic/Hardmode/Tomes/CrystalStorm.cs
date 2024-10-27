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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    
    public class CrystalStorm : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.CrystalStorm);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle stylea = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Volume= 0.2f, Pitch = 1f, PitchVariance = .35f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(stylea, player.Center);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/star_impact_01") with { Volume= 0.2f, Pitch = 0.15f, PitchVariance = .35f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class CrystalStormShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool isBlue = false;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalStorm);
        }


        bool justTileCollided = false;
        float fadeInPower = 0f;
        float overallAlpha = 0f;

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                isBlue = Main.rand.NextBool();
            

            Color col = isBlue ? Color.DeepSkyBlue : Color.DeepPink;

            #region trail info
            //Trail1 info dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value;
            trail1.trailColor = col * 0.7f; //0.7
            trail1.trailPointLimit = 120;
            trail1.trailWidth = (int)(15f * projectile.scale); //8f
            trail1.trailMaxLength = 120; //120

            trail1.shouldSmooth = false;
            if (justTileCollided && timer > 0 && trail1.trailRotations.Count >= 1)
            {
                float oldRot = trail1.trailRotations[trail1.trailRotations.Count() - 1];
                float currentRot = projectile.velocity.ToRotation();

                float newRot = Utils.AngleLerp(oldRot, currentRot, 0.5f);

                trail1.trailRotations[trail1.trailRotations.Count() - 1] = newRot;

                trail1.trailPositions.Add(projectile.Center);// projectile.velocity;
                trail1.trailRotations.Add(projectile.velocity.ToRotation());
                
                justTileCollided = false;
            }


            trail1.trailTime = timer * 0.05f;
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;

            trail1.TrailLogic();
            #endregion


            //Always release a dust every 2 frames if velocity is above threshold
            bool aboveThresh = projectile.velocity.Length() > 10f;
            bool shouldSpawnDust = (aboveThresh || Main.rand.NextBool(3));

            //Dust
            if (timer % 2 == 0 && shouldSpawnDust && timer > 5) //timer mod 1 with high res smoke looks cool
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowPixelCross>(), newColor: col, Scale: Main.rand.NextFloat(0.15f, 0.25f) * projectile.scale);

                if (aboveThresh)
                    Main.dust[d].velocity *= 1.25f;
                Main.dust[d].velocity += projectile.velocity * 0.43f;
                Main.dust[d].velocity *= 0.4f;

                Main.dust[d].customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.25f, timeBeforeSlow: 8, preSlowPower: 0.97f, postSlowPower: 0.88f,
                    velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: true);
            }

            if (timer % 4 == 0 && Main.rand.NextBool())
            {


                //Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), 
                //    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.NextFloat(2f, 4f),
                //    newColor: col * 1.25f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.25f);

                //p.velocity += projectile.velocity * 0.4f;
                /*
                Vector2 vel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * Main.rand.NextFloat(1f, 1.75f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    vel, newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.25f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, timeBeforeSlow: 5, preSlowPower: 0.99f, postSlowPower: 0.91f, 
                    velToBeginShrink: 1.25f, fadePower: 0.93f, shouldFadeColor: false);

                p.velocity += projectile.velocity * 1f;
                */
            }


            float timeForSpinIn = Math.Clamp(timer / 40f, 0f, 1f); //40
            fadeInPower = Easings.easeOutSine(timeForSpinIn);

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);

            Lighting.AddLight(projectile.Center, col.ToVector3() * projectile.scale * 0.5f);

            timer++;

            #region VanillaAI minus dust

            projectile.light = projectile.scale * 0.5f;
            projectile.rotation += projectile.velocity.X * 0.2f;
            projectile.ai[1] += 1f;
            if (projectile.type == 94)
            {
                if (false)//Main.rand.Next(4) == 0)
                {
                    int num305 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 70);
                    Main.dust[num305].noGravity = true;
                    Dust dust50 = Main.dust[num305];
                    Dust dust2 = dust50;
                    dust2.velocity *= 0.5f;
                    dust50 = Main.dust[num305];
                    dust2 = dust50;
                    dust2.scale *= 0.9f;
                }
                projectile.velocity *= 0.985f;
                if (projectile.ai[1] > 130f)
                {
                    projectile.scale -= 0.05f;
                    if ((double)projectile.scale <= 0.2)
                    {
                        for (int i = 0; i < 4 + Main.rand.Next(1, 3); i++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(1.75f, 1.75f);

                            Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                                newColor: col * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.35f));
                        }

                        projectile.scale = 0.2f;
                        projectile.Kill();
                    }
                }
            }

            #endregion
            return false;
        }

        public BaseTrailInfo trail1 = new BaseTrailInfo();

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Utils.DrawBorderString(Main.spriteBatch, "" + projectile.scale, projectile.Center - Main.screenPosition + new Vector2(0f, -100), Color.White);

            //return true;

            trail1.TrailDrawing(Main.spriteBatch);

            Color colToUse = isBlue ? Color.DeepSkyBlue : Color.DeepPink;
            colToUse *= overallAlpha;

            
            Texture2D GlowTex = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Tomes/CrystalShardWhiteGlow").Value;
            Texture2D SoftGlow = Mod.Assets.Request<Texture2D>("Assets/Orbs/SoftGlow64").Value;
            Texture2D Star = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;

            Texture2D BlueTex = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Tomes/CrystalShardBlue").Value;
            Texture2D PinkTex = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Tomes/CrystalShardPink").Value;

            Texture2D TexToUse = isBlue ? BlueTex : PinkTex;

            Vector2 DrawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = TexToUse.Size() / 2f;

            //Star
            float starAlpha = (1f - fadeInPower) * 0.45f * overallAlpha;
            float spinInBonusRot = MathHelper.Lerp(7f * projectile.direction, 0f, Easings.easeOutQuint(fadeInPower));

            Vector2 vec2Scale = new Vector2(1.25f, 0.55f) * projectile.scale * (1f - (fadeInPower * 0.75f));

            Main.EntitySpriteDraw(Star, DrawPos, null, Color.White with { A = 0 } * starAlpha, projectile.velocity.ToRotation(), Star.Size() / 2f, vec2Scale * 0.6f, SpriteEffects.None);
            Main.EntitySpriteDraw(Star, DrawPos, null, Color.White with { A = 0 } * starAlpha, projectile.velocity.ToRotation(), Star.Size() / 2f, vec2Scale * 0.6f, SpriteEffects.None);


            //Border
            for (int i = 0; i < 8; i++) //4
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);

                Main.EntitySpriteDraw(TexToUse, DrawPos + offset, null, Color.White with { A = 0 } * 2f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * 1f, SpriteEffects.None);

            }

            //Under Glows
            Main.EntitySpriteDraw(SoftGlow, DrawPos, null, colToUse with { A = 0 } * 0.15f, projectile.rotation, SoftGlow.Size() / 2f, projectile.scale * 0.45f, SpriteEffects.None);

            Main.EntitySpriteDraw(GlowTex, DrawPos, null, colToUse with { A = 0 }, projectile.rotation, GlowTex.Size() / 2f, projectile.scale, SpriteEffects.None);
            Main.EntitySpriteDraw(GlowTex, DrawPos, null, colToUse with { A = 0 }, projectile.rotation, GlowTex.Size() / 2f, projectile.scale * 0.5f, SpriteEffects.None);


            //Main Tex
            Main.EntitySpriteDraw(TexToUse, DrawPos, null, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale, SpriteEffects.None);

            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color col = isBlue ? Color.DeepSkyBlue : Color.DeepPink;


            for (int i = 0; i < 6 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: col * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale );
            }

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            int soundVariant1 = Main.rand.Next(3);

            soundVariant1 = 2;

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_" + soundVariant1) with { Volume = 0.45f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1, };
            SoundEngine.PlaySound(style, projectile.Center);
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            trail1.trailPositions.RemoveAt(trail1.trailPositions.Count - 1);
            trail1.trailRotations.RemoveAt(trail1.trailRotations.Count - 1);

            justTileCollided = true;

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
