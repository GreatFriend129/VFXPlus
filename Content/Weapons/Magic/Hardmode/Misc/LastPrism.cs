using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPLus.Common;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{

    //This also handles the combined laser vfx
    public class LastPrismHeldProjectileOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LastPrism) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LastPrismToggle;
        }

        //How far away from prism does laser start
        private float offsetDistance = 18f;

        float endRot = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (projectile.ai[0] == 180f)
            {
                GetCombinedLaserInfo();

                drawCombinedLaser = true;

                for (int i = 0; i < 35; i++)
                {
                    Color col = Main.hslToRgb(Main.rand.NextFloat(0f, 1f), 1f, 0.5f);


                    Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1.5f) * Main.rand.NextFloat(10f, 30f);

                    Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<WindLine>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1.5f);
                    p.customData = new WindLineBehavior(VelFadePower: 0.92f, TimeToStartShrink: 11, YScale: 0.5f);
                }

                //FlashSystem.SetCAFlashEffect(0.4f, 25, 1f, 0.75f, true);

                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = 60f;

                combinedLaserStartBoostPower = 1f;

                //Sound
                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Cries/astrolotl") with { Volume = .15f, Pitch = .40f, MaxInstances = -1 };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/water_blast_projectile_spell_03") with { Volume = .25f, Pitch = .8f, MaxInstances = -1 };
                SoundEngine.PlaySound(style2, projectile.Center);

                SoundStyle style4 = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = .15f, Pitch = .3f, MaxInstances = -1 };
                SoundEngine.PlaySound(style4, projectile.Center);

                SoundStyle style5 = new SoundStyle("Terraria/Sounds/Item_163") with { Volume = 0.5f, Pitch = .7f, MaxInstances = -1 }; 
                SoundEngine.PlaySound(style5, projectile.Center);
            }

            if (projectile.ai[0] > 180 && projectile.ai[0] < 195)
            {
                float pow = Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower;
                Main.player[projectile.owner].GetModPlayer<ScreenShakePlayer>().ScreenShakePower = Math.Clamp(pow, 15f, 60f);
            }

            if (drawCombinedLaser)
            {
                if (timer % 1 == 0)
                {
                    float dist = projectile.ai[2];

                    for (int i = 0; i < dist * 0.75f; i += 125)
                    {
                        Vector2 pos = projectile.Center + new Vector2(i, 0f).RotatedBy(projectile.velocity.ToRotation());
                        float rot = projectile.velocity.ToRotation();


                        //Color rainbow = Main.hslToRgb(Main.rand.NextFloat(0f, 1f), 1f, 0.55f, 0) * 1f;
                        int dustColsLength = lpci.dustColors.Count;
                        Color rainbow = lpci.dustColors[Main.rand.Next(0, dustColsLength)];
                        //rainbow = Color.Lerp(rainbow, Color.White, 0.25f);

                        if (Main.rand.NextBool(3))
                        {
                            Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-35, 35);
                            Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.05f) * Main.rand.NextFloat(2, 7) * 1.4f; //1f

                            if (!Main.rand.NextBool(3))
                            {
                                Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowFlare>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.5f);
                                d.noLight = false;
                                d.customData = new GlowFlareBehavior(0.4f, 2.5f);
                            }
                            else
                            {
                                Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowPixelCross>(), vel * 3.5f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.35f);
                                d.noLight = false;
                                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.17f, postSlowPower:0.84f, velToBeginShrink: 5f, fadePower: 0.9f, shouldFadeColor: false);
                            }
                        }

                    }
                }

                //End point dust
                if (timer % 1 == 0)
                {
                    Vector2 dustPos = projectile.Center + new Vector2(projectile.ai[2], 0f).RotatedBy(projectile.velocity.ToRotation());
                    for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++) //4 //2,2
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(22f, 22f);

                        int dustColsLength = lpci.dustColors.Count;
                        Color rainbow = lpci.dustColors[Main.rand.Next(0, dustColsLength)];
                        //rainbow = Color.Lerp(rainbow, Color.White, 0.25f);
                        //Color rainbow = Main.hslToRgb(Main.rand.NextFloat(0f, 1f), 1f, 0.55f, 0);

                        Dust p = Dust.NewDustPerfect(dustPos, ModContent.DustType<WindLine>(), vel,
                            newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1f);
                        p.customData = new WindLineBehavior(VelFadePower: 0.92f, TimeToStartShrink: 5, YScale: 0.5f);


                        if (i == 0)
                        {
                            Dust p2 = Dust.NewDustPerfect(dustPos + vel * 3f, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 0.85f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.35f);
                            p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.1f);
                        }

                    }
                }

                //Light at end of laser
                Vector2 lightPos = projectile.Center + new Vector2(projectile.ai[2], 0f).RotatedBy(projectile.velocity.ToRotation());
                Lighting.AddLight(lightPos, Color.White.ToVector3() * 1.1f);

                endRot += 1.15f * (projectile.velocity.X > 0 ? 1f : -1f); //0.85f

            }

            combinedLaserStartBoostPower = Math.Clamp(MathHelper.Lerp(combinedLaserStartBoostPower, -0.5f, 0.06f), 0f, 10f);

            timer++;

            return base.PreAI(projectile);
        }


        float combinedLaserStartBoostPower = 0f;
        float combinedLaserScale = 1f;
        bool drawCombinedLaser = false;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (drawCombinedLaser)
            {
                #region BlackBorder
                Texture2D black = Mod.Assets.Request<Texture2D>("Assets/BlackWall").Value;

                if (Main.player[projectile.owner] == Main.LocalPlayer && false)
                {
                    Vector2 pos = projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[projectile.owner].gfxOffY);

                    float opac = 0.06f;

                    Vector2 blackOrigin = new Vector2(black.Width / 2, 0);
                    Vector2 blackScale = new Vector2(5f, 5f) * 1f;

                    Main.EntitySpriteDraw(black, pos, null, Color.Black * opac, projectile.velocity.ToRotation(), blackOrigin, blackScale, 0f);
                    Main.EntitySpriteDraw(black, pos, null, Color.Black * opac, projectile.velocity.ToRotation() + MathHelper.Pi, blackOrigin, blackScale, 0f);
                }

                #endregion


                ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
                {
                    RainbowLaser(projectile);
                    RainbowSigil(projectile);
                });
            }


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 1f * Main.player[projectile.owner].gfxOffY);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects SE = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            //Border
            Color[] rainbow = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.DodgerBlue, Color.Violet };
            for (int i = 0; i < 6; i++)
            {
                float rot = i * (MathHelper.TwoPi / 6f);

                float prog = Math.Clamp(projectile.ai[0] / 180f, 0f, 1f);
                float dis = MathHelper.Lerp(15f, 3f, Easings.easeOutQuad(prog)); //10f
                float scale = MathHelper.Lerp(2f, 1f, prog);

                Vector2 off = new Vector2(dis, 0f).RotatedBy(rot + (float)Main.timeForVisualEffects * 0.15f);
                Main.EntitySpriteDraw(vanillaTex, drawPos + off + new Vector2(0f, 0f), sourceRectangle,
                   rainbow[i] with { A = 0 } * 0.35f * Easings.easeInSine(prog), projectile.rotation, TexOrigin, projectile.scale * 1.07f * scale, SE);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2CircularEdge(2f, 2f), sourceRectangle, Color.White with { A = 0 } * 0.1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f, SE);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 180 }, projectile.rotation, TexOrigin, projectile.scale, SE);

            //Not returning true and drawing manually because the last prism apparently doesn't respect gfxOffY 
            return false;
        }

        Effect myEffect = null;
        Effect laserEffect = null;
        public void RainbowLaser(Projectile projectile)
        {
            float rot = projectile.velocity.ToRotation();

            Vector2 startPoint = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * offsetDistance;
            Vector2 endPoint = startPoint + new Vector2(projectile.ai[2] - offsetDistance, 0f).RotatedBy(rot);

            Vector2[] pos_arr = { startPoint, endPoint };
            float[] rot_arr = { rot, rot };

            Color StripColor(float progress) => Color.White;
            float StripWidth(float progress) => (120f * 1f) * combinedLaserScale + (combinedLaserStartBoostPower * 250f); //200

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            #region Params
            if (laserEffect == null)
                laserEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            laserEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            String GradLocation = "VFXPlus/Assets/Gradients/";

            laserEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value); //ThinLineGlowClear
            laserEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>(GradLocation + lpci.textureLocation).Value);
            laserEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            laserEffect.Parameters["satPower"].SetValue(0.8f - (combinedLaserStartBoostPower * 0.8f)); //0.9f

            laserEffect.Parameters["sampleTexture1"].SetValue(CommonTextures.ThinGlowLine.Value);
            laserEffect.Parameters["sampleTexture2"].SetValue(CommonTextures.spark_06.Value);
            laserEffect.Parameters["sampleTexture3"].SetValue(CommonTextures.Extra_196_Black.Value);
            laserEffect.Parameters["sampleTexture4"].SetValue(CommonTextures.Trail5Loop.Value);

            laserEffect.Parameters["grad1Speed"].SetValue(lpci.grad1Speed);
            laserEffect.Parameters["grad2Speed"].SetValue(lpci.grad2Speed);
            laserEffect.Parameters["grad3Speed"].SetValue(lpci.grad3Speed);
            laserEffect.Parameters["grad4Speed"].SetValue(lpci.grad4Speed);

            laserEffect.Parameters["tex1Mult"].SetValue(1.25f);
            laserEffect.Parameters["tex2Mult"].SetValue(1.5f);
            laserEffect.Parameters["tex3Mult"].SetValue(1.15f);
            laserEffect.Parameters["tex4Mult"].SetValue(2.5f); //1.5
            laserEffect.Parameters["totalMult"].SetValue(1f);

            float dist = (endPoint - startPoint).Length();
            float repVal = dist / 2000f;
            laserEffect.Parameters["gradientReps"].SetValue(0.75f * repVal); //1f
            laserEffect.Parameters["tex1reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex2reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex3reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex4reps"].SetValue(1.15f * repVal);

            laserEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.024f); //0.006
            #endregion

            laserEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public void RainbowSigil(Projectile projectile)
        {
            //TODO load all of these once
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Flare/Simple Lens Flare_11").Value;
            Texture2D star2 = Mod.Assets.Request<Texture2D>("Assets/Flare/flare_16").Value;
            Texture2D sigil = Mod.Assets.Request<Texture2D>("Assets/Orbs/whiteFireEyeA").Value;
            Texture2D orbGlow = Mod.Assets.Request<Texture2D>("Assets/Orbs/circle_05").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RainbowSigil", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["rotation"].SetValue(endRot * 0.03f * 2.5f);
            myEffect.Parameters["rainbowRotation"].SetValue(endRot * 0.01f * 2.5f);
            myEffect.Parameters["intensity"].SetValue(1f);
            myEffect.Parameters["fadeStrength"].SetValue(1f);

            Vector2 drawPos = projectile.Center - Main.screenPosition + projectile.velocity.SafeNormalize(Vector2.UnitX) * offsetDistance;
            float rot = projectile.velocity.ToRotation();
            Vector2 endPoint = projectile.Center - Main.screenPosition + new Vector2(projectile.ai[2] - offsetDistance, 0f).RotatedBy(rot);


            float sin1 = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);
            float sin2 = MathF.Cos((float)Main.timeForVisualEffects * 0.06f);
            float sin3 = -MathF.Cos(((float)Main.timeForVisualEffects * 0.08f) / 2f) + 1f;

            Vector2 sigilScale1 = new Vector2(0.2f, 1f) * 0.55f * combinedLaserScale * 1f;
            Vector2 sigilScale2 = sigilScale1 * (1.75f + (0.25f * sin1)) * 1f;

            //Main.spriteBatch.Draw(orb, endPoint + new Vector2(0f, 0f), null, Color.White with { A = 0 } * 0.3f, 0f, orb.Size() / 2f, 2f, 0, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            //Main sigil
            Main.spriteBatch.Draw(sigil, drawPos, null, Color.White, rot, sigil.Size() / 2, sigilScale1, 0, 0f);
            Main.spriteBatch.Draw(sigil, drawPos, null, Color.White, rot, sigil.Size() / 2, sigilScale1, 0, 0f);

            Main.spriteBatch.Draw(star, drawPos + new Vector2(1f, 0f).RotatedBy(rot) * (15f * sin3), null, Color.White, rot, star.Size() / 2, sigilScale2, 0, 0f);

            Main.spriteBatch.Draw(star2, drawPos, null, Color.White, rot, star2.Size() / 2, sigilScale1 * 1f, 0, 0f);
            Main.spriteBatch.Draw(star2, drawPos, null, Color.White, rot, star2.Size() / 2, sigilScale1 * 1f, 0, 0f);


            //Flare at the end of the laser
            Main.spriteBatch.Draw(orbGlow, endPoint + new Vector2(0f, 0f), null, Color.White, endRot * 0.1f, orbGlow.Size() / 2f, 0.5f, 0, 0f);

            float endScale = 0.7f + (combinedLaserStartBoostPower * 0.5f);
            Main.spriteBatch.Draw(star, endPoint, null, Color.White, endRot * 0.02f, star.Size() / 2f, endScale * 0.45f, 0, 0f);
            Main.spriteBatch.Draw(star2, endPoint, null, Color.White, endRot * 0.05f, star2.Size() / 2f, endScale * 0.6f, 0, 0f);
            Main.spriteBatch.Draw(star2, endPoint, null, Color.White, endRot * 0.077f, star2.Size() / 2f, endScale * 0.35f, 0, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.EffectMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //^ Reset with EffectMatrix because otherwise the wind lines draw fucked up for some reason
        }


        LastPrismColorInfo lpci = null;
        public void GetCombinedLaserInfo()
        {
            String configOption = ModContent.GetInstance<MiscCustomization>().LastPrismPrideColor;

            if (configOption == "None")
                lpci = new LastPrismColorInfo(LastPrismColorType.None, "RainbowGrad1", 0.66f, 0.66f, 1.03f, 0.77f);
            else if (configOption == "Lesbian")
                lpci = new LastPrismColorInfo(LastPrismColorType.Lesbian, "Pride/LesbianGrad", 0.66f, 0.66f, 1.03f, 0.77f);
            else if (configOption == "Bisexual")
                lpci = new LastPrismColorInfo(LastPrismColorType.Bisexual, "Pride/BisexualGrad", 1f, 1f, 1f, 1f);
            else if (configOption == "Trans")
                lpci = new LastPrismColorInfo(LastPrismColorType.Trans, "Pride/TransGrad", 0.8f, 0.8f, 0.8f, 0.8f);
            else if (configOption == "Non-Binary") //Fucked loop
                lpci = new LastPrismColorInfo(LastPrismColorType.NonBinary, "Pride/NBGrad", 0.85f, 0.85f, 0.85f, 0.85f);
            else if (configOption == "Asexual") //Fucked Black
                lpci = new LastPrismColorInfo(LastPrismColorType.Asexual, "Pride/AceGrad", 1f, 1f, 1f, 1f);
            else if (configOption == "Aromantic") //Fuvcked Black
                lpci = new LastPrismColorInfo(LastPrismColorType.Aromantic, "Pride/AroGrad", 1f, 1f, 1f, 1f);
            else if (configOption == "Aroace")
                lpci = new LastPrismColorInfo(LastPrismColorType.Aroace, "Pride/AroaceGrad", 1.1f, 1.1f, 1.1f, 1.1f);
        }
    }

    public class LastPrismColorInfo
    {
        public String textureLocation = "";

        public float grad1Speed = 2f / 3f;
        public float grad2Speed = 2f / 3f;
        public float grad3Speed = 3.1f / 3f;
        public float grad4Speed = 2.3f / 3f;

        public LastPrismColorInfo(String TexLocation, float Grad1Speed = 1f, float Grad2Speed = 1f, float Grad3Speed = 1f, float Grad4Speed = 1f)  
        { 
            textureLocation = TexLocation;
            grad1Speed = Grad1Speed;
            grad2Speed = Grad2Speed;
            grad3Speed = Grad3Speed;
            grad4Speed = Grad4Speed;
        }

        public LastPrismColorInfo(LastPrismColorType Type, String TexLocation, float Grad1Speed = 1f, float Grad2Speed = 1f, float Grad3Speed = 1f, float Grad4Speed = 1f)
        {
            //Yandev moment
            if (Type == LastPrismColorType.None)
            {
                dustColors.Add(Color.Red);
                dustColors.Add(Color.Orange);
                dustColors.Add(Color.Yellow);
                dustColors.Add(Color.LawnGreen);
                dustColors.Add(Color.DeepSkyBlue);
                dustColors.Add(Color.Purple);
            }
            else if (Type == LastPrismColorType.Lesbian)
            {
                dustColors.Add(Color.Purple);
                dustColors.Add(Color.Orange);
                dustColors.Add(Color.Goldenrod);
                dustColors.Add(Color.HotPink);
                dustColors.Add(Color.HotPink);
            }
            else if (Type == LastPrismColorType.Bisexual)
            {
                dustColors.Add(Color.DodgerBlue);
                dustColors.Add(Color.DeepPink);
                dustColors.Add(Color.Purple);
            }
            else if (Type == LastPrismColorType.Trans)
            {
                dustColors.Add(Color.SkyBlue);
                dustColors.Add(Color.LightSkyBlue);
                dustColors.Add(Color.DeepSkyBlue);
                dustColors.Add(Color.HotPink);
                dustColors.Add(Color.Pink);
                dustColors.Add(Color.LightPink);
            }
            else if (Type == LastPrismColorType.NonBinary)
            {
                dustColors.Add(Color.Gold);
                dustColors.Add(Color.Purple);
                dustColors.Add(Color.Purple * 1f);
            }
            else if (Type == LastPrismColorType.Asexual)
            {
                dustColors.Add(Color.Purple);
                dustColors.Add(Color.Purple * 1.5f);
            }
            else if (Type == LastPrismColorType.Aromantic)
            {
                dustColors.Add(Color.ForestGreen);
                dustColors.Add(Color.DarkGreen);
            }
            else if (Type == LastPrismColorType.Aroace)
            {
                dustColors.Add(Color.Orange);
                dustColors.Add(Color.Gold);
                dustColors.Add(Color.DeepSkyBlue);
                dustColors.Add(Color.DodgerBlue);
                dustColors.Add(Color.DodgerBlue);
                dustColors.Add(Color.Gold);
            }

            textureLocation = TexLocation;

            grad1Speed = Grad1Speed;
            grad2Speed = Grad2Speed;
            grad3Speed = Grad3Speed;
            grad4Speed = Grad4Speed;
        }

        public List<Color> dustColors = new List<Color>();

        public List<Color> laserMainColors = new List<Color>();

        public List<Color> laserBackColors = new List<Color>();
    }

    public enum LastPrismColorType
    {
        None = 0,
        Lesbian = 1, 
        Bisexual = 2, 
        Trans = 3,
        NonBinary = 4, 
        Asexual = 5,
        Aromantic = 6, 
        Aroace = 7
    }

    public class LastPrismLaserOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LastPrismLaser) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LastPrismToggle;
        }

        public override void SetDefaults(Projectile entity)
        {
            entity.hide = true;
            base.SetDefaults(entity);
        }

        public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        int timer = 0;
        LastPrismColorInfo lpci = null;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                String configOption = ModContent.GetInstance<MiscCustomization>().LastPrismPrideColor;

                if (configOption == "None")
                    lpci = new LastPrismColorInfo(LastPrismColorType.None, "RainbowGrad1", 0.66f, 0.66f, 1.03f, 0.77f);
                else if (configOption == "Lesbian")
                    lpci = new LastPrismColorInfo(LastPrismColorType.Lesbian, "Pride/LesbianGrad", 1f, 1f, 1f, 1f);
                else if (configOption == "Bisexual")
                    lpci = new LastPrismColorInfo(LastPrismColorType.Bisexual, "Pride/BisexualGrad", 1f, 1f, 1f, 1f);
                else if (configOption == "Trans")
                    lpci = new LastPrismColorInfo(LastPrismColorType.Trans, "Pride/TransGrad", 0.66f, 0.66f, 0.66f, 0.66f);
                else if (configOption == "Non-Binary") //Fucked loop
                    lpci = new LastPrismColorInfo(LastPrismColorType.NonBinary, "Pride/NBGrad", 1f, 1f, 1f, 1f);
                else if (configOption == "Asexual") //Fucked Black
                    lpci = new LastPrismColorInfo(LastPrismColorType.Asexual, "Pride/AceGrad", 1f, 1f, 1f, 1f);
                else if (configOption == "Aromantic") //Fuvcked Black
                    lpci = new LastPrismColorInfo(LastPrismColorType.Aromantic, "Pride/AroGrad", 1f, 1f, 1f, 1f);
                else if (configOption == "Aroace")
                    lpci = new LastPrismColorInfo(LastPrismColorType.Aroace, "Pride/AroaceGrad", 1f, 1f, 1f, 1f);
            }


            #region vanilla LPL behavior (without dust) && now respects gfxOffYproj
            Vector2? vector133 = null;
            if (projectile.velocity.HasNaNs() || projectile.velocity == Vector2.Zero)
            {
                projectile.velocity = -Vector2.UnitY;
            }

            if (false)
                Main.NewText("PENIS");
            else
            {
                if (projectile.type != 632 || !Main.projectile[(int)projectile.ai[1]].active || Main.projectile[(int)projectile.ai[1]].type != 633)
                {
                    projectile.Kill();
                    return false;
                }
                float num750 = (float)(int)projectile.ai[0] - 2.5f;
                Vector2 vector140 = Vector2.Normalize(Main.projectile[(int)projectile.ai[1]].velocity);
                Projectile projectile2 = Main.projectile[(int)projectile.ai[1]];
                float num751 = num750 * ((float)Math.PI / 6f);
                float num752 = 20f;
                Vector2 zero2 = Vector2.Zero;
                float num753 = 1f;
                float num754 = 15f;
                float num756 = -2f;
                if (projectile2.ai[0] < 180f)
                {
                    num753 = 1f - projectile2.ai[0] / 180f;
                    num754 = 20f - projectile2.ai[0] / 180f * 14f;
                    if (projectile2.ai[0] < 120f)
                    {
                        num752 = 20f - 4f * (projectile2.ai[0] / 120f);
                        projectile.Opacity = projectile2.ai[0] / 120f * 0.4f;
                    }
                    else
                    {
                        num752 = 16f - 10f * ((projectile2.ai[0] - 120f) / 60f);
                        projectile.Opacity = 0.4f + (projectile2.ai[0] - 120f) / 60f * 0.6f;
                    }
                    num756 = -22f + projectile2.ai[0] / 180f * 20f;
                }
                else
                {
                    num753 = 0f;
                    num752 = 1.75f;
                    num754 = 6f;
                    projectile.Opacity = 1f;
                    num756 = -2f;
                }
                float num757 = (projectile2.ai[0] + num750 * num752) / (num752 * 6f) * ((float)Math.PI * 2f);
                num751 = Vector2.UnitY.RotatedBy(num757).Y * ((float)Math.PI / 6f) * num753;
                zero2 = (Vector2.UnitY.RotatedBy(num757) * new Vector2(4f, num754)).RotatedBy(projectile2.velocity.ToRotation());
                projectile.position = projectile2.Center + vector140 * 16f - projectile.Size / 2f + new Vector2(0f, 0f - Main.projectile[(int)projectile.ai[1]].gfxOffY);
                projectile.position += projectile2.velocity.ToRotation().ToRotationVector2() * num756;
                projectile.position += zero2;
                projectile.velocity = Vector2.Normalize(projectile2.velocity).RotatedBy(num751);
                projectile.scale = 1.4f * (1f - num753);
                projectile.damage = projectile2.damage;
                if (projectile2.ai[0] >= 180f)
                {
                    ////////!!!!!!!!!!!!!!!! I added this
                    projectile2.ai[2] = projectile.localAI[1];

                    projectile.damage *= 3;
                    vector133 = projectile2.Center;
                }
                if (!Collision.CanHitLine(Main.player[projectile.owner].Center, 0, 0, projectile2.Center, 0, 0))
                {
                    vector133 = Main.player[projectile.owner].Center;
                }
                projectile.friendly = projectile2.ai[0] > 30f;
            }
            if (projectile.velocity.HasNaNs() || projectile.velocity == Vector2.Zero)
            {
                projectile.velocity = -Vector2.UnitY;
            }
            float num761 = projectile.velocity.ToRotation();

            projectile.rotation = num761 - (float)Math.PI / 2f;
            projectile.velocity = num761.ToRotationVector2();
            float num762 = 0f;
            float num763 = 0f;
            Vector2 samplingPoint = projectile.Center;
            if (vector133.HasValue)
            {
                samplingPoint = vector133.Value;
            }
            if (projectile.type == 632)
            {
                num762 = 2f;
                num763 = 0f;
            }

            float[] array2 = new float[(int)num762];
            Collision.LaserScan(samplingPoint, projectile.velocity, num763 * projectile.scale, 2400f, array2);
            float num764 = 0f;
            for (int num765 = 0; num765 < array2.Length; num765++)
            {
                num764 += array2[num765];
            }
            num764 /= num762;
            float amount = 0.5f;
            if (projectile.type == 455)
            {
                NPC nPC3 = Main.npc[(int)projectile.ai[1]];
                if (nPC3.type == 396)
                {
                    Player player11 = Main.player[nPC3.target];
                    if (!Collision.CanHitLine(nPC3.position, nPC3.width, nPC3.height, player11.position, player11.width, player11.height))
                    {
                        num764 = Math.Min(2400f, Vector2.Distance(nPC3.Center, player11.Center) + 150f);
                        amount = 0.75f;
                    }
                }
            }
            if (projectile.type == 632)
            {
                amount = 0.75f;
            }
            projectile.localAI[1] = MathHelper.Lerp(projectile.localAI[1], num764, amount);

            if (projectile.type != 632 || !(Math.Abs(projectile.localAI[1] - num764) < 100f) || !(projectile.scale > 0.15f))
            {
                return false;
            }

            float laserLuminance = 0.5f;
            float laserAlphaMultiplier = 0f;
            float lastPrismHue = projectile.GetLastPrismHue(projectile.ai[0], ref laserLuminance, ref laserAlphaMultiplier);
            Color color = Main.hslToRgb(lastPrismHue, 1f, laserLuminance);
            color.A = (byte)((float)(int)color.A * laserAlphaMultiplier);
            Color color2 = color;
            Vector2 vector154 = projectile.Center + projectile.velocity * (projectile.localAI[1] - 14.5f * projectile.scale);
            float x5 = Main.rgbToHsl(new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB)).X;
            for (int num791 = 220; num791 < 2; num791++) //!!!!!!!!!!!!!!!!
            {
                float num792 = projectile.velocity.ToRotation() + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
                float num793 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                Vector2 vector155 = new Vector2((float)Math.Cos(num792) * num793, (float)Math.Sin(num792) * num793);
                int num794 = Dust.NewDust(vector154, 0, 0, 267, vector155.X, vector155.Y);
                Main.dust[num794].color = color;
                Main.dust[num794].scale = 1.2f;
                if (projectile.scale > 1f)
                {
                    Dust dust86 = Main.dust[num794];
                    Dust dust212 = dust86;
                    dust212.velocity *= projectile.scale;
                    dust86 = Main.dust[num794];
                    dust212 = dust86;
                    dust212.scale *= projectile.scale;
                }
                Main.dust[num794].noGravity = true;
                if (projectile.scale != 1.4f && num794 != 6000)
                {
                    Dust dust160 = Dust.CloneDust(num794);
                    dust160.color = Color.White;
                    Dust dust85 = dust160;
                    Dust dust212 = dust85;
                    dust212.scale /= 2f;
                }
                float hue = (x5 + Main.rand.NextFloat() * 0.4f) % 1f;
                Main.dust[num794].color = Color.Lerp(color, Main.hslToRgb(hue, 1f, 0.75f), projectile.scale / 1.4f);
            }
            if (Main.rand.Next(5) == 0 && false) //!!!!!!!!!!!!!!!!!
            {
                Vector2 vector157 = projectile.velocity.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * projectile.width;
                int num795 = Dust.NewDust(vector154 + vector157 - Vector2.One * 4f, 8, 8, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust87 = Main.dust[num795];
                Dust dust212 = dust87;
                dust212.velocity *= 0.5f;
                Main.dust[num795].velocity.Y = 0f - Math.Abs(Main.dust[num795].velocity.Y);
            }
            DelegateMethods.v3_1 = color.ToVector3() * 0.3f;
            float value24 = 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
            Vector2 size2 = new Vector2(projectile.velocity.Length() * projectile.localAI[1], (float)projectile.width * projectile.scale);
            float num796 = projectile.velocity.ToRotation();
            if (Main.netMode != 2)
            {
                ((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(projectile.position + new Vector2(size2.X * 0.5f, 0f).RotatedBy(num796), new Color(0.5f, 0.1f * (float)Math.Sign(value24) + 0.5f, 0f, 1f) * Math.Abs(value24), size2, RippleShape.Square, num796);
            }
            Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * projectile.localAI[1], (float)projectile.width * projectile.scale, DelegateMethods.CastLight);


            #endregion


            #region tileDust
            Projectile parent = Main.projectile[(int)projectile.ai[1]];

            if (timer % 1 == 0 && parent.ai[0] < 180f)
            {
                for (int i = 0; i < 1; i++)
                {
                    //Color col = Main.hslToRgb(lastPrismHue, 1f, 0.65f);// color;
                    int dustColsLength = lpci.dustColors.Count;
                    Color col = lpci.dustColors[Main.rand.Next(0, dustColsLength)];

                    Vector2 vel = Main.rand.NextVector2Circular(2.75f, 2.75f) * (1f + projectile.scale * 0.5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + (projectile.velocity * projectile.localAI[1]), ModContent.DustType<PixelGlowOrb>(), vel * Main.rand.NextFloat(0.8f, 1.05f), 
                        newColor: col * 1f, Scale: Main.rand.NextFloat(0.35f, 0.4f) * projectile.scale);
                    p.customData = DustBehaviorUtil.AssignBehavior_PGOBase(velToBeginShrink: 2.5f, fadePower: 0.9f);
                }
            }
            #endregion

            timer++;
            return false;
        }


        float overallScale = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Projectile parent = Main.projectile[(int)projectile.ai[1]];

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                if (parent.ai[0] < 180f)
                    DrawVertexTrailRainbow(projectile, false);
            });

            return false;

        }

        Effect myEffect = null;
        //makes the scrolling of each laser start at a random point
        float randomTimeOffset = Main.rand.NextFloat(0f, 0.15f);
        public void DrawVertexTrailRainbow(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            float colAlpha = projectile.Opacity * 0.4f;
            float easedAlpha = Easings.easeOutQuad(projectile.Opacity);

            float laserLuminance = 0.5f;
            float laserAlphaMultiplier = 0f;
            Color lastPrismCol = Main.hslToRgb(projectile.GetLastPrismHue(projectile.ai[0], ref laserLuminance, ref laserAlphaMultiplier), 1f, laserLuminance) * colAlpha;

            //Make the color a bit brighter
            lastPrismCol = Color.Lerp(lastPrismCol, Color.White, 0.1f);

            Vector2 startPoint = projectile.Center + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Vector2 endPoint = startPoint + (projectile.velocity * projectile.localAI[1]);
            float dist = (endPoint - startPoint).Length();

            //EndPoints
            Texture2D bloomOrb = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;
            float bloomEndSize = 1.3f * colAlpha;
            //Main.EntitySpriteDraw(bloomOrb, endPoint - Main.screenPosition, null, lastPrismCol with { A = 0 } * 0.85f, projectile.velocity.ToRotation(), bloomOrb.Size() / 2f, bloomEndSize, 0, 0);
            Main.EntitySpriteDraw(bloomOrb, endPoint - Main.screenPosition, null, Color.White with { A = 0 } * 0.85f, projectile.velocity.ToRotation(), bloomOrb.Size() / 2f, 1f * bloomEndSize, 0, 0);

            //Main.EntitySpriteDraw(bloomOrb, startPoint - Main.screenPosition, null, lastPrismCol with { A = 0 } * 0.85f, projectile.velocity.ToRotation(), bloomOrb.Size() / 2f, bloomEndSize, 0, 0);
            Main.EntitySpriteDraw(bloomOrb, startPoint - Main.screenPosition, null, Color.White with { A = 0 } * 0.85f, projectile.velocity.ToRotation(), bloomOrb.Size() / 2f, 1f * bloomEndSize, 0, 0);


            //TRAIL
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture1 = CommonTextures.EnergyTex.Value;
            Texture2D trailTexture2 = CommonTextures.ThinGlowLine.Value;

            Vector2[] pos_arr = { startPoint, endPoint };
            float[] rot_arr = { projectile.velocity.ToRotation(), projectile.velocity.ToRotation() };

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.0f;

            Color StripColor(float progress) => Color.White;
            float StripWidth1(float progress) => 40f * overallScale * sineWidthMult * Easings.easeOutCirc(colAlpha); //50

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth1, -Main.screenPosition, includeBacksides: true);

            #region shaderInfo
            String GradLocation = "VFXPlus/Assets/Gradients/";

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/ThinLineGlowClear").Value);
            myEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>(GradLocation + lpci.textureLocation).Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["satPower"].SetValue(0.25f); //higher power -> less affected by background  |0.15f

            myEffect.Parameters["sampleTexture1"].SetValue(CommonTextures.ThinGlowLine.Value);
            myEffect.Parameters["sampleTexture2"].SetValue(CommonTextures.spark_06.Value);
            myEffect.Parameters["sampleTexture3"].SetValue(CommonTextures.Extra_196_Black.Value);
            myEffect.Parameters["sampleTexture4"].SetValue(CommonTextures.Trail5Loop.Value); //smokeTrail4_512


            myEffect.Parameters["grad1Speed"].SetValue(lpci.grad1Speed);
            myEffect.Parameters["grad2Speed"].SetValue(lpci.grad2Speed);
            myEffect.Parameters["grad3Speed"].SetValue(lpci.grad3Speed);
            myEffect.Parameters["grad4Speed"].SetValue(lpci.grad4Speed);

            myEffect.Parameters["tex1Mult"].SetValue(2f * easedAlpha); //2f
            myEffect.Parameters["tex2Mult"].SetValue(1f * easedAlpha); //1.5
            myEffect.Parameters["tex3Mult"].SetValue(1.15f * easedAlpha);
            myEffect.Parameters["tex4Mult"].SetValue(2.5f * easedAlpha); //1.5
            myEffect.Parameters["totalMult"].SetValue(1f);


            //We want the number of repititions to be relative to the number of points
            float repValue = dist / 500;
            myEffect.Parameters["gradientReps"].SetValue(0.35f * repValue); //1f
            myEffect.Parameters["tex1reps"].SetValue(1f * repValue); //2.5
            myEffect.Parameters["tex2reps"].SetValue(0.3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(1f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(0.25f * repValue);

            myEffect.Parameters["uTime"].SetValue(((float)Main.timeForVisualEffects * -0.025f) + randomTimeOffset); //-0.015

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            #endregion
        }
    }

    public class RainbowSigilTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;
        }

        public override bool? CanDamage() => false;

        public float direction;

        int timer = 0;
        public override void AI()
        {
            Projectile.rotation += 0.08f;

            //Populate Points
            if (timer == 0 || true)
            {
                trailPositions.Clear();
                trailRotations.Clear();

                int pointsToCreate = 350;
                float distance = 2000f;

                for (int i = 0; i < pointsToCreate; i++)
                {
                    float distUnit = distance / (float)pointsToCreate;

                    trailPositions.Add(Projectile.Center + new Vector2(distUnit * i, 0f).RotatedBy(Projectile.rotation));
                    trailRotations.Add(Projectile.rotation);
                }

                trailPositions.Add(Projectile.Center + new Vector2(distance, 0f).RotatedBy(Projectile.rotation));
                trailRotations.Add(0f);

            }

            overallScale = Math.Clamp(MathHelper.Lerp(overallScale, 1.25f, 0.09f), 0f, 1f);


            if (timer > 5 && timer % 1 == 0)
            {
                for (int i = 0; i < 2000 * 0.9f; i += 100)
                {
                    Vector2 pos = Projectile.Center + new Vector2(i, 0f);
                    float rot = 0f;

                    Color rainbow = Color.HotPink;

                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-35, 35);
                        Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.05f) * Main.rand.NextFloat(2, 7);

                        Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<LineSpark>(), vel * 2.5f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.5f);
                        d.noLight = false;
                        d.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.97f, killEarlyTime: 10, XScale: 0.5f, YScale: 0.2f, shouldFadeColor: true);
                    }

                }
            }

            timer++;
        }

        float overallScale = 0f;
        float overallAlpha = 1f;

        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();
        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
            {
                RainbowLaser();
            });
            return false;
        }

        Effect laserEffect = null;
        public void RainbowLaser()
        {
            Vector2 startPoint = Projectile.Center;
            Vector2 endPoint = startPoint + new Vector2(2000f, 0f);

            Vector2[] pos_arr = trailPositions.ToArray();
            float[] rot_arr = trailRotations.ToArray();


            Color StripColor(float progress) => Color.White;

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            #region Params
            if (laserEffect == null)
                laserEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            laserEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            laserEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value); //ThinLineGlowClear
            laserEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/CyverGrad").Value);
            laserEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            laserEffect.Parameters["satPower"].SetValue(0.8f); //0.9f

            laserEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/ThinGlowLine").Value);
            laserEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_06").Value);
            laserEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            laserEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail5Loop").Value);

            laserEffect.Parameters["grad1Speed"].SetValue(1f); //1f
            laserEffect.Parameters["grad2Speed"].SetValue(1f); //1f
            laserEffect.Parameters["grad3Speed"].SetValue(1f); //1f
            laserEffect.Parameters["grad4Speed"].SetValue(1f); //1f

            laserEffect.Parameters["tex1Mult"].SetValue(1.25f);
            laserEffect.Parameters["tex2Mult"].SetValue(1.5f);
            laserEffect.Parameters["tex3Mult"].SetValue(1.15f);
            laserEffect.Parameters["tex4Mult"].SetValue(2.5f); //1.5
            laserEffect.Parameters["totalMult"].SetValue(1.25f);

            float dist = (endPoint - startPoint).Length();
            float repVal = dist / 2000f;
            laserEffect.Parameters["gradientReps"].SetValue(0.75f * repVal); //1f
            laserEffect.Parameters["tex1reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex2reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex3reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex4reps"].SetValue(1.15f * repVal);

            laserEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.022f); //0.006
            #endregion

            laserEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public float StripWidth(float progress)
        {
            float size = 1f * overallScale;
            float start = (float)Math.Cbrt(Utils.GetLerpValue(0f, 0.2f, progress, true));// Math.Clamp(1f * (float)Math.Pow(progress, 0.5f), 0f, 1f);
            float cap = (float)Math.Cbrt(Utils.GetLerpValue(1f, 0.95f, progress, true));
            return 140 * overallScale * start; //150

        }

    }

}
