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


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class StakeLauncher : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.StakeLauncher);
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
                    GunID: ItemID.StakeLauncher,
                    AnimTime: 10,
                    NormalXOffset: 16f,
                    DestXOffset: 0f,
                    YRecoilAmount: 0.07f,
                    HoldOffset: new Vector2(0f, 2f)
                    );
            }

            return true;
        }

    }
    public class StakeLauncherShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Stake);
        }
        public override bool InstancePerEntity => true;

        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 45;

            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.rotation);

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousRotations.Add(projectile.rotation);

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            if (timer % 3 == 0 && Main.rand.NextBool(2) && timer > 5)
            {
                Color thisBrown = new Color(161 - 40, 144 - 40, 106 - 40) * 0.55f;

                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-5f, 5f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(5f, 13f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 0.8f, newColor: thisBrown * 1f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                dp.alpha = 12;
                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0.25f);
            }

            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 0f);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = new Vector2(vanillaTex.Width * 0.5f, vanillaTex.Height * 0.25f);
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            for (int i = 0; i < previousPositions.Count - 1; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition;
                float trailAlpha = progress * projectile.Opacity;

                if (i > previousPositions.Count - 15)
                {
                    float thisAlpha = Easings.easeInQuad(progress) * projectile.Opacity;

                    Main.EntitySpriteDraw(vanillaTex, trailPos, null, Color.White * thisAlpha * 0.3f, projectile.rotation, TexOrigin, projectile.scale, SE);
                }

                Vector2 flareScale = new Vector2(1f, 0.45f * progress * projectile.Opacity);
                Main.EntitySpriteDraw(flare, trailPos, null, new Color(161, 144, 106) with { A = 0 } * trailAlpha * 0.15f, projectile.rotation + MathHelper.PiOver2, flare.Size() / 2f, flareScale, SE);
            }



            for (int i = 0; i < 5; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, null, new Color(161, 144, 106) with { A = 0 } * projectile.Opacity * 0.4f, projectile.rotation, TexOrigin, projectile.scale * 1.05f, SE);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SE);


            return false;

        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {


            base.OnKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.penetrate != 1)
            {
                Color thisBrown = new Color(50, 25, 0) * 1.5f;

                for (int i = 0; i < 3; i++)
                {
                    float arrowVel = 7f;
                    Vector2 randomStart = Main.rand.NextVector2Circular(2.25f, 2.25f) * 1f;
                    Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: thisBrown * 2f, Scale: Main.rand.NextFloat(0.4f, 0.45f));
                    dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.5f;

                    dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.15f, preSlowPower: 0.97f, timeBeforeSlow: 6, postSlowPower: 0.92f, velToBeginShrink: 3.5f, fadePower: 0.85f, shouldFadeColor: false);
                }


                for (int i = 0; i < 3; i++)
                {
                    float arrowVel = 7f;
                    Vector2 randomStart = Main.rand.NextVector2Circular(2.25f, 2.25f) * 1f;
                    Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MuraLineDust>(), randomStart, newColor: thisBrown * 1.25f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.5f);
                    dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * arrowVel * 0.5f;
                    dust.alpha = 12;
                    dust.customData = new MuraLineBehavior(new Vector2(0.6f, 1.5f), WhiteIntensity: 0.25f);

                }
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
