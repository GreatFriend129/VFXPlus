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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class ShadowflameHexDoll : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.ShadowFlameHexDoll);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/hero_super_dash_burst") with { Volume = 0.2f, Pitch = -.25f, PitchVariance = .2f, MaxInstances = -1 }; 
            //SoundEngine.PlaySound(style, position);
            return true;
        }

    }
    public class ShadowflameHexDollShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ShadowFlame);
        }

        int tendril_index = 0;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {

            //Spawn tendril visuals projectile
            if (timer == 0)
            {
                int p = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<ShadowflameTendrilVFX>(), 0, 0, projectile.owner);
                Main.projectile[p].rotation = projectile.velocity.ToRotation();
                tendril_index = p;

                //projectile.aiStyle = -1;
                //Main.NewText(projectile.aiStyle);
            }

            (Main.projectile[tendril_index].ModProjectile as ShadowflameTendrilVFX).AddTendrilPosition(projectile.Center, projectile.velocity.ToRotation());
            
            //Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStrong>(), Vector2.Zero, Scale: 0.5f, newColor: Color.OrangeRed);
            //Update tendril coords
            ShadowflameHexDollVanillaAI(projectile);


            timer++;
            return false;
            //return base.PreAI(projectile);
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
            
            Texture2D vanillaTex = TextureAssets.Projectile[2].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + drawPosOffset;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White, projectile.velocity.ToRotation(), TexOrigin, 1f, SpriteEffects.None);

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

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


        //Vanilla SHD behavior but without the purple dust
        public void ShadowflameHexDollVanillaAI(Projectile projectile)
        {
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
            //Projectile.hide = true; //for draw behind

            Projectile.timeLeft = 4400; //180
        }

        //public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        //{
        //    overPlayers.Add(index);
        //}

        int timer = 0;
        float true_width = 1f;
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
                true_width -= 0.02f;
                true_alpha -= 0.02f;
            }

            bool shouldStopDust = (true_width <= 0.99);
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

                    }
                }
            }

            if (true_width <= 0 || true_alpha <= 0)
                Projectile.active = false;

            timer++;
        }

        Effect myEffect = null;
        public override bool PreDraw(ref Color lightColor)
        {

            Color purple = new Color(61, 2, 92); //new Color(121, 7, 179);
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);

            #region Portal
                Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/Starlight").Value;

                Vector2 drawPos = startPos - Main.screenPosition;
                Vector2 v2Scale = new Vector2(0.6f, 1f) * true_width;

                Main.EntitySpriteDraw(portal, drawPos, null, Color.Black * 0.3f * true_alpha, Projectile.rotation + MathHelper.PiOver2, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);


                Main.EntitySpriteDraw(portal, drawPos, null, purple with { A = 0 } * true_alpha, Projectile.rotation + MathHelper.PiOver2, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, drawPos, null, purple with { A = 0 } * true_alpha, Projectile.rotation + MathHelper.PiOver2, portal.Size() / 2, v2Scale * 1.75f, SpriteEffects.None);

                Main.EntitySpriteDraw(portal, drawPos, null, Color.White with { A = 0 } * true_alpha, Projectile.rotation + MathHelper.PiOver2, portal.Size() / 2, v2Scale * 1f, SpriteEffects.None);

                #endregion

            #region trail
                Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value;
                Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/LintyTrail").Value;


                if (myEffect == null)
                    myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

                //Convert lists to arrays for use in vertex strip
                Vector2[] pos_arr = l_positions.ToArray();
                float[] rot_arr = l_rotations.ToArray();

                VertexStrip vertexStrip = new VertexStrip();
                vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

                VertexStrip vertexStrip2 = new VertexStrip();
                vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);

                myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
                myEffect.Parameters["progress"].SetValue(timer * -0.03f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
                myEffect.Parameters["ColorOne"].SetValue(darkPurple.ToVector3() * 2.5f); //*2f | 2.5f | 3f
            myEffect.Parameters["reps"].SetValue(1f);

            myEffect.Parameters["glowThreshold"].SetValue(0.9f);
                myEffect.Parameters["glowIntensity"].SetValue(1.1f);


                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                vertexStrip2.DrawTrail();
                vertexStrip2.DrawTrail();

                //Under Layer
                myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
                myEffect.Parameters["ColorOne"].SetValue(purple.ToVector3() * 3f);


                myEffect.Parameters["glowThreshold"].SetValue(0.5f);
                myEffect.Parameters["glowIntensity"].SetValue(2f);

                myEffect.CurrentTechnique.Passes["MainPS"].Apply();
                vertexStrip.DrawTrail();
                vertexStrip.DrawTrail();

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                #endregion

            return false;
        }

        public void AddTendrilPosition(Vector2 position, float rotation)
        {
            l_positions.Add(position);
            l_rotations.Add(rotation);
        }

        public Color StripColor(float progress)
        {
            float alpha = 1f;

            alpha = 1f - Easings.easeInSine(progress);

            Color color = new Color(0f, 0f, 0f, alpha);

            return color * Easings.easeInSine(true_alpha);
        }
        public float StripWidth(float progress)
        {
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 100f, Easings.easeInCirc(num)) * 0.4f * Easings.easeInQuad(true_width); //* 1.15f * Easings.easeInSine(width); //0.5f; // 0.3f 
        }

        public float StripWidth2(float progress)
        {
            float num = 1f;
            float lerpValue = Utils.GetLerpValue(0f, 0.4f, 1f - progress, clamped: true);
            num *= 1f - (1f - lerpValue) * (1f - lerpValue);
            return MathHelper.Lerp(0f, 90f, Easings.easeInCirc(num)) * 0.35f * Easings.easeInQuad(true_width); 
        }

    }
}
