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
    
    public class SapphireStaff : GlobalItem 
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

            return true;
        }

    }
    public class SapphireStaffShotOverride : GlobalProjectile, IDrawAdditive
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SapphireBolt) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.SapphireStaffToggle;
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

            projectile.velocity *= 1.02f;
            projectile.velocity.Y += 0.3f;

            #region trail info
            //Trail1 info dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/LintyTrail").Value;
            trail1.trailColor = Color.DeepSkyBlue * fadeInAlpha * 0.55f;
            trail1.trailPointLimit = 15;
            trail1.trailWidth = (int)(5f * projectile.scale * overallScale); //8f
            trail1.trailMaxLength = 320; //120
            trail1.timesToDraw = 2;

            trail1.trailTime = timer * 0.05f;
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.useEffectMatrix = true;
            trail1.TrailLogic();
            #endregion

            if (timer % 3 == 0 && Main.rand.NextBool(1))
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<PixelGlowOrb>(), newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.35f, 0.4f));
                Main.dust[d].velocity -= projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.45f;
            }

            if (timer % 5 == 0)
                frame = (frame + 1) % 4;


            Lighting.AddLight(projectile.Center, Color.DodgerBlue.ToVector3() * 0.8f * fadeInAlpha);

            fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, 1.25f, 0.04f), 0f, 1f);


            float timeForPopInAnim = 35; //37
            float animProgress = Math.Clamp((timer + 11) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.75f)) * 1f;

            timer++;
            return false;
        }

        public void DrawAdditive(SpriteBatch sb) { /*trail1.TrailDrawing(sb, false);*/ }

        public float overallScale = 0f;
        float fadeInAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                trail1.TrailDrawing(Main.spriteBatch, true);
            });

            Texture2D fireball = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/PreHardmode/GemStaves/Fireballs/SapphireFireball").Value;
            Texture2D glorb = Mod.Assets.Request<Texture2D>("Assets/Orbs/GlorbPMA3").Value;
            Texture2D star = CommonTextures.RainbowRod.Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            int frameHeight = fireball.Height / 4;
            int startY = frameHeight * frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, fireball.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            SpriteEffects se = projectile.velocity.X > 0f ? SpriteEffects.None : SpriteEffects.FlipVertically;


            Main.EntitySpriteDraw(glorb, drawPos, null, Color.DeepSkyBlue with { A = 0 } * fadeInAlpha * 0.5f, projectile.rotation, glorb.Size() / 2, new Vector2(projectile.scale, projectile.scale * 0.5f) * overallScale, SpriteEffects.None);
            Main.EntitySpriteDraw(fireball, drawPos, sourceRectangle, Color.White * fadeInAlpha, projectile.rotation, origin, projectile.scale * overallScale, se);

            //Star 
            Vector2 starDrawPos = drawPos + projectile.rotation.ToRotationVector2() * 10f * projectile.scale;

            float dir = projectile.velocity.X > 0 ? 1 : -1;

            float starRotation = MathHelper.Lerp(0f, MathHelper.Pi * 3f * dir, Easings.easeOutQuad(fadeInAlpha)) + ((float)Main.timeForVisualEffects * 0.05f * dir);
            float starScale = Easings.easeOutQuint(1f - fadeInAlpha) * projectile.scale * overallScale;

            Main.EntitySpriteDraw(star, starDrawPos, null, Color.DodgerBlue with { A = 0 } * fadeInAlpha, starRotation, star.Size() / 2f, starScale, se);
            Main.EntitySpriteDraw(star, starDrawPos, null, Color.White with { A = 0 } * fadeInAlpha, starRotation, star.Size() / 2f, starScale * 0.5f, se);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.5f, 3.5f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.45f, 0.75f));

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 7; i++)
            {
                Color col = Main.rand.NextBool(2) ? Color.DeepSkyBlue : Color.DodgerBlue;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.5f, 3f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 1.2f));
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.DodgerBlue, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 10,
                overallAlpha: 0.12f, DrawWhiteCore: false, 1f, 1f);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_118") with { Volume = 1f, Pitch = .2f, PitchVariance = .2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);


            //Color between = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.25f);
            //Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<FeatheredGlowDust>(), Velocity: Vector2.Zero, newColor: between, Scale: 0.35f);
            
            //FeatheredGlowBehavior fgb = new FeatheredGlowBehavior(AlphaChangeSpeed: 0.65f, timeToChangeAlpha: 6, ScaleChangeSpeed: 1.15f, timeToKill: 120, OverallAlpha: 0.75f);
            //fgb.DrawWhiteCore = false;
            //d1.customData = fgb;


            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
            return base.OnTileCollide(projectile, oldVelocity);
        }

    }

}
