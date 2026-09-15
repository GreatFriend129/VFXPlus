using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;
using VFXPlus.Content.Projectiles;



namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Swords
{
    
    public class IceBladeItemOverride : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.IceBlade);
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults(Item entity)
        {
            //entity.shootsEveryUse = true;
            entity.noUseGraphic = true;
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity);
        }

        public override void UseAnimation(Item item, Player player)
        {
            //Starfury shooting is at 45120 in Player.cs | maxmana ding is at 39040 |
            //39718 for flag setting stuff that doesn't shoot every swing (beam sword, ice blade)

            float adjustedItemScale = player.GetAdjustedItemScale(item); // Get the melee scale of the player and item.
            int trail = Projectile.NewProjectile(player.GetSource_FromThis(), player.MountedCenter, new Vector2(player.direction, 0f), ModContent.ProjectileType<BaseSwordProj>(), 0, 0f, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);

            Vector3[] gradCols = {
                    Color.Black.ToVector3(),
                    Color.DeepSkyBlue.ToVector3(),
                    Color.SkyBlue.ToVector3(),
                    Color.LightSkyBlue.ToVector3(),
                };


            SwordProjInfo info = new SwordProjInfo(item.type, gradCols, 4f, 0f, 45f, 5, 3f, 1.35f, 1f);
            info.flowSpeed = 0f;
            (Main.projectile[trail].ModProjectile as BaseSwordProj).info = info;

            base.UseAnimation(item, player);
        }

        public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
        {
            if (Main.rand.Next(5) == 0)
            {
                //Color col = Main.rand.NextBool(2) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
                //int d = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<PulseInOutDust>(), 0f, 0f, 0, col, Main.rand.NextFloat(0.35f, 0.55f) * 2f);
                //Main.dust[d].velocity *= 0.25f;
                //Main.dust[d].customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 20, 0.5f, 0.5f, Pixelize: true);
            }

            //Using itemAnimation instead of itemTime is very important here
            //Sometimes itemTime will stay at zero even when using the weapon for some reason
            if (player.itemAnimation % 4 == 0 && false)
            {
                Color col = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                GeneralUtilities.GetPointOnSwungItemPath(player, 40f, 40f, 0.35f + 0.65f * Main.rand.NextFloat(), player.GetAdjustedItemScale(item), out var location2, out var outwardDirection2);

                Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);

                Dust d = Dust.NewDustPerfect(location2, ModContent.DustType<PulseInOutDust>(), vector2 * 1.5f, 0, col with { A = 200 }, Main.rand.NextFloat(0.7f, 0.9f));

                int pulseTime = Main.rand.Next(18, 22);
                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, pulseTime, 0.5f, 0.5f, Pixelize: true);
            }

            if (player.itemAnimation % 4 == 0 && false)
            {
                Color col = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                GeneralUtilities.GetPointOnSwungItemPath(player, 40f, 40f, 0.35f + 0.65f * Main.rand.NextFloat(), player.GetAdjustedItemScale(item), out var location2, out var outwardDirection2);

                Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);

                //Dust d = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, ModContent.DustType<GlowPixelCross>(), 0f, 0f, 0, col, Main.rand.NextFloat(0.15f, 0.3f));
                //d.velocity *= 0.25f;
                //d.velocity += vector2 * 0.5f;

                Dust d = Dust.NewDustPerfect(location2, ModContent.DustType<GlowPixelCross>(), vector2 * 1.5f, 0, col with { A = 200 }, Main.rand.NextFloat(0.15f, 0.3f));
                d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.1f, timeBeforeSlow: 3, preSlowPower: 0.99f, postSlowPower: 0.92f,
                    velToBeginShrink: 0.75f, fadePower: 0.95f, shouldFadeColor: false);
            }

            //Lighting.AddLight(player.itemLocation, Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f).ToVector3() * 0.25f);

            //Vanilla
            /*
            if (Main.rand.Next(5) == 0)
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 58, 0f, 0f, 150, default(Color), 1.2f);
            }
            if (Main.rand.Next(10) == 0)
            {
                Gore.NewGore(null, new Vector2(hitbox.X, hitbox.Y), default(Vector2), Main.rand.Next(16, 18));
            }
            */

            base.MeleeEffects(item, player, hitbox);
        }

        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {

            
        }

    }

    public class IceBladeShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.IceBolt);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 0;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPositions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (timer % 1 == 0 && timer > 3)
            {
                Color fireCol = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.15f);
                fireCol = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.5f);

                fireCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0f);


                Color bloomCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.75f);
                //fireCol = Color.DeepSkyBlue;

                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.2f) * -Main.rand.NextFloat(2.5f, 7f); //2.5-7
                    FireParticleAlpha fire = new FireParticleAlpha(projectile.Center + new Vector2(0f, 0f), -vel, 0.6f, fireCol, colorMult: 1f, bloomAlpha: 2f, AlphaFade: 0.95f, 
                        EndAlpha: 1f, BlackRemoveThreshold: 1f);
                    fire.bloomColor = bloomCol with { A = 200 };

                    fire.scaleFadePower = 1.05f;
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }

            }

            if (timer % 2 == 0 && Main.rand.NextBool() && false)
            {
                Color dustCol = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                Vector2 dustVel = Main.rand.NextVector2Circular(1f, 1f) + projectile.velocity * 0.02f;

                Dust d = Dust.NewDustPerfect(projectile.Center + Main.rand.NextVector2Circular(10f, 10f), ModContent.DustType<PulseInOutDust>(), dustVel, newColor: dustCol with { A = 200 });
                d.scale *= Main.rand.NextFloat(1f, 1.5f) * 1f;

                d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, 14, 0.15f, 0.85f, Pixelize: true);
            }

            overallAlpha = 1f;
            //overallScale = 1f;

            //overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.05f), 0f, 1f);

            float fadeInTime = Math.Clamp((timer + 6f) / 18f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;
            return false;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        float overallScale = 1f;
        float overallAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawPixelatedStuff(projectile, false);
            });
            DrawPixelatedStuff(projectile, true);



            //Gash
            Texture2D gash = CommonTextures.Flare.Value;

            float drawScale = projectile.scale * overallScale;

            Vector2 gashPos = projectile.Center - Main.screenPosition + projectile.velocity.SafeNormalize(Vector2.UnitX) * 0f;
            float gashProg = Utils.GetLerpValue(0, 20, timer, true);
            float gashRot = 0f;// projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Vector2 gashScale = new Vector2(2f * Easings.easeOutSine(gashProg), 0.5f) * drawScale;
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.HotPink with { A = 50 } * Easings.easeOutQuad(1f - gashProg) * 0.25f, gashRot, gash.Size() / 2f, gashScale * 2f, SpriteEffects.None);
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.White with { A = 50 } * Easings.easeOutQuad(1f - gashProg) * 0.25f, gashRot, gash.Size() / 2f, gashScale * 1f, SpriteEffects.None);

            Vector2 newGashScale = new Vector2(1f, 0.5f) * drawScale;
            Vector2 newGashScale2 = new Vector2(1f + ((float)(Main.timeForVisualEffects * 0.05f) % 1f), 0.5f) * drawScale;
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.DeepPink with { A = 100 } * 0.5f, gashRot, gash.Size() / 2f, newGashScale * 2f, SpriteEffects.None);
            //Main.EntitySpriteDraw(gash, gashPos, null, Color.White with { A = 100 } * 0.5f, gashRot, gash.Size() / 2f, newGashScale2 * 1f, SpriteEffects.None);



            return false;

        }

        public void DrawPixelatedStuff(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            float drawScale = projectile.scale * overallScale;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Deerclops_ice_attack_1") with { Volume = .05f, Pitch = 0.9f, PitchVariance = 0.3f, MaxInstances = 1 };
            SoundEngine.PlaySound(style, projectile.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Item_107Trim") with { Volume = .27f, Pitch = .7f, PitchVariance = 0.2f, MaxInstances = 1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            for (int i = 220; i < 4; i++)
            {
                Color col2 = Color.Lerp(Color.SkyBlue, Color.DeepSkyBlue, 0.15f);

                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), vel, newColor: col2, Scale: Main.rand.NextFloat(0.25f, 0.5f) * 1.15f);
                HighResSmokeBehavior hrsb = new HighResSmokeBehavior();
                hrsb.overallAlpha = 0.3f;
                hrsb.isPixelated = true;
                d.customData = hrsb;
            }

            Color betweenBlue = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.5f);
            for (int i = 220; i < 5 + Main.rand.Next(1, 6); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                    newColor: betweenBlue * 0.65f, Scale: Main.rand.NextFloat(0.25f, 0.5f) * projectile.scale);

                p.velocity += projectile.velocity * 0.05f;
            }

            return true;
        }

    }
}
