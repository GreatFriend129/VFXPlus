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
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class MarrowOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Marrow);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            player.GetModPlayer<HeldBowPlayer>().arrowType = ProjectileID.BoneArrow;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.Marrow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;
            //player.GetModPlayer<HeldBowPlayer>().underGlowColor = new Color(42, 2, 82);
            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);


    }
    public class MarrowShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BoneArrow);
        }
        public override bool InstancePerEntity => true;


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 32; //18

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

            //Line Dust
            if (timer % 3 == 0 && Main.rand.NextBool(1) && timer > 5)
            {
                float rot = projectile.velocity.ToRotation();

                Vector2 pos = projectile.Center + new Vector2(0f, Main.rand.NextFloat(-6f, 6f)).RotatedBy(rot);
                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -Main.rand.NextFloat(3f, 9f);

                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<MuraLineDust>(), vel * 1f, newColor: Color.Beige * 0.35f, Scale: Main.rand.NextFloat(0.3f, 0.65f) * 0.4f);
                dp.alpha = 13;

                dp.customData = new MuraLineBehavior(new Vector2(0.6f, 1f), WhiteIntensity: 0f);
            }

            float fadeInTime = Math.Clamp((timer + 18f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }


        float starPower = 1f;
        float overallScale = 1f;
        float overallAlpha = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;


            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;



            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);


            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                if (i > 22)
                {
                    float progress = (float)(i) / previousPositions.Count;

                    Vector2 trailPos = previousPositions[i] - Main.screenPosition;
                    float trailAlpha = Easings.easeInQuad(progress) * projectile.Opacity;
                    Vector2 trailScale = new Vector2(1f, 1f) * overallScale;

                    Main.EntitySpriteDraw(vanillaTex, trailPos, null, Color.White * trailAlpha * 0.35f, previousRotations[i], TexOrigin, trailScale, SE);
                }

            }
            //Border
            for (int i = 0; i < 4; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(2f, 2f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, null, Color.Beige with { A = 0 } * projectile.Opacity * 0.5f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;

        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition;
                //float trailAlpha = progress * progress * projectile.Opacity;
                //Vector2 trailScale = new Vector2(progress, 1f) * overallScale;

                //Main.EntitySpriteDraw(vanillaTex, trailPos, null, Color.Silver with { A = 0 } * trailAlpha * 0.15f, previousRotations[i], TexOrigin, trailScale, SE);


                float size2 = (0.5f + (0.5f * progress));
                Vector2 vec2Scale = new Vector2(1.5f, 0.75f * size2) * overallScale * projectile.scale * 0.5f;
                Main.EntitySpriteDraw(flare, trailPos, null, new Color(143, 116, 87) with { A = 0 } * 0.2f * progress,
                    previousRotations[i] + MathHelper.PiOver2, flare.Size() / 2f, vec2Scale, SpriteEffects.None);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
            {

                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: new Color(143, 116, 87), Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            for (int num533 = 0; num533 < 6; num533++)
            {
                int num534 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, DustID.Bone);
                Main.dust[num534].noGravity = true;
                Dust dust207 = Main.dust[num534];
                Dust dust334 = dust207;
                dust334.velocity -= projectile.velocity * 0.5f;
                dust334.color = dust334.color with { A = 0 };
            }

            int randSound = Main.rand.Next(3);
            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_skeleton_hurt_" + randSound) with { Volume = 0.4f, Pitch = -.30f, PitchVariance = 0.1f }; 
            SoundEngine.PlaySound(style, projectile.Center);

            SoundEngine.PlaySound(SoundID.Dig, projectile.Center);

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
