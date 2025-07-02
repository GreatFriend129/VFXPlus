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
using Terraria.Graphics;
using VFXPlus.Content.Weapons.Magic.Hardmode.Misc;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class HeatRay : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.HeatRay) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.HeatRayToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = position + velocity.SafeNormalize(Vector2.UnitX) * 28f;

            //Smoke
            Color colBetween = Color.Lerp(Color.OrangeRed, Color.Orange, 0.45f);
            for (int i = 0; i < 15; i++)
            {
                float prog = (float)i / 15f;

                Color col = Color.Lerp(colBetween, Color.Black, 1f - prog);

                Dust d = Dust.NewDustPerfect(position, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.25f, 1.5f) * 1f,
                    newColor: col with { A = 0 } * prog, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.75f); //GlowPixelAlts looks interesting too
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18) + 5, 0.92f, 0.01f, 0.12f); //12 28

                d.velocity += velocity * 0.45f * (prog);
            }


            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/rescue_ranger_fire") with { Volume = .1f, Pitch = 0.55f, PitchVariance = .1f };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle styla = new SoundStyle("Terraria/Sounds/Item_122") with { Volume = 0.15f, Pitch = .9f, PitchVariance = 0.11f, MaxInstances = -1 };
            SoundEngine.PlaySound(styla, player.Center);

            return true;
        }

        public override Vector2? HoldoutOffset(int type)
        {
            return new Vector2(0f, 1f);
            return base.HoldoutOffset(type);
        }

    }
    public class HeatRayShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.HeatRay) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.HeatRayToggle;
        }

        int timer = 0;
        int vfxIndex = -1;
        public override bool PreAI(Projectile projectile)
        {
            if (vfxIndex == -1)
            {
                Vector2 offset = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 29f; //25
                int p = Projectile.NewProjectile(null, offset, Vector2.Zero, ModContent.ProjectileType<HeatRayVFX>(), 0, 0, projectile.owner);
                Main.projectile[p].rotation = projectile.velocity.ToRotation();
                vfxIndex = p;
            }


            if (timer % 3 == 0 && Main.rand.NextBool())
            {
                Vector2 pos = new Vector2(0f, Main.rand.NextFloat(-12f, 12f)).RotatedBy(projectile.velocity.ToRotation());

                Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(6f, 14f);

                Color ora = new Color(255, 160, 20);

                Dust dp = Dust.NewDustPerfect(projectile.Center + pos, ModContent.DustType<LineSpark>(), 
                    vel, newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.45f, 0.8f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    0.5f, 0.25f);
            }

            timer++;

            return false;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (vfxIndex != -1)
                (Main.projectile[vfxIndex].ModProjectile as HeatRayVFX).endPos = projectile.Center;
            
            //Dust
            for (int i = 0; i < 4 + Main.rand.Next(0, 2); i++)
            {
                Vector2 vel = new Vector2(-8, 0).RotatedBy(projectile.velocity.ToRotation());

                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LineSpark>(),
                    vel.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-1.15f, 1.15f)) * Main.rand.Next(7, 18),
                    newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.3f);

                p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f);
            }

            for (int fg = 0; fg < 3 + Main.rand.Next(0, 4); fg++)
            {
                Vector2 randomStart = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1) * 3f * -1f;
                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.35f) * 1.5f, newColor: new Color(255, 130, 20) * 0.85f, 
                    Scale: Main.rand.NextFloat(0.75f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 3 + Main.rand.Next(2); i++)
            {
                Vector2 v = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(2) * -1f;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 6f), 0,
                    Color.Orange, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            //Smoke
            Color colBetween = Color.Lerp(Color.OrangeRed, Color.Orange, 0.35f);
            for (int i = 0; i < 15; i++)
            {
                float prog = (float)i / 15f;

                Color col = Color.Lerp(colBetween, Color.Black, 1f - prog);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.35f, 2.5f) * 1f,
                    newColor: col with { A = 0 } * prog, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.75f); //GlowPixelAlts looks interesting too
                d.customData = new MediumSmokeBehavior(Main.rand.Next(4, 18) + 5, 0.92f, 0.01f, 0.15f); //12 28

                //d.velocity += projectile.oldVelocity * -0.5f * (prog);
            }


            if (vfxIndex != -1)
                (Main.projectile[vfxIndex].ModProjectile as HeatRayVFX).endPos = projectile.Center;

            base.OnKill(projectile, timeLeft);
        }
    }


    // Creating a separete proj for VFX so it can last longer than the heat ray proj itself.
    // The heat ray proj dies immediately cuz its a dust based laser that does like 200 extra updates on frame 1 then dies
    public class HeatRayVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 1000; //180
        }


        int timer = 0;
        float true_width = 1f;
        float true_alpha = 1f;

        float startingScroll = 0;

        public Vector2 startPos = Vector2.Zero;
        public Vector2 endPos = Vector2.Zero;
        public override void AI()
        {
            if (timer == 0)
            {
                startingScroll = Main.rand.NextFloat(0f, 1000f);
                startPos = Projectile.Center;
                Projectile.velocity = Vector2.Zero;

            }

            true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.05f), 0, 1f); //0.04

            float timeForWidthOut = 19;
            float animProgress = Math.Clamp((float)timer / timeForWidthOut, 0f, 1f);

            true_width = MathHelper.Lerp(1f, 0f, Easings.easeOutSine(animProgress));

            if (timer == 100 || true_width <= 0.07f)
                Projectile.active = false;

            if (timer != 0)
            {
                DelegateMethods.v3_1 = Color.Orange.ToVector3() * 0.7f * Projectile.scale * Easings.easeOutCirc(true_width);
                Utils.PlotTileLine(startPos, endPos, 15f * Easings.easeOutCirc(true_width), DelegateMethods.CastLight);
            }


            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            #region laser
            if (endPos.Equals(Vector2.Zero))
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawStar();
                DrawTrail();
            });
            #endregion

            return false;
        }

        public void DrawTrail()
        {
            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/EnergyTex").Value; //|spark_06 | Extra_196_Black
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value;

            Vector2[] pos_arr = { startPos, endPos };
            float[] rot_arr = { Projectile.rotation, Projectile.rotation };


            Color StripColor(float progress) => Color.White * true_alpha;
            float StripWidth(float progress) => 18f * true_width;
            float StripWidth2(float progress) => 45f * true_width;

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            float dist = (startPos - endPos).Length();
            float repValue = dist / 600f;
            myEffect.Parameters["reps"].SetValue(repValue);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.03f); //timer * 0.02
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);

            //UnderLayer
            Color underCol = Color.Lerp(Color.OrangeRed, Color.Orange, 0.4f);
            myEffect.Parameters["ColorOne"].SetValue(underCol.ToVector3() * 3f * true_alpha);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.White.ToVector3() * 5f);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f); //0.6
            myEffect.Parameters["glowIntensity"].SetValue(2.2f); //2.25
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }

        public void DrawStar()
        {
            #region endPoints
            Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;
            Texture2D glorb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            Color inBetweenOrange = Color.Lerp(Color.Orange, Color.OrangeRed, 0.75f);

            int dir = Projectile.rotation.ToRotationVector2().X >= 0 ? 1 : -1;
            float rot = Projectile.rotation;

            float starRot = (float)(Main.timeForVisualEffects * 0.15f * dir);
            float starOuterScale = 1.6f * true_width * 0.35f;
            float starInnerScale = 0.8f * true_width;

            //Start star
            Vector2 startStarPos = startPos - Main.screenPosition;
            Main.EntitySpriteDraw(portal, startStarPos + Main.rand.NextVector2Circular(1f, 1f), null, inBetweenOrange with { A = 0 } * true_alpha * 1.15f, rot, portal.Size() / 2, starOuterScale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, startStarPos + Main.rand.NextVector2Circular(2f, 2f), null, inBetweenOrange with { A = 0 } * true_alpha, rot, portal.Size() / 2, starOuterScale * 1.75f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, startStarPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.White with { A = 0 } * true_alpha, rot, portal.Size() / 2, starOuterScale * 1.15f, SpriteEffects.None);


            //End star
            Vector2 endStarPos = endPos - Main.screenPosition;
            Main.EntitySpriteDraw(portal, endStarPos + Main.rand.NextVector2Circular(1f, 1f), null, inBetweenOrange with { A = 0 } * true_alpha * 1.15f, rot, portal.Size() / 2, starOuterScale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, endStarPos + Main.rand.NextVector2Circular(2f, 2f), null, inBetweenOrange with { A = 0 } * true_alpha, rot, portal.Size() / 2, starOuterScale * 1.75f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, endStarPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.White with { A = 0 } * true_alpha, rot, portal.Size() / 2, starOuterScale * 1.15f, SpriteEffects.None);


            #endregion
        }

    }

}
