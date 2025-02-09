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

    public class ShotgunProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;

            Projectile.DamageType = DamageClass.Ranged;

            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;


        //The item ID of the weapon we are using
        public int gunID = 1;

        public float BaseXOffset = 18f;
        public float GoalXOffset = 4f;
        public float yRecoilPower = 0.075f;

        public Vector2 HoldoutOffset = Vector2.Zero; 
        public Vector2 TipPosition = Vector2.Zero; 
        public Vector2 StarPosition = Vector2.Zero; 

        public int timeToStartFade = 1;

        public void SetProjInfo(int GunID, float NormalXOffset, float DestXOffset, float YRecoilAmount,
            Vector2 HoldOffset, Vector2 TipPos, Vector2 StarPos)
        {
            gunID = GunID;
            BaseXOffset = NormalXOffset;
            GoalXOffset = DestXOffset;
            yRecoilPower = YRecoilAmount;
            HoldoutOffset = HoldOffset;
            TipPosition = TipPos;
            StarPosition = StarPos;
        }

        bool yRecoilReachedDest = false;

        //The angle of the gun shot
        float shotAngle = 0f;

        //Which muzzle flash texture to use
        int muzzleFlashNum = 1;

        int timer = 0;
        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            ProjectileExtensions.KillHeldProjIfPlayerDeadOrStunned(Projectile);

            Projectile.velocity = Vector2.Zero;

            //Kill proj if player is done with item use 
            if (Player.itemAnimation <= 1)
                Projectile.active = false;

            //Store the shot angle
            if (timer == 0 && Projectile.owner == Main.myPlayer)
            {
                shotAngle = (Main.MouseWorld - Player.Center).ToRotation();
                bonusPower = 1f;

                muzzleFlashNum = Main.rand.Next(1, 4);

                XOffset = BaseXOffset;
            }

            GunDirection = shotAngle.ToRotationVector2();
            Player.ChangeDir(GunDirection.X > 0 ? 1 : -1);

            #region recoil
            if (timer > 1)
            {
                if (yRecoilReachedDest == false)
                    YRecoil = Math.Clamp(MathHelper.Lerp(YRecoil, yRecoilPower + 0.4f, 0.24f), 0, yRecoilPower);
                else
                    YRecoil = Math.Clamp(MathHelper.Lerp(YRecoil, -0.2f, 0.06f), 0, yRecoilPower);

                if (YRecoil == yRecoilPower)
                    yRecoilReachedDest = true;

            }

            if (timer == 0)
            {
                XOffset = GoalXOffset;
            }
            XOffset = Math.Clamp(MathHelper.Lerp(XOffset, BaseXOffset, 0.07f), 0, BaseXOffset);

            #endregion

            if (timer > timeToStartFade) 
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

        float XOffset = 0f;
        float YRecoil = 0f;

        float bonusPower = 0f;
        float muzzleFlashPower = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Texture = TextureAssets.Item[gunID].Value;

            String path = "Assets/MuzzleFlashes/Sprite/MiddleMuzzleFlash" + muzzleFlashNum;

            Texture2D MuzzleFlash = Mod.Assets.Request<Texture2D>(path).Value;
            Texture2D MuzzleFlashGlow = Mod.Assets.Request<Texture2D>(path + "Glow").Value;

            Player Player = Main.player[Projectile.owner];
            SpriteEffects mySE = Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Vector2 heldOffset = new Vector2(HoldoutOffset.X, HoldoutOffset.Y * Player.direction).RotatedBy(Projectile.rotation);         
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Player.gfxOffY) + heldOffset;

            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);
            Color[] colors = { between, Color.OrangeRed, Color.Orange, Color.White };

            if (gunID == ItemID.VenusMagnum)
            {
                colors[0] = Color.ForestGreen;
                colors[1] = Color.LawnGreen;
                colors[2] = Color.Green;
                colors[3] = Color.Lerp(Color.LightGreen, Color.White, 0.9f);

                MuzzleFlash = Mod.Assets.Request<Texture2D>(path + "Green").Value;
            }
            else if (gunID == ItemID.SDMG)
            {
                colors[0] = Color.Aqua * 0.5f;
                colors[1] = Color.Aqua;
                colors[2] = Color.Aqua;
                colors[3] = Color.White;

                MuzzleFlash = Mod.Assets.Request<Texture2D>(path + "Blue").Value;
            }

            //Muzzle Flash
            Vector2 muzzleFlashPos = drawPos + new Vector2(TipPosition.X, TipPosition.Y * Player.direction).RotatedBy(Projectile.rotation); //33 -3
            Vector2 muzzleFlashOrigin = new Vector2(MuzzleFlash.Width / 2f, MuzzleFlash.Height / 2f);

            float easedMuzzleFlashAlpha = Easings.easeInSine(muzzleFlashPower);
            float muzzleFlashScale = Projectile.scale * 2f * Easings.easeOutSine(muzzleFlashPower);


            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos + Main.rand.NextVector2Circular(3f, 3f), null, colors[0] with { A = 0 } * easedMuzzleFlashAlpha * 0.75f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);

            Main.spriteBatch.Draw(MuzzleFlash, muzzleFlashPos, null, colors[3] * easedMuzzleFlashAlpha * 1f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);

            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos, null, colors[0] with { A = 0 } * (1f * bonusPower), Projectile.rotation, muzzleFlashOrigin, 3f * (1f - bonusPower), mySE, 0f);



            //Star on tip of gun
            Texture2D Flash = (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/CrispStarPMA");

            Vector2 starPos = drawPos + new Vector2(StarPosition.X, StarPosition.Y * Player.direction).RotatedBy(Projectile.rotation);

            float starRot = (float)Main.timeForVisualEffects * 0.15f * Player.direction;

            float starAlpha = 0.65f * Easings.easeInSine(bonusPower);

            Main.spriteBatch.Draw(Flash, starPos, null, colors[1] with { A = 0 } * starAlpha, starRot, Flash.Size() / 2, 0.4f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flash, starPos, null, colors[2] with { A = 0 } * starAlpha, starRot, Flash.Size() / 2, 0.3f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flash, starPos, null, Color.White with { A = 0 } * starAlpha, starRot, Flash.Size() / 2, 0.2f, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Texture, drawPos, null, lightColor, Projectile.rotation, Texture.Size() / 2, Projectile.scale, mySE, 0f);

            return false;
        }

    }
}
