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


namespace VFXPlus.Content.Weapons.Ranged.Ammo.Darts
{
    public class IchorDartOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.IchorDart);
        }

        int dustRandomOffsetTime = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            
            int trailCount = 19; // 14
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            if (timer == 0)
                dustRandomOffsetTime = Main.rand.Next(0, 3);

            if ((timer + dustRandomOffsetTime) % 5 == 0 && Main.rand.NextBool(4) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-10f, 10f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3f, 9f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 0.8f, newColor: Color.DarkGoldenrod * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                dp.alpha = 12;
                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0.35f);
            }

            float fadeInTime = Math.Clamp((timer + 16f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;

            #region vanillaAI
            projectile.ai[0] += 1f;

            projectile.localAI[0] += 1f;
            if (projectile.localAI[0] > 3f)
            {
                projectile.alpha = 0;
            }
            if (projectile.ai[0] >= 20f)
            {
                projectile.ai[0] = 20f;
                if (projectile.type != 477)
                {
                    projectile.velocity.Y += 0.075f;
                }
            }
            if (projectile.type == 479 && Main.myPlayer == projectile.owner)
            {
                if (projectile.ai[1] >= 0f)
                {
                    projectile.maxPenetrate = (projectile.penetrate = -1);
                }
                else if (projectile.penetrate < 0)
                {
                    projectile.maxPenetrate = (projectile.penetrate = 1);
                }
                if (projectile.ai[1] >= 0f)
                {
                    projectile.ai[1] += 1f;
                }
                if (projectile.ai[1] > (float)Main.rand.Next(5, 30))
                {
                    projectile.ai[1] = -1000f;
                    float num134 = projectile.velocity.Length();
                    Vector2 vector25 = projectile.velocity;
                    vector25.Normalize();
                    int num135 = Main.rand.Next(2, 4);
                    if (Main.rand.Next(4) == 0)
                    {
                        num135++;
                    }
                    for (int num136 = 0; num136 < num135; num136++)
                    {
                        Vector2 vector26 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                        vector26.Normalize();
                        vector26 += vector25 * 2f;
                        vector26.Normalize();
                        vector26 *= num134;
                        Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center.X, projectile.Center.Y, vector26.X, vector26.Y, projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, -1000f);
                    }

                    int a = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<IchorHitFlare>(), 0, 0f, projectile.owner);
                    (Main.projectile[a].ModProjectile as IchorHitFlare).starRotDir = (projectile.velocity.X > 0 ? true : false);

                    SoundStyle style = new SoundStyle("Terraria/Sounds/Item_171") with { Volume = .25f, Pitch = .4f, PitchVariance = 0.1f, MaxInstances = -1 }; 
                    SoundEngine.PlaySound(style, projectile.Center);

                    for (int i = 0; i < 4 + Main.rand.Next(1, 3); i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                        Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                            newColor: Color.DarkOrange, Scale: Main.rand.NextFloat(0.25f, 0.3f) * projectile.scale);
                    }
                }
            }

            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            if (projectile.velocity.Y > 16f)
            {
                projectile.velocity.Y = 16f;
            }

            #endregion

            return false;
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, true);
            });
            DrawTrail(projectile, false);

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.White with { A = 0 } * 0.4f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }
        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.Yellow, Color.DarkGoldenrod, 1f - Easings.easeInCubic(progress)) * progress;

                float size1 = (0.5f + (0.5f * progress)) * projectile.scale;

                Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.45f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);

                if (i > 1)
                {
                    float middleProg = (float)(i - 1) / previousPostions.Count;

                    float size2 = (0.5f + (0.5f * progress));
                    Vector2 vec2Scale = new Vector2(1.5f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                    Main.EntitySpriteDraw(flare, AfterImagePos, null, new Color(255, 255, 100) with { A = 0 } * 0.1f * middleProg,
                        previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SE);
                }
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, projectile.Center);


            for (int num351 = 0; num351 < 4 + Main.rand.Next(3); num351++)
            {
                int num353 = Dust.NewDust(projectile.Center, 10, 10, DustID.Ichor, newColor: Color.Orange, Scale: 0.85f);
                Main.dust[num353].noGravity = true;
                Dust dust267 = Main.dust[num353];
                Dust dust334 = dust267;
                dust334.velocity *= 1.5f;
                dust267 = Main.dust[num353];
                dust334 = dust267;
                dust334.velocity -= projectile.oldVelocity * 0.15f; //0.3
                dust267 = Main.dust[num353];
                dust334 = dust267;
                dust334.scale += (float)Main.rand.Next(150) * 0.001f;

            }

            return true;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.Center, oldVelocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    public class IchorHitFlare : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 100;
            Projectile.scale = 1f;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        int timer = 0;
        public float scale = 1.15f;
        public float alpha = 1;
        public Color col = Color.DarkOrange;

        public bool starRotDir = false;

        public override void AI()
        {
            Projectile.scale = Math.Clamp(MathHelper.Lerp(Projectile.scale, -0.5f, 0.08f), 0, 10);
            Projectile.rotation += 0.2f * (starRotDir ? -1 : 1);

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;

            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.Black * alpha * 0.2f, Projectile.rotation * -1, Flare.Size() / 2, Projectile.scale * scale, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, col with { A = 0 } * alpha, Projectile.rotation * -1, Flare.Size() / 2, Projectile.scale * scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flare, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * alpha, Projectile.rotation, Flare.Size() / 2, Projectile.scale * 0.5f * scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
