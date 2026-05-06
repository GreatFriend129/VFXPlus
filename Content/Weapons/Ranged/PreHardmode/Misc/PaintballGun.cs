using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class PaintballGunOverride : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.PainterPaintballGun);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //Kill the current CLR held proj
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == ModContent.ProjectileType<BasicRecoilProj>())
                    if (p.owner == player.whoAmI)
                        p.active = false;
            }

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.PainterPaintballGun,
                    AnimTime: 17,
                    NormalXOffset: 16f,
                    DestXOffset: 7f,
                    YRecoilAmount: 0.07f,
                    HoldOffset: new Vector2(0f, 1f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.25f;

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/JuniorShot") with { Volume = 0.25f, PitchVariance = .3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, position);

            return true;
        }
    }
    public class PaintballOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PainterPaintball) && false;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                ballColor = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);

                Vector2 dir = projectile.velocity.SafeNormalize(Vector2.UnitX);

                int pulse = Projectile.NewProjectile(null, projectile.Center + dir * 5f, dir * 1.5f, ModContent.ProjectileType<PaintballGunPulse>(), 0, 0, Main.myPlayer);
                (Main.projectile[pulse].ModProjectile as PaintballGunPulse).color = ballColor;

                //projectile.localAI[2] = Main.rand.NextBool() ? 5 : 6;
            }

            int trailCount = 6; //6
            Vector2 trailPos = projectile.Center + projectile.velocity;
            previousRotations.Add(projectile.velocity.ToRotation()); //
            previousPositions.Add(trailPos);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 2f);

            if (timer % 5 == 0 && Main.rand.NextFloat() < 0.05f)
            {
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PaintDripDust>(), new Vector2(0f, 0f), newColor: ballColor, Scale: 1f);
            }


            timer++;

            #region VanillaAI
            Color newColor2 = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);
            newColor2.A = 200;
            projectile.localAI[0] += 1f;
            if (!(projectile.localAI[0] < 2f))
            {
                if (projectile.localAI[0] == 2f)
                {
                    ///SoundEngine.PlaySound(in SoundID.Item5, projectile.position);
                    for (int num52 = 0; num52 < 4; num52++)
                    {
                        int num53 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 76, projectile.velocity.X, projectile.velocity.Y, 0, newColor2, 1.1f);
                        Main.dust[num53].noGravity = true;
                        Main.dust[num53].velocity = projectile.Center - Main.dust[num53].position;
                        Main.dust[num53].velocity.Normalize();
                        Main.dust[num53].velocity *= -3f;
                        Main.dust[num53].velocity += projectile.velocity / 2f;
                    }
                }
                else
                {
                    projectile.frame++;
                    if (projectile.frame > 2)
                    {
                        projectile.frame = 0;
                    }
                    for (int num54 = 220; num54 < 1; num54++)
                    {
                        int num55 = Dust.NewDust(new Vector2(projectile.position.X + 4f, projectile.position.Y + 4f), projectile.width - 8, projectile.height - 8, ModContent.DustType<SnowDustCopy>(), projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 0, newColor2, 0.9f);
                        Main.dust[num55].position = projectile.Center;
                        Main.dust[num55].noGravity = true;
                        Main.dust[num55].velocity = projectile.velocity * 0.5f;
                    }
                }
            }
            #endregion
            // Apply gravity after a quarter of a second
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 15f)
            {
                projectile.ai[0] = 15f;
                projectile.velocity.Y += 0.1f;
            }

            // The projectile is rotated to face the direction of travel
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Cap downward velocity
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            return false;// base.PreAI(projectile);
        }

        Color ballColor = Color.Red;

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ballColor = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);

            Color lightColorCopy = lightColor;
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawVertexTrail(projectile, false, lightColorCopy);
            });
            DrawVertexTrail(projectile, true, lightColorCopy);
            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(Projectile projectile, bool giveUp, Color lightColor)
        {
            if (giveUp)
                return;
            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Content/Weapons/Ranged/PreHardmode/Misc/PaintGlob2").Value;

            float widthSubtract = Math.Clamp(projectile.velocity.Length() * 0.07f, 0f, 1f);

            Color lighterCol = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);
            Color darkerCol = Main.hslToRgb(projectile.ai[1] - 0.03f, 1f, 0.25f);

            Color StripColor(float progress) => Color.Lerp(lighterCol.MultiplyRGBA(lightColor), darkerCol.MultiplyRGBA(lightColor), 0f);
            float StripWidth(float progress) {
                return (4f - widthSubtract) * overallScale;
            }
            VertexStripFixed vertexStrip = new VertexStripFixed();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            Effect chainEffect = ModContent.Request<Effect>("Playground/Effects/TrailShaders/ChainShader", AssetRequestMode.ImmediateLoad).Value;


            Matrix view = Matrix.Identity;
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

            chainEffect.Parameters["WorldViewProjection"].SetValue(view * projectionMatrix);
            chainEffect.Parameters["progress"].SetValue(0f);
            chainEffect.Parameters["reps"].SetValue(1f);

            chainEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            chainEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

    }

    public class PaintballOverride2 : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PainterPaintball);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                ballColor = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);

                Vector2 dir = projectile.velocity.SafeNormalize(Vector2.UnitX);

                int pulse = Projectile.NewProjectile(null, projectile.Center + dir * 5f, dir * 1.5f, ModContent.ProjectileType<PaintballGunPulse>(), 0, 0, Main.myPlayer);
                (Main.projectile[pulse].ModProjectile as PaintballGunPulse).color = ballColor;

                //projectile.localAI[2] = Main.rand.NextBool() ? 5 : 6;
            }

            int trailCount = 14; //6 | 14
            previousRotations.Add(projectile.velocity.ToRotation()); //
            previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            float fadeInTime = Math.Clamp((timer + 13f) / 30f, 0f, 1f);
            overallScale = 1f;// Easings.easeInOutBack(fadeInTime, 0f, 2f);

            if (timer % 5 == 0 && Main.rand.NextFloat() < 0.05f)
            {
                Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PaintDripDust>(), new Vector2(0f, 0f), newColor: ballColor, Scale: 1f);
            }

            if (timer % 2 == 0 && Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SnowDustCopyQuickFade>(), Main.rand.NextVector2Circular(1f, 2f), newColor: ballColor, Scale: 1f);
                d.velocity -= projectile.velocity * 0.25f;
                d.noGravity = true;
            } 

            timer++;

            #region VanillaAI
            Color newColor2 = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);
            newColor2.A = 200;
            projectile.localAI[0] += 1f;
            if (!(projectile.localAI[0] < 2f))
            {
                if (projectile.localAI[0] == 2f)
                {
                    ///SoundEngine.PlaySound(in SoundID.Item5, projectile.position);
                    for (int num52 = 0; num52 < 4; num52++)
                    {
                        int num53 = Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<SnowDustCopy>(), projectile.velocity.X, projectile.velocity.Y, 0, newColor2, 1.1f);
                        Main.dust[num53].noGravity = true;
                        Main.dust[num53].velocity = projectile.Center - Main.dust[num53].position;
                        Main.dust[num53].velocity.Normalize();
                        Main.dust[num53].velocity *= -3f;
                        Main.dust[num53].velocity += projectile.velocity / 2f;
                    }
                }
                else
                {
                    projectile.frame++;
                    if (projectile.frame > 2)
                    {
                        projectile.frame = 0;
                    }
                    for (int num54 = 220; num54 < 1; num54++)
                    {
                        int num55 = Dust.NewDust(new Vector2(projectile.position.X + 4f, projectile.position.Y + 4f), projectile.width - 8, projectile.height - 8, ModContent.DustType<SnowDustCopy>(), projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 0, newColor2, 0.9f);
                        Main.dust[num55].position = projectile.Center;
                        Main.dust[num55].noGravity = true;
                        Main.dust[num55].velocity = projectile.velocity * 0.5f;
                    }
                }
            }
            #endregion
            // Apply gravity after a quarter of a second
            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 15f)
            {
                projectile.ai[0] = 15f;
                projectile.velocity.Y += 0.1f; //.1
            }

            // The projectile is rotated to face the direction of travel
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Cap downward velocity
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }


            previousRotations.Add(projectile.velocity.ToRotation()); //
            previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            return false;// base.PreAI(projectile);
        }

        Color ballColor = Color.Red;

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ballColor = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);

            Color lightColorCopy = lightColor;
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawVertexTrail(projectile, false, lightColorCopy);
            });
            DrawVertexTrail(projectile, true, lightColorCopy);
            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(Projectile projectile, bool giveUp, Color lightColor)
        {
            if (giveUp)
                return;

            Effect chainEffect = ModContent.Request<Effect>("Playground/Effects/TrailShaders/ChainShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            //float widthSubtract = Math.Clamp(projectile.velocity.Length() * 0.07f, 0f, 1f);

            Color lighterCol = Main.hslToRgb(projectile.ai[1], 1f, 0.5f);
            Color darkerCol = Main.hslToRgb(projectile.ai[1] - 0.03f, 1f, 0.25f);

            float StripWidth(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.85f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.85f, progress, true);
                    toReturn = Easings.easeInCubic(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.85f, 1f, progress, true);
                    toReturn = Easings.easeOutQuad(1f - LV);
                }

                if (toReturn < 0.15f)
                    toReturn = 0f;

                return toReturn * overallScale * 2.25f * 1f; //2.5

            }

            VertexStripFixed vertexStrip = new VertexStripFixed();
            
            

            chainEffect.Parameters["progress"].SetValue(0f);
            chainEffect.Parameters["reps"].SetValue(1f);
            chainEffect.Parameters["TrailTexture"].SetValue(trailTexture);

            Color col = darkerCol;
            for (int i = 0; i < 5; i++)
            {
                if (i == 4)
                    col = lighterCol;


                Color StripColor(float progress) => col.MultiplyRGBA(lightColor);
                Vector2 offset = (2f * (i * MathHelper.PiOver2).ToRotationVector2());

                if (i == 4)
                    offset = Vector2.Zero;

                vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

                Matrix transform = Matrix.CreateTranslation(new Vector3(offset, 0f));
                Matrix view = Matrix.Identity;
                Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

                chainEffect.Parameters["WorldViewProjection"].SetValue(transform * view * projectionMatrix);

                chainEffect.CurrentTechnique.Passes["DefaultPass"].Apply();

                vertexStrip.DrawTrail();


            }

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

    }

    public class PaintballGunPulse : ModProjectile
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
            float timeForPulse = 20f; //30
            float myProg = Utils.GetLerpValue(0f, timeForPulse, (float)timer, true);

            // Easings.easeOutSine(myProg);

            progress = myProg;

            if (progress > 0.99f)
            {
                progress = 1f;
                Projectile.active = false;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            timer++;
        }

        Effect myEffect = null;

        public override bool PreDraw(ref Color lightColor)
        {
            Color lightColCopy = lightColor;
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawEffect(lightColCopy, false);
            });

            DrawEffect(lightColCopy, true);

            return false;
        }

        public Color color;
        public void DrawEffect(Color lightCol, bool giveUp = false)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(1.5f * progress);// * Easings.easeOutQuint(progress)); //0.6
            myEffect.Parameters["ringPower"].SetValue(0.25f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(1f);
            myEffect.Parameters["ringWaveLength"].SetValue(41f);

            //Caustic values
            Vector3[] gradCols2 = {
                Color.Black.ToVector3(),
                color.ToVector3(),
                color.ToVector3(),
            };

            myEffect.Parameters["gradColors"].SetValue(gradCols2);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols2.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(1f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(2.0f);

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha);
            myEffect.Parameters["fadeStrength"].SetValue(1f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 50 * new Vector2(0.35f, 0.8f) * overallScale, SpriteEffects.None, 0f); //150

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }
    }

    public class PaintballGunPulseBIG : ModProjectile
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
            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);
            Color purple3 = new Color(121, 7, 179);

            if (timer == 0)
                color = purple;
            
            float timeForPulse = 30f;
            float myProg = Utils.GetLerpValue(0f, timeForPulse, (float)timer, true);

            // Easings.easeOutSine(myProg);

            progress = myProg;

            if (progress > 0.99f)
            {
                progress = 1f;
                Projectile.active = false;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

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

        public Color color;
        public void DrawEffect(bool giveUp = false)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Pixel").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/RadialPulse", AssetRequestMode.ImmediateLoad).Value;


            myEffect.Parameters["causticTexture"].SetValue(Mod.Assets.Request<Texture2D>("Assets/Noise/Trail_2").Value);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["progress"].SetValue(progress);//0.42f

            //Ring values
            myEffect.Parameters["ringRadiusStart"].SetValue(0f);
            myEffect.Parameters["ringThicknessStart"].SetValue(1.5f * progress);// * Easings.easeOutQuint(progress)); //0.6
            myEffect.Parameters["ringPower"].SetValue(0.25f);
            myEffect.Parameters["ringMult"].SetValue(2f);
            myEffect.Parameters["ringWaveSpeed"].SetValue(0.6f);
            myEffect.Parameters["ringWaveStrength"].SetValue(1f);
            myEffect.Parameters["ringWaveLength"].SetValue(41f);

            //Caustic values
            Vector3[] gradCols2 = {
                Color.Black.ToVector3(),
                color.ToVector3(),
                color.ToVector3(),
            };

            myEffect.Parameters["gradColors"].SetValue(gradCols2);
            myEffect.Parameters["numberOfColors"].SetValue(gradCols2.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(1.0f); //3.0
            myEffect.Parameters["posterizationSteps"].SetValue(2.0f);

            myEffect.Parameters["totalAlpha"].SetValue(Easings.easeOutQuint(1f - progress) * overallAlpha);
            myEffect.Parameters["fadeStrength"].SetValue(1f); //.35


            myEffect.Parameters["zoom"].SetValue(2f); //7f
            myEffect.Parameters["flowSpeed"].SetValue(3f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            float rot = (float)Main.timeForVisualEffects * 0.1f;

            Main.spriteBatch.Draw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, 150 * new Vector2(0.35f, 0.8f) * overallScale, SpriteEffects.None, 0f); //150

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

        }
    }

}
