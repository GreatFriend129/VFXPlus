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
using Terraria.Graphics;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class CrystalSerpent : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.CrystalSerpent);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            

            return true;
        }

    }
    public class CrystalSerpentShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalPulse);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 40; ///12
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center + new Vector2(0f, 0f));

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            bool addInBetween = true;
            if (addInBetween)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPostions.Add(projectile.Center + projectile.velocity * 0.5f + new Vector2(0f, 0f));

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPostions.Count > trailCount)
                    previousPostions.RemoveAt(0);
            }

            //projectile.velocity.Y += 0.62f;

            //New Dust
            //Vector2 dustPos = projectile.Center + new Vector2(0f, 100f) + Main.rand.NextVector2Circular(5f, 5f);
            //Dust wow2 = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), projectile.velocity * 0.5f, newColor: Color.HotPink, Scale: 0.7f);
            //wow2.alpha = 2;

            Color inBetween = Color.Lerp(Color.HotPink, Color.DeepPink, 0.5f);

            Vector2 dustPo2s = projectile.Center + new Vector2(0f, 0f) + Main.rand.NextVector2Circular(3f, 3f);
            Dust star = Dust.NewDustPerfect(dustPo2s, ModContent.DustType<GlowPixelCross>(), projectile.velocity * 0.15f, newColor: inBetween, Scale: 0.3f);
            //star.alpha = 2;

            Vector2 dustPo2s2 = projectile.Center + new Vector2(0f, 0f) + Main.rand.NextVector2Circular(3f, 3f);
            Dust star2 = Dust.NewDustPerfect(dustPo2s2 + projectile.velocity, ModContent.DustType<GlowPixelCross>(), projectile.velocity * 0.15f, newColor: Color.DeepPink, Scale: 0.3f);
            //star2.alpha = 2;

            //Vector2 dustPos2 = projectile.Center + new Vector2(0f, 100f) + Main.rand.NextVector2Circular(15f, 15f);
            //Dust wow = Dust.NewDustPerfect(dustPos2, ModContent.DustType<MuraLineBasic>(), projectile.velocity * -0.5f, newColor: Color.DeepPink, Scale: 0.25f);
            //wow.alpha = 10;

            #region vanillaAI
            int a = false ? 0 : 100;
            for (int num253 = a; num253 < 3; num253++)
            {
                int num254 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 254, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1.2f);
                Main.dust[num254].position = (Main.dust[num254].position + projectile.Center) / 2f;
                Main.dust[num254].noGravity = true;
                Dust dust72 = Main.dust[num254];
                Dust dust3 = dust72;
                dust3.velocity *= 0.5f;
            }
            for (int num255 = a; num255 < 2; num255++)
            {
                int num256 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 255, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 0.4f);
                switch (num255)
                {
                    case 0:
                        Main.dust[num256].position = (Main.dust[num256].position + projectile.Center * 5f) / 6f;
                        break;
                    case 1:
                        Main.dust[num256].position = (Main.dust[num256].position + (projectile.Center + projectile.velocity / 2f) * 5f) / 6f;
                        break;
                }
                Dust dust73 = Main.dust[num256];
                Dust dust3 = dust73;
                dust3.velocity *= 0.1f;
                Main.dust[num256].noGravity = true;
                Main.dust[num256].fadeIn = 1f;
            }
            #endregion
            timer++;
            return false;
            //return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Draw(projectile);
            });

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);


            float drawRot = projectile.velocity.ToRotation();
            Vector2 scale = new Vector2(1.15f, 0.5f) * projectile.scale * 1.5f;


            //Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, Color.Black * 0.15f, drawRot, TexOrigin, scale * 0.7f, SpriteEffects.None, 0f);

            Color col1 = Color.DeepPink;
            Color col2 = Color.HotPink;

            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, col2 with { A = 0 }, drawRot, TexOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, col1 with { A = 0 }, drawRot, TexOrigin, scale * 0.55f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 }, drawRot, TexOrigin, scale * 0.3f, SpriteEffects.None, 0f);

            return false;
        }

        public void Draw(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f); //3f

                    float startScale = projectile.scale + sineScale;

                    Color col = Color.Lerp(Color.HotPink * 1.25f, Color.DeepPink, 0.5f);

                    float easedFadeValue = progress * progress;


                    Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress); //
                    Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress); //0.1f 0.2f

                    //Black
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.4f * easedFadeValue,
                        projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * projectile.scale, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);


                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

                }

            }

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

    public class CrystalSerpentShardOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalPulse2);
        }

        float drawAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14; //20
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            
            Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -2f;

            Color purp = Color.HotPink;

            GlowPixelAltBehavior dustBehavior = new GlowPixelAltBehavior();
            dustBehavior.base_fadeOutPower = 0.91f;
            dustBehavior.base_timeToKill = 30; //30

            //Dust c = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelAlts>(), dustVel.RotatedByRandom(0.25f), 0, Color.HotPink, 0.53f);
            //c.alpha = 2;
            //c.noLight = true;
            //c.customData = dustBehavior;

            if (Main.rand.NextBool())
            {
                Dust a = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelRise>(), dustVel.RotatedByRandom(0.5f), 0, Color.HotPink, 0.33f);
                a.alpha = 2;
                a.noLight = true;
                a.customData = dustBehavior;

            }
            
            timer++;

            #region vanillaCode
            projectile.ai[1] += 1f;
            float num257 = (60f - projectile.ai[1]) / 60f;
            if (projectile.ai[1] > 40f)
            {
                projectile.Kill();
            }
            projectile.velocity.Y += 0.2f;
            if (projectile.velocity.Y > 18f)
            {
                projectile.velocity.Y = 18f;
            }
            projectile.velocity.X *= 0.98f;
            for (int num258 = 220; num258 < 2; num258++)
            {
                int num259 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 254, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 1.1f);
                Main.dust[num259].position = (Main.dust[num259].position + projectile.Center) / 2f;
                Main.dust[num259].noGravity = true;
                Dust dust74 = Main.dust[num259];
                Dust dust3 = dust74;
                dust3.velocity *= 0.3f;
                dust74 = Main.dust[num259];
                dust3 = dust74;
                dust3.scale *= num257;
            }
            for (int num260 = 220; num260 < 1; num260++)
            {
                int num261 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 255, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 0.6f);
                Main.dust[num261].position = (Main.dust[num261].position + projectile.Center * 5f) / 6f;
                Dust dust75 = Main.dust[num261];
                Dust dust3 = dust75;
                dust3.velocity *= 0.1f;
                Main.dust[num261].noGravity = true;
                Main.dust[num261].fadeIn = 0.9f * num257;
                dust75 = Main.dust[num261];
                dust3 = dust75;
                dust3.scale *= num257;
            }


            #endregion

            return false;
            return base.PreAI(projectile);
        }



        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVertexTrail(false);
            });

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);


            float drawRot = projectile.velocity.ToRotation();
            Vector2 scale = new Vector2(0.75f, 1.45f) * projectile.scale * 0.85f;


            //Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, Color.Black * 0.15f, drawRot, TexOrigin, scale * 0.7f, SpriteEffects.None, 0f);

            Color col1 = Color.HotPink;
            Color col2 = Color.HotPink;

            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, col2 with { A = 0 }, drawRot, TexOrigin, scale * 0.8f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, col1 with { A = 0 }, drawRot, TexOrigin, scale * 0.55f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 }, drawRot, TexOrigin, scale * 0.3f, SpriteEffects.None, 0f);


            return false;

        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress);
            float StripWidth(float progress) => 30f * Easings.easeOutQuad(progress);// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.08f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            //vertexStrip.DrawTrail();

            //Over layer
            Color inbetween = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);
            myEffect.Parameters["ColorOne"].SetValue(inbetween.ToVector3() * 5f);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f);
            myEffect.Parameters["glowIntensity"].SetValue(2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color purp = Color.HotPink; //135

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
            foreach (Vector2 pos in previousPostions)
            {
                i++;
                if (i % 7 == 0 && i > previousPostions.Count * 0.4f)
                {
                    int a = Dust.NewDust(pos, 0, 0, ModContent.DustType<GlowFlare>(), 0, 0, newColor: purp, Scale: 0.35f);
                    Main.dust[a].customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);
                    Main.dust[a].velocity *= ((i * 0.06f));
                    Main.dust[a].velocity -= projectile.velocity * 0.25f;
                }
            }
            return base.PreKill(projectile, timeLeft);
        }
    }

}
