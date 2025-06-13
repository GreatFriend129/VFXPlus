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
using Terraria.GameContent;


namespace VFXPlus.Content.Projectiles
{
    public class BloodHitAnim : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.timeLeft = 200;
            Projectile.penetrate = -1;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;


        public override void AI()
        {
            if (Projectile.frame == 0 && Projectile.timeLeft == 200)
            {
                //Hold the first frame for a little bit longer to add more oomf //OMG OOMFIE
                Projectile.frameCounter = 1;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 1)
            {
                if (Projectile.frame == 9)
                    Projectile.active = false;

                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Assets/Anim/BloodHit").Value;
            Texture2D Ball = Mod.Assets.Request<Texture2D>("Assets/Orbs/SoftGlow64").Value;

            int frameHeight = Tex.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Vector2 offset = new Vector2(15f, 0f).RotatedBy(Projectile.rotation) * Projectile.scale;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + offset;

            Rectangle sourceRectangle = new Rectangle(0, startY, Tex.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            Main.spriteBatch.Draw(Ball, drawPos, null, Color.Red with { A = 0 } * 0.3f, Projectile.rotation, Ball.Size() / 2f, Projectile.scale * 1.5f, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Tex, drawPos, sourceRectangle, Color.Red * 0.2f, Projectile.rotation + MathHelper.PiOver2, origin, Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Tex, drawPos, sourceRectangle, Color.Red, Projectile.rotation + MathHelper.PiOver2, origin, Projectile.scale * 0.75f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
