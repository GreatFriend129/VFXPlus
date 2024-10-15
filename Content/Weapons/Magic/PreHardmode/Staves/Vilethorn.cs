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


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Staves
{
    
    public class Vilethorn : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Vilethorn);
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
    public class VilethornBaseShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VilethornBase);
        }


        float scale = 0;
        float alpha = 1;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 8)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .035f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; 
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);

                //SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_156") with { Volume = 0.15f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                //SoundEngine.PlaySound(style4, projectile.Center);
            }


            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 3f));

            //scale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress)); 30 | (timer + 5) 

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);

            timer++;
            return true;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);

            //Border
            for (int i = 0; i < 10; i++)
            {
                float myAlpha = projectile.Opacity * alpha;// 1f - scale; // * projectile.Opacity;
                //float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null,
                    new Color(61, 2, 92) with { A = 0 } * 1f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None);
            }

            //Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);
            return false;
            
        }

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);

            //base.PostDraw(projectile, lightColor);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            //return false;
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

    }

    public class VilethornTipShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.VilethornTip);
        }


        float scale = 0;
        float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 8)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .035f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, };
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);
            }


            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 4f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);


            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;


            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);

            //Border
            for (int i = 0; i < 10; i++)
            {
                float myAlpha = projectile.Opacity * alpha;// 1f - scale; // * projectile.Opacity;
                //float opacitySquared = projectile.Opacity * projectile.Opacity;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null,
                    new Color(61, 2, 92) with { A = 0 } * 1f * myAlpha, projectile.rotation, vanillaTex.Size() / 2, vec2Scale * 1.1f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            //return false;
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

    }

}
