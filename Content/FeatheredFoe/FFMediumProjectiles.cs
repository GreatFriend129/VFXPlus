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
            Projectile.timeLeft = 500;

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

            Projectile.velocity = new Vector2(0f, -13f);
            
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
            DrawWindVortex(-1, Color.White * 0.33f);

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


            DrawWindVortex(1, Color.White * 0.7f);
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

            for (int i = 0; i < 60; i++) //60
            {

                Texture2D texture;
                Rectangle frame;
                Vector2 scale;
                float rotation = MathHelper.PiOver2;
                if (r.NextFloat() < 0.1f)
                {
                    texture = windTexture;
                    frame = texture.Bounds;
                    scale = new(0.3f, 0.66f); //0.3 0.66
                }
                else
                {
                    texture = dustTexture;
                    frame = texture.Frame(verticalFrames: 3, frameY: r.Next(3));
                    scale = new(1f, 1f);
                    rotation += speedTime * NextFloatF(r, 0.8f, 1.2f);
                }
                var origin = frame.Size() / 2f;
                float speed = NextFloatF(r, 0.8f, 4f);
                //float progress = (speedTime * speed + NextFloatF(r, 0f, 1f)) % 3f;
                float progress = (speedTime * speed + r.NextFloat()) % 3f;

                float scaleWave = MathF.Sin(progress * MathHelper.Pi);
                float waveDistance = NextFloatF(r, 40f, 120f); //40 120 overall distance of the ring

                //float yWave = Helper.Wave(Main.GlobalTimeWrappedHourly * 2f, 0.5f, 1.5f);
                float time = speedTime * 2f;
                float min = 0.5f;
                float max = 1.5f;

                float yWave = min + ((float)Math.Sin(time) + 1f) / 2f * (max - min);

                float yOffset = scaleWave * waveDistance * 0.3f * yWave; //0.3
                float xWave = MathF.Sin(progress * MathHelper.Pi - MathHelper.PiOver2) * yDir;
                var drawPosition = Projectile.Center + new Vector2(xWave * waveDistance, NextFloatF(r, -20f, 14f) + yOffset * yDir); //-20 14 | -120

                Main.spriteBatch.Draw(
                    texture,
                    (drawPosition - Main.screenPosition).Floor(),
                    frame,
                    color,
                    rotation - xWave / 3f * yWave * yDir,
                    origin,
                    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f,
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

    public class ExplodingExplosionTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public int timer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.timeLeft = 20000;
            Projectile.penetrate = -1;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (timer == 1)
                Projectile.rotation = Main.rand.NextFloat(6.28f);

            if (timer > 100)
            {
                if (timer == 101)
                    pulseVal = 1f;

                Projectile.frameCounter++;
                if (Projectile.frameCounter >= 3)
                {
                    if (Projectile.frame == 6)
                        timer = 0;

                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
                }
            }

            pulseVal *= 0.8f;

            timer++;
        }

        float pulseVal = 0f;

        float alpha = 1f;
        float scale = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D ExploA = Mod.Assets.Request<Texture2D>("Assets/Anim/GrayscaleVanillaExplode").Value;
            Texture2D ExploB = Mod.Assets.Request<Texture2D>("Assets/Anim/VanillaExplodeWhiteGlow").Value;
            Texture2D ExploC = Mod.Assets.Request<Texture2D>("Assets/Anim/VanillaExplodeWhite").Value;
            Texture2D ExploD = Mod.Assets.Request<Texture2D>("Assets/Anim/BlueFlareDarkGlowPMA").Value;

            int frameHeight = ExploD.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle sourceRectangle = new Rectangle(0, startY, ExploA.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;


            
            Main.spriteBatch.Draw(ExploB, Projectile.Center - Main.screenPosition, sourceRectangle, Color.DodgerBlue with { A = 0 } * (2f * pulseVal), Projectile.rotation, origin, 1.75f * (1f - pulseVal), 0, 0f);


            Main.spriteBatch.Draw(ExploD, Projectile.Center - Main.screenPosition, sourceRectangle, Color.Black * 0.4f, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(ExploD, Projectile.Center - Main.screenPosition, sourceRectangle, Color.White with { A = 0 }, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);


            /*

                        //Color glowColor = Color.DeepSkyBlue;
            //glowColor.A = 0;

            //Color glowColor2 = Color.White;
            //glowColor2.A = 0;

            // Get this frame on texture
            Rectangle sourceRectangle = new Rectangle(0, startY, Explo.Width, frameHeight);


            Vector2 origin = sourceRectangle.Size() / 2f;

            Vector2 scale12 = new Vector2(1f, 1f);

            Main.spriteBatch.Draw(Explo, Projectile.Center - Main.screenPosition, sourceRectangle, Color.Black * 0.4f, Projectile.rotation, origin, scale12, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Explo, Projectile.Center - Main.screenPosition, sourceRectangle, glowColor2, Projectile.rotation, origin, scale12, SpriteEffects.None, 0f);
            */
            return false;
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
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha);
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
            int trailCount = 7; //5
            previousRotations.Add(Projectile.rotation);
            previousPostions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            Projectile.rotation -= 0.25f;

            Projectile.velocity = new Vector2(0f, 0f);


            if (timer % 1 == 0)
            {
                float scale = 1.4f;

                SmallSmokeBehavior ssb = new SmallSmokeBehavior(ColorIntensity: 4f, 0.92f);

                Vector2 random = Main.rand.NextVector2CircularEdge(40f, 40f) * Main.rand.NextFloat(1f, 5f);

                Vector2 vel = random.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * Main.rand.NextFloat(4f, 14f) * 0.25f;

                Dust star = Dust.NewDustPerfect(Projectile.Center + random, ModContent.DustType<SmallSmoke>(), vel, newColor: Color.White, Scale: scale);
                star.customData = ssb;
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVanillaSwirl(true);
                GoddamnMonsoonCirc(50); //460 | 50 | 10
                DrawVanillaSwirl2(true);
            });

            DrawVanillaSwirl2(false);
            
            return false;
        }

        public void DrawVanillaSwirl(bool giveUp = true)
        {
            if (giveUp) return;

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Extra_50").Value;
            Texture2D Tex2 = Mod.Assets.Request<Texture2D>("Content/VFXTest/PinkSwirl").Value;
            Texture2D Tex3 = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaSwirl").Value;

            float ProjScale = Projectile.scale * 1f;


            Texture2D TexA = Tex3;// Tex3;// TextureAssets.Projectile[641].Value;
            Texture2D TexB = Tex;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Vector2 TexAOrigin = TexA.Size() / 2f;
            Vector2 TexBOrigin = TexB.Size() / 2f;

            Color projectileColor = Lighting.GetColor((int)((double)Projectile.position.X + (double)Projectile.width * 0.5) / 16, (int)(((double)Projectile.position.Y + (double)Projectile.height * 0.5) / 16.0));
            Color color155 = Projectile.GetAlpha(projectileColor);

            float rot = Projectile.rotation;

            Color color158 = color155 * 0.8f;
            color158.A /= 2;

            Color color159 = Color.Lerp(color155, Color.Black, 0.5f);
            color159.A = color155.A;
            float num317 = 0.95f + (rot * 0.75f).ToRotationVector2().Y * 0.1f;
            color159 *= num317;
            float scale22 = 0.6f + ProjScale * 0.6f * num317;

            Main.EntitySpriteDraw(TexB, drawPos, null, color159, -rot + 0.35f, TexBOrigin, scale22, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(TexB, drawPos, null, color155, -rot, TexBOrigin, ProjScale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(TexA, drawPos, null, color158 with { A = 0 }, -rot * 0.7f, TexAOrigin, ProjScale, SpriteEffects.FlipHorizontally);
            Main.EntitySpriteDraw(TexA, drawPos, null, color158 with { A = 0 }, -rot * -0.7f, TexAOrigin, ProjScale, SpriteEffects.None);
            Main.EntitySpriteDraw(TexB, drawPos, null, color155 with { A = 0 } * 0.8f, rot * 0.5f, TexBOrigin, ProjScale * 0.9f, SpriteEffects.None);
            color155.A = 0;
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
            float endScale = 7.47f * cosOff;

            float scale = 1f;

            Main.EntitySpriteDraw(Swirl, drawPos, null, Color.Black * 0.35f, (float)Main.timeForVisualEffects * 0.08f, origin, endScale * 0.25f, SpriteEffects.FlipHorizontally);

            for (int i = 0; i < 12; i++) //18
            {
                float prog = 1f - ((float)i / 12f);

                //prog = Easings.easeOutSine(prog);

                //End Color <--> Start color
                Color between = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.95f);
                Color col = Color.Lerp(between * 1f, Color.LightSkyBlue, prog);

                //Color col = Color.Lerp(Color.DeepSkyBlue * 0.5f, Color.LightSkyBlue * 1f, prog);

                float alpha = prog;

                float newRot = (float)Main.timeForVisualEffects * 0.025f * scale;

                //
                //if (i < 1)
                //Main.EntitySpriteDraw(Swirl, drawPos, null, Color.Black * alpha, newRot, origin, scale * 1f, SpriteEffects.FlipHorizontally);

                float newScale = MathHelper.Lerp(endScale, startScale, Easings.easeOutCubic(prog));

                Main.EntitySpriteDraw(Swirl, drawPos + new Vector2(0f * i, 0f), null, col with { A = 0 } * alpha, newRot, origin, newScale * 1.25f, SpriteEffects.FlipHorizontally);




                //8
                if (i >= 8)
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

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 1120f; //120
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
            //Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value;
            Texture2D windTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_89").Value; //PixelSwirl
            Texture2D dustTexture = Mod.Assets.Request<Texture2D>("Content/Dusts/Textures/Basic").Value;

            FastRandom r = new(Main.player[Projectile.owner].name.GetHashCode());
            float speedTime = Main.GlobalTimeWrappedHourly * 2f;

            float minRange = 40f; //40f | 240 920 for full screen
            float maxRange = 220f; //120
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

                Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.SkyBlue with { A = 0 } * 0.9f, randomRot + rotation + MathHelper.PiOver2, origin,
    new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

                //Main.EntitySpriteDraw(texture, drawPosition - Main.screenPosition, frame, Color.White, randomRot + rotation + MathHelper.PiOver2, origin,
    //new Vector2(scale.X * scaleWave * scaleWave, scale.Y * scaleWave) * 3f, SpriteEffects.None);

            }
        }

        public float NextFloatFastRandom(FastRandom random, float min, float max)
        {
            return min + random.NextFloat() * (max - min);
        }

    }

}