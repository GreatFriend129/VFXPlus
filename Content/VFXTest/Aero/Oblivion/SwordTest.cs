/*
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using static Terraria.NPC;

namespace VFXPlus.Content.VFXTest.Aero.Oblivion
{
    public class Oblivion : ModItem
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        bool tick = false;
        public override void SetDefaults()
        {
            Item.damage = 113;

            Item.width = 60;
            Item.height = 68;
            Item.useAnimation = 18;
            Item.useTime = 18;

            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.shoot = ModContent.ProjectileType<OblivionHeldProjectile>();

            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
        }

        public override bool AltFunctionUse(Player player) => true;


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, (tick ? 1 : 0));

            tick = !tick;

            return false;
        }
    }
    //Basic Swing
    public class OblivionHeldProjectile : BaseSwingSwordProj
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.timeLeft = 10000;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = Projectile.height = 70;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.extraUpdates = 3;
        }

        public override bool? CanDamage()
        {
            bool shouldDamage = (getProgress(easingProgress) >= 0.3f && getProgress(easingProgress) <= 0.75f) && justHitTime <= -1;
            return shouldDamage;
        }

        bool hasHitEnemy = false;
        bool playedSound = false;
        public override void AI()
        {
            SwingHalfAngle = 185;
            easingAdditionAmount = 0.009f; //0.011
            offset = 55;
            frameToStartSwing = 3 * 3;
            timeAfterEnd = 5 * 3;

            StandardHeldProjCode();
            StandardSwingUpdate();

            if (getProgress(easingProgress) >= 0.3f && !playedSound)
            {
                //SoundStyle styleaa = new SoundStyle("AerovelenceMod/Sounds/Effects/Tech/ShittySword2") with { Volume = 0.45f, Pitch = 0f, PitchVariance = 0.15f };
                //SoundEngine.PlaySound(styleaa, Projectile.Center);

                playedSound = true;
            }

            if (timer % 1 == 0 && justHitTime <= 0 && getProgress(easingProgress) > 0.1f)
            {
                previousRotations.Add(Projectile.rotation);

                if (previousRotations.Count > 17)
                    previousRotations.RemoveAt(0);
            }

            //Dust
            int dustMod = 5;
            if (timer % dustMod == 0 && (getProgress(easingProgress) >= 0.2f && getProgress(easingProgress) <= 0.8f) && justHitTime <= 0)
            {
                Dust d = Dust.NewDustPerfect(Main.player[Projectile.owner].Center + currentAngle.ToRotationVector2() * Main.rand.NextFloat(50f, 100f), ModContent.DustType<PixelGlowOrb>(),
                    currentAngle.ToRotationVector2().RotatedByRandom(0.3f).RotatedBy(MathHelper.PiOver2 * (Projectile.ai[0] > 0 ? 1 : -1)) * -Main.rand.NextFloat(2f, 5f),
                    0, newColor: Color.DeepPink, Main.rand.NextFloat(0.45f, 0.65f));
                d.scale *= Projectile.scale;

                d.noLight = false;
                d.customData = DustBehaviorUtil.AssignBehavior_PGOBase(postSlowPower: 0.9f, velToBeginShrink: 2.5f, fadePower: 0.9f);
            }

            float colVal = 0.65f + (float)Math.Sin(getProgress(easingProgress) * Math.PI) * 0.25f;
            Lighting.AddLight(Projectile.Center, Color.DeepPink.ToVector3() * colVal);

            justHitTime--;
        }

        public List<float> previousRotations = new List<float>();

        public override bool PreDraw(ref Color lightColor)
        {

            SlashDraw(false);


            return false;
        }

        public void SlashDraw(bool giveUp)
        {

            float progBoost = (float)Math.Sin(getProgress(easingProgress) * Math.PI);

            float scaleBoost = ((float)Math.Sin(getProgress(easingProgress) * Math.PI) * 0.3f);

            #region slash
            Texture2D Slash = Mod.Assets.Request<Texture2D>("Assets/Slash/PaintSwing2").Value;

            Vector2 SlashPos = Main.player[Projectile.owner].Center - Main.screenPosition + new Vector2(20f * (float)Math.Sin(MathHelper.Pi * getProgress(easingProgress)), 0).RotatedBy(originalAngle);

            float slashScale = 0.65f + ((float)Math.Sin(getProgress(easingProgress) * Math.PI) * 1.1f);

            Color betweenPink = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);
            Color slashColor = Color.Lerp(Color.Black * 0.3f, betweenPink, Easings.easeInOutCirc(progBoost));

            Main.spriteBatch.Draw(Slash, SlashPos, null, slashColor with { A = 0 } * progBoost, originalAngle + MathHelper.PiOver2, Slash.Size() / 2, slashScale * 0.25f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Slash, SlashPos, null, Color.HotPink with { A = 0 } * progBoost * 0.15f, originalAngle + MathHelper.PiOver2, Slash.Size() / 2, slashScale * 0.25f, SpriteEffects.None, 0f);


            //Main.spriteBatch.Draw(Slash2, SlashPos + new Vector2(200f, 0f), null, slashColor with { A = 0 } * progBoost, originalAngle + MathHelper.PiOver2, Slash.Size() / 2, slashScale * 0.25f, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Slash2, SlashPos + new Vector2(200f, 0f), null, Color.HotPink with { A = 0 } * progBoost * 0.15f, originalAngle + MathHelper.PiOver2, Slash.Size() / 2, slashScale * 0.25f, SpriteEffects.None, 0f);
            #endregion


        }

        public override void OnHitNPC(NPC target, HitInfo hit, int damageDone)
        {

        }


        // Find the start and end of the sword and use a line collider to check for collision with enemies
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Main.player[Projectile.owner].MountedCenter;
            Vector2 end = start + currentAngle.ToRotationVector2() * ((Projectile.Size.Length() * 1.25f) * Projectile.scale); //1.2f
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint); //15f
        }

        //Sword easing
        public override float getProgress(float x)
        {
            float toReturn = 0f;

            #region easeExpo

            //pre 0.5
            if (x <= 0.5f)
            {
                toReturn = (float)(Math.Pow(2, (16 * x) - 8)) / 2;
            }
            else if (x > 0.5)
            {
                toReturn = (float)(2 - ((Math.Pow(2, (-16 * x) + 8)))) / 2;
            }

            //post 0.5
            if (x == 0)
                toReturn = 0;
            if (x == 1)
                toReturn = 1;

            return toReturn;


            #endregion;
        }
    }
}
*/