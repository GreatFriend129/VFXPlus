using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;
using VFXPlus.Content.VFXTest;
using static tModPorter.ProgressUpdate;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class BetsysWrathOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ApprenticeStaffT3) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BetsysWrathToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }
    public class BetsysWrathShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ApprenticeStaffT3Shot) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BetsysWrathToggle;
        }


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
            }
            
            int trailCount = 14;
            if (timer % 2 == 0)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);
            }


            if (timer % 2 == 0 && Main.rand.NextBool(2) && timer > 10 && false)
            {
                Vector2 dustPos = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * -6f;
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f) - projectile.velocity * 0.4f;

                Color dustCol = Color.Lerp(Color.OrangeRed, Color.Orange, 0.8f);
                float dustScale = Main.rand.NextFloat(0.4f, 0.75f) * 1f;

                Dust smoke = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: Color.Purple * 0.5f, Scale: dustScale);
                smoke.alpha = 2;
            }

            if (timer % 1 == 0 && timer > 3)
            {
                Color colA = Color.Lerp(Color.OrangeRed, Color.Orange, 0.65f);
                Color colB = Color.Lerp(Color.Purple, Color.Red, 0f);

                Color colToUse = Main.rand.NextFloat() < 0.33f ? colB : colA;

                Vector2 vel = -projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f);
                FireParticle fire = new FireParticle(projectile.Center, vel, 0.75f, colToUse, colorMult: 1f, bloomAlpha: 1.65f, AlphaFade: 0.9f);
                fire.scaleFadePower = 1.09f;
                ShaderParticleHandler.SpawnParticle(fire);
            }

            if (timer % 1 == 0 && timer > 10 && false)
            {
                Vector2 VelDir = projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 dustVel = (VelDir * Main.rand.NextFloat(1f, 4.1f)).RotateRandom(0.3f);

                Color colA = Color.Lerp(Color.OrangeRed, Color.Orange, 0.65f);
                Color colB = Color.Lerp(Color.Purple, Color.Red, 0f);

                Color colToUse = Main.rand.NextFloat() < 0.33f ? colB : colA;

                Vector2 newVel = -projectile.velocity.RotateRandom(0.2f) * 0.5f;
                FireParticle fire = new FireParticle(projectile.Center, newVel, 1f, colToUse, colorMult: 1f, bloomAlpha: 1.65f, AlphaFade: 0.88f); //colMult3 || alphafade .92
                fire.scaleFadePower = 1.04f; //1.06 look sweet at higher proj speed
                fire.renderLayer = RenderLayer.UnderProjectiles;
                ShaderParticleHandler.SpawnParticle(fire);
            }

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.1f + MathHelper.Lerp(0f, 0.9f, Easings.easeInOutBack(animProgress, 0f, 1.35f));

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.5f, 0.09f), 0f, 1f);

            //Color lightColor = Color.Lerp(Color.OrangeRed, Color.Orange, 0.3f);
            //Lighting.AddLight(projectile.position, lightColor.ToVector3() * 1.25f * overallScale);

            timer++;

            #region vanillaAI
            //Yes this is actually what it is like
            //It tries to have a max y velocity of 32 but it gets overriden by the default gravity cap lmao

            projectile.ai[0] += 1f;
            if (projectile.ai[0] >= 10f)
            {
                projectile.velocity.Y += 0.1f;
            }
            if (projectile.ai[0] >= 20f)
            {
                projectile.velocity.Y += 0.1f;
            }
            if (projectile.ai[0] > 20f)
            {
                projectile.ai[0] = 20f;
            }
            projectile.velocity.X *= 0.99f;
            if (projectile.velocity.Y > 32f)
            {
                projectile.velocity.Y = 32f;
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }
            #endregion

            return false;
        }

        float overallAlpha = 0f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawFireball(projectile, false);
            });

            DrawFireball(projectile, true);

            return false;
        }

        public void DrawFireball(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D FireBall = Mod.Assets.Request<Texture2D>("Assets/Pixel/FireBallBlur").Value;
            Texture2D FireBallPixel = Mod.Assets.Request<Texture2D>("Assets/Pixel/Extra_91").Value;
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            float rot = projectile.velocity.ToRotation();

            Vector2 totalScale = new Vector2(overallScale, 1f) * projectile.scale * 0.85f * 1.43f;

            Color betweenGold = Color.Lerp(Color.Gold, Color.OrangeRed, 0.6f);//0.75

            Vector2 off = rot.ToRotationVector2() * -10f * totalScale;
            Main.EntitySpriteDraw(Glow, drawPos, null, Color.OrangeRed with { A = 0 } * overallAlpha * 0.2f, rot + MathHelper.PiOver2, Glow.Size() / 2f, totalScale * 1.3f, SpriteEffects.None);


            Color outerCol = Color.Orange * 0.4f;
            for (int i = 0; i < 1; i++)
            {
                Main.EntitySpriteDraw(FireBall, drawPos + off, null, outerCol with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale, SpriteEffects.None);
            }

            #region after image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                Vector2 pos = previousPositions[i] - Main.screenPosition + off;

                float progress = (float)i / previousRotations.Count;

                Vector2 size = (1f - (progress * 0.5f)) * totalScale;

                float colVal = progress;

                //End - Start
                Color col = Color.Lerp(Color.Purple * 3f, betweenGold, Easings.easeOutQuad(progress)) * progress * 0.7f;

                Vector2 size2 = (1f - (progress * 0.15f)) * totalScale;
                Main.EntitySpriteDraw(FireBallPixel, pos + Main.rand.NextVector2Circular(10f, 10f) * (1f - progress), null, col with { A = 0 } * 0.85f * overallAlpha * colVal,
                        previousRotations[i] + MathHelper.PiOver2, FireBallPixel.Size() / 2f, size2, SpriteEffects.None);

                Vector2 vec2Scale = new Vector2(0.25f, 1.15f) * size;

                Main.EntitySpriteDraw(FireBall, pos + Main.rand.NextVector2Circular(0f, 0f) * (1f - progress), null, col with { A = 0 } * 1.25f * overallAlpha * colVal,
                        previousRotations[i] + MathHelper.PiOver2, FireBall.Size() / 2f, vec2Scale * 1.5f, SpriteEffects.None);
            }
            #endregion

            Vector2 v2scale = new Vector2(1f, 1f);

            Main.EntitySpriteDraw(FireBall, drawPos + off + off + Main.rand.NextVector2Circular(2f, 2f), null, betweenGold with { A = 0 } * overallAlpha * 0.75f, rot + MathHelper.PiOver2, FireBall.Size() / 2f, totalScale * v2scale, SpriteEffects.None);

            Main.EntitySpriteDraw(FireBall, drawPos + off, null, Color.White with { A = 0 } * overallAlpha, rot + MathHelper.PiOver2, FireBall.Size() / 2f, v2scale * totalScale * 0.6f, SpriteEffects.None);

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {


            for (int i = 0; i < 10; i++)
            {
                float prog = (float)i / 10;

                Vector2 veloF = Main.rand.NextVector2CircularEdge(7f, 7f) * Easings.easeOutSine(prog) * 1f; //12

                float alphaFade = Main.rand.NextFloat(0.94f, 0.95f);
                float fireScale = Main.rand.NextFloat(1.75f, 2.25f);

                //Color.Lerp(Color.OrangeRed, Color.Red, 0.5f)
                Color colA = Color.Lerp(Color.OrangeRed, Color.Orange, 0.65f); //65
                Color colB = Color.Lerp(Color.Purple, Color.Red, 0.15f);

                Color colToUse = Main.rand.NextFloat() < 0.33f ? colB : colA;

                FireParticle fire = new FireParticle(projectile.Center, veloF, fireScale, colToUse, colorMult: 1.15f, bloomAlpha: 1.5f, AlphaFade: alphaFade, VelFade: 0.9f);
                fire.scaleFadePower = 1.01f; //1.05
                ShaderParticleHandler.SpawnParticle(fire);
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, projectile.Center);

            Vector2 impactPoint = projectile.Center;
            #region vanillaKill
            Rectangle hitbox = projectile.Hitbox;
            //This one is good
            for (int num773 = 0; num773 < projectile.oldPos.Length / 2; num773 += 2)
            {
                hitbox.X = (int)projectile.oldPos[num773].X;
                hitbox.Y = (int)projectile.oldPos[num773].Y;
                for (int num784 = 0; num784 < 1; num784++)
                {
                    int num795 = Utils.SelectRandom<int>(Main.rand, 6, 55, 158);
                    int num807 = Dust.NewDust(hitbox.TopLeft(), projectile.width, projectile.height, num795, projectile.direction, -2.5f);
                    Main.dust[num807].alpha = 200;
                    Dust dust102 = Main.dust[num807];
                    Dust dust334 = dust102;
                    dust334.velocity *= 2.4f;
                    dust102 = Main.dust[num807];
                    dust334 = dust102;
                    dust334.scale += Main.rand.NextFloat();
                    dust102 = Main.dust[num807];
                    dust334 = dust102;
                    dust334.scale -= 0.5f;
                    if (Main.dust[num807].type == 55)
                    {
                        Main.dust[num807].color = Color.Lerp(new Color(128, 0, 180, 128), Color.Gold, Main.rand.NextFloat());
                    }
                    Main.dust[num807].noLight = true;
                }
            }
            //This one is good too
            for (int num818 = 10; num818 < projectile.oldPos.Length; num818 += 2)
            {
                hitbox.X = (int)projectile.oldPos[num818].X;
                hitbox.Y = (int)projectile.oldPos[num818].Y;
                for (int num829 = 0; num829 < 2; num829++)
                {
                    if (Main.rand.Next(2) != 0)
                    {
                        int num840 = Utils.SelectRandom<int>(Main.rand, 55);
                        int num851 = Dust.NewDust(hitbox.TopLeft(), projectile.width, projectile.height, num840, projectile.direction, -2.5f);
                        Main.dust[num851].alpha = 120;
                        Dust dust99 = Main.dust[num851];
                        Dust dust334 = dust99;
                        dust334.velocity *= 2.4f;
                        dust99 = Main.dust[num851];
                        dust334 = dust99;
                        dust334.scale += Main.rand.NextFloat() * 0.7f;
                        dust99 = Main.dust[num851];
                        dust334 = dust99;
                        dust334.scale -= 0.5f;
                        if (Main.dust[num851].type == 55)
                        {
                            Main.dust[num851].color = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat());
                        }
                        Main.dust[num851].noLight = true;
                    }
                }
            }
            //This one is maybe good too
            for (int num862 = 50000; num862 < projectile.oldPos.Length; num862++)
            {
                hitbox.X = (int)projectile.oldPos[num862].X;
                hitbox.Y = (int)projectile.oldPos[num862].Y;
                for (int num873 = 0; num873 < 1; num873++)
                {
                    if (Main.rand.Next(3) != 0)
                    {
                        int num884 = Utils.SelectRandom<int>(Main.rand, 55);
                        int num895 = Dust.NewDust(hitbox.TopLeft(), projectile.width, projectile.height, num884, projectile.direction, -2.5f);
                        Main.dust[num895].alpha = 80;
                        Dust dust96 = Main.dust[num895];
                        Dust dust334 = dust96;
                        dust334.velocity *= 0.3f;
                        dust96 = Main.dust[num895];
                        dust334 = dust96;
                        dust334.velocity += projectile.velocity * 0.5f;
                        dust96 = Main.dust[num895];
                        dust334 = dust96;
                        dust334.scale += Main.rand.NextFloat() * 0.7f;
                        dust96 = Main.dust[num895];
                        dust334 = dust96;
                        dust334.scale -= 0.5f;
                        if (Main.dust[num895].type == 55)
                        {
                            Main.dust[num895].color = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat());
                        }
                        Main.dust[num895].noLight = true;
                    }
                }
            }
            /*
            for (int num906 = 0; num906 < 20; num906++)
            {
                if (Main.rand.Next(3) != 0)
                {
                    int num918 = 228;
                    Dust dust277 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, num918)];
                    dust277.noGravity = true;
                    dust277.scale = 1.25f + Main.rand.NextFloat();
                    dust277.fadeIn = 1.5f;
                    Dust dust94 = dust277;
                    Dust dust334 = dust94;
                    dust334.velocity *= 6f;
                    dust277.noLight = true;
               }
            }
            for (int num929 = 0; num929 < 20; num929++)
            {
                if (Main.rand.Next(3) != 0)
                {
                    int num940 = 55;
                    Dust dust278 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, num940)];
                    dust278.noGravity = true;
                    dust278.scale = 1.25f + Main.rand.NextFloat();
                    dust278.fadeIn = 1.5f;
                    Dust dust91 = dust278;
                    Dust dust334 = dust91;
                    dust334.velocity *= 6f;
                    dust278.noLight = true;
                    dust278.color = new Color(0, 0, 220, 128);
                }
            }
            */
            if (projectile.owner == Main.myPlayer)
            {
                projectile.position = projectile.Center;
                projectile.Size = new Vector2(140f);
                projectile.Center = projectile.position;
                projectile.penetrate = -1;
                projectile.usesLocalNPCImmunity = true;
                projectile.localNPCHitCooldown = -1;
                projectile.Damage();
            }
            #endregion

            return false;
            return base.PreKill(projectile, timeLeft);
        }
    }

}
