using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Dusts;
using VFXPlus.Common;
using Terraria.Utilities.Terraria.Utilities;
using Terraria.Utilities;
using System.Runtime.InteropServices;

namespace VFXPlus.Content.FeatheredFoe
{
    public class WindBarrier : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 4400; //180
            Projectile.extraUpdates = 25;
        }

        public Vector2 center = Vector2.Zero;

        float startAngle = 0f;
        float endAngle = 0f;
        int timer = 0;
        float width = 1f;

        public List<Vector2> previousPositions;
        public List<float> previousRotations;

        Vector2[] positions;
        float[] rotations;
        public override void AI()
        {
            if (timer == 0)
            {
                previousPositions = new List<Vector2>();
                previousRotations = new List<float>();

                Projectile.timeLeft = 13400;
            }

            if (timer < 1400)
            {
                previousPositions.Add(Projectile.Center);
                previousRotations.Add(new Vector2(3f + (timer * 0.0015f), 0f).RotatedBy((timer * 0.012f) + startAngle).ToRotation());

                Projectile.velocity = new Vector2(3f + (timer * 0.0015f), 0f).RotatedBy((timer * 0.012f) + startAngle);

                positions = previousPositions.ToArray();
                rotations = previousRotations.ToArray();
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.extraUpdates = 20;
            }

            int mod = 10;
            if (timer % mod == 0)
            {
                int maxAdd = Projectile.timeLeft < 300 ? 50 : 100;
                for (int i = Main.rand.Next(0, 15); i < previousPositions.Count; i += Main.rand.Next(5, maxAdd))
                {
                    int randChance = 17;
                    if (Main.rand.NextBool(randChance))
                    {
                        Vector2 sideOffset = (previousRotations[i] + MathHelper.PiOver2).ToRotationVector2() * (Projectile.timeLeft < 300 ? Main.rand.NextFloat(-5, 5.01f) : Main.rand.NextFloat(-10, 10.1f));

                        SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 5f, 0.92f);

                        float dustScale = Main.rand.NextFloat(1f, 1.5f);

                        Dust wind = Dust.NewDustPerfect(previousPositions[i] + sideOffset, 176, previousRotations[i].ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.75f * 1f, newColor: Color.DodgerBlue with { A = 0 } * 1f, Scale: dustScale); //dust176
                        wind.noGravity = true;
                    }

                    int randChance2 = 20;
                    if (Main.rand.NextBool(randChance2))
                    {
                        Color col = Color.LightSkyBlue;
                        Vector2 sideOffset = (previousRotations[i] + MathHelper.PiOver2).ToRotationVector2() * (Projectile.timeLeft < 300 ? Main.rand.NextFloat(-5, 5.01f) : Main.rand.NextFloat(-10, 10.1f));

                        SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 5f, 0.92f);

                        Dust star = Dust.NewDustPerfect(previousPositions[i] + sideOffset, ModContent.DustType<SmallSmoke>(),
                        previousRotations[i].ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.75f, newColor: col, Scale: 2f);
                        star.customData = ssb;
                    }
                }


            }

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexFade", AssetRequestMode.ImmediateLoad).Value;

            VertexStrip strip = new VertexStrip();
            strip.PrepareStripWithProceduralPadding(positions, rotations, StripColor, StripWidth, -Main.screenPosition, true);
            ShaderParams();

            Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.75f);



            Texture2D glow = Mod.Assets.Request<Texture2D>("Assets/Slash/twirl_03").Value;
            float rot = (float)Main.timeForVisualEffects * 0.15f;
            float rot2 = (float)Main.timeForVisualEffects * 0.23f;


            Texture2D glow2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/circle_02black").Value;
            Main.EntitySpriteDraw(glow2, center - Main.screenPosition, null, between with { A = 0 } * 0.04f, rot, glow2.Size() / 2f, 2.2f, 0);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Main.EntitySpriteDraw(glow, center - Main.screenPosition, null, Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f) with { A = 0 } * 0.03f, rot, glow.Size() / 2f, 1.75f, 0);
                Main.EntitySpriteDraw(glow, center - Main.screenPosition, null, Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.5f) with { A = 0 } * 0.03f, rot2, glow.Size() / 2f, 1.7f, 0);


                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                strip.DrawTrail();

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

                DrawParticles(75);

            });


            return false;
        }

        public void DrawParticles(int count = 50)
        {
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/AnotherLineGlow").Value;
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 1.5f; //2f

            float minRange = 290f; //| 240 920 for full screen | 
            float maxRange = 440f; //444
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.7f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.66f, 0.3f) * 1f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.65f, 0.65f);
                    rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f); //0.8f, 4f
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance;

                Color col = Color.LightSkyBlue * 1f;


                if (texture.Equals(windTexture))
                    col = Color.White * 0.35f;
                else
                    col = Color.DeepSkyBlue * 0.85f;

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, col with { A = 0 }, randomRot + rotation, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);
            }
        }

        public void ShaderParams()
        {
            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/WackBeam").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.Silver.ToVector3() * 1.5f); //1f
            myEffect.Parameters["satPower"].SetValue(0f); //0.25

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail7").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/FlameTrail").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);


            Color c1 = Color.DimGray * 0.5f;
            Color c2 = Color.LightSlateGray * 0.5f;
            Color c3 = Color.DimGray * 0.5f;
            Color c4 = Color.DimGray * 0.5f;

            myEffect.Parameters["Color1"].SetValue(c1.ToVector4());
            myEffect.Parameters["Color2"].SetValue(c2.ToVector4());
            myEffect.Parameters["Color3"].SetValue(c3.ToVector4());
            myEffect.Parameters["Color4"].SetValue(c4.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(0f);
            myEffect.Parameters["Color2Mult"].SetValue(0.5f); //0.5 ^-vv
            myEffect.Parameters["Color3Mult"].SetValue(0f);
            myEffect.Parameters["Color4Mult"].SetValue(0.25f);
            myEffect.Parameters["totalMult"].SetValue(0.75f);

            myEffect.Parameters["tex1reps"].SetValue(4f);
            myEffect.Parameters["tex2reps"].SetValue(4f);
            myEffect.Parameters["tex3reps"].SetValue(4f);
            myEffect.Parameters["tex4reps"].SetValue(4f);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.021f);

        }
        public Color StripColor(float progress)
        {
            Color color = Color.White * 0f;
            color.A = 0;
            return color;
        }
        public float StripWidth(float progress)
        {
            float size = Utils.GetLerpValue(13400f, 12800f, Projectile.timeLeft, true) * Utils.GetLerpValue(0f, 200f, Projectile.timeLeft, true);
            float start = Math.Clamp(1.5f * (float)Math.Pow(progress, 0.5f), 0f, 1f);
            float cap = (float)Math.Cbrt(Utils.GetLerpValue(1f, 0.85f, progress, true));

            //!Very potential source of lag! 
            float sineScale = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.14f) * 0.08f; 

            return start * size * 125f * cap * sineScale;// * (1.1f + (float)Math.Cos(timer) * (0.08f - progress * 0.06f));

        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }
    }

    public class UmbrellaTelegraph : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 224400; //180
            Projectile.extraUpdates = 0;
        }


        public bool shouldFadeOut = false;
        public int targetPlayer = -1;

        int timer = 0;
        float overallAlpha = 1f;
        float initialProg = 0f;
        public override void AI()
        {
            //Do a big burst of wind particles on startup
            if (timer == 0)
            {
                int dustPerSide = 55 * 2;

                //The problem is that if the dust spawns far enough off-screen it just disappears
                int dir = 1;
                for (int i = 0; i < dustPerSide; i++)
                {
                    Vector2 windDustSpawnPosition = Projectile.Center + (new Vector2(-650f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloatDirection() * 900f) * 1f);
                    Vector2 windDustVelocity = new Vector2(1f, dir * 0.15f) * dir * Main.rand.NextFloat(0.1f, 1.8f) * 45f;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(1f, 2f));
                    wind.noGravity = true;
                }
                dir = -1;
                for (int i = 0; i < dustPerSide; i++)
                {
                    Vector2 windDustSpawnPosition = Projectile.Center + (new Vector2((-650f + Main.rand.NextFloat(-400f, 0f)) * dir, Main.rand.NextFloatDirection() * 900f) * 1f);
                    Vector2 windDustVelocity = new Vector2(1f, dir * 0.15f) * dir * Main.rand.NextFloat(0.1f, 1.8f) * 45f;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(1f, 2f));
                    wind.noGravity = true;
                }

                //Sound
                //SoundStyle style = new SoundStyle("Terraria/Sounds/Zombie_67") with { Volume = 0.05f, Pitch = 0.2f, MaxInstances = -1 }; 
                //SoundEngine.PlaySound(style, Projectile.Center);

                //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Research_0") with { Volume = 0.05f, Pitch = -0.15f, MaxInstances = -1 }; 
                //SoundEngine.PlaySound(style2, Projectile.Center);

                //SoundStyle style3 = new SoundStyle("AerovelenceMod/Sounds/Effects/AnnihilatorCharge") with { Volume = 0.15f, Pitch = 1f, MaxInstances = -1 };
                //SoundEngine.PlaySound(style3, Projectile.Center);

            }

            #region Position Handling

            if (targetPlayer == -1)
                targetPlayer = Main.myPlayer;

            //This makes it so that the player can only be at most 350 away from the Projectile vertically
            float distToPlayer = Main.player[targetPlayer].Center.Y - Projectile.Center.Y;
            if (distToPlayer > 0)
            {
                float val = distToPlayer - 350f;

                if (val > 0)
                    Projectile.Center = new Vector2(Projectile.Center.X, Projectile.Center.Y + val);

            }
            else
            {
                float val = distToPlayer + 350f;

                if (val < 0)
                    Projectile.Center = new Vector2(Projectile.Center.X, Projectile.Center.Y + val);
            }
            #endregion


            int timeForInitialAnim = 30;
            float initProg = Math.Clamp((float)timer / (float)timeForInitialAnim, 0f, 1f);
            initialProg = initProg;

            //Dust that comes closer and closer as the telegraph shortens
            if (timer < timeForInitialAnim / 2f)
            {
                for (int i = 0; i < 10; ++i) //5
                {
                    int side = Main.rand.NextBool() ? 1 : -1;

                    float spawnY = Main.rand.NextFloat(-500f, 500f);
                    float spawnX = -200 + -1000f * (1f - Easings.easeOutQuart(initProg));

                    Vector2 spawnPos = Projectile.Center + new Vector2(spawnX * side, spawnY);

                    Vector2 velocity = new Vector2(5 + (20f * (1f - initProg)), 0f) * side;

                    Dust p = Dust.NewDustPerfect(spawnPos, ModContent.DustType<WindLine>(), velocity * 1f, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.25f, 0.5f));

                    WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, 1f, 1f, true);
                    wlb.randomVelRotatePower = 0.2f;

                    p.customData = wlb;
                }
            }

            //Fade out started by FF NPC
            if (shouldFadeOut)
            {
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.08f), 0f, 1f);

                if (overallAlpha == 0f)
                    Projectile.active = false;
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D glow = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_06").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06
            Texture2D glowThick = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06

            float safeZoneWidth = 95f;


            int leftX = (int)MathHelper.Lerp(-1000, -safeZoneWidth, Easings.easeOutQuart(initialProg));// -100;
            int rightX = (int)MathHelper.Lerp(1000, safeZoneWidth, Easings.easeOutQuart(initialProg));//100;


            float colorIntensity = (0.75f + (MathF.Sin((float)Main.timeForVisualEffects * 0.2f) * 0.15f)) * Easings.easeInQuad(initialProg) * overallAlpha;
            int sineHeightAddition = (int)(Math.Sin(Main.timeForVisualEffects * 0.05f) * 30f);
            int sineWidthAddition = (int)(Math.Sin(Main.timeForVisualEffects * 0.08f) * 20f);


            int wallWidth = (int)MathHelper.Lerp(2300f, 120f, Easings.easeOutSine(initialProg)) + sineWidthAddition;
            int wallHeight = 2000 + sineHeightAddition;
            Rectangle glowFrame = new Rectangle(0, 0, glow.Width, glow.Height / 2);
            Rectangle glowThickFrame = new Rectangle(0, 0, glowThick.Width, glowThick.Height / 2);


            //Left Wall---------------------------------------
            Rectangle glowTarget = new Rectangle((int)drawPos.X + leftX - wallWidth, (int)drawPos.Y, wallHeight, wallWidth);

            Main.spriteBatch.Draw(glowThick, glowTarget, glowThickFrame, Color.DeepSkyBlue with { A = 0 } * 0.25f * colorIntensity, MathHelper.PiOver2, glowThick.Size() / 2f, SpriteEffects.FlipVertically, 0);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.DeepSkyBlue with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);

            //White
            glowTarget = new Rectangle((int)drawPos.X + leftX - (wallWidth / 3) - 2, (int)drawPos.Y, wallHeight, wallWidth / 3);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.White with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);


            //Right Wall--------------------------------------------------
            glowTarget = new Rectangle((int)drawPos.X + rightX, (int)drawPos.Y, wallHeight, wallWidth);

            Main.spriteBatch.Draw(glowThick, glowTarget, glowThickFrame, Color.DeepSkyBlue with { A = 0 } * 0.25f * colorIntensity, MathHelper.PiOver2, glowThick.Size() / 2f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.DodgerBlue with { A = 0 } * 1.5f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.None, 0);

            //White
            glowTarget = new Rectangle((int)drawPos.X + rightX + 2, (int)drawPos.Y, wallHeight, wallWidth / 3);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.White with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.None, 0);

            return false;
        }

    }

    public class Stormwall : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 224400;
        }



        int timer = 0;
        float overallWidth = 1f;
        float overallAlpha = 0f;

        public bool shouldFade = false;
        public int targetPlayer = -1;
        public override void AI()
        {
            if (targetPlayer == -1)
                targetPlayer = Main.myPlayer;

            Projectile.Center = new Vector2(Projectile.Center.X, Main.player[targetPlayer].Center.Y - (Main.screenHeight / 2f));

            //Projectile.Center = new Vector2(Main.player[targetPlayer].Center.X, Main.player[targetPlayer].Center.Y - (Main.screenHeight / 2f));

            //This makes it so that the player can only be at most 500 away from the Projectile
            float distToPlayer = Main.player[targetPlayer].Center.X - Projectile.Center.X;
            if (distToPlayer > 0)
            {
                float val = distToPlayer - 500f;

                if (val > 0)
                    Projectile.Center = new Vector2(Projectile.Center.X + val, Projectile.Center.Y);
                
            }
            else
            {
                float val = distToPlayer + 500f;

                if (val < 0)
                    Projectile.Center = new Vector2(Projectile.Center.X + val, Projectile.Center.Y);
            }

            if (shouldFade)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.05f), 0f, 1f);
            else
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.03f), 0f, 1f);

            if (shouldFade = overallAlpha == 0f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D cloud = Mod.Assets.Request<Texture2D>("Assets/smoketrailsmudge").Value;  //smokeTrailsmudge

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float sineA = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.03f) * 0.2f;
            float sineB = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.11f) * 0.11f;
            float sineC = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.05f;
            float sineD = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.08f;



            Vector2 vec2Scale1 = new Vector2(8f * sineC, 0.7f * sineA).RotatedBy(Projectile.rotation) * Projectile.scale;
            Vector2 vec2Scale2 = new Vector2(8f * sineD, 0.7f * sineB).RotatedBy(Projectile.rotation) * Projectile.scale;

            Vector2 drawOffset = new Vector2(200f * sineD, 0f);

            Main.spriteBatch.Draw(cloud, drawPos + drawOffset, null, Color.SkyBlue with { A = 0 } * 0.5f * overallAlpha, Projectile.rotation, cloud.Size() / 2f, vec2Scale1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(cloud, drawPos, null, Color.SkyBlue with { A = 0 } * 0.5f * overallAlpha, Projectile.rotation, cloud.Size() / 2f, vec2Scale2, SpriteEffects.FlipVertically, 0);

            Vector2 coreScale1 = new Vector2(vec2Scale1.X, vec2Scale1.Y * 0.4f);
            Vector2 coreScale2 = new Vector2(vec2Scale2.X, vec2Scale2.Y * 0.4f);

            Main.spriteBatch.Draw(cloud, drawPos + drawOffset, null, Color.White with { A = 0 } * 0.5f * overallAlpha, Projectile.rotation, cloud.Size() / 2f, coreScale1, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(cloud, drawPos, null, Color.White with { A = 0 } * 0.5f * overallAlpha, Projectile.rotation, cloud.Size() / 2f, coreScale2, SpriteEffects.FlipVertically, 0);


            return false;
        }

    }

    public class WindPulse : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 9000;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = Projectile.hostile = false;

            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 800;
        }

        public override bool? CanDamage() => false;
    
        int timer = 0;
        public float scale = 0.25f;
        float alpha = 1;

        public float intensity = 1f;
        public int timeForPulse = 40;
        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            if (timer == 0)
            {
                Projectile.ai[0] = 0;
                Projectile.rotation = Main.rand.NextFloat(6.28f);
            }

            if (timer <= timeForPulse) //40
            {
                scale = MathHelper.Lerp(0f, 1f, Easings.easeOutQuint((float)timer / (float)timeForPulse));
            }

            if (timer >= 0)
            {

                if (timer >= (timeForPulse * 0.15f)) //6
                    alpha -= 0.065f;
            }

            Projectile.timeLeft = 2;

            if (alpha <= 0)
            {
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawShit(false);
            });

            return false;
        }

        public void DrawShit(bool giveUp = false)
        {
            if (giveUp)
                return;

            //String toAsset = "Assets/Orbs/whiteFireEyeA";

            String toAsset = "Assets/Orbs/ElectricPopC"; //circle_05

            //if (special) toAsset = "Assets/Orbs/ElectricPopC";

            Texture2D Flare = Mod.Assets.Request<Texture2D>(toAsset).Value;

            float rot = ((float)Main.timeForVisualEffects * 0.12f * Projectile.ai[0]) + Projectile.rotation;
            float scale2 = scale * 0.35f;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/T_Lu_Noise_30").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/BarelyBlueGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/T_Lu_Noise_30").Value);

            myEffect.Parameters["flowSpeed"].SetValue(0.3f);
            myEffect.Parameters["vignetteSize"].SetValue(1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.8f);
            myEffect.Parameters["distortStrength"].SetValue(0.06f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.01f);
            myEffect.Parameters["colorIntensity"].SetValue(alpha * 0.15f * intensity);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            myEffect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        }
    }

    public class MadisonVFXOld : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 224400; //180
            Projectile.extraUpdates = 0;
        }


        public bool shouldFadeOut = false;

        int timer = 0;
        float overallAlpha = 1f;
        float initialProg = 0f;
        public override void AI()
        {
            //Do a big burst of wind particles on startup
            if (timer == 0 && false)
            {
                int dustPerSide = 55 * 2;

                //The problem is that if the dust spawns far enough off-screen it just disappears
                int dir = 1;
                for (int i = 0; i < dustPerSide; i++)
                {
                    Vector2 windDustSpawnPosition = Projectile.Center + (new Vector2(-650f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloatDirection() * 900f) * 1f);
                    Vector2 windDustVelocity = new Vector2(1f, dir * 0.15f) * dir * Main.rand.NextFloat(0.1f, 1.8f) * 45f;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(1f, 2f));
                    wind.noGravity = true;
                }
                dir = -1;
                for (int i = 0; i < dustPerSide; i++)
                {
                    Vector2 windDustSpawnPosition = Projectile.Center + (new Vector2((-650f + Main.rand.NextFloat(-400f, 0f)) * dir, Main.rand.NextFloatDirection() * 900f) * 1f);
                    Vector2 windDustVelocity = new Vector2(1f, dir * 0.15f) * dir * Main.rand.NextFloat(0.1f, 1.8f) * 45f;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(1f, 2f));
                    wind.noGravity = true;
                }
            }



            int timeForInitialAnim = 40;
            float initProg = Math.Clamp((float)(timer - 1f) / (float)timeForInitialAnim, 0f, 1f);
            initialProg = initProg;

            //Dust that comes closer and closer as the telegraph shortens
            if (timer < timeForInitialAnim / 6f)
            {
                for (int i = 0; i < 8; ++i) //5
                {
                    int side = Main.rand.NextBool() ? 1 : -1;

                    float spawnY = Main.rand.NextFloat(-500f, 500f);
                    float spawnX = -1400f * (Easings.easeOutQuad(initProg));

                    Vector2 spawnPos = Projectile.Center + new Vector2(spawnX * side, spawnY);
                    spawnPos.X += -75 * side;

                    Vector2 velocity = new Vector2(5 + (20f * (1f - initProg)), 0f) * side;

                    Dust p = Dust.NewDustPerfect(spawnPos, ModContent.DustType<WindLine>(), velocity * -1f * Main.rand.NextFloat(0.8f, 1.05f), newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.25f, 0.5f));

                    WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, 1f, 1f, true);
                    wlb.randomVelRotatePower = 0.1f;

                    p.customData = wlb;
                }
            }

            if (initProg > 0f)
                shouldFadeOut = true;

            //Fade out started by FF NPC
            if (shouldFadeOut)
            {
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.08f), 0f, 1f);

                if (overallAlpha == 0f)
                    Projectile.active = false;
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D glow = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06
            Texture2D glowThick = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06

            int rightX = 100 + (int)MathHelper.Lerp(0f, 800f, Easings.easeOutQuad(initialProg)); // 1000
            int leftX = -100 + (int)MathHelper.Lerp(0f, -800f, Easings.easeOutQuad(initialProg));// 1000

            float colorIntensity = (0.75f + (MathF.Sin((float)Main.timeForVisualEffects * 0.2f) * 0.15f)) * 0.1f * overallAlpha;
            int sineHeightAddition = (int)(Math.Sin(Main.timeForVisualEffects * 0.05f) * 30f);
            int sineWidthAddition = (int)(Math.Sin(Main.timeForVisualEffects * 0.08f) * 20f);


            int wallWidth = (int)MathHelper.Lerp(1300f, 120f, Easings.easeOutSine(initialProg)) + sineWidthAddition;
            int wallHeight = 2000 + sineHeightAddition;
            Rectangle glowFrame = new Rectangle(0, 0, glow.Width, glow.Height / 2);
            Rectangle glowThickFrame = new Rectangle(0, 0, glowThick.Width, glowThick.Height / 2);


            //Left Wall---------------------------------------
            Rectangle glowTarget = new Rectangle((int)drawPos.X + leftX - wallWidth, (int)drawPos.Y, wallHeight, wallWidth);

            //Main.spriteBatch.Draw(glowThick, glowTarget, glowThickFrame, Color.DeepSkyBlue with { A = 0 } * 0.25f * colorIntensity, MathHelper.PiOver2, glowThick.Size() / 2f, SpriteEffects.FlipVertically, 0);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.DodgerBlue with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);

            //White
            glowTarget = new Rectangle((int)drawPos.X + leftX - (wallWidth / 3), (int)drawPos.Y, wallHeight, wallWidth / 3);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.LightSkyBlue with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);


            //Right Wall--------------------------------------------------
            glowTarget = new Rectangle((int)drawPos.X + rightX, (int)drawPos.Y, wallHeight, wallWidth);

            //Main.spriteBatch.Draw(glowThick, glowTarget, glowThickFrame, Color.DeepSkyBlue with { A = 0 } * 0.25f * colorIntensity, MathHelper.PiOver2, glowThick.Size() / 2f, SpriteEffects.None, 0);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.DodgerBlue with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.None, 0);

            //White
            glowTarget = new Rectangle((int)drawPos.X + rightX, (int)drawPos.Y, wallHeight, wallWidth / 3);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.LightSkyBlue with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.None, 0);

            return false;
        }

    }

    public class MadisonVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 224400; //180
            Projectile.extraUpdates = 0;
        }


        public bool shouldFadeOut = false;

        int timer = 0;
        float overallAlpha = 1f;
        float initialProg = 0f;
        public override void AI()
        {
            //Do a big burst of wind particles on startup
            if (timer == 0 && false)
            {
                int dustPerSide = 55 * 2;

                //The problem is that if the dust spawns far enough off-screen it just disappears
                int dir = 1;
                for (int i = 0; i < dustPerSide; i++)
                {
                    Vector2 windDustSpawnPosition = Projectile.Center + (new Vector2(-650f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloatDirection() * 900f) * 1f);
                    Vector2 windDustVelocity = new Vector2(1f, dir * 0.15f) * dir * Main.rand.NextFloat(0.1f, 1.8f) * 45f;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(1f, 2f));
                    wind.noGravity = true;
                }
                dir = -1;
                for (int i = 0; i < dustPerSide; i++)
                {
                    Vector2 windDustSpawnPosition = Projectile.Center + (new Vector2((-650f + Main.rand.NextFloat(-400f, 0f)) * dir, Main.rand.NextFloatDirection() * 900f) * 1f);
                    Vector2 windDustVelocity = new Vector2(1f, dir * 0.15f) * dir * Main.rand.NextFloat(0.1f, 1.8f) * 45f;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(1f, 2f));
                    wind.noGravity = true;
                }
            }



            //int timeForInitialAnim = 60;
            //float initProg = Math.Clamp((float)timer / (float)timeForInitialAnim, 0f, 1f);
            //initialProg = initProg;

            //Dust that comes closer and closer as the telegraph shortens
            //if (timer < timeForInitialAnim / 2f && false)
            //{
            //    for (int i = 0; i < 10; ++i) //5
            //    {
            //        int side = Main.rand.NextBool() ? 1 : -1;
            //
            //        float spawnY = Main.rand.NextFloat(-500f, 500f);
            //        float spawnX = -200 + -500f * (Easings.easeInQuart(initProg));

            //        Vector2 spawnPos = Projectile.Center + new Vector2(spawnX * side, spawnY);

            //        Vector2 velocity = new Vector2(5 + (20f * (1f - initProg)), 0f) * side;

            //        Dust p = Dust.NewDustPerfect(spawnPos, ModContent.DustType<WindLine>(), velocity * -1f, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.25f, 0.5f));

            //        WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.5f, 1f, 1f, true);
            //        wlb.randomVelRotatePower = 0.2f;

            //        p.customData = wlb;
            //    }
            //}

            if (timer > 8f)
                shouldFadeOut = true;

            //Fade out started by FF NPC
            if (shouldFadeOut)
            {
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.08f), 0f, 1f);

                if (overallAlpha == 0f)
                    Projectile.active = false;
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D glow = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_06").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06
            Texture2D glowThick = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //TextureLaser goes kinda crazy, and smokeTrail4_512 and spark_06




            float colorIntensity = (0.75f + (MathF.Sin((float)Main.timeForVisualEffects * 0.2f) * 0.15f)) * 0.25f * overallAlpha;
            int sineHeightAddition = (int)(Math.Sin(Main.timeForVisualEffects * 0.05f) * 30f);
            int sineWidthAddition = (int)(Math.Sin(Main.timeForVisualEffects * 0.08f) * 20f);


            int wallWidth = (int)MathHelper.Lerp(1300f, 120f, Easings.easeOutSine(initialProg)) + sineWidthAddition;
            int wallHeight = 2000 + sineHeightAddition;
            Rectangle glowFrame = new Rectangle(0, 0, glow.Width, glow.Height / 2);


            //Left Wall---------------------------------------
            Rectangle glowTarget = new Rectangle((int)drawPos.X - wallWidth, (int)drawPos.Y, wallHeight, wallWidth);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.DeepSkyBlue with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);

            //White
            glowTarget = new Rectangle((int)drawPos.X - (wallWidth / 3) - 2, (int)drawPos.Y, wallHeight, wallWidth / 3);
            Main.spriteBatch.Draw(glow, glowTarget, glowFrame, Color.White with { A = 0 } * 2f * colorIntensity, MathHelper.PiOver2, glow.Size() / 2f, SpriteEffects.FlipVertically, 0);

            return false;
        }

    }

    public class WindTrail : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 1000; //180
            Projectile.extraUpdates = 0;
        }



        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        public override void AI()
        {
            if (timer == 0)
                Projectile.ai[0] = 1f;

            int trailCount = 20;  //14
            previousRotations.Add(Projectile.velocity.ToRotation()); //
            previousPositions.Add(Projectile.Center + Projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            //Projectile.velocity = Projectile.velocity.RotatedBy(0.045f * Projectile.ai[0]); //0.045

            //float rotVal = MathF.Sin((float)timer * 0.6f) * 0.06f;
            //Projectile.velocity = Projectile.velocity.RotatedBy(rotVal); //0.045

            //Projectile.velocity = Projectile.velocity.RotatedBy(0.09f * Projectile.ai[0]); //0.045

            if (timer % 4 == 0)
                Projectile.ai[0] *= -1f;


            float timeForPopInAnim = 23; //33
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.75f)) * 1.3f;

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawVertexTrail(false);
            });
            DrawVertexTrail(true);

            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/s06sBloom").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0f;


            Color StripColor(float progress) => Color.White * Easings.easeInSine(progress) * overallAlpha;

            float StripWidth(float progress)
            {
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

                return toReturn * sineWidthMult * overallScale * 120f * 0.75f; //50
            }

            float StripWidth2(float progress)
            {
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

                return toReturn * overallScale * 40f * 0.75f; //50
            }


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.025f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.SkyBlue.ToVector3() * 2f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1.2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Color between = Color.Lerp(Color.OrangeRed, Color.Orange, 1f);
            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(Color.SkyBlue.ToVector3() * 2.5f); //Hotpink4.5
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            /*
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/LavaTrailBloom").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/LavaTrailBloom").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0f;


            Color StripColor(float progress) => Color.White * Easings.easeInSine(progress) * overallAlpha;

            float StripWidth(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.5f)
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutQuad(LV);
                }
                else
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = Easings.easeOutQuad(1f - LV);
                }

                return toReturn * sineWidthMult * overallScale * 50f; //80
            }

            //float StripWidth(float progress) => Math.Clamp(120f * sineWidthMult * overallScale * Easings.easeInSine(progress), 20f, 100f) * 0.3f;
            //float StripWidth2(float progress) => Math.Clamp(75f * sineWidthMult * overallScale * Easings.easeInSine(progress), 20f, 100f) * 0.3f; //75


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.035f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);


            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(Color.Aquamarine.ToVector3() * 2.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.Aquamarine.ToVector3() * 2f);
            myEffect.Parameters["glowThreshold"].SetValue(0.8f);
            myEffect.Parameters["glowIntensity"].SetValue(1.2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();


            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            */
            ////////////////////////////////
            ///*/
            //Good shit two

            /* 8
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/LavaTrailBloom").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0f;


            Color StripColor(float progress) => Color.White * Easings.easeInSine(progress) * overallAlpha;

            float StripWidth(float progress)
            {
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

                return toReturn * sineWidthMult * overallScale * 120f * 0.75f; //50
            }

            float StripWidth2(float progress)
            {
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

                return toReturn * overallScale * 40f * 0.75f; //50
            }


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.035f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.HotPink.ToVector3() * 2f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1.2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Color between = Color.Lerp(Color.OrangeRed, Color.Orange, 1f);
            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(Color.HotPink.ToVector3() * 4.5f); //Hotpink4.5
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            */

        }

    }

    public class WindTrail2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 1000; //180
            Projectile.extraUpdates = 1;
        }



        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        float flipTimer = 0f;
        int flipAmount = 0;
        public override void AI()
        {
            if (timer == 0)
                Projectile.ai[0] = 1f;

            int trailCount = 50;  //14
            previousRotations.Add(Projectile.velocity.ToRotation()); //
            previousPositions.Add(Projectile.Center + Projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Projectile.velocity = Projectile.velocity.RotatedBy(0.045f * Projectile.ai[0]); //0.045

            if (timer % 4 == 0 && Main.rand.NextBool(20))
                Projectile.ai[0] *= -1f;


            float timeForPopInAnim = 23; //33
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.75f)) * 1.3f;

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawVertexTrail(false);
            });
            DrawVertexTrail(true);

            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Noise/LavaNoise").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0f;


            Color StripColor(float progress) => Color.White * Easings.easeInCirc(progress) * overallAlpha;

            float StripWidth(float progress)
            {
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

                return toReturn * sineWidthMult * overallScale * 15f; //50
            }

            float StripWidth2(float progress)
            {
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

                return toReturn * overallScale * 60; //50
            }


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.035f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.OrangeRed.ToVector3() * 2.5f);
            myEffect.Parameters["glowThreshold"].SetValue(0.8f);
            myEffect.Parameters["glowIntensity"].SetValue(2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Color between = Color.Lerp(Color.OrangeRed, Color.Orange, 1f);
            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(Color.Goldenrod.ToVector3() * 2.5f); //Hotpink4.5
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        }

    }


    public class WindPulseTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 9000;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = Projectile.hostile = false;

            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 800;
        }

        public override bool? CanDamage() => false;

        int timer = 0;
        public float scale = 1f;
        float alpha = 1;

        public float intensity = 1f;
        public int timeForPulse = 40;
        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            if (timer == 0)
            {
                Projectile.ai[0] = 1f;
                Projectile.rotation = Main.rand.NextFloat(6.28f);
            }

            //if (timer <= timeForPulse) //40
           // {
            //    scale = MathHelper.Lerp(0f, 1f, Easings.easeOutQuint((float)timer / (float)timeForPulse));
           // }

            if (timer >= 0)
            {

                //if (timer >= (timeForPulse * 0.15f)) //6
                //    alpha -= 0.065f;
            }

            Projectile.timeLeft = 2;

            if (alpha <= 0)
            {
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawShit(false);
            });
            DrawShit(true);

            return false;
        }

        public void DrawShit(bool giveUp = false)
        {
            if (giveUp)
                return;

            //String toAsset = "Assets/Orbs/whiteFireEyeA";

            String toAsset = "Assets/Orbs/bigCircle2"; //circle_05

            //if (special) toAsset = "Assets/Orbs/ElectricPopC";

            Texture2D Flare = Mod.Assets.Request<Texture2D>(toAsset).Value;

            float rot = ((float)Main.timeForVisualEffects * 0.02f * Projectile.ai[0]) + Projectile.rotation;
            float scale2 = scale * 1.65f;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/T_Lu_Noise_30").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/SofterBlueGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Swirl").Value);

            myEffect.Parameters["flowSpeed"].SetValue(0.3f);
            myEffect.Parameters["vignetteSize"].SetValue(0.05f);
            myEffect.Parameters["vignetteBlend"].SetValue(1f);
            myEffect.Parameters["distortStrength"].SetValue(0.06f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.01f);
            myEffect.Parameters["colorIntensity"].SetValue(alpha * 0.5f * intensity);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.EffectMatrix);

            myEffect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

        }
    }


    public class WindPulseTest2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 9000;
        }
        public override void SetDefaults()
        {
            Projectile.friendly = Projectile.hostile = false;

            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 800;
        }

        public override bool? CanDamage() => false;

        int timer = 0;
        public float scale = 1f;
        float alpha = 1;

        public float intensity = 1f;
        public int timeForPulse = 40;
        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            if (timer == 0)
            {
                Projectile.ai[0] = 1f;
                Projectile.rotation = Main.rand.NextFloat(6.28f);
            }

            //if (timer <= timeForPulse) //40
            // {
            //    scale = MathHelper.Lerp(0f, 1f, Easings.easeOutQuint((float)timer / (float)timeForPulse));
            // }

            if (timer >= 0)
            {

                //if (timer >= (timeForPulse * 0.15f)) //6
                //    alpha -= 0.065f;
            }

            Projectile.timeLeft = 2;

            if (alpha <= 0)
            {
                Projectile.active = false;
            }

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawShit(false);
            });
            DrawShit(true);

            return false;
        }

        public void DrawShit(bool giveUp = false)
        {
            if (giveUp)
                return;

            //String toAsset = "Assets/Orbs/whiteFireEyeA";

            String toAsset = "Assets/Orbs/bigCircle2"; //circle_05

            //if (special) toAsset = "Assets/Orbs/ElectricPopC";

            Texture2D Flare = Mod.Assets.Request<Texture2D>(toAsset).Value;
            Texture2D Flare2 = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            float rot = ((float)Main.timeForVisualEffects * 0.02f * Projectile.ai[0]) + Projectile.rotation;
            float scale2 = scale * 1f;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Compiler/CustomRadialScrollBase", AssetRequestMode.ImmediateLoad).Value;

            Vector3 fireCol1 = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 fireCol2 = new Vector3(0.74f, 0.13f, 0.06f);
            Vector3 fireCol3 = new Vector3(0.87f, 0.4f, 0.051f);
            Vector3 fireCol4 = new Vector3(0.95f, 0.63f, 0.16f);
            Vector3 fireCol5 = new Vector3(1.0f, 1.0f, 1.0f);

            Vector3[] colors = { fireCol1, fireCol2, fireCol3, fireCol4, fireCol5 };
            float[] powers = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

            myEffect.Parameters["zoom"].SetValue(0.5f);
            myEffect.Parameters["numberOfColors"].SetValue(5);
            myEffect.Parameters["gradColors"].SetValue(colors);
            myEffect.Parameters["gradPowers"].SetValue(powers);


            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Noise_1").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Swirl").Value);

            myEffect.Parameters["flowSpeed"].SetValue(0.4f);
            myEffect.Parameters["distortStrength"].SetValue(0.02f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["colorIntensity"].SetValue(alpha * 1f * intensity);

            //Main.spriteBatch.Draw(Flare2, Projectile.Center - Main.screenPosition + new Vector2(0f, 0f), null, Color.OrangeRed with { A = 255 } * 1f, rot, Flare2.Size() / 2, scale2 * Projectile.scale * 3f, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Flare2, Projectile.Center - Main.screenPosition + new Vector2(0f, 0f), null, Color.Orange with { A = 255 } * 1f, rot, Flare2.Size() / 2, scale2 * Projectile.scale * 1f, SpriteEffects.None, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, myEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White, rot, Flare.Size() / 2, scale2 * Projectile.scale, SpriteEffects.None, 0f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }

}