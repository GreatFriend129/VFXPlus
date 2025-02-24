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
            int trailCount = 30; //40
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            if (timer % 2 == 0 && timer > 20)
            {
                Color col = Main.rand.NextBool() ? Color.White : Color.SkyBlue;

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelFast>(), Alpha: 100, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
                d.position -= projectile.velocity;

                Vector2 dustVel = (projectile.velocity * Main.rand.NextFloat(0.85f, 1.15f) * -1f).RotateRandom(0.3f);
                d.velocity = dustVel;

                d.fadeIn = 50;
            }

            float timeForPopInAnim = 50;
            float animProgress = Math.Clamp((timer + 5) / timeForPopInAnim, 0f, 1f);

            //overallAlpha = 0.2f + MathHelper.Lerp(0f, 0.8f, Easings.easeInOutBack(animProgress, 1f, 1f));

            starPower = Math.Clamp(MathHelper.Lerp(starPower, 1.25f, 0.02f), 0f, 1f);
            overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.03f), 0f, 1f);

            Lighting.AddLight(projectile.Center, Color.LightSkyBlue.ToVector3() * 0.6f);


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

        }

        float starPower = 0f;
        public float overallAlpha = 0f;
        Effect myEffect = null;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Drawing(Main.spriteBatch, projectile, true);
            });

            Drawing(Main.spriteBatch, projectile, false);

            return false;
        }

        public void Drawing(SpriteBatch sb, Projectile projectile, bool returnImmediately)
        {
            if (returnImmediately)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            //Star
            if (starPower < 1)
            {
                Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;

                float dir = projectile.velocity.X > 0 ? 1 : -1;

                float starRotation = MathHelper.Lerp(0f, MathHelper.Pi * 2f * dir, Easings.easeInOutQuad(starPower)) + projectile.rotation;
                float starScale = Easings.easeOutQuint(1f - starPower) * projectile.scale * 1.25f;

                Vector2 starScaleVec2 = new Vector2(1f, 0.5f) * starScale;


                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScaleVec2, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation, star.Size() / 2f, starScaleVec2 * 0.65f, SpriteEffects.None);

                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScaleVec2, SpriteEffects.None);
                Main.EntitySpriteDraw(star, drawPos, null, Color.SkyBlue with { A = 0 } * starPower, starRotation + MathHelper.PiOver2, star.Size() / 2f, starScaleVec2 * 0.65f, SpriteEffects.None);
            }

            //Orb
            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/PartiGlow").Value;
            Vector2 vec2Scale = new Vector2(1f, 0.45f) * overallAlpha;

            Main.EntitySpriteDraw(orb, drawPos, null, Color.White with { A = 0 } * 0.1f, projectile.velocity.ToRotation(), orb.Size() / 2f, 2f * overallAlpha, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, Color.LightSkyBlue with { A = 0 } * 1f, projectile.velocity.ToRotation(), orb.Size() / 2f, vec2Scale, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, drawPos, null, Color.White with { A = 0 } * 1f, projectile.velocity.ToRotation(), orb.Size() / 2f, vec2Scale * 0.5f, SpriteEffects.None);



            Color StripColor(float progress) => Color.White * 0.35f; //0.15f

            //Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value;
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Content/Weapons/Magic/Hardmode/Staves/SpectreAssets/lostsoul").Value;
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
            myEffect.Parameters["reps"].SetValue(0.75f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.White.ToVector3() * 1f);

            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            //vertexStrip.DrawTrail();

            //vertexStrip.DrawTrail();

            //Under Layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["ColorOne"].SetValue(Color.DodgerBlue.ToVector3() * 2f);
            myEffect.Parameters["progress"].SetValue(timer * 0.06f);
            myEffect.Parameters["reps"].SetValue(1.5f);

            myEffect.Parameters["glowThreshold"].SetValue(0.65f);
            myEffect.Parameters["glowIntensity"].SetValue(1.5f);

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public float StripWidth(float progress)
        {
            return 5f * overallAlpha;
            
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 100f, Easings.easeInCirc(num)) * 0.4f * Easings.easeInQuad(1f); //* 1.15f * Easings.easeInSine(width); //0.5f; // 0.3f 
        }

        public float StripWidth2(float progress)
        {
            
            float width = 20f;
            float pinchAmount = 0.5f;

            if (progress < 0.5f)
            {
                float lerpValue = Utils.GetLerpValue(0f, pinchAmount, progress, clamped: true);
                float num = 1f - (1f - lerpValue) * (1f - lerpValue);
                return MathHelper.Lerp(0f, width, num) * overallAlpha;
            }
            else if (progress >= 0.5)
            {
                return width * overallAlpha;

                float lerpValue = Utils.GetLerpValue(0f, pinchAmount, 1 - progress, clamped: true);
                float num = 1f - (1f - lerpValue) * (1f - lerpValue);
                return MathHelper.Lerp(0f, width, num);
            }

            return 0f;
            
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < previousPostions.Count; i++)
            {
                Vector2 pos = previousPostions[i];
                Vector2 velRot = previousRotations[i].ToRotationVector2();

                if (i % 2 == 0)
                {
                    //int a = Dust.NewDust(pos, 0, 0, ModContent.DustType<GlowFlare>(), 0, 0, newColor: Color.SkyBlue, Scale: 0.4f);
                    //Main.dust[a].customData = new GlowFlareBehavior(0.4f, 2.5f, 1f);
                    //Main.dust[a].velocity *= ((i * 0.06f));
                    //Main.dust[a].velocity -= projectile.velocity * 0.5f;

                    Color col = Main.rand.NextBool() ? Color.White : Color.SkyBlue;

                    Dust d = Dust.NewDustPerfect(pos, ModContent.DustType<GlowPixelFast>(), Alpha: 100, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));

                    Vector2 dustVel = (velRot * Main.rand.NextFloat(1f, 4.1f) * -0.5f).RotateRandom(0.3f);
                    d.velocity = dustVel + Main.rand.NextVector2Circular(3f, 3f);

                    //d.fadeIn = 50;
                }

            }

            if (timer % 2 == 0 && timer > 2000)
            {
                Color col = Main.rand.NextBool() ? Color.White : Color.SkyBlue;

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelFast>(), Alpha: 100, newColor: col, Scale: Main.rand.NextFloat(0.35f, 0.55f));
                d.position -= projectile.velocity;

                Vector2 dustVel = (projectile.velocity * Main.rand.NextFloat(0.85f, 1.15f) * -1f).RotateRandom(0.3f);
                d.velocity = dustVel;

                d.fadeIn = 50;
            }

            return base.PreKill(projectile, timeLeft);
        }
    }

    //MySoulIsFullOfVoid
    /*
     * 
     * Color StripColor(float progress) => Color.White * 1f;

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
     * 
    */
}
