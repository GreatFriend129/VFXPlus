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



namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Other
{
    
    public class AquaScepter : GlobalItem 
    {

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.AquaScepter) && false;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f, MaxInstances = -1 };

            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {


            
            return true;
        }

    }
    public class AquaScepterShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.WaterStream) && false;
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            projectile.rotation = projectile.velocity.ToRotation();

            #region trail info
            //Trail1 info dump
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/LintyTrail").Value;
            trail1.trailColor = Color.DodgerBlue * fadeInAlpha * 0.7f;
            trail1.trailPointLimit = 120;
            trail1.trailWidth = (int)(8f * projectile.scale);
            trail1.trailMaxLength = 1000;

            trail1.pinch = true;


            trail1.trailTime = timer * 0.05f;
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            #endregion

            //Lighting.AddLight(projectile.Center, Color.DodgerBlue.ToVector3() * 0.8f * fadeInAlpha);

            projectile.velocity.Y += 0.25f;

            fadeInAlpha = 1f;
            //fadeInAlpha = Math.Clamp(MathHelper.Lerp(fadeInAlpha, 1.25f, 0.04f), 0f, 1f);
            timer++;
            return false;
        }


        float fadeInAlpha = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            trail1.TrailDrawing(Main.spriteBatch);
            trail1.TrailDrawing(Main.spriteBatch);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/ENV_water_splash_01") with { Volume = .2f, Pitch = .15f, PitchVariance = .2f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Pitch = -.7f, PitchVariance = .25f, MaxInstances = 1, Volume = 0.35f };
            //SoundEngine.PlaySound(style, projectile.Center);

            return base.OnTileCollide(projectile, oldVelocity);
        }

    }

}
