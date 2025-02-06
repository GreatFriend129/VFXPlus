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
using Terraria.GameContent;
using System.Linq;
using Microsoft.Build.Evaluation;
using System.IO;
using System.Reflection.Metadata;

namespace VFXPlus.Content.VFXTest
{

    public class MusketTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;

            Projectile.DamageType = DamageClass.Ranged;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public int AnimationTime = 30;

        float shotAngle = 0f;

        Vector2 storedMuzzleFlashPos = Vector2.Zero;

        int timer = 0;
        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            ProjectileExtensions.KillHeldProjIfPlayerDeadOrStunned(Projectile);

            Projectile.velocity = Vector2.Zero;

            //if (Player.itemTime <= 1)
            //    Projectile.active = false;

            if (timer - 1 == AnimationTime)
                Projectile.active = false;

            if (timer == 0 && Projectile.owner == Main.myPlayer)
            {
                shotAngle = (Main.MouseWorld - Player.Center).ToRotation();
                bonusPower = 1f;
            }

            GunDirection = shotAngle.ToRotationVector2();
            Player.ChangeDir(GunDirection.X > 0 ? 1 : -1);

            //XOffset
            if (timer == 2)
            {
                XOffset = 5f;
            }
            if (timer > 2)
            {
                float easeProgress = MathHelper.Lerp(0f, 1f, Math.Clamp((timer - 3f) / 20f, 0f, 1f));
                XOffset = MathHelper.Lerp(5f, 18f, Easings.easeOutCubic(easeProgress)); //trycubic
            }


            //YOffset
            if (timer > 3) //1 | 4
            {
                if (YRecoilReachedEnd == false)
                    YRecoil = Math.Clamp(MathHelper.Lerp(YRecoil, 1f, 0.12f), 0, 0.3f);
                else
                    YRecoil = Math.Clamp(MathHelper.Lerp(YRecoil, -0.2f, 0.06f), 0, 0.3f);

                if (YRecoil == 0.3f)
                    YRecoilReachedEnd = true;
            }

            if (timer > 3)
                muzzleFlashPower = Math.Clamp(MathHelper.Lerp(muzzleFlashPower, -0.5f, 0.15f), 0f, 1f);

            //StandardHeldProjCode
            GunDirection = shotAngle.ToRotationVector2().RotatedBy(YRecoil * Player.direction * -1f); ;
            Projectile.Center = Player.MountedCenter + (GunDirection * XOffset);
            Projectile.velocity = Vector2.Zero;
            Player.itemRotation = shotAngle;

            if (Player.direction != 1)
                Player.itemRotation -= 3.14f;

            Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);

            Player.heldProj = Projectile.whoAmI;
            Projectile.rotation = GunDirection.ToRotation();

            bonusPower *= 0.8f;

            timer++;
        }

        public Vector2 GunDirection = Vector2.Zero;

        float XOffset = 18f;
        float YRecoil = 0f;
        bool YRecoilReachedEnd = false;

        float bonusPower = 0f;
        float muzzleFlashPower = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Texture = TextureAssets.Item[ItemID.Musket].Value;

            Texture2D MuzzleFlash = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/OrangePixelMuzzleFlash").Value;
            Texture2D MuzzleFlashGlow = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/OrangePixelMuzzleFlashGlow").Value;
            Texture2D MuzzleFlashGlowWhite = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/WhitePixelMuzzleFlashGlow").Value;

            Player Player = Main.player[Projectile.owner];
            SpriteEffects mySE = Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Vector2 holdoutOffset = new Vector2(0f, 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Player.gfxOffY) + holdoutOffset;



            Vector2 muzzleFlashPos = drawPos + holdoutOffset + new Vector2(33f, -3f * Player.direction).RotatedBy(Projectile.rotation);
            Vector2 muzzleFlashOrigin = new Vector2(MuzzleFlash.Width / 2f, MuzzleFlash.Height / 2f);

            float easedMuzzleFlashAlpha = Easings.easeInSine(muzzleFlashPower);
            float muzzleFlashScale = Projectile.scale * 0.75f;// * Easings.easeOutCirc(muzzleFlashPower) * 0.75f;

            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);


            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos + Main.rand.NextVector2Circular(3f, 3f), null, between with { A = 0 } * easedMuzzleFlashAlpha * 0.75f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);

            Main.spriteBatch.Draw(MuzzleFlash, muzzleFlashPos, null, Color.White * easedMuzzleFlashAlpha * 1f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);


            //Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos, null, between with { A = 0 } * muzzleFlashPower * 0.5f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);
            Main.spriteBatch.Draw(MuzzleFlashGlowWhite, muzzleFlashPos, null, between with { A = 0 } * (1f * bonusPower), Projectile.rotation, muzzleFlashOrigin, 1.5f * (1f - bonusPower), mySE, 0f);


            Main.spriteBatch.Draw(Texture, drawPos, null, lightColor, Projectile.rotation, Texture.Size() / 2, Projectile.scale, mySE, 0f);
            return false;
        }

    }

    public class MusketTest2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;

            Projectile.DamageType = DamageClass.Ranged;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public int AnimationTime = 40;

        float shotAngle = 0f;

        Vector2 storedMuzzleFlashPos = Vector2.Zero;

        int timer = 0;
        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            ProjectileExtensions.KillHeldProjIfPlayerDeadOrStunned(Projectile);

            Projectile.velocity = Vector2.Zero;

            if (Player.itemTime <= 1)
                Projectile.active = false;

            if (timer - 1 == AnimationTime)
                Projectile.active = false;

            if (timer == 0 && Projectile.owner == Main.myPlayer)
            {
                shotAngle = (Main.MouseWorld - Player.Center).ToRotation();
                bonusPower = 1f;
            }

            GunDirection = shotAngle.ToRotationVector2();
            Player.ChangeDir(GunDirection.X > 0 ? 1 : -1);

            int XAnimTime = 25;
            float goalX = 4f;
            float baseX = 18f;

            //Should add up to 1
            Vector2 animRatioX = new Vector2(0.15f, 0.85f);

            float xAnimProgress = (float)(Math.Clamp(timer, 0f, XAnimTime) / XAnimTime);

            //Move Out
            if (xAnimProgress < animRatioX.X)
            {
                float prog = xAnimProgress / animRatioX.X;
                XOffset = MathHelper.Lerp(baseX, goalX, Easings.easeInOutBack(prog, 1f, 0f));
            }
            //Move back in
            else
            {
                float prog = (xAnimProgress - animRatioX.X) / animRatioX.Y;
                XOffset = MathHelper.Lerp(goalX, baseX, Easings.easeInOutBack(prog, 1f, 0f));//;Easings.easeInOutHarsh(prog));// ||easeInOutBack(1f0f) \\outCirc
            }

            int YAnimTime = 25;
            float goalY = 0.25f;
            float baseY = 0f;

            //MUST add up to 1
            Vector2 animRatioY = new Vector2(0.25f, 0.75f);

            float yAnimProgress = (float)(Math.Clamp(timer, 0f, YAnimTime) / YAnimTime);

            //RecoilUp
            if (yAnimProgress < animRatioY.X)
            {
                float prog = yAnimProgress / animRatioY.X;
                YRecoil = MathHelper.Lerp(baseY, goalY, Easings.easeOutCubic(prog));
            }
            //RecoilDown
            else
            {
                float prog = (yAnimProgress - animRatioY.X) / animRatioY.Y;
                YRecoil = MathHelper.Lerp(goalY, baseY, Easings.easeInOutBack(prog, 0f, 1f)); //
            }


            if (timer > 3)
                muzzleFlashPower = Math.Clamp(MathHelper.Lerp(muzzleFlashPower, -0.5f, 0.15f), 0f, 1f);

            //StandardHeldProjCode
            GunDirection = shotAngle.ToRotationVector2().RotatedBy(YRecoil * Player.direction * -1f); ;
            Projectile.Center = Player.MountedCenter + (GunDirection * XOffset);
            Projectile.velocity = Vector2.Zero;
            Player.itemRotation = shotAngle;

            if (Player.direction != 1)
                Player.itemRotation -= 3.14f;

            Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);

            Player.heldProj = Projectile.whoAmI;
            Projectile.rotation = GunDirection.ToRotation();

            bonusPower *= 0.8f;

            timer++;
        }

        public Vector2 GunDirection = Vector2.Zero;

        float XOffset = 18f;
        float YRecoil = 0f;
        bool YRecoilReachedEnd = false;

        float bonusPower = 0f;
        float muzzleFlashPower = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Utils.DrawBorderString(Main.spriteBatch, "" + YRecoil, Projectile.Center - Main.screenPosition + new Vector2(0f, -60f), Color.White);
            
            Texture2D Texture = TextureAssets.Item[ItemID.Musket].Value;

            Texture2D MuzzleFlash = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/OrangePixelMuzzleFlash").Value;
            Texture2D MuzzleFlashGlow = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/OrangePixelMuzzleFlashGlow").Value;
            Texture2D MuzzleFlashGlowWhite = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/WhitePixelMuzzleFlashGlow").Value;

            Player Player = Main.player[Projectile.owner];
            SpriteEffects mySE = Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Vector2 holdoutOffset = new Vector2(0f, 1f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Player.gfxOffY) + holdoutOffset;



            Vector2 muzzleFlashPos = drawPos + holdoutOffset + new Vector2(33f, -3f * Player.direction).RotatedBy(Projectile.rotation);
            Vector2 muzzleFlashOrigin = new Vector2(MuzzleFlash.Width / 2f, MuzzleFlash.Height / 2f);

            float easedMuzzleFlashAlpha = Easings.easeInSine(muzzleFlashPower);
            float muzzleFlashScale = Projectile.scale * 0.75f;// * Easings.easeOutCirc(muzzleFlashPower) * 0.75f;

            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);
            Main.spriteBatch.Draw(Texture, drawPos, null, lightColor, Projectile.rotation, Texture.Size() / 2, Projectile.scale, mySE, 0f);


            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos + Main.rand.NextVector2Circular(3f, 3f), null, between with { A = 0 } * easedMuzzleFlashAlpha * 0.75f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);

            Main.spriteBatch.Draw(MuzzleFlash, muzzleFlashPos, null, Color.White * easedMuzzleFlashAlpha * 1f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);


            //Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos, null, between with { A = 0 } * muzzleFlashPower * 0.5f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);
            Main.spriteBatch.Draw(MuzzleFlashGlowWhite, muzzleFlashPos, null, between with { A = 0 } * (1f * bonusPower), Projectile.rotation, muzzleFlashOrigin, 1.5f * (1f - bonusPower), mySE, 0f);


            return false;
        }

    }

    public class MinisharkTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;

            Projectile.DamageType = DamageClass.Ranged;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public int AnimationTime = 40;

        float shotAngle = 0f;

        Vector2 storedMuzzleFlashPos = Vector2.Zero;

        int timer = 0;
        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            ProjectileExtensions.KillHeldProjIfPlayerDeadOrStunned(Projectile);

            Projectile.velocity = Vector2.Zero;

            if (Player.itemTime <= 1)
                Projectile.active = false;

            if (timer - 1 == AnimationTime)
                Projectile.active = false;

            if (timer == 0 && Projectile.owner == Main.myPlayer)
            {
                shotAngle = (Main.MouseWorld - Player.Center).ToRotation();
                bonusPower = 1f;
            }

            GunDirection = shotAngle.ToRotationVector2();
            Player.ChangeDir(GunDirection.X > 0 ? 1 : -1);

            int XAnimTime = 25 / 2;
            float goalX = 12f;
            float baseX = 18f;

            //Should add up to 1
            Vector2 animRatioX = new Vector2(0.15f, 0.85f);

            float xAnimProgress = (float)(Math.Clamp(timer, 0f, XAnimTime) / XAnimTime);

            //Move Out
            if (xAnimProgress < animRatioX.X)
            {
                float prog = xAnimProgress / animRatioX.X;
                XOffset = MathHelper.Lerp(baseX, goalX, Easings.easeInOutBack(prog, 1f, 0f));
            }
            //Move back in
            else
            {
                float prog = (xAnimProgress - animRatioX.X) / animRatioX.Y;
                XOffset = MathHelper.Lerp(goalX, baseX, Easings.easeInOutBack(prog, 1f, 0f));//;Easings.easeInOutHarsh(prog));// ||easeInOutBack(1f0f) \\outCirc
            }

            int YAnimTime = 25 / 2;
            float goalY = 0.1f;
            float baseY = 0f;

            //MUST add up to 1
            Vector2 animRatioY = new Vector2(0.15f, 0.85f);

            float yAnimProgress = (float)(Math.Clamp(timer, 0f, YAnimTime) / YAnimTime);

            //RecoilUp
            if (yAnimProgress < animRatioY.X)
            {
                float prog = yAnimProgress / animRatioY.X;
                YRecoil = MathHelper.Lerp(baseY, goalY, Easings.easeOutCubic(prog));
            }
            //RecoilDown
            else
            {
                float prog = (yAnimProgress - animRatioY.X) / animRatioY.Y;
                YRecoil = MathHelper.Lerp(goalY, baseY, Easings.easeInOutBack(prog, 0f, 1f)); //
            }


            if (timer > 1)
                muzzleFlashPower = Math.Clamp(MathHelper.Lerp(muzzleFlashPower, -0.5f, 0.15f), 0f, 1f);

            //StandardHeldProjCode
            GunDirection = shotAngle.ToRotationVector2().RotatedBy(YRecoil * Player.direction * -1f); ;
            Projectile.Center = Player.MountedCenter + (GunDirection * XOffset);
            Projectile.velocity = Vector2.Zero;
            Player.itemRotation = shotAngle;

            if (Player.direction != 1)
                Player.itemRotation -= 3.14f;

            Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);

            Player.heldProj = Projectile.whoAmI;
            Projectile.rotation = GunDirection.ToRotation();

            bonusPower *= 0.8f;

            timer++;
        }

        public Vector2 GunDirection = Vector2.Zero;

        float XOffset = 18f;
        float YRecoil = 0f;
        bool YRecoilReachedEnd = false;

        float bonusPower = 0f;
        float muzzleFlashPower = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Utils.DrawBorderString(Main.spriteBatch, "" + YRecoil, Projectile.Center - Main.screenPosition + new Vector2(0f, -60f), Color.White);

            Texture2D Texture = TextureAssets.Item[ItemID.Minishark].Value;

            Texture2D MuzzleFlash = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/MiddleMuzzleFlash").Value;
            Texture2D MuzzleFlashGlow = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/MiddleMuzzleFlashGlow").Value;

            Player Player = Main.player[Projectile.owner];
            SpriteEffects mySE = Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Vector2 holdoutOffset = new Vector2(0f, 0f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Player.gfxOffY) + holdoutOffset;



            Vector2 muzzleFlashPos = drawPos + holdoutOffset + new Vector2(38f, 3f * Player.direction).RotatedBy(Projectile.rotation);
            Vector2 muzzleFlashOrigin = new Vector2(MuzzleFlash.Width / 2f, MuzzleFlash.Height / 2f);

            float easedMuzzleFlashAlpha = Easings.easeInSine(muzzleFlashPower);
            float muzzleFlashScale = Projectile.scale * 2f;// * Easings.easeOutCirc(muzzleFlashPower) * 0.75f;

            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);


            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos + Main.rand.NextVector2Circular(3f, 3f), null, between with { A = 0 } * easedMuzzleFlashAlpha * 0.75f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, 0, 0f);

            Main.spriteBatch.Draw(MuzzleFlash, muzzleFlashPos, null, Color.White * easedMuzzleFlashAlpha * 1f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, 0, 0f);

            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos, null, between with { A = 0 } * (1f * bonusPower), Projectile.rotation, muzzleFlashOrigin, 3f * (1f - bonusPower), 0, 0f);

            Main.spriteBatch.Draw(Texture, drawPos, null, lightColor, Projectile.rotation, Texture.Size() / 2, Projectile.scale, mySE, 0f);

            return false;
        }

    }


}