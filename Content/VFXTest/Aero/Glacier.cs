using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using VFXPlus.Content.Dusts;
using VFXPlus.Common.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using Terraria.DataStructures;
using static Terraria.NPC;
using ReLogic.Content;
using VFXPlus.Content.Projectiles;
using VFXPlus.Common.Interfaces;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace VFXPlus.Content.VFXTest.Aero
{

    public class Glacier : ModItem
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = Item.height = 44;

            Item.useTime = Item.useAnimation = 24;

            Item.UseSound = SoundID.Item1;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<GlacierProj>();
            Item.UseSound = SoundID.Item1;
            Item.shootSpeed = 34f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            return true;
        }

        public override bool OnPickup(Player player) //Another check for the problem mentioned above
        {
            return base.OnPickup(player);
        }
    }

    public class GlacierProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        Vector2 initialVelocityDir = Vector2.Zero;
        float initialVelocityLength = 0f;

        bool returnToPlayer = false;
        int timer = 0;
        public override void AI()
        {
            int trailCount = 22;
            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center + Projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            previousRotations.Add(Projectile.rotation);
            previousPositions.Add(Projectile.Center + Projectile.velocity * 1.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer == 0)
            {
                initialVelocityDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                initialVelocityLength = Projectile.velocity.Length();

                Projectile.rotation = Main.rand.NextFloat(6.28f);
            }

            int timeToReachMiddle = 20;
            if (timer < timeToReachMiddle)
            {
                float progress = (float)timer / (float)timeToReachMiddle;
                float easedProgress = Easings.easeInOutHarsh(progress);

                Projectile.velocity = Vector2.Lerp(initialVelocityDir * initialVelocityLength, Vector2.Zero, easedProgress);
            }
            else
            {
                int timeToReturn = 30;

                float progress = (float)(timer - timeToReachMiddle) / (float)timeToReturn;
                float easedProgress = Easings.easeOutQuad(progress);

                Vector2 velDir = (Main.player[Projectile.owner].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                Projectile.velocity = Vector2.Lerp(Vector2.Zero, velDir * initialVelocityLength, easedProgress);

                float distToPlayer = (Main.player[Projectile.owner].Center - Projectile.Center).Length();
                if (distToPlayer < 20)
                {
                    Projectile.active = false;
                }
            }

            for (int i = 0; i < 1; i++)
            {
                int num622 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.SpookyWood, 0f, 0f, 0, default, 1.75f);
                Main.dust[num622].noGravity = true;
                Main.dust[num622].scale = 1f;
            }

            if (timer % 2 == 0 && Main.rand.NextBool(2) && timer != 0)
            {
                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = Projectile.Center,
                    MovementVector = Main.rand.NextVector2Circular(0.5f, 0.5f) + Projectile.velocity * 0.15f
                };

                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.BlackLightningSmall, particleSettings);
            }



            if (initialVelocityDir.X > 0)
                Projectile.rotation += 0.56f;
            else
                Projectile.rotation -= 0.56f;

            float fadeInTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f);
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            timer++;
        }

        int initialDir = 1;

        float overallAlpha = 1f;
        float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D MainTex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/DeadmanScythe2").Value;
            Texture2D GlowTex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/DeadmanScythe2Glow").Value;
            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = MainTex.Size() / 2f;
            SpriteEffects SE = Projectile.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.25f, Projectile.rotation, Orb.Size() / 2f, 0.8f * Projectile.scale * overallScale, SE);
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Black * 0.3f, Projectile.rotation, Orb.Size() / 2f, 0.4f * Projectile.scale * overallScale, SE);


            //Swirl
            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingB").Value; //A |0.75f time|

            float ringRot = (float)Main.timeForVisualEffects * 0.6f;
            float ringRot2 = (float)Main.timeForVisualEffects * 0.6f;

            float ringScale = 0.15f * Projectile.scale * overallScale;
            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.MediumPurple with { A = 0 } * 1f, ringRot, RingTex.Size() / 2f, ringScale, SE);
            Main.EntitySpriteDraw(RingTex, drawPos, null, new Color(150, 50, 200) with { A = 0 } * 1f, ringRot + MathHelper.PiOver4, RingTex.Size() / 2f, ringScale * 1.1f, SE);


            //After-Image
            for (int i = 220; i < previousPositions.Count - 1; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.MediumPurple, Color.Black, 1f - Easings.easeOutCubic(progress));

                float size1 = Easings.easeOutQuad(progress) * overallScale * Projectile.scale;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(MainTex, AfterImagePos, null, col with { A = 0 } * progress * 1f,
                    previousRotations[i], TexOrigin, size1 * overallScale, SE);

            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(GlowTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), null,
                    Color.MediumPurple with { A = 0 } * 0.35f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(MainTex, drawPos, null, lightColor, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE);

            //Main.EntitySpriteDraw(GlowTex, drawPos, null, Color.LightSkyBlue with { A = 0 } * 0.1f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, 0);

            return false;
        }

    }

}