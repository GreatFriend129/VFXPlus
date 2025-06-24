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
using Terraria.GameContent.Drawing;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class SkyFracture : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SkyFracture) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SkyFractureToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            Vector2 pos = position + velocity.SafeNormalize(Vector2.UnitX) * 30;

            for (int i = 10; i < 2 + Main.rand.Next(0, 3); i++) //2 //0,3
            {

                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = pos,
                    MovementVector = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-1.5f, 1.5f)) * Main.rand.NextFloat(0f, 4f)
                };
                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.SilverBulletSparkle, particleSettings);

            }


            return true;
        }

    }
    public class SkyFractureShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SkyFracture) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SkyFractureToggle;
        }

        float sineOffset = Main.rand.NextFloat(6.28f);
        int randomTrailLengthOffset = Main.rand.Next(-1, 2);


        BaseTrailInfo trail1 = new BaseTrailInfo();
        Vector2 spawnPos = Vector2.Zero;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 4f;

            if (timer == 0)
            {
                projectile.alpha = 0;
                spawnPos = projectile.Center;
                projectile.ai[0] = projectile.scale;

                //Sounds really cool use for something else
                //SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_hurt_1") with { Volume = 0.8f, Pitch = .9f, PitchVariance = 0.2f, MaxInstances = 1 };
                //SoundEngine.PlaySound(style3, projectile.Center);

                SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/hero_butterfly_blade") with { Volume = 0.2f, Pitch = .8f, PitchVariance = .3f, };
                SoundEngine.PlaySound(style2, projectile.Center);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/star_impact_01") with { Volume = 0.5f, Pitch = .5f, PitchVariance = .3f, }; 
                SoundEngine.PlaySound(style, projectile.Center);

                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * 1f;
                Dust d = Dust.NewDustPerfect(spawnPos, ModContent.DustType<CirclePulse>(), vel, newColor: Color.DodgerBlue);

                d.scale = 0.025f;
                d.customData = new CirclePulseBehavior(0.05f, true, 12, 0.35f, 0.7f);
            }

            int trailCount = 8 + randomTrailLengthOffset;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer % 2 == 0 && timer > 0 && Main.rand.NextBool())
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.3f, 0.4f));
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
            }

            float timeForPopInAnim = 15f;
            float animProgress = Math.Clamp((float)(timer / timeForPopInAnim), 0f, 1f);
            projectile.scale = projectile.ai[0] + (1f - Easings.easeOutCirc(animProgress)) * 0.5f;

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.4f, 0.08f), 0f, 1f);


            Lighting.AddLight(projectile.Center, Color.DeepSkyBlue.ToVector3() * 0.75f * overallAlpha);

            Color thisBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.65f) * overallAlpha;

            #region trail info
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value; //EvenThinnerGlowLine == Really clean
            trail1.trailPointLimit = 90;
            trail1.trailWidth = (int)(18 * overallAlpha);
            trail1.trailMaxLength = 200 * overallAlpha;
            trail1.timesToDraw = 2;
            trail1.shouldSmooth = false;
            trail1.pinch = true;
            trail1.useEffectMatrix = true;

            trail1.trailColor = thisBlue;

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            #endregion

            // V Super nice
            #region alt 
            /*
            Color thisPink = Color.Lerp(Color.HotPink, Color.DeepPink, 0.08f) * overallAlpha;
            Color thisBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.65f) * overallAlpha;

            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/LavaTrailV1").Value; //EvenThinnerGlowLine == Really clean
            trail1.trailPointLimit = 90;
            trail1.trailWidth = (int)(18 * overallAlpha);
            trail1.trailMaxLength = 200 * overallAlpha;
            trail1.timesToDraw = 2;
            trail1.shouldSmooth = false;
            trail1.pinch = true;
            trail1.useEffectMatrix = true;

            trail1.trailColor = thisBlue;

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            */
            #endregion

            timer++;
            return false;
        }

        float fadeInPower = 0f;
        public float overallAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            trail1.trailTime = (float)Main.timeForVisualEffects * 0.035f;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                //trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);
            //trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);

            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            Texture2D glorb = CommonTextures.feather_circle128PMA.Value;

            //So apparently sky fracture is like the only thing in all of terraria that uses a horizontal spritesheet
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(14, 1, frameX: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            Vector2 glorbScale = new Vector2(1f, 0.4f) * 0.8f;
            Vector2 glorbOffset = new Vector2(0f, -1.5f * projectile.scale).RotatedBy(projectile.velocity.ToRotation());
            Main.EntitySpriteDraw(glorb, drawPos + glorbOffset, null, Color.DeepSkyBlue with { A = 0 } * overallAlpha * 0.25f, projectile.velocity.ToRotation(), glorb.Size() / 2f, glorbScale * projectile.scale * overallAlpha, SpriteEffects.None);
            Main.EntitySpriteDraw(glorb, drawPos + glorbOffset, null, Color.SkyBlue with { A = 0 } * overallAlpha * 0.1f, projectile.velocity.ToRotation(), glorb.Size() / 2f, glorbScale * projectile.scale * overallAlpha * 0.75f, SpriteEffects.None);

            float sinVal = (float)Math.Sin(Main.timeForVisualEffects * 0.12f);

            //Border
            for (int i = 0; i < 4; i++)
            {

                float dist = 4f + (sinVal * 1f);

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);


                float opacity = projectile.Opacity * (0.85f + sinVal * 0.15f);
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle, 
                    Color.DodgerBlue with { A = 0 } * 0.15f * opacity, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallAlpha, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.DeepSkyBlue * overallAlpha * 0.5f, projectile.rotation, TexOrigin, projectile.scale * overallAlpha, SpriteEffects.None);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.SkyBlue with { A = 0 } * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallAlpha, SpriteEffects.None);
            
            
            return true;

        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            //So apparently sky fracture is like the only thing in all of terraria that uses a horizontal spritesheet
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            Color color = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.85f);

            float overallWidth = 1f + ((float)Math.Sin(Main.timeForVisualEffects * 0.11f) * 0.25f);
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition;

                //float trailAlpha = progress * progress * projectile.Opacity;

                //Start End
                Color trailColor = Color.Lerp(color, Color.DodgerBlue, Easings.easeOutSine(1f - progress)) * Easings.easeOutCirc(progress);

                Vector2 trailScaleThick = new Vector2(0.5f, 0.1f + Easings.easeInQuad(progress) * 0.45f);
                trailScaleThick.X *= 1f;
                Vector2 trailScaleThin = new Vector2(trailScaleThick.X, trailScaleThick.Y * 0.55f);

                trailScaleThick.Y *= overallWidth;

                Main.EntitySpriteDraw(flare, trailPos, null, trailColor with { A = 0 } * 0.75f, projectile.velocity.ToRotation(),
                    flare.Size() / 2f, trailScaleThick, 0);

                //Main.EntitySpriteDraw(flare, trailPos, null, Color.White with { A = 0 }, previousRotations[i], 
                //flare.Size() / 2f, trailScaleThin, 0);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_10") with { Volume = 0.75f, Pitch = 1f, PitchVariance = 0.25f };
            SoundEngine.PlaySound(style, projectile.Center);


            for (int i = 0; i < Main.rand.Next(4, 9); i++)
            {
                float velMult = Main.rand.NextFloat(1.5f, 4f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.55f, 1f));

                if (dust.scale > 0.9f)
                    dust.velocity *= 0.5f;

                dust.scale *= 1.3f;

                dust.fadeIn = Main.rand.NextFloat(0.25f, 1f);
                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
            }
       
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepSkyBlue, Scale: 0.12f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 20; i < 2 + Main.rand.Next(0,2); i++)
            {
                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = projectile.Center,
                    MovementVector = Main.rand.NextVector2Circular(2f, 2f) + projectile.velocity * 0.025f
                };

                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.SilverBulletSparkle, particleSettings);
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
