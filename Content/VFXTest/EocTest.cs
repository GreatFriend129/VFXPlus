using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Microsoft.CodeAnalysis;
using Terraria.GameContent.Drawing;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using Terraria.Utilities;
using Terraria.GameContent;
using Microsoft.Build.Evaluation;
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.Dusts;
using System.Collections.Generic;
using VFXPLus.Common;
using Terraria.Graphics;
using Terraria.Map;

namespace VFXPlus.Content.VFXTest
{
    public class EocTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 111800;
        }

        bool phase2 = true;
        int timer = 0;
        public override void AI()
        {
            int trailCount = 14;
            previousRotations.Add(Projectile.rotation);
            previousPostions.Add(Projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            Projectile.rotation = (Main.player[Main.myPlayer].Center - Projectile.Center).ToRotation();

            phase2 = true;

            if (timer % 5 == 0)
                frame = (frame + 1) % 3;

            Player owner = Main.player[Projectile.owner];
            BasicMovementVariant3(owner.Center + new Vector2(0f, -200f), 4f);
            timer++;
        }

        void BasicMovementVariant3(Vector2 goalPos, float moveSpeed = 6f, float clampDistance = 240f)
        {
            Projectile.velocity *= 0.875f;

            Vector2 trueGoal = goalPos - Projectile.Center;
            //Vector2 lerpGoal = Vector2.Lerp(Projectile.Center, goalPos, 0.8f);

            //Vector2 goTo = trueGoal;
            float speed = moveSpeed * MathHelper.Clamp(trueGoal.Length() / clampDistance, 0, 1);
            if (trueGoal.Length() < speed)
            {
                speed = trueGoal.Length();
            }
            Projectile.velocity += trueGoal.SafeNormalize(Vector2.Zero) * speed;

        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        int frame = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 ballPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 drawPos = ballPos + new Vector2(-20f, 0f).RotatedBy(rot);

            Texture2D Ball = CommonTextures.SoftGlow.Value;

            Texture2D MainTex = Mod.Assets.Request<Texture2D>("Content/VFXTest/BasicEoc").Value;
            Texture2D Vein = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocVein").Value;
            Texture2D VeinGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocVeinGlow").Value;
            Texture2D FullGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocFullGlow").Value;

            int frameHeight = MainTex.Height / 6;
            int startY = frameHeight * (frame + (phase2 ? 3 : 0));
            Rectangle sourceRectangle = new Rectangle(0, startY, MainTex.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            DrawAfterImage();

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(FullGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                    Color.Red with { A = 0 } * 0.2f, rot, origin, 1.05f, 0f);
            }

            Main.EntitySpriteDraw(Ball, ballPos + new Vector2(-5f, 0f).RotatedBy(rot), null, Color.Red with { A = 0 } * 0.55f, rot, Ball.Size() / 2f, 0.35f, 0f);

            Main.EntitySpriteDraw(MainTex, drawPos, sourceRectangle, lightColor, rot, origin, 1.05f, 0f);

            Main.EntitySpriteDraw(VeinGlow, drawPos, sourceRectangle, Color.DarkRed with { A =  0 }, rot, origin, 1.05f, 0f);
            Main.EntitySpriteDraw(VeinGlow, drawPos, sourceRectangle, Color.Red with { A = 0 } * 0.5f, rot, origin, 1.05f, 0f);

            Main.EntitySpriteDraw(Vein, drawPos, sourceRectangle, lightColor, rot, origin, 1.05f, 0f);


            return false;
        }

        public void DrawAfterImage()
        {
            Texture2D FullGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocFullGlow").Value;
            int frameHeight = FullGlow.Height / 6;
            int startY = frameHeight * (frame + (phase2 ? 3 : 0));
            Rectangle sourceRectangle = new Rectangle(0, startY, FullGlow.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.Crimson, Color.DarkRed, 1f - Easings.easeInCubic(progress)) * progress;

                float size1 = Projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + new Vector2(-20f, 0f).RotatedBy(previousRotations[i]);

                Main.EntitySpriteDraw(FullGlow, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.25f,
                    previousRotations[i], origin, size1, 0f);
            }
        }
    }

    public class EoCDashTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 111800;
        }

        bool phase2 = true;
        int timer = 0;
        public override void AI()
        {
            int trailCount = 14;
            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center + Projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            phase2 = true;

            if (timer % 5 == 0)
                frame = (frame + 1) % 3;

            Player owner = Main.player[Projectile.owner];

            EyeSwordDash(owner);

            timer++;
        }

        int advancer = 0;
        float storedRotation = 0f;
        Vector2 storedPosForLerp = Vector2.Zero;
        Vector2 storedPos = Vector2.Zero;
        public void EyeSwordDash(Player player)
        {
            int timeToAim = 60;
            float dashTime = 30f; //40f
            float dashDistance = 800f;

            if (advancer == 0)
            {
                if (timer == 0) 
                    storedPos = (Projectile.Center - player.Center).SafeNormalize(Vector2.UnitX).RotateRandom(3.5f);

                //NPC position lerps to a distance that shrinks with time
                Vector2 vecToPlayer = storedPos * (550 * (1 - (timer * 0.0025f)));

                Projectile.Center = Vector2.Lerp(Projectile.Center, player.Center + vecToPlayer, Math.Clamp(timer * 0.005f, 0, 0.8f));
                Projectile.rotation = (player.Center - Projectile.Center).ToRotation();

                if (timer == timeToAim)
                {
                    storedRotation = Projectile.rotation;
                    storedPosForLerp = Projectile.Center;
                    advancer++;

                    timer = -1;

                    previousPositions.Clear();
                    previousRotations.Clear();
                }
            }
            else if (advancer == 1)
            {
                float progress = Math.Clamp(timer / dashTime, 0f, 1f);

                Projectile.Center = Vector2.Lerp(storedPosForLerp, storedPosForLerp + storedRotation.ToRotationVector2() * dashDistance, Easings.easeOutQuart(progress)); //expo


                //Spawn Dust
                if (progress > 0f && Easings.easeOutCubic(progress) < 0.85f)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (Main.rand.NextBool())
                        {
                            Vector2 yOffset = new Vector2(0f, 1f).RotatedBy(storedRotation) * Main.rand.NextFloat(-50f, 50f);

                            Vector2 fakeNPCVel = storedRotation.ToRotationVector2() * -10f * Main.rand.NextFloat(0.75f, 1.25f);

                            Color dustCol = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.5f, 1f));

                            Dust p = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f) + yOffset, ModContent.DustType<WindLine>(), fakeNPCVel,
                                newColor: dustCol, Scale: Main.rand.NextFloat(0.5f, 0.75f));

                            WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 15, ShrinkYScalePower: 0.75f, 1f, 0.5f, true);
                            wlb.drawWhiteCore = false;
                            p.customData = wlb;
                        }
                    }
                }

                if (timer == dashTime)
                {
                    advancer = 0;
                    timer = -1;

                    previousPositions.Clear();
                    previousRotations.Clear();
                }
            }
        }


        bool drawAfterImage = false;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        int frame = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 ballPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 drawPos = ballPos + new Vector2(-20f, 0f).RotatedBy(rot);

            Texture2D Ball = CommonTextures.SoftGlow.Value;

            Texture2D MainTex = Mod.Assets.Request<Texture2D>("Content/VFXTest/BasicEoc").Value;
            Texture2D Vein = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocVein").Value;
            Texture2D VeinGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocVeinGlow").Value;
            Texture2D FullGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocFullGlow").Value;

            int frameHeight = MainTex.Height / 6;
            int startY = frameHeight * (frame + (phase2 ? 3 : 0));
            Rectangle sourceRectangle = new Rectangle(0, startY, MainTex.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            DrawAfterImage();

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(FullGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                    Color.Red with { A = 0 } * 0.2f, rot, origin, 1.05f, 0f);
            }

            //Main.EntitySpriteDraw(FullGlow, drawPos, null, Color.Red with { A = 0 }, 0f, MainTex.Size() / 2f, 1.05f, 0f);

            Main.EntitySpriteDraw(Ball, ballPos + new Vector2(-5f, 0f).RotatedBy(rot), null, Color.Red with { A = 0 } * 0.55f, rot, Ball.Size() / 2f, 0.35f, 0f);

            Main.EntitySpriteDraw(MainTex, drawPos, sourceRectangle, lightColor, rot, origin, 1.05f, 0f);

            Main.EntitySpriteDraw(VeinGlow, drawPos, sourceRectangle, Color.DarkRed with { A = 0 }, rot, origin, 1.05f, 0f);
            Main.EntitySpriteDraw(VeinGlow, drawPos, sourceRectangle, Color.Red with { A = 0 } * 0.5f, rot, origin, 1.05f, 0f);

            Main.EntitySpriteDraw(Vein, drawPos, sourceRectangle, lightColor, rot, origin, 1.05f, 0f);


            return false;
        }

        public void DrawAfterImage()
        {
            Texture2D FullGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/BasicEoc").Value;
            int frameHeight = FullGlow.Height / 6;
            int startY = frameHeight * (frame + (phase2 ? 3 : 0));
            Rectangle sourceRectangle = new Rectangle(0, startY, FullGlow.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            float AEAlpha = advancer == 1 ? 0.45f : 0.25f;

            //After-Image
            for (int i = 0; i < previousRotations.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.Crimson, Color.DarkRed, 1f - Easings.easeInCubic(progress)) * progress;

                float size1 = Projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition + new Vector2(-20f, 0f).RotatedBy(previousRotations[i]);

                Main.EntitySpriteDraw(FullGlow, AfterImagePos, sourceRectangle, col with { A = 0 } * AEAlpha,
                    previousRotations[i], origin, size1, 0f);
            }
        }

    }

}