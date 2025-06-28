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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VFXPlus.Common.Drawing;
using static tModPorter.ProgressUpdate;
using Terraria.Graphics.Shaders;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class LaserMachineGun : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.LaserMachinegun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LaserMachinegunToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }

    }
    public class LaserMachineGunHeldProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LaserMachinegun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LaserMachinegunToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            
            int trailCount = 10;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            #region vanilla code dear god

            Player player = Main.player[projectile.owner];
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            float num = (float)Math.PI / 2f;
            int num12 = 2;
            float num23 = 0f;

            projectile.ai[0] += 1f;
            int num34 = 0;
            if (projectile.ai[0] >= 40f)
            {
                num34++;
            }
            if (projectile.ai[0] >= 80f)
            {
                num34++;
            }
            if (projectile.ai[0] >= 120f)
            {
                num34++;
            }
            int num45 = 24;
            int num56 = 6;
            projectile.ai[1] += 1f;
            bool flag = false;
            if (projectile.ai[1] >= (float)(num45 - num56 * num34))
            {
                projectile.ai[1] = 0f;
                flag = true;
            }
            projectile.frameCounter += 1 + num34;
            if (projectile.frameCounter >= 4)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame >= 6)
                {
                    projectile.frame = 0;
                }
            }
            if (projectile.soundDelay <= 0)
            {
                projectile.soundDelay = num45 - num56 * num34;
                if (projectile.ai[0] != 1f)
                {
                    //SoundStyle style = new SoundStyle("Terraria/Sounds/Research_3") with { Volume = 0.3f, Pitch = .65f, PitchVariance = .2f };
                    //SoundEngine.PlaySound(style, projectile.Center);
                    //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_158") with { Volume = 1f, Pitch = .45f, PitchVariance = 0.1f };
                    //SoundEngine.PlaySound(style2, projectile.Center);

                    SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.5f }, projectile.Center);
                }
            }
            if (projectile.ai[1] == 1f && projectile.ai[0] != 1f)
            {
                Vector2 spinningpoint = Vector2.UnitX * 24f;
                spinningpoint = spinningpoint.RotatedBy(projectile.rotation - (float)Math.PI / 2f);
                Vector2 vector12 = projectile.Center + spinningpoint;
                for (int i = 20; i < 2; i++)
                {
                    int num66 = Dust.NewDust(vector12 - Vector2.One * 8f, 16, 16, 135, projectile.velocity.X / 2f, projectile.velocity.Y / 2f, 100);
                    Main.dust[num66].velocity *= 0.66f;
                    Main.dust[num66].noGravity = true;
                    Main.dust[num66].scale = 1.4f;
                }
            }
            if (flag && Main.myPlayer == projectile.owner)
            {
                if (player.channel && player.CheckMana(player.inventory[player.selectedItem], -1, pay: true) && !player.noItems && !player.CCed)
                {
                    float num77 = player.inventory[player.selectedItem].shootSpeed * projectile.scale;
                    Vector2 vector23 = vector;
                    Vector2 value = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector23;
                    if (player.gravDir == -1f)
                    {
                        value.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector23.Y;
                    }
                    Vector2 vector34 = Vector2.Normalize(value);
                    if (float.IsNaN(vector34.X) || float.IsNaN(vector34.Y))
                    {
                        vector34 = -Vector2.UnitY;
                    }
                    vector34 *= num77;
                    if (vector34.X != projectile.velocity.X || vector34.Y != projectile.velocity.Y)
                    {
                        projectile.netUpdate = true;
                    }
                    projectile.velocity = vector34;
                    int num79 = ProjectileID.LaserMachinegunLaser;
                    float num2 = 14f;
                    int num3 = 7;
                    for (int j = 0; j < 2; j++)
                    {
                        vector23 = projectile.Center + new Vector2(Main.rand.Next(-num3, num3 + 1), Main.rand.Next(-num3, num3 + 1));
                        Vector2 spinningpoint3 = Vector2.Normalize(projectile.velocity) * num2;
                        spinningpoint3 = spinningpoint3.RotatedBy(Main.rand.NextDouble() * 0.19634954631328583 - 0.098174773156642914);
                        if (float.IsNaN(spinningpoint3.X) || float.IsNaN(spinningpoint3.Y))
                        {
                            spinningpoint3 = -Vector2.UnitY;
                        }
                        Projectile.NewProjectile(null, vector23.X, vector23.Y, spinningpoint3.X, spinningpoint3.Y, num79, projectile.damage, projectile.knockBack, projectile.owner);

                        //Vector2 CCShot = spinningpoint3.SafeNormalize(Vector2.UnitX) * 40f;
                        //Projectile.NewProjectile(null, vector23.X, vector23.Y, CCShot.X , CCShot.Y, num79, projectile.damage, projectile.knockBack, projectile.owner);
                    }

                    ShootFX(projectile);
                }
                else
                {
                    projectile.Kill();
                }
            }
            projectile.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            projectile.timeLeft = 2;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.SetDummyItemTime(num12);
            player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(projectile.velocity.Y * (float)projectile.direction, projectile.velocity.X * (float)projectile.direction) + num23);

            #endregion

            timer++;
            return false;// base.PreAI(projectile);
        }

        public void ShootFX(Projectile projectile)
        {
            Vector2 offsetPos = projectile.velocity.SafeNormalize(Vector2.UnitX) * 5f + projectile.Center;

            Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * 2.5f; //3f
            Dust d = Dust.NewDustPerfect(offsetPos, ModContent.DustType<CirclePulse>(), vel, newColor: Color.DodgerBlue);
            //d.scale = 0.15f;
            CirclePulseBehavior b = new CirclePulseBehavior(0.25f, true, 1, 0.4f, 0.8f);
            b.drawLayer = "Dusts";
            d.customData = b;

            Dust d2 = Dust.NewDustPerfect(offsetPos, ModContent.DustType<CirclePulse>(), vel * 1.2f, newColor: Color.DodgerBlue);
            CirclePulseBehavior b2 = new CirclePulseBehavior(0.25f, true, 1, 0.2f, 0.4f);
            b2.drawLayer = "Dusts";
            d2.customData = b2;

            //Dust
            for (int i = 0; i < 8 - Main.rand.Next(0, 3); i++) //6
            {
                Vector2 offsetPos2 = projectile.velocity.SafeNormalize(Vector2.UnitX) * 10f + projectile.Center;

                Dust dp = Dust.NewDustPerfect(offsetPos2, ModContent.DustType<LineSpark>(),
                    vel.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f)) * Main.rand.Next(7, 18),
                    newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.45f, 0.6f) * 0.45f); //35

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f);

            }

            //SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.75f, Pitch = 0.25f }, projectile.Center);

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Research_3") with { Volume = 0.3f, Pitch = .65f, PitchVariance = .2f };
            //SoundEngine.PlaySound(style, projectile.Center);
            //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_158") with { Volume = 0.4f, Pitch = .5f, PitchVariance = 0.15f };
            //SoundEngine.PlaySound(style2, projectile.Center);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Glowmask is Glow_35
            
            Texture2D Tex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D GlowMask = TextureAssets.GlowMask[35].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Tex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            float rot = projectile.velocity.ToRotation() + MathHelper.PiOver2;
            SpriteEffects se = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            Main.EntitySpriteDraw(Tex, drawPos + new Vector2(0f, 0f), sourceRectangle, lightColor, rot, TexOrigin, projectile.scale, se);


            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color col = Color.SkyBlue with { A = 0 } * Easings.easeInCubic(progress);
                Color col2 = Color.DodgerBlue with { A = 0 } * progress;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(GlowMask, AfterImagePos, sourceRectangle, col * 0.8f,
                      previousRotations[i], TexOrigin, projectile.scale, SpriteEffects.None);
            }


            Main.EntitySpriteDraw(GlowMask, drawPos + new Vector2(0f, 0f), sourceRectangle, Color.White with { A = 0 }, rot, TexOrigin, projectile.scale, se);

            return false;
        }
    }

    public class LaserMachinegunShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LaserMachinegunLaser) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.LaserMachinegunToggle;
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            if (timer % 2 == 0 && projectile.ai[1] == 0)
            {
                int trailCount = 30;
                previousRotations.Add(projectile.rotation);
                previousPositions.Add(projectile.Center);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPositions.Count > trailCount)
                    previousPositions.RemoveAt(0);


                bool addInBetween = true;
                if (addInBetween)
                {
                    previousRotations.Add(projectile.rotation);
                    previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);

                    if (previousRotations.Count > trailCount)
                        previousRotations.RemoveAt(0);

                    if (previousPositions.Count > trailCount)
                        previousPositions.RemoveAt(0);
                }
            }
            else if (projectile.ai[1] == 1)
            {
                if (timer % 1 == 0)
                {
                    if (previousPositions.Count > 0)
                    {
                        previousRotations.RemoveAt(0);
                        previousPositions.RemoveAt(0);
                    }
                }
            }
            
            if (timer % 5 == 0 && timer > 6 && projectile.ai[1] != 1 && Main.rand.NextBool(2))
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), dustVel, newColor: Color.DeepSkyBlue * 0.75f, Scale: Main.rand.NextFloat(0.15f, 0.25f) * 1.75f);
                da.velocity += projectile.velocity.RotatedByRandom(0.2f) * 0.65f;
                da.alpha = 12;
            }


            timer++;
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawLaserShot(projectile);
            });

            //projectile.ai[1] becomes 1 when tile it

            return false;
        }


        public void DrawLaserShot(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;
            Texture2D line2 = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            for (int i = 220; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float easedProg = Easings.easeInCirc(progress);

                float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.05f) * 0.15f;

                float size = Easings.easeOutSine(1f * progress) * projectile.scale + sineScale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                float colPower = easedProg;


                Color newBlue = Color.Lerp(Color.Blue, Color.DeepSkyBlue, 0.85f);

                Color newCol = newBlue;//Color.Lerp(Color.White, newBlue, progress);
                Color newCol2 = Color.DodgerBlue * 1.5f;


                Vector2 lineScale = new Vector2(2.25f, 0.3f + 0.4f * progress * 1.75f);
                Vector2 lineScale2 = new Vector2(2.25f, 0.15f + 0.2f * progress * 1.25f);

                //Black
                //Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.2f * progress * progress * progress,
                //    projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * projectile.scale, SpriteEffects.None);

                //Main
                Main.EntitySpriteDraw(line2, AfterImagePos, null, newCol with { A = 0 } * 0.5f * progress * progress,
                    projectile.velocity.ToRotation(), line2.Size() / 2f, lineScale * projectile.scale, SpriteEffects.None);

                if (progress == 0)
                {
                    Main.EntitySpriteDraw(line, AfterImagePos, null, newCol with { A = 0 } * 0.5f * progress * progress,
                        projectile.velocity.ToRotation(), line.Size() / 2f, lineScale * projectile.scale, SpriteEffects.None);
                }

                //White
                Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * progress * progress,
                    projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * projectile.scale, SpriteEffects.None);
            }

            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Color color = Main.hslToRgb((timer * 0.01f + projectile.ai[1]) % 1f, 1f, 0.4f, 0);

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Vector2 vec2Scale = new Vector2(1f, 0.8f * progress) * projectile.scale * 0.75f;
                Vector2 vec2Scale2 = new Vector2(1f, 0.4f * progress) * projectile.scale * 0.75f;

                Main.EntitySpriteDraw(line, AfterImagePos, null, color with { A = 0 },
                       previousRotations[i], line.Size() / 2f, vec2Scale, SpriteEffects.None);

                Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 1f * progress,
                       previousRotations[i], line.Size() / 2f, vec2Scale2, SpriteEffects.None);

            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < Main.rand.Next(2, 6); i++)
            {

                float velMult = Main.rand.NextFloat(1.5f, 3f);
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(velMult, velMult) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelGlowOrb>(), randomStart, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.55f, 1f));

                if (dust.scale > 0.9f)
                    dust.velocity *= 0.5f;

                dust.scale *= 0.9f;

                dust.fadeIn = Main.rand.NextFloat(0.25f, 1f);
                dust.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.15f, timeBeforeSlow: 4, postSlowPower: 0.89f, fadePower: 0.9f, velToBeginShrink: 3f, colorFadePower: 1f);
            }

            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DeepSkyBlue, Scale: 0.08f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
                overallAlpha: 0.10f, DrawWhiteCore: true, 1f, 1f);

            return false;
            //return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);
            
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.25f, Pitch = 1f, PitchVariance = 0.25f, MaxInstances = -1 }, projectile.Center);


            projectile.Kill();


            for (int i = 0; i < 2; i++)
            {
                if (previousPositions.Count >= 1)
                {
                    int toRemove = previousPositions.Count - 1;
                    previousPositions.RemoveAt(toRemove);
                    previousRotations.RemoveAt(toRemove);
                }
            }
            
            
            return base.OnTileCollide(projectile, oldVelocity);
        }
    }
}
