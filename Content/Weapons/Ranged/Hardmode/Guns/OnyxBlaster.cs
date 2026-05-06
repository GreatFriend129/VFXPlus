using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Gores;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest;
using VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Guns
{
    public class OnyxBlaster : GlobalItem 
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.OnyxBlaster);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicGunProjMiddle>(), 0, 0, player.whoAmI);


            if (Main.projectile[gun].ModProjectile is BasicGunProjMiddle held)
            {
                held.SetProjInfo(
                    GunID: ItemID.OnyxBlaster,
                    AnimTime: 26,
                    NormalXOffset: 18f,
                    DestXOffset: -6f, //-4
                    YRecoilAmount: 0.5f,
                    HoldOffset: new Vector2(0f, 2f),
                    TipPos: new Vector2(38f, -3f),
                    StarPos: new Vector2(32f, -3f)
                    );

                held.timeToStartFade = 2;
                held.isShotgun = true;
            }

            //Explosion
            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(38f, -3f * dir).RotatedBy(velocity.ToRotation());

            Color purple3 = new Color(121, 7, 179);
            for (int i = 0; i < 11; i++) //16
            {
                float progress = (float)i / 10;
                Color col = Color.Lerp(purple3 * 2f, Color.Black * 1f, progress);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 1.15f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.5f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.75f);

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 1.5f;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.1f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 2 + Main.rand.Next(0, 3); i++)
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.15f);


                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1.5f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.9f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 2f;
            }

            //Bullet Casing
            Gore.NewGore(source, position + velocity, new Vector2(velocity.X * -0.25f, -0.75f), ModContent.GoreType<PurpleCasing>());


            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Gun/ONYXBlast") with { Volume = 1f, Pitch = 0f, PitchVariance = 0.1f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, position);

            return true;
        }
    }

    public class OnyxBlasterShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.BlackBolt;
        }


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount2 = 18; //25
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount2)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount2)
                previousPostions.RemoveAt(0);


            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1f)) * 1f;
            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);



            #region vanillaAI with tweaked Dust
            if (projectile.alpha <= 0)
            {
                for (int num20 = 0; num20 < 2; num20++)
                {
                    int num21 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 240);
                    Main.dust[num21].noGravity = true;
                    Main.dust[num21].velocity *= 0.3f;
                    Main.dust[num21].velocity -= projectile.velocity * Main.rand.NextFloat(0.45f, 0.55f);
                    Main.dust[num21].noLight = true;
                }
            }
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 55;
                projectile.scale = 1.3f;
                if (projectile.alpha < 0)
                {
                    projectile.alpha = 0;
                    float num22 = 8f; //16 
                    for (int num24 = 0; (float)num24 < num22; num24++)
                    {
                        Vector2 spinningpoint7 = Vector2.UnitX * 0f;
                        spinningpoint7 += -Vector2.UnitY.RotatedBy((float)num24 * ((float)Math.PI * 2f / num22)) * new Vector2(1f, 4f);
                        spinningpoint7 = spinningpoint7.RotatedBy(projectile.velocity.ToRotation());
                        if (num24 % 3 == 0)
                        {
                            int num25 = Dust.NewDust(projectile.Center, 0, 0, 240);
                            Main.dust[num25].scale = 1f;
                            Main.dust[num25].noLight = true;
                            Main.dust[num25].noGravity = true;
                            Main.dust[num25].position = projectile.Center + spinningpoint7;
                            Main.dust[num25].velocity = Main.dust[num25].velocity * 1f + projectile.velocity * 0.4f;
                        }
                        else
                        {
                            int dust = Dust.NewDust(projectile.Center, 0, 0, ModContent.DustType<GlowPixelCross>(), newColor: new Color(121, 7, 179) * 1.5f, Scale: Main.rand.NextFloat(0.65f, 0.75f) * 0.75f);
                            Main.dust[dust].position = projectile.Center + spinningpoint7 - projectile.velocity.SafeNormalize(Vector2.UnitX) * 30;
                            Main.dust[dust].velocity = Main.dust[dust].velocity * 1f + projectile.velocity * 0.3f;
                            Main.dust[dust].noLight = false;

                            Main.dust[dust].customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                                rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 3, postSlowPower: Main.rand.NextFloat(0.87f, 0.92f), velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
                        }
                    }
                }
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            #endregion
            timer++;
            return false;
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVertexTrail(false);
            });
            

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D glowTex = TextureAssets.Extra[75].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            float drawScale = overallScale * projectile.scale;

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);

            Color color110 = new Color(120, 40, 222, 120) * overallAlpha;
            //VnillaDraw
            for (int num163 = 0; num163 < 4; num163++)
            {
                Main.EntitySpriteDraw(glowTex, drawPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)num163) * 4f, null, color110, projectile.rotation, TexOrigin, 
                    drawScale, 0f);

            }


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, projectile.rotation, TexOrigin, drawScale, SpriteEffects.None);

            return false;

        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);
            Color purp = new Color(215, 18, 215);

            Texture2D trailTexture1 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinnerGlowTrail").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White;
            float StripWidth(float progress) => 45f * Easings.easeOutSine(progress) * overallScale;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));
            float StripWidth2(float progress) => 85f * Easings.easeOutSine(progress) * overallScale;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.04f); //timer * 0.02
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture1);
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.75f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            vertexStrip.DrawTrail();

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["ColorOne"].SetValue(purple.ToVector3() * 2.5f);
            myEffect.Parameters["glowThreshold"].SetValue(0.9f);
            myEffect.Parameters["glowIntensity"].SetValue(1.8f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);
            Color purple3 = new Color(121, 7, 179);


            #region vanillAI
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = 160);
            projectile.Center = projectile.position;
            projectile.maxPenetrate = -1;
            projectile.penetrate = -1;
            projectile.Damage();
            //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            Vector2 vector20 = projectile.Center + Vector2.One * -20f;
            int num94 = 40;
            int num95 = num94;
            for (int num96 = 0; num96 < 4; num96++)
            {
                int num98 = Dust.NewDust(vector20, num94, num95, 240, 0f, 0f, 100, default(Color), 1f);
                Main.dust[num98].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * num94 / 2f;
            }
            for (int num99 = 220; num99 < 20; num99++)
            {
                int num100 = Dust.NewDust(vector20, num94, num95, 62, 0f, 0f, 200, default(Color), 3.7f);
                Main.dust[num100].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * num94 / 2f;
                Main.dust[num100].noGravity = true;
                Main.dust[num100].noLight = true;
                Dust dust139 = Main.dust[num100];
                Dust dust334 = dust139;
                dust334.velocity *= 3f;
                dust139 = Main.dust[num100];
                dust334 = dust139;
                dust334.velocity += projectile.DirectionTo(Main.dust[num100].position) * (2f + Main.rand.NextFloat() * 4f);
                num100 = Dust.NewDust(vector20, num94, num95, 62, 0f, 0f, 100, default(Color), 1.5f);
                Main.dust[num100].position = projectile.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * num94 / 2f;
                dust139 = Main.dust[num100];
                dust334 = dust139;
                dust334.velocity *= 2f;
                Main.dust[num100].noGravity = true;
                Main.dust[num100].fadeIn = 1f;
                Main.dust[num100].color = Color.Crimson * 0.5f;
                Main.dust[num100].noLight = true;
                dust139 = Main.dust[num100];
                dust334 = dust139;
                dust334.velocity += projectile.DirectionTo(Main.dust[num100].position) * 8f;
            }
            for (int num101 = 220; num101 < 20; num101++)
            {
                int num102 = Dust.NewDust(vector20, num94, num95, 62, 0f, 0f, 0, default(Color), 2.7f);
                Main.dust[num102].position = projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(projectile.velocity.ToRotation()) * num94 / 2f;
                Main.dust[num102].noGravity = true;
                Main.dust[num102].noLight = true;
                Dust dust138 = Main.dust[num102];
                Dust dust334 = dust138;
                dust334.velocity *= 3f;
                dust138 = Main.dust[num102];
                dust334 = dust138;
                dust334.velocity += projectile.DirectionTo(Main.dust[num102].position) * 2f;
            }
            for (int num103 = 0; num103 < 50; num103++)
            {
                int num104 = Dust.NewDust(vector20, num94, num95, 240, 0f, 0f, 0, default(Color), 1.5f);
                Main.dust[num104].position = projectile.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(projectile.velocity.ToRotation()) * num94 / 2f;
                Main.dust[num104].noGravity = true;
                Dust dust137 = Main.dust[num104];
                Dust dust334 = dust137;
                dust334.velocity *= 3f;
                dust137 = Main.dust[num104];
                dust334 = dust137;
                dust334.velocity += projectile.DirectionTo(Main.dust[num104].position) * 3f;
            }
            #endregion

            Vector2 effectPos = projectile.Center;

            for (int i = 0; i < 19; i++)
            {
                float prog = (float)i / 19f;

                float proggg = Main.rand.NextFloat();
                Color col = Color.Lerp(purple3 * 2f, Color.Black * 1f, proggg);

                Dust d = Dust.NewDustPerfect(effectPos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 3f) * 2.75f,
                    newColor: col * prog, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 1f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(15, 25), 0.93f, 0.01f, 0.9f); //12 28
            }

            for (int i = 0; i < 12 + Main.rand.Next(0, 6); i++)
            {
                Color col = Main.rand.NextBool() ? Color.Purple * 2f : Color.Purple * 2f;


                float velMult = Main.rand.NextFloat(2f, 6f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(effectPos + randomStart * 5f, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0,
                    newColor: purple3 * 1.5f, Scale: Main.rand.NextFloat(0.35f, 0.55f));

                dust.scale *= 1.75f;

                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);

                dust.noLight = false;
            }

            for (int i = 0; i < 4 + Main.rand.Next(0, 5); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(2f, 2f) * 2f;
                Dust dust = Dust.NewDustPerfect(effectPos + randomStart * 5f, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: purple3 * 1.5f, Scale: Main.rand.NextFloat(0.65f, 0.75f) * 0.65f);

                //dust.velocity += projectile.velocity * 0.2f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            int pulse = Projectile.NewProjectile(null, effectPos + new Vector2(0f, 0f), Vector2.Zero, ModContent.ProjectileType<OnyxPulse>(), 0, 0, Main.myPlayer);


            return false;// base.PreKill(projectile, timeLeft);
        }

    }

    public class OnyxPulse : ModProjectile
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
        float overallScale = 1f;

        int timer = 0;

        float progress = 0f;
        public override void AI()
        {
            if (timer == 0)
                Projectile.rotation = Main.rand.NextFloat(6.28f);

            float timeForPulse = 22f;
            float myProg = Utils.GetLerpValue(0f, timeForPulse, (float)timer, true);

            //Easings.easeOutSine(myProg);

            progress = myProg;

            if (progress > 0.99f)
            {
                progress = 1f;
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawEffect(false);
            });

            DrawEffect(true);

            return false;
        }

        public void DrawEffect(bool giveUp = false)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value); //Trail_2 0.45 totalAlpha
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(1f * progress);// * Easings.easeOutQuint(progress)); //0.5
            myEffect.Parameters["ringPower"].SetValue(0.25f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(0.25f);
            myEffect.Parameters["ringWaveLength"].SetValue(41f);

            //Caustic values

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);
            Color purple3 = new Color(121, 7, 179);

            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                darkPurple.ToVector3() * 1f,
                purple.ToVector3(),
                purple3.ToVector3()
            };


            myEffect.Parameters["gradColors"].SetValue(gradCols);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(2.0f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(4.0f);

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha * 0.65f); //.45
            myEffect.Parameters["fadeStrength"].SetValue(0f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 160 * new Vector2(1f, 1f) * overallScale, SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //V1
            /*
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Noise_1").Value); //Trail_2 0.45 totalAlpha
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(0.5f * progress);// * Easings.easeOutQuint(progress)); //0.5
            myEffect.Parameters["ringPower"].SetValue(0.15f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(0.5f);
            myEffect.Parameters["ringWaveLength"].SetValue(21f * 0f);

            //Caustic values

            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                new Color(28, 3, 51).ToVector3() * 1f,
                new Color(54, 15, 90).ToVector3() * 1f,
                new Color(82, 32, 129).ToVector3(),
                new Color(96, 31, 158).ToVector3(),
                new Color(106, 50, 159).ToVector3()
            };

            
            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);
            Color purple3 = new Color(121, 7, 179);

            Vector3[] gradCols = {
                Color.Black.ToVector3(),
                darkPurple.ToVector3(),
                purple3.ToVector3(),
                purple.ToVector3()
            };

            myEffect.Parameters["gradColors"].SetValue(gradCols);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(3.0f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(0.0f);

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha * 0.65f);
            myEffect.Parameters["fadeStrength"].SetValue(0f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 160 * new Vector2(1f, 1f) * overallScale, SpriteEffects.None, 0f); //0.3

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            */
        }
    }


}
