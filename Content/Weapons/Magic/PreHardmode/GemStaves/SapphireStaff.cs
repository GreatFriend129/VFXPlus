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
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Interfaces;
using Microsoft.Xna.Framework.Graphics.PackedVector;



namespace VFXPlus.Content.Weapons.Magic.PreHardmode.GemStaves
{
    
    public class SapphireStaffOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SapphireStaff) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SapphireStaffToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f, MaxInstances = -1 };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle style4 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_43") with { Volume = 0.8f, Pitch = .25f, PitchVariance = 0.05f };
            SoundEngine.PlaySound(style4, player.Center);

            SoundStyle style3 = new SoundStyle("Terraria/Sounds/Custom/dd2_etherian_portal_dryad_touch") with { Volume = .3f, Pitch = 1f, PitchVariance = .15f, MaxInstances = -1, };
            SoundEngine.PlaySound(style3, player.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_20") with { Volume = 0.65f, Pitch = .45f, PitchVariance = 0.1f};
            SoundEngine.PlaySound(style2, player.Center);

            //I don't think this works for such a slow projectile
            //Vector2 velNormalized = velocity.SafeNormalize(Vector2.UnitX);
            //Dust d2 = Dust.NewDustPerfect(position + velNormalized * 55f, ModContent.DustType<CirclePulse>(), velNormalized * 0.25f, newColor: Color.DodgerBlue);
            //CirclePulseBehavior b2 = new CirclePulseBehavior(0.2f, true, 4, 0.2f, 0.4f);
            //b2.drawLayer = "Dusts";
            //d2.customData = b2;
            //d2.scale = 0.2f * 0.15f;

            return true;
        }

    }
    public class SapphireStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SapphireBolt) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SapphireStaffToggle;
        }

        int frame = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            projectile.rotation = projectile.velocity.ToRotation();

            int trailCount = 19;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 3 == 0 && Main.rand.NextBool(1) && timer > 7)
            {
                //int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.35f, 0.4f));
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<GlowPixelCross>(), newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.3f, 0.35f));
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.35f;
            }

            if (timer % 5 == 0)
                frame = (frame + 1) % 4;


            Lighting.AddLight(projectile.Center, Color.DodgerBlue.ToVector3() * 0.8f * fadeInAlpha);

            fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, 1.25f, 0.06f), 0f, 1f);
            starSpinInPower = Math.Clamp(MathHelper.Lerp(starSpinInPower, 1.25f, 0.04f), 0f, 1f); 

            float timeForPopInAnim = 33; //37
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.75f)) * 1f;

            timer++;
            return false;
        }


        float starSpinInPower = 0f;
        float overallScale = 0f;
        float fadeInAlpha = 0f;
        List<float> previousRotations = new List<float>();
        List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                NewTrail(projectile, false);
            });
            NewTrail(projectile, true);

            Texture2D fireball = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/PreHardmode/GemStaves/Fireballs/SapphireFireball").Value;
            Texture2D star = CommonTextures.CrispStarPMA.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            drawPos += projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f;

            int frameHeight = fireball.Height / 4;
            int startY = frameHeight * frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, fireball.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            SpriteEffects se = projectile.velocity.X > 0f ? SpriteEffects.None : SpriteEffects.FlipVertically;


            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(fireball, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, Color.White with { A = 0 } * fadeInAlpha, projectile.rotation, origin, 1.05f * projectile.scale * overallScale, se);
            }

            Main.EntitySpriteDraw(fireball, drawPos, sourceRectangle, Color.White * fadeInAlpha, projectile.rotation, origin, projectile.scale * overallScale, se);

            //Star 
            Vector2 starDrawPos = drawPos + projectile.rotation.ToRotationVector2() * 10f * projectile.scale;

            float dir = projectile.velocity.X > 0 ? 1 : -1;

            float starRotation = MathHelper.Lerp(0f, MathHelper.Pi * 3f * dir, Easings.easeOutQuad(starSpinInPower)) + ((float)Main.timeForVisualEffects * 0.05f * dir);
            float starScale = Easings.easeOutQuint(1f - starSpinInPower) * projectile.scale * overallScale * 0.7f;

            Main.EntitySpriteDraw(star, starDrawPos, null, Color.DodgerBlue with { A = 0 } * starSpinInPower, starRotation, star.Size() / 2f, starScale, se);
            Main.EntitySpriteDraw(star, starDrawPos, null, Color.White with { A = 0 } * starSpinInPower, starRotation, star.Size() / 2f, starScale * 0.5f, se);

            return false;
        }

        public void NewTrail(Projectile projectile, bool returnImmediately)
        {
            if (returnImmediately)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Orb
            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Color[] cols = { Color.DeepSkyBlue * 0.75f, Color.DodgerBlue * 0.525f, Color.Blue * 0.375f };
            float[] scales = { 1.15f, 1.6f, 2.5f };

            float orbRot = projectile.velocity.ToRotation();
            float orbAlpha = 0.8f * fadeInAlpha;
            Vector2 orbScale = new Vector2(0.85f, 0.55f) * 0.3f * projectile.scale * overallScale;
            Vector2 orbOrigin = orb.Size() / 2f;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, orbRot, orbOrigin, orbScale * scales[2] * sineScale2, SpriteEffects.None);

            Texture2D Spike = CommonTextures.SoulSpike.Value;

            for (int i = 0; i < previousPositions.Count; i++)
            {
                float scale = (float)i / previousPositions.Count;
                Vector2 vec2ScaleTrail = new Vector2(scale * 0.5f * Easings.easeOutSine(fadeInAlpha), Easings.easeInQuad(scale) * 0.6f) * projectile.scale;

                Vector2 drawPosAI = previousPositions[i] - Main.screenPosition;

                Color betweenBlue = Color.Lerp(Color.DodgerBlue, Color.SkyBlue, 0.4f);
                Color col = Color.Lerp(Color.Blue, betweenBlue, scale) * scale * fadeInAlpha;

                Main.spriteBatch.Draw(Spike, drawPosAI, null, col with { A = 0 } * 0.75f, previousRotations[i], Spike.Size() / 2f, vec2ScaleTrail, SpriteEffects.None, 0f);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 7; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.45f, 0.7f));

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 4; i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.DeepSkyBlue : Color.DodgerBlue;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 3f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 1.1f));
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

            }

            //Light Dust
            ///Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DodgerBlue, Scale: 0.2f);

            ///softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
             ///   overallAlpha: 0.12f, DrawWhiteCore: false, 1f, 1f);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_118") with { Volume = 0.75f, Pitch = .2f, PitchVariance = .2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);


            Color between = Color.Lerp(Color.DodgerBlue, Color.Blue, 0f);
            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<FeatheredGlowDust>(), Velocity: Vector2.Zero, newColor: between, Scale: 1f);

            //FeatheredGlowBehavior fgb = new FeatheredGlowBehavior(AlphaChangeSpeed: 0.65f, timeToChangeAlpha: 6, ScaleChangeSpeed: 0.85f, timeToKill: 120, OverallAlpha: 0.75f);

            FeatheredGlowBehavior fgb = new FeatheredGlowBehavior(AlphaChangeSpeed: 0.65f, timeToChangeAlpha: 6, ScaleChangeSpeed: 0.85f, timeToKill: 120, OverallAlpha: 0.15f);
            fgb.DrawWhiteCore = true;
            d1.customData = fgb;



            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
            return base.OnTileCollide(projectile, oldVelocity);
        }

    }

}
