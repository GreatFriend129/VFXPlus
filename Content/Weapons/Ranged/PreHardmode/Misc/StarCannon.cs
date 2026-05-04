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
using VFXPlus.Common.Drawing;
using rail;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class StarCannon : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.StarCannon);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.StarCannon,
                    AnimTime: 15,
                    NormalXOffset: 16f,
                    DestXOffset: 3f,
                    YRecoilAmount: 0.13f,
                    HoldOffset: new Vector2(0f, 1f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_67") with { Volume = 0.2f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style5 = new SoundStyle("Terraria/Sounds/Research_2") with { Volume = 0.1f, Pitch = 1f, };
            SoundEngine.PlaySound(style5, player.Center);

            SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/starUIToss") with { Volume = .05f, Pitch = 0.7f, PitchVariance = 0.15f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Volume = .15f, Pitch = .55f, PitchVariance = .1f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, player.Center);

            Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            float circlePulseSize = 0.25f;

            //Dust d2 = Dust.NewDustPerfect(position + velNormalized * 3f, ModContent.DustType<CirclePulse>(), velNormalized * 3f, newColor: Color.OrangeRed);
            //CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            //b2.drawLayer = "Dusts";
            //d2.customData = b2;
            //d2.scale = circlePulseSize * 0.05f;

            return true;
        }
    }
    public class FallenStarOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.StarCannonStar);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            currentRot = projectile.velocity.ToRotation();

            #region trail (add 2 positions each frame)
            int trailCount = 28;
            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousVelRots.Add(currentRot);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousVelRots.Count > trailCount)
                previousVelRots.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            #endregion

            #region dust
            if (timer % 2 == 0 && Main.rand.NextBool(2) && timer > 3)
            {
                Color col = Main.rand.NextBool() ? Color.Gold : Color.DodgerBlue;

                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust de = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: 0.38f);

                de.velocity *= 0.45f;
                de.velocity += currentRot.ToRotationVector2() * 3f;

            }

            #endregion

            timer++;


            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.05f), 0f, 1f);

            float fadeInTime = Math.Clamp((timer + 3f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            projectile.soundDelay = 10;

            if (timer == 1 && false)
            {
                SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/starUIToss") with { Volume = .1f, Pitch = 0.7f, PitchVariance = 0.15f, MaxInstances = 1};
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Volume = .2f, Pitch = .55f, PitchVariance = .1f, MaxInstances = -1 };
                SoundEngine.PlaySound(style2, projectile.Center);

                //SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_9") with { Volume = 0.25f, Pitch = .2f, PitchVariance = 0.05f, MaxInstances = -1 };
                //SoundEngine.PlaySound(style3, projectile.Center);
            }

            #region vanillaAI
            projectile.rotation += (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y)) * 0.01f * (float)projectile.direction;

            Vector2 vector2 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector2 / 2f, vector2 + new Vector2(400f))) && Main.rand.NextBool(20)) //6
            {
                int num932 = Utils.SelectRandom<int>(Main.rand, 16, 17, 17, 17);
                if (Main.tenthAnniversaryWorld)
                {
                    num932 = Utils.SelectRandom<int>(Main.rand, 16, 16, 16, 17);
                }
                Gore.NewGore(null, projectile.position, projectile.velocity * 0.2f, num932);
            }
            projectile.light = 0.9f;
            if (Main.rand.Next(20) == 0 || (Main.tenthAnniversaryWorld && Main.rand.Next(15) == 0))
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f, 150, default(Color), 1.2f);
            }
            #endregion

            return false;
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
            float adjustedScale = projectile.scale * 1f * overallScale;

            Texture2D FireBall = CommonTextures.FireBallBlur.Value;
            Texture2D FireBallPixel = CommonTextures.Extra_91.Value;

            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;
            Texture2D CrispStar = CommonTextures.CrispStarPMA.Value;

            Texture2D VStar = Mod.Assets.Request<Texture2D>("Assets/Pixel/VanillaStar").Value;

            Vector2 off = (currentRot).ToRotationVector2() * -25f * adjustedScale;

            Vector2 pos = projectile.Center - Main.screenPosition;
            float rot = currentRot + MathHelper.PiOver2;

            float thisAlpha = overallAlpha * 0.15f;

            //Glorb
            Color orbCol1 = Color.DeepSkyBlue * 0.75f;
            Color orbCol2 = Color.DodgerBlue * 0.525f;
            Color orbCol3 = Color.Blue * 0.375f;

            float scale1 = 0.75f * 1.25f; //
            float scale2 = 1.6f * 1.25f; //
            float scale3 = 2.5f * 1.25f; //

            Main.EntitySpriteDraw(Glow, pos, null, orbCol1 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale1 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol2 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale2 * 0.55f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, pos, null, orbCol3 with { A = 0 } * overallAlpha * 0.35f, rot, Glow.Size() / 2f, adjustedScale * scale3 * 0.55f, SpriteEffects.None);

            //Star
            Vector2 starScale = new Vector2(0.85f, 1.45f) * (1f - overallScale);
            Main.EntitySpriteDraw(CrispStar, pos, null, Color.DodgerBlue with { A = 0 }, rot, CrispStar.Size() / 2f, 1.4f * starScale, SpriteEffects.None);
            Main.EntitySpriteDraw(CrispStar, pos, null, Color.White with { A = 0 }, rot, CrispStar.Size() / 2f, 0.55f * starScale, SpriteEffects.None); //0.6

            #region after image
            if (previousVelRots != null && previousPostions != null)
            {
                for (int i = 0; i < previousVelRots.Count; i++)
                {
                    float progress = (float)i / previousVelRots.Count;

                    float size = (1f - (progress * 0.5f)) * adjustedScale;

                    float colVal = progress;

                    Color col = Color.Lerp(Color.DodgerBlue, Color.Yellow * 1f, Easings.easeInCirc(progress)) * progress * 0.75f;
                    //Color col = Color.Lerp(Color.Blue * 10f, Color.Yellow * 1f, progress) * progress * 0.75f;


                    float size2 = (1f - (progress * 0.15f)) * adjustedScale;

                    Main.EntitySpriteDraw(FireBallPixel, previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(4f, 4f), null, col with { A = 0 } * 1.15f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                    Vector2 vec2Scale = new Vector2(0.25f * progress, 1.15f) * size;

                    Main.EntitySpriteDraw(FireBall, previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(8f, 8f), null, col with { A = 0 } * 1.25f * thisAlpha * colVal,
                            previousVelRots[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
                }

            }
            #endregion

            Vector2 v2scale = new Vector2(1.25f, 1f);
            Main.EntitySpriteDraw(FireBall, pos + off + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepPink with { A = 0 } * thisAlpha, rot, FireBall.Size() / 2f, adjustedScale * v2scale, SpriteEffects.None);

            //Pink Star
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            for (int i = 0; i < 4; i++)
            {
                Color col = Color.Orange;
                Main.EntitySpriteDraw(VStar, drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f), null, col with { A = 0 } * 0.8f * thisAlpha, projectile.rotation, VStar.Size() / 2f, adjustedScale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(VStar, drawPos, null, Color.OrangeRed, projectile.rotation, VStar.Size() / 2f, adjustedScale, SpriteEffects.None);

            //Main.EntitySpriteDraw(VStar, drawPos, null, Color.White with { A = 0 }, projectile.rotation, VStar.Size() / 2f, adjustedScale, SpriteEffects.None);
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

                float lerpValue2 = Utils.GetLerpValue(num4, num4 * (5f / 6f), proj.localAI[0], clamped: true);
                float lerpValue3 = Utils.GetLerpValue(0f, 120f, proj.Center.Distance(center), clamped: true);
                lerpValue *= lerpValue3;
                lerpValue2 *= Utils.GetLerpValue(0f, 15f, proj.localAI[0], clamped: true);
                
                color2 = Color.DodgerBlue * 0.15f;// * (lerpValue2 * lerpValue);

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
            float num5 = (float)(Main.timeForVisualEffects * 2f) / 60f;
            Vector2 vector3 = (vector5 + vector * 0.5f) + Main.rand.NextVector2Circular(2f, 2f);
            Color color3 = Color.Gold * 0.5f * num2;
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
                Main.EntitySpriteDraw(origin: rectangle.Size() / 2f, texture: value4, position: position, sourceRectangle: rectangle, color: color, rotation: rotation, scale: overallScale * proj.scale * 1f, effects: SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.75f, Pitch = 0.15f, PitchVariance = 0.05f, MaxInstances = -1 }, projectile.position);

            Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Gold, Scale: 0.2f);

            da.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 20,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 7 + Main.rand.Next(3); i++)
            {
                Color col = Main.rand.NextBool() ? Color.Gold : Color.DodgerBlue;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 7f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }

            #region vanillaKill
            Color newColor7 = Color.CornflowerBlue;
            if (Main.tenthAnniversaryWorld && (projectile.type == 12 || projectile.type == 955))
            {
                newColor7 = Color.HotPink;
                newColor7.A /= 2;
            }
            for (int num635 = 0; num635 < 7; num635++)
            {
                ////Dust.NewDust(projectile.position, projectile.width, projectile.height, 58, projectile.velocity.X * 0.1f, projectile.velocity.Y * 0.1f, 150, default(Color), 0.8f);
            }
            for (float num636 = 0f; num636 < 1f; num636 += 0.225f)  //0.125
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num636 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (4f + Main.rand.NextFloat() * 4f), 150, newColor7).noGravity = true;
            }
            for (float num637 = 0f; num637 < 1f; num637 += 0.35f) //0.25f
            {
                Dust.NewDustPerfect(projectile.Center, 278, Vector2.UnitY.RotatedBy(num637 * ((float)Math.PI * 2f) + Main.rand.NextFloat() * 0.5f) * (2f + Main.rand.NextFloat() * 3f), 150, Color.Gold).noGravity = true;
            }
            Vector2 vector54 = new Vector2(Main.screenWidth, Main.screenHeight);
            if (projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector54 / 2f, vector54 + new Vector2(400f))))
            {
                for (int num638 = 0; num638 < 4; num638++)
                {
                    Gore.NewGore(projectile.GetSource_FromThis(), projectile.position, Main.rand.NextVector2CircularEdge(0.5f, 0.5f) * projectile.velocity.Length(), Utils.SelectRandom<int>(Main.rand, 16, 17, 17, 17, 17, 17, 17, 17));
                }
            }
            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5 + Main.rand.Next(3); i++)
            {
                Color col = Main.rand.NextBool() ? Color.Gold : Color.DodgerBlue;

                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
            }
        }
    }


}
