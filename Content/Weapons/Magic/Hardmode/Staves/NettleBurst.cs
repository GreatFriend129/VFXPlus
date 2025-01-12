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
using VFXPlus.Content.Weapons.Magic.Hardmode.Staves;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class NettleBurst : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.NettleBurst);
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
    public class NettleBurstBaseOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NettleBurstLeft || entity.type == ProjectileID.NettleBurstRight);
        }


        public float scale = 0f;
        public float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            
            if (timer == 0 && projectile.ai[1] % 2 == 0)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.14f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.14f);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .035f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; 
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);
            }


            float timeForPopInAnim = 25;
            float animProgress = Math.Clamp((timer + 7) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2.5f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);

            if (timer == 7) //10
            {
                for (int num78 = 220; num78 < 4; num78++)
                {
                    int num79 = Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.WoodFurniture, projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 150, Color.LightPink, 1.3f);
                    Main.dust[num79].noGravity = true;
                    Dust dust23 = Main.dust[num79];
                    Dust dust3 = dust23;
                    dust3.velocity *= 0.5f;
                }

                for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++)
                {
                    Color col = Color.Brown * 2f;

                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(5f, 5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, DustID.WoodFurniture, vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: Color.LightPink, Scale: Main.rand.NextFloat(1f, 1.3f)); //3

                    p.alpha = 200;

                    p.noGravity = true;
                    Dust dust23 = p;
                    Dust dust3 = dust23;
                    dust3.velocity *= 0.5f;
                }
            }

            timer++;
            return true;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + new Vector2(3f, 2f).RotatedBy(projectile.rotation);

            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                for (int i = 0; i < 4; i++)
                {
                    float myAlpha = projectile.Opacity * alpha;

                    float dist = 1.5f;

                    Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                    Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.15f * projectile.direction);

                    Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, null,
                        Color.Red with { A = 0 } * 0.75f * myAlpha, projectile.rotation, vanillaTex.Size() / 2f, vec2Scale, SpriteEffects.None);
                }
            });

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2f, vec2Scale, SpriteEffects.None);

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

    }

    public class NettleBurstTipShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NettleBurstEnd);
        }


        float scale = 0;
        float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 4)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .035f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);
            }


            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2.5f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);

            if (timer == 3 && false) //10
            {
                for (int i = 0; i < 3 + Main.rand.Next(1, 3); i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(5f, 5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, DustID.Dirt, vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: Color.Red * 0.5f, Scale: Main.rand.NextFloat(0.15f, 0.3f) * projectile.scale * 2f); //3
                }
            }

            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    float myAlpha = projectile.Opacity * alpha;

                    Main.spriteBatch.Draw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null,
                        Color.Red with { A = 0 } * 0.25f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.05f, SpriteEffects.None, 0f); //1.1f
                }
            });


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            //return false;
            return base.PreKill(projectile, timeLeft);
        }

    }
}
