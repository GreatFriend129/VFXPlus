using System;
using System.Collections.Generic;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using VFXPlus.Common;
using Terraria.GameContent;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Dusts;
using Terraria.ID;

namespace VFXPlus.Content.FeatheredFoe
{
    public class MadisonTornado : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 64;


            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 250;

        }

        public override bool? CanDamage()
        {
            return timer > 50;
        }

        float alpha = 0f;
        float scale = 0f;

        int timer = 0;
        public int advancer = 0;
        public int startDir = 1;


        //Max vel after we start accelerating
        public float postAccelMaxVel = 21;

        public float yVel = 0f;

        //Max vel before we start accelerating
        public float maxVel = 14f; //18f
        public int timeForVelShift = 70;// 60

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            Projectile.scale = 1.15f;
            
            int trailCount = 14; //7
            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            //AlphaScale
            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            scale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1.5f));

            alpha = Math.Clamp(MathHelper.Lerp(alpha, 1.5f, 0.09f), 0f, 1f);


            //Movement
            Vector2 initialVel = new Vector2((maxVel * 1.25f) * startDir, 0f);
            Vector2 goalVel = new Vector2(-maxVel * startDir, 0f);
            float timeForMovement = timeForVelShift;
            float timeProgress = Math.Clamp((float)timer / timeForMovement, 0f, 1f);
            Projectile.velocity = Vector2.Lerp(initialVel, goalVel, Easings.easeInOutSine(timeProgress));

            if (timer > timeForVelShift * 1.05f)
            {
                maxVel = Math.Clamp(maxVel * 1.005f, 0f, postAccelMaxVel);
            }

            //Dust--
            //Vanilla weather pain dust
            if (timer % 3 == 0 && Main.rand.NextBool())
            {
                int num6 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 16, Projectile.velocity.X, Projectile.velocity.Y, 120, default(Color), 0.35f);
                Main.dust[num6].noGravity = true;
                Main.dust[num6].fadeIn = 0.45f;
                Main.dust[num6].velocity = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -2f) + Projectile.velocity * 0.75f;
                for (int j = 0; j < 1; j++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 16, Projectile.velocity.X, Projectile.velocity.Y, 60, default(Color), 0.35f);
                    dust.noGravity = true;
                    dust.fadeIn = 0.35f;
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f) * 0.2f + new Vector2(0f, -0.4f) + Projectile.velocity * 1.5f;
                    dust.position -= Projectile.velocity * 3f;
                }
            }

            if (timer % 2 == 0)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f) - Projectile.velocity * 0.5f;

                float dustScale = Main.rand.NextFloat(0.5f, 1f);

                Dust wind = Dust.NewDustPerfect(dustPos, 176, dustVel * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 0.5f, Scale: dustScale); //dust176
                wind.noGravity = true;
            }

            //Anim
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Projectile.rotation = 0f + Projectile.velocity.X * 0.02f;


            #region vanillaWeatherPainStuff
            /*
            SlotId val;
            if (this.soundDelay == 0)
            {
                this.soundDelay = -1;
                float[] array = this.localAI;
                val = SoundEngine.PlayTrackedSound(in SoundID.DD2_BookStaffTwisterLoop, base.Center);
                array[1] = ((SlotId)(ref val)).ToFloat();
            }
            ActiveSound activeSound = SoundEngine.GetActiveSound(SlotId.FromFloat(this.localAI[1]));
            if (activeSound != null)
            {
                activeSound.Position = base.Center;
                activeSound.Volume = 1f - Math.Max(this.ai[1] - 555f, 0f) / 15f;
            }
            else
            {
                float[] array2 = this.localAI;
                val = SlotId.Invalid;
                array2[1] = ((SlotId)(ref val)).ToFloat();
            }
            this.ai[1] += 1f;
            if (this.ai[1] > 560f)
            {
                this.alpha = (int)MathHelper.Lerp(0f, 250f, (this.ai[1] - 560f) / 10f);
            }
            if (this.ai[1] >= 570f)
            {
                this.Kill();
            }
            float num = 555f;
            for (int i = 0; i < 1000; i++)
            {
                if (i != base.whoAmI && Main.projectile[i].active && Main.projectile[i].owner == this.owner && Main.projectile[i].type == this.type && this.timeLeft > Main.projectile[i].timeLeft && Main.projectile[i].ai[1] < num)
                {
                    Main.projectile[i].ai[1] = num;
                    Main.projectile[i].netUpdate = true;
                }
            }
            */
            #endregion

            Projectile.position.Y += yVel;
            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //DrawWindDrill(-1, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.25f) * 0.35f, 20);

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/FFTornado").Value;
            Texture2D Bloom = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Tex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.Lerp(Color.DodgerBlue, Color.SkyBlue, progress) * progress * progress * alpha;

                float size2 = (0.5f + (progress * 0.75f)) * Projectile.scale * scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(Tex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.15f,
                        previousRotations[i], TexOrigin, size2, se);
            }
            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(Tex, drawPos + Main.rand.NextVector2Circular(5f, 5f), sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * 0.35f * alpha, Projectile.rotation, TexOrigin, Projectile.scale * 1.07f * scale, se);
            }

            //Bloom
            Vector2 bloomScale = new Vector2(0.7f, 0.9f);
            Main.EntitySpriteDraw(Bloom, drawPos, null, Color.DeepSkyBlue with { A = 0 } * alpha * 0.25f, Projectile.rotation, Bloom.Size() / 2f, bloomScale * Projectile.scale * scale, se);
            Main.EntitySpriteDraw(Bloom, drawPos, null, Color.SkyBlue with { A = 0 } * alpha * 0.15f, Projectile.rotation, Bloom.Size() / 2f, bloomScale * Projectile.scale * scale * 0.8f, se);


            Main.EntitySpriteDraw(Tex, drawPos, sourceRectangle, lightColor * alpha, Projectile.rotation, TexOrigin, Projectile.scale * scale, se);


            //DrawWindDrill(1, Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f) * 0.75f, 20); //0.7f

            return false;
        }

        public void DrawWindDrill(int yDir, Color color, int particleCount)
        {
            float drillRotation = Projectile.rotation;

            FastRandom r = new("PENIS".GetHashCode());

            float speedTime = Main.GlobalTimeWrappedHourly * 1.25f;

            var windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Nightglow").Value;
            var dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            //Todo swap name
            float ringMinLength = 45f;
            float ringMaxLength = 15f; 

            for (int i = 0; i < particleCount; i++) //160
            {
                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = drillRotation + MathHelper.PiOver2;
                if (r.NextFloat() < 0.1f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new(0.11f * 2f, 0.25f * 2f); //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.45f, 0.45f);
                    rotation += speedTime * GeneralUtilities.NextFloatF(r, 0.8f, 1.2f);
                }
                var origin = frame.Size() / 2f;
                float speed = GeneralUtilities.NextFloatF(r, 0.8f, 4f);

                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float waveDistance = GeneralUtilities.NextFloatF(r, ringMinLength, ringMaxLength) + 10f; 

                float time = speedTime * 2f;
                float min = 1f; //Controls how much the ring sways 
                float max = 1f; //^

                float yWave = min + ((float)Math.Sin(time) + 1f) / 2f * (max - min);

                float yOffset = scaleWave * waveDistance * 0.15f * yWave; //0.3 --> How wide is the gap of the ring ()
                float xWave = MathF.Sin(progress * MathHelper.Pi - MathHelper.PiOver2) * yDir;

                Vector2 posOffset = new Vector2(0f, 20f).RotatedBy(drillRotation);

                var drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, GeneralUtilities.NextFloatF(r, -60f, 15f) + yOffset * yDir).RotatedBy(drillRotation) + posOffset;

                //Makes the rings closer to tip smaller
                float ringDistProg = 1f - Utils.GetLerpValue(ringMinLength, ringMaxLength, waveDistance, true);
                float adjustedScale = 0.9f + (0.45f * ringDistProg);

                Main.spriteBatch.Draw(texture, (drawPosition - Main.screenPosition).Floor(), frame, Color.Black * 0.25f, rotation - xWave / 3f * yWave * yDir, origin,
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 2.25f, SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(
                    texture,
                    (drawPosition - Main.screenPosition).Floor(),
                    frame,
                    color with { A = 0 } * alpha * 3f,
                    rotation - xWave / 3f * yWave * yDir,
                    origin,
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 2.25f,
                    SpriteEffects.None,
                    0f
                );
            }
        }

    }
    public class TornadoTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 64;

            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 11500;
        }


        float animProgress = 0;
        float alpha = 0f;
        float scale = 0f;

        int timer = 0;
        public int advancer = 0;
        public int startDir = 1;
        public float additionAmount = 0.1f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override void AI()
        {

            //Projectile.timeLeft = 100;

            //Projectile.velocity = new Vector2(0f, -13f);

            Projectile.scale = 1.15f;//1.15f;

            int trailCount = 7; //5
            previousRotations.Add(Projectile.rotation);
            previousPostions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            //AlphaScale
            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f);

            scale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress));

            Projectile.rotation = 0f + Projectile.velocity.X * 0.05f;

            if (Projectile.velocity.Length() <= 25)
            {
                float velAddition = startDir == 1 ? -0.12f : 0.12f;

                //Projectile.velocity.X += velAddition;
            }

            //Dust
            if (timer % 3 == 0)
            {
                int num6 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 16, Projectile.velocity.X, Projectile.velocity.Y, 120, default(Color), 0.5f);
                Main.dust[num6].noGravity = true;
                Main.dust[num6].fadeIn = 0.9f;
                Main.dust[num6].velocity = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -2f) + Projectile.velocity * 0.75f;
                for (int j = 0; j < 2; j++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 16, Projectile.velocity.X, Projectile.velocity.Y, 60, default(Color), 0.5f);
                    dust.noGravity = true;
                    dust.fadeIn = 0.7f;
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f) * 0.2f + new Vector2(0f, -0.4f) + Projectile.velocity * 1.5f;
                    dust.position -= Projectile.velocity * 3f;
                }
            }

            //Anim
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
            //Projectile.spriteDirection = Projectile.velocity.X >= 0 ? 1 : -1;


            #region vanillaWeatherPainStuff
            /*
            SlotId val;
            if (this.soundDelay == 0)
            {
                this.soundDelay = -1;
                float[] array = this.localAI;
                val = SoundEngine.PlayTrackedSound(in SoundID.DD2_BookStaffTwisterLoop, base.Center);
                array[1] = ((SlotId)(ref val)).ToFloat();
            }
            ActiveSound activeSound = SoundEngine.GetActiveSound(SlotId.FromFloat(this.localAI[1]));
            if (activeSound != null)
            {
                activeSound.Position = base.Center;
                activeSound.Volume = 1f - Math.Max(this.ai[1] - 555f, 0f) / 15f;
            }
            else
            {
                float[] array2 = this.localAI;
                val = SlotId.Invalid;
                array2[1] = ((SlotId)(ref val)).ToFloat();
            }
            this.ai[1] += 1f;
            if (this.ai[1] > 560f)
            {
                this.alpha = (int)MathHelper.Lerp(0f, 250f, (this.ai[1] - 560f) / 10f);
            }
            if (this.ai[1] >= 570f)
            {
                this.Kill();
            }
            float num = 555f;
            for (int i = 0; i < 1000; i++)
            {
                if (i != base.whoAmI && Main.projectile[i].active && Main.projectile[i].owner == this.owner && Main.projectile[i].type == this.type && this.timeLeft > Main.projectile[i].timeLeft && Main.projectile[i].ai[1] < num)
                {
                    Main.projectile[i].ai[1] = num;
                    Main.projectile[i].netUpdate = true;
                }
            }
            */
            #endregion
            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawWindVortex(-1, Color.DeepSkyBlue * 0.33f);

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/FFTornado").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Tex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue * 0.15f, progress) * progress;

                    float size2 = (1f + (progress * 0.25f)) * Projectile.scale * scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(Tex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.55f,
                            previousRotations[i], TexOrigin, size2, se);
                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(Tex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.SkyBlue with { A = 0 } * 0.5f, Projectile.rotation, TexOrigin, Projectile.scale * 1.1f * scale, se);
            }

            Main.EntitySpriteDraw(Tex, drawPos, sourceRectangle, lightColor, Projectile.rotation, TexOrigin, Projectile.scale * scale, se);


            DrawWindVortex(1, Color.SkyBlue * 0.7f);
            return false;
        }

        public void DrawWindVortex(int yDir, Color color)
        {
            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());

            float speedTime = Main.GlobalTimeWrappedHourly * 1.25f;

            //var windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            var windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            var dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;


            //Main.NewText(Main.player[Projectile.owner].name.GetHashCode());

            float ringMinLength = 115f; //20f
            float ringMaxLength = 20f; //115


            //160
            for (int i = 0; i < 80; i++) //60
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.1f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new(0.15f, 0.33f); //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(0.5f, 0.5f);
                    rotation += speedTime * NextFloatF(r, 0.8f, 1.2f);
                }
                var origin = frame.Size() / 2f;
                float speed = NextFloatF(r, 0.8f, 4f);

                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float waveDistance = NextFloatF(r, ringMinLength, ringMaxLength); //40 120 overall distance of the ring

                float time = speedTime * 2f;
                float min = 0.95f; //Controls how much the ring sways 
                float max = 0.95f; //^

                float yWave = min + ((float)Math.Sin(time) + 1f) / 2f * (max - min);

                float yOffset = scaleWave * waveDistance * 0.3f * yWave; //0.3
                float xWave = MathF.Sin(progress * MathHelper.Pi - MathHelper.PiOver2) * yDir;
                var drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -120f, 40f) + yOffset * yDir); //Vertical distance of the ring

                //Makes the rings closer to tip smaller
                float ringDistProg = 1f - Utils.GetLerpValue(ringMinLength, ringMaxLength, waveDistance, true);
                float adjustedScale = 1f + (0.35f * ringDistProg);

                Main.spriteBatch.Draw(
                    texture,
                    (drawPosition - Main.screenPosition).Floor(),
                    frame,
                    color with { A = 0 },
                    rotation - xWave / 3f * yWave * yDir,
                    origin,
                    new Vector2(scale.X * scaleWave * scaleWave * adjustedScale, scale.Y * scaleWave * adjustedScale) * 3f,
                    SpriteEffects.None,
                    0f
                );
            }

        }

        public float NextFloatF(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

        public override void OnKill(int timeLeft)
        {

            base.OnKill(timeLeft);
        }
    }

    public class FFWindOrb : ModProjectile
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

        float overallAlpha = 0f;
        float overallScale = 0f;

        int timer = 0;
        
        public int startDir = 1;
        public int playerID = -1;

        public override void AI()
        {
            if (playerID == -1)
            {
                Main.NewText("playerID == -1 | FFWindOrb");
                Projectile.active = false;
                return;
            }
            Player target = Main.player[playerID];

            if (timer == 0)
            {
                Projectile.ai[0] = Projectile.velocity.Length();

                int featherCount = 3; //3
                for (int i = 220; i < featherCount; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount) * i;

                    int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                    if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                    {
                        bof.ParentProj = Projectile.whoAmI;
                        bof.orbitSpeed = 0.25f; //0.1
                        bof.originalDir = new Vector2(1f, 0f).RotatedBy(rot);
                        bof.orbitDistance = 150f; //200
                        bof.orbitDir = startDir;
                    }
                }

                int featherCount2 = 10;
                for (int i = 1; i < featherCount2; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount2) * i;

                    int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                    float reverseI = featherCount2 - i;
                    if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                    {
                        bof.ParentProj = Projectile.whoAmI;
                        bof.orbitSpeed = (0.015f + (reverseI * 0.005f)) * 2f; //0.1
                        bof.originalDir = new Vector2(0f, 1f);
                        bof.orbitDistance = 25f + (50 * i); //200
                        bof.orbitDir = startDir;
                    }
                }

                for (int i = 1; i < featherCount2; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount2) * i;

                    int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                    float reverseI = featherCount2 - i;
                    if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                    {
                        bof.ParentProj = Projectile.whoAmI;
                        bof.orbitSpeed = (0.015f + (reverseI * 0.005f)) * 2f;
                        bof.originalDir = new Vector2(0f, -1f);
                        bof.orbitDistance = 25f + (50 * i); //200
                        bof.orbitDir = startDir;
                    }
                }

            }

            //SpeedUp
            float speedUpProg = Utils.GetLerpValue(0f, 1f, (float)timer / 70f, true);
            if (timer < 120)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * (Projectile.velocity.Length() + Easings.easeInQuart(speedUpProg) * 0.1f);

            //Homing
            if (timer < 90 && timer > 50 && false)
            {
                float oldSpeed = Projectile.velocity.Length();
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                float velLength = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * velLength, 0.02f);

                if (Projectile.velocity.Length() < oldSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * oldSpeed;
            }

            if (timer % 1 == 0)
            {
                float scale = 1.4f;

                SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 3f, 0.92f);

                Vector2 random = Main.rand.NextVector2CircularEdge(40f, 40f) * Main.rand.NextFloat(1f, 5f);

                Vector2 vel = random.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.25f;

                Dust star = Dust.NewDustPerfect(Projectile.Center + random, ModContent.DustType<SmallSmoke>(), vel, newColor: Color.White, Scale: scale);
                star.customData = ssb;
            }

            float timeForPopInAnim = 35; //37
            float animProgress = Math.Clamp((timer + 13) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 0.75f)) * 1f;

            float animProgress2 = Math.Clamp((float)timer / 45f, 0f, 1f);
            Projectile.rotation = MathHelper.Lerp(-2f * (Projectile.velocity.X > 0 ? 1 : -1), 0f, Easings.easeOutQuart(animProgress2));

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                GoddamnMonsoonCirc(50); //460 | 50 | 10

                DrawVanillaSwirl2(true);
            });

            DrawVanillaSwirl2(false);
            
            return false;
        }

        public void DrawVanillaSwirl2(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Swirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirl").Value; //FireSpot goes kinda crazy| same with PixelSwirl

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 origin = Swirl.Size() / 2f;

            float sinOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.2f;
            float cosOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.077f) * 0.1f;

            float startScale = sinOff;
            float endScale = 6f * cosOff;

            float scale = 1f;

            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.1f, 0f, Orb.Size() / 2f, endScale * 1f * overallScale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.35f, 0f, Orb.Size() / 2f, endScale * 0.15f * overallScale, SpriteEffects.FlipHorizontally);

            for (int i = 0; i < 12; i++) //18
            {
                float prog = 1f - ((float)i / 12f);

                //prog = Easings.easeOutSine(prog);

                //End Color <--> Start color
                Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);
                Color col = Color.Lerp(between * 1f, Color.LightSkyBlue, Easings.easeOutSine(prog));


                float alpha = prog;
                float newRot = rot + (float)Main.timeForVisualEffects * 0.025f * scale; //(i % 2 == 0 ? 1f : -1f);
                float newScale = MathHelper.Lerp(endScale, startScale, Easings.easeOutCubic(prog));

                Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha, rot + newRot, origin, newScale * 1.25f * overallScale, SpriteEffects.FlipHorizontally);

                //8
                if (i >= 8)
                    scale = scale * 1.25f;
                else
                    scale = scale * 1.15f;
            }

        }

        public void GoddamnMonsoonCirc(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value; //PixelSwirl
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new("mule".GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 1.75f;

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 150f; //200
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


                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.32f);
                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, col with { A = 0 } * 0.9f, randomRot + rotation + MathHelper.PiOver2, origin, 
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

    }

    public class FFWindOrb3 : ModProjectile
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

        float overallAlpha = 0f;
        float overallScale = 0f;

        int timer = 0;

        public int startDir = 1;
        public int playerID = -1;

        public override void AI()
        {
            if (playerID == -1)
            {
                Main.NewText("playerID == -1 | FFWindOrb");
                Projectile.active = false;
                return;
            }
            Player target = Main.player[playerID];

            if (timer == 0)
            {
                Projectile.ai[0] = Projectile.velocity.Length();

                int featherCount = 3; //3
                for (int i = 220; i < featherCount; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount) * i;

                    int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                    if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                    {
                        bof.ParentProj = Projectile.whoAmI;
                        bof.orbitSpeed = 0.25f; //0.1
                        bof.originalDir = new Vector2(1f, 0f).RotatedBy(rot);
                        bof.orbitDistance = 150f; //200
                        bof.orbitDir = startDir;
                    }
                }

                int featherCount2 = 5;
                for (int i = 1; i < featherCount2; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount2) * i;

                    int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                    float reverseI = featherCount2 - i;
                    if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                    {
                        bof.ParentProj = Projectile.whoAmI;
                        bof.orbitSpeed = 0.03f;// (0.015f + (reverseI * 0.005f)) * 0.65f; //0.1
                        bof.originalDir = new Vector2(0f, 1f);
                        bof.orbitDistance = 25f + (150 * i); //200
                        bof.orbitDir = startDir;
                    }
                }

                for (int i = 1; i < featherCount2; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount2) * i;

                    int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicOrbitingFeather>(), 1, 1);

                    float reverseI = featherCount2 - i;
                    if (Main.projectile[a].ModProjectile is BasicOrbitingFeather bof)
                    {
                        bof.ParentProj = Projectile.whoAmI;
                        bof.orbitSpeed = 0.03f;// (0.015f + (reverseI * 0.005f)) * 0.65f;
                        bof.originalDir = new Vector2(0f, -1f);
                        bof.orbitDistance = 25f + (150 * i); //200
                        bof.orbitDir = startDir;
                    }
                }

            }

            //SpeedUp
            float speedUpProg = Utils.GetLerpValue(0f, 1f, (float)timer / 70f, true);
            if (timer < 120)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * (Projectile.velocity.Length() + Easings.easeInQuart(speedUpProg) * 0.1f);

            //Homing
            if (timer < 90 && timer > 50 && false)
            {
                float oldSpeed = Projectile.velocity.Length();
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                float velLength = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * velLength, 0.02f);

                if (Projectile.velocity.Length() < oldSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * oldSpeed;
            }

            if (timer % 1 == 0)
            {
                float scale = 1.4f;

                SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 3f, 0.92f);

                Vector2 random = Main.rand.NextVector2CircularEdge(40f, 40f) * Main.rand.NextFloat(1f, 5f);

                Vector2 vel = random.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.25f;

                Dust star = Dust.NewDustPerfect(Projectile.Center + random, ModContent.DustType<SmallSmoke>(), vel, newColor: Color.White, Scale: scale);
                star.customData = ssb;
            }

            float timeForPopInAnim = 35; //37
            float animProgress = Math.Clamp((timer + 13) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 0.75f)) * 1f;

            float animProgress2 = Math.Clamp((float)timer / 45f, 0f, 1f);
            Projectile.rotation = MathHelper.Lerp(-2f * (Projectile.velocity.X > 0 ? 1 : -1), 0f, Easings.easeOutQuart(animProgress2));

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                GoddamnMonsoonCirc(50); //460 | 50 | 10

                DrawVanillaSwirl2(true);
            });

            DrawVanillaSwirl2(false);

            return false;
        }

        public void DrawVanillaSwirl2(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Swirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirl").Value; //FireSpot goes kinda crazy| same with PixelSwirl

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 origin = Swirl.Size() / 2f;

            float sinOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.2f;
            float cosOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.077f) * 0.1f;

            float startScale = sinOff;
            float endScale = 6f * cosOff;

            float scale = 1f;

            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.1f, 0f, Orb.Size() / 2f, endScale * 1f * overallScale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.35f, 0f, Orb.Size() / 2f, endScale * 0.15f * overallScale, SpriteEffects.FlipHorizontally);

            for (int i = 0; i < 12; i++) //18
            {
                float prog = 1f - ((float)i / 12f);

                //prog = Easings.easeOutSine(prog);

                //End Color <--> Start color
                Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);
                Color col = Color.Lerp(between * 1f, Color.LightSkyBlue, Easings.easeOutSine(prog));


                float alpha = prog;
                float newRot = rot + (float)Main.timeForVisualEffects * 0.025f * scale; //(i % 2 == 0 ? 1f : -1f);
                float newScale = MathHelper.Lerp(endScale, startScale, Easings.easeOutCubic(prog));

                Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha, rot + newRot, origin, newScale * 1.25f * overallScale, SpriteEffects.FlipHorizontally);

                //8
                if (i >= 8)
                    scale = scale * 1.25f;
                else
                    scale = scale * 1.15f;
            }

        }

        public void GoddamnMonsoonCirc(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value; //PixelSwirl
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new("mule".GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 1.75f;

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 150f; //200
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


                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.32f);
                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, col with { A = 0 } * 0.9f, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

    }

    public class FFWindOrb2 : ModProjectile
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

        public float endVel = 2f;// 10f

        float overallAlpha = 0f;
        float overallScale = 0f;

        int timer = 0;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override void AI()
        {
            if (timer == 0)
                Projectile.ai[0] = Projectile.velocity.ToRotation();


            float timeForVelFadeIn = 1f;
            float velFadeInProg = Math.Clamp((float)timer / timeForVelFadeIn, 0f, 1f);

            //Main.NewText(Projectile.velocity.ToRotation(), Projectile.ai[0]);

            Projectile.velocity = Projectile.ai[0].ToRotationVector2() * (endVel);

            if (endVel < 16f)
            {
                endVel *= 1.05f;
            }

            if (timer == 0)
            {
                int featherCount = 6;
                for (int i = 0; i < featherCount; i++)
                {
                    float rot = (MathHelper.TwoPi / (float)featherCount) * i;

                    float dir = Projectile.ai[0];
                    Vector2 goalPos = new Vector2(300, 0f).RotatedBy(rot).RotatedBy(dir); //325
                    Vector2 spawnPos = Projectile.Center + goalPos + dir.ToRotationVector2() * -500f; //-300

                    int a = Projectile.NewProjectile(null, spawnPos, Vector2.Zero, ModContent.ProjectileType<RelativeToProjFeather>(), 1, 1);



                    if (Main.projectile[a].ModProjectile is RelativeToProjFeather rtpf)
                    {
                        rtpf.ParentProj = Projectile.whoAmI;
                        rtpf.goalPos = goalPos;
                    }
                }

            }

            int trailCount = 7; //5
            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            //Projectile.rotation -= 0.25f;

            if (timer % 1 == 0)
            {
                float scale = 1.4f;

                SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 3f, 0.92f);

                Vector2 random = Main.rand.NextVector2CircularEdge(40f, 40f) * Main.rand.NextFloat(1f, 5f);

                Vector2 vel = random.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.25f;

                Dust star = Dust.NewDustPerfect(Projectile.Center + random, ModContent.DustType<SmallSmoke>(), vel, newColor: Color.White, Scale: scale);
                star.customData = ssb;
            }

            float timeForPopInAnim = 35; //37
            float animProgress = Math.Clamp((timer + 13) / timeForPopInAnim, 0f, 1f);

            overallScale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 0.75f)) * 1f;

            float animProgress2 = Math.Clamp((float)timer / 45f, 0f, 1f);
            Projectile.rotation = MathHelper.Lerp(-2f * (Projectile.velocity.X > 0 ? 1 : -1), 0f, Easings.easeOutQuart(animProgress2));


            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                GoddamnMonsoonCirc(50); //460 | 50 | 10

                DrawVanillaSwirl2(false);
            });

            DrawVanillaSwirl2(true);

            return false;
        }

        public void DrawVanillaSwirl2(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Swirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirl").Value; //FireSpot goes kinda crazy| same with PixelSwirl

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 origin = Swirl.Size() / 2f;

            float sinOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.2f;
            float cosOff = 1f + MathF.Sin((float)Main.timeForVisualEffects * 0.077f) * 0.1f;

            float startScale = sinOff;
            float endScale = 6f * cosOff;

            float scale = 1f;


            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.1f, 0f, Orb.Size() / 2f, endScale * 1f * overallScale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.35f, 0f, Orb.Size() / 2f, endScale * 0.15f * overallScale, SpriteEffects.FlipHorizontally);

            for (int i = 0; i < 12; i++) //18
            {
                float prog = 1f - ((float)i / 12f);

                //prog = Easings.easeOutSine(prog);

                //End Color <--> Start color
                Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);
                Color col = Color.Lerp(between * 1f, Color.LightSkyBlue, Easings.easeOutSine(prog));


                float alpha = prog;
                float newRot = rot +(float)Main.timeForVisualEffects * 0.025f * scale; //(i % 2 == 0 ? 1f : -1f);
                float newScale = MathHelper.Lerp(endScale, startScale, Easings.easeOutCubic(prog));

                Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha, rot + newRot, origin, newScale * 1.25f * overallScale, SpriteEffects.FlipHorizontally);

                //8
                if (i >= 8)
                    scale = scale * 1.25f;
                else
                    scale = scale * 1.15f;
            }

        }

        public void GoddamnMonsoonCirc(int count = 50)
        {
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/Feather").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value; //PixelSwirl
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new("mule".GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 1.75f;

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 270f; //120
            for (int i = 0; i < count * overallScale; i++)
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


                Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.32f);
                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, col with { A = 0 } * 0.9f * overallScale, randomRot + rotation + MathHelper.PiOver2, origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

    }

    public class LineBarrier : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

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

        public bool startBlocking = false;
        public bool startFade = false;

        int timer = 0;

        public int dir = 1;
        public override void AI()
        {

            if (startBlocking)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 spawnVec = new Vector2(Main.rand.NextFloat(-1100f, 1100f), Main.rand.NextFloat(-12f, 12f)).RotatedBy(Projectile.rotation);

                    float velVal = Main.rand.NextFloat(5f, 18f) * dir;
                    Vector2 vel = new Vector2(velVal, 0f).RotatedBy(Projectile.rotation);

                    Vector2 windDustSpawnPosition = Projectile.Center + spawnVec;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, vel * 1f, newColor: Color.SkyBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(1f, 2f));
                    wind.noGravity = true;
                }

                for (int i = 0; i < 1; i++)
                {
                    Vector2 spawnVec = new Vector2(Main.rand.NextFloat(-1100f, 1100f), Main.rand.NextFloat(-12f, 12f)).RotatedBy(Projectile.rotation);

                    float velVal = Main.rand.NextFloat(5f, 18f) * dir;
                    Vector2 vel = new Vector2(velVal * 3f, 0f).RotatedBy(Projectile.rotation);

                    Vector2 windDustSpawnPosition = Projectile.Center + spawnVec;

                    Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, ModContent.DustType<WindLine>(), vel * 1f, newColor: Color.DodgerBlue with { A = 0 } * 1f, Scale: Main.rand.NextFloat(0.35f, 0.55f));
                    WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.65f, 2f, 1f, true);
                    wind.customData = wlb;

                }
            }

            timer++;
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glowThick = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Vector2 vec2Scale = new Vector2(100f, 0.75f);

            Color col = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.25f);

            float sineWidthAddition1 = 1f + (MathF.Sin((float)Main.timeForVisualEffects * 0.08f) * 0.25f);
            float sineWidthAddition2 = 1f + (MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.25f);

            Vector2 vec2Scale1 = new Vector2(15f, 0.1f * sineWidthAddition1);
            Vector2 vec2Scale2 = new Vector2(15f, 0.04f * sineWidthAddition2);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Main.EntitySpriteDraw(glowThick, drawPos, null, col with { A = 0 }, Projectile.rotation, glowThick.Size() / 2f, vec2Scale1, 0);

                if (startBlocking)
                    Main.EntitySpriteDraw(glowThick, drawPos, null, Color.White with { A = 0 }, Projectile.rotation, glowThick.Size() / 2f, vec2Scale2, 0);
            });

            return false;
        }

    }

    public class WindDirShotNado : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }

        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 56; //64

            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 17000;
        }


        public Vector2 orbitVector = new Vector2(200f, 0f);
        public float endingShotDir = 0f;
        
        public float orbitTime = 80;
        public float spinInRadialDistance = MathHelper.Pi * 2f;
        public float startDashSpeed = 4f;
        public float lateDashSpeed = 29f;

        float drawAlpha = 0f;
        float drawScale = 0f;

        int timer = 0;
        public int advancer = 0;

        public int playerID = -1;

        public override void AI()
        {
            if (timer == 0)
            Projectile.scale *= 1f;

            #region behavior
            if (playerID == -1)
            {
                Main.NewText("playerID == -1 | WindDirShotNado");
                Projectile.active = false;
                return;
            }
            Player target = Main.player[playerID];

            if (target.active == false)
                Projectile.Kill();

            float spinInProgress = (float)timer / orbitTime;
            if (timer <= orbitTime)
            {
                Projectile.damage = 0;

                float rot = MathHelper.Lerp(spinInRadialDistance, 0f, Easings.easeOutQuart(spinInProgress));
                Vector2 goalPos = orbitVector.RotatedBy(rot) * Easings.easeOutCirc(spinInProgress);
                Projectile.Center = goalPos + target.Center;

                Projectile.velocity = Vector2.Zero;

                if (timer == orbitTime)
                {
                    Projectile.velocity = endingShotDir.ToRotationVector2() * startDashSpeed;
                    Projectile.timeLeft = 100;
                }
            }
            else
            {
                Projectile.damage = 10;

                Projectile.velocity = (Projectile.velocity + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 1f);
                if (Projectile.velocity.Length() > lateDashSpeed)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * lateDashSpeed;
            }
            #endregion

            #region draw+dust

            int trailCount = 14; //7
            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            //AlphaScale
            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            drawScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 1.5f));

            drawAlpha = Math.Clamp(MathHelper.Lerp(drawAlpha, 1.5f, 0.09f), 0f, 1f);


            
            //Vanilla weather pain dust
            if (timer % 3 == 0 && Main.rand.NextBool())
            {
                int num6 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 16, Projectile.velocity.X, Projectile.velocity.Y, 120, default(Color), 0.35f);
                Main.dust[num6].noGravity = true;
                Main.dust[num6].fadeIn = 0.45f;
                Main.dust[num6].velocity = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -2f) + Projectile.velocity * 0.75f;
                for (int j = 0; j < 1; j++)
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 16, Projectile.velocity.X, Projectile.velocity.Y, 60, default(Color), 0.35f);
                    dust.noGravity = true;
                    dust.fadeIn = 0.35f;
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f) * 0.2f + new Vector2(0f, -0.4f) + Projectile.velocity * 1.5f;
                    dust.position -= Projectile.velocity * 3f;
                }
            }

            if (timer % 1 == 0 && timer > orbitTime)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f) - Projectile.velocity * 0.5f;

                float dustScale = Main.rand.NextFloat(0.5f, 1f);

                Dust wind = Dust.NewDustPerfect(dustPos, 176, dustVel * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 0.5f, Scale: dustScale); //dust176
                wind.noGravity = true;
            }

            //Anim
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }

            Projectile.rotation = 0f + Projectile.velocity.X * 0.02f;
            #endregion

            timer++;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/FeatheredFoe/Assets/FFTornado").Value;
            Texture2D Bloom = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Tex.Frame(1, Main.projFrames[Projectile.type], frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.Lerp(Color.DodgerBlue, Color.SkyBlue, progress) * progress * progress * drawAlpha;

                float size2 = (0.5f + (progress * 0.75f)) * Projectile.scale * drawScale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(Tex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.25f,
                        previousRotations[i], TexOrigin, size2, se);
            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(Tex, drawPos + Main.rand.NextVector2Circular(5f, 5f), sourceRectangle,
                    Color.LightSkyBlue with { A = 0 } * 0.35f * drawAlpha, Projectile.rotation, TexOrigin, Projectile.scale * 1.05f * drawScale, se);
            }

            //Bloom
            Vector2 bloomScale = new Vector2(0.7f, 0.9f);
            Main.EntitySpriteDraw(Bloom, drawPos, null, Color.DeepSkyBlue with { A = 0 } * drawAlpha * 0.25f, Projectile.rotation, Bloom.Size() / 2f, bloomScale * Projectile.scale * drawScale, se);
            Main.EntitySpriteDraw(Bloom, drawPos, null, Color.SkyBlue with { A = 0 } * drawAlpha * 0.15f, Projectile.rotation, Bloom.Size() / 2f, bloomScale * Projectile.scale * drawScale * 0.8f, se);

            //Main
            Main.EntitySpriteDraw(Tex, drawPos, sourceRectangle, lightColor * drawAlpha, Projectile.rotation, TexOrigin, Projectile.scale * drawScale, se);
            return false;
        }

    }
}