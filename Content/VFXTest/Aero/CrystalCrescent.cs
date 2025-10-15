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
using Terraria.GameContent.Drawing;
using Terraria.Physics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace VFXPlus.Content.VFXTest.Aero
{
    /*
    public class CrystalCrescent : ModItem
    {
        
        public override void SetDefaults()
        {
            Item.damage = 26;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 2f;

            Item.useStyle = ItemUseStyleID.Shoot;

            Item.rare = ItemRarityID.Yellow;
            Item.width = 28;
            Item.height = 28;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.shoot = ModContent.ProjectileType<CrystalCrescentProj>();
            Item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class CrystalCrescentProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";


        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 140;
            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override bool? CanDamage()
        {
            return rotSpeed > 0.75f;
        }

        public int timer = 0;
        public override void AI()
        {
            if (timer == 0)
            {
                Projectile.rotation = -MathHelper.PiOver4;

                int a = Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<CrystalCrescentProjVFX>(), 0, 0f, Projectile.owner);
                (Main.projectile[a].ModProjectile as CrystalCrescentProjVFX).parent = Projectile.whoAmI;
            }
            float startingRotation = Projectile.rotation;
            
            ProjectileExtensions.KillHeldProjIfPlayerDeadOrStunned(Projectile);

            Player player = Main.player[Projectile.owner];

            if (!player.channel)
                Projectile.active = false;

            Projectile.Center = player.MountedCenter + new Vector2(0f, 3f) + new Vector2(300f, 0f);
            Projectile.position.X += player.width / 2 * player.direction;
            Projectile.spriteDirection = player.direction;

            Projectile.rotation += (0.3f * rotSpeed) * player.direction;
            if (Projectile.rotation > MathHelper.TwoPi)
                Projectile.rotation -= MathHelper.TwoPi;
            else if (Projectile.rotation < 0)
                Projectile.rotation += MathHelper.TwoPi;

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.itemRotation = Projectile.rotation;

            Projectile.velocity = Vector2.Zero;
            player.itemRotation = player.direction == 1 ? 0 : 0;
            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

            //Trail
            int trailCount = 18;

            float betweenRot = MathHelper.Lerp(startingRotation, Projectile.rotation, 0.5f);
            previousRotations.Add(betweenRot);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            previousRotations.Add(Projectile.rotation);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);


            //Dust
            if (timer % 2 == 0 && rotSpeed >= 0.75f)
            {
                Vector2 vel = (Projectile.rotation + MathHelper.PiOver4 * 2f).ToRotationVector2() * 8f * player.direction;
                Dust da = Dust.NewDustPerfect(Projectile.Center + Projectile.rotation.ToRotationVector2() * 40f + new Vector2(0f, 0f), ModContent.DustType<ElectricSparkGlow>(), vel, newColor: Color.DodgerBlue, Scale: Main.rand.NextFloat(0.4f, 0.6f) * 3f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.9f, FadeScalePower: 0.94f, FadeVelPower: 0.9f, Pixelize: true, XScale: 1f, YScale: 0.45f, WhiteLayerPower: 0.5f);
                esb.randomVelRotatePower = 0.5f;
                da.customData = esb;
            }

            float fadeInTime = Math.Clamp((timer + 5f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1f) * 1.15f;

            float speedInTime = Math.Clamp(timer / 55f, 0f, 1f);
            rotSpeed = Easings.easeInOutCubic(speedInTime);

            timer++;
        }

        public float rotSpeed = 0f;
        float overallAlpha = 1f;
        public float overallScale = 1f;
        public List<float> previousRotations = new List<float>();
        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D MainTex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/CrystalCrescentProj").Value;
            Texture2D GlowTexFull = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/CrystalCrescentGlowFull").Value;
            Texture2D Glowmask = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/CrystalCrescentProjGlowmask").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 TexOrigin = MainTex.Size() / 2f;
            SpriteEffects SE = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;


            //After-Image
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                //Start End
                Color col = Color.Lerp(Color.LightSkyBlue * 1f, Color.DeepSkyBlue, 1f - Easings.easeInOutCubic(progress));

                Main.EntitySpriteDraw(GlowTexFull, drawPos, null, col with { A = 0 } * 0.1f * progress * progress,
                    previousRotations[i], TexOrigin, Projectile.scale * overallScale, SE);
            }

            Main.EntitySpriteDraw(GlowTexFull, drawPos, null, Color.SkyBlue with { A = 0 }, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE);

            Main.EntitySpriteDraw(MainTex, drawPos, null, lightColor, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE);

            Main.EntitySpriteDraw(Glowmask, drawPos, null, Color.White, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE);
            Main.EntitySpriteDraw(Glowmask, drawPos + Main.rand.NextVector2Circular(2f, 2f), null, Color.DeepSkyBlue with { A = 0 } * 0.35f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE);

            return false;
        }

        public override void OnHitNPC(NPC target, HitInfo hit, int damageDone)
        {

            
        }
    }

    //This is in a separate proj so it can draw under the player while also using a shader (resetting spritebatch cause proj to no longer draw under player arm)
    public class CrystalCrescentProjVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override bool? CanDamage() => false;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;

            Projectile.penetrate = -1;

            Projectile.hide = true;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public int parent = -1;
        public override void AI()
        {
            if (Main.projectile[parent].active == false || Main.projectile[parent].type != ModContent.ProjectileType<CrystalCrescentProj>())
                Projectile.active = false;

            Projectile.Center = Main.projectile[parent].Center;

            Projectile.velocity = Vector2.Zero;
        }


        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<AdditivePixelationSystem>().QueueRenderAction("Dusts", () =>
            {
                DrawAura(true);
            });
            DrawAura(false);
            

            return false;
        }

        public void DrawAura(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Orb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle").Value;
            Texture2D OrbPMA = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Projectile proj = Main.projectile[parent];
            Vector2 drawPos = proj.Center - Main.screenPosition;

            #region DrawElectric


            int timer = (proj.ModProjectile as CrystalCrescentProj).timer;
            float overallScale = (proj.ModProjectile as CrystalCrescentProj).overallScale;
            float rotSpeed = (proj.ModProjectile as CrystalCrescentProj).rotSpeed;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Radial/NewRadialScroll", AssetRequestMode.ImmediateLoad).Value;


            float orbRot = timer * 0.08f * Main.player[proj.owner].direction * rotSpeed;
            float orbScale = 0.33f * overallScale * proj.scale;

            myEffect.Parameters["causticTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/WaterEnergyNoise").Value);
            myEffect.Parameters["gradientTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/ThunderGrad").Value);
            myEffect.Parameters["distortTexture"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Noise/noise").Value);
            myEffect.Parameters["flowSpeed"].SetValue(1f);
            myEffect.Parameters["distortStrength"].SetValue(0.1f); //0.1
            myEffect.Parameters["uTime"].SetValue(timer * 0.015f);

            myEffect.Parameters["vignetteSize"].SetValue(0.2f);
            myEffect.Parameters["vignetteBlend"].SetValue(1f);
            myEffect.Parameters["colorIntensity"].SetValue(0.75f * Easings.easeInCirc(rotSpeed));

            Main.spriteBatch.Draw(OrbPMA, drawPos, null, Color.DeepSkyBlue with { A = 0 } * 0.25f * rotSpeed, orbRot, OrbPMA.Size() / 2, orbScale * 3f, SpriteEffects.None, 0f);
            //Main.spriteBatch.Draw(Border, drawPos, null, Color.DeepSkyBlue with { A = 0 } * 0.25f, orbRot, Border.Size() / 2, orbScale * 0.65f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, default, myEffect, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(Orb, drawPos, null, Color.White, orbRot, Orb.Size() / 2, orbScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Orb, drawPos, null, Color.White, orbRot, Orb.Size() / 2, orbScale, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.AlphaBlend, Main.DefaultSamplerState, default, default, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            #endregion
        }
    }
    */
}