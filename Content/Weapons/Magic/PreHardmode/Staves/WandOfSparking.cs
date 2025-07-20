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
using System.Runtime.InteropServices;
using VFXPlus.Common.Drawing;
using static Terraria.ModLoader.ModContent;
using Terraria.ModLoader.Config;
using Terraria.Graphics;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Staves
{
    public class WandOfSparkingShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WandOfSparkingSpark) && GetInstance<VFXPlusToggles>().MagicToggle.WandOfSparkingToggle;
        }


        int timer = 0;
        float fadeInPower = 0f;
        float spinInPower = 0f;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 15;  //14
            previousRotations.Add(projectile.velocity.ToRotation()); //
            previousPositions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -2f;
            Color orangeToUse = Color.Lerp(Color.Orange, Color.OrangeRed, 0.8f);

            if (timer % 1 == 0 && timer > 5 && false)
            {
                Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);


                //Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    //newColor: between with { A = 55 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                //d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 10), 0.98f, 0.01f, 0.35f); //12 28
                //d.rotation = Main.rand.NextFloat(6.28f);
                //d.velocity += projectile.velocity * -0.1f;

                Dust d2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 0.8f) * 0f,
                    newColor: between with { A = 0 }, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.3f);
                d2.customData = new MediumSmokeBehavior(Main.rand.Next(12, 24), 0.95f, 0.01f, 0.15f); //12 28
                d2.rotation = Main.rand.NextFloat(6.28f);
                d2.velocity += projectile.velocity * -0.1f;
            }

            GlowPixelAltBehavior dustBehavior = new GlowPixelAltBehavior();
            dustBehavior.base_fadeOutPower = 0.91f;
            dustBehavior.base_timeToKill = 30;

            //Use GPA dust to create a trail
            //Dust c = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelAlts>(), dustVel.RotatedByRandom(0.25f), 0, orangeToUse, 0.53f * fadeInPower);
            //c.alpha = 2;
            //c.noLight = true;
            //c.customData = dustBehavior;

            //50% chance to make a second one
            if (Main.rand.NextBool())
            {
                //Dust a = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelRise>(), dustVel.RotatedByRandom(0.5f), 0, Color.OrangeRed, 0.33f * fadeInPower);
                //a.alpha = 2;
                //a.noLight = true;
                //a.customData = dustBehavior;
            }

            float timeForSpinIn = Math.Clamp((float)timer / 38f, 0f, 1f); //28
            spinInPower = timeForSpinIn;// Easings.easeOutCirc(timeForSpinIn);


            //Fade in over 15 ticks
            if (timer < 47)
            {
                float timeForFadeIn = Math.Clamp(timer / 15f, 0f, 1f);
                fadeInPower = Easings.easeOutCirc(timeForFadeIn);
            }
            //Fade out over 8 ticks
            else
            {
                float timeForFadeIn = Math.Clamp((timer - 47f) / 8f, 0f, 1f);
                fadeInPower = Easings.easeOutCirc(1f - timeForFadeIn);
            }


            timer++;
            return true;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawVertexTrail(false);
                DrawStar(projectile, false);
            });
            DrawVertexTrail(true);
            DrawStar(projectile, true);

            return false;

        }


        List<float> previousRotations = new List<float>();
        List<Vector2> previousPositions = new List<Vector2>();
        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value; //smoketrailsmudge
            // CommonTextures.Extra_196_Black.Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress) * fadeInPower * 0.65f; // 0.75
            float StripWidth(float progress) => 35f * Easings.easeOutQuad(progress) * fadeInPower; //35


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.05f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //Black UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //vertexStrip.DrawTrail();
            //vertexStrip.DrawTrail();

            //Over layer
            Color col = Color.Lerp(Color.OrangeRed, Color.Orange, 0.1f);

            myEffect.Parameters["ColorOne"].SetValue(col.ToVector3() * 5f);
            myEffect.Parameters["glowThreshold"].SetValue(0.9f);
            myEffect.Parameters["glowIntensity"].SetValue(2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public void DrawStar(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Tex = CommonTextures.CrispStarPMA.Value;
            Texture2D Orb = CommonTextures.PartiGlowPMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);

            float spinInBonusRot = MathHelper.Lerp(8f * projectile.direction, 0f, Easings.easeOutCirc(spinInPower));
            float drawRot = projectile.velocity.ToRotation() - spinInBonusRot;

            Vector2 TexOrigin = Tex.Size() / 2f;

            Vector2 vec2scale = new Vector2(0.45f, 0.45f) * projectile.scale * 1.35f * fadeInPower;


            Color orangeToUse = Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);
            Color orangeToUse2 = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f); //1 0.75

            //Black under layer
            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, Color.Black * 0.35f, drawRot, TexOrigin, vec2scale * 0.6f, SpriteEffects.None, 0f);

            //Orb
            Main.spriteBatch.Draw(Orb, drawPos + posOffset, null, orangeToUse2 with { A = 0 } * 0.2f, drawRot, Orb.Size() / 2f, vec2scale * 1.25f, SpriteEffects.None, 0f);

            //Main layer
            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, orangeToUse2 with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, orangeToUse with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.45f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.LightYellow with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.35f, SpriteEffects.None, 0f);

        }
    }

}
