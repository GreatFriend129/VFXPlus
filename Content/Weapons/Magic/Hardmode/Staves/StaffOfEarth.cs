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
using Terraria.GameContent.Drawing;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    public class StaffOfEarthShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BoulderStaffOfEarth) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.StaffOfEarthToggle;
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.25f, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.45f, newColor: Color.Orange * 0.3f, Scale: 0.01f);
                CirclePulseBehavior b2 = new CirclePulseBehavior(0.65f, true, 2, 0.2f, 0.4f);
                b2.drawLayer = "UnderProjectiles";
                d2.customData = b2;

                Dust d2A = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.55f, newColor: Color.Orange * 0.35f, Scale: 0.01f);
                CirclePulseBehavior b2A = new CirclePulseBehavior(0.5f, true, 1, 0.15f, 0.3f);
                b2A.drawLayer = "UnderProjectiles";
                d2A.customData = b2A;
            }
            
            if (timer % 1 == 0)
            {
                int trailCount = 25; //25
                previousVelRots.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousVelRots.Count > trailCount)
                    previousVelRots.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }


            if (timer % 2 == 0 && Main.rand.NextBool(2) && timer > 8 && projectile.velocity.Length() > 3f)
            {
                Color dustCol = Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: dustCol, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.75f);

                p.customData = DustBehaviorUtil.AssignBehavior_PGOBase(velToBeginShrink: 4f);

                p.velocity += projectile.velocity * 0.25f;
            }

            #region trail
            int trueTrailWidth = (int)(100f * fadeInAlpha * projectile.scale); //20

            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value;
            trail1.trailPointLimit = 90;
            trail1.trailWidth = trueTrailWidth * 0;
            trail1.trailMaxLength = 130 * projectile.scale; //65
            trail1.timesToDraw = 2;
            trail1.shouldSmooth = false;
            trail1.pinchHead = false;
            trail1.pinchTail = false;
            trail1.useEffectMatrix = true;

            trail1.trailColor = Color.OrangeRed;// Color.Lerp(Color.OrangeRed, Color.Orange, 0.8f);

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            #endregion

            float fadeInTime = Math.Clamp((timer + 10f) / 15f, 0f, 1f);
            fadeInAlpha = Easings.easeInCirc(fadeInTime);

            timer++;

            return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;
        public List<float> previousVelRots = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            trail1.trailTime = (float)Main.timeForVisualEffects * 0.03f;

            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: true);
            });

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(projectile, false);
            });
            DrawAfterImage(projectile, true);

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            Color col1 = Color.Lerp(Color.SkyBlue, Color.White, 0.2f);

            float velVal = projectile.velocity.Length() * 0.2f;
            float borderAlpha = Math.Clamp(velVal, 0.5f, 1f);
            //Border
            for (int i = 0; i < 6; i++)
            {
                float scale = 1.05f + (i * 0.1f);
                float opacity = Math.Clamp(0.7f - (0.09f * i), 0.05f, 1f);
                float dist = 3f + (i * 0.4f);

                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(dist, dist), sourceRectangle,
                    col1 with { A = 0 } * opacity * borderAlpha, projectile.rotation, TexOrigin, projectile.scale * scale * fadeInAlpha, SpriteEffects.None);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * fadeInAlpha, SpriteEffects.None);

            //Glowmask
            Texture2D GlowMask = TextureAssets.GlowMask[252].Value;
            Main.EntitySpriteDraw(GlowMask, drawPos, null, Color.White, projectile.rotation, GlowMask.Size() / 2f, projectile.scale * fadeInAlpha, SpriteEffects.None);

            for (int k = 0; k < 1; k++)
            {
                Main.EntitySpriteDraw(GlowMask, drawPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.White with { A = 0 }, projectile.rotation, GlowMask.Size() / 2f, projectile.scale * fadeInAlpha, SpriteEffects.None);
            }
            return false;

        }

        public void DrawAfterImage(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < previousVelRots.Count; i++)
            {
                float progress = (float)i / previousVelRots.Count;

                float size2 = 1f * Easings.easeOutCirc(progress) * projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.SkyBlue with { A = 0 } * 0.5f * Easings.easeInSine(progress) * progress, //0.5f
                        previousVelRots[i], TexOrigin, size2 * fadeInAlpha, SpriteEffects.None);

            }

            Texture2D line = CommonTextures.SoulSpike.Value;
            Color between2 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.85f);
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / (float)previousPositions.Count;

                Vector2 offset1 = Vector2.Zero;

                //offset1 += Main.rand.NextVector2Circular(5f * projectile.scale, 5f * projectile.scale) * fadeInAlpha * projectile.scale;


                Vector2 flarePos = previousPositions[i] - Main.screenPosition;

                Color col = Color.Lerp(Color.OrangeRed, between2, Easings.easeOutSine(progress));

                Vector2 lineScale = new Vector2(0.75f, 1.5f * progress) * fadeInAlpha * projectile.scale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, col with { A = 0 } * 0.35f * Easings.easeInSine(progress),
                    previousVelRots[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Vector2 innerScale = new Vector2(0.75f, 1.5f * 0.25f * progress) * fadeInAlpha * projectile.scale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, Color.LightYellow with { A = 0 } * 0.25f * Easings.easeInSine(progress),
                    previousVelRots[i], line.Size() / 2f, innerScale, SpriteEffects.None);
            }
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

        bool hitOnce = false;
        int timeSinceLastTileCollide = -1;
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            int shockwaveCount = 7;
            if (oldVelocity.Length() > 7)
            {
                Projectile.NewProjectile(null, projectile.Center, Vector2.UnitX, ModContent.ProjectileType<GraydeeTileShockwave>(), 0, 0, Main.player[projectile.owner].whoAmI, 0, shockwaveCount);
                Projectile.NewProjectile(null, projectile.Center, Vector2.UnitX * -1, ModContent.ProjectileType<GraydeeTileShockwave>(), 0, 0, Main.player[projectile.owner].whoAmI, 0, -shockwaveCount);


                for (int i = 0; i < 4 + oldVelocity.Length(); i++) //16
                {
                    float progress = (float)i / 31;
                    Color col = Color.Lerp(Color.Black, Color.Orange, Easings.easeOutQuad(progress));

                    Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 3f) * 2.65f,
                        newColor: col with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 1.5f);
                    d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.3f);
                }
            }

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    class GraydeeTileShockwave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private int TileType => (int)Projectile.ai[0];
        private int ShockwavesLeft => (int)Projectile.ai[1];//Positive and Negitive


        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.extraUpdates = 6; //5
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1060; //

            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 1000)
            {
                if (Projectile.timeLeft < 1002 && Projectile.timeLeft > 80)
                    Projectile.Kill();

                Projectile.velocity.Y = 12f; //4f
            }
            else
            {
                Projectile.velocity.Y = Projectile.timeLeft <= 10 ? 1f : -1f;

                if (Projectile.timeLeft == 19 && Math.Abs(ShockwavesLeft) > 0)
                {
                    Projectile proj = Projectile.NewProjectileDirect(Projectile.InheritSource(Projectile), new Vector2((int)Projectile.Center.X / 16 * 16 + 16 * Math.Sign(ShockwavesLeft)
                    , (int)Projectile.Center.Y / 16 * 16 - 32),
                    Vector2.Zero, Projectile.type, Projectile.damage, 0, Main.myPlayer, TileType, Projectile.ai[1] - Math.Sign(ShockwavesLeft));
                    proj.extraUpdates = 1 + (int)(Math.Abs(ShockwavesLeft) / 3f);
                }

            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (ShockwavesLeft == 0)
                return false;

            int val = ShockwavesLeft > 0 ? 1 : -1;

            float alpha = Math.Clamp(0.25f + (ShockwavesLeft * val) * 0.45f, 0f, 1f);
            
            if (Projectile.timeLeft < 21)
                Main.spriteBatch.Draw(Terraria.GameContent.TextureAssets.Tile[TileType].Value, Projectile.position - Main.screenPosition, new Rectangle(18, 0, 16, 16), lightColor * alpha * 0.5f);

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.timeLeft > 800)
            {
                Point16 point = new Point16((int)((Projectile.Center.X + Projectile.width / 3f * Projectile.spriteDirection) / 16), Math.Min(Main.maxTilesY, (int)(Projectile.Center.Y / 16) + 1));
                Tile tile = Framing.GetTileSafely(point.X, point.Y);

                if (tile != null && WorldGen.InWorld(point.X, point.Y, 1) && tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    Projectile.timeLeft = 20;
                    Projectile.ai[0] = tile.TileType;
                    Projectile.tileCollide = false;
                    Projectile.position.Y += 16;

                    for (float num315 = 2f; num315 < 0.51; num315 += 0.25f)
                    {
                        float angle = MathHelper.ToRadians(-Main.rand.Next(70, 130));
                        Vector2 vecangle = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * num315 * 2f;
                        int dustID = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, (int)(Projectile.height / 2f), ModContent.DustType<GlowPixelAlts>(), 0f, 0f, 50, 
                            Color.Orange * 0.25f, Main.rand.NextFloat(0.45f, 0.7f));
                        Main.dust[dustID].velocity = vecangle;
                    }
                }
            }
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
    }

}
