using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using VFXPlus.Content.Dusts;
using VFXPlus.Common.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using Terraria.DataStructures;
using static Terraria.NPC;
using ReLogic.Content;
using VFXPlus.Content.Projectiles;
using VFXPlus.Common.Interfaces;

namespace VFXPlus.Content.VFXTest.Aero
{
    public class DarknessDischarge : ModItem
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private int shotCounter = 0;
        
        public override void SetDefaults()
        {
            Item.damage = 26;
            Item.knockBack = 2;


            Item.rare = ItemRarityID.Yellow;
            Item.width = 58;
            Item.height = 20;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shootSpeed = 7f;
            Item.knockBack = 6f;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.shoot = ModContent.ProjectileType<DarknessDart>();
            //item.UseSound = SoundID.Item92;
            Item.noUseGraphic = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            return true;
        }

        public override bool OnPickup(Player player) //Another check for the problem mentioned above
        {
            shotCounter = 0;
            return base.OnPickup(player);
        }
    }

    public class DarknessDart : ModProjectile, IDrawAdditive
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;

            Projectile.extraUpdates = 2;
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();
        BaseTrailInfo trail2 = new BaseTrailInfo();


        int timer = 0;
        public override void AI()
        {

            #region trailInfo
            //Trail1 Info Dump
            trail1.trailTexture = ModContent.Request<Texture2D>("AerovelenceMod/Assets/EnergyTex").Value;
            trail1.trailColor = Color.White * 1f;
            trail1.trailPointLimit = 400;
            trail1.trailWidth = 20;
            trail1.trailMaxLength = 700;
            trail1.timesToDraw = 1;
            trail1.pinch = true;
            trail1.shouldSmooth = true;

            trail1.trailTime = timer * 0.01f;
            trail1.trailRot = Projectile.velocity.ToRotation();
            trail1.trailPos = Projectile.Center;
            trail1.TrailLogic();

            //Trail2 Info Dump
            trail2.trailTexture = ModContent.Request<Texture2D>("AerovelenceMod/Assets/Extra_196_Black").Value;
            trail2.trailColor = Color.Wheat;
            trail2.trailPointLimit = 400;
            trail2.trailWidth = 60;
            trail2.trailMaxLength = 700;
            trail2.timesToDraw = 2;
            trail2.pinch = true;
            trail2.shouldSmooth = true;

            trail2.gradient = true;
            trail2.gradientTexture = ModContent.Request<Texture2D>("AerovelenceMod/Assets/Gradients/CyverGrad2").Value;
            trail2.shouldScrollColor = true;
            trail2.gradientTime = timer * 0.015f;

            trail2.trailTime = timer * 0.02f;
            trail2.trailRot = Projectile.velocity.ToRotation();
            trail2.trailPos = Projectile.Center;
            trail2.TrailLogic();
            #endregion

            Projectile.velocity = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX) * 8;

            timer++;
        }

        public override bool PreKill(int timeLeft)
        {
            return base.PreKill(timeLeft);
        }

        float overallAlpha = 1f;
        float overallScale = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            trail1.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
            trail2.TrailDrawing(Main.spriteBatch, doAdditiveReset: false);
        }
    }
}