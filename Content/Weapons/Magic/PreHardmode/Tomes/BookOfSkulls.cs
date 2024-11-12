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


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Tomes
{
    
    public class BookOfSkulls : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BookofSkulls);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle stylees = new SoundStyle("Terraria/Sounds/Item_117") with { Pitch = .45f, PitchVariance = .25f, Volume = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(stylees, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Custom/dd2_book_staff_cast_0") with { Volume = 0.3f, Pitch = -0.25f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Item_8") with { Volume = 0.85f, PitchVariance = 0.1f, Pitch = .05f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class BookOfSkullsShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BookOfSkullsSkull);
        }


        BaseTrailInfo trail1 = new BaseTrailInfo();
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //Trail
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/LintyTrail").Value;
            trail1.trailColor = Color.OrangeRed * fadeInAlpha * 0.7f;
            trail1.trailPointLimit = 40;
            trail1.trailWidth = (int)(18f * projectile.scale);
            trail1.trailMaxLength = 120;

            trail1.trailTime = timer * 0.05f;
            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();


            float progress = Math.Clamp(timer / 50f, 0f, 1f);
            fadeInAlpha = MathHelper.Lerp(0f, 1f, progress);

            timer++;



            return false;

        }

        float fadeInAlpha = 0f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            //Utils.DrawBorderString(Main.spriteBatch, "" + projectile.aiStyle, projectile.Center + new Vector2(0f, -100) - Main.screenPosition, Color.White);

            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/LintyTrail").Value;
            trail1.TrailDrawing(Main.spriteBatch);
            trail1.TrailDrawing(Main.spriteBatch);
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/FlameTrail").Value;
            trail1.TrailDrawing(Main.spriteBatch);
            trail1.TrailDrawing(Main.spriteBatch);
            trail1.TrailDrawing(Main.spriteBatch);
            trail1.TrailDrawing(Main.spriteBatch);

            Texture2D pixelSwirl = Mod.Assets.Request<Texture2D>("Assets/Pixel/PixelSwirl").Value;
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;


            
            return true;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
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
