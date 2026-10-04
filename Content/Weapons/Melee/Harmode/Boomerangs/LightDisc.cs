using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Dusts;


namespace VFXPlus.Content.Weapons.Melee.Hardmode.Boomerangs
{
    
    public class LightDiscProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public List<float> previousRotations = new List<float>();

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LightDisc);
        }

        public List<Vector2> previousPositions = new List<Vector2>();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 12; //18
            previousPositions.Add(projectile.Center + projectile.velocity);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);


            float fadeInTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            //float fadeInAlphaTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            //overallAlpha = Easings.easeInOutHarsh(fadeInTime);

            //visualRotation = projectile.rotation;
            visualRotation += 0.45f * projectile.direction * overallScale;


            timer++;
            return true;
        }

        float visualRotation = 0f;
        float overallAlpha = 1f;
        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
                DrawSpinningUnder(projectile, false); 
            });
            DrawSpinningUnder(projectile, true);


            Texture2D VanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D Glowmask = TextureAssets.LightDisc.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(VanillaTex, drawPos, null, lightColor * overallAlpha, visualRotation, VanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Glowmask, drawPos, null, Color.White * overallAlpha, visualRotation, VanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);

            return false;

        }

        Effect trailEffect = null;
        public void DrawTrail(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/BasicGlowTrail", AssetRequestMode.ImmediateLoad).Value;


            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value; 
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            //Blue DodgerBlue

            Color darker = Color.DodgerBlue;
            Color brighter = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.5f);

            Color StripColor(float progress) => Color.Lerp(darker, brighter, progress) with { A = 200 } * 1f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.18f;

                
                float toReturn = 0f;
                if (progress < 0.75f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.75f, progress, true);
                    toReturn = Easings.easeInSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.75f, 1f, progress, true);
                    toReturn = 1f;// Easings.easeOutSine(1f - LV);
                }

                return 30f * sineWidthMult;// * toReturn * sineWidthMult * overallScale;


                //float toReturn = 0f;
                if (progress < 0.85f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.85f, progress, true);
                    toReturn = Easings.easeInSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.85f, 1f, progress, true);
                    toReturn = Easings.easeOutSine(1f - LV);
                }

                return 40f * toReturn * sineWidthMult * overallScale;
            }

            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.05f);

            trailEffect.Parameters["bodyIntensity"].SetValue(1f);
            trailEffect.Parameters["bodyPower"].SetValue(2f);
            trailEffect.Parameters["posterizationSteps"].SetValue(0f);

            trailEffect.Parameters["whiteGlowSize"].SetValue(0.75f);
            trailEffect.Parameters["whiteGlowPower"].SetValue(4f);


            trailEffect.Parameters["TrailTexture1"].SetValue(trailTexture1);
            trailEffect.Parameters["tex1reps"].SetValue(2f);
            trailEffect.Parameters["tex1Intensity"].SetValue(2f * 0f); 

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(1f * 0f);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public void DrawSpinningUnder(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            //Main.EntitySpriteDraw(Orb, drawPos, null, Color.Blue with { A = 0 } * 0.2f, 0f, Orb.Size() / 2f, 1f * projectile.scale * overallScale, SpriteEffects.None);

            //Ring
            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingB").Value; //A |0.75f time|

            float ringRot = (float)Main.timeForVisualEffects * 0.3f * projectile.direction;

            float ringScale = 0.09f * projectile.scale * overallScale; //125
            float ringAlpha = overallAlpha * 1f;

            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.DeepSkyBlue with { A = 100 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.DodgerBlue with { A = 100 } * ringAlpha, ringRot + MathHelper.Pi, RingTex.Size() / 2f, ringScale * 1.1f, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            previousPositions.Clear();
            previousRotations.Clear();

            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
