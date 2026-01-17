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
using static tModPorter.ProgressUpdate;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class PulseBowOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.PulseBow);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.GetModPlayer<HeldBowPlayer>().arrowType = ProjectileID.WoodenArrowHostile;
            player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.PulseBow;
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, -2f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;
            //player.GetModPlayer<HeldBowPlayer>().underGlowColor = new Color(42, 2, 82);


            float circlePulseSize = 0.13f;

            Dust d2 = Dust.NewDustPerfect(position, ModContent.DustType<CirclePulse>(), velocity.SafeNormalize(Vector2.UnitX) * 3f, newColor: Color.DodgerBlue);
            CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize, true, 6, 0.2f, 0.4f);
            b2.drawLayer = "Dusts";
            d2.customData = b2;
            d2.scale = circlePulseSize * 0.05f;

            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);


    }
    public class PulseBowShotOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.PulseBolt);
        }
        public override bool InstancePerEntity => true;


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 32; //18

            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            float fadeInTime = Math.Clamp((timer + 18f) / 35f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

            timer++;
            return false;
            return base.PreAI(projectile);
        }


        float overallScale = 1f;
        float overallAlpha = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);


            return false;

        }

        float starPower = 0f;
        float randomSineOffset = Main.rand.NextFloat(0f, 10f);
        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            Color color = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.75f);

            float overallWidth = 1f + ((float)Math.Sin(Main.timeForVisualEffects * 0.06f) * 0.15f);

            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition;

                //float trailAlpha = progress * progress * projectile.Opacity;

                //Start End
                Color trailColor = Color.Lerp(color, Color.DodgerBlue, Easings.easeOutSine(1f - progress));

                Vector2 trailScaleThick = new Vector2(0.5f, 0.1f + Easings.easeInSine(progress) * 0.45f) * overallScale;
                Vector2 trailScaleThin = new Vector2(trailScaleThick.X, trailScaleThick.Y * 0.55f);

                trailScaleThick.Y *= overallWidth;

                Main.EntitySpriteDraw(flare, trailPos, null, trailColor with { A = 0 }, previousRotations[i], 
                    flare.Size() / 2f, trailScaleThick, 0);

                //Main.EntitySpriteDraw(flare, trailPos, null, Color.White with { A = 0 }, previousRotations[i], 
                    //flare.Size() / 2f, trailScaleThin, 0);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            Color color = Color.Lerp(Color.DeepSkyBlue, Color.DodgerBlue, 0.75f);
            for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: color, Scale: Main.rand.NextFloat(0.35f, 0.55f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.75f, Pitch = 0.15f, PitchVariance = 0.05f, MaxInstances = -1 }, projectile.Center);


            return false;
        }

        bool frameAfterBounce = false;
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Dust
            float rot = Main.rand.NextFloat(6.28f);
            Color starCol = Color.Lerp(Color.DeepSkyBlue, Color.DodgerBlue, 0.65f);
            DustBehaviorUtil.StarDustDrawInfo sdic = new DustBehaviorUtil.StarDustDrawInfo(true, false, false, true, false, 1f);

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: starCol, Scale: 0.95f); //0.8
            d1.rotation = 0f + rot;
            d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.85f, shouldFadeColor: true, sdci: sdic);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: starCol, Scale: 0.95f);
            d2.rotation = MathHelper.PiOver4 + rot;
            d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.85f, shouldFadeColor: true, sdci: sdic);

            for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: starCol * 1f, Scale: Main.rand.NextFloat(0.3f, 0.4f));

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, preSlowPower: 0.98f, timeBeforeSlow: 5, postSlowPower: 0.88f, 
                    velToBeginShrink: 2f, fadePower: 0.88f, shouldFadeColor: false);
            }

            frameAfterBounce = true;

            //Remove the newest trail pos to hide tear
            previousPositions.RemoveAt(previousPositions.Count - 1);
            previousRotations.RemoveAt(previousRotations.Count - 1);

            //Sound
            //SoundStyle style = new SoundStyle("Terraria/Sounds/Research_3") with { Volume = 0.2f, Pitch = .25f, PitchVariance = .2f, MaxInstances = -1 };
            //SoundEngine.PlaySound(style, projectile.Center);

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.15f, PitchVariance = 0.15f, MaxInstances = -1 }, projectile.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_158") with { Volume = 0.15f, Pitch = .65f, PitchVariance = 0.15f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, projectile.Center);

            return base.OnTileCollide(projectile, oldVelocity);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color starCol = Color.Lerp(Color.DeepSkyBlue, Color.DodgerBlue, 0.65f);

            for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(1f, 3.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), vel,
                    newColor: starCol * 1f, Scale: Main.rand.NextFloat(0.3f, 0.4f));

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, preSlowPower: 0.98f, timeBeforeSlow: 5, postSlowPower: 0.88f,
                    velToBeginShrink: 2f, fadePower: 0.88f, shouldFadeColor: false);
            }
        }

    }

}
