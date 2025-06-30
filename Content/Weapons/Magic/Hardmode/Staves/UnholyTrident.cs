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
                Vector2 v = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust sa = Dust.NewDustPerfect(player.Center + v + velocity.SafeNormalize(Vector2.UnitX) * 40f, ModContent.DustType<GlowPixelCross>(), 
                    velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.9f) * Main.rand.NextFloat(1f, 4f), 0,
                    new Color(42, 2, 82) * 3f, Main.rand.NextFloat(0.35f, 0.5f) * 0.75f);

                sa.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.9f, shouldFadeColor: false);

                Vector2 v2 = Main.rand.NextVector2Circular(3f, 3f);

                Dust sa2 = Dust.NewDustPerfect(player.Center + v2 + velocity.SafeNormalize(Vector2.UnitX) * 45f, ModContent.DustType<MuraLineBasic>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1.1f) * Main.rand.NextFloat(1.5f, 6f), 255,
                    new Color(42, 2, 82) * 3f, Main.rand.NextFloat(0.25f, 0.65f) * 0.75f);
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

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //TODO: cleanup
            
            int trailCount = 16; //8
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 4 == 0 && Main.rand.NextBool() && timer > 8)
            {
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.25f);

                p.velocity -= projectile.velocity * 0.4f;
            }

            if (timer % 3 == 0 && timer > 8 && true)
            {
                Vector2 sideOffset = new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(projectile.velocity.ToRotation());
                Vector2 vel = -projectile.velocity * 0.25f;

                Dust line = Dust.NewDustPerfect(projectile.Center + sideOffset, ModContent.DustType<MuraLineBasic>(), vel, 255, 
                    newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.6f);

            }

            //Looks cool, but makes the weapon a little messy, maybe use this for shadowflame knife and/or bow?
            if (timer % 1 == 0)
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

            timer++;

            projectile.velocity.Y += 0.5f;

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

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            DrawTrail(projectile, false);

            //Border
            for (int i = 220; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    Color.Purple with { A = 0 } * 0.9f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * fadeInAlpha, SpriteEffects.None);
            }


            //MainTex
            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * fadeInAlpha, SpriteEffects.None);
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

                Vector2 trailScaleThick = new Vector2(0.5f, 0.2f + Easings.easeInSine(progress) * 0.45f) * fadeInAlpha;
                Vector2 trailScaleThin = new Vector2(trailScaleThick.X, trailScaleThick.Y * 0.55f);

                trailScaleThick.Y *= overallWidth;

                Main.EntitySpriteDraw(flare, trailPos, null, trailColor, previousRotations[i] - MathHelper.PiOver4,
                    flare.Size() / 2f, trailScaleThick, 0);

                //Main.EntitySpriteDraw(flare, trailPos, null, Color.White with { A = 0 }, previousRotations[i], 
                //flare.Size() / 2f, trailScaleThin, 0);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
