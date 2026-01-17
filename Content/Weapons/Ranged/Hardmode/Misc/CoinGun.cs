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
    
    public class CoinGunOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.CoinGun);
        }

        public override void SetDefaults(Item entity)
        {
            entity.noUseGraphic = true;
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.CoinGun,
                    AnimTime: 15,
                    NormalXOffset: 21f,
                    DestXOffset: 8f,
                    YRecoilAmount: 0.1f,
                    HoldOffset: new Vector2(0f, 0f)
                    );

                held.compositeArmAlwaysFull = false;
            }

            SoundStyle style = new SoundStyle("Terraria/Sounds/Research_0") with { Volume = .02f, Pitch = 0.9f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/MoneyShot") with { Volume = 0.05f, Pitch = -.15f, PitchVariance = .15f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_38") with { Volume = .15f, Pitch = 1f, PitchVariance = 0.25f, MaxInstances = -1 };
            SoundEngine.PlaySound(style4, player.Center);

            SoundStyle style5 = SoundID.Item110 with { Volume = 0.15f, PitchVariance = 0.15f, Pitch = 0.25f };
            SoundEngine.PlaySound(style5, player.Center);

            return true;
        }

    }
    public class CoinGunShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CopperCoin || entity.type == ProjectileID.SilverCoin || entity.type == ProjectileID.GoldCoin || entity.type == ProjectileID.PlatinumCoin);
        }
        public override bool InstancePerEntity => true;

        int timer = 0;

        public override bool PreAI(Projectile projectile)
        {
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.rotation);

            int trailCount = 27;
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

            int colIndex = projectile.type - 158;
            Color[] dustCols = { new Color(110, 65, 65), Color.Silver, Color.Goldenrod, Color.White };

            if (timer > 5 && timer % 3 == 0 && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), vel, newColor: dustCols[colIndex], Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;
            }

            bool isHighTier = (projectile.type == ProjectileID.GoldCoin || projectile.type == ProjectileID.PlatinumCoin);
            if (timer > 5 && timer % 6 == 0 && Main.rand.NextBool(3) && isHighTier)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel, newColor: dustCols[colIndex], Scale: Main.rand.NextFloat(0.45f, 0.5f) * 0.5f);
                d.alpha = 2;
                d.velocity += -projectile.velocity.RotatedByRandom(0.1f) * 0.55f;
                d.velocity *= 0.35f;

                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(velToBeginShrink: 3f);
            }

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.25f, 0.04f), 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D glorb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int colIndex = projectile.type - 158;

            Color[] trailColors = { new Color(80, 65, 65) * 1.5f, Color.Silver, Color.Orange, Color.White };
            Color[] glorbColors = { new Color(120, 65, 65) * 0.75f, Color.Silver, Color.Goldenrod, Color.Silver };

            Vector2 glorbScale = new Vector2(0.35f, 0.8f) * projectile.scale * 0.6f;
            Main.EntitySpriteDraw(glorb, drawPos, null, glorbColors[colIndex] with { A = 0 } * projectile.Opacity * 0.75f, projectile.rotation, glorb.Size() / 2f, glorbScale * 0.5f, 0);
            Main.EntitySpriteDraw(glorb, drawPos, null, glorbColors[colIndex] with { A = 0 } * projectile.Opacity * 0.3f, projectile.rotation, glorb.Size() / 2f, glorbScale, 0);
            Main.EntitySpriteDraw(glorb, drawPos, null, glorbColors[colIndex] with { A = 0 } * projectile.Opacity * 0.1f, projectile.rotation, glorb.Size() / 2f, glorbScale * 1.25f, 0);


            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition + new Vector2(0f, 0f);
                float trailAlpha = progress * progress * projectile.Opacity;
                Vector2 trailScale = new Vector2(progress, 1f);

                Main.EntitySpriteDraw(vanillaTex, trailPos, null, trailColors[colIndex] with { A = 0 } * trailAlpha * 0.5f, projectile.rotation, TexOrigin, trailScale, SE);
            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(2.5f, 2.5f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, null, glorbColors[colIndex] with { A = 0 } * projectile.Opacity * 0.8f, projectile.rotation, TexOrigin, projectile.scale * 1.1f, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale, SE);

            //Star
            Texture2D Flash = (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/RainbowRod");

            float starRot = (float)Main.timeForVisualEffects * 0.15f * (projectile.velocity.X > 0 ? 1f : -1f);
            float starAlpha = overallAlpha * projectile.Opacity;

            Main.spriteBatch.Draw(Flash, drawPos, null, trailColors[colIndex] with { A = 0 } * 0.75f * starAlpha, starRot, Flash.Size() / 2, 0.6f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flash, drawPos, null, glorbColors[colIndex] with { A = 0 } * 0.75f * starAlpha, starRot, Flash.Size() / 2, 0.45f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flash, drawPos, null, Color.White with { A = 0 } * 0.75f * starAlpha, starRot, Flash.Size() / 2, 0.3f, SpriteEffects.None, 0f);


            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int colIndex = projectile.type - 158;
            Color[] dustCols = { new Color(110, 65, 65), Color.Silver, Color.Goldenrod, Color.White };

            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {

                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: dustCols[colIndex], Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_2") with { Volume = 0.25f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1, };
            SoundEngine.PlaySound(style, projectile.Center);

            //Vanilla Dust
            int[] dustTypes = { 9, 11, 19, 11 };
            for (int num533 = 0; num533 < 3; num533++) //10->5
            {
                int num534 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, dustTypes[colIndex]);
                Main.dust[num534].noGravity = true;
                Dust dust207 = Main.dust[num534];
                Dust dust334 = dust207;
                dust334.velocity -= projectile.velocity * 0.5f;
                dust334.color = dust334.color with { A = 0 };
            }

            SoundEngine.PlaySound(SoundID.Dig, projectile.Center);

            return false;
        }

    }

}
