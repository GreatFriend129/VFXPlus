using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class FrostburnArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.FrostburnArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = 0;

        float randomSineOffset = Main.rand.NextFloat();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 11 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);

            int EU = 1 + projectile.extraUpdates;

            //Want less dust when the arrow has extra updates (magic quiver)
            int mod = Math.Clamp(1 * EU, 1, 100);

            //Fire Particles
            if (timer % 1 == 0 && Main.rand.NextBool(1))
            {

                Color frostBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);//0.15
                Color frostBlue2 = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);//0.15

                for (int i = 0; i < 1; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) + projectile.velocity * 0.2f; //0.5

                    float posOffset = -4f + (10f * i);

                    Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * posOffset;

                    FireParticleAlpha fire = new FireParticleAlpha(dustPos + new Vector2(0f, 0f), dustVel, 0.5f, frostBlue * 0.5f, colorMult: 1f, bloomAlpha: 1.75f, AlphaFade: 0.97f, //col * 0.5f
                        EndAlpha: 0.75f, BlackRemoveThreshold: 0.75f); //75
                    fire.bloomColor = frostBlue with { A = 200 };
                    fire.scaleFadePower = 1.01f; //1.05
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }

            if (timer % mod == 0 && Main.rand.NextBool(3))
            {
                int num4 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.IceTorch, projectile.velocity.X * -0.55f, projectile.velocity.Y * -0.55f, 150,
                    default(Color), 1.3f);
                Main.dust[num4].noGravity = true;
                Main.dust[num4].velocity.X *= 3f;
                Main.dust[num4].velocity.Y *= 3f;
                Main.dust[num4].velocity = (Main.dust[num4].velocity + projectile.velocity) / 2f;
            }


            float fadeInTime = Math.Clamp((timer + 3f * EU) / 15f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;

            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;// Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);


            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.White with { A = 0 } * 0.15f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            Main.EntitySpriteDraw(orb, drawPos, null, Color.SkyBlue with { A = 150 } * 0.1f * overallAlpha, projectile.rotation, orb.Size() / 2, new Vector2(0.35f, 0.65f) * overallScale, SE);

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.SkyBlue with { A = 0 } * 0.25f * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D Flare = CommonTextures.Flare.Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color betweenBlue = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.75f); //0.65
            Color betweenBlue2 = Color.Lerp(Color.SkyBlue, Color.LightSkyBlue, 0f); //0.65

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(betweenBlue2, betweenBlue, 1f - progress) * progress * overallAlpha;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;
                float size2 = (0.5f + (0.5f * progress)) * projectile.scale;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 50 } * progress * 0.5f,
                    previousRotations[i], TexOrigin, size2 * overallScale, SpriteEffects.None);

                if (i < previousPositions.Count - 1)
                {
                    float yScaleMult = 1f + (float)Math.Sin((Main.timeForVisualEffects * 0.11f) + randomSineOffset) * 0.25f;

                    float middleProg = (float)(i - 1) / previousPositions.Count;

                    float size3 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(3f, 1f * size3 * yScaleMult) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(Flare, AfterImagePos, null, col with { A = 80 } * 0.35f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, Flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            Color dustCol = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f);
            for (int i = 0; i < 4; i++)
            {
                float arrowVel = Math.Clamp(projectile.oldVelocity.Length(), 1f, 7f);
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: dustCol, Scale: Main.rand.NextFloat(0.6f, 0.7f) * 0.8f);
                dust.velocity += projectile.oldVelocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.15f;

                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 2f, fadePower: 0.85f, shouldFadeColor: false);
            }

            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num418 = 0; num418 < 4; num418++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.IceTorch);
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }
}
