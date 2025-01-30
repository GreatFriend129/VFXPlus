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
using System.Timers;
using VFXPlus.Common.Drawing;
using Terraria.Physics;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.Graphics;
using static tModPorter.ProgressUpdate;
using System.Runtime.InteropServices;
using static Terraria.GameContent.Bestiary.IL_BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    
    public class MagnetSphere : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.MagnetSphere);
        }

        public override void SetDefaults(Item entity)
        {
            entity.scale = 1f;
            
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/ElectricExplode") with { Volume = 0.05f, Pitch = 0.35f, PitchVariance = 0.15f, MaxInstances = -1, };
            SoundEngine.PlaySound(style, position);

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_106") with { Volume = .15f, Pitch = .5f, PitchVariance = 0.05f, MaxInstances = -1 };
            SoundEngine.PlaySound(style3, position);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Thunder_0") with { Volume = .1f, Pitch = 1f, PitchVariance = .12f, };
            SoundEngine.PlaySound(style2, position);

            return true;
        }

    }
    public class MagnetSphereOrbOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MagnetSphereBall);
        }

        float overallScale = 1f;
        float overallAlpha = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (projectile.timeLeft > 30)
            {
                float timeForPopInAnim = 30;
                float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
                overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.5f));

                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.08f), 0f, 1f);
            }
            else
            {
                float timeForPopInAnim = 30;
                float animProgress = Math.Clamp((projectile.timeLeft + -5) / timeForPopInAnim, 0f, 1f);
                overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 0f));

                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.06f), 0f, 1f);
            }
            timer++;
            return true;
        }

        float orbRot = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            orbRot += 0.015f * (projectile.velocity.X > 0 ? 1 : -1);

            float rot = orbRot;
            //float rot = timer * 0.04f * (projectile.velocity.X > 0 ? 1 : -1);

            float drawScale = projectile.scale * overallScale * 0.55f;
            float ballScale = 1f - (MathF.Sin((float)Main.timeForVisualEffects * 0.14f) * 0.06f);


            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            Texture2D Lightning = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;

            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;




            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
            {
                Main.spriteBatch.Draw(Ball, drawPos, null, Color.Black * 0.5f * overallAlpha, 0f, Ball.Size() / 2, 1.6f * drawScale, SpriteEffects.None, 0f);

                Color col = new Color(0, 210, 138) with { A = 0 } * overallAlpha;
                Color col2 = new Color(0, 240, 145) with { A = 0 } * overallAlpha;

                Main.spriteBatch.Draw(Ball, drawPos, null, Color.White with { A = 0 } * overallAlpha, 0f, Ball.Size() / 2, 0.90f * drawScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Ball, drawPos, null, col * 0.7f, rot, Ball.Size() / 2, 1.2f * drawScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Ball, drawPos, null, col2 * 0.15f, rot, Ball.Size() / 2, 2.4f * drawScale, SpriteEffects.None, 0f);
            });


            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("Dusts", () =>
            {
                myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/WaterEnergyNoise").Value);
                myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/MagnetGrad").Value);
                myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Swirl").Value);
                myEffect.Parameters["flowSpeed"].SetValue(1f);
                myEffect.Parameters["distortStrength"].SetValue(0.1f);

                myEffect.Parameters["vignetteSize"].SetValue(0.2f);
                myEffect.Parameters["vignetteBlend"].SetValue(0.8f);
                myEffect.Parameters["colorIntensity"].SetValue(1.75f * Easings.easeInCirc(overallAlpha));
                myEffect.Parameters["uTime"].SetValue(timer * 0.015f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

                Main.spriteBatch.Draw(Lightning, drawPos, null, new Color(255, 255, 255, 0), rot, Lightning.Size() / 2, 0.4f * drawScale * (ballScale + 0.4f), SpriteEffects.None, 0f);
                //Main.spriteBatch.Draw(Lightning, drawPos, null, new Color(255, 255, 255, 0), rot * -1f, Lightning.Size() / 2, 0.4f * drawScale * 0.7f * (ballScale + 0.4f), SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
                Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            });



            //ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("Dusts", () =>
            //{
                //DrawOrb(projectile, false);
            //});

            //DrawOrb(projectile, true);

            return false;

        }

        public void DrawOrb(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            float rot = timer * 0.04f;

            float drawScale = projectile.scale * overallScale * 0.55f; 
            float ballScale = 1f - ((float)Math.Sin((float)timer * 0.03f) * 0.12f);


            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            Texture2D Lightning = Mod.Assets.Request<Texture2D>("Assets/Orbs/bigCircle2").Value;


            Effect myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/WaterEnergyNoise").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/MagnetGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Swirl").Value);
            myEffect.Parameters["flowSpeed"].SetValue(1f);
            myEffect.Parameters["distortStrength"].SetValue(0.1f);
            myEffect.Parameters["uTime"].SetValue(timer * 0.015f);

            myEffect.Parameters["vignetteSize"].SetValue(0.2f);
            myEffect.Parameters["vignetteBlend"].SetValue(1f);
            myEffect.Parameters["colorIntensity"].SetValue(1.75f);

            Main.spriteBatch.Draw(Ball, drawPos, null, Color.Black * 1f, 0f, Ball.Size() / 2, 1.5f * drawScale, SpriteEffects.None, 0f);

            //Main.spriteBatch.End();
            //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            Color col = new Color(0, 210, 138) with { A = 0 };
            Color col2 = new Color(0, 240, 145) with { A = 0 };

            Main.spriteBatch.Draw(Ball, drawPos, null, Color.White with { A = 0 }, 0f, Ball.Size() / 2, 1.05f * drawScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Ball, drawPos, null, col * 0.7f, rot, Ball.Size() / 2, 1.35f * drawScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Ball, drawPos, null, col2 * 0.3f, rot, Ball.Size() / 2, 2.4f * drawScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(Lightning, drawPos, null, new Color(255, 255, 255, 0), rot, Lightning.Size() / 2, 0.4f * drawScale * (ballScale + 0.4f), SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Lightning, drawPos, null, new Color(255, 255, 255, 0), rot * -1f, Lightning.Size() / 2, 0.4f * drawScale * 0.7f * (ballScale + 0.4f), SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    public class MagnetSphereLightningOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MagnetSphereBolt);
        }

        Vector2 startPoint = Vector2.Zero;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                startPoint = projectile.Center;

            timer++;
            return false;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int a = Projectile.NewProjectile(null, startPoint + Main.rand.NextVector2Circular(15f, 15f), Vector2.Zero, ModContent.ProjectileType<MagnetSphereLightningVFX>(), 0, 0, projectile.owner);

            (Main.projectile[a].ModProjectile as MagnetSphereLightningVFX).startPoint = startPoint;
            (Main.projectile[a].ModProjectile as MagnetSphereLightningVFX).endPoint = projectile.Center;

            return base.PreKill(projectile, timeLeft);
        }

    }


    public class MagnetSphereLightningVFX : ModProjectile
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

        public override bool? CanDamage() => false;


        public Vector2 startPoint;
        public Vector2 endPoint;
        public float direction;

        int timer = 0;
        public override void AI()
        {
            if (timer == 0)
            {
                //startPoint = Projectile.Center;
                //endPoint = Projectile.Center + new Vector2(100f, 0f);//.RotatedByRandom(0f) * Main.rand.NextFloat(0.9f, 1.1f);

                direction = (endPoint - Projectile.Center).ToRotation();

                Dust p2 = Dust.NewDustPerfect(endPoint, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Aquamarine, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.3f);
                p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.02f);

                Dust p3 = Dust.NewDustPerfect(endPoint, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Aquamarine, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.15f);
                p3.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.03f);

                //Sound

                int soundVar = Main.rand.Next(0, 3);
                SoundStyle style5 = new SoundStyle("Terraria/Sounds/Custom/dd2_lightning_bug_zap_" + soundVar) with { Volume = 0.45f, Pitch = 0.51f, PitchVariance = 0.15f, MaxInstances = -1 };
                SoundEngine.PlaySound(style5, endPoint);

                SoundStyle stylea = new SoundStyle("AerovelenceMod/Sounds/Effects/lightning_flash_01") with { Volume = 0.12f, Pitch = 1f, PitchVariance = 0.2f, MaxInstances = -1 };
                SoundEngine.PlaySound(stylea, endPoint);


            }

            Projectile.spriteDirection = direction.ToRotationVector2().X > 0 ? 1 : -1;

            if (timer == 0)
            {
                float length = (endPoint - startPoint).Length();
                int relativeMidpoints = 1 + (int)(2 * (length / 400f)); //400

                if (length < 70)
                    relativeMidpoints = 0;

                int numberOfMidpoints = relativeMidpoints;// 3 + (Main.rand.NextBool() ? 1 : 0); //Make relative to distance between start and end later

                //Add the start point
                trailPositions.Add(startPoint);

                //Add the midpoints
                float distance = startPoint.Distance(endPoint);
                for (int i = 1; i <= numberOfMidpoints; i++)
                {
                    float progress = (float)i / (float)numberOfMidpoints;

                    float distanceBetweenMidpoints = distance * (1f / (numberOfMidpoints + 1f));

                    Vector2 newMidPointBasePosition = startPoint + direction.ToRotationVector2() * (distanceBetweenMidpoints * i);

                    //Offset the position vertically by a random amount (rotated by direction)
                    float verticalOffset = Main.rand.NextFloat(-30f, 30f);
                    float horizontalOffset = Main.rand.NextFloat(-15f, 15f); //-10 10

                    newMidPointBasePosition += new Vector2(horizontalOffset, verticalOffset).RotatedBy(direction);

                    trailPositions.Add(newMidPointBasePosition);
                }

                //Add the end point
                trailPositions.Add(endPoint);

                //Calculate point rotations
                for (int i = 0; i < trailPositions.Count - 1; i++)
                {
                    Vector2 thisPoint = trailPositions[i];
                    Vector2 nextPoint = trailPositions[i + 1];

                    float rot = (nextPoint - thisPoint).ToRotation();
                    trailRotations.Add(rot);
                }

                //Add final rotation
                trailRotations.Add(trailRotations[trailRotations.Count - 1]);

                originalPoints = trailPositions;


                //Do dust
                //Dust from orb
                for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++) //4 //2,2
                {
                    Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) * 1f;
                    vel += trailRotations[0].ToRotationVector2() * 9f;

                    Dust p = Dust.NewDustPerfect(startPoint, ModContent.DustType<PixelatedLineSpark>(), vel, newColor: new Color(0, 190, 138), Scale: Main.rand.NextFloat(0.9f, 1.15f) * 0.4f);

                    p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.9f, preShrinkPower: 0.99f, postShrinkPower: 0.89f, timeToStartShrink: 8 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                        0.75f, 0.5f, shouldFadeColor: false);
                }

                //End dust
                for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++) //4 //2,2
                {
                    Vector2 vel = Main.rand.NextVector2Circular(7f, 7f) * 1f;

                    Dust p = Dust.NewDustPerfect(endPoint, ModContent.DustType<PixelatedLineSpark>(), vel,
                        newColor: new Color(0, 190, 138), Scale: Main.rand.NextFloat(0.9f, 1.15f) * 0.4f);

                    p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.89f, preShrinkPower: 0.99f, postShrinkPower: 0.89f, timeToStartShrink: 4 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                        0.75f, 0.5f, shouldFadeColor: false);

                }
            }

            if (timer % 1 == 0 && timer != 0)
            {
                for (int i = 1; i < trailRotations.Count - 1; i++)
                {
                    trailPositions[i] = originalPoints[i] + Main.rand.NextVector2Circular(10f, 10f);

                }
            }

            if (timer < 9)
                Lighting.AddLight(endPoint, Color.Aquamarine.ToVector3() * 0.9f);

            if (timer == 15) //10
                Projectile.active = false;

            timer++;
        }

        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();

        public List<Vector2> originalPoints = new List<Vector2>();

        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions.Count == 0)
                return false;

            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;


            Vector2 startPos = trailPositions[0] - Main.screenPosition;
            Vector2 endPos = trailPositions[trailPositions.Count - 1] - Main.screenPosition;
            float startRot = trailRotations[0];
            float endRot = trailRotations[trailPositions.Count - 1];


            float vfxTime = (float)Main.timeForVisualEffects;
            float elboost = Easings.easeInOutBack(Utils.GetLerpValue(0, 7, timer, true), 0f, 5f) * Utils.GetLerpValue(15, 10, timer, true); //8
            elboost = Math.Clamp(elboost, 0.2f, 10f);

            Vector2 vec2Scale = new Vector2(1f, 0.85f) * 1.5f * elboost;


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Main.EntitySpriteDraw(flare, startPos, null, Color.Aquamarine with { A = 0 }, startRot, flare.Size() / 2f, vec2Scale * 0.5f, SpriteEffects.None);
                Main.EntitySpriteDraw(flare, startPos, null, Color.White with { A = 0 }, startRot, flare.Size() / 2f, vec2Scale * 0.25f, SpriteEffects.None);

                Main.EntitySpriteDraw(flare, endPos, null, Color.Aquamarine with { A = 0 }, endRot + (vfxTime * 0.2f * Projectile.spriteDirection), flare.Size() / 2f, vec2Scale * 0.5f, SpriteEffects.None);
                Main.EntitySpriteDraw(flare, endPos, null, Color.White with { A = 0 }, endRot, flare.Size() / 2f, vec2Scale * 0.25f, SpriteEffects.None);

                DrawTrail();

            });

            return false;
        }

        Effect myEffect = null;
        public void DrawTrail()
        {
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value; //|spark_06 | Extra_196_Black
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value;

            Vector2[] pos_arr = trailPositions.ToArray();
            float[] rot_arr = trailRotations.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.15f;
            float elboost = Easings.easeInOutBack(Utils.GetLerpValue(0, 7, timer, true), 0f, 8f) * Utils.GetLerpValue(15, 10, timer, true);
            elboost = Math.Clamp(elboost, 0.2f, 10f);

            Color StripColor(float progress) => Color.White * 1f;
            float StripWidth(float progress) => 45f * 1f * sineWidthMult * 0.5f * elboost;
            float StripWidth2(float progress) => 80f * 1f * sineWidthMult * 0.5f * elboost; //65
            //^ Doing Easings.easeOutQuad(progress) * Easings.easeInQuad(progress) gives a really nice zigzag patter (or do 1f - EaseIn)

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            float dist = (endPoint - startPoint).Length();
            float repValue = dist / 400f;
            myEffect.Parameters["reps"].SetValue(repValue * 0.8f);
            //myEffect.Parameters["reps"].SetValue(0.8f);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.02f); //timer * 0.02

            //UnderLayer
            Color col = new Color(0, 190, 138) with { A = 0 };


            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);

            //myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.3f);
            //myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //vertexStrip2.DrawTrail();

            myEffect.Parameters["ColorOne"].SetValue(Color.Aquamarine.ToVector3() * 1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.Aquamarine.ToVector3() * 3f);
            myEffect.Parameters["glowThreshold"].SetValue(0.8f);
            myEffect.Parameters["glowIntensity"].SetValue(1.27f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }

    }

    public class JuttedLightning : ModProjectile
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

        public override bool? CanDamage() => false;


        public Vector2 startPoint;
        public Vector2 endPoint;
        public float direction;

        int timer = 0;
        public override void AI()
        {
            if (timer == 0)
            {
                startPoint = Projectile.Center;
                endPoint = Projectile.Center + new Vector2(1600f, 0f).RotatedByRandom(0f) * Main.rand.NextFloat(0.9f, 1.1f);

                direction = (endPoint - Projectile.Center).ToRotation();
            }

            int numberOfMidpoints = 9; //Make relative to distance between start and end later

            if (timer % 1 == 0)
            {
                trailPositions.Clear();
                trailRotations.Clear();

                //Add the start point
                trailPositions.Add(startPoint);

                //Add the midpoints
                float distance = startPoint.Distance(endPoint);
                for (int i = 1; i <= numberOfMidpoints; i++)
                {
                    float progress = (float)i / (float)numberOfMidpoints;

                    float distanceBetweenMidpoints = distance * (1f / (numberOfMidpoints + 1f));

                    Vector2 newMidPointBasePosition = startPoint + direction.ToRotationVector2() * (distanceBetweenMidpoints * i);
                    newMidPointBasePosition += new Vector2(Main.rand.NextFloat(-5f, 5f), 0f).RotatedBy(direction);

                    //Offset the position vertically by a random amount (rotated by direction)

                    //float prog = (MathF.Sin(MathHelper.Pi * progress) / 1f) + 0.5f;

                    float overallOffset = 40f;// * prog; 

                    float verticalOffset = 0f;
                    if (firstLoop)
                    {
                        verticalOffset = Main.rand.NextFloat(-overallOffset, overallOffset);
                        offsetValues.Add(verticalOffset);
                    }
                    else
                    {
                        verticalOffset = offsetValues[i - 1] + Main.rand.NextFloat(3f, 12f) * 0.6f * (Main.rand.NextBool() ? 1f : -1f);

                        if (verticalOffset > overallOffset)
                            verticalOffset = overallOffset;
                        if (verticalOffset < -overallOffset)
                            verticalOffset = -overallOffset;

                        offsetValues[i - 1] = verticalOffset;
                    }


                    newMidPointBasePosition += new Vector2(0f, verticalOffset).RotatedBy(direction);

                    trailPositions.Add(newMidPointBasePosition);
                }

                //Add the end point
                trailPositions.Add(endPoint);

                //Calculate point rotations
                for (int i = 0; i < trailPositions.Count - 1; i++)
                {
                    Vector2 thisPoint = trailPositions[i];
                    Vector2 nextPoint = trailPositions[i + 1];

                    float rot = (nextPoint - thisPoint).ToRotation();
                    trailRotations.Add(rot);
                }

                //Add final rotation
                trailRotations.Add(direction);

                firstLoop = false;
            }

            timer++;
        }

        float overallScale = 0f;
        float overallAlpha = 1f;
        bool firstLoop = true;

        public List<float> trailRotations = new List<float>();
        public List<Vector2> trailPositions = new List<Vector2>();
        public List<float> offsetValues = new List<float>();
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions.Count != trailRotations.Count)
                Main.NewText("BIG MISTAKE");

            Texture2D Debug = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //Main.EntitySpriteDraw(Debug, startPoint - Main.screenPosition, null, Color.White, 0f, Debug.Size() / 2f, 0.7f, SpriteEffects.None);
            //Main.EntitySpriteDraw(Debug, endPoint - Main.screenPosition, null, Color.Red, 3.14f / 2f, Debug.Size() / 2f, new Vector2(1f, 0.25f), SpriteEffects.None);

            for (int i = 0; i < trailPositions.Count; i++)
            {
                Vector2 pos = trailPositions[i];
                float rot = trailRotations[i];

                //Main.EntitySpriteDraw(Debug, pos - Main.screenPosition, null, Color.White, rot, Debug.Size() / 2f, 0.7f, SpriteEffects.None);
            }
            DrawTrail();

            return false;
        }

        Effect myEffect = null;
        public void DrawTrail()
        {
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/s06sBloom").Value; //|spark_06 | Extra_196_Black
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value;

            Vector2[] pos_arr = trailPositions.ToArray();
            float[] rot_arr = trailRotations.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.0f;

            Color StripColor(float progress) => Color.White;
            float StripWidth(float progress) => 70f * 1f * sineWidthMult * (progress > 0.5f ? 1f - progress : progress) ;// (1f - Easings.easeInSine(progress));
            float StripWidth2(float progress) => 130f * 1f * sineWidthMult * (progress > 0.5f ? 1f - progress : progress);
            //^ Doing Easings.easeOutQuad(progress) * Easings.easeInQuad(progress) gives a really nice zigzag patter (or do 1f - EaseIn)

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //float dist = (node.head - node.tail).Length();
            //float repValue = dist / 700f;
            //myEffect.Parameters["reps"].SetValue(repValue);
            myEffect.Parameters["reps"].SetValue(2f);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.02f); //timer * 0.02

            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["ColorOne"].SetValue(Color.DeepSkyBlue.ToVector3() * 3f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.DeepSkyBlue.ToVector3() * 6f);
            myEffect.Parameters["glowThreshold"].SetValue(0.4f);
            myEffect.Parameters["glowIntensity"].SetValue(2.2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }

    }

}
