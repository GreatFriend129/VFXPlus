using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using Terraria;
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
    
    public class VolcanoItemOverride : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.FieryGreatsword);
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
                    Color.Red.ToVector3(),
                    Color.Lerp(Color.Red, Color.OrangeRed, 0.5f).ToVector3(),
                    Color.Lerp(Color.OrangeRed, Color.Orange, 0f).ToVector3(),
                    Color.Lerp(Color.OrangeRed, Color.Orange, 0.5f).ToVector3(),
                };


            SwordProjInfo info = new SwordProjInfo(item.type, gradCols, 8f, 0f, 90f, 6, 4f, 1f, 0f);
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
            if (player.itemAnimation % 1 == 0)
            {
                //this.GetPointOnSwungItemPath(70f, 70f, 0.2f + 0.8f * Main.rand.NextFloat(), this.GetAdjustedItemScale(sItem), out var location, out var outwardDirection);
                //Vector2 vector = outwardDirection.RotatedBy((float)Math.PI / 2f * (float)base.direction * this.gravDir);

                Color col = Main.rand.NextBool(4) ? Color.Gold : Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

                GeneralUtilities.GetPointOnSwungItemPath(player, 40, 40, 0.25f + 0.75f * Main.rand.NextFloat(), player.GetAdjustedItemScale(item), out var location2, out var outwardDirection2);
                //GeneralUtilities.GetPointOnSwungItemPath(player, 60, 60, 0.2f + 0.8f * Main.rand.NextFloat(), player.GetAdjustedItemScale(item), out var location2, out var outwardDirection2);

                Vector2 vector2 = outwardDirection2.RotatedBy((float)Math.PI / 2f * (float)player.direction * player.gravDir);

                Color fireRed = Color.Lerp(Color.OrangeRed, Color.Red, 0.05f);//0.15
                
                FireParticleAlpha fire = new FireParticleAlpha(location2 - vector2 * 4f, vector2 * 2f, 1f, fireRed, colorMult: 1.5f, bloomAlpha: 3f, AlphaFade: 0.98f);
                fire.bloomColor = fireRed with { A = 150 };
                //fire.scaleFadePower = 1.08f;
                fire.renderLayer = RenderLayer.UnderProjectiles;
                ShaderParticleHandler.SpawnParticle(fire);

                //Dust d = Dust.NewDustPerfect(location2, ModContent.DustType<PulseInOutDust>(), vector2 * 1.5f, 0, col with { A = 200 }, Main.rand.NextFloat(0.7f, 0.9f));
                //int pulseTime = Main.rand.Next(18, 22);
                //d.customData = new PulseInOutDustBehavior(PulseInOutDustBehavior.DrawOptions.GlowStarSharp, pulseTime, 0.5f, 0.5f, Pixelize: true);
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
}
