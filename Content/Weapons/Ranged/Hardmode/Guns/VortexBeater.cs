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
using Terraria.Graphics;
using Terraria.Physics;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Guns
{
    public class VortexBeaterHeldProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VortexBeater);
        }

        float GunDirection = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            //Changes:
            //Lower sound volume dear god
            //Dust out of muzzle
            #region vanillaAI
            Player player = Main.player[projectile.owner];
            float num = (float)Math.PI / 2f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            int num12 = 2;
            float num23 = 0f;
            if (projectile.type == 615)
            {
                num = 0f;
                if (projectile.spriteDirection == -1)
                {
                    num = (float)Math.PI;
                }
                projectile.ai[0] += 1f;
                int num47 = 0;
                if (projectile.ai[0] >= 40f)
                {
                    num47++;
                }
                if (projectile.ai[0] >= 80f)
                {
                    num47++;
                }
                if (projectile.ai[0] >= 120f)
                {
                    num47++;
                }
                int num48 = 5;
                int num49 = 0;
                projectile.ai[1] -= 1f;
                bool flag3 = false;
                int num50 = -1;
                if (projectile.ai[1] <= 0f)
                {
                    projectile.ai[1] = num48 - num49 * num47;
                    flag3 = true;
                    if ((int)projectile.ai[0] / (num48 - num49 * num47) % 7 == 0)
                    {
                        num50 = 0;
                    }
                }
                projectile.frameCounter += 1 + num47;
                if (projectile.frameCounter >= 4)
                {
                    projectile.frameCounter = 0;
                    projectile.frame++;
                    if (projectile.frame >= Main.projFrames[projectile.type])
                    {
                        projectile.frame = 0;
                    }
                }
                if (projectile.soundDelay <= 0)
                {
                    projectile.soundDelay = num48 - num49 * num47;
                    if (projectile.ai[0] != 1f)
                    {
                        //SoundEngine.PlaySound(in SoundID.Item36 with { Volume = 0.5f }, projectile.position);
                        SoundEngine.PlaySound(SoundID.Item36 with { Volume = 0.35f }, projectile.position);
                    }
                }
                if (flag3 && Main.myPlayer == projectile.owner)
                {
                    bool canShoot = player.channel && player.HasAmmo(player.inventory[player.selectedItem]) && !player.noItems && !player.CCed; //!? for HasAmmo(, canUse: true)
                    int projToShoot = 14;
                    float speed = 14f;
                    int Damage = player.GetWeaponDamage(player.inventory[player.selectedItem]);
                    float KnockBack = player.inventory[player.selectedItem].knockBack;
                    if (canShoot)
                    {
                        //player.PickAmmo(player.inventory[player.selectedItem], ref projToShoot, ref speed, ref canShoot, ref Damage, ref KnockBack, out var usedAmmoItemId);
                        //IEntitySource projectileSource_Item_WithPotentialAmmo = player.GetProjectileSource_Item_WithPotentialAmmo(player.HeldItem, usedAmmoItemId);

                        //Tmod version of PickAmmoFunction
                        int usedAmmoItemId = 0;
                        player.PickAmmo(player.inventory[player.selectedItem], out projToShoot, out speed, out Damage, out KnockBack, out usedAmmoItemId);

                        //I think this is equivalent 
                        IEntitySource projectileSource_Item_WithPotentialAmmo = projectile.GetSource_FromThis();


                        KnockBack = player.GetWeaponKnockback(player.inventory[player.selectedItem], KnockBack);
                        float num51 = player.inventory[player.selectedItem].shootSpeed * projectile.scale;
                        Vector2 vector18 = vector;
                        Vector2 value11 = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector18;
                        if (player.gravDir == -1f)
                        {
                            value11.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector18.Y;
                        }
                        Vector2 spinningpoint7 = Vector2.Normalize(value11);
                        if (float.IsNaN(spinningpoint7.X) || float.IsNaN(spinningpoint7.Y))
                        {
                            spinningpoint7 = -Vector2.UnitY;
                        }
                        spinningpoint7 *= num51;
                        spinningpoint7 = spinningpoint7.RotatedBy(Main.rand.NextDouble() * 0.13089969754219055 - 0.065449848771095276);
                        if (spinningpoint7.X != projectile.velocity.X || spinningpoint7.Y != projectile.velocity.Y)
                        {
                            projectile.netUpdate = true;
                        }
                        projectile.velocity = spinningpoint7;
                        for (int n = 0; n < 1; n++)
                        {
                            Vector2 spinningpoint8 = Vector2.Normalize(projectile.velocity) * speed;
                            spinningpoint8 = spinningpoint8.RotatedBy(Main.rand.NextDouble() * 0.19634954631328583 - 0.098174773156642914);
                            if (float.IsNaN(spinningpoint8.X) || float.IsNaN(spinningpoint8.Y))
                            {
                                spinningpoint8 = -Vector2.UnitY;
                            }
                            Projectile.NewProjectile(projectileSource_Item_WithPotentialAmmo, vector18.X, vector18.Y, spinningpoint8.X, spinningpoint8.Y, projToShoot, Damage, KnockBack, projectile.owner);

                            for (int i = 0; i < 2; i++)
                            {
                                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 1.75f);
                                int dir = projectile.velocity.X > 0 ? 1 : -1;
                                Vector2 posOffset = new Vector2(51f, -5f * dir).RotatedBy(projectile.velocity.ToRotation());

                                
                                Dust d = Dust.NewDustPerfect(vector18, 229, vel, newColor: Color.Aqua * 1f, Scale: Main.rand.NextFloat(0.5f, 1f) * 0.65f);
                                d.noGravity = true;

                                d.position += posOffset;
                                d.velocity += spinningpoint8.SafeNormalize(Vector2.UnitX) * 6f;                    
                            }

                            GunDirection = projectile.velocity.ToRotation();
                            muzzleFlashNum = Main.rand.Next(1, 4);
                            muzzleFlashAlpha = 1f;
                        }
                        if (num50 == 0)
                        {
                            projToShoot = 616;
                            speed = 8f;
                            for (int num52 = 0; num52 < 1; num52++)
                            {
                                Vector2 spinningpoint9 = Vector2.Normalize(projectile.velocity) * speed;
                                spinningpoint9 = spinningpoint9.RotatedBy(Main.rand.NextDouble() * 0.39269909262657166 - 0.19634954631328583);
                                if (float.IsNaN(spinningpoint9.X) || float.IsNaN(spinningpoint9.Y))
                                {
                                    spinningpoint9 = -Vector2.UnitY;
                                }
                                Projectile.NewProjectile(projectileSource_Item_WithPotentialAmmo, vector18.X, vector18.Y, spinningpoint9.X, spinningpoint9.Y, projToShoot, Damage + 20, KnockBack * 1.25f, projectile.owner);
                            }
                        }
                    }
                    else
                    {
                        projectile.Kill();
                    }
                }
                else muzzleFlashAlpha = Math.Clamp(MathHelper.Lerp(muzzleFlashAlpha, -0.5f, 0.14f), 0f, 1f);

            }

            projectile.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            projectile.timeLeft = 2;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.SetDummyItemTime(num12);
            player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(projectile.velocity.Y * (float)projectile.direction, projectile.velocity.X * (float)projectile.direction) + num23);

            if (projectile.type == 615)
            {
                projectile.position.Y += player.gravDir * 2f;
            }

            #endregion


            timer++;
            return false;
        }

        int muzzleFlashNum = 0;
        float muzzleFlashAlpha = 0f;
        Effect myEffect = null;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Player owner = Main.player[projectile.owner];
            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D GlowMask = TextureAssets.GlowMask[192].Value;

            String path = "Assets/MuzzleFlashes/Sprite/MiddleMuzzleFlash" + muzzleFlashNum;

            Texture2D MuzzleFlash = Mod.Assets.Request<Texture2D>(path + "Blue").Value;
            Texture2D MuzzleFlashGlow = Mod.Assets.Request<Texture2D>(path + "Glow").Value;

            Vector2 muzzleFlashPos = drawPos + new Vector2(42f, -5f * owner.direction).RotatedBy(projectile.velocity.ToRotation()); //33 -3
            Vector2 muzzleFlashOrigin = new Vector2(MuzzleFlash.Width / 2f, MuzzleFlash.Height / 2f);

            float easedMuzzleFlashAlpha = Easings.easeOutSine(muzzleFlashAlpha);
            float muzzleFlashScale = projectile.scale * 2f * Easings.easeOutSine(muzzleFlashAlpha);

            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.Aquamarine with { A = 0 } * easedMuzzleFlashAlpha * 0.75f, 
                projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, se, 0f);
            Main.spriteBatch.Draw(MuzzleFlash, muzzleFlashPos, null, Color.White * easedMuzzleFlashAlpha, projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, se, 0f);
            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos, null, Color.SkyBlue with { A = 0 } * muzzleFlashAlpha, projectile.rotation, muzzleFlashOrigin, 3f * (1f - muzzleFlashAlpha), se, 0f);



            Main.EntitySpriteDraw(vanillaTex, drawPos + new Vector2(0f, 0f), sourceRectangle, lightColor, projectile.rotation, TexOrigin, 1f, se);

            for (int i = 0; i < 3; i++)
            {
                Vector2 offPos = drawPos + new Vector2(0f, 0f) + Main.rand.NextVector2Circular(3f, 3f);
                Main.EntitySpriteDraw(GlowMask, offPos, sourceRectangle, Color.White with { A = 0 } * 0.25f, projectile.rotation, TexOrigin, 1f, se);
            }


            Main.EntitySpriteDraw(GlowMask, drawPos + new Vector2(0f, 0f), sourceRectangle, Color.White with { A = 0 }, projectile.rotation, TexOrigin, 1f, se);

            return false;
        }

    }


    public class VortexBeaterRocketOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VortexBeaterRocket);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 25 * 2; //
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            //Add 2 positions per frame
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            if (timer % 5 == 0 && timer > 20)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);

                Dust dp = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, 0f), ModContent.DustType<GlowPixelAlts>(), vel, newColor: Color.MediumAquamarine, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 0.45f);
                dp.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 1f;
                dp.alpha = 2;
            }

            if (timer % 12 == 0 && timer != 0)
            {
                float circlePulseSize = 0.05f;

                Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), projectile.velocity * 0.25f, newColor: Color.Aquamarine);
                CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 3, 0.2f, 0.35f);
                b2.drawLayer = "Dusts";
                d2.customData = b2;
                d2.scale = circlePulseSize * 0.75f;
            }

            #region vanillaAI without dust
            if (projectile.alpha < 170 && false)
            {
                float num137 = 3f;
                for (int num138 = 0; (float)num138 < num137; num138++)
                {
                    int num139 = Dust.NewDust(projectile.position, 1, 1, 229);
                    Main.dust[num139].position = projectile.Center - projectile.velocity / num137 * num138;
                    Main.dust[num139].velocity *= 0f;
                    Main.dust[num139].noGravity = true;
                    Main.dust[num139].alpha = 200;
                    Main.dust[num139].scale = 0.5f;
                }
            }
            float num140 = (float)Math.Sqrt(projectile.velocity.X * projectile.velocity.X + projectile.velocity.Y * projectile.velocity.Y);
            float num141 = projectile.localAI[0];
            if (num141 == 0f)
            {
                projectile.localAI[0] = num140;
                num141 = num140;
            }
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 25;
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            float num142 = projectile.position.X;
            float num144 = projectile.position.Y;
            float num145 = 800f;
            bool flag6 = false;
            int num146 = 0;
            projectile.ai[0] += 1f;
            if (projectile.ai[0] > 20f)
            {
                projectile.ai[0] -= 1f;
                if (projectile.ai[1] == 0f)
                {
                    for (int num147 = 0; num147 < 200; num147++)
                    {
                        if (Main.npc[num147].CanBeChasedBy(this) && (projectile.ai[1] == 0f || projectile.ai[1] == (float)(num147 + 1)))
                        {
                            float num148 = Main.npc[num147].position.X + (float)(Main.npc[num147].width / 2);
                            float num149 = Main.npc[num147].position.Y + (float)(Main.npc[num147].height / 2);
                            float num150 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num148) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num149);
                            if (num150 < num145 && Collision.CanHit(new Vector2(projectile.position.X + (float)(projectile.width / 2), projectile.position.Y + (float)(projectile.height / 2)), 1, 1, Main.npc[num147].position, Main.npc[num147].width, Main.npc[num147].height))
                            {
                                num145 = num150;
                                num142 = num148;
                                num144 = num149;
                                flag6 = true;
                                num146 = num147;
                            }
                        }
                    }
                    if (flag6)
                    {
                        projectile.ai[1] = num146 + 1;
                    }
                    flag6 = false;
                }
                if (projectile.ai[1] != 0f)
                {
                    int num151 = (int)(projectile.ai[1] - 1f);
                    if (Main.npc[num151].active && Main.npc[num151].CanBeChasedBy(this, ignoreDontTakeDamage: true))
                    {
                        float num152 = Main.npc[num151].position.X + (float)(Main.npc[num151].width / 2);
                        float num153 = Main.npc[num151].position.Y + (float)(Main.npc[num151].height / 2);
                        if (Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num152) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num153) < 1000f)
                        {
                            flag6 = true;
                            num142 = Main.npc[num151].position.X + (float)(Main.npc[num151].width / 2);
                            num144 = Main.npc[num151].position.Y + (float)(Main.npc[num151].height / 2);
                        }
                    }
                }
                if (!projectile.friendly)
                {
                    flag6 = false;
                }
                if (flag6)
                {
                    float num246 = num141;
                    Vector2 vector27 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                    float num155 = num142 - vector27.X;
                    float num156 = num144 - vector27.Y;
                    float num157 = (float)Math.Sqrt(num155 * num155 + num156 * num156);
                    num157 = num246 / num157;
                    num155 *= num157;
                    num156 *= num157;
                    int num158 = 8;
                    projectile.velocity.X = (projectile.velocity.X * (float)(num158 - 1) + num155) / (float)num158;
                    projectile.velocity.Y = (projectile.velocity.Y * (float)(num158 - 1) + num156) / (float)num158;
                }

            }
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;

            #endregion


            totalAlpha = Math.Clamp(MathHelper.Lerp(totalAlpha, 1.15f, 0.05f), 0f, 1f);
            totalScale = Math.Clamp(MathHelper.Lerp(totalScale, 1.35f, 0.03f), 0f, 1f);

            Lighting.AddLight(projectile.Center, Color.Aquamarine.ToVector3() * 0.65f);

            timer++;
            return false;
        }


        float totalAlpha = 0f;
        float totalScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D GlowMask = TextureAssets.GlowMask[193].Value;
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 drawScale = new Vector2(totalScale, 1f) * projectile.scale;// * totalScale;

            //Bloomball
            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;
            Vector2 ballScale = new Vector2(0.85f * totalScale * projectile.scale, 1f) * drawScale;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawVertexTrail(false);


                Main.EntitySpriteDraw(Ball, drawPos, null, Color.Aquamarine with { A = 0 } * totalAlpha * 0.2f, projectile.rotation, Ball.Size() / 2f, ballScale * 0.5f, se);
                Main.EntitySpriteDraw(Ball, drawPos, null, Color.Aquamarine with { A = 0 } * totalAlpha * 0.5f, projectile.rotation, Ball.Size() / 2f, ballScale * 0.3f, se);
            });

            DrawVertexTrail(true);



            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 3f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.15f * projectile.direction);

                Main.EntitySpriteDraw(GlowMask, offsetDrawPos, sourceRectangle,
                    Color.White with { A = 0 } * 1.2f * totalAlpha, projectile.rotation, TexOrigin, drawScale * 1.1f, se);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * totalAlpha, projectile.rotation, TexOrigin, drawScale, se);

            Main.EntitySpriteDraw(GlowMask, drawPos, sourceRectangle, Color.White with { A = 0 } * totalAlpha, projectile.rotation, TexOrigin, drawScale, se);


            return false;
        }


        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp || false)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/Trail5Loop").Value; //ThinGlowLine
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //ThinGlowLine

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.15f;

            Color StripColor(float progress) => Color.White * Easings.easeInSine(progress);
            float StripWidth(float progress) => Math.Clamp(120f * sineWidthMult * totalScale * Easings.easeInSine(progress), 20f, 100f) * 0.3f;
            float StripWidth2(float progress) => Math.Clamp(75f * sineWidthMult * totalScale * Easings.easeInSine(progress), 20f, 100f) * 0.3f; //75


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //timer * 0.02
            myEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(Color.Aquamarine.ToVector3() * 2.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.Aquamarine.ToVector3() * 2f);
            myEffect.Parameters["glowThreshold"].SetValue(0.8f);
            myEffect.Parameters["glowIntensity"].SetValue(1.2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            #region vanilla Kill
            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = 80);
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            for (int num359 = 0; num359 < 4; num359++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
            }
            for (int num360 = 0; num360 < 40; num360++)
            {
                //int num361 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 229, 0f, 0f, 200, default(Color), 2.5f);
                //Main.dust[num361].noGravity = true;
                //Dust dust260 = Main.dust[num361];
                //Dust dust334 = dust260;
                //dust334.velocity *= 2f;
                //num361 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 229, 0f, 0f, 200, default(Color), 1.5f);
                //dust260 = Main.dust[num361];
                //dust334 = dust260;
                //dust334.velocity *= 1.2f;
                //Main.dust[num361].noGravity = true;
            }
            for (int num362 = 0; num362 < 1; num362++)
            {
                //int num365 = Gore.NewGore(projectile.position + new Vector2((float)(projectile.width * Main.rand.Next(100)) / 100f, (float)(projectile.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64));
                //Gore gore55 = Main.gore[num365];
                //Gore gore64 = gore55;
                //gore64.velocity *= 0.3f;
                //Main.gore[num365].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                //Main.gore[num365].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
            }
            projectile.Damage();
            #endregion

            #region SoftGlow + Circle
            Dust softGlow = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Aqua * 0.75f, Scale: 0.25f);
            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.91f, sizeChangeSpeed: 0.92f, timeToKill: 150,
                overallAlpha: 0.25f, DrawWhiteCore: true, 1f, 1f);

            Dust softGlow2 = Dust.NewDustPerfect(projectile.Center + projectile.velocity, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Aqua * 1.25f, Scale: 0.15f);
            softGlow2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 2, timeToChangeScale: 0, fadeSpeed: 0.91f, sizeChangeSpeed: 0.92f, timeToKill: 150,
                overallAlpha: 0.25f, DrawWhiteCore: true, 1f, 1f);

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.3f, true, 1, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Aquamarine * 0.5f);
            d1.scale = 0.1f;
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f).RotatedBy(0f);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.Aquamarine * 0.5f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f).RotatedBy(0f);
            #endregion

            for (int i = 0; i < 16; i++)
            {
                float prog = (float)i / 16f;

                float proggg = Main.rand.NextFloat();

                Color colA = Color.Lerp(new Color(0, 240, 145), Color.Aqua, 0.85f);
                Color colB = new Color(0, 240, 145);

                Color col = Color.Lerp(colA with { A = 0 } * 1f * Main.rand.NextFloat(0.5f, 1f), Color.Black * 0.75f, 1f - prog);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 2f) * 2.5f,
                    newColor: col * prog, Scale: Main.rand.NextFloat(0.9f, 1.3f) * 1f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(15, 25), 0.93f, 0.01f, 0.9f); //12 28

            }

            for (int i = 0; i < 15 + Main.rand.Next(0, 4); i++)
            {
                
                float velMult = Main.rand.NextFloat(3f, 7f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                
                if (Main.rand.NextBool())
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, Alpha: 0, newColor: Color.Aqua, Scale: Main.rand.NextFloat(0.35f, 0.65f));
                    dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.91f, velToBeginShrink: 3f, colorFadePower: 1f);
                }
                else
                {
                    Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart * 0.5f, newColor: Color.Aquamarine, Scale: Main.rand.NextFloat(0.65f, 0.75f) * 0.65f);

                    dust.noLight = false;
                    dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
                }

            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
