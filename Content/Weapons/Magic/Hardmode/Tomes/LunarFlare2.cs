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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    public class LunarFlareShotOverride2 : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LunarFlare) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LunarFlareToggle;
        }

        float overallScale = 1f;
        float overallAlpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            int trailCount = 54;  //60
            Vector2 trailPos = projectile.Center;
            previousRotations.Add(projectile.velocity.ToRotation()); //
            previousPositions.Add(trailPos);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            float timeForPopInAnim = 33; 
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.75f)) * 1.1f; //2f

            if (projectile.ai[1] != -1)
                Lighting.AddLight(projectile.Center, Color.Aqua.ToVector3() * projectile.scale * 1f);


            if (timer % 3 == 0 && false)
            {
                for (int i = 0; i < 1; i++)
                {
                    Dust daa = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RenderTargetDustTest>(), Vector2.Zero, Scale: Main.rand.NextFloat(0.85f, 1.15f));
                    daa.rotation = Main.rand.NextFloat(6.28f);

                    //Dust daa = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RenderTargetDustTest>(), projectile.velocity * 0.5f, Scale: Main.rand.NextFloat(0.85f, 1.15f));
                    //daa.velocity = projectile.velocity.RotateRandom(0.2f) * Main.rand.NextFloat(0.85f, 1.15f);
                }


            }

            if (timer % 8 == 0 && Main.rand.NextBool(2) && projectile.ai[1] != -1 && false)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(projectile.velocity.Length() * -3f, Main.rand.NextFloat(-16f, 16f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(10f, 18f);

                Color between = Color.Lerp(Color.Aquamarine, Color.Aqua, Main.rand.NextFloat());

                Dust dp = Dust.NewDustPerfect(pos + new Vector2(0f, 0f), ModContent.DustType<WindLine>(), vel, newColor: between * 1f, Scale: Main.rand.NextFloat(0.75f, 1f));

                WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.9f, TimeToStartShrink: 7, ShrinkYScalePower: 0.5f, XScale: 1.5f, YScale: Main.rand.NextFloat(0.5f, 0.75f), true, KillEarlyTime: 14);
                wlb.colorAlpha = 50;
                wlb.whiteCoreIntensity = 0.5f;
                dp.customData = wlb;
            }

            if (timer % 8 == 0 && Main.rand.NextBool(2) && projectile.ai[1] != -1 && false)
            {
                Color between = Color.Lerp(Color.Aquamarine, Color.Aqua, Main.rand.NextFloat());

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(),
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 7f),
                    newColor: between, Scale: Main.rand.NextFloat(0.6f, 1.2f) * 0.35f);
                //p.alpha = 2;
                p.velocity += projectile.velocity * 0.45f;

                ///p.customData = DustBehaviorUtil.AssignBehavior_GSSBase(rotPower: 0.55f, postSlowPower: 0.89f, velToBeginShrink: 3.75f, fadePower: 0.92f, shouldFadeColor: false);
                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.55f, postSlowPower: 0.89f, velToBeginShrink: 4f, fadePower: 0.92f, shouldFadeColor: false);
            }

            #region VanillaAI
            if (projectile.ai[1] != -1f && projectile.position.Y > projectile.ai[1])
            {
                projectile.tileCollide = true;
            }
            if (projectile.position.HasNaNs())
            {
                projectile.Kill();
                return false;
            }
            //bool num205 = WorldGen.SolidTile(Framing.GetTileSafely((int)projectile.position.X / 16, (int)projectile.position.Y / 16));
            //Dust dust22 = Main.dust[Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 229)];
            //dust22.position = projectile.Center;
            //dust22.velocity = Vector2.Zero;
            //dust22.noGravity = true;
            //if (num205)
            //{
            //    dust22.noLight = true;
            //}
            if (projectile.ai[1] == -1f)
            {
                //Spawn Explosion VFX
                if (projectile.ai[0] == 0f && Main.myPlayer == projectile.owner)
                {
                    Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<LunarExplosionAnim>(), 0, 0f);
                }

                projectile.ai[0] += 1f;
                projectile.velocity = Vector2.Zero;
                projectile.tileCollide = false;
                projectile.penetrate = -1;
                projectile.position = projectile.Center;
                projectile.width = (projectile.height = 140);
                projectile.Center = projectile.position;
                projectile.alpha -= 10;
                if (projectile.alpha < 0)
                {
                    projectile.alpha = 0;
                }
                if (++projectile.frameCounter >= projectile.MaxUpdates * 3)
                {
                    projectile.frameCounter = 0;
                    projectile.frame++;
                }
                if (projectile.ai[0] >= (float)(Main.projFrames[projectile.type] * projectile.MaxUpdates * 3))
                {
                    projectile.Kill();
                }
                return false;
            }
            projectile.alpha = 255;
            if (projectile.numUpdates == 0)
            {
                int num206 = -1;
                float num207 = 60f;
                for (int num208 = 0; num208 < 200; num208++)
                {
                    NPC nPC2 = Main.npc[num208];
                    if (nPC2.CanBeChasedBy(this))
                    {
                        float num209 = projectile.Distance(nPC2.Center);
                        if (num209 < num207 && Collision.CanHitLine(projectile.Center, 0, 0, nPC2.Center, 0, 0))
                        {
                            num207 = num209;
                            num206 = num208;
                        }
                    }
                }
                if (num206 != -1)
                {
                    projectile.ai[0] = 0f;
                    projectile.ai[1] = -1f;
                    projectile.netUpdate = true;
                    return false;
                }
            }
            #endregion

            timer++;
            return false;
        }

        float justShotVal = 1f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawVertexTrail(projectile, false);
            });
            //DrawVertexTrail(projectile, true);


            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            if (projectile.ai[1] == -1)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine3").Value; //ThinGlowLine3
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/LavaTrailV1").Value; //RL3
            //Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Lightning5").Value; //


            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("Playground/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            float sineWidthMult = 1f;// 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.1f;


            Color StripColor(float progress) => Color.White * Easings.easeOutCubic(progress) * overallAlpha;

            float StripWidth(float progress)
            {
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

                return toReturn * sineWidthMult * overallScale * 170f; //150
            }

            float StripWidth2(float progress)
            {
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

                return toReturn * overallScale * 25; //30
            }


            VertexStripFixed vertexStrip = new VertexStripFixed();
            VertexStripFixed vertexStrip2 = new VertexStripFixed();

            //VertexStrip vertexStrip = new VertexStrip();
            //VertexStrip vertexStrip2 = new VertexStrip();

            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            //VertexStripFixed vertexStrip2 = new VertexStripFixed();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);//0.25
            myEffect.Parameters["posterizationSteps"].SetValue(0.0f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.Aqua.ToVector3() * 1f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip.DrawTrail();



            Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);
            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(between.ToVector3() * 4f);
            myEffect.CurrentTechnique.Passes["DefaultPass"].Apply();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //The only thing the LunarFlare projectile does is spawn dust/gores, so we can safely return false without impacting functionality 
            return false;
            /* vanillaKill for reference
            bool flag2 = WorldGen.SolidTile(Framing.GetTileSafely((int)base.position.X / 16, (int)base.position.Y / 16));
			for (int num69 = 0; num69 < 4; num69++)
			{
				Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 31, 0f, 0f, 100, default(Color), 1.5f);
			}
			for (int num70 = 0; num70 < 4; num70++)
			{
				int num71 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 229, 0f, 0f, 0, default(Color), 2.5f);
				Main.dust[num71].noGravity = true;
				Dust dust118 = Main.dust[num71];
				Dust dust334 = dust118;
				dust334.velocity *= 3f;
				if (flag2)
				{
					Main.dust[num71].noLight = true;
				}
				num71 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 229, 0f, 0f, 100, default(Color), 1.5f);
				dust118 = Main.dust[num71];
				dust334 = dust118;
				dust334.velocity *= 2f;
				Main.dust[num71].noGravity = true;
				if (flag2)
				{
					Main.dust[num71].noLight = true;
				}
			}
			for (int num72 = 0; num72 < 1; num72++)
			{
				int num73 = Gore.NewGore(base.position + new Vector2((float)(base.width * Main.rand.Next(100)) / 100f, (float)(base.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64));
				Gore gore32 = Main.gore[num73];
				Gore gore64 = gore32;
				gore64.velocity *= 0.3f;
				Main.gore[num73].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
				Main.gore[num73].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
			}
            */
            return base.PreKill(projectile, timeLeft);
        }
    }

    public class LunarCrack : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }

        float overallAlpha = 1f;
        float overallScale = 0.25f;

        int timer = 0;
        public override void AI()
        {

            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);


            Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * overallScale);

            int timeToStart = 0;
            int timeForAnim = 36; //40
            float animProg = Utils.GetLerpValue(timeToStart, timeToStart + timeForAnim, timer, true);
            float animProg2 = Utils.GetLerpValue(timeToStart, timeToStart + timeForAnim * 0.5f, timer, true);

            overallAlpha = animProg;

            //overallScale = Easings.easeOutCubic(animProg);
            overallScale = Easings.easeOutCubic(animProg2); //MathHelper.Clamp(MathHelper.Lerp(overallScale, 1.5f, 0.08f), 0f, 1f);

            if (animProg == 1f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawCrack(false);
            });

            DrawCrack(true);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            //Main.spriteBatch.Draw(Ball, drawPos, null, Color.DodgerBlue with { A = 100 } * 0.5f * (1f - overallAlpha), Projectile.rotation, Ball.Size() / 2f, Projectile.scale * overallScale * 2f, SpriteEffects.None, 0f); //0.3


            return false;
        }

        Effect myEffect = null;
        public void DrawCrack(bool giveUp = false)
        {
            if (giveUp)
                return;



            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Crack = Mod.Assets.Request<Texture2D>("Assets/Crack/LunarCrack2").Value;
            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Crack/FireSwirlB1k").Value;// Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value; //FireSwirlB1k
            //Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value; //FireSwirlB1k

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("Playground/Effects/Filter/Dissolve", AssetRequestMode.ImmediateLoad).Value;

            float maskVal = Easings.easeInOutQuad(overallAlpha);

            Color myCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

            myEffect.Parameters["progress"].SetValue(1f - maskVal);
            myEffect.Parameters["maskTexture"].SetValue(Mask);
            myEffect.Parameters["zoom"].SetValue(1f);

            myEffect.Parameters["innerCol"].SetValue(myCol.ToVector3());
            myEffect.Parameters["outerCol"].SetValue(myCol.ToVector3());
            myEffect.Parameters["dissolveColMult"].SetValue(1f);

            myEffect.Parameters["mainTexWidth"].SetValue(Crack.Width / 2f);
            myEffect.Parameters["mainTexHeight"].SetValue(Crack.Height / 2f);

            //Main Tex
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(Crack, drawPos, null, Color.White with { A = 0 } * 1f, Projectile.rotation, Crack.Size() / 2f, 0.85f * Projectile.scale * overallScale * 0.4f + (maskVal * 0.1f), SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

    }


    public class PinkCrack : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }

        float overallAlpha = 1f;
        float overallScale = 0.25f;

        int timer = 0;
        public override void AI()
        {

            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);


            Lighting.AddLight(Projectile.Center, Color.HotPink.ToVector3() * overallScale);

            int timeToStart = 0;
            int timeForAnim = 30; //40
            float animProg = Utils.GetLerpValue(timeToStart, timeToStart + timeForAnim, timer, true);
            float animProg2 = Utils.GetLerpValue(timeToStart, timeToStart + timeForAnim * 0.5f, timer, true);

            overallAlpha = animProg;

            //overallScale = Easings.easeOutCubic(animProg);
            overallScale = Easings.easeOutCubic(animProg2); //MathHelper.Clamp(MathHelper.Lerp(overallScale, 1.5f, 0.08f), 0f, 1f);

            if (animProg == 1f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawCrack(false);
            });

            DrawCrack(true);

            return false;
        }

        Effect myEffect = null;
        public void DrawCrack(bool giveUp = false)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Crack = Mod.Assets.Request<Texture2D>("Assets/Crack/GlowCrack").Value;
            Texture2D Mask = Mod.Assets.Request<Texture2D>("Assets/Noise/noise").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Misc/GlowCrack", AssetRequestMode.ImmediateLoad).Value;

            float maskVal = Easings.easeInOutQuad(overallAlpha);

            Color myCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0f);

            myEffect.Parameters["color"].SetValue(myCol.ToVector3() * 6f); //15
            myEffect.Parameters["glowThreshold"].SetValue(0.8f); //0.8f
            myEffect.Parameters["glowPower"].SetValue(2.5f); //3.5
            myEffect.Parameters["progress"].SetValue(maskVal);
            myEffect.Parameters["maskTexture"].SetValue(Mask);

            myEffect.Parameters["innerCol"].SetValue(Color.HotPink.ToVector3());
            myEffect.Parameters["outerCol"].SetValue(Color.HotPink.ToVector3());

            myEffect.Parameters["mainTexWidth"].SetValue(Crack.Width / 2f);
            myEffect.Parameters["mainTexHeight"].SetValue(Crack.Height / 2f);

            //Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            //Main.spriteBatch.Draw(Ball, drawPos, null, myCol with { A = 0 } * Easings.easeInSine(overallAlpha) * 0.25f, Projectile.rotation, Ball.Size() / 2f, Projectile.scale * overallScale * 1.45f, SE, 0f); //0.3

            //Main Tex
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(Crack, drawPos, null, myCol * overallAlpha * 1f, Projectile.rotation, Crack.Size() / 2f, Projectile.scale * overallScale * 0.4f + (maskVal * 0.2f), SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

    }

}
