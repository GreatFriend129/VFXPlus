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
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.Projectiles;
using VFXPlus.Content.VFXTest.Aero;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Misc
{
    
    public class StyngerOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Stynger);
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.Stynger,
                    AnimTime: 15,
                    NormalXOffset: 16f,
                    DestXOffset: 3f,
                    YRecoilAmount: 0.13f,
                    HoldOffset: new Vector2(0f, -1f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            return true;
        }

    }
    public class StyngerShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Stynger);
        }
        public override bool InstancePerEntity => true;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.rotation);

            int trailCount = 18;
            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;// new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);
                float trailAlpha = progress * progress * projectile.Opacity ;
                Vector2 trailScale = new Vector2(1f * projectile.Opacity, 1f) * progress;

                Main.EntitySpriteDraw(vanillaTex, trailPos, null, Color.White * trailAlpha * 0.5f, projectile.rotation, TexOrigin, trailScale, SE);
            }

            //Border
            for (int i = 0; i < 5; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, null, Color.White with { A = 0 } * projectile.Opacity * 0.8f, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SE);


            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            ProjectileExtensions.DoBaseRocketExplosionFX(projectile, projectile.Center);

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.5f, Pitch = 0.6f, PitchVariance = 0.1f }, projectile.Center);

            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.5f, Pitch = -0.6f, PitchVariance = 0.1f, MaxInstances = -1 }, projectile.Center);

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Item_45") with { Volume = 0.4f, Pitch = -.75f, MaxInstances = -1 };
            SoundEngine.PlaySound(style3, projectile.Center);

            #region vanillaAI
            //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            for (int num937 = 220; num937 < 10; num937++)
            {
                int num938 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust112 = Main.dust[num938];
                Dust dust334 = dust112;
                dust334.velocity *= 0.9f;
            }
            for (int num939 = 220; num939 < 5; num939++)
            {
                int num941 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[num941].noGravity = true;
                Dust dust113 = Main.dust[num941];
                Dust dust334 = dust113;
                dust334.velocity *= 3f;
                num941 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
                dust113 = Main.dust[num941];
                dust334 = dust113;
                dust334.velocity *= 2f;
            }
            //int num942 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
            //Gore gore30 = Main.gore[num942];
            //Gore gore64 = gore30;
            //gore64.velocity *= 0.3f;
            //Main.gore[num942].velocity.X += Main.rand.Next(-1, 2);
            //Main.gore[num942].velocity.Y += Main.rand.Next(-1, 2);
            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 150;
            projectile.height = 150;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            projectile.penetrate = -1;
            projectile.maxPenetrate = 0;
            projectile.Damage();
            if (projectile.owner == Main.myPlayer)
            {
                int num943 = Main.rand.Next(2, 6);
                for (int num944 = 0; num944 < num943; num944++)
                {
                    float num945 = Main.rand.Next(-100, 101);
                    num945 += 0.01f;
                    float num946 = Main.rand.Next(-100, 101);
                    num945 -= 0.01f;
                    float num947 = (float)Math.Sqrt(num945 * num945 + num946 * num946);
                    num947 = 8f / num947;
                    num945 *= num947;
                    num946 *= num947;
                    int num948 = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center.X - projectile.oldVelocity.X, projectile.Center.Y - projectile.oldVelocity.Y, num945, num946, 249, projectile.damage, projectile.knockBack, projectile.owner);
                    Main.projectile[num948].maxPenetrate = 0;
                }
            }
            #endregion

            return false;
        }

    }

    public class StyngerShrapnelOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.StyngerShrapnel);
        }
        public override bool InstancePerEntity => true;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.rotation);

            int trailCount = 10;
            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }


            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);
                float trailAlpha = progress * projectile.Opacity;
                Vector2 trailScale = new Vector2(1f * projectile.Opacity, 1f) * progress;

                Main.EntitySpriteDraw(vanillaTex, trailPos, sourceRectangle, Color.White * trailAlpha * 0.5f, projectile.rotation, TexOrigin, trailScale, SE);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, sourceRectangle, Color.White with { A = 0 } * projectile.Opacity * 0.4f, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SE);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SE);


            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            #region Explosion
            for (int i = 0; i < 2 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 12; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f);

                float progress = (float)i / 11;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.75f, 2.5f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.7f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 1f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.OrangeRed, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 5 + Main.rand.Next(0, 3); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 13, postSlowPower: 0.92f,
                    velToBeginShrink: 4f, fadePower: 0.91f, shouldFadeColor: false);
            }

            #endregion

            SoundStyle styleA = new SoundStyle("VFXPlus/Sounds/Effects/Fire/FlareImpact") with { Volume = 0.15f, Pitch = -.1f, PitchVariance = .25f, };
            SoundEngine.PlaySound(styleA, projectile.Center);

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0f, PitchVariance = 0.25f, MaxInstances = -1 }, projectile.Center);


            #region vanillaKill
            //SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            for (int num949 = 220; num949 < 7; num949++)
            {
                int num950 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust109 = Main.dust[num950];
                Dust dust334 = dust109;
                dust334.velocity *= 0.8f;
            }
            for (int num952 = 220; num952 < 2; num952++)
            {
                int num953 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 2.5f);
                Main.dust[num953].noGravity = true;
                Dust dust111 = Main.dust[num953];
                Dust dust334 = dust111;
                dust334.velocity *= 2.5f;
                num953 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 6, 0f, 0f, 100, default(Color), 1.5f);
                dust111 = Main.dust[num953];
                dust334 = dust111;
                dust334.velocity *= 1.5f;
            }
            //int num954 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
            //Gore gore29 = Main.gore[num954];
            //Gore gore64 = gore29;
            //gore64.velocity *= 0.2f;
            //Main.gore[num954].velocity.X += Main.rand.Next(-1, 2);
            //Main.gore[num954].velocity.Y += Main.rand.Next(-1, 2);
            projectile.position.X += projectile.width / 2;
            projectile.position.Y += projectile.height / 2;
            projectile.width = 100;
            projectile.height = 100;
            projectile.position.X -= projectile.width / 2;
            projectile.position.Y -= projectile.height / 2;
            projectile.penetrate = -1;
            projectile.Damage();
            #endregion

            return false;
        }

    }
}
