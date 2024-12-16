using System;
using System.Collections.Generic;
using Terraria.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using VFXPlus.Common;

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
}