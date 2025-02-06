using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using System.Linq;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using static Terraria.ModLoader.ModContent;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Staves
{
    
    public class WandOfFrosting : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.WandofFrosting) && GetInstance<VFXPlusToggles>().MagicToggle.WandOfFrostingToggle;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };

            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_42") with { Volume = .3f, Pitch = .25f, PitchVariance = .15f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class WandOfFrostingShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WandOfFrostingFrost) && GetInstance<VFXPlusToggles>().MagicToggle.WandOfFrostingToggle;
        }

        int timer = 0;
        float fadeInPower = 0f;
        float spinInPower = 0f;
        public override bool PreAI(Projectile projectile)
        {
            Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX) * -2f;
            Color blueToUse = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.35f);

            GlowPixelAltBehavior dustBehavior = new GlowPixelAltBehavior();
            dustBehavior.base_fadeOutPower = 0.91f;
            dustBehavior.base_timeToKill = 30; //30

            Dust c = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelAlts>(), dustVel.RotatedByRandom(0.25f), 0, blueToUse, 0.53f * fadeInPower);
            c.alpha = 2;
            c.noLight = true;
            c.customData = dustBehavior;

            if (Main.rand.NextBool())
            {
                Dust a = Dust.NewDustPerfect(projectile.Center + projectile.velocity * 0.5f, ModContent.DustType<GlowPixelRise>(), dustVel.RotatedByRandom(0.5f), 0, Color.DeepSkyBlue, 0.33f * fadeInPower);
                a.alpha = 2;
                a.noLight = true;
                a.customData = dustBehavior;

            }

            //Smoke dust
            if (timer % 4 == 0)
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<HighResSmoke>(), newColor: Color.LightSkyBlue * fadeInPower, Scale: Main.rand.NextFloat(0.25f, 0.5f));
                Main.dust[d].velocity += projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
                Main.dust[d].customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.55f);
            }

            float timeForSpinIn = Math.Clamp(timer / 28f, 0f, 1f);
            spinInPower = Easings.easeOutCirc(timeForSpinIn);

            //Fade in over 15 ticks
            if (timer < 47)
            {
                float timeForFadeIn = Math.Clamp(timer / 15f, 0f, 1f);
                fadeInPower = Easings.easeOutCirc(timeForFadeIn);
            }
            //Fade out over 8 ticks
            else
            {
                float timeForFadeIn = Math.Clamp((timer - 47f) / 8f, 0f, 1f);
                fadeInPower = Easings.easeOutCirc(1f - timeForFadeIn);
            }

            timer++;
            return true;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D Tex = CommonTextures.RainbowRod.Value;
            Texture2D Orb = CommonTextures.PartiGlowPMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 posOffset = Main.rand.NextVector2Circular(2f, 2f);

            float spinInBonusRot = MathHelper.Lerp(12f * projectile.direction, 0f, spinInPower);
            float drawRot = projectile.velocity.ToRotation() - spinInBonusRot;

            Vector2 TexOrigin = Tex.Size() / 2f;
            Vector2 vec2scale = new Vector2(0.45f, 0.45f) * projectile.scale * 1.35f * fadeInPower;

            Color blueToUse = Color.SkyBlue;
            Color blueToUse2 = Color.DeepSkyBlue;

            //Black Under Layer
            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, Color.Black * 0.25f, drawRot, TexOrigin, vec2scale * 0.6f, SpriteEffects.None, 0f);

            //Orb
            Main.spriteBatch.Draw(Orb, drawPos + posOffset, null, blueToUse2 with { A = 0 } * 0.2f, drawRot, Orb.Size() / 2f, vec2scale * 1.15f, SpriteEffects.None, 0f);

            //Main layer
            Main.spriteBatch.Draw(Tex, drawPos + posOffset, null, blueToUse with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.7f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(1.25f, 1.25f), null, blueToUse2 with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.45f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos + Main.rand.NextVector2Circular(0.75f, 0.75f), null, Color.White with { A = 0 } * fadeInPower, drawRot, TexOrigin, vec2scale * 0.25f, SpriteEffects.None, 0f);

            return false;
        }

    }

}
