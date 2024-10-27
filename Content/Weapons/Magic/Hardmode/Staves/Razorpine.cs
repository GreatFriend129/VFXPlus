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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class Razorpine : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Razorpine);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            /*
            //Dust
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Vector2 dustPos = position + velocity.SafeNormalize(Vector2.UnitX) * 25;
                Dust dp = Dust.NewDustPerfect(dustPos, DustID.JungleGrass,
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 9),
                    Scale: Main.rand.NextFloat(0.90f, 1.1f) * 1f);

                dp.alpha = 100;
                dp.noGravity = true;
                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f); //80

            }
            */
            return true;
        }

    }
    public class RazorpineShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PineNeedleFriendly);
        }

        float fadeInScale = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 12;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInProg = Math.Clamp(timer / 20f, 0f, 1f);
            fadeInScale = Easings.easeOutCubic(fadeInProg);

            if (timer % 10 == 0 && Main.rand.NextBool())
            {

                Dust p = Dust.NewDustPerfect(projectile.Center, DustID.Everscream,
                    projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(2f, 4f),
                    newColor: Color.White, Scale: Main.rand.NextFloat(0.9f, 1.1f) * 1f);

                p.noGravity = true;
                p.velocity += projectile.velocity * 0.2f;
            }

            timer++;
            return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //return true;
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Vector2 TexOrigin = vanillaTex.Size() / 2f;

            Vector2 vec2Scale = new Vector2(fadeInScale, 1f) * projectile.scale;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Vector2 vec2Scale2 = new Vector2(progress, 1f) * projectile.scale;


                    Color col = Color.Lerp(Color.White * 0f, Color.White * 1f, progress) * progress * projectile.Opacity;
                    Color colw = Color.White * Easings.easeInCirc(progress);

                    float size2 = 1f * progress * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, null, colw * 0.75f,
                            previousRotations[i], TexOrigin, vec2Scale2, SpriteEffects.None);


                }

            }

            //Border
            for (int i = 0; i < -4; i++)
            {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(MathHelper.PiOver2 * i);

                Main.EntitySpriteDraw(vanillaTex, drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.03f * projectile.direction), null,
                    Color.DarkGreen with { A = 0 } * 1f, projectile.rotation, TexOrigin, vec2Scale, SpriteEffects.None);
            }

            for (int i = 0; i < 5; i++)
            {
                float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null, 
                    Color.DarkGreen with { A = 0 } * 1.5f * opacitySquared, projectile.rotation, TexOrigin, vec2Scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, TexOrigin, vec2Scale, SpriteEffects.None);
            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
