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
using System.Timers;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Tomes
{
    
    public class RazorbladeTyphoon : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.RazorbladeTyphoon) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.RazorbladeTyphoonToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.scale = 1f;
            
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle stylea = new SoundStyle("AerovelenceMod/Sounds/Effects/star_impact_01") with { Volume= 0.2f, Pitch = 1f, PitchVariance = .35f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(stylea, player.Center);

            return true;
        }

    }
    public class RazorbladeTyphoonShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Typhoon) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.RazorbladeTyphoonToggle;
        }

        float scale = 0;
        float alpha = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 40; //40
            
            if (timer % 1 == 0)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPostions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPostions.Count > trailCount)
                    previousPostions.RemoveAt(0);
            }


            if (projectile.timeLeft < 12) //25
            {
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.25f, 0.08f), 0f, 1f);
                scale = Math.Clamp(MathHelper.Lerp(scale, -0.05f, 0.02f), 0f, 1f);

            }
            else
            {
                float timeForPopInAnim = 50;
                float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f);

                scale = 0.4f + MathHelper.Lerp(0f, 0.6f, Easings.easeInOutBack(animProgress, 0f, 1f));
                alpha = 1f;
            }



            if (timer % 2 == 0 && timer > 3 && Main.rand.NextBool(2))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3.5f, 3.5f);

                Dust da = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<GlowFlare>(), dustVel, newColor: Color.DeepSkyBlue * 0.75f, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 2f);
                da.velocity += projectile.velocity.RotatedByRandom(0.2f) * 0.65f;
                //da.alpha = 12;
            }

            if (timer % 6 == 0)
                projectile.frame = (projectile.frame + 1) % 3;

            if (projectile.localAI[1] > 10f && Main.rand.Next(3) == 0)
            {
                int num637 = 2;
                for (int num638 = 0; num638 < num637; num638++)
                {
                    Vector2 vector125 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - (float)Math.PI / 2f).ToRotationVector2() * Main.rand.Next(3, 8);

                    Vector2 randomSpawnPos = Main.rand.NextVector2Circular(projectile.width, projectile.height);
                    int num639 = Dust.NewDustPerfect(projectile.Center + randomSpawnPos, ModContent.DustType<GlowPixelAlts>(), vector125 * 2f, 10, Scale: 0.35f).dustIndex;

                    Main.dust[num639].color = Color.DeepSkyBlue * 0.5f;
                    Main.dust[num639].noGravity = true;
                    Main.dust[num639].noLight = true;
                    Dust dust123 = Main.dust[num639];
                    Dust dust212 = dust123;
                    dust212.velocity /= 4f;
                    dust123 = Main.dust[num639];
                    dust212 = dust123;
                    dust212.velocity -= projectile.velocity;
                }
            }

            timer++;


            #region vanillaAI
            projectile.localAI[1]++;
            if (projectile.localAI[1] > 10f && Main.rand.Next(3) == 0)
            {
                int num637 = 0;
                for (int num638 = 0; num638 < num637; num638++)
                {
                    Vector2 spinningpoint13 = Vector2.Normalize(projectile.velocity) * new Vector2(projectile.width, projectile.height) / 2f;
                    spinningpoint13 = spinningpoint13.RotatedBy((double)(num638 - (num637 / 2 - 1)) * Math.PI / (double)num637) + projectile.Center;
                    Vector2 vector125 = ((float)(Main.rand.NextDouble() * 3.1415927410125732) - (float)Math.PI / 2f).ToRotationVector2() * Main.rand.Next(3, 8);
                    int num639 = Dust.NewDust(spinningpoint13 + vector125, 0, 0, ModContent.DustType<GlowPixelAlts>(), vector125.X * 2f, vector125.Y * 2f, 10, default(Color), 0.35f);

                    //Offset pos
                    Vector2 offset = new Vector2(0f, -projectile.height / 2f).RotatedBy(projectile.velocity.ToRotation());
                    //Main.dust[num639].position += offset;

                    Main.dust[num639].color = Color.DeepSkyBlue * 0.5f;
                    Main.dust[num639].noGravity = true;
                    Main.dust[num639].noLight = true;
                    Dust dust123 = Main.dust[num639];
                    Dust dust212 = dust123;
                    dust212.velocity /= 4f;
                    dust123 = Main.dust[num639];
                    dust212 = dust123;
                    dust212.velocity -= projectile.velocity;
                }
                projectile.alpha -= 5;
                if (projectile.alpha < 50)
                {
                    projectile.alpha = 50;
                }
                projectile.rotation += projectile.velocity.X * 0.1f;
                //if (timer % 7 == 0)
                    //projectile.frame = (projectile.frame + 1) % 3;
                //projectile.frame = (int)(projectile.localAI[1] / 3f) % 3;
                Lighting.AddLight((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16, 0.1f, 0.4f, 0.6f);
            }
            int num640 = -1;
            Vector2 vector126 = projectile.Center;
            float num641 = 500f;
            if (projectile.localAI[0] > 0f)
            {
                projectile.localAI[0]--;
            }
            if (projectile.ai[0] == 0f && projectile.localAI[0] == 0f)
            {
                for (int num642 = 0; num642 < 200; num642++)
                {
                    NPC nPC17 = Main.npc[num642];
                    if (nPC17.CanBeChasedBy(this) && (projectile.ai[0] == 0f || projectile.ai[0] == (float)(num642 + 1)))
                    {
                        Vector2 center18 = nPC17.Center;
                        float num643 = Vector2.Distance(center18, vector126);
                        if (num643 < num641 && Collision.CanHit(projectile.position, projectile.width, projectile.height, nPC17.position, nPC17.width, nPC17.height))
                        {
                            num641 = num643;
                            vector126 = center18;
                            num640 = num642;
                        }
                    }
                }
                if (num640 >= 0)
                {
                    projectile.ai[0] = num640 + 1;
                    projectile.netUpdate = true;
                }
                num640 = -1;
            }
            if (projectile.localAI[0] == 0f && projectile.ai[0] == 0f)
            {
                projectile.localAI[0] = 30f;
            }
            bool flag27 = false;
            if (projectile.ai[0] != 0f)
            {
                int num645 = (int)(projectile.ai[0] - 1f);
                if (Main.npc[num645].active && !Main.npc[num645].dontTakeDamage && Main.npc[num645].immune[projectile.owner] == 0)
                {
                    float num646 = Main.npc[num645].position.X + (float)(Main.npc[num645].width / 2);
                    float num647 = Main.npc[num645].position.Y + (float)(Main.npc[num645].height / 2);
                    float num648 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num646) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num647);
                    if (num648 < 1000f)
                    {
                        flag27 = true;
                        vector126 = Main.npc[num645].Center;
                    }
                }
                else
                {
                    projectile.ai[0] = 0f;
                    flag27 = false;
                    projectile.netUpdate = true;
                }
            }
            if (flag27)
            {
                Vector2 v6 = vector126 - projectile.Center;
                float num649 = projectile.velocity.ToRotation();
                float num650 = v6.ToRotation();
                double num651 = num650 - num649;
                if (num651 > Math.PI)
                {
                    num651 -= Math.PI * 2.0;
                }
                if (num651 < -Math.PI)
                {
                    num651 += Math.PI * 2.0;
                }
                projectile.velocity = projectile.velocity.RotatedBy(num651 * 0.10000000149011612);
            }
            float num652 = projectile.velocity.Length();
            projectile.velocity.Normalize();
            projectile.velocity *= num652 + 0.0025f;
            #endregion

            return false;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D line = CommonTextures.Flare.Value;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = Color.Lerp(Color.Blue, Color.Aqua * 1f, progress) * 0.5f;// * Easings.easeInQuad(progress);

                    //DodgerBlue or Blue

                    float size2 = 1f * projectile.scale * scale * progress;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Vector2 offset1 = new Vector2(0f, -22f * progress * projectile.scale).RotatedBy(projectile.velocity.ToRotation());
                    Vector2 offset2 = new Vector2(0f, 22f * progress * projectile.scale).RotatedBy(projectile.velocity.ToRotation());

                    offset1 += Main.rand.NextVector2Circular(7f, 10f * progress) * projectile.scale;
                    offset2 += Main.rand.NextVector2Circular(7f, 10f * progress) * projectile.scale; //1f 1.5f

                    Vector2 innerScale = new Vector2(size2 * 1.25f, size2 * 1.25f * 0.1f);

                    Main.EntitySpriteDraw(line, AfterImagePos + offset1, null, col with { A = 0 } * 0.45f * progress * alpha,
                        previousRotations[i], line.Size() / 2f, size2 * 1.25f, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos + offset2, null, col with { A = 0 } * 0.45f * progress * alpha,
                        previousRotations[i], line.Size() / 2f, size2 * 1.25f, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos + offset1, null, Color.White with { A = 0 } * 0.6f * progress * alpha,
                        previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos + offset2, null, Color.White with { A = 0 } * 0.6f * progress * alpha,
                        previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);

                }
            });

            for (int i = 0; i < 4; i++)
            {
                float dist = 5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                float opacitySquared = 1f;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.DeepSkyBlue with { A = 0 } * 0.25f * alpha, projectile.rotation, TexOrigin, projectile.scale * 1f * scale, SpriteEffects.None);
            }

            //Border
            for (int i = 0; i < 6; i++)
            {
                //float opacitySquared = 1f;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                   Color.White with { A = 0 } * alpha * alpha * alpha * alpha, projectile.rotation, TexOrigin, projectile.scale * 1f * scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * alpha, projectile.rotation, TexOrigin, projectile.scale * scale, SpriteEffects.None);

            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor with { A = 0 } * 0.1f, projectile.rotation, TexOrigin, projectile.scale * scale, SpriteEffects.None);

            return false;

        }

        public void WhatTheFuckThisLooksSoCool(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            //After-Image
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                if (previousRotations != null && previousPostions != null)
                {
                    for (int i = 0; i < previousRotations.Count; i++)
                    {
                        float progress = (float)i / previousRotations.Count;

                        Color col = Color.Lerp(Color.Aquamarine, Color.DeepSkyBlue * 1f, progress) * 0.5f;// * Easings.easeInQuad(progress);

                        float size2 = 1f * projectile.scale * scale * progress;

                        Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                        Vector2 offset1 = new Vector2(0f, -22f * progress).RotatedBy(projectile.velocity.ToRotation()) + Main.rand.NextVector2Circular(15f, 15f);
                        Vector2 offset2 = new Vector2(0f, 22f * progress).RotatedBy(projectile.velocity.ToRotation()) + Main.rand.NextVector2Circular(15f, 15f);

                        Vector2 innerScale = new Vector2(size2 * 1.25f, size2 * 1.25f * 0.1f);

                        Main.EntitySpriteDraw(line, AfterImagePos + offset1, null, col with { A = 0 } * 0.5f * progress,
                            previousRotations[i], line.Size() / 2f, size2 * 1.25f, SpriteEffects.None);

                        Main.EntitySpriteDraw(line, AfterImagePos + offset2, null, col with { A = 0 } * 0.5f * progress,
                            previousRotations[i], line.Size() / 2f, size2 * 1.25f, SpriteEffects.None);

                        Main.EntitySpriteDraw(line, AfterImagePos + offset1, null, Color.White with { A = 0 } * 0.5f * progress,
    previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
                        Main.EntitySpriteDraw(line, AfterImagePos + offset2, null, Color.White with { A = 0 } * 0.5f * progress,
    previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
                        //Black
                        //Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, Color.Black * 0.1f * progress, 
                        //projectile.rotation, TexOrigin, size2, SpriteEffects.None);

                        //Main
                        //Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.45f,
                        // projectile.rotation, TexOrigin, size2, SpriteEffects.None);
                    }

                }
            });

            for (int i = 0; i < 4; i++)
            {
                float dist = 5f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                float opacitySquared = 1f;
                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.DeepSkyBlue with { A = 0 } * 0.25f * scale, projectile.rotation, TexOrigin, projectile.scale * 1f, SpriteEffects.None);
            }

            //Border
            for (int i = 20; i < 6; i++)
            {
                //float opacitySquared = 1f;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                   Color.White with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1f * scale, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White, projectile.rotation, TexOrigin, projectile.scale * scale, SpriteEffects.None);

            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor with { A = 0 } * 0.1f, projectile.rotation, TexOrigin, projectile.scale * scale, SpriteEffects.None);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < previousPostions.Count; i++)
            {
                if (i % 4 == 0)
                {
                    Vector2 pos = previousPostions[i];
                    float rot = previousRotations[i];

                    Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust d = Dust.NewDustPerfect(pos + vel, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DeepSkyBlue * 0.75f, Scale: Main.rand.NextFloat(0.4f, 0.8f) * 0.75f);
                    //d.customData = new GlowFlareBehavior(GlowThreshold: 0.45f, GlowPower: 2.5f, TotalBoost: 1f);
                    d.velocity += rot.ToRotationVector2() * Main.rand.NextFloat(2f, 6f);
                }

            }

            for (int k = 0; k < 5 + Main.rand.Next(4, 9); k++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowFlare>(), vel, newColor: Color.DeepSkyBlue * 0.75f, Scale: Main.rand.NextFloat(0.4f, 1f));
                //d.customData = new GlowFlareBehavior(GlowThreshold: 0.45f, GlowPower: 2.5f, TotalBoost: 1f);
                d.velocity += projectile.velocity * 0.4f;
            }


            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/ENV_water_splash_01") with { Volume = 0.2f, Pitch = -0.25f, PitchVariance = 0.2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
