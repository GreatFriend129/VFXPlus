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
using static System.Runtime.InteropServices.JavaScript.JSType;
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
            //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = .12f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
            //SoundEngine.PlaySound(style, player.Center);

            //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Research_1") with { Pitch = .85f, PitchVariance = .2f, Volume = 0.35f };
            //SoundEngine.PlaySound(style2, player.Center);

            //SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/laser_fire") with { Volume = 0.1f, Pitch = -.33f, PitchVariance = 0.1f, };
            //SoundEngine.PlaySound(style);

            //Dust
            for (int i = 10; i < 6 + Main.rand.Next(0, 4); i++) //2 //0,3
            {
                Dust dp = Dust.NewDustPerfect(position + velocity * 2, ModContent.DustType<LineSpark>(),
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.Next(6, 19),
                    newColor: Color.OrangeRed, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                    1f, 0.25f); //80

            }

            //SoundStyle style = new SoundStyle("Terraria/Sounds/Item_108") with { Pitch = .78f, PitchVariance = 0.1f, Volume = 0.3f };
            //SoundEngine.PlaySound(style, player.Center);

            //SoundStyle styla = new SoundStyle("Terraria/Sounds/Item_122") with { Pitch = 1f, Volume = 0.9f, PitchVariance = 0.11f };
            //SoundEngine.PlaySound(styla, player.Center);

            SoundStyle style2 = new SoundStyle("AerovelenceMod/Sounds/Effects/TF2/rescue_ranger_fire") with { Volume = .1f, Pitch = 0.5f, PitchVariance = .1f };
            SoundEngine.PlaySound(style2, player.Center);

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

        int vfxIndex = -1;
        public override bool PreAI(Projectile projectile)
        {
            if (vfxIndex == -1)
            {
                int p = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<HeatRayVFX>(), 0, 0, projectile.owner);
                Main.projectile[p].rotation = projectile.velocity.ToRotation();
                vfxIndex = p;
            }


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

        public Vector2 startPos = Vector2.Zero;
        public Vector2 endPos = Vector2.Zero;
        public override void AI()
        {
            if (timer == 0)
            {
                startPos = Projectile.Center;
                Projectile.velocity = Vector2.Zero;
            }

            true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.05f), 0, 1f); //0.04

            if (timer == 100 || true_width == 0)
                Projectile.active = false;

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            #region endPoints
            Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;

            //Vector2 v2Scale = new Vector2(0.6f, 1f) * true_width;

            float rot = Projectile.rotation;

            float starOuterScale = 0.8f * true_width;
            float starInnerScale = 0.4f * true_width;
            //Start star
            Main.EntitySpriteDraw(portal, startPos - Main.screenPosition, null, Color.Orange with { A = 0 } * true_alpha, rot, portal.Size() / 2, starOuterScale, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, startPos - Main.screenPosition, null, Color.White with { A = 0 } * true_alpha, rot, portal.Size() / 2, starInnerScale, SpriteEffects.None);

            //End star
            Main.EntitySpriteDraw(portal, endPos - Main.screenPosition, null, Color.Orange with { A = 0 } * true_alpha, rot, portal.Size() / 2, starOuterScale, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, endPos - Main.screenPosition, null, Color.White with { A = 0 } * true_alpha, rot, portal.Size() / 2, starInnerScale, SpriteEffects.None);

            #endregion

            //if (myEffect == null)
            //myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/ComboLaserVertex", AssetRequestMode.ImmediateLoad).Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            #region laser
            if (endPos.Equals(Vector2.Zero))
                return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                //Create arrays
                Vector2[] pos_arr = { startPos, endPos };
                float[] rot_arr = { Projectile.rotation, Projectile.rotation };

                Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/Trail5Loop").Value;
                Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value;



                VertexStrip vertexStrip = new VertexStrip();
                vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

                VertexStrip vertexStripBlack = new VertexStrip();
                vertexStripBlack.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


                myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
                myEffect.Parameters["progress"].SetValue(timer * -0.04f);

                //Black Layer
                #region black
                myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
                myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 1f);
                myEffect.Parameters["glowThreshold"].SetValue(1f);

                myEffect.CurrentTechnique.Passes["MainPS"].Apply();

                //vertexStripBlack.DrawTrail();
                #endregion

                //Main layer
                myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
                myEffect.Parameters["ColorOne"].SetValue(Color.DeepSkyBlue.ToVector3() * 2f);

                myEffect.Parameters["glowThreshold"].SetValue(0.5f);
                myEffect.Parameters["glowIntensity"].SetValue(1.3f);


                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                vertexStrip.DrawTrail();
                vertexStrip.DrawTrail();

                //Layer 2
                myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
                myEffect.Parameters["ColorOne"].SetValue(Color.SkyBlue.ToVector3() * 2f);

                myEffect.Parameters["glowThreshold"].SetValue(0.9f);
                myEffect.Parameters["glowIntensity"].SetValue(1.1f);


                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                vertexStripBlack.DrawTrail();
                vertexStripBlack.DrawTrail();

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            });
            #endregion


            return false;
        }

        public void ShaderParams()
        {
            
            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            myEffect.Parameters["satPower"].SetValue(0.45f); //higher power -> less affected by background 

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/ThinGlowLine").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/FlameTrail").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Trail7").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/smokeTrail4_512").Value);

            // orange = 255, 165, 0 
            // orangeRed = 255 69 0 
            Color a = Color.Orange;
            Color b = Color.OrangeRed;
            Color c1 = new Color(255, 96, 30, 255);
            Color c2 = new Color(240, 79, 30, 255);
            Color c3 = new Color(255, 173, 30, 255);
            Color c4 = new Color(255, 169, 30, 255);

            myEffect.Parameters["Color1"].SetValue(a.ToVector4());
            myEffect.Parameters["Color2"].SetValue(c2.ToVector4());
            myEffect.Parameters["Color3"].SetValue(c3.ToVector4());
            myEffect.Parameters["Color4"].SetValue(c4.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(1.75f * 1f); //1.75
            myEffect.Parameters["Color2Mult"].SetValue(1.15f * 1f);
            myEffect.Parameters["Color3Mult"].SetValue(1.25f * 1f); //0.25f
            myEffect.Parameters["Color4Mult"].SetValue(1.75f * 1f); //0.75f
            myEffect.Parameters["totalMult"].SetValue(1f);


            //We want the number of repititions to be relative to the length of the laser
            float dist = (Projectile.Center - endPos).Length();
            float repValue = dist / 1000f;

            myEffect.Parameters["tex1reps"].SetValue(2.5f * repValue);
            myEffect.Parameters["tex2reps"].SetValue(3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(3f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(10f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.04f * 1f);
            
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
            return 50f * true_width; //80f

            //float num = 1f;
            //float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            //num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            //return MathHelper.Lerp(0f, 100f, Easings.easeInCirc(num)) * 0.4f * Easings.easeInQuad(true_width); //* 1.15f * Easings.easeInSine(width); //0.5f; // 0.3f 
        }

        public float StripWidth2(float progress)
        {
            return 180f;
            
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 90f, Easings.easeInCirc(num)) * 0.35f * Easings.easeInQuad(true_width);
        }

    }

}
