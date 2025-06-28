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
    public class MeteorStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Meteor1) || (entity.type == ProjectileID.Meteor2) || (entity.type == ProjectileID.Meteor3) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.MeteorStaffToggle;
        }

        float drawRotation = 0f;

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 40; //45 -- > 35
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            int trailCount2 = 25; //30 -> 22
            previousRotations2.Add(projectile.velocity.ToRotation());
            previousPositions2.Add(projectile.Center);

            if (previousRotations2.Count > trailCount2)
                previousRotations2.RemoveAt(0);

            if (previousPositions2.Count > trailCount2)
                previousPositions2.RemoveAt(0);

            if (timer % 3 == 0 && Main.rand.NextBool())
            {

                Dust p = Dust.NewDustPerfect(projectile.Center, DustID.Torch,
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f) * 3f,
                    newColor: Color.Orange, Scale: Main.rand.NextFloat(0.9f, 1.1f) * 1.5f);

                p.noGravity = true;
                p.velocity -= projectile.velocity * 0.2f;
            }

            drawRotation += projectile.velocity.X * 0.03f; //

            float timeForPopInAnim = 90;
            float animProgress = Math.Clamp((timer + 25) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1f)) * 0.95f;

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.04f), 0f, 1f);
            //0f + MathHelper.Lerp(0f, 1f, Easings.easeOutQuart(animProgress));


            if (timer < 18) //21
            {
                Vector2 vel2 = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col2 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.9f); //75

                Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SmallSmoke>(), vel2, newColor: col2 with { A = 0 }, Scale: 1.85f);
                d2.velocity += projectile.velocity.RotatedByRandom(0.75f) * 0.15f;

                SmallSmokeBehavior ssb = new SmallSmokeBehavior(7f, 0.89f);
                ssb.timeBetweenFrames = 3;
                d2.customData = ssb;
            }

            Lighting.AddLight(projectile.Center, Color.OrangeRed.ToVector3() * projectile.scale * 1.65f); //0.030

            timer++;

            #region vanillaAI (minus dust)
            if (projectile.position.Y > Main.player[projectile.owner].position.Y - 300f)
            {
                projectile.tileCollide = true;
            }
            if ((double)projectile.position.Y < Main.worldSurface * 16.0)
            {
                projectile.tileCollide = true;
            }
            projectile.scale = projectile.ai[1];
            projectile.rotation += projectile.velocity.X * 2f;
            Vector2 vector20 = projectile.Center + Vector2.Normalize(projectile.velocity) * 10f;
            
            #endregion
            return false;
        }

        public List<float> previousRotations2 = new List<float>();
        public List<Vector2> previousPositions2 = new List<Vector2>();

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawVertexTrail(false);
                DrawPixelTrail(projectile, false);
            });
            float rot = projectile.velocity.ToRotation();

            //Orb
            //Texture2D orb = Mod.Assets.Request<Texture2D>("Content/VFXTest/GoozmaGlowSoft").Value;
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 originPoint = projectile.Center - Main.screenPosition;

            Color col1 = Color.LightGoldenrodYellow * 0.75f; //Gold
            Color col2 = Color.Orange * 0.525f;
            Color col3 = Color.OrangeRed * 0.375f;

            float scale1 = 0.85f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;
            Vector2 scale = new Vector2(1f * overallScale, overallScale * 0.7f) * projectile.scale * 0.75f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, originPoint, null, col1 with { A = 0 } * overallAlpha, rot, orb.Size() / 2f, scale1 * scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col2 with { A = 0 } * overallAlpha, rot, orb.Size() / 2f, scale2 * scale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, col3 with { A = 0 } * overallAlpha, rot, orb.Size() / 2f, scale3 * scale * sineScale2, SpriteEffects.None);


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Vector2 vec2Scale = new Vector2(overallScale, overallScale) * 1.25f * projectile.scale;

            
            //Border
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null, 
                    Color.Orange with { A = 0 } * 1.5f * overallAlpha, drawRotation, TexOrigin, vec2Scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, drawRotation, TexOrigin, vec2Scale, SpriteEffects.None);

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.Orange with { A = 0 } * 0.15f * overallAlpha, drawRotation, TexOrigin, vec2Scale, SpriteEffects.None);


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
            Vector2[] pos_arr = previousPositions2.ToArray();
            float[] rot_arr = previousRotations2.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress * progress);
            float StripWidth(float progress) => 45f * Easings.easeOutQuad(progress) * overallScale;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.02f * 1f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.25f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            //vertexStrip.DrawTrail();

            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(Color.OrangeRed.ToVector3() * 10f);
            myEffect.Parameters["glowThreshold"].SetValue(0.6f);
            myEffect.Parameters["glowIntensity"].SetValue(2.25f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public void DrawPixelTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            if (previousRotations != null && previousPositions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.Lerp(Color.Gold, Color.OrangeRed * 1f, progress) * 1f * overallAlpha;// * Easings.easeInQuad(progress);


                    Vector2 lineScale = new Vector2(1f * overallScale, 0.3f * progress) * progress * overallScale;

                    Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                    //2 > 1
                    Vector2 offset1 = new Vector2(0f, -22f * progress * projectile.scale).RotatedBy(projectile.velocity.ToRotation());
                    Vector2 offset2 = new Vector2(0f, 22f * progress * projectile.scale).RotatedBy(projectile.velocity.ToRotation());

                    
                    offset1 += Main.rand.NextVector2Circular(1f + (10f * (1f - progress)), 15f).RotatedBy(projectile.velocity.ToRotation());
                    offset2 += Main.rand.NextVector2Circular(1f + (10f * (1f - progress)), 15f).RotatedBy(projectile.velocity.ToRotation()); //1f 1.5f

                    Vector2 innerScale = new Vector2(1f * overallScale, 0.15f * progress) * progress * overallScale;

                    Main.EntitySpriteDraw(line, AfterImagePos + offset1, null, col with { A = 0 } * 0.85f * progress * overallAlpha * 0.5f,
                        previousRotations[i], line.Size() / 2f, lineScale * 1.25f, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos + offset2, null, col with { A = 0 } * 0.85f * progress * overallAlpha * 0.5f,
                        previousRotations[i], line.Size() / 2f, lineScale * 1.25f, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos + offset1, null, Color.White with { A = 0 } * 0.26f * progress * overallAlpha * 0.5f,
                        previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
                    Main.EntitySpriteDraw(line, AfterImagePos + offset2, null, Color.White with { A = 0 } * 0.26f * progress * overallAlpha * 0.5f,
                        previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);

                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            #region vanillaDust
            SoundEngine.PlaySound(in SoundID.Item89, projectile.position);
            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = (int)(128f * projectile.scale);
            projectile.height = (int)(128f * projectile.scale);
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            for (int num392 = 220; num392 < 8; num392++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
            }
            for (int num393 = 100; num393 < 32; num393++)
            {
                int num394 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[num394].noGravity = true;
                Dust dust253 = Main.dust[num394];
                Dust dust334 = dust253;
                dust334.velocity *= 3f;
                num394 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
                dust253 = Main.dust[num394];
                dust334 = dust253;
                dust334.velocity *= 2f;
                Main.dust[num394].noGravity = true;
            }
            for (int num395 = 0; num395 < 2; num395++)
            {
                //int num396 = Gore.NewGore(null, projectile.position + new Vector2((float)(projectile.width * Main.rand.Next(100)) / 100f, (float)(projectile.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64));
                //Gore gore53 = Main.gore[num396];
                //Gore gore64 = gore53;
                //gore64.velocity *= 0.3f;
                //Main.gore[num396].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                //Main.gore[num396].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
            }
            if (projectile.owner == Main.myPlayer)
            {
                projectile.localAI[1] = -1f;
                projectile.maxPenetrate = 0;
                projectile.Damage();
            }
            for (int num398 = 0; num398 < 5; num398++)
            {
                int num399 = Utils.SelectRandom<int>(Main.rand, 6, 259, 158);
                int num400 = Dust.NewDust(projectile.position, projectile.width, projectile.height, num399, 2.5f * (float)projectile.direction, -2.5f);
                Main.dust[num400].alpha = 200;
                Dust dust255 = Main.dust[num400];
                Dust dust334 = dust255;
                dust334.velocity *= 2.4f;
                dust255 = Main.dust[num400];
                dust334 = dust255;
                dust334.scale += Main.rand.NextFloat();
            }
            #endregion

            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 6f), 0,
                    Color.Orange, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 22; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);

                float progress = (float)i / 21;
                Color col = Color.Lerp(Color.Black, col1, progress);

                //Color.Lerp(Color.Black, Color.Orange, Main.rand.NextFloat())
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.5f, 3.4f) * 1.65f,
                    newColor: col with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.75f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 1f); //12 28
            }

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.4f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Orange * 0.25f);
            d1.scale = 0.04f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.OrangeRed * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);

            return false;
            return base.PreKill(projectile, timeLeft);
        }

    }

}
