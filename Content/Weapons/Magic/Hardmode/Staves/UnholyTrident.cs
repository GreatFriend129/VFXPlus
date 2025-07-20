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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class UnholyTrident : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.UnholyTrident) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.UnholyTridentToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(2f, 2f);
                Dust sa = Dust.NewDustPerfect(player.MountedCenter + v + velocity.SafeNormalize(Vector2.UnitX) * 45f, ModContent.DustType<GlowPixelCross>(), 
                    velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(6.28f) * Main.rand.NextFloat(1f, 2f), 0,
                    new Color(42, 2, 82) * 2f, Main.rand.NextFloat(0.35f, 0.5f) * 0.75f);

                sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 1, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.9f, shouldFadeColor: false);

            }

            for (int i = 0; i < 6 + Main.rand.Next(2); i++)
            {
                Vector2 v2 = Main.rand.NextVector2Circular(3f, 3f);

                Dust sa2 = Dust.NewDustPerfect(player.MountedCenter + velocity.SafeNormalize(Vector2.UnitX) * 45f, ModContent.DustType<MuraLineBasic>(),
                    v2, 255, new Color(42, 2, 82) * 2f, Main.rand.NextFloat(0.25f, 0.65f) * 0.65f);
            }

            return true;
        }

    }
    public class UnholyTridentShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.UnholyTridentFriendly) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.UnholyTridentToggle;
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //This projectile oscilates its scale but we don't want that
            if (timer == 0)
                startingScale = projectile.scale;

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);
            Color purple3 = new Color(121, 7, 179);

            #region Trail
            int trailCount = 16; //8
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            int trueTrailWidth = (int)(35f * overallScale * projectile.scale * 2f); //20

            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/EvenThinnerGlowLine").Value;
            trail1.trailPointLimit = 90;
            trail1.trailWidth = trueTrailWidth * 0;
            trail1.trailMaxLength = 130 * projectile.scale; //65
            trail1.timesToDraw = 2;
            trail1.shouldSmooth = false;
            trail1.pinchHead = false;
            trail1.pinchTail = true;
            trail1.useEffectMatrix = true;

            trail1.trailColor = darkPurple;

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            #endregion


            if (timer % 4 == 0 && Main.rand.NextBool() && timer > 8)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.25f);

                p.velocity -= projectile.velocity * 0.4f;
            }

            if (timer % 3 == 0 && timer > 8)
            {
                Vector2 sideOffset = new Vector2(0f, Main.rand.NextFloat(-8f, 8f)).RotatedBy(projectile.velocity.ToRotation());
                Vector2 vel = -projectile.velocity * 0.25f;

                Dust line = Dust.NewDustPerfect(projectile.Center + sideOffset, ModContent.DustType<MuraLineBasic>(), vel, 255, 
                    newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.6f);

            }

            //Looks cool, but makes the weapon a little messy, maybe use this for shadowflame knife and/or bow?
            if (timer % 1 == 0 && false)
            {
                Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelAlts>(), Velocity: initVel, newColor: new Color(42, 2, 82), Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.45f);

                a.velocity *= 0.25f;
                a.velocity += projectile.velocity * 0.15f;


                Vector2 posOffset2 = Main.rand.NextVector2Circular(2f, 2f);
                Vector2 initVel2 = Main.rand.NextVector2Circular(1.25f, 1.25f);

                Dust a2 = Dust.NewDustPerfect(projectile.Center + posOffset2 + (projectile.velocity * 0.5f), ModContent.DustType<GlowPixelAlts>(), Velocity: initVel2, newColor: new Color(42, 2, 82), Scale: Main.rand.NextFloat(0.85f, 1.15f) * 0.45f);

                a2.velocity *= 0.25f;
                a2.velocity += projectile.velocity * 0.15f;
            }


            float fadeInTime = Math.Clamp((timer + 5f) / 15f, 0f, 1f);
            fadeInAlpha = Easings.easeInCirc(fadeInTime);

            float animProgress = Math.Clamp((timer + 5f) / 30f, 0f, 1f);
            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 2f));


            Lighting.AddLight(projectile.Center, purple.ToVector3() * 2f);

            timer++;


            #region vanillaAI
            if (projectile.localAI[1] < 15f)
            {
                projectile.localAI[1] += 1f;
            }
            else
            {
                //int num314 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y + 4f), 8, 8, 27, projectile.oldVelocity.X, projectile.oldVelocity.Y, 100, default(Color), 0.6f);
                //Dust dust54 = Main.dust[num314];
                //Dust dust2 = dust54;
                //dust2.velocity *= -0.25f;

                if (projectile.localAI[0] == 0f)
                {
                    projectile.scale -= 0.02f;
                    projectile.alpha += 30;
                    if (projectile.alpha >= 250)
                    {
                        projectile.alpha = 255;
                        projectile.localAI[0] = 1f;
                    }
                }
                else if (projectile.localAI[0] == 1f)
                {
                    projectile.scale += 0.02f;
                    projectile.alpha -= 30;
                    if (projectile.alpha <= 0)
                    {
                        projectile.alpha = 0;
                        projectile.localAI[0] = 0f;
                    }
                }
            }

            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 0.785f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
            #endregion
            return false;
            return base.PreAI(projectile);
        }

        float startingScale = 0f;
        float overallScale = 0f;

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            trail1.trailTime = (float)Main.timeForVisualEffects * 0.06f;

            //ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            //{
            //    trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
            //});

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(projectile, false);
            });
            DrawAfterImage(projectile, true);


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            drawPos += new Vector2(-15f, 15f).RotatedBy(projectile.rotation);

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //Border
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
                offset += projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)i) * 2f;

                Main.EntitySpriteDraw(vanillaTex, drawPos + offset, sourceRectangle, Color.White with { A = 0 } * fadeInAlpha, projectile.rotation, TexOrigin, startingScale * overallScale, SpriteEffects.None);
            }


            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * fadeInAlpha, projectile.rotation, TexOrigin, startingScale * overallScale, SpriteEffects.None);

            return false;

        }

        public void DrawAfterImage(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);
            Color purple3 = new Color(121, 7, 179);

            float trueScale = startingScale * overallScale;

            //Orb
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Vector2 originPoint = projectile.Center - Main.screenPosition;
            originPoint += new Vector2(-15f, 15f).RotatedBy(projectile.rotation);

            Color[] cols = { Color.Purple * 0.75f, darkPurple * 0.525f, darkPurple * 0.375f };
            float[] scales = { 0.85f, 1.6f, 2.5f };

            float orbRot = projectile.rotation - MathHelper.PiOver4;
            float orbAlpha = 0.25f;
            Vector2 orbScale = new Vector2(0.7f, 0.2f) * 1.5f * trueScale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, originPoint, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            //Main.EntitySpriteDraw(orb, originPoint, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orb.Size() / 2f, orbScale * scales[2] * sineScale2, SpriteEffects.None);


            Texture2D line = CommonTextures.SoulSpike.Value;
            Color between2 = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / (float)previousPositions.Count;

                Vector2 offset1 = Main.rand.NextVector2Circular(3.5f * trueScale, 3.5f * trueScale * progress).RotatedBy(projectile.rotation - MathHelper.PiOver4) * overallScale;


                Vector2 flarePos = previousPositions[i] - Main.screenPosition;

                Color col = Color.Lerp(Color.DeepSkyBlue * 1.5f, between2, Easings.easeOutCubic(progress)) * fadeInAlpha;

                Vector2 lineScale = new Vector2(1f, 1f) * progress * trueScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, darkPurple with { A = 0 } * 2f * progress,
                    previousRotations[i] - MathHelper.PiOver4, line.Size() / 2f, lineScale, SpriteEffects.None);

                Vector2 innerScale = new Vector2(1f, 1f * 0.1f) * progress * trueScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, Color.White with { A = 0 } * 0.6f * progress * fadeInAlpha,
                    previousRotations[i] - MathHelper.PiOver4, line.Size() / 2f, innerScale, SpriteEffects.None);
            }
        }

        private BlendState _multiplyBlendState = null;
        public void DrawShadowTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Main.spriteBatch.End();

            if (_multiplyBlendState == null)
            {
                _ = BlendState.AlphaBlend;
                _ = BlendState.Additive;
                _multiplyBlendState = new BlendState
                {
                    ColorBlendFunction = BlendFunction.ReverseSubtract,
                    ColorDestinationBlend = Blend.One,
                    ColorSourceBlend = Blend.SourceAlpha,
                    AlphaBlendFunction = BlendFunction.ReverseSubtract,
                    AlphaDestinationBlend = Blend.One,
                    AlphaSourceBlend = Blend.SourceAlpha
                };
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, _multiplyBlendState, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


            Color thisPurple = new Color(0, 38, 0) * 4f;
            //After-Image
            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            Color color = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.75f);

            float overallWidth = 1f + ((float)Math.Sin(Main.timeForVisualEffects * 0.06f) * 0.15f);

            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition;

                //float trailAlpha = progress * progress * projectile.Opacity;

                //Start End
                Color trailColor = thisPurple;// Color.Lerp(color, Color.DodgerBlue, Easings.easeOutSine(1f - progress));

                Vector2 trailScaleThick = new Vector2(1f, Easings.easeInQuad(progress) * 0.5f) * fadeInAlpha;
                Vector2 trailScaleThin = new Vector2(trailScaleThick.X, trailScaleThick.Y * 0.55f);

                trailScaleThick.Y *= overallWidth;

                Main.EntitySpriteDraw(flare, trailPos, null, trailColor * progress, previousRotations[i] - MathHelper.PiOver4,
                    flare.Size() / 2f, trailScaleThick, 0);

                //Main.EntitySpriteDraw(flare, trailPos, null, Color.White with { A = 0 }, previousRotations[i], 
                //flare.Size() / 2f, trailScaleThin, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            #region vanillaKill
            SoundEngine.PlaySound(in SoundID.Item10, projectile.position);
            for (int num1000 = 4; num1000 < 31; num1000++)
            {
                float num1001 = projectile.oldVelocity.X * (30f / (float)num1000);
                float num1002 = projectile.oldVelocity.Y * (30f / (float)num1000);
                //int num1003 = Dust.NewDust(new Vector2(projectile.position.X - num1001, projectile.position.Y - num1002), 8, 8, 27, projectile.oldVelocity.X, projectile.oldVelocity.Y, 100, default(Color), 1.4f);
                //Main.dust[num1003].noGravity = true;
                //Dust dust87 = Main.dust[num1003];
                //Dust dust334 = dust87;
                //dust334.velocity *= 0.5f;


                //Vector2 pos = new Vector2(projectile.Center.X - num1001, projectile.Center.Y - num1002);
                //Dust sa = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelCross>(), 
                //    Main.rand.NextVector2Circular(circRange, circRange) + projectile.oldVelocity * 0.35f, 0,
                //    new Color(42, 2, 82) * 2f, Main.rand.NextFloat(0.35f, 0.5f) * 0.8f);
                
                //sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 1, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.9f, shouldFadeColor: false);

                //num1003 = Dust.NewDust(new Vector2(projectile.position.X - num1001, projectile.position.Y - num1002), 8, 8, 27, projectile.oldVelocity.X, projectile.oldVelocity.Y, 100, default(Color), 0.9f);
                //dust87 = Main.dust[num1003];
                //dust334 = dust87;
                //dust334.velocity *= 0.5f;
            }

            for (int i = 3; i < 18; i++)
            {
                float xProg = projectile.oldVelocity.X * (18f / (float)i);
                float yProg = projectile.oldVelocity.Y * (18f / (float)i);

                float circRange = i % 2 == 0 ? 0.75f : 1.75f;

                Vector2 pos = new Vector2(projectile.Center.X - xProg, projectile.Center.Y - yProg);
                Dust sa = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelCross>(),
                    Main.rand.NextVector2Circular(circRange, circRange) + projectile.oldVelocity * 0.4f, 0,
                    new Color(42, 2, 82) * 2f, Main.rand.NextFloat(0.35f, 0.5f) * 0.8f);
                sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 1, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.9f, shouldFadeColor: false);
            }

            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 3 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(2f, 2f);
                Dust sa = Dust.NewDustPerfect(projectile.Center + v, ModContent.DustType<GlowPixelCross>(),
                    Main.rand.NextVector2CircularEdge(1.25f, 1.25f) * Main.rand.NextFloat(1f, 2f), 0,
                    new Color(42, 2, 82) * 2f, Main.rand.NextFloat(0.35f, 0.5f) * 0.8f);

                sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 1, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.9f, shouldFadeColor: false);

            }

            for (int i = 0; i < 6 + Main.rand.Next(2); i++)
            {
                Vector2 v2 = Main.rand.NextVector2Circular(3f, 3f);

                Dust sa2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineBasic>(), v2, 255, new Color(42, 2, 82) * 2f, Main.rand.NextFloat(0.25f, 0.65f) * 0.65f);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
