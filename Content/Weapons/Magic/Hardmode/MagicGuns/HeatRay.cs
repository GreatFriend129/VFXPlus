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
            return lateInstatiation && (item.type == ItemID.HeatRay);
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPos = position + velocity.SafeNormalize(Vector2.UnitX) * 28f;

            for (int i = 0; i < 7 + Main.rand.Next(0, 2); i++)
            {
                Vector2 randomStart = velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1) * 3f;

                Dust gd = Dust.NewDustPerfect(spawnPos, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.35f) * 1.5f, newColor: new Color(255, 130, 20) * 0.85f,
                    Scale: Main.rand.NextFloat(0.75f, 1.4f) * 1f);
                gd.alpha = 2;
            }


            //Smoke
            for (int i = 0; i < 7 + Main.rand.Next(0, 3); i++)
            {
                Vector2 smvel = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                Dust sm = Dust.NewDustPerfect(spawnPos, ModContent.DustType<HighResSmoke>(), smvel, newColor: new Color(255, 130, 20) * 0.85f, Scale: Main.rand.NextFloat(0.35f, 0.75f));
                sm.customData = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
            }

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/rescue_ranger_fire") with { Volume = .1f, Pitch = 0.55f, PitchVariance = .1f };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle styla = new SoundStyle("Terraria/Sounds/Item_122") with { Volume = 0.15f, Pitch = .9f, PitchVariance = 0.11f, MaxInstances = -1 };
            SoundEngine.PlaySound(styla, player.Center);

            /* very cool
            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/rescue_ranger_fire") with { Volume = .1f, Pitch = 0.5f, PitchVariance = .1f };
            SoundEngine.PlaySound(style2, player.Center);

            SoundEngine.PlaySound(SoundID.Item70 with { Pitch = .9f, Volume = 0.4f, MaxInstances = -1, PitchVariance = 0.25f }, player.Center);

            SoundStyle styla = new SoundStyle("Terraria/Sounds/Item_122") with { Volume = 0.25f, Pitch = .9f, PitchVariance = 0.11f, MaxInstances = -1 };
            SoundEngine.PlaySound(styla, player.Center);
            */

            return true;
        }

    }
    public class HeatRayShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.HeatRay);
        }

        int timer = 0;
        int vfxIndex = -1;
        public override bool PreAI(Projectile projectile)
        {
            if (vfxIndex == -1)
            {
                Vector2 offset = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 25f;
                int p = Projectile.NewProjectile(null, offset, Vector2.Zero, ModContent.ProjectileType<HeatRayVFX>(), 0, 0, projectile.owner);
                Main.projectile[p].rotation = projectile.velocity.ToRotation();
                vfxIndex = p;
            }


            if (timer % 4 == 0 && false)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + projectile.velocity * 1.25f;

                Color ora = new Color(255, 160, 20);

                Dust dp = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<PixelatedLineSpark>(), 
                    vel, newColor: Color.Orange, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 2.45f);

                dp.velocity += projectile.velocity * 2.15f;

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.25f);
            }

            timer++;

            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            //Dust
            for (int i = 0; i < 12 + Main.rand.Next(0, 2); i++) //4 //2,2
            {
                Vector2 vel = new Vector2(-8, 0).RotatedBy(projectile.velocity.ToRotation());

                Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.5f);

                Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<LineSpark>(),
                    vel.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-1.15f, 1.15f)) * Main.rand.Next(7, 18),
                    newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.3f);

                p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.5f);
            }

            for (int fg = 0; fg < 18 + Main.rand.Next(0, 4); fg++)
            {
                Vector2 randomStart = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1) * 3f * -1f;
                Dust gd = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelAlts>(), randomStart * Main.rand.NextFloat(0.3f, 1.35f) * 1.5f, newColor: new Color(255, 130, 20) * 0.85f, 
                    Scale: Main.rand.NextFloat(0.75f, 1.4f) * 0.5f);
                gd.alpha = 2;
            }

            for (int i = 0; i < 3 + Main.rand.Next(3); i++)
            {
                Vector2 v = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(2) * -1f;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 6f), 0,
                    Color.Orange, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;

                //sa.velocity += vec * 2f;
            }

            //Smoke
            for (int i = 0; i < 7 + Main.rand.Next(0, 3); i++)
            {
                Vector2 smvel = Main.rand.NextVector2Circular(1f, 1f) * Main.rand.NextFloat(1f, 3f);
                Dust sm = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<HighResSmoke>(), smvel, newColor: new Color(255, 130, 20) * 1f, Scale: Main.rand.NextFloat(0.35f, 0.75f));
                sm.customData = DustBehaviorUtil.AssignBehavior_HRSBase(frameToStartFade: 5, fadeDuration: 25, velSlowAmount: 1f,
                    overallAlpha: 1f, drawSoftGlowUnder: true, softGlowIntensity: 1f);
            }

            if (vfxIndex != -1)
                (Main.projectile[vfxIndex].ModProjectile as HeatRayVFX).endPos = projectile.Center;

            base.OnKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_40") with { Pitch = -.7f, PitchVariance = .25f, MaxInstances = 1, Volume = 0.35f };
            //SoundEngine.PlaySound(style, projectile.Center);

            return base.OnTileCollide(projectile, oldVelocity);
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
            //Projectile.hide = true; //for draw behind

            Projectile.timeLeft = 1000; //180
        }

        //public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        //{
        //    overPlayers.Add(index);
        //}

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

            if (timer == 100 || true_width <= 0.05f)
                Projectile.active = false;

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            #region endPoints
            Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;
            Texture2D glorb = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            //Vector2 v2Scale = new Vector2(0.6f, 1f) * true_width;
            Color inBetweenOrange = Color.Lerp(Color.Orange, Color.OrangeRed, 0.25f);

            int dir = Projectile.rotation.ToRotationVector2().X >= 0 ? 1 : -1; 
            float rot = Projectile.rotation;

            float starRot = (float)(Main.timeForVisualEffects * 0.05f * dir);
            float starOuterScale = 1.6f * true_width;
            float starInnerScale = 0.8f * true_width;

            //Start star
            //Main.EntitySpriteDraw(glorb, startPos - Main.screenPosition, null, inBetweenOrange with { A = 0 } * true_alpha * 0.5f, rot, glorb.Size() / 2, 0.75f * true_width, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, startPos - Main.screenPosition, null, inBetweenOrange with { A = 0 } * true_alpha * 1.15f, rot, portal.Size() / 2, starOuterScale, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, startPos - Main.screenPosition, null, Color.White with { A = 0 } * true_alpha, rot, portal.Size() / 2, starInnerScale, SpriteEffects.None);

            //End star
            //Main.EntitySpriteDraw(glorb, endPos - Main.screenPosition, null, inBetweenOrange with { A = 0 } * true_alpha, rot, glorb.Size() / 2, 0.75f * true_width, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, endPos - Main.screenPosition, null, inBetweenOrange with { A = 0 } * true_alpha * 1.15f, rot, portal.Size() / 2, starOuterScale, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, endPos - Main.screenPosition, null, Color.White with { A = 0 } * true_alpha, rot, portal.Size() / 2, starInnerScale, SpriteEffects.None);

            #endregion

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/ComboLaserVertex", AssetRequestMode.ImmediateLoad).Value;

            #region laser
            if (endPos.Equals(Vector2.Zero))
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                //Create arrays
                Vector2[] pos_arr = { startPos, endPos };
                float[] rot_arr = { Projectile.rotation, Projectile.rotation };

                VertexStrip vertexStrip = new VertexStrip();
                vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
                NewShaderParams();

                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                vertexStrip.DrawTrail();

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            });
            #endregion


            return false;
        }

        public void ShaderParams()
        {
            
            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/ThinLineGlowClear").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            myEffect.Parameters["satPower"].SetValue(0.6f); //higher power -> less affected by background 

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Extra_196_Black").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/smokeTrail4_512").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/s06sBloom").Value); //smokeTrail4_512

            // orange = 255, 165, 0 
            // orangeRed = 255 69 0 
            Color a = Color.Orange;
            Color b = Color.OrangeRed;
            Color c = new Color(255, 110, 5);
            Color c1 = new Color(255, 96, 30, 255);
            Color c2 = new Color(240, 79, 30, 255);
            Color c3 = new Color(255, 173, 30, 255);
            Color c4 = new Color(255, 169, 30, 255);

            myEffect.Parameters["Color1"].SetValue(c.ToVector4());
            myEffect.Parameters["Color2"].SetValue(Color.White.ToVector4());
            myEffect.Parameters["Color3"].SetValue(c3.ToVector4());
            myEffect.Parameters["Color4"].SetValue(c4.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(1f * 1f); //1.75
            myEffect.Parameters["Color2Mult"].SetValue(0.75f * 1f);
            myEffect.Parameters["Color3Mult"].SetValue(0.5f * 1f); //0.25f
            myEffect.Parameters["Color4Mult"].SetValue(0.75f * 1f); //0.75f
            myEffect.Parameters["totalMult"].SetValue(1.15f);


            //We want the number of repititions to be relative to the length of the laser
            float dist = (Projectile.Center - endPos).Length();
            float repValue = dist / 1000f;

            myEffect.Parameters["tex1reps"].SetValue(2.5f * repValue);
            myEffect.Parameters["tex2reps"].SetValue(2.5f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(2.5f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(2.5f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.02f * 1f);
            
        }

        public void NewShaderParams()
        {

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/ThinLineGlowClear").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            myEffect.Parameters["satPower"].SetValue(0.8f); //higher power -> less affected by background 

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/ThinGlowLine").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/s06sBloom").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/s06sBloom").Value); //smokeTrail4_512

            // orange = 255, 165, 0 
            // orangeRed = 255 69 0 
            Color a = Color.Orange;
            Color b = Color.OrangeRed;
            Color c = new Color(255, 110, 5);
            Color c1 = new Color(255, 96, 30, 255);
            Color c2 = new Color(240, 79, 30, 255);
            Color c3 = new Color(255, 173, 30, 255);
            Color c4 = new Color(255, 169, 30, 255);

            myEffect.Parameters["Color1"].SetValue(a.ToVector4());
            myEffect.Parameters["Color2"].SetValue(c1.ToVector4());
            myEffect.Parameters["Color3"].SetValue(Color.White.ToVector4());
            myEffect.Parameters["Color4"].SetValue(c4.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(1.25f * 1f); //1.75
            myEffect.Parameters["Color2Mult"].SetValue(1.25f * 1f);
            myEffect.Parameters["Color3Mult"].SetValue(0.75f * 1f); //0.25f
            myEffect.Parameters["Color4Mult"].SetValue(1f * 0f); //0.75f
            myEffect.Parameters["totalMult"].SetValue(1.15f);


            //We want the number of repititions to be relative to the length of the laser
            float dist = (Projectile.Center - endPos).Length();
            float repValue = dist / 1000f;

            myEffect.Parameters["tex1reps"].SetValue(2.5f * repValue);
            myEffect.Parameters["tex2reps"].SetValue(2.5f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(2.5f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(2.5f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.03f + startingScroll);

        }


        public Color StripColor(float progress)
        {
            return Color.White;

            float alpha = 1f;
            alpha = 1f - Easings.easeInSine(progress);
            Color color = new Color(0f, 0f, 0f, alpha);
            return color * Easings.easeInSine(true_alpha);
        }
        public float StripWidth(float progress)
        {
            return 80f * true_width; //80f

            //float num = 1f;
            //float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            //num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            //return MathHelper.Lerp(0f, 100f, Easings.easeInCirc(num)) * 0.4f * Easings.easeInQuad(true_width); //* 1.15f * Easings.easeInSine(width); //0.5f; // 0.3f 
        }

        public float StripWidth2(float progress)
        {
            return 40f;
            
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 90f, Easings.easeInCirc(num)) * 0.35f * Easings.easeInQuad(true_width);
        }

    }

}
