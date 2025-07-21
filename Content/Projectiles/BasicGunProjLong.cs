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

    public class BasicGunProjLong : ModProjectile
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

        //How long should the recoil take
        public int AnimationTime = 25;

        //The item ID of the weapon we are using
        public int gunID = 1;

        public float BaseXOffset = 18f;
        public float GoalXOffset = 4f;
        public float yRecoilPower = 0.2f;

        public Vector2 HoldoutOffset = Vector2.Zero; // 0 1
        public Vector2 TipPosition = Vector2.Zero; // 33 -3
        public Vector2 StarPosition = Vector2.Zero; // 30 -3

        public void SetProjInfo(int GunID, int AnimTime, float NormalXOffset, float DestXOffset, float YRecoilAmount,
            Vector2 HoldOffset, Vector2 TipPos, Vector2 StarPos)
        {
            gunID = GunID;
            AnimationTime = AnimTime;
            BaseXOffset = NormalXOffset;
            GoalXOffset = DestXOffset;
            yRecoilPower = YRecoilAmount;
            HoldoutOffset = HoldOffset;
            TipPosition = TipPos;
            StarPosition = StarPos;
        }


        //The angle of the gun shot
        float shotAngle = 0f;

        int timer = 0;
        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];
            ProjectileExtensions.KillHeldProjIfPlayerDeadOrStunned(Projectile);

            Projectile.velocity = Vector2.Zero;

            //Kill proj if player is done with item use
            if (Player.itemTime <= 1)
                Projectile.active = false;

            //Or if we have reached the end of the animation for a long time
            if (timer == AnimationTime * 2f)
                Projectile.active = false;

            //Store the shot angle
            if (timer == 0 && Projectile.owner == Main.myPlayer)
            {
                shotAngle = (Main.MouseWorld - Player.Center).ToRotation();
                bonusPower = 1f;

                XOffset = BaseXOffset;
            }

            GunDirection = shotAngle.ToRotationVector2();
            Player.ChangeDir(GunDirection.X > 0 ? 1 : -1);

            #region XRecoil
            int XAnimTime = AnimationTime;
            float goalX = GoalXOffset;
            float baseX = BaseXOffset; //The Normal XOffset of the gun

            //Should add up to 1 (but does not need to)
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
            #endregion

            #region YRecoil
            int timeToStartYAnim = 3;
            int YAnimTime = AnimationTime;
            float goalY = yRecoilPower; //The amount of recoil (radians) of the shot
            float baseY = 0f;

            //Should add up to 1 (but does not need to)
            Vector2 animRatioY = new Vector2(0.25f, 0.75f);

            if (timer >= timeToStartYAnim)
            {
                float yAnimProgress = (float)(Math.Clamp((timer - timeToStartYAnim), 0f, YAnimTime) / YAnimTime);

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
            }
            #endregion

            if (timer > 3)
                muzzleFlashPower = Math.Clamp(MathHelper.Lerp(muzzleFlashPower, -0.5f, 0.14f), 0f, 1f);

            //StandardHeldProjCode
            GunDirection = shotAngle.ToRotationVector2().RotatedBy(YRecoil * Player.direction * -1f); ;
            Projectile.Center = Player.MountedCenter + (GunDirection * XOffset);
            Projectile.velocity = Vector2.Zero;
            Player.itemRotation = shotAngle;

            if (Player.direction != 1)
                Player.itemRotation -= 3.14f;

            Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);

            float Xprog = Utils.GetLerpValue(goalX, baseX, XOffset, true);

            if (Xprog > 0.75f || true)
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, GunDirection.ToRotation() - MathHelper.PiOver2);
            else if (Xprog > 0.5f)
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, GunDirection.ToRotation() - MathHelper.PiOver2);
            else if (Xprog > 0.25f)
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, GunDirection.ToRotation() - MathHelper.PiOver2);
            else
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, GunDirection.ToRotation() - MathHelper.PiOver2);

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

            Texture2D MuzzleFlash = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/Sprite/SmallPixelMuzzleFlash").Value;
            Texture2D MuzzleFlashGlow = Mod.Assets.Request<Texture2D>("Assets/MuzzleFlashes/Sprite/SmallPixelMuzzleFlashGlow").Value;


            Player Player = Main.player[Projectile.owner];
            SpriteEffects mySE = Player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            Vector2 heldOffset = new Vector2(HoldoutOffset.X, HoldoutOffset.Y * Player.direction).RotatedBy(Projectile.rotation);
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Player.gfxOffY) + heldOffset;

            //float gunScale = Projectile.scale;
            //if (gunID == ItemID.Uzi)
            //    gunScale *= 0.85f;

            Main.spriteBatch.Draw(Texture, drawPos, null, lightColor, Projectile.rotation, Texture.Size() / 2, Projectile.scale, mySE, 0f);


            Color between = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);

            Color[] colors = { between, Color.OrangeRed, Color.Orange };

            if (gunID == ItemID.RedRyder || gunID == ItemID.TheUndertaker)
            {
                colors[0] = Color.Lerp(Color.OrangeRed, Color.Red, 0.5f);
                colors[1] = Color.Red;
                colors[2] = Color.OrangeRed;
            }
            else if (gunID == ItemID.Uzi)
            {
                colors[0] = Color.Lerp(Color.Orange, Color.OrangeRed, 0.25f);
                colors[1] = Color.Orange;
                colors[2] = Color.Yellow;
            }

            //Muzzle Flash
            Vector2 muzzleFlashPos = drawPos + new Vector2(TipPosition.X, TipPosition.Y * Player.direction).RotatedBy(Projectile.rotation); //33 -3
            Vector2 muzzleFlashOrigin = new Vector2(MuzzleFlash.Width / 2f, MuzzleFlash.Height / 2f);

            float easedMuzzleFlashAlpha = Easings.easeInSine(muzzleFlashPower);
            float muzzleFlashScale = Projectile.scale * 0.75f;


            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos + Main.rand.NextVector2Circular(3f, 3f), null, colors[0] with { A = 0 } * easedMuzzleFlashAlpha * 0.75f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);

            Main.spriteBatch.Draw(MuzzleFlash, muzzleFlashPos, null, Color.White * easedMuzzleFlashAlpha * 1f, Projectile.rotation, muzzleFlashOrigin, muzzleFlashScale, mySE, 0f);

            Main.spriteBatch.Draw(MuzzleFlashGlow, muzzleFlashPos, null, colors[0] with { A = 0 } * (1f * bonusPower), Projectile.rotation, muzzleFlashOrigin, 1.5f * (1f - bonusPower) * Projectile.scale, mySE, 0f);


            //Star on tip of gun
            Texture2D Flash = (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/CrispStarPMA");

            Vector2 starPos = drawPos + new Vector2(StarPosition.X, StarPosition.Y * Player.direction).RotatedBy(Projectile.rotation);

            //starPos += HoldoutOffset.RotatedBy(Projectile.rotation);

            float starRot = (float)Main.timeForVisualEffects * 0.15f * Player.direction;

            Main.spriteBatch.Draw(Flash, starPos, null, colors[1] with { A = 0 } * 0.75f * bonusPower, starRot, Flash.Size() / 2, 0.4f * Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flash, starPos, null, colors[2] with { A = 0 } * 0.75f * bonusPower, starRot, Flash.Size() / 2, 0.3f * Projectile.scale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flash, starPos, null, Color.White with { A = 0 } * 0.75f * bonusPower, starRot, Flash.Size() / 2, 0.2f * Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

    }
}
