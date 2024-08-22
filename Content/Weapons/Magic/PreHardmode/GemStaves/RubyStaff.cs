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
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Terraria.GameContent;
using Terraria.Graphics;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.MagicGuns
{
    
    public class RubyStaff : GlobalItem 
    {

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.RubyStaff);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };

            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {



            //Dust
            /*
            for (int i = 0; i < 3 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<LineSpark>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 15),
                    newColor: Color.Green, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    0.95f, 0.75f); //80

            }
            */
            return true;
        }

    }
    public class RubyStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RubyBolt);
        }

        public List<Vector2> l_positions = new List<Vector2>();
        public List<float> l_rotations = new List<float>();
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            projectile.rotation = projectile.velocity.ToRotation();

            if (timer % 2 == 0 && Main.rand.NextBool(2))
            {
                int d = Dust.NewDust(projectile.Center, 5, 5, ModContent.DustType<GlowPixelCross>(), newColor: Color.Red, Scale: 0.35f);
                Main.dust[d].velocity += projectile.velocity * 0.25f;
                Main.dust[d].velocity *= 0.25f;
            }

            fadeInAlpha = 1f;

            //Trail
            l_positions.Add(projectile.Center + projectile.velocity);
            l_rotations.Add(projectile.rotation);

            int trailMaxLength = 20;
            if (l_positions.Count > trailMaxLength)
            {
                l_positions.RemoveAt(0);
                l_rotations.RemoveAt(0);
            }

            timer++;
            return false;
        }


        float fadeInAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Trail
            #region Trail
            

            #endregion

            //Spark
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Nightglow").Value;
            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/SoftGlow64").Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Vector2 vec2ScaleLine = new Vector2(1f, 1f) * projectile.scale;
            Vector2 vec2ScaleOrb = new Vector2(0.8f, 0.5f) * projectile.scale;

            Main.spriteBatch.Draw(line, drawPos, null, Color.Black * 0.8f, projectile.rotation - MathHelper.PiOver2, line.Size() / 2, vec2ScaleLine * 1.2f, SpriteEffects.None, 0.0f);

            Main.spriteBatch.Draw(line, drawPos, null, Color.Red with { A = 0 } * 0.75f, projectile.rotation - MathHelper.PiOver2, line.Size() / 2, vec2ScaleLine * 1.2f, SpriteEffects.None, 0.0f);
            Main.spriteBatch.Draw(line, drawPos, null, Color.White with { A = 0 } * 1f, projectile.rotation - MathHelper.PiOver2, line.Size() / 2, vec2ScaleLine * 0.6f, SpriteEffects.None, 0.0f);


            Main.spriteBatch.Draw(orb, drawPos, null, Color.Red with { A = 0 } * 0.65f, projectile.rotation, orb.Size() / 2, vec2ScaleOrb, SpriteEffects.None, 0.0f);


            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return false;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {

            return base.OnTileCollide(projectile, oldVelocity);
        }

    }

}
