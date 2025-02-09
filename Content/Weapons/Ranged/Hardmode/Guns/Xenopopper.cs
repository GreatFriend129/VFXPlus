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
using Terraria.Graphics;
using Terraria.Physics;
using AerovelenceMod.Content.Items.Weapons.AreaPistols.ErinGun;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Guns
{
    public class Xenopopper : GlobalItem
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Xenopopper);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            //entity.noUseGraphic = true;
            base.SetDefaults(entity);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            return true;
        }
    }


    public class XenoPopperBubbleOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Xenopopper);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            
            return true;
        }


        float totalAlpha = 0f;
        float totalScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //Bloomball
            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;


            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.White * 0.75f, projectile.rotation, TexOrigin, projectile.scale, 0);



            float glowscale = (float)(Math.Sin(Main.GlobalTimeWrappedHourly * 6f) / 5f + 1f) * 1.15f;
            Main.EntitySpriteDraw(vanillaTex, drawPos, null, Color.White with { A = 0 } * 0.25f, projectile.rotation, TexOrigin, projectile.scale * glowscale, 0);

            Main.EntitySpriteDraw(Ball, drawPos + new Vector2(0f, 0f), null, Color.Purple with { A = 0 } * 0.5f, projectile.rotation, Ball.Size() / 2f, projectile.scale * 0.3f, 0);


            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            

            return true;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
