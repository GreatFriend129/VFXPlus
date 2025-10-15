using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using VFXPlus.Content.Dusts;
using VFXPlus.Common.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using Terraria.DataStructures;
using static Terraria.NPC;
using static tModPorter.ProgressUpdate;
using ReLogic.Content;
using Terraria.Physics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;
using static System.Formats.Asn1.AsnWriter;
using VFXPlus.Content.Projectiles;
using Terraria.Graphics;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Net;

namespace VFXPlus.Content.VFXTest.Aero
{
    public class CavernsYoyoVFX : ModProjectile
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

        public override bool? CanCutTiles() => false;
        public override bool? CanDamage() => false;

        public Vector2 startPoint;
        public Vector2 endPoint;
        public float direction;

        int timer = 0;
        public override void AI()
        {
            if (timer == 0)
            {
                direction = (endPoint - Projectile.Center).ToRotation();

                Dust p2 = Dust.NewDustPerfect(endPoint, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.3f);
                p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.02f);

                Dust p3 = Dust.NewDustPerfect(endPoint, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.15f);
                p3.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.03f);

                //Sound
                int soundVar = Main.rand.Next(0, 3);
                SoundStyle style5 = new SoundStyle("Terraria/Sounds/Custom/dd2_lightning_bug_zap_" + soundVar) with { Volume = 0.35f, Pitch = 0.51f, PitchVariance = 0.15f, MaxInstances = -1 };
                SoundEngine.PlaySound(style5, endPoint);

                SoundStyle stylea = new SoundStyle("AerovelenceMod/Sounds/Effects/lightning_flash_01") with { Volume = 0.08f, Pitch = 1f, PitchVariance = 0.2f, MaxInstances = -1 };
                SoundEngine.PlaySound(stylea, endPoint);
            }

            Projectile.spriteDirection = direction.ToRotationVector2().X > 0 ? 1 : -1;

            if (timer == 0)
            {
                float length = (endPoint - startPoint).Length();
                int relativeMidpoints = 1 + (int)(2 * (length / 400f));

                if (length < 100)
                    relativeMidpoints = 0;

                int numberOfMidpoints = relativeMidpoints;// 3 + (Main.rand.NextBool() ? 1 : 0); //Make relative to distance between start and end later

                //Add the start point
                trailPositions.Add(startPoint);

                //Add the midpoints
                float distance = startPoint.Distance(endPoint);
                for (int i = 1; i <= numberOfMidpoints; i++)
                {
                    float distanceBetweenMidpoints = distance * (1f / (numberOfMidpoints + 1f));

                    Vector2 newMidPointBasePosition = startPoint + direction.ToRotationVector2() * (distanceBetweenMidpoints * i);

                    //Offset the position vertically by a random amount (rotated by direction)
                    float verticalOffset = Main.rand.NextFloat(-25f, 25f);
                    float horizontalOffset = Main.rand.NextFloat(-15f, 15f);

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

                    Dust p = Dust.NewDustPerfect(startPoint, ModContent.DustType<PixelatedLineSpark>(), vel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.9f, 1.15f) * 0.4f);

                    p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.9f, preShrinkPower: 0.99f, postShrinkPower: 0.89f, timeToStartShrink: 8 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                        0.75f, 0.5f, shouldFadeColor: false);
                }

                //End dust
                for (int i = 0; i < 6 + Main.rand.Next(0, 4); i++) //4 //2,2
                {
                    Vector2 vel = Main.rand.NextVector2Circular(7f, 7f) * 1f;

                    Dust p = Dust.NewDustPerfect(endPoint, ModContent.DustType<PixelatedLineSpark>(), vel,
                        newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.9f, 1.15f) * 0.4f);

                    p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.89f, preShrinkPower: 0.99f, postShrinkPower: 0.89f, timeToStartShrink: 4 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                        0.75f, 0.5f, shouldFadeColor: false);

                }
            }

            if (timer % 5 == 0 && timer != 0)
            {
                for (int i = 1; i < trailRotations.Count - 1; i++)
                {
                    trailPositions[i] = originalPoints[i] + Main.rand.NextVector2CircularEdge(10f, 10f);

                }
            }

            if (timer < 9)
                Lighting.AddLight(endPoint, Color.DeepSkyBlue.ToVector3() * 0.9f);

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


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("Dusts", () =>
            {
                Color betweenBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.7f);

                Main.EntitySpriteDraw(flare, startPos, null, betweenBlue with { A = 0 }, startRot, flare.Size() / 2f, vec2Scale * 0.5f, SpriteEffects.None);
                Main.EntitySpriteDraw(flare, startPos, null, Color.White with { A = 0 }, startRot, flare.Size() / 2f, vec2Scale * 0.25f, SpriteEffects.None);

                Main.EntitySpriteDraw(flare, endPos, null, betweenBlue with { A = 0 }, endRot + (vfxTime * 0.2f * Projectile.spriteDirection), flare.Size() / 2f, vec2Scale * 0.5f, SpriteEffects.None);
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

            Color betweenBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.75f);

            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/LavaTrailBloom").Value; //|spark_06 | Extra_196_Black
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value;

            Vector2[] pos_arr = trailPositions.ToArray();
            float[] rot_arr = trailRotations.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.15f;
            float elboost = Easings.easeInOutBack(Utils.GetLerpValue(0, 7, timer, true), 0f, 8f) * Utils.GetLerpValue(15, 10, timer, true);
            elboost = Math.Clamp(elboost, 0.2f, 10f);

            Color StripColor(float progress) => Color.White * 1f;
            float StripWidth(float progress) => 35f * 1f * sineWidthMult * 0.5f * elboost;
            float StripWidth2(float progress) => 100f * 1f * sineWidthMult * 0.5f * elboost;

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            float dist = (endPoint - startPoint).Length();
            float repValue = dist / 200f;
            myEffect.Parameters["reps"].SetValue(repValue * 0.8f);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.05f);

            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(betweenBlue.ToVector3() * 1.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.SkyBlue.ToVector3() * 2f);
            myEffect.Parameters["glowThreshold"].SetValue(0.8f);
            myEffect.Parameters["glowIntensity"].SetValue(1.27f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }
    }
}