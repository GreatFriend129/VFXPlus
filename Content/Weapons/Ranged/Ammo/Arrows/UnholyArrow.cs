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
using System.IO;
using static tModPorter.ProgressUpdate;
using Color = Microsoft.Xna.Framework.Color;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class UnholyArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.UnholyArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = Main.rand.Next(0, 3);

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 14 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);
            

            if ((timer + dustRandomOffsetTime) % 3 == 0 && Main.rand.NextBool() && timer > 5 && false)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.NextFloat(2f, 4f),
                    newColor: new Color(80, 0, 80), Scale: Main.rand.NextFloat(0.4f, 0.5f));

                p.velocity += projectile.velocity * -0.2f;

                p.customData = new GlowFlareBehavior(1f, 1f, 2);
            }

            if ((timer + dustRandomOffsetTime) % 4 == 0 && Main.rand.NextBool() && timer > 5)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, DustID.Shadowflame,
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * -Main.rand.NextFloat(2f, 4f),
                    newColor: Color.White, Scale: Main.rand.NextFloat(0.2f, 0.25f) * 3f);

                p.velocity += projectile.velocity * -0.15f;
            }

            float EU = 1f + projectile.extraUpdates;

            float fadeInTime = Math.Clamp((timer + 7f * EU) / 20f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);

            DrawTrail(projectile, false);


            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SpriteEffects.None);

            return false;
        }

        private BlendState _multiplyBlendState = null;
        public void DrawTrail(Projectile projectile, bool giveUp)
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

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);

            float sineScale = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.34f) * 0.15f;
            float sineScale2 = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.21f) * 0.06f;

            //Green = 0|128|0
            Color thisPurple = new Color(0, 38, 0) * 4f;

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;
                
                //Start End
                Color col = Color.Lerp(thisPurple, new Color(0, 60, 0), 1f - Easings.easeInCubic(progress)) * progress;

                float size = (0.6f + (0.4f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Vector2 arrowScale = new Vector2(size * sineScale2, size) * overallScale;
                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col,// * progress * 1f,
                    previousRotations[i], TexOrigin, arrowScale, SpriteEffects.None);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;
                    float size2 = (0.6f + (0.4f * progress));
                    Vector2 vec2Scale = new Vector2(1.75f, 1f * size2 * sineScale) * overallScale * projectile.scale * 0.5f;
                    
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, col,// * 1f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            }

            for (int num163 = 0; num163 < 4; num163++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(3f, 3f);
                Main.EntitySpriteDraw(vanillaTex, offset + drawPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)num163) * 2f, null, thisPurple * 1f, projectile.rotation, TexOrigin,
                    projectile.scale * overallScale, 0);

            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int num351 = 0; num351 < 8; num351++)
            {
                int num353 = Dust.NewDust(projectile.Center, 10, 10, 27);
                Main.dust[num353].noGravity = true;
                Dust dust267 = Main.dust[num353];
                Dust dust334 = dust267;
                dust334.velocity *= 2f;
                dust267 = Main.dust[num353];
                dust334 = dust267;
                dust334.velocity -= projectile.oldVelocity * 0.3f;
                dust267 = Main.dust[num353];
                dust334 = dust267;
                dust334.scale += (float)Main.rand.Next(150) * 0.001f;
            }

            #region vanillaKill
            SoundEngine.PlaySound(SoundID.Dig, projectile.position);
            for (int num624 = 0; num624 < 2; num624++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Shadowflame, 0f, 0f, 0, default(Color), 0.75f);
            }
            #endregion

            return false;// base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.penetrate != 1)
            {
                for (int i = 0; i < 8; i++)
                {
                    float arrowVel = 7f;
                    Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                    Dust dust = Dust.NewDustPerfect(projectile.Center, DustID.Shadowflame, randomStart, newColor: Color.White, Scale: Main.rand.NextFloat(0.85f, 1.15f));
                    dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.5f;

                    dust.noGravity = true;
                }
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.Center, oldVelocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
