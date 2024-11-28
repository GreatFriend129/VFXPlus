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
            TrailPreDraw();
            return false;
        }
        public void TrailPreDraw()
        {
            Texture2D texture = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

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
                Color color = Main.hslToRgb((Projectile.localAI[2] * 0.01f + 0.5f) % 1f, 1f, 0.7f, 0);//Pastel(MathHelper.ToRadians(Projectile.ai[1] + k * 2), true);
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
                        Main.spriteBatch.Draw(texture, drawPos, null, color with { A = 0 }, betweenPositions.ToRotation(), drawOrigin, scale, SpriteEffects.None, 0f);
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


}