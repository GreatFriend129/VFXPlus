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
using Terraria.GameContent;
using System.Linq;
using Microsoft.Build.Evaluation;
using System.IO;

namespace VFXPlus.Content.VFXTest
{
    public class SupernovaScatterSots : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.extraUpdates = 4;
            Projectile.timeLeft = 900;
            Projectile.penetrate = -1;
        }

        Vector2[] trailPos = new Vector2[60];
        public void cataloguePos()
        {
            Vector2 current = Projectile.Center;
            for (int i = 0; i < trailPos.Length; i++)
            {
                Vector2 previousPosition = trailPos[i];
                trailPos[i] = current;
                current = previousPosition;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                TrailPreDraw();
            });
            return false;
        }
        public void TrailPreDraw()
        {
            //Texture2D texture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            Texture2D texture = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            //Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 previousPosition = Projectile.Center;
            for (int k = 0; k < trailPos.Length; k++)
            {
                float scale = Projectile.scale * 0.75f * (trailPos.Length - k) / (float)trailPos.Length;
                if (trailPos[k] == Vector2.Zero)
                {
                    break;
                }
                Color color = Main.hslToRgb((Projectile.localAI[2] * 0.01f + 0.5f) % 1f, 1f, 0.4f, 0);//Pastel(MathHelper.ToRadians(Projectile.ai[1] + k * 2), true);
                color.A = 0;

                Color rainbow = Main.hslToRgb((Projectile.localAI[2] * 0.01f + 0.5f) % 1f, 1f, 0.7f, 0);


                Vector2 drawPos = trailPos[k] - Main.screenPosition;
                Vector2 currentPos = trailPos[k];
                Vector2 betweenPositions = previousPosition - currentPos;
                color = color * ((trailPos.Length - k) / (float)trailPos.Length) * 0.33f;
                float max = betweenPositions.Length() / (texture.Width * scale);
                for (int i = 0; i < max; i++)
                {
                    drawPos = previousPosition + -betweenPositions * (i / max) - Main.screenPosition;
                    if (trailPos[k] != Projectile.Center)
                    {
                        Main.spriteBatch.Draw(texture, drawPos, null, color with { A = 0 }, betweenPositions.ToRotation(), drawOrigin, scale, SpriteEffects.None, 0f);
                        Main.spriteBatch.Draw(texture, drawPos, null, Color.White with { A = 0 } * ((trailPos.Length - k) / (float)trailPos.Length) * 0.33f, betweenPositions.ToRotation(), drawOrigin, scale * 0.75f, SpriteEffects.None, 0f);

                    }
                }
                previousPosition = currentPos;
            }
        }

        public static Color Pastel(float radians, bool pinkify = false)
        {
            Color color = Color.White;
            if (pinkify)
                color = new Color(253, 198, 234);
            return PastelGradient(radians, color);
        }
        public static Color PastelGradient(float radians, Color overrideColor)
        {
            float newAi = radians;
            double center = 190;
            Vector2 circlePalette = new Vector2(1, 0).RotatedBy(newAi);
            double width = 65 * circlePalette.Y;
            int red = (int)(center + width);
            circlePalette = new Vector2(1, 0).RotatedBy(newAi + MathHelper.ToRadians(120));
            width = 65 * circlePalette.Y;
            int grn = (int)(center + width);
            circlePalette = new Vector2(1, 0).RotatedBy(newAi + MathHelper.ToRadians(240));
            width = 65 * circlePalette.Y;
            int blu = (int)(center + width);
            if (overrideColor == Color.White)
                return new Color(red, grn, blu);
            else
                return new Color(red, grn, blu).MultiplyRGB(overrideColor);
        }


        private bool hasHit = false;
        private bool runOnce = true;
        public override void AI()
        {
            cataloguePos();
            if (runOnce)
            {
                runOnce = false;
                //Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)Projectile.Center.X, (int)Projectile.Center.Y, 60, 0.8f, -0.1f);
            }
            if (false && Main.rand.NextBool(40) || (hasHit && Main.rand.NextBool(8)))
            {
                //Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<CopyDust4>());
                ///dust.color = ColorHelper.Pastel(MathHelper.ToRadians(Projectile.ai[1]), true);
                //dust.noGravity = true;
                //dust.fadeIn = 0.1f;
                //dust.scale = 1.3f;
                //dust.velocity *= 0.2f;
                //dust.alpha = 125;
            }
            if (!Projectile.velocity.Equals(new Vector2(0, 0)))
                Projectile.rotation = Projectile.velocity.ToRotation();
            if (hasHit)
            {
                if (Projectile.timeLeft > 60)
                    Projectile.timeLeft = 60;
            }
            else
            {
                float sin = (float)Math.Sin(MathHelper.ToRadians(Projectile.ai[1] * 1.1f)) * 0.25f;
                Projectile.Center += new Vector2(0, sin).RotatedBy(Projectile.velocity.ToRotation());
                Projectile.ai[1]++;
                //int target = SOTSNPCs.FindTarget_Basic(Projectile.Center, 640);
                if (FindNearestNPCMouse(500f, true, false, true, out int index))
                {
                    NPC npc = Main.npc[index];
                    if (npc.CanBeChasedBy())
                    {
                        Vector2 toNPC = npc.Center - Projectile.Center;
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toNPC.SafeNormalize(Vector2.Zero) * 5f, 0.01f);
                    }
                    //else
                        //triggerUpdate();
                }
            }
            //if (Projectile.timeLeft < 60 && !hasHit)
            //triggerUpdate();

            Projectile.localAI[2]++;
        }

        private bool FindNearestNPCMouse(float range, bool scanTiles, bool targetIsFriendly, bool ignoreCritters, out int npcIndex)
        {
            npcIndex = -1;
            bool foundNPC = false;
            double dist = range * range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                //Make sure NPC is valid anyway
                if (npc.active && npc.life > 0)
                {
                    if (!npc.hide && !npc.dontTakeDamage)
                    {
                        //Target and NPC friendliness are same
                        if (npc.friendly == targetIsFriendly)
                        {
                            //if ignoring critters, make sure lifemax > 10, id is not dummy, and npc does not drop item
                            if ((!(npc.lifeMax < 10 || npc.type == NPCID.TargetDummy || npc.catchItem != 0) && ignoreCritters) || !ignoreCritters)
                            {
                                //cache this
                                float compDist = Main.MouseWorld.DistanceSQ(npc.Center);
                                //Distance is shorter than current distance, but did not overflow (underflow)
                                if (compDist < dist && compDist > 0)
                                {
                                    //ignore tiles, OR scan tiles and can hit anyway
                                    if (!scanTiles || (scanTiles && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1)))
                                    {
                                        npcIndex = i;
                                        dist = compDist;
                                        foundNPC = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Case: Failed to Find NPC
            if (!foundNPC)
                npcIndex = -1;
            return foundNPC;
        }
    }


    public class SyctheTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 1900;
            Projectile.penetrate = -1;
        }

        int timer = 0;
        public override void AI()
        {
            Projectile.Opacity = 1f;


            timer++;
        }

        public float overallAlpha = 1f;
        public float overallScale = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawShit(true);
            });
            DrawShit(false);

            return false;
        }
        public void DrawShit(bool giveUp)
        {
            if (giveUp)
                return;

            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);
            Color purple3 = new Color(121, 7, 179);

            Projectile proj = Projectile;

            Asset<Texture2D> asset = TextureAssets.Projectile[ProjectileID.TrueNightsEdge];
            Rectangle rectangle = asset.Frame(1, 4);
            Vector2 origin = rectangle.Size() / 2f;
            float num = proj.scale * 1.1f * overallScale * 1f;
            SpriteEffects effects = ((!(proj.ai[0] >= 0f)) ? SpriteEffects.FlipVertically : SpriteEffects.None);
            float num6 = 0.975f;
            float fromValue = Lighting.GetColor(proj.Center.ToTileCoordinates()).ToVector3().Length() / (float)Math.Sqrt(3.0);
            fromValue = Utils.Remap(fromValue, 0.2f, 1f, 0f, 1f);
            float num7 = MathHelper.Min(0.15f + fromValue * 0.85f, Utils.Remap(proj.localAI[0], 30f, 96f, 1f, 0f));
            _ = proj.Size / 2f;
            float num8 = 2f;
            for (float num9 = num8; num9 >= 0f; num9 -= 1f)
            {
                if (true || !(proj.oldPos[(int)num9] == Vector2.Zero))
                {
                    Vector2 vector = proj.Center - proj.velocity * 0.5f * num9;
                    float num10 = (float)Main.timeForVisualEffects * 0.22f;// proj.oldRot[(int)num9] + proj.ai[0] * ((float)Math.PI * 2f) * 0.1f * (0f - num9);
                    Vector2 position = vector - Main.screenPosition;
                    float num11 = 1f - num9 / num8;
                    float num12 = proj.Opacity * num11 * num11 * 0.85f;
                    Color color = new Color(80, 160, 50, 120);
                    Main.spriteBatch.Draw(asset.Value, position, rectangle, color * num7 * num12, num10 + proj.ai[0] * ((float)Math.PI / 4f) * -1f, origin, num * num6, effects, 0f);
                    Color color2 = new Color(155, 255, 100);
                    Color color3 = Color.White * num12 * 0.5f;
                    color3.A = (byte)((float)(int)color3.A * (1f - num7));
                    Color color4 = color3 * num7 * 0.5f;
                    color4.G = (byte)((float)(int)color4.G * num7);
                    color4.R = (byte)((float)(int)color4.R * (0.25f + num7 * 0.75f));
                    float num13 = 3f;
                    for (float num2 = (float)Math.PI * -2f + (float)Math.PI * 2f / num13; num2 < 0f; num2 += (float)Math.PI * 2f / num13)
                    {
                        float num3 = Utils.Remap(num2, (float)Math.PI * -2f, 0f, 0f, 0.5f);
                        Main.spriteBatch.Draw(asset.Value, position, rectangle, color4 * 0.15f * num3, num10 + proj.ai[0] * 0.01f + num2, origin, num, effects, 0f);
                        Main.spriteBatch.Draw(asset.Value, position, rectangle, new Color(200, 255, 0) * fromValue * num12 * num3, num10 + num2, origin, num * 0.8f, effects, 0f);
                        Main.spriteBatch.Draw(asset.Value, position, rectangle, color2 * fromValue * num12 * MathHelper.Lerp(0.05f, 0.4f, fromValue) * num3, num10 + num2, origin, num * num6, effects, 0f);
                        Main.spriteBatch.Draw(asset.Value, position, asset.Frame(1, 4, 0, 3), Color.White * MathHelper.Lerp(0.05f, 0.5f, fromValue) * num12 * num3, num10 + num2, origin, num, effects, 0f);
                    }
                    Main.spriteBatch.Draw(asset.Value, position, rectangle, color4 * 0.15f, num10 + proj.ai[0] * 0.01f, origin, num, effects, 0f);
                    Main.spriteBatch.Draw(asset.Value, position, rectangle, new Color(200, 255, 0) * num7 * num12, num10, origin, num * 0.8f, effects, 0f);
                    Main.spriteBatch.Draw(asset.Value, position, rectangle, color2 * fromValue * num12 * MathHelper.Lerp(0.05f, 0.4f, num7), num10, origin, num * num6, effects, 0f);
                    Main.spriteBatch.Draw(asset.Value, position, asset.Frame(1, 4, 0, 3), Color.White * MathHelper.Lerp(0.05f, 0.5f, num7) * num12, num10, origin, num, effects, 0f);
                }
            }
            float num4 = 1f - proj.localAI[0] * 1f / 80f;
            if (num4 < 0.5f)
            {
                num4 = 0.5f;
            }
            Vector2 drawpos = proj.Center - Main.screenPosition + (proj.rotation + 0.471238941f * proj.ai[0]).ToRotationVector2() * ((float)asset.Width() * 0.5f - 4f) * num * num4;
            float num5 = MathHelper.Min(num7, MathHelper.Lerp(1f, fromValue, Utils.Remap(proj.localAI[0], 0f, 80f, 0f, 1f)));
            //Main.DrawPrettyStarSparkle(proj.Opacity, SpriteEffects.None, drawpos, new Color(255, 255, 255, 0) * proj.Opacity * 0.5f * num5, new Color(150, 255, 100) * num5, proj.Opacity, 0f, 1f, 1f, 2f, (float)Math.PI / 4f, new Vector2(2f, 2f), Vector2.One);

        }


    }

}