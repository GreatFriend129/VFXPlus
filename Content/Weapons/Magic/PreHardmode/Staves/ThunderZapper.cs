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


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Staves
{
    
    public class ThunderZapper : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ThunderStaff);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item4 with { Volume = 0f };
            base.SetDefaults(entity); 
        }


        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            Vector2 pos = position + velocity.SafeNormalize(Vector2.UnitX) * 45;

            Color col = Color.DeepSkyBlue;
            for (int i = 0; i < 5 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(pos, ModContent.DustType<ElectricSparkGlow>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-1f, 1f)) * Main.rand.Next(3, 9) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.5f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91
                
                if (i < 4)
                    esb.randomVelRotatePower = 1f; //1f
                dp.customData = esb;
            }

            int soundVar = Main.rand.Next(0, 3);
            SoundStyle style5 = new SoundStyle("Terraria/Sounds/Custom/dd2_lightning_bug_zap_" + soundVar) with { Volume = 0.45f, Pitch = 0.51f, PitchVariance = 0.15f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style5, player.Center);

            SoundStyle stylea = new SoundStyle("AerovelenceMod/Sounds/Effects/lightning_flash_01") with { Volume = 0.12f, Pitch = 1f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(stylea, position);

            ///SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/LightningMothSFX_1") with { Volume = 0.25f, Pitch = 1f, PitchVariance = 0.15f, MaxInstances = -1, };
            ///SoundEngine.PlaySound(style2, position);

            //////////////


            //XSoundStyle style2 = new SoundStyle("Terraria/Sounds/Thunder_0") with { Volume = 0.1f, Pitch = 1f, PitchVariance = .15f, MaxInstances = -1 };
            //XSoundEngine.PlaySound(style2, player.Center);


            //SoundStyle style4 = new SoundStyle("Terraria/Sounds/Item_94") with { Volume = 0.25f, Pitch = 0.8f, PitchVariance = .25f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(style4, position);


            //Valid sounds:
            //SoundStyle stylea = new SoundStyle("AerovelenceMod/Sounds/Effects/lightning_flash_01") with { Volume = 0.25f, Pitch = 1f, PitchVariance = 0.2f, MaxInstances = -1 };
            //SoundEngine.PlaySound(stylea, position);
            //int soundVar = Main.rand.Next(0, 3);
            //SoundStyle style5 = new SoundStyle("Terraria/Sounds/Custom/dd2_lightning_bug_zap_" + soundVar) with { Volume = 1f, Pitch = .2f, PitchVariance = 0.1f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style5, player.Center);

            return true;
        }

    }
    public class ThunderStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;


        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ThunderStaffShot);
        }


        Vector2 initialVel;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                initialVel = projectile.velocity;

            int trailCount = 16; //16
            previousPostions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (timer % 3 == 0 && Main.rand.NextBool() && timer > 5)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(1.5f, 1.5f);

                Dust da = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), dustVel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.4f, 0.6f) * 2f);
                da.velocity += initialVel.RotatedByRandom(0.25f) * 1.25f;

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.93f, FadeScalePower: 0.98f, FadeVelPower: 0.93f, Pixelize: true, XScale: 1f, YScale: 0.35f);
                esb.randomVelRotatePower = 0.1f;
                da.customData = esb;

            }

            if (timer % 1 == 0 && timer > 5 && false)
            {
                int d = Dust.NewDust(projectile.position, 7, 7, ModContent.DustType<HighResSmoke>(), newColor: Color.LightSkyBlue, Scale: Main.rand.NextFloat(0.25f, 0.45f));
                Main.dust[d].velocity += projectile.velocity * 0.5f;
                Main.dust[d].velocity *= 0.45f;
                Main.dust[d].customData = DustBehaviorUtil.AssignBehavior_HRSBase(overallAlpha: 0.25f);
            }

            if (projectile.timeLeft < 12)
                fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, -0.5f, 0.1f), 0f, 1f);
            else if (timer > 3)
                fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, 1.25f, 0.04f), 0f, 1f);


            //pop doesn't really work well cuz it happens the same time as a jump 
            //float timeForPopInAnim = 25;
            //float animProgress = Math.Clamp((timer) / timeForPopInAnim, 0f, 1f);
            //scale = 0.25f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 2f)) * 0.75f;

            #region vanillaAIWhatTheActualFuck
            if (++projectile.frameCounter >= 4)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame >= Main.projFrames[projectile.type])
                {
                    projectile.frame = 0;
                }
            }
            projectile.alpha -= 15;
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            projectile.ai[0]++;
            if ((int)projectile.ai[0] % 2 != 0 && Main.rand.Next(4) == 0)
            {
                projectile.ai[0]++;
            }
            float num253 = 5f;
            switch ((int)projectile.ai[0])
            {
                case 10:
                    projectile.velocity.Y -= num253;
                    break;
                case 12:
                    projectile.velocity.Y += num253;
                    break;
                case 18:
                    projectile.velocity.Y += num253;
                    break;
                case 20:
                    projectile.velocity.Y -= num253;
                    projectile.ai[0] = 0f;
                    break;
            }
            if (Main.rand.Next(10) == 0 && timer > 5 && false)
            {
                Dust dust3 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 226, projectile.velocity.X, projectile.velocity.Y, 50, default(Color), 0.4f);
                dust3.noGravity = true;
                dust3.velocity = dust3.velocity * 0f + projectile.velocity * 0.5f;
                if (Main.rand.Next(3) != 0)
                {
                    Dust dust124 = dust3;
                    Dust dust212 = dust124;
                    dust212.velocity *= 1.4f;
                }
            }
            Lighting.AddLight(projectile.Center, 0.2f, 0.5f, 0.7f);
            
            #endregion

            timer++;
            return false;
        }

        float scale = 1f;
        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Draw(projectile, false);
            });

            Draw(projectile, true);

            return false;
        }

        public void Draw(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            //Orb
            Texture2D Glow = Mod.Assets.Request<Texture2D>("Assets/Orbs/GlowCircleFlare").Value;
            Color orbCol1 = Color.SkyBlue with { A = 0 } * fadeInAlpha;

            float scale1 = 2f * scale * projectile.scale;
            Main.EntitySpriteDraw(Glow, projectile.Center - Main.screenPosition, null, orbCol1 with { A = 0 } * 0.35f, projectile.velocity.ToRotation(), Glow.Size() / 2f, scale1, SpriteEffects.None);

            //Core
            Texture2D Core = (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/Twinkle");
            Texture2D Core2 = (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/CrispStarPMA");

            float rot = projectile.velocity.ToRotation();
            Color thisCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.75f) * fadeInAlpha;

            Vector2 vec2Sccale = new Vector2(1.15f, 0.75f) * projectile.scale * 1.15f * scale;
            Vector2 vec2Sccale2 = new Vector2(1.15f, 0.5f) * projectile.scale * 1.15f * scale;

            if (previousPostions != null)
            {
                for (int i = 0; i < previousPostions.Count; i++)
                {
                    float progress = (float)i / previousPostions.Count;

                    Vector2 thisScale = vec2Sccale * scale * 1f * Easings.easeOutSine(progress);
                    Vector2 thisScale2 = vec2Sccale2 * scale * 1f * Easings.easeOutSine(progress);

                    Vector2 drawPos = previousPostions[i] - Main.screenPosition;
                    Color col = thisCol with { A = 0 } * 0.85f * Easings.easeInSine(progress); //125, 198, 255

                    float prevRot = previousRotations[i];

                    Main.spriteBatch.Draw(Core, drawPos, null, col, prevRot, Core.Size() / 2, thisScale, SpriteEffects.None, 0);
                    Main.spriteBatch.Draw(Core, drawPos, null, col, prevRot, Core.Size() / 2, thisScale2, SpriteEffects.None, 0);
                }

            }

            Main.spriteBatch.Draw(Core2, projectile.Center - Main.screenPosition, null, thisCol with { A = 0 }, rot, Core2.Size() / 2, scale * projectile.scale * 0.6f * 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Core2, projectile.Center - Main.screenPosition, null, thisCol with { A = 0 }, rot, Core2.Size() / 2, scale * projectile.scale * 0.6f * 0.5f, SpriteEffects.None, 0f);

        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_hurt_1") with { Volume = 0.2f, Pitch = .8f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style, projectile.Center);


            int soundVar = Main.rand.Next(0, 4);
            SoundStyle style5 = new SoundStyle("Terraria/Sounds/Custom/dd2_lightning_aura_zap_" + soundVar) with { Volume = 0.55f, Pitch = -0.1f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style5, projectile.Center);


            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = initialVel.RotatedByRandom(1.5f) * Main.rand.NextFloat(0.25f, 1.1f);

                Dust a = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<ElectricSparkGlow>(), 
                    vel, newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.5f, 1f) * 0.95f);

                a.customData = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f);
            }

            for (int i = 0; i < 7; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f) * 1.5f;
                //dustVel += initialVel * 0.2f;

                Color middleBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.15f, 0.4f));
                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.90f, velToBeginShrink: 1f, fadePower: 0.92f, shouldFadeColor: false);
            }

            return false;
            for (int i = 0; i < previousPostions.Count; i++)
            {
                if (i % 3 == 0 && i > 2)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f) * 1.5f;

                    dustVel += initialVel * 0.25f;

                    Color middleBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);

                    Dust gd = Dust.NewDustPerfect(previousPostions[i], ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.15f, 0.4f));
                    gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.3f, timeBeforeSlow: 5,
                        preSlowPower: 0.94f, postSlowPower: 0.90f, velToBeginShrink: 1f, fadePower: 0.92f, shouldFadeColor: false);
                }
            }

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_wither_beast_crystal_impact_" + soundVariant1) with { Volume = 0.45f, Pitch = 0f, PitchVariance = .25f, MaxInstances = -1, };
            //SoundEngine.PlaySound(style, projectile.Center);
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
