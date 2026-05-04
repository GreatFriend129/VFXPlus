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
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Guns
{
    public class Xenopopper : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Xenopopper);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicGunProjMiddle>(), 0, 0, player.whoAmI);

            if (Main.projectile[gun].ModProjectile is BasicGunProjMiddle held)
            {
                held.SetProjInfo(
                    GunID: ItemID.Xenopopper,
                    AnimTime: 18,
                    NormalXOffset: 15f,
                    DestXOffset: -3f, //-4
                    YRecoilAmount: 0.25f,
                    HoldOffset: new Vector2(0f, 2f),
                    TipPos: new Vector2(28f, -3f),
                    StarPos: new Vector2(22f, -3f)
                    );

                held.timeToStartFade = 0;
                held.isShotgun = true;
            }

            int dir = velocity.X > 0 ? 1 : -1;
            Vector2 muzzlePos = position + new Vector2(34f, -2f * dir).RotatedBy(velocity.ToRotation()) + new Vector2(0f, 1f);

            //Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 50f;
            for (int i = 0; i < 11; i++) //16
            {
                Color col1 = Color.Lerp(Color.Purple, Color.MediumPurple, 0.35f);

                float progress = (float)i / 10;
                Color col = Color.Lerp(Color.Purple * 0.4f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 1f) * 1f,
                    newColor: col * 0.75f, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.35f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18), 0.98f, 0.01f, 0.75f); //12 28

                d.rotation = Main.rand.NextFloat(6.28f);

                d.velocity += velocity.SafeNormalize(Vector2.UnitX) * 0.5f;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Purple, Scale: 0.1f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.1f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 2 + Main.rand.Next(0, 3); i++)
            {
                Color col1 = Color.Lerp(Color.Purple, Color.MediumPurple, 0.15f);


                Vector2 randomStart = Main.rand.NextVector2Circular(1.5f, 1.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(muzzlePos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1.5f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                    velToBeginShrink: 10f, fadePower: 0.9f, shouldFadeColor: false);

                dust.velocity += velocity.SafeNormalize(Vector2.UnitX) * 2f;
            }

            //Bullet Casing
            //Gore.NewGore(source, position + velocity, new Vector2(velocity.X * -0.25f, -0.75f), ModContent.GoreType<PurpleCasing>());

            return true;
        }
    }


    public class XenoPopperBubbleOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Xenopopper);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            totalAlpha = 0.2f + Math.Clamp(MathHelper.Lerp(totalAlpha, 1.5f, 0.12f), 0f, 1f) * 0.8f;

            float progress = Math.Clamp((timer + 5f) / 30f, 0f, 1f); //timer / 50
            totalScale = 0.2f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(progress, 0f, 2f)) * 0.8f;

            timer++;
            return true;
        }


        float totalAlpha = 0f;
        float totalScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Bloomball
            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.White * 0.75f * totalAlpha, projectile.rotation, TexOrigin, projectile.scale * totalScale, 0);

            float glowscale = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 6f) / 5f + 1f) * 1.15f;
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.White with { A = 0 } * 0.25f * totalAlpha, projectile.rotation, TexOrigin, projectile.scale * glowscale * totalScale, 0);

            Main.EntitySpriteDraw(Ball, drawPos + new Vector2(0f, 0f), null, Color.Purple with { A = 0 } * 0.5f * totalAlpha, projectile.rotation, Ball.Size() / 2f, projectile.scale * 0.3f * totalScale, 0);


            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.75f, 1.75f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: Color.Purple * 1.5f, Scale: Main.rand.NextFloat(0.15f, 0.3f) * projectile.scale);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(shouldFadeColor: false, fadePower: 0.92f);
            }

            Color betweenPurp = Color.Lerp(Color.Purple, Color.MediumPurple, 0.5f);
            Vector2 dir = Main.rand.NextVector2Unit();

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.03f * projectile.scale, true, 1, 0.3f, 0.3f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: betweenPurp);
            d1.customData = cpb2;
            d1.velocity = dir * 0.01f;

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: betweenPurp);
            d2.customData = cpb2;
            d2.velocity = dir * -0.01f;

            d1.scale = 0.04f;
            d2.scale = 0.04f;

            #region vanillaAI
            SoundEngine.PlaySound(SoundID.Item96 with { Volume = 0.35f }, projectile.position);
            int num305 = Main.rand.Next(5, 9) - 4;
            for (int num306 = 0; num306 < num305; num306++)
            {
                int num307 = Dust.NewDust(projectile.Center, 0, 0, 171, 0f, 0f, 100, default(Color), 0.8f);
                Dust dust256 = Main.dust[num307];
                Dust dust334 = dust256;
                dust334.velocity *= 0.8f;
                Main.dust[num307].position = Vector2.Lerp(Main.dust[num307].position, projectile.Center, 0.5f);
                Main.dust[num307].noGravity = true;
            }
            if (projectile.owner == Main.myPlayer)
            {
                Vector2 vector36 = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY);
                if (Main.player[projectile.owner].gravDir == -1f)
                {
                    vector36.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y;
                }
                Vector2 vector37 = Vector2.Normalize(vector36 - projectile.Center);
                vector37 *= projectile.localAI[1];
                Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center.X, projectile.Center.Y, vector37.X, vector37.Y, (int)projectile.localAI[0], projectile.damage, projectile.knockBack, projectile.owner);
            }

            #endregion

            return false;
        }

    }

}
