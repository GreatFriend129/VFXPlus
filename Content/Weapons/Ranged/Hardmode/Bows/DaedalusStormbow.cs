using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VFXPlus.Content.Dusts;
using VFXPlus.Common.Drawing;
using Microsoft.Xna.Framework.Graphics;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class DaedalusStormbowOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.DaedalusStormbow);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            
            //Vector2 vector54 = new Vector2(num102, num113);
            //vector54.X = (float)Main.mouseX + Main.screenPosition.X - pointPoisition.X;
            //vector54.Y = (float)Main.mouseY + Main.screenPosition.Y - pointPoisition.Y - 1000f;
            //player.itemRotation = (float)Math.Atan2(vector54.Y * (float)base.direction, vector54.X * (float)base.direction);
            //NetMessage.SendData(13, -1, -1, null, base.whoAmI);
            //NetMessage.SendData(41, -1, -1, null, base.whoAmI);


            int num12 = 3;
            if (ProjectileID.Sets.FiresFewerFromDaedalusStormbow[type])
            {
                if (Main.rand.Next(3) == 0)
                {
                    num12--;
                }
            }
            else if (Main.rand.Next(3) == 0)
            {
                num12++;
            }
            for (int k = 0; k < num12; k++)
            {
                // Vector2 pointPoisition = new Vector2(base.position.X + (float)base.width * 0.5f + (float)(Main.rand.Next(201) * -base.direction) + ((float)Main.mouseX + Main.screenPosition.X - base.position.X), this.MountedCenter.Y - 600f);
                // pointPoisition.X = (pointPoisition.X * 10f + base.Center.X) / 11f + (float)Main.rand.Next(-100, 101);

                Vector2 pointPoisition = new Vector2(player.position.X + (float)player.width * 0.5f + (float)(Main.rand.Next(201) * -player.direction) + ((float)Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
                pointPoisition.X = (pointPoisition.X * 10f + player.Center.X) / 11f + (float)Main.rand.Next(-100, 101);
                pointPoisition.Y -= 150 * k;
                float num102 = (float)Main.mouseX + Main.screenPosition.X - pointPoisition.X;
                float num113 = (float)Main.mouseY + Main.screenPosition.Y - pointPoisition.Y;
                if (num113 < 0f)
                {
                    num113 *= -1f;
                }
                if (num113 < 20f)
                {
                    num113 = 20f;
                }
                float num124 = (float)Math.Sqrt(num102 * num102 + num113 * num113);
                num124 = item.shootSpeed / num124; // num124 = speed / num124;

                num102 *= num124;
                num113 *= num124;
                float num23 = num102 + (float)Main.rand.Next(-40, 41) * 0.03f;
                float speedY = num113 + (float)Main.rand.Next(-40, 41) * 0.03f;
                num23 *= (float)Main.rand.Next(75, 150) * 0.01f;
                pointPoisition.X += Main.rand.Next(-50, 51);
                int num34 = Projectile.NewProjectile(source, pointPoisition.X, pointPoisition.Y, num23, speedY, type, damage, knockback); //,i)
                Main.projectile[num34].noDropItem = true;

                Vector2 vel = new Vector2(num23, speedY);

                //Dust
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel2 = Main.rand.NextVector2Circular(1.5f, 1.5f);
                    Color col2 = Color.Lerp(Color.SkyBlue, Color.Pink, 0.9f); //75

                    Dust d2 = Dust.NewDustPerfect(pointPoisition, ModContent.DustType<SmallSmoke>(), vel2, newColor: col2 with { A = 0 }, Scale: 1.5f);
                    d2.velocity += vel.RotatedByRandom(0.75f) * 0.07f * i;

                    SmallSmokeBehavior ssb = new SmallSmokeBehavior(7f, 0.89f);
                    ssb.timeBetweenFrames = 3;
                    d2.customData = ssb;
                }

                Vector2 portalVel = vel.SafeNormalize(Vector2.UnitX) * 5f;
                int portal = Projectile.NewProjectile(null, pointPoisition, portalVel, ModContent.ProjectileType<StormbowVFX>(), 0, 0, Main.myPlayer);
                Main.projectile[portal].rotation = vel.ToRotation() + MathHelper.PiOver2;
            }

            return false;
        }

    }

    public class StormbowVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            //Make sure to draw projectile even if its position is off screen
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        //Safety Checks
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2400;
        }

        int timer = 0;
        float true_width = 1f;
        float true_alpha = 1f;

        public override void AI()
        {
            if (timer > 2)
                true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.08f), 0, 1f);

            if (timer == 100 || true_width <= 0.05f)
                Projectile.active = false;

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawPortal(false);
            });
            DrawPortal(true);

            return false;
        }

        public void DrawPortal(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //Portal at first node
            Vector2 portalPos = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;

            float easedScale = true_width;
            Vector2 v2Scale = new Vector2(1f * easedScale, 0.25f + (easedScale * 0.75f)) * Projectile.scale * 1.25f;

            
            Main.EntitySpriteDraw(portal, portalPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.DeepPink with { A = 0 } * 0.5f, rot, portal.Size() / 2f, v2Scale * 1.25f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, portalPos, null, Color.Pink with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, portalPos, null, Color.White with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale * 0.5f, SpriteEffects.None);

        }
    }

}
