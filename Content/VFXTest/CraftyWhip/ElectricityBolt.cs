using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace VFXPlus.Content.VFXTest.CraftyWhip
{
    public class ElectricityBolt : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 14;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 20;
            Projectile.spriteDirection = Projectile.direction;
        }

        int i;

        public override void AI()
        {
            i++;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.frameCounter++;
            if (Projectile.frameCounter % 3 == 0) //does the exact same thing but more elegant
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= 4)
                    Projectile.frame = 0;
            }
            
            if (i % 5 == 0)
            {
                //int dust = Dust.NewDust(Projectile.position, Projectile.width / 2, Projectile.height / 2, DustID.Firework_Blue);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 600);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 20; i < 8; i++)
            {
                int p = Dust.NewDust(Projectile.Center, Projectile.width / 2, Projectile.height / 2, DustID.FireworkFountain_Blue, Scale: 1.2f);
                Main.dust[p].noGravity = true;
                Main.dust[p].velocity *= 2f;
            }
            //base.Kill(timeLeft);
        }
    }
}