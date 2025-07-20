using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using System.Linq;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using static Terraria.ModLoader.ModContent;
using VFXPlus.Common.Drawing;
using System.Collections.Generic;
using Terraria.Graphics;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Staves
{

    public class WandOfFrostingShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WandOfFrostingFrost) && GetInstance<VFXPlusToggles>().MagicToggle.WandOfFrostingToggle;
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
            Color blueToUse = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.35f);

            GlowPixelAltBehavior dustBehavior = new GlowPixelAltBehavior();
            dustBehavior.base_fadeOutPower = 0.91f;
            dustBehavior.base_timeToKill = 30; //30

            //Dust c = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelAlts>(), dustVel.RotatedByRandom(0.25f), 0, blueToUse, 0.53f * fadeInPower);
            //c.alpha = 2;
            //c.noLight = true;
            //c.customData = dustBehavior;

            if (Main.rand.NextBool())
            {
                //Dust a = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelRise>(), dustVel.RotatedByRandom(0.5f), 0, Color.DeepSkyBlue, 0.33f * fadeInPower);
                //a.alpha = 2;
                //a.noLight = true;
                //a.customData = dustBehavior;

            }

            //Smoke dust
            if (timer % 4 == 0)
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<HighResSmoke>(), newColor: Color.LightSkyBlue * fadeInPower, Scale: Main.rand.NextFloat(0.25f, 0.5f));
                Main.dust[d].velocity *= 0.8f;
                Main.dust[d].velocity += projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;

                HighResSmokeBehavior hrsb = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.45f);
                hrsb.isPixelated = true;
                Main.dust[d].customData = hrsb;

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

            Texture2D Tex = CommonTextures.CrispStarPMA.Value;
            Texture2D Orb = CommonTextures.PartiGlowPMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);

            float spinInBonusRot = MathHelper.Lerp(8f * projectile.direction, 0f, Easings.easeOutCirc(spinInPower));
            float drawRot = projectile.velocity.ToRotation() - spinInBonusRot;

            Vector2 TexOrigin = Tex.Size() / 2f;

            Vector2 vec2scale = new Vector2(0.45f, 0.45f) * projectile.scale * 1.35f * fadeInPower;


            Color blueToUse = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.35f);
            Color blueToUse2 = Color.DeepSkyBlue;//Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f); //1 0.75

            //Black under layer
            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, Color.Black * 0.35f, drawRot, TexOrigin, vec2scale * 0.6f, SpriteEffects.None, 0f);

            //Orb
            //Main.spriteBatch.Draw(Orb, drawPos + posOffset, null, blueToUse2 with { A = 0 } * 0.2f, drawRot, Orb.Size() / 2f, vec2scale * 1.25f, SpriteEffects.None, 0f);

            //Main layer
            //Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, blueToUse2 with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, blueToUse with { A = 0 } * fadeInPower * 0.5f, drawRot, TexOrigin, vec2scale * 0.45f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.35f, SpriteEffects.None, 0f);

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
            Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

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


            Color blueToUse = Color.SkyBlue;// Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);
            Color blueToUse2 = Color.DeepSkyBlue;// Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f); //1 0.75

            //Black under layer
            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, Color.Black * 0.35f, drawRot, TexOrigin, vec2scale * 0.6f, SpriteEffects.None, 0f);

            //Orb
            Main.spriteBatch.Draw(Orb, drawPos + posOffset, null, blueToUse2 with { A = 0 } * 0.2f, drawRot, Orb.Size() / 2f, vec2scale * 1.25f, SpriteEffects.None, 0f);

            //Main layer
            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, blueToUse2 with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, blueToUse with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.45f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.35f, SpriteEffects.None, 0f);

        }
    }

}
