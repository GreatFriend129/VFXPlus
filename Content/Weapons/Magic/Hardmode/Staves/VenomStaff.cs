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
using Terraria.Graphics;
using VFXPlus.Common.Drawing;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.VFXTest;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    public class VenomStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VenomFang) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.VenomStaffToggle;
        }


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                timeOffset = Main.rand.NextFloat(0f, 10f);
                randomLengthOffset = Main.rand.Next(-4, 5);
            }

            int trailCount = 29 + randomLengthOffset; //22 |32
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            //Add a second pos this tick
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center + projectile.velocity + projectile.velocity * 0.33f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            float timeForPopInAnim = 32;
            float animProgress = Math.Clamp((timer + 8) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 3f)) * 1f;
            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);

            int timeForBloomGrowAnim = 28;
            float bloomGrowProgress = Math.Clamp((float)timer / timeForBloomGrowAnim, 0f, 1f);
            bloomVel = Vector2.Lerp(Vector2.Zero, projectile.velocity, Easings.easeOutQuad(bloomGrowProgress));

            if (timer % 3 == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                Color purp = new Color(255, 85, 255); //135

                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -6f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) - projectile.velocity * 0.15f;


                float dustScale = Main.rand.NextFloat(0.55f, 0.75f) * 0.45f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowFlare>(), dustVel, newColor: purp, Scale: dustScale);
                smoke.alpha = 2;
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            projectile.spriteDirection = projectile.velocity.X > 0 ? 1 : -1;

            Lighting.AddLight(projectile.Center, Color.Purple.ToVector3() * 0.95f);

            timer++;

            #region vanillaAI (w/o dust)
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 50;
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            if (projectile.alpha == 0)
            {
                //int num86 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 163, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 1.2f);
                //Main.dust[num86].noGravity = true;
                //Main.dust[num86].velocity *= 0.3f;
                //Main.dust[num86].velocity -= projectile.velocity * 0.4f;
            }
            #endregion
            return false;
        }

        int randomLengthOffset = 0;
        Vector2 bloomVel = Vector2.Zero;

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawVertexTrail(projectile, false);
            });
            DrawVertexTrail(projectile, true);

            //Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            //Vector2 TexOrigin = vanillaTex.Size() / 2f;
            //SpriteEffects se = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //float scale = projectile.scale * overallScale * 0f;
            //float rot = projectile.rotation;


            //Draw FlareLine
            Color purp = new Color(255, 85, 255); //135
            Color purp2 = new Color(210, 40, 210); //135

            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;
            Texture2D lineHalf = Mod.Assets.Request<Texture2D>("Assets/Pixel/Medusa_Gray").Value;

            //Bloom
            Vector2 bloomOrigin = new Vector2(0f, lineHalf.Height / 2f);
            Vector2 bloomScale = new Vector2(0.1f * bloomVel.Length(), 1.5f);

            Main.EntitySpriteDraw(lineHalf, drawPos, null, purp2 with { A = 0 } * 0.5f * overallAlpha,
                projectile.velocity.ToRotation() + MathHelper.Pi, bloomOrigin, bloomScale, SpriteEffects.None);


            Vector2 lineScale = new Vector2(0.75f, 1.16f * overallScale);

            Main.EntitySpriteDraw(line, drawPos + Main.rand.NextVector2Circular(1.4f, 1.4f), null, purp with { A = 0 } * overallAlpha * 0.75f,
                projectile.velocity.ToRotation(), line.Size() / 2f, lineScale * 0.8f, SpriteEffects.None);

            Main.EntitySpriteDraw(line, drawPos + Main.rand.NextVector2Circular(0.7f, 0.7f), null, purp with { A = 0 } * overallAlpha * 1f,
                projectile.velocity.ToRotation(), line.Size() / 2f, lineScale * 0.55f, SpriteEffects.None);

            Main.EntitySpriteDraw(line, drawPos + Main.rand.NextVector2Circular(0.35f, 0.35f), null, Color.White with { A = 0 } * overallAlpha * 1f,
                projectile.velocity.ToRotation(), line.Size() / 2f, lineScale * 0.3f, SpriteEffects.None);

            return false;

        }

        float timeOffset = 0;
        Effect myEffect = null;
        public void DrawVertexTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.09f + timeOffset) * 0.15f;

            Color StripColor(float progress) => Color.White * (progress * progress * progress);
            float StripWidth(float progress) => 25f * Easings.easeOutCubic(progress) * overallScale * sineWidthMult;
            //^ Doing Easings.easeOutQuad(progress) * Easings.easeInQuad(progress) gives a really nice zigzag patter (or do 1f - EaseIn)

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.05f + timeOffset);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f * overallAlpha);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //vertexStrip.DrawTrail();

            Color purp = new Color(200, 20, 200);

            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(purp.ToVector3() * 3f * overallAlpha);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f); //0.6
            myEffect.Parameters["glowIntensity"].SetValue(2f); //2.25
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color purp = new Color(255, 85, 255); //135

            for (int num462 = 0; num462 < 3 + Main.rand.Next(3); num462++) //10
            {
                int num464 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, ModContent.DustType<GlowPixelCross>(), 0f, 0f,
                    newColor: purp, Scale: 0.2f * Main.rand.NextFloat(0.8f, 1f));
                Main.dust[num464].noGravity = true;
                Main.dust[num464].velocity -= projectile.oldVelocity.RotatedByRandom(0.25f) * 0.35f;
                Main.dust[num464].velocity *= 0.5f;
            }

            for (int i2 = 0; i2 < 2 + Main.rand.Next(0, 2); i2++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: purp * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.35f) * projectile.scale);
            }

            int i = 0;
            foreach (Vector2 pos in previousPositions)
            {
                i++;
                if (i % 7 == 0 && i > previousPositions.Count * 0.4f)
                {
                    int a = Dust.NewDust(pos, 0, 0, ModContent.DustType<GlowFlare>(), 0, 0, newColor: purp, Scale: 0.35f);
                    Main.dust[a].customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);
                    Main.dust[a].velocity *= ((i * 0.06f));
                    Main.dust[a].velocity -= projectile.velocity * 0.25f;
                }
            }


            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_40") with { Pitch = -.8f, PitchVariance = .5f, MaxInstances = -1, Volume = 0.1f };
            SoundEngine.PlaySound(style, projectile.Center);

            int soundVariant2 = Main.rand.Next(3);
            SoundStyle tile_hit = new SoundStyle("Terraria/Sounds/Dig_" + soundVariant2) with { Volume = 0.08f, Pitch = 0.7f, PitchVariance = 0.3f, MaxInstances = -1 };
            SoundEngine.PlaySound(tile_hit, projectile.Center);

            return false;
        }


        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
