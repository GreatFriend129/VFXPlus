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
using Terraria.GameContent;
using System.Threading;
using Terraria.GameContent.Drawing;
using VFXPlus.Common.Drawing;
using Terraria.Graphics;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class SpectreStaff : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.SpectreStaff);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            
            return true;
        }

    }
    public class SpectreStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LostSoulFriendly);
        }

        BaseTrailInfo trail1 = new BaseTrailInfo();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 40;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center + projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            #region trail info
            trail1.trailTexture = ModContent.Request<Texture2D>("VFXPlus/Content/Weapons/Magic/Hardmode/Staves/SpectreAssets/GluttonyGhoul").Value;

            trail1.trailColor = Color.Lerp(Color.LightSkyBlue, Color.White, 0.5f);
            trail1.timesToDraw = 2;

            trail1.pinch = false;
            trail1.pinchAmount = 0.9f;

            trail1.trailPointLimit = 150;
            trail1.trailWidth = (int)(12f * projectile.scale);
            trail1.trailMaxLength = 175;

            trail1.trailRot = projectile.velocity.ToRotation();
            trail1.trailPos = projectile.Center + projectile.velocity;
            trail1.TrailLogic();
            #endregion


            if (timer % 1 == 0)
            {
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelFast>(), Alpha: 100, newColor: Color.White, Scale: Main.rand.NextFloat(0.25f, 0.4f));

                Vector2 dustVel = (projectile.velocity * Main.rand.NextFloat(0.85f, 1.15f) * -1f).RotateRandom(0.3f);
                d.velocity = dustVel;

                d.fadeIn = 50;
            }

            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.4f, 0.1f), 0f, 1f);

            #region vanillaCode Minus Dust

            float num408 = projectile.Center.X;
            float num409 = projectile.Center.Y;
            float num412 = 400f;
            bool flag14 = false;
            int num413 = 0;
            if (true)
            {
                for (int num414 = 0; num414 < 200; num414++)
                {
                    if (Main.npc[num414].CanBeChasedBy(this) && projectile.Distance(Main.npc[num414].Center) < num412 && Collision.CanHit(projectile.Center, 1, 1, Main.npc[num414].Center, 1, 1))
                    {
                        float num415 = Main.npc[num414].position.X + (float)(Main.npc[num414].width / 2);
                        float num416 = Main.npc[num414].position.Y + (float)(Main.npc[num414].height / 2);
                        float num417 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num415) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num416);
                        if (num417 < num412)
                        {
                            num412 = num417;
                            num408 = num415;
                            num409 = num416;
                            flag14 = true;
                            num413 = num414;
                        }
                    }
                }
            }
            
            if (flag14)
            {
                float num423 = 6f;

                Vector2 vector107 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                float num424 = num408 - vector107.X;
                float num425 = num409 - vector107.Y;
                float num426 = (float)Math.Sqrt(num424 * num424 + num425 * num425);
                float num427 = num426;
                num426 = num423 / num426;
                num424 *= num426;
                num425 *= num426;

                projectile.velocity.X = (projectile.velocity.X * 20f + num424) / 21f;
                projectile.velocity.Y = (projectile.velocity.Y * 20f + num425) / 21f;


            }
            #endregion
            timer++;
            return false;

            return base.PreAI(projectile);
        }


        public float overallAlpha = 0f;
        Effect myEffect = null;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Color StripColor(float progress) => Color.White * 1f;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Staves/SpectreAssets/HolySolidSpectre").Value;
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/FlameTrail").Value;


            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(0f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.White.ToVector3() * 0.15f); 

            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);


            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            //vertexStrip.DrawTrail();

            //Under Layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["ColorOne"].SetValue(Color.White.ToVector3() * 1f);
            myEffect.Parameters["progress"].SetValue(timer * 0.04f);

            myEffect.Parameters["glowThreshold"].SetValue(0.4f);
            myEffect.Parameters["glowIntensity"].SetValue(1.5f);

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //vertexStrip.DrawTrail();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();


            //ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            // {
            //    trail1.timesToDraw = 0;
            //    trail1.TrailDrawing(Main.spriteBatch);
            //});

            //trail1.timesToDraw = 0;
            //trail1.TrailDrawing(Main.spriteBatch);

            return false;
        }

        public float StripWidth(float progress)
        {
            return 15f * overallAlpha;
            
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 100f, Easings.easeInCirc(num)) * 0.4f * Easings.easeInQuad(1f); //* 1.15f * Easings.easeInSine(width); //0.5f; // 0.3f 
        }

        public float StripWidth2(float progress)
        {
            
            float width = 30f;
            float pinchAmount = 0.5f;

            if (progress < 0.5f)
            {
                float lerpValue = Utils.GetLerpValue(0f, pinchAmount, progress, clamped: true);
                float num = 1f - (1f - lerpValue) * (1f - lerpValue);
                return MathHelper.Lerp(0f, width, num) * 1f;
            }
            else if (progress >= 0.5)
            {
                float lerpValue = Utils.GetLerpValue(0f, pinchAmount, 1 - progress, clamped: true);
                float num = 1f - (1f - lerpValue) * (1f - lerpValue);
                return MathHelper.Lerp(0f, width, num) * 1f;
            }

            return 0f;
            
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

    //MySoulIsFullOfVoid
    /*
     * 
     * 
     * 
    */
}
