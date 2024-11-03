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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class SpiritFlame : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SpiritFlame);
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
    public class SpiritFlameShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.SpiritFlame);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            float adjustedRot = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            projectile.rotation = projectile.velocity.Length() > 0 ? adjustedRot : 0f;
            
            int trailCount = 9;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (timer == 0)
            {
                Color littleLessPurple = new Color(121, 7, 179);

                //little offset because proj center is not center of fireball visually
                Vector2 dustSpawnPos = projectile.Center;// + new Vector2(0f, 7f * projectile.scale);

                Dust d1 = Dust.NewDustPerfect(dustSpawnPos, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: littleLessPurple, Scale: 1f);
                d1.rotation = 0f;
                d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

                Dust d2 = Dust.NewDustPerfect(dustSpawnPos, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: littleLessPurple, Scale: 1f);
                d2.rotation = MathHelper.PiOver4;
                d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

                for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                    Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: littleLessPurple, Scale: Main.rand.NextFloat(0.75f, 1.25f) * 0.4f);
                    d.fadeIn = Main.rand.Next(0, 4);
                    d.alpha = Main.rand.Next(0, 2);
                    d.noLight = false;
                }
            }

            if (timer % 5 == 0 && Main.rand.NextBool())
            {

                Vector2 vel = Main.rand.NextVector2Circular(2.75f, 2.75f) + projectile.velocity * 0.25f;

                Color purp = new Color(61, 2, 92); //42 2 82

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: purp * 3f, Scale: Main.rand.NextFloat(0.2f, 0.25f));

                p.velocity += projectile.velocity * 0.2f;

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(shouldFadeColor: false);

            }

            timer++;

            inFadePower = Math.Clamp(MathHelper.Lerp(inFadePower, 1.35f, 0.1f), 0f, 1f);

            return base.PreAI(projectile);
        }

        float inFadePower = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Utils.DrawBorderString(Main.spriteBatch, "" + inFadePower, projectile.Center + new Vector2(0f, -50) - Main.screenPosition, Color.White);

            Color purp = new Color(121, 7, 179) * inFadePower;
            Color purp2 = new Color(61, 2, 92);

            Texture2D SoftGlow = Mod.Assets.Request<Texture2D>("Assets/Orbs/SoftGlow64").Value;
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;


            Vector2 posOffset = new Vector2(0f, -6f * projectile.scale).RotatedBy(projectile.rotation);
            Vector2 drawPos = projectile.Center - Main.screenPosition + posOffset;
            
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            float easeVal = Easings.easeInOutBack(inFadePower, 0f, 10f);
            //Vector2 vec2Scale = new Vector2(easeVal, (easeVal * 0.25f) + 0.75f);
            Vector2 vec2Scale = new Vector2(easeVal, 1f);

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    Color col = (purp2 * 3f) * progress * inFadePower;

                    float size2 = (0.25f + (progress * 0.75f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + posOffset;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.5f, //0.5f
                            previousRotations[i], TexOrigin, vec2Scale * size2, SpriteEffects.None);
                }

            }

            //Orb glow
            Vector2 orbScale = new Vector2(0.75f * inFadePower, 1.25f) * projectile.scale * 0.5f;
            Main.EntitySpriteDraw(SoftGlow, drawPos, null, purp with { A = 0 } * inFadePower * 0.75f, projectile.rotation, SoftGlow.Size() / 2f, orbScale, SpriteEffects.None);


            //Border
            for (int i = 0; i < 8; i++)
            {
                float opacitySquared = inFadePower * inFadePower;
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, 
                    purp2 with { A = 0 } * 0.75f * opacitySquared, projectile.rotation, TexOrigin, vec2Scale * 1.05f, SpriteEffects.None);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * inFadePower, projectile.rotation, TexOrigin, vec2Scale * projectile.scale, SpriteEffects.None);
            return false;

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //return true;
            if (projectile.ai[0] < 0f)
                return false;

            #region CerobaImpact
            Color littleLessPurple = new Color(121, 7, 179);

            Color newPurple = new Color(61, 2, 92); //new Color(121, 7, 179);
            Color darkPurple = new Color(42, 2, 82);

            Color purp1 = newPurple; //deep pink
            Color purp2 = Color.Purple; // hot pink

            //Impact
            for (int i = 0; i < 6 + Main.rand.Next(0, 4); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3f, 3f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: littleLessPurple * 3f, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 13, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);
            }

            for (int i = 0; i < 7 + Main.rand.Next(0, 3); i++)
            {
                if (i > 4)
                {
                    Vector2 smvel = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                    Dust sm = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), smvel, newColor: littleLessPurple * 1f, Scale: Main.rand.NextFloat(0.35f, 0.75f));
                    sm.customData = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f, 
                        overallAlpha: 1f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
                }

                Color col = Main.rand.NextBool() ? newPurple * 2f : newPurple;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 5f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.75f, 1.25f) * 1f);
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: littleLessPurple * 2f, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.15f, DrawWhiteCore: true, 1f, 1f);

            //Sound
            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_14") with { Pitch = .3f, MaxInstances = -1, PitchVariance = 0.2f, Volume = 0.3f };
            SoundEngine.PlaySound(style, projectile.Center);

            string randomSound = Main.rand.NextBool() ? "2" : "1";

            SoundStyle style4 = new SoundStyle("Terraria/Sounds/Custom/dd2_flameburst_tower_shot_" + randomSound) with { Pitch = .25f, PitchVariance = .32f, MaxInstances = -1, Volume = 0.35f };
            SoundEngine.PlaySound(style4, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_62") with { Volume = .23f, Pitch = .51f, PitchVariance = .27f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            #endregion

            #region vanillaKillStuff
            if (projectile.ai[0] >= 0f)
            {
                int num143 = 80;
                projectile.position = projectile.Center;
                projectile.width = (projectile.height = num143);
                projectile.Center = projectile.position;
                projectile.Damage();
                SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
                int num144 = 15;
            }

            #endregion

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            //base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
