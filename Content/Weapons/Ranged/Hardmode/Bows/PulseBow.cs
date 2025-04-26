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
            player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;
            //player.GetModPlayer<HeldBowPlayer>().underGlowColor = new Color(42, 2, 82);
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

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;

            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                Vector2 trailPos = previousPositions[i] - Main.screenPosition;

                //float trailAlpha = progress * progress * projectile.Opacity;

                //Start End
                Color trailColor = Color.Lerp(Color.SkyBlue, Color.DodgerBlue, 1f - progress);

                Vector2 trailScaleThick = new Vector2(0.5f, 0.1f + Easings.easeInQuad(progress) * 0.5f) * overallScale;
                Vector2 trailScaleThin = new Vector2(trailScaleThick.X, trailScaleThick.Y * 0.55f);

                Main.EntitySpriteDraw(flare, trailPos, null, trailColor with { A = 0 }, previousRotations[i], 
                    flare.Size() / 2f, trailScaleThick, 0);

                //Main.EntitySpriteDraw(flare, trailPos, null, Color.White with { A = 0 }, previousRotations[i], 
                    //flare.Size() / 2f, trailScaleThin, 0);
            }
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++)
            {
                Vector2 dustVel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.Pi + Main.rand.NextFloat(-1f, 1f)) * Main.rand.NextFloat(1f, 3f);
                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.2f, 0.4f) * 1.5f);

                p.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                        rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 8, postSlowPower: 0.92f, velToBeginShrink: 4f, fadePower: 0.88f, shouldFadeColor: false);
            }

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
