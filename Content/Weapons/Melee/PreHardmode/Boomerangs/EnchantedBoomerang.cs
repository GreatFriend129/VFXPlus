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


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Boomerangs
{
    public class EnchantedBoomerangProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.EnchantedBoomerang);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14; //18
            previousPositions.Add(projectile.Center + projectile.velocity);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);


            float fadeInTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            overallScale = Easings.easeInOutHarsh(fadeInTime);


            //visualRotation = projectile.rotation;
            visualRotation += 0.45f * projectile.direction * overallScale;

            if (timer % 6 == 0)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f);

                Color dustCol = Main.rand.NextBool(40) ? Color.Red : Color.Lerp(Color.DodgerBlue, Color.Blue, 0.5f);

                Dust d = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PulseInOutDust>(), dustVel, 0, dustCol with { A = 100 }, Main.rand.NextFloat(0.7f, 0.9f));
                d.velocity += projectile.velocity * 0.2f;
                d.scale *= 0.7f;

                int pulseTime = Main.rand.Next(18, 22);
                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.VanillaStar, pulseTime, 0.5f, 0.5f, Pixelize: false);
            }

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
                DrawTrail(false);
                DrawSpinningUnder(projectile, false); 
            });
            DrawSpinningUnder(projectile, true);


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            float drawScale = projectile.scale * overallScale;

            for (int i = 1; i < 6; i++)
            {
                float progress = (float)i / 6;
                float inverseProg = 1f - progress;

                float rot = visualRotation - (MathHelper.Pi * 0.75f * progress * projectile.direction);
                Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.White * inverseProg * overallAlpha * 0.25f,
                    rot, vanillaTex.Size() / 2f, drawScale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);

            return false;

        }

        Effect trailEffect = null;
        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp)
                return;

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/BasicGlowTrail", AssetRequestMode.ImmediateLoad).Value;


            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value; //OuterLavaTrail with 10f mult |Extra_196_Black

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color between = Color.Lerp(Color.Aqua, Color.Blue, 0.15f);

            //new Color(255, 45, 0), new Color(255, 100, 15) | Color.Blue, Color.DodgerBlue

            //
            //Blue DodgerBlue
            Color StripColor(float progress) => Color.Lerp(new Color(66, 66, 255), new Color(24, 123, 236), progress) with { A = 175 } * 1f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.18f;

                float toReturn = 0f;
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

                return 20f * toReturn * sineWidthMult * overallScale;
            }

            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.05f);

            trailEffect.Parameters["bodyIntensity"].SetValue(1.25f);
            trailEffect.Parameters["bodyPower"].SetValue(1f);
            trailEffect.Parameters["posterizationSteps"].SetValue(0f);

            trailEffect.Parameters["whiteGlowSize"].SetValue(0.5f);
            trailEffect.Parameters["whiteGlowPower"].SetValue(2f);


            trailEffect.Parameters["TrailTexture1"].SetValue(trailTexture1);
            trailEffect.Parameters["tex1reps"].SetValue(2f);
            trailEffect.Parameters["tex1Intensity"].SetValue(2f); //2f

            trailEffect.Parameters["TrailTexture2"].SetValue(trailTexture2);
            trailEffect.Parameters["tex2reps"].SetValue(2f);
            trailEffect.Parameters["tex2Intensity"].SetValue(1f);

            trailEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


        public void DrawSpinningUnder(Projectile projectile, bool giveUp)
        {
            if (giveUp || true)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Blue with { A = 0 } * 0.2f, 0f, Orb.Size() / 2f, 1f * projectile.scale * overallScale, SpriteEffects.None);

            //Trail
            Texture2D trailTex = CommonTextures.Flare.Value;
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 spikeScale = new Vector2(1f, 0.75f * progress) * progress;
                Color spikeCol = Color.Red * 0.15f * overallAlpha * Easings.easeInQuad(progress) * 0f;

                Main.EntitySpriteDraw(trailTex, previousPositions[i] - Main.screenPosition, null, spikeCol with { A = 0 } * overallAlpha * progress,
                        previousRotations[i], trailTex.Size() / 2f, projectile.scale * overallScale * spikeScale, SpriteEffects.None);
            }


            //
            //Texture2D TrailTex = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpikeHalf").Value;

            //Vector2 trailScale1 = new Vector2(0.25f * projectile.velocity.Length(), 0.5f);
            //Vector2 trailScale2 = new Vector2(0.3f * projectile.velocity.Length(), 0.25f);
            //float trailRot = projectile.velocity.ToRotation();
            //Vector2 origin = new Vector2(TrailTex.Width, TrailTex.Height / 2f);

            //Main.EntitySpriteDraw(TrailTex, drawPos, null, Color.Red with { A = 0 } * overallAlpha * 0.5f, trailRot, origin, trailScale1, SpriteEffects.None);
            //Main.EntitySpriteDraw(TrailTex, drawPos, null, Color.White with { A = 0 } * overallAlpha, trailRot, origin, trailScale2, SpriteEffects.None);


            //Ring
            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingB").Value; //A |0.75f time|

            float ringRot = (float)Main.timeForVisualEffects * 0.6f * projectile.direction;

            float ringScale = 0.125f * projectile.scale * overallScale; //125
            float ringAlpha = overallAlpha * 1f;

            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.DodgerBlue with { A = 50 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Blue with { A = 50 } * ringAlpha, ringRot + MathHelper.PiOver4, RingTex.Size() / 2f, ringScale * 1.1f, SpriteEffects.None);
            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Red with { A = 50 } * ringAlpha, ringRot + MathHelper.PiOver2, RingTex.Size() / 2f, ringScale * 0.5f, SpriteEffects.None);

            //ringScale = 0.135f;
            //drawPos += new Vector2(0f, -100f);
            //Main.EntitySpriteDraw(RingTex, drawPos, null, Color.DodgerBlue with { A = 0 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Blue with { A = 0 } * ringAlpha, ringRot + MathHelper.PiOver4, RingTex.Size() / 2f, ringScale * 1.1f, SpriteEffects.None);
            //Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Red with { A = 0 } * ringAlpha, ringRot + MathHelper.PiOver2, RingTex.Size() / 2f, ringScale * 0.5f, SpriteEffects.None);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
