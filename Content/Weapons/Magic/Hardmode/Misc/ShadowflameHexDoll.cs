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
using Terraria.Graphics;
using rail;
using VFXPlus.Common.Drawing;
using VFXPlus.Content.Weapons.Magic.Hardmode.Staves;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
   
    public class ShadowflameHexDollShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ShadowFlame) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ShadowflameHexDollToggle;
        }

        int tendril_index = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //Spawn tendril vfx projectile
            if (timer == 0 && Main.myPlayer == projectile.owner)
            {
                int p = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<ShadowflameTendrilVFX>(), 0, 0, projectile.owner);
                Main.projectile[p].rotation = projectile.velocity.ToRotation();
                tendril_index = p;
            }

            //Update tendril coords
            if (Main.projectile[tendril_index] != null)
                (Main.projectile[tendril_index].ModProjectile as ShadowflameTendrilVFX).AddTendrilPosition(projectile.Center, projectile.velocity.ToRotation());

            #region vanilla AI without dust 

            Vector2 center12 = projectile.Center;
            projectile.scale = 1f - projectile.localAI[0];
            projectile.width = (int)(20f * projectile.scale);
            projectile.height = projectile.width;
            projectile.position.X = center12.X - (float)(projectile.width / 2);
            projectile.position.Y = center12.Y - (float)(projectile.height / 2);
            if ((double)projectile.localAI[0] < 0.1)
            {
                projectile.localAI[0] += 0.01f;
            }
            else
            {
                projectile.localAI[0] += 0.025f;
            }
            if (projectile.localAI[0] >= 0.95f)
            {
                projectile.Kill();
            }
            projectile.velocity.X += projectile.ai[0] * 1.5f;
            projectile.velocity.Y += projectile.ai[1] * 1.5f;
            if (projectile.velocity.Length() > 16f)
            {
                projectile.velocity.Normalize();
                projectile.velocity *= 16f;
            }
            projectile.ai[0] *= 1.05f;
            projectile.ai[1] *= 1.05f;

            /*
            if (projectile.scale < 1f)
            {
                for (int num913 = 0; (float)num913 < projectile.scale * 10f; num913++)
                {
                    int num914 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 27, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 1.1f);
                    Main.dust[num914].position = (Main.dust[num914].position + projectile.Center) / 2f;
                    Main.dust[num914].noGravity = true;
                    Dust dust133 = Main.dust[num914];
                    Dust dust2 = dust133;
                    dust2.velocity *= 0.1f;
                    dust133 = Main.dust[num914];
                    dust2 = dust133;
                    dust2.velocity -= projectile.velocity * (1.3f - projectile.scale);
                    Main.dust[num914].fadeIn = 100 + projectile.owner;
                    dust133 = Main.dust[num914];
                    dust2 = dust133;
                    dust2.scale += projectile.scale * 0.75f;
                }
            }
            */
            #endregion

            timer++;
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //Let tendril know we have detatched
            if (Main.projectile[tendril_index] != null)
                (Main.projectile[tendril_index].ModProjectile as ShadowflameTendrilVFX).isAttached = false;

            //return false;
            return base.PreKill(projectile, timeLeft);
        }
    }

    public class ShadowflameTendrilVFX : ModProjectile
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

            Projectile.timeLeft = 4400; //180
        }

        int timer = 0;
        int timeSinceDetatched = 0;
        
        float true_width = 0f;
        float true_alpha = 1f;

        public List<Vector2> l_positions = new List<Vector2>();
        public List<float> l_rotations = new List<float>();

        public bool isAttached = true;

        Vector2 startPos = Vector2.Zero;
        public override void AI()
        {
            if (timer == 0)
            {
                startPos = Projectile.Center;

                Projectile.velocity = Vector2.Zero;
            }


            if (!isAttached && timer % 1 == 0)
            {
                if (l_positions.Count > 3)
                    l_positions.RemoveAt(l_positions.Count - 1);

                if (timeSinceDetatched > 3)
                    true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.08f), 0, 1f);

                timeSinceDetatched++;
            }
            else
                true_width = Math.Clamp(MathHelper.Lerp(true_width, 2f, 0.1f), 0, 1f);

            bool shouldStopDust = (true_width <= 0.85);
            if (timer > 5 && timer % 4 == 0 && !shouldStopDust)
            {
                for (int i = 0; i < l_positions.Count * 0.75f; i += 5)
                {
                    Vector2 pos = l_positions[i];
                    float rot = l_rotations[i];

                    if (Main.rand.NextBool())
                    {
                        Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10, 10);
                        Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.15f) * Main.rand.NextFloat(2, 7);

                        Color col = Color.White;

                        Dust d = Dust.NewDustPerfect(pos + offset, DustID.Shadowflame, vel, newColor: col * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 1f);
                        d.alpha = 100;
                        d.noGravity = true;
                    }


                    if (i % 2 == 0 && Main.rand.NextBool())
                    {
                        Vector2 offset2 = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10, 10);
                        Vector2 vel2 = rot.ToRotationVector2().RotatedByRandom(0.15f) * Main.rand.NextFloat(2, 7);

                        Dust d2 = Dust.NewDustPerfect(pos + offset2, ModContent.DustType<GlowPixelCross>(), vel2, newColor: new Color(42, 2, 82) * 3f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.25f);
                        d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 3, postSlowPower: 0.92f, velToBeginShrink: 1.5f, fadePower: 0.93f, shouldFadeColor: false);
                        d2.noLight = false;
                    }
                }
            }

            if (true_width <= 0.15)
                Projectile.active = false;

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawTrail();

                Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;

                //Portal at first node
                Vector2 portalPos = startPos - Main.screenPosition;
                float rot = Projectile.rotation + MathHelper.PiOver2;

                float easedScale = Easings.easeInQuad(true_width);// Easings.easeInOutCubic(true_width);
                Vector2 v2Scale = new Vector2(1f * easedScale, 0.5f + (easedScale * 0.5f)) * Projectile.scale * 1f;

                Color purple = new Color(61, 2, 92);
                Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);
                Color purple3 = new Color(121, 7, 179);

                Main.EntitySpriteDraw(portal, portalPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.Purple with { A = 0 } * 0.5f, rot, portal.Size() / 2f, v2Scale * 1.15f, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, portalPos, null, purple3 with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, portalPos, null, Color.White with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale * 0.65f, SpriteEffects.None);

            });


            return false;
        }

        public void DrawTrail()
        {
            Color purple = new Color(61, 2, 92);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);

            Color purple3 = new Color(121, 7, 179);
            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/s06sBloom").Value; //|spark_06 | Extra_196_Black
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinnerGlowTrail").Value;

            Vector2[] pos_arr = l_positions.ToArray();
            float[] rot_arr = l_rotations.ToArray();


            Color StripColor(float progress) => Color.White * true_alpha;

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //float dist = (node.head - node.tail).Length();
            //float repValue = dist / 700f;
            myEffect.Parameters["reps"].SetValue(0.5f);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * -0.02f); //timer * 0.02
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(darkPurple.ToVector3() * 2.85f * true_alpha);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Color purp = new Color(215, 18, 215);

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(purp.ToVector3() * 5f * true_alpha);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f); //0.6
            myEffect.Parameters["glowIntensity"].SetValue(2.2f); //2.25
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }

        public float StripWidth(float progress)
        {
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 25f, Easings.easeInCirc(num)) * Easings.easeInQuad(true_width) * 0.85f; //* 1.15f * Easings.easeInSine(width); //0.5f; // 0.3f 
        }

        public float StripWidth2(float progress)
        {
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 70f, Easings.easeInCirc(num)) * Easings.easeInQuad(true_width) * 0.85f; //100 | 25 -- 75 25
        }

        public void AddTendrilPosition(Vector2 position, float rotation)
        {
            l_positions.Add(position);
            l_rotations.Add(rotation);
        }

    }
}
