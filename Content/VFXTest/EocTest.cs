using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System;
using Microsoft.CodeAnalysis;
using Terraria.GameContent.Drawing;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using Terraria.Utilities;
using Terraria.GameContent;
using Microsoft.Build.Evaluation;
using static tModPorter.ProgressUpdate;
using VFXPlus.Content.Dusts;
using System.Collections.Generic;
using VFXPLus.Common;
using Terraria.Graphics;

namespace VFXPlus.Content.VFXTest
{
    public class EocTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 111800;
        }

        bool phase2 = true;
        int timer = 0;
        public override void AI()
        {
            Projectile.rotation = (Main.player[Main.myPlayer].Center - Projectile.Center).ToRotation();
            //Projectile.rotation += 0.2f;

            phase2 = true;

            if (timer % 5 == 0)
                frame = (frame + 1) % 3;

            timer++;
        }

        int frame = 0;
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 ballPos = Projectile.Center - Main.screenPosition;

            float rot = Projectile.rotation;
            Vector2 drawPos = ballPos + new Vector2(-20f, 0f).RotatedBy(rot);

            Texture2D Ball = CommonTextures.SoftGlow.Value;

            Texture2D MainTex = Mod.Assets.Request<Texture2D>("Content/VFXTest/BasicEoc").Value;
            Texture2D Vein = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocVein").Value;
            Texture2D VeinGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocVeinGlow").Value;
            Texture2D FullGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/EocFullGlow").Value;

            int frameHeight = MainTex.Height / 6;
            int startY = frameHeight * (frame + (phase2 ? 3 : 0));
            Rectangle sourceRectangle = new Rectangle(0, startY, MainTex.Width, frameHeight);
            Vector2 origin = sourceRectangle.Size() / 2f;

            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(FullGlow, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle,
                    Color.Red with { A = 0 } * 0.2f, rot, origin, 1.05f, 0f);
            }

            //Main.EntitySpriteDraw(FullGlow, drawPos, null, Color.Red with { A = 0 }, 0f, MainTex.Size() / 2f, 1.05f, 0f);

            Main.EntitySpriteDraw(Ball, ballPos + new Vector2(-5f, 0f).RotatedBy(rot), null, Color.Red with { A = 0 } * 0.55f, rot, Ball.Size() / 2f, 0.35f, 0f);

            Main.EntitySpriteDraw(MainTex, drawPos, sourceRectangle, lightColor, rot, origin, 1.05f, 0f);

            Main.EntitySpriteDraw(VeinGlow, drawPos, sourceRectangle, Color.DarkRed with { A =  0 }, rot, origin, 1.05f, 0f);
            Main.EntitySpriteDraw(VeinGlow, drawPos, sourceRectangle, Color.Red with { A = 0 } * 0.5f, rot, origin, 1.05f, 0f);

            Main.EntitySpriteDraw(Vein, drawPos, sourceRectangle, lightColor, rot, origin, 1.05f, 0f);


            return false;
        }


    }

}