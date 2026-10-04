using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class ShadowflameBow : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ShadowFlameBow);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.GetModPlayer<HeldBowPlayer>().arrowType = ProjectileID.ShadowFlameArrow;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.ShadowFlameBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;


            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);

    }
    public class ShadowflameBowShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ShadowFlameArrow);
        }
        public override bool InstancePerEntity => true;



        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            previousPositions.Add(projectile.Center + projectile.velocity);
            previousRotations.Add(projectile.velocity.ToRotation());

            int trailCount = 12; //14
            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            if (projectile.ai[2] % 3 == 0 && Main.rand.NextBool(2) && projectile.ai[2] > 2)
            {
                //Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                //    projectile.rotation.ToRotationVector2().RotatedBy(-MathHelper.PiOver2 + Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2, 4),
                //    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.15f, 0.2f));
            }


            if (timer % 1 == 0 && Main.rand.NextBool(1))
            {
                Color purple = new Color(61, 2, 92);
                Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);

                Color purple3 = new Color(121, 7, 179);
                //Color fireRed = Color.Lerp(Color.OrangeRed, Color.Red, 0.05f);//0.15

                for (int i = 0; i < 1; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(0.5f, 0.5f) + projectile.velocity * 0.2f; //0.5

                    float posOffset = -4f + (10f * i);

                    Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * posOffset;

                    FireParticleAlpha fire = new FireParticleAlpha(dustPos + new Vector2(0f, 0f), dustVel, 0.5f, darkPurple, colorMult: 2f, bloomAlpha: 4f, AlphaFade: 0.97f, //col * 0.5f
                        EndAlpha: 0.75f, BlackRemoveThreshold: 0.75f);
                    fire.bloomColor = purple3 with { A = 150 };
                    fire.scaleFadePower = 1.01f; //1.05
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }


            timer++;
            return base.PreAI(projectile);
        }


        float overallScale = 1f;
        float overallAlpha = 1f;
        List<Vector2> previousPositions = new List<Vector2>();
        List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);

            Texture2D arrowTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;


            //Texture2D spike = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/DiamondGlowPMA").Value;
            //Vector2 spikeScale = new Vector2(0.25f, 0.55f) * 0.6f;

            //Main.EntitySpriteDraw(spike, arrowPos, null, Color.Purple with { A = 0 } * 2f, projectile.rotation, spike.Size() / 2, projectile.scale * spikeScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(spike, arrowPos, null, Color.White with { A = 0 }, projectile.rotation, spike.Size() / 2, projectile.scale * 0.5f * spikeScale, SpriteEffects.None);


            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition + Main.rand.NextVector2Circular(3, 3), null, Color.MediumPurple with { A = 0 } * 0.5f, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);

            }

            Main.EntitySpriteDraw(arrowTex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, arrowTex.Size() / 2, projectile.scale, SpriteEffects.None);


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

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);

            Color purple3 = new Color(121, 7, 179);

            Color StripColor(float progress) => Color.Lerp(new Color(42, 2, 82), new Color(42, 2, 82), progress) with { A = 175 } * 1f;

            float StripWidth(float progress)
            {
                float sineWidthMult = 1f + (float)Math.Cos((Main.timeForVisualEffects * 0.24f) + (progress * 12f)) * 0.18f;

                /*
                float toReturn = 0f;
                if (progress < 0.5f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = 1f;// Easings.easeOutSine(1f - LV);
                }
                */


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

                return 25f * toReturn * sineWidthMult * overallScale; //25
            }

            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.05f);

            trailEffect.Parameters["bodyIntensity"].SetValue(4f);
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

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //Dust
            for (int i = 0; i < 5 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: Color.Purple, Scale: Main.rand.NextFloat(0.28f, 0.32f));
            }
            return base.PreKill(projectile, timeLeft);
        }

    }

}
