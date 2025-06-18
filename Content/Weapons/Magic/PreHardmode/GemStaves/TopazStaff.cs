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
using VFXPlus.Common.Interfaces;



namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class TopazStaff : GlobalItem
    {

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.TopazStaff) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.TopazStaffToggle;
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

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_20") with { Volume = 0.65f, Pitch = .45f, PitchVariance = 0.1f };
            SoundEngine.PlaySound(style2, player.Center);

            return true;
        }

    }
    public class TopazStaffShotOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.TopazBolt) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.TopazStaffToggle;
        }

        public override void SetDefaults(Projectile entity) { entity.hide = true; }
        public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index); //Needed for trail to be under proj
            base.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();

        int frame = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            projectile.rotation = projectile.velocity.ToRotation();

            #region trail info
            //Trail1 info dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/LintyTrail").Value;
            trail1.trailColor = Color.Orange * fadeInAlpha * 0.7f; //10 155 10
            trail1.trailPointLimit = 15;
            trail1.trailWidth = (int)(8f * projectile.scale);
            trail1.trailMaxLength = 120;

            trail1.trailTime = timer * 0.05f;
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            #endregion

            if (timer % 3 == 0 && Main.rand.NextBool(1)) //timer mod 1 with high res smoke looks cool
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: Color.Orange, Scale: Main.rand.NextFloat(0.35f, 0.4f));
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
            }

            if (timer % 5 == 0)
                frame = (frame + 1) % 4;


            Lighting.AddLight(projectile.Center, Color.Orange.ToVector3() * 0.9f * fadeInAlpha);

            fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, 1.25f, 0.04f), 0f, 1f);
            timer++;
            return false;
        }


        public void DrawAdditive(SpriteBatch sb) { trail1.TrailDrawing(sb, false); }

        float fadeInAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D fireball = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/PreHardmode/GemStaves/Fireballs/TopazFireball").Value;
            Texture2D glorb = Mod.Assets.Request<Texture2D>("Assets/Orbs/GlorbPMA3").Value;
            Texture2D star = CommonTextures.RainbowRod.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            int frameHeight = fireball.Height / 4;
            int startY = frameHeight * frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, fireball.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            SpriteEffects se = projectile.velocity.X > 0f ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(glorb, drawPos, null, Color.Orange with { A = 0 } * fadeInAlpha * 0.5f, projectile.rotation, glorb.Size() / 2, new Vector2(projectile.scale, projectile.scale * 0.5f) * 1f, SpriteEffects.None);
            Main.EntitySpriteDraw(fireball, drawPos, sourceRectangle, Color.White * fadeInAlpha, projectile.rotation, origin, projectile.scale, se);

            //Star 
            Vector2 starDrawPos = drawPos + projectile.rotation.ToRotationVector2() * 10f * projectile.scale;

            float dir = projectile.velocity.X > 0 ? 1 : -1;

            float starRotation = MathHelper.Lerp(0f, MathHelper.Pi * 3f * dir, Easings.easeOutQuad(fadeInAlpha)) + ((float)Main.timeForVisualEffects * 0.05f * dir);
            float starScale = Easings.easeOutQuint(1f - fadeInAlpha) * projectile.scale * 1f;

            Main.EntitySpriteDraw(star, starDrawPos, null, Color.Orange with { A = 0 } * fadeInAlpha, starRotation, star.Size() / 2f, starScale, se);
            Main.EntitySpriteDraw(star, starDrawPos, null, Color.White with { A = 0 } * fadeInAlpha, starRotation, star.Size() / 2f, starScale * 0.5f, se);

            return false;
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.25f, 3.25f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.Orange, Scale: Main.rand.NextFloat(0.45f, 0.75f));

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 7; i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.DarkOrange : Color.Orange;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 2.75f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 1.2f));
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Orange, Scale: 0.15f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
                overallAlpha: 0.10f, DrawWhiteCore: false, 1f, 1f);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_118") with { Volume = 1f, Pitch = .2f, PitchVariance = .2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 0; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(1.75f, 1.75f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: new Color(10, 173, 15), Scale: Main.rand.NextFloat(0.55f, 0.65f));

                dust.velocity += projectile.velocity * 0.25f;

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.9f, shouldFadeColor: false);
            }

            for (int i = 0; i < 0; i++)
            {
                Color col = new Color(10, 173, 15);
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.75f));
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

                d.velocity += projectile.velocity * 0.25f;
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
