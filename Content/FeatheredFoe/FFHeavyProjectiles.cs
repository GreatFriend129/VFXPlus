using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria;
using VFXPlus.Common.Drawing;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Build.Evaluation;

namespace VFXPlus.Content.FeatheredFoe
{
    public class FFMaelstrom : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;


            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCs.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
        }

        float animProgress = 0;
        float overallAlpha = 0f;
        float overallScale = 0f;

        int timer = 0;
        public int advancer = 0;
        public int startDir = 1;
        public float additionAmount = 0.1f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override void AI()
        {
            Projectile.rotation -= 0.25f;

            if (timer == 0)
            {
                //Orbiting feather border
                int featherCount = 12;
                for (int i = 0; i < featherCount; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount) * i;

                    for (int j = 0; j < 4; j++)
                    {
                        int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                        if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                        {
                            bof.ParentProj = Projectile.whoAmI;
                            bof.orbitSpeed = 0.03f;
                            bof.originalDir = new Vector2(1f, 0f).RotatedBy(rot + (-0.15f * j));
                            bof.orbitDistance = 815f + 30 * j;
                            bof.orbitDir = startDir;
                        }
                    }
                }

                //Inner feathers
                int innerFeatherCount = 0; //12
                for (int i = 0; i < innerFeatherCount; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)innerFeatherCount) * i;

                    for (int j = 0; j < 7; j++)
                    {
                        if (j != 3 && j != 6 && j != 9)
                        {
                            int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                            if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                            {
                                bof.ParentProj = Projectile.whoAmI;
                                bof.orbitSpeed = 0.03f; //0.04
                                bof.originalDir = new Vector2(1f, 0f).RotatedBy(rot);
                                bof.orbitDistance = 150f + (85f * j); //270
                                bof.orbitDir = startDir;
                            }
                        }

                    }

                }
            }

            if (timer % 1 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float scale = 2.2f;

                    SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 3f, 0.92f);

                    Vector2 random = Main.rand.NextVector2CircularEdge(120f, 120f) * Main.rand.NextFloat(1f, 5f);

                    Vector2 vel = random.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.25f;

                    Dust star = Dust.NewDustPerfect(Projectile.Center + random, ModContent.DustType<SmallSmoke>(), vel, newColor: Color.White, Scale: scale);
                    star.customData = ssb;


                    //Normal Dust
                    Vector2 random2 = Main.rand.NextVector2CircularEdge(120f, 120f) * Main.rand.NextFloat(1f, 6f);
                    Vector2 vel2 = random2.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0f) * Main.rand.NextFloat(4f, 14f);
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + random2, 176, vel2, newColor: Color.DodgerBlue with { A = 0 }, Scale: Main.rand.NextFloat(1f, 1.5f));
                    dust.noGravity = true;
                }
            }

            float timeForPopInAnim = 35; //37
            float animProgress = Math.Clamp((timer + 13) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 0.75f)) * 1f;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                GoddamnMonsoonCirc(200); //250
            });

            DrawVanillaSwirl2(false);

            return false;
        }

        Effect myEffect = null;
        public void DrawVanillaSwirl2(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Swirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirlGlow").Value; //FireSpot goes kinda crazy| same with PixelSwirl

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Vector2 origin = Swirl.Size() / 2f;

            float sinOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.2f;
            float cosOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.077f) * 0.1f;

            float startScale = sinOff;
            float endScale = 30f * cosOff; //6f

            float scale = 1f;


            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            //Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.1f, 0f, Orb.Size() / 2f, endScale * 1f * overallScale, SpriteEffects.FlipHorizontally);
            //Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.5f, 0f, Orb.Size() / 2f, endScale * 0.15f * overallScale, SpriteEffects.FlipHorizontally);

            float reps = 20f;
            for (int i = 0; i < reps; i++)
            {
                float prog = 1f - ((float)i / reps);

                //prog = Easings.easeOutCubic(prog);

                //End Color <--> Start color
                Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);
                Color col = Color.Lerp(between * 1f, Color.LightSkyBlue, Easings.easeOutSine(prog));


                float alpha = Easings.easeInSine(prog) * 0.2f; //0.2
                float newRot = (float)Main.timeForVisualEffects * 0.07f * (scale * 0.17f); //025 25 | 005 017
                float newScale = MathHelper.Lerp(endScale, startScale, Easings.easeOutCubic(prog));

                Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha, newRot, origin, newScale * 1.25f * overallScale, SpriteEffects.FlipHorizontally); //1.25

                //8
                if (i >= reps * 0.66f)
                    scale = scale * 1.25f;
                else
                    scale = scale * 1.15f;
            }

            Texture2D Circ = ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/circle_02").Value;

            float circRot = ((float)Main.timeForVisualEffects * 0.02f);
            float circScale = 1f * 5.65f;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/T_Lu_Noise_30").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/SofterBlueGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/Swirl").Value);

            myEffect.Parameters["flowSpeed"].SetValue(0.3f);
            myEffect.Parameters["vignetteSize"].SetValue(1f);
            myEffect.Parameters["vignetteBlend"].SetValue(0.8f);
            myEffect.Parameters["distortStrength"].SetValue(0.04f);
            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            myEffect.Parameters["colorIntensity"].SetValue(0.3f);


            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, myEffect, Main.GameViewMatrix.TransformationMatrix);
            myEffect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(Circ, Projectile.Center - Main.screenPosition, null, Color.White, circRot, Circ.Size() / 2, circScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public void GoddamnMonsoonCirc(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value; //PixelSwirl
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new("mule".GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 1.5f; //1.75f

            float minRange = 140f; //240f
            float maxRange = 920f; //920
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                float alpha = 1f;
                if (r.NextFloat() < 0.3f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                    alpha = 0.5f;
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
                    rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f);
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance;


                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.45f);
                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, col with { A = 0 } * 0.9f * alpha, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

    }

    public class LotusFlower : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;


            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 22900;

        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
        }

        float animProgress = 0;
        float overallAlpha = 0f;
        float overallScale = 0f;

        int timer = 0;
        public int advancer = 0;
        public int startDir = 1;
        public float additionAmount = 0.1f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override void AI()
        {
            Projectile.rotation -= 0.25f;

            if (timer % 1 == 0)
            {
                float scale = 2.2f;

                SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 3f, 0.92f);

                Vector2 random = Main.rand.NextVector2CircularEdge(120f, 120f) * Main.rand.NextFloat(1f, 5f);

                Vector2 vel = random.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.25f;

                Dust star = Dust.NewDustPerfect(Projectile.Center + random, ModContent.DustType<SmallSmoke>(), vel, newColor: Color.SkyBlue, Scale: scale);
                star.customData = ssb;
            }

            float timeForPopInAnim = 35; //37
            float animProgress = Math.Clamp((timer + 13) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 0.75f)) * 1f;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                GoddamnMonsoonCirc(50); //200

                DrawVanillaSwirl2(true);
            });

            DrawVanillaSwirl2(false);

            return false;
        }

        public void DrawVanillaSwirl2(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Swirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirlGlow").Value; //FireSpot goes kinda crazy| same with PixelSwirl

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 origin = Swirl.Size() / 2f;

            float sinOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.1f;
            float cosOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.077f) * 0.05f;

            float startScale = sinOff;
            float endScale = 40f * cosOff; //6f

            float scale = 0.9f;


            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            //Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.1f, 0f, Orb.Size() / 2f, endScale * 1f * overallScale, SpriteEffects.FlipHorizontally);
            //Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.5f, 0f, Orb.Size() / 2f, endScale * 0.15f * overallScale, SpriteEffects.FlipHorizontally);

            float reps = 36f;
            for (int i = 0; i < reps; i++)
            {
                float prog = 1f - ((float)i / reps);

                //prog = Easings.easeOutCubic(prog);

                //End Color <--> Start color
                Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);
                Color col = Color.Lerp(between * 1f, Color.LightSkyBlue, Easings.easeOutSine(prog));


                float customAlphaProg = MathF.Pow(prog, 3.5f);
                float alpha = customAlphaProg * 0.4f;


                float newRot = (float)Main.timeForVisualEffects * 0.035f * (scale * 0.18f); //(i % 2 == 0 ? 1f : -1f);

                float customEaseProg = 1f - MathF.Pow(1f - prog, 2.65f);

                float newScale = MathHelper.Lerp(endScale, startScale, customEaseProg);

                Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha, newRot, origin, newScale * 1.25f * overallScale, SpriteEffects.FlipHorizontally);

                //8
                if (i >= reps * 0.66f)
                    scale = scale * 1.25f;
                else
                    scale = scale * 1.15f;
            }

        }

        public void GoddamnMonsoonCirc2(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 2f;

            float minRange = 240f; //40f | 240 920 for full screen
            float maxRange = 920f; //120
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.3f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
                    rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f);
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance;

                //Vector2 drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -20f, 14f) + yOffset * yDir); //-20 14 | -120

                //float prog = (float)i / (float)count - 1f;
                //Color col = Main.hslToRgb((prog + timer * 0.005f) % 1f, 1f, 0.7f, 0);

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.Aquamarine with { A = 0 }, randomRot + rotation + MathHelper.PiOver2, origin,
    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

                //Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White, randomRot + rotation + MathHelper.PiOver2, origin,
                //new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public void GoddamnMonsoonCirc(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value; //PixelSwirl
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new("mule".GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 1.65f; //175

            float minRange = 140f; //240f
            float maxRange = 480f; //920
            for (int i = 0; i < count; i++)
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                float alpha = 1f;
                if (r.NextFloat() < 0.3f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new Vector2(0.3f, 0.66f) * 0.4f; //0.3 0.66
                    alpha = 0.5f;
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
                    rotation += speedTime * NextFloatFastRandom(r, 0.8f, 1.2f);
                }
                Vector2 origin = frame.Size() / 2f;
                float speed = NextFloatFastRandom(r, 0.8f, 4f);
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float ringDistance = NextFloatFastRandom(r, minRange, maxRange);

                float randomRot = NextFloatFastRandom(r, 0f, MathHelper.TwoPi) + speedTime * speed;

                Vector2 drawPosition = Projectile.Center + new Vector2(1f, 0f).RotatedBy(randomRot) * ringDistance;


                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.36f);
                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, col with { A = 0 } * 0.9f * alpha, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

    }

}