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
using Terraria.Graphics;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    public class ShadowbeamStaffShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.ShadowBeamFriendly) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.ShadowbeamStaffToggle;
        }

        //projectile.ai[0] holds the index of the VFX projectile

        Vector2 previousHead;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            //Create shadowbeam vfx proj
            if (timer == 0)
            {
                previousHead = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 55; //60

                if (Main.myPlayer == projectile.owner)
                {
                    int p = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<ShadowbeamStaffVFX>(), 0, 0, projectile.owner);

                    projectile.ai[0] = p;
                    projectile.netUpdate = true;
                }

                float circlePulseSize = 0.6f;

                Color purple = new Color(61, 2, 92);
                Color darkPurple = new Color(42, 2, 82);
                Color purple3 = new Color(121, 7, 179);

                Dust d2 = Dust.NewDustPerfect(previousHead, ModContent.DustType<CirclePulse>(), projectile.velocity * -0.01f, newColor: purple3 * 0.3f);
                CirclePulseBehavior b2 = new CirclePulseBehavior(circlePulseSize * 1.5f, true, 1, 0.25f, 0.25f);
                b2.drawLayer = "Dusts";
                d2.customData = b2;

                for (int i = 0; i < 7 + Main.rand.Next(0, 4); i++)
                {
                    Dust dp = Dust.NewDustPerfect(previousHead, ModContent.DustType<ElectricSparkGlow>(),
                        projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.45f, 0.45f)) * Main.rand.Next(3, 8) * 1.45f,
                        newColor: purple * 2f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 1.85f);

                    ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.91f, Pixelize: true, XScale: 0.75f, YScale: 0.75f,
                        UnderGlowPower: 3f, WhiteLayerPower: 0.25f, DrawWhiteWithAlphaZero: false); //0.91

                    if (i < 8)
                        esb.randomVelRotatePower = 0.7f;
                    dp.customData = esb;
                }


                for (int i = 0; i < 5 + Main.rand.Next(3); i++)
                {
                    Color col = Main.rand.NextBool(2) ? purple3 : darkPurple;
                    Vector2 vel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) * Main.rand.NextFloat(0.5f, 2.75f);
                    Dust d = Dust.NewDustPerfect(previousHead, ModContent.DustType<RoaParticle>(), vel, newColor: col * 1.5f, Scale: Main.rand.NextFloat(0.5f, 1.2f) * 1f);
                    d.fadeIn = Main.rand.Next(0, 4);
                    d.alpha = Main.rand.Next(0, 2);
                    d.noLight = false;

                    d.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * 3.5f;
                }

            }

            timer++;
            return false;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            //Add node and disconnect proj
            AddNewNode(projectile);

            (Main.projectile[(int)projectile.ai[0]].ModProjectile as ShadowbeamStaffVFX).isAttached = false;

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: Color.Purple, Scale: 1.25f);
            d1.rotation = projectile.velocity.ToRotation();
            d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: Color.Purple, Scale: 1.25f);
            d2.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver4;
            d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            for (int i = 0; i < 5; i++)
            {
                Vector2 randomStart = Main.rand.NextVector2Circular(3.25f, 3.25f) * 1f;
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.Purple * 4f, Scale: Main.rand.NextFloat(0.45f, 0.55f) * 1.15f);

                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(
                    rotPower: 0.15f, preSlowPower: 0.96f, timeBeforeSlow: 12, postSlowPower: 0.92f, velToBeginShrink: 3f, fadePower: 0.91f, shouldFadeColor: false);

                dust.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * 3f;
            }

            for (int i = 0; i < 7; i++)
            {
                Color purple = new Color(61, 2, 92);
                Color darkPurple = new Color(42, 2, 82); 
                Color purple3 = new Color(121, 7, 179);

                Color col = Main.rand.NextBool(2) ? purple3 : darkPurple;
                Vector2 vel = Main.rand.NextVector2CircularEdge(1.25f, 1.25f) * Main.rand.NextFloat(0.5f, 2.75f);
                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<RoaParticle>(), vel, newColor: col * 2f, Scale: Main.rand.NextFloat(0.5f, 1.2f) * 1f);
                d.fadeIn = Main.rand.Next(0, 4);
                d.alpha = Main.rand.Next(0, 2);
                d.noLight = false;

                d.velocity += projectile.velocity.SafeNormalize(Vector2.UnitX) * 4f;
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Add node
            AddNewNode(projectile);

            Color purple = new Color(61, 2, 92);

            Color dustCol = purple;

            Dust d1 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: dustCol, Scale: 1.3f);
            d1.rotation = 0f + oldVelocity.ToRotation();
            d1.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);

            Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowStarSharp>(), Vector2.Zero, newColor: dustCol, Scale: 1.3f);
            d2.rotation = MathHelper.PiOver4 + oldVelocity.ToRotation();
            d2.customData = DustBehaviorUtil.AssignBehavior_GSSBase(fadePower: 0.9f, shouldFadeColor: false);


            SoftGlowDustBehavior sgdb = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.8f, sizeChangeSpeed: 0.9f, timeToKill: 20, 
                overallAlpha: 0.5f, DrawWhiteCore: true, 1f, 1f);

            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: dustCol, Scale: 0.13f);
            softGlow.customData = sgdb;

            //Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<ShadowbeamStarVFX>(), 0, 0, Main.myPlayer);

            return base.OnTileCollide(projectile, oldVelocity);
        }

        public void AddNewNode(Projectile projectile)
        {
            if (projectile.ai[0] == -1)
            {
                Main.NewText("vfxIndex is -1 | I don't know how this would ever happen");
                return;
            }

            ShadowbeamNode sbn = new ShadowbeamNode(previousHead, projectile.Center);
            (Main.projectile[(int)projectile.ai[0]].ModProjectile as ShadowbeamStaffVFX).nodes.Add(sbn);
            previousHead = projectile.Center;
        }

    }

    public class ShadowbeamStaffVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            //Make sure to draw projectile even if its position is off screen
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 7500;
        }

        //Safety Checks
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2400;

            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        int timer = 0;
        float true_width = 1f;
        float true_alpha = 1f;

        public bool isAttached = true;
        public override void AI()
        {
            if (!isAttached && timer > 60)
                Projectile.active = false;


            //Cast Lights and create dust
            if (nodes.Count > 0)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    Vector2 head = nodes[i].head;
                    Vector2 tail = nodes[i].tail;


                    DelegateMethods.v3_1 = Color.Purple.ToVector3() * 1.25f * Projectile.scale * true_width;
                    Utils.PlotTileLine(head, tail, 10f * true_width, DelegateMethods.CastLight);

                    float dist = (head - tail).Length();

                    float numberOfReps = dist / 75f;

                    if (timer == 6 && true_width > 0.2f)
                    {
                        for (int j = 0; j < numberOfReps; j++)
                        {
                            float rand = Main.rand.Next();

                            float percent = Main.rand.NextFloat(0f, 0.8f);

                            Vector2 pos = Vector2.Lerp(head, tail, percent);
                            Vector2 off = Main.rand.NextVector2Circular(14f, 14f);

                            Vector2 vel = nodes[i].rot.ToRotationVector2().RotatedByRandom(0.08f) * Main.rand.NextFloat(2, 10);

                            Dust d = Dust.NewDustPerfect(pos + off, ModContent.DustType<LineSpark>(), vel * 2f, newColor: Color.Purple * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.5f);
                            d.noLight = false;
                            d.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.89f, postShrinkPower: 0.89f, timeToStartShrink: 16, killEarlyTime: 100, XScale: 0.2f, YScale: 0.35f, shouldFadeColor: false);
                        }
                    }
                    
                }
            }

            if (timer > 6)
                true_width = Math.Clamp(MathHelper.Lerp(true_width, -0.5f, 0.08f), 0, 1f); 

            if (timer == 100 || true_width <= 0.05f)
                Projectile.active = false;

            timer++;
        }

        Effect myEffect = null;
        public List<ShadowbeamNode> nodes = new List<ShadowbeamNode>();
        public override bool PreDraw(ref Color lightColor)
        {
            if (nodes.Count <= 0) return false;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    ShadowbeamNode node = nodes[i];
                    DrawNodeTrail(node);
                }

                Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/GlowingFlare").Value;

                //Portal at first node
                Vector2 portalPos = nodes[0].head - Main.screenPosition;
                float rot = nodes[0].rot + MathHelper.PiOver2;

                float easedScale = true_width;// Easings.easeInOutCubic(true_width);
                Vector2 v2Scale = new Vector2(1f * easedScale, 0.5f + (easedScale * 0.5f)) * Projectile.scale * 1.5f;

                Color purple = new Color(61, 2, 92);
                Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);
                Color purple3 = new Color(121, 7, 179);

                Main.EntitySpriteDraw(portal, portalPos + Main.rand.NextVector2Circular(3f, 3f), null, Color.Purple with { A = 0 } * 0.5f, rot, portal.Size() / 2f, v2Scale * 1.15f, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, portalPos, null, purple3 with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, portalPos, null, Color.White with { A = 0 } * 1f, rot, portal.Size() / 2f, v2Scale * 0.65f, SpriteEffects.None);

            });

            return false;
        }

        public void DrawNodeTrail(ShadowbeamNode node)
        {
            Color purple = new Color(61, 2, 92); 
            Color darkPurple = new Color(42, 2, 82);  // Color.Purple;//new Color(61, 2, 92);

            Color purple3 = new Color(121, 7, 179);
            #region shaderPrep
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/s06sBloom").Value; //|spark_06 | Extra_196_Black
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value;

            Vector2[] pos_arr = { node.tail, node.head };
            float[] rot_arr = { node.rot, node.rot };

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.0f;

            Color StripColor(float progress) => Color.White;
            float StripWidth(float progress) => 30f * true_width * sineWidthMult; //25
            float StripWidth2(float progress) => 90f * true_width * sineWidthMult;
            //^ Doing Easings.easeOutQuad(progress) * Easings.easeInQuad(progress) gives a really nice zigzag patter (or do 1f - EaseIn)

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);
            #endregion

            #region Shader

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;
           
            float dist = (node.head - node.tail).Length();
            float repValue = dist / 700f;
            myEffect.Parameters["reps"].SetValue(repValue);

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f); //timer * 0.02
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);

            //UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(purple.ToVector3() * 3f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Color purp = new Color(215, 18, 215);

            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(purp.ToVector3() * 10f);
            myEffect.Parameters["glowThreshold"].SetValue(0.7f); //0.6
            myEffect.Parameters["glowIntensity"].SetValue(2.2f); //2.25
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            #endregion
        }
    }


    public class ShadowbeamNode
    {
        public Vector2 head;
        public Vector2 tail;
        public float rot;

        public ShadowbeamNode(Vector2 start, Vector2 end)
        {
            head = start;
            tail = end;

            rot = (end - start).ToRotation();
        }
    }

    public class ShadowbeamStarVFX : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        //Safety Checks
        public override bool? CanDamage() => false;
        public override bool? CanCutTiles() => false;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;

            Projectile.timeLeft = 2400;
        }

        int timer = 0;

        public override void AI()
        {
            float fadeInTime = Math.Clamp((float)(timer + 8) / 22f, 0f, 1f);
            overallScale = Easings.easeInBack(fadeInTime);

            if (fadeInTime == 1f)
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.12f), 0f, 1f);

            if (overallAlpha == 0f)
                Projectile.active = false;

            //Projectile.spriteDirection = Projectile.velocity.X > 0 ? -1 : 1;

            timer++;
        }

        float overallScale = 1f;
        float overallAlpha = 1f;
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawPortal(false);
            });
            DrawPortal(true);

            return false;
        }

        public void DrawPortal(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D Flare = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value; 
            Texture2D orb = Mod.Assets.Request<Texture2D>("Assets/Pixel/CrispStarPMA").Value;

            Vector2 originPoint = Projectile.Center - Main.screenPosition;

            int dir = Projectile.spriteDirection;

            Color purple = Color.Purple;


            float rot = (float)Main.timeForVisualEffects * 0.1f * Projectile.spriteDirection;

            float starScale = (1f - overallScale) * 0.75f;

            Main.spriteBatch.Draw(Flare, originPoint, null, purple with { A = 0 } * 2f, rot, Flare.Size() / 2, starScale, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(Flare, originPoint, null, Color.White with { A = 0 }, rot, Flare.Size() / 2, starScale * 0.65f, SpriteEffects.None, 0f);
        }
    }

}
