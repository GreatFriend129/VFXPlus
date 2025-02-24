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
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.GameContent.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Arrows
{
    public class HolyArrowOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.HolyArrow);
        }

        int trailOffsetAmount = Main.rand.Next(-1, 2);
        int dustRandomOffsetTime = Main.rand.Next(0, 3);

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 18 + trailOffsetAmount;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

           

            float EU = 1f + projectile.extraUpdates;

            float fadeInTime = Math.Clamp((timer + 7f * EU) / 20f * EU, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;

            #region vanillaAI

            if (timer % 2 == 0 && Main.rand.NextBool(4))
            {
                int num208 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 40);
                Main.dust[num208].noGravity = true;
                Main.dust[num208].scale = 0.9f;
                Main.dust[num208].velocity *= 0.5f;
                Main.dust[num208].velocity -= projectile.velocity * 0.15f;

            }

            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 15f)
            {
                projectile.ai[0] = 15f;
                projectile.velocity.Y += 0.1f;
            }

            if (projectile.type != 344 && projectile.type != 498)
            {
                projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            }

            if (projectile.velocity.Y < -16f)
            {
                projectile.velocity.Y = -16f;
            }
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            #endregion



            return false;
        }

        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count - 1; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    //Start End
                    Color col = Color.HotPink * progress;

                    float size1 = 1f * progress * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.5f,
                        previousRotations[i], TexOrigin, size1 * overallScale, SE);

                    Color betweenPink = Color.Lerp(Color.HotPink, Color.Pink, 0.75f);
                    if (i > 1)
                    {
                        float middleProg = (float)(i - 1) / previousPostions.Count;

                        float size2 = (0.5f + (0.5f * progress));
                        Vector2 vec2Scale = new Vector2(2.5f, 1f * size2) * overallScale * projectile.scale * 0.5f;
                        Main.EntitySpriteDraw(flare, AfterImagePos, null, betweenPink with { A = 0 } * 0.2f * middleProg,
                            previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SE);
                    }
                }
            }

            Main.EntitySpriteDraw(orb, drawPos, null, Color.HotPink with { A = 0 } * 0.15f, projectile.rotation, orb.Size() / 2, new Vector2(0.35f, 0.6f) * overallScale, SE);

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 }, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;
        }


    }

    public class HolyArrowStarOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.HallowStar);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            Vector2 previousPosition = projectile.Center;

            currentRot = projectile.velocity.ToRotation();

            int trailCount = 34;
            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.33f);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.66f);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            #region dust

            if (timer % 3 == 0 && Main.rand.NextBool(2) && timer > 3)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.HotPink, Scale: 0.6f);

                Dust dust57 = de;
                Dust dust212 = dust57;
                dust212.velocity *= 0.45f;
                dust57 = de;
                dust212 = dust57;
                dust212.velocity += currentRot.ToRotationVector2() * 6f;

            }

            if (timer % 3 == 0 && timer > 8f && false)
            {
                Vector2 sideOffset = new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(projectile.velocity.ToRotation());
                //Vector2 vel = -velDir * velPower;

                //Dust line = Dust.NewDustPerfect(projectile.Center + sideOffset, ModContent.DustType<MuraLineBasic>(), vel, 255,
                //    newColor: Color.DeepPink * 0.35f, Scale: Main.rand.NextFloat(0.35f, 0.5f) * 0.75f);

                //line.customData = new MuraLineBehavior(new Vector2(1f, 1f), WhiteIntensity: 0.6f);
            }

            #endregion

            timer++;


            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.04f), 0f, 1f);
            //overallScale = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.05f), 0f, 1f);

            float fadeInTime = Math.Clamp((timer + 3f) / 30f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            projectile.localAI[0] += 1f;

            //Tile collide slightly earlier than vanilla to stop the star going slightly into ground
            if (projectile.Center.Y > projectile.ai[1] - 15)
                projectile.tileCollide = true;

            return true;
        }


        float overallScale = 0f;
        float overallAlpha = 0f;
        float currentRot = 0f;
        public List<float> previousVelRots = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            CerobaStyleDraw(projectile);

            if (previousPostions.Count > 0)
                DrawProjWithStarryTrail(projectile, lightColor, 0f);
            return false;
        }

        public void CerobaStyleDraw(Projectile projectile)
        {
            float adjustedScale = projectile.scale * 1.25f * overallScale;
            
            Texture2D FireBall = CommonTextures.FireBallBlur.Value;
            Texture2D FireBallPixel = CommonTextures.Extra_91.Value;

            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;
            Texture2D CrispStar = CommonTextures.CrispStarPMA.Value;

            Texture2D VStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;
            Texture2D VStarBlack = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStarBlackBG").Value;

            Vector2 off = (currentRot).ToRotationVector2() * -25f * adjustedScale;

            Vector2 pos = projectile.Center - Main.screenPosition;
            float rot = currentRot + MathHelper.PiOver2;

            float thisAlpha = overallAlpha * 0.15f;

            Color orbCol1 = Color.MediumPurple * 0.75f;//  Color.Pink * 0.75f;
            Color orbCol2 = Color.Purple * 0.525f;//Color.HotPink * 0.525f;
            Color orbCol3 = Color.Purple * 0.375f;//Color.DeepPink * 0.375f;

            float scale1 = 0.75f;
            float scale2 = 1.6f;
            float scale3 = 2.5f;

            Main.EntitySpriteDraw(Glow, pos, null, orbCol1 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale1 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol2 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale2 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol3 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale3 * 0.55f, SpriteEffects.None);

            Vector2 starScale = new Vector2(0.85f, 1.45f) * (1f - overallScale);
            Main.EntitySpriteDraw(CrispStar, pos, null, Color.HotPink with { A = 0 }, rot, CrispStar.Size() / 2f, 1.4f * starScale, SpriteEffects.None);
            Main.EntitySpriteDraw(CrispStar, pos, null, Color.White with { A = 0 }, rot, CrispStar.Size() / 2f, 0.55f * starScale, SpriteEffects.None); //0.6

            #region after image
            if (previousVelRots != null && previousPostions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i++)
                {
                    float progress = (float)i / previousVelRots.Count;

                    float size = (1f - (progress * 0.5f)) * adjustedScale;

                    float colVal = progress;

                    Color col = Color.Lerp(Color.MediumPurple * 0.75f, Color.HotPink, progress) * progress * 0.75f;

                    float size2 = (1f - (progress * 0.15f)) * adjustedScale;

                    Main.EntitySpriteDraw(FireBallPixel, previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(4f, 4f), null, col with { A = 0 } * 1.15f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                    Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                    Main.EntitySpriteDraw(FireBall, previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(4f, 4f), null, col with { A = 0 } * 1.25f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
                }

            }
            #endregion

            Vector2 v2scale = new Vector2(1.25f, 1f);

            //black
            //Main.EntitySpriteDraw(FireBallPixel, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.Black * thisAlpha * 0.5f, rot, FireBallPixel.Size() / 2f, adjustedScale * v2scale, SpriteEffects.None);


            Main.EntitySpriteDraw(FireBall, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink with { A = 0 } * thisAlpha, rot, FireBall.Size() / 2f, adjustedScale * v2scale, SpriteEffects.None);

            //Pink Star
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            for (int i = 0; i < 6; i++)
            {
                Color col = Color.MediumPurple;
                Main.EntitySpriteDraw(VStar, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), null, col with { A = 0 } * 0.8f * thisAlpha, projectile.rotation, VStar.Size() / 2f, adjustedScale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(VStar, drawPos, null, Color.LightPink, projectile.rotation, VStar.Size() / 2f, adjustedScale, SpriteEffects.None);
            //Main.EntitySpriteDraw(VStarBlack, drawPos, null, Color.White with { A = 0 } * 1f, projectile.rotation, VStarBlack.Size() / 2f, scale * 1f, SpriteEffects.None);

            Main.EntitySpriteDraw(VStar, drawPos, null, Color.White with { A = 0 }, projectile.rotation, VStar.Size() / 2f, adjustedScale, SpriteEffects.None);
        }

        private void DrawProjWithStarryTrail(Projectile proj, Color projectileColor, SpriteEffects dir)
        {
            Color color = new Color(255, 255, 255, projectileColor.A - 255);
            Vector2 vector = proj.velocity;
            Color color2 = Color.Blue * 0.1f;
            Vector2 spinningpoint = new Vector2(0f, -4f);
            float num = 0f;
            float t = vector.Length();
            float num2 = Utils.GetLerpValue(3f, 5f, t, clamped: true);
            bool flag = true;
            if (true)
            {
                vector = proj.position - previousPostions[0];
                float num3 = vector.Length();
                if (num3 == 0f)
                {
                    vector = Vector2.UnitY;
                }
                else
                {
                    vector *= 5f / num3;
                }
                Vector2 origin = Main.MouseWorld;// new Vector2(proj.ai[0], proj.ai[1]);
                Vector2 center = Main.player[proj.owner].Center;
                float lerpValue = Utils.GetLerpValue(0f, 120f, origin.Distance(center), clamped: true);
                float num4 = 90f;
                if (proj.type == 857)
                {
                    num4 = 60f;
                    flag = false;
                }
                float lerpValue2 = Utils.GetLerpValue(num4, num4 * (5f / 6f), proj.localAI[0], clamped: true);
                float lerpValue3 = Utils.GetLerpValue(0f, 120f, proj.Center.Distance(center), clamped: true);
                lerpValue *= lerpValue3;
                lerpValue2 *= Utils.GetLerpValue(0f, 15f, proj.localAI[0], clamped: true);
                color2 = Color.HotPink * 0.15f;// * (lerpValue2 * lerpValue);
                if (proj.type == 857)
                {
                    color2 = proj.GetFirstFractalColor() * 0.15f * (lerpValue2 * lerpValue);
                }
                spinningpoint = new Vector2(0f, -2f);
                float lerpValue4 = Utils.GetLerpValue(num4, num4 * (2f / 3f), proj.localAI[0], clamped: true);
                lerpValue4 *= Utils.GetLerpValue(0f, 20f, proj.localAI[0], clamped: true);
                num = -0.3f * (1f - lerpValue4);
                num += -1f * Utils.GetLerpValue(15f, 0f, proj.localAI[0], clamped: true);
                num *= lerpValue;
                num2 = lerpValue2 * lerpValue;

                num = 0f; //.15
                num2 = 1f;
            }
            Vector2 vector5 = proj.Center + vector;
            Texture2D value = TextureAssets.Projectile[856].Value;
            _ = new Rectangle(0, 0, value.Width, value.Height).Size() / 2f;
            Texture2D value2 = TextureAssets.Extra[91].Value;
            Rectangle value3 = value2.Frame();
            Vector2 origin2 = new Vector2((float)value3.Width / 2f, 10f);
            _ = Color.Cyan * 0.5f * num2;
            Vector2 vector2 = new Vector2(0f, proj.gfxOffY);
            float num5 = (float)Main.timeForVisualEffects / 60f;
            Vector2 vector3 = vector5 + vector * 0.5f;
            Color color3 = Color.White * 0.5f * num2;
            color3.A = 0;
            Color color4 = color2 * num2;
            color4.A = 0;
            Color color5 = color2 * num2;
            color5.A = 0;
            Color color6 = color2 * num2;
            color6.A = 0;
            float num6 = vector.ToRotation();

            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5), value3, color4, num6 + (float)Math.PI / 2f, origin2, overallScale * (1.5f + num), SpriteEffects.None);
            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + (float)Math.PI * 2f / 3f), value3, color5, num6 + (float)Math.PI / 2f, origin2, overallScale * (1.1f + num), SpriteEffects.None);
            Main.EntitySpriteDraw(value2, vector3 - Main.screenPosition + vector2 + spinningpoint.RotatedBy((float)Math.PI * 2f * num5 + 4.18879032f), value3, color6, num6 + (float)Math.PI / 2f, origin2, overallScale * (1.3f + num), SpriteEffects.None);
            Vector2 vector4 = vector5 - vector * 0.5f;
            for (float num7 = 0f; num7 < 1f; num7 += 0.5f)
            {
                float num8 = num5 % 0.5f / 0.5f;
                num8 = (num8 + num7) % 1f;
                float num9 = num8 * 2f;
                if (num9 > 1f)
                {
                    num9 = 2f - num9;
                }
                Main.EntitySpriteDraw(value2, vector4 - Main.screenPosition + vector2, value3, color3 * num9, num6 + (float)Math.PI / 2f, origin2, overallScale * (0.3f + num8 * 0.5f), SpriteEffects.None);
            }
            if (flag)
            {
                float rotation = proj.rotation + proj.localAI[1];
                _ = (float)Main.timeForVisualEffects / 240f;
                _ = Main.GlobalTimeWrappedHourly;
                float globalTimeWrappedHourly = Main.GlobalTimeWrappedHourly;
                globalTimeWrappedHourly %= 5f;
                globalTimeWrappedHourly /= 2.5f;
                if (globalTimeWrappedHourly >= 1f)
                {
                    globalTimeWrappedHourly = 2f - globalTimeWrappedHourly;
                }
                globalTimeWrappedHourly = globalTimeWrappedHourly * 0.5f + 0.5f;
                Vector2 position = proj.Center - Main.screenPosition;
                Main.instance.LoadItem(75);
                Texture2D value4 = TextureAssets.Item[75].Value;
                Rectangle rectangle = value4.Frame(1, 8);
                Main.EntitySpriteDraw(origin: rectangle.Size() / 2f, texture: value4, position: position, sourceRectangle: rectangle, color: color, rotation: rotation, scale: overallScale * proj.scale * 1.25f, effects: SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

    }

}
