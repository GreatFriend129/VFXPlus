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
using Terraria.GameContent;
using System.Drawing.Drawing2D;
using Terraria.GameContent.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class RainbowGun : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.RainbowGun);
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            SoundStyle style = new SoundStyle("AerovelenceMod/Sounds/Effects/laser_line") with { Pitch = .55f, Volume = 0.35f, PitchVariance = 0.1f }; 
            SoundEngine.PlaySound(style, player.Center);

            SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_176") with { Volume = 1f, Pitch = 1f, PitchVariance = 0.1f }; 
            SoundEngine.PlaySound(style2, player.Center);

            foreach (Projectile p in Main.projectile)
            {
                //Separate 
                if (p.type == ModContent.ProjectileType<RainbowGunVFX>())
                {
                    if (p.owner == player.whoAmI)
                    {
                        (p.ModProjectile as RainbowGunVFX).shouldFade = true;
                    }
                }
            }

            return true;
        }

    }
    public class RainbowGunFrontShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RainbowFront);
        }

        int timer = 0;
        int vfxIndex = -1;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 2)
            {
                //Spawn the VFX projectile
                int p = Projectile.NewProjectile(null, projectile.Center, Vector2.Zero, ModContent.ProjectileType<RainbowGunVFX>(), 0, 0, projectile.owner);
                vfxIndex = p;
                Main.projectile[p].rotation = projectile.velocity.ToRotation();

                //Start the VFX proj at random color
                float randRainbowOffset = Main.rand.NextFloat(0f, 1f);
                (Main.projectile[p].ModProjectile as RainbowGunVFX).rainbowOffset = randRainbowOffset;

                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = projectile.Center,
                    MovementVector = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * 20f //30

                };
                ParticleOrchestraSettings particleSettings2 = new()
                {
                    PositionInWorld = projectile.Center,
                    MovementVector = Main.rand.NextVector2CircularEdge(10f, 10f)

                };

                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.RainbowRodHit, particleSettings);
                //ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.RainbowRodHit, particleSettings2);

                //Circle Pulses
                float randomStart = Main.rand.NextFloat(0f, 1f);
                for (int j = 0; j < 10; j++)
                {
                    //Color rainbow = Main.hslToRgb((j * 0.1f + randomStart) % 1f, 1f, 0.6f, 0) * 1f;
                    Color rainbow = Main.hslToRgb((randRainbowOffset + 0.08f) % 1f, 1f, 0.7f, 0) * 0.75f;
                    // ^ Doing + 0.08 so that the colors match up by the time the circle are visible

                    if (j == 0)
                        rainbow = Color.White;

                    float progress = (float)j / 9;
                    int inverse = 9 - j;

                    float widthStretch = Easings.easeOutSine(progress) * 0.5f; //j * 0.1f

                    Vector2 lessVel = projectile.velocity * (0.1f + (j * 0.035f));

                    Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity, ModContent.DustType<CirclePulse>(), 0.75f * lessVel, newColor: rainbow);
                    d2.scale = 0.05f;
                    CirclePulseBehavior b2 = new CirclePulseBehavior(0.17f + (j * 0.00f), true, 2, 0.25f, 0.35f + widthStretch); //0.35scale
                    b2.drawLayer = "Dusts";
                    d2.customData = b2;
                }

                //Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), lessVel, newColor: Color.White);
                //CirclePulseBehavior b = new CirclePulseBehavior(0.3f, true, 1, 0.4f, 0.8f);
                //b.drawLayer = "Dusts";
                //d.customData = b;

                //Dust d2 = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<CirclePulse>(), lessVel * 1.2f, newColor: Color.White);
                //CirclePulseBehavior b2 = new CirclePulseBehavior(0.3f, true, 1, 0.2f, 0.4f);
                //b2.drawLayer = "Dusts";
                //d2.customData = b2;
                //Rainbow burst dust (unused)

                for (int i = 200; i < 22 + Main.rand.Next(0, 2); i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(10f, 10f) * 2f;

                    Color rainbow = Main.hslToRgb((i * 0.05f + 0.5f) % 1f, 1f, 0.6f, 0) * 1f;

                    Dust pa = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.25f, ModContent.DustType<PixelatedLineSpark>(), vel,
                        newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.7f);

                    pa.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.83f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                        1f, 0.5f, shouldFadeColor: false);

                    if (i % 1 == 0)
                    {
                        Dust p2 = Dust.NewDustPerfect(projectile.Center + (vel * 3f) - projectile.velocity, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.2f);
                        p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.04f);
                    }

                }
            }

            if (vfxIndex != -1)
            {
                //Add point to VFX Proj
                (Main.projectile[vfxIndex].ModProjectile as RainbowGunVFX).AddPoint(projectile.Center, projectile.velocity.ToRotation());
            }

            if (timer % 2 == 0 && false)
            {
                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = projectile.Center,
                    MovementVector = Main.rand.NextVector2CircularEdge(1f, 1f) + projectile.velocity * 0.05f

                };

                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.ShimmerBlock, particleSettings);
            }


            timer++;

            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects se = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 0 }, projectile.rotation, TexOrigin, projectile.scale, se);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (vfxIndex != -1)
            {
                (Main.projectile[vfxIndex].ModProjectile as RainbowGunVFX).isAttached = false;
                (Main.projectile[vfxIndex].ModProjectile as RainbowGunVFX).headCollidePower = 2f;
            }

            for (int i = 220; i < 10 + Main.rand.Next(0, 2); i++) //4 //2,2
            {
                Vector2 vel = Main.rand.NextVector2Circular(9f, 9f);

                Color rainbow = Main.hslToRgb((i * 0.05f + 0.5f) % 1f, 1f, 0.6f, 0) * 1f;

                Dust p = Dust.NewDustPerfect(projectile.Center - projectile.velocity, ModContent.DustType<PixelatedLineSpark>(), vel,
                    newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.7f);

                p.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                    1f, 0.5f, shouldFadeColor: false);

                if (i % 122 == 0)
                {
                    Dust p2 = Dust.NewDustPerfect(projectile.Center + (vel * 4f) - projectile.velocity, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.2f);
                    p2.customData = DustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.1f);
                }

            }

            return base.PreKill(projectile, timeLeft);
        }
    }

    //I will pass on the backshots. I want to focus on myself. I strive for a well organized schedule that balances work and personal life, allowing me to focus on my career goals while maintaining a healthy lifestyle. By prioritizing tasks, setting realistic deadlines, and continuously learning and improving my skills I am
    public class RainbowGunBackShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RainbowBack);
        }

        public override bool PreAI(Projectile projectile)
        {
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }

    }

    // 1 projectile exists for each time the gun is shot (stores points from RainbowGunFront travel path)
    public class RainbowGunVFX : ModProjectile
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
        }

        int timer = 0;
        float true_width = 1f;
        float true_alpha = 0.1f;


        public bool shouldFade = false;
        public bool isAttached = true;

        //Random color offset, used when spawning in the projectile in order to sync color with dustFX
        public float rainbowOffset = 0;

        public float headCollidePower = 1f;

        public override void AI()
        {
            if (!shouldFade)
                true_alpha = Math.Clamp(MathHelper.Lerp(true_alpha, 1.5f, 0.05f), 0f, 1f); //0.04
            else
            {
                true_alpha = Math.Clamp(MathHelper.Lerp(true_alpha, -0.4f, 0.1f), 0f, 1f); //0.04

                if (true_alpha < 0.1f)
                    Projectile.active = false;
            }

            //For the head of the trail to pop when rainbowgunfront dies
            headCollidePower = Math.Clamp(MathHelper.Lerp(headCollidePower, 0.5f, 0.07f), 1f, 2f); 

            //Cast light from every 10 points. This is a possible source of lag if rainbow is long | "LagPos"
            for (int i = 0; i < l_positions.Count; i += 10)
            {
                //Dust.NewDustPerfect(l_positions[i], DustID.Adamantite, new Vector2(0f, -10f));
                Lighting.AddLight(l_positions[i], Main.hslToRgb((timer * 0.01f + rainbowOffset) % 1f, 1f, 0.75f, 0).ToVector3() * 0.75f);
            }


            //Line dust all throughout trail
            if (timer > 5 && timer % 1 == 0)
            {
                for (int i = 0; i < l_positions.Count * 0.9f; i += 3)
                {
                    Vector2 pos = l_positions[i];
                    float rot = l_rotations[i];

                    Color rainbow = Main.hslToRgb((timer * 0.01f + rainbowOffset) % 1f, 1f, 0.5f, 0) * 0.75f;

                    if (Main.rand.NextBool() && (!shouldFade || Main.rand.NextBool()))
                    {
                        Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15, 15);
                        Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.05f) * Main.rand.NextFloat(2, 7);


                        Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<LineSpark>(), vel * 1.5f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.5f);
                        d.noLight = false;
                        if (shouldFade)
                            d.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.9f, postShrinkPower: 0.92f, timeToStartShrink: 10, killEarlyTime: 100, XScale: 0.4f, YScale: 0.25f, shouldFadeColor: true, colorFadePower: 0.92f);
                        else
                            d.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.97f, killEarlyTime: 10, XScale: 0.5f, YScale: 0.2f, shouldFadeColor: true);
                    }

                }
            }

            #region unused dust patterns
            /*
            if (timer % 4 == 0 && Main.rand.NextBool(3) && shouldFade && false)
            {
                Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15, 15);
                Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.2f) * Main.rand.NextFloat(2, 7);

                Color col = Color.White;

                Dust d = Dust.NewDustPerfect(pos + offset, DustID.PortalBoltTrail, vel * 1.5f, newColor: rainbow * 2f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 1f);
                d.alpha = 100;
                d.noGravity = true;
            }

            
            if (i % 2 == 0 && Main.rand.NextBool() && false)
            {
                Vector2 offset2 = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-10, 10);
                Vector2 vel2 = rot.ToRotationVector2().RotatedByRandom(0.15f) * Main.rand.NextFloat(2, 9);

                Dust d2 = Dust.NewDustPerfect(pos + offset2, ModContent.DustType<ElectricSparkGlow>(), vel2 * 1.25f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.75f);

                ElectricSparkBehavior esb = new ElectricSparkBehavior(KillEarlyTime: 100, XScale: 1f, YScale: 0.5f, UnderGlowPower: 2f, WhiteLayerPower: 0.15f);
                d2.customData = esb;
                //d2.customData = DustBehaviorUtil.AssignBehavior_GPCBase(timeBeforeSlow: 3, postSlowPower: 0.92f, velToBeginShrink: 1.5f, fadePower: 0.93f, shouldFadeColor: false);

            }
            */
            #endregion

            timer++;
        }

        Effect myEffect = null;
        public List<Vector2> l_positions = new List<Vector2>();
        public List<float> l_rotations = new List<float>();
        public override bool PreDraw(ref Color lightColor)
        {
            if (l_positions.Count == 0 || l_rotations.Count == 0)
                return false;

            float ease1 = shouldFade ? Easings.easeOutQuad(true_alpha) : Easings.easeInOutBack(true_alpha, 0f, 5f);
            float ease2 = shouldFade ? Easings.easeOutQuad(true_alpha) : Easings.easeInOutBack(true_alpha, 0f, 2f);
            float ease3 = shouldFade ? Easings.easeOutQuad(true_alpha) : Easings.easeOutCirc(true_alpha);

            Color rainbow = Main.hslToRgb((timer * 0.01f + rainbowOffset) % 1f, 1f, 0.7f, 0) * 0.75f * ease1;

            #region Portal
            Texture2D portal = Mod.Assets.Request<Texture2D>("Assets/Pixel/RainbowRod").Value;
            Texture2D bloom = Mod.Assets.Request<Texture2D>("Assets/Orbs/feather_circle128PMA").Value;

            float sinScale = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.03f) * 0.2f;

            Vector2 v2Scale = new Vector2(1f * ease1, 0.6f * ease2) * true_width * sinScale;


            Vector2 drawPosStart = l_positions[0] - Main.screenPosition;
            float starRotStart = l_rotations[0] + MathHelper.PiOver2;

            Main.EntitySpriteDraw(portal, drawPosStart, null, Color.Black * 0.3f * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);

            Main.EntitySpriteDraw(bloom, drawPosStart, null, rainbow with { A = 0 } * true_alpha * 0.4f, starRotStart, bloom.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(2f, 2f), null, rainbow with { A = 0 } * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(3f, 3f), null, rainbow with { A = 0 } * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 1.75f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(5f, 5f), null, Color.White with { A = 0 } * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            //Draw at end if disconnected
            if (true)
            {
                float lerpXVal = MathHelper.Lerp(1f, 0.6f, headCollidePower - 1f);
                float lerpYVal = MathHelper.Lerp(0.6f, 1f, headCollidePower - 1f);

                if (isAttached)
                    v2Scale = new Vector2(0.6f * ease1, 1f * ease2) * true_width * sinScale * 0.85f;
                else
                    v2Scale = new Vector2(lerpXVal * ease1, lerpYVal * ease2) * true_width * sinScale;
                //v2Scale *= headCollidePower;

                Vector2 drawPosEnd = l_positions[l_positions.Count - 1] - Main.screenPosition;
                float starRotEnd = l_rotations[l_rotations.Count - 1] + MathHelper.PiOver2;

                Main.EntitySpriteDraw(portal, drawPosEnd, null, Color.Black * 0.3f * true_alpha, starRotEnd, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);

                Main.EntitySpriteDraw(bloom, drawPosEnd, null, rainbow with { A = 0 } * true_alpha * 0.4f, starRotEnd, bloom.Size() / 2, v2Scale * 1f, SpriteEffects.None);

                Main.EntitySpriteDraw(portal, drawPosEnd + Main.rand.NextVector2Circular(2f, 2f), null, rainbow with { A = 0 } * true_alpha, starRotEnd, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, drawPosEnd + Main.rand.NextVector2Circular(3f, 3f), null, rainbow with { A = 0 } * true_alpha, starRotEnd, portal.Size() / 2, v2Scale * 1.75f, SpriteEffects.None);
                Main.EntitySpriteDraw(portal, drawPosEnd + Main.rand.NextVector2Circular(5f, 5f), null, Color.White with { A = 0 } * true_alpha, starRotEnd, portal.Size() / 2, v2Scale * 1f, SpriteEffects.None);
            }

            #endregion

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/ComboLaserVertex", AssetRequestMode.ImmediateLoad).Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);

            return false;
        }

        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp)
                return;

            //Create arrays
            Vector2[] pos_arr = l_positions.ToArray();
            float[] rot_arr = l_rotations.ToArray();

            float widthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.07f) * 0.15f;

            Color StripColor(float progress) => Color.White * true_alpha;
            float StripWidth(float progress) => 40f * Easings.easeOutQuad(true_alpha) * widthMult;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            ShaderParams();

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
        public void ShaderParams()
        {
            float fadeOut = shouldFade ? Math.Clamp(Easings.easeOutSine(true_alpha), 0.15f, 1f) : true_alpha;

            Color rainbow = Main.hslToRgb((timer * 0.01f + rainbowOffset) % 1f, 1f, 0.72f, 0) * 0.75f * fadeOut; //0.6 gives a fucking amazing orange

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/ThinLineGlowClear").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["satPower"].SetValue(0.95f); //higher power -> less affected by background  |95 | 3f looks very goozma

            myEffect.Parameters["sampleTexture1"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/ThinGlowLine").Value);
            myEffect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/s06sBloom").Value);
            myEffect.Parameters["sampleTexture3"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/spark_07_Black").Value);
            myEffect.Parameters["sampleTexture4"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/FlameTrail").Value); //smokeTrail4_512

            myEffect.Parameters["Color1"].SetValue(rainbow.ToVector4());
            myEffect.Parameters["Color2"].SetValue(rainbow.ToVector4());
            myEffect.Parameters["Color3"].SetValue(rainbow.ToVector4());
            myEffect.Parameters["Color4"].SetValue(rainbow.ToVector4());

            myEffect.Parameters["Color1Mult"].SetValue(1.55f * 1f); 
            myEffect.Parameters["Color2Mult"].SetValue(1.55f * 1f);
            myEffect.Parameters["Color3Mult"].SetValue(0.75f * 1f); 
            myEffect.Parameters["Color4Mult"].SetValue(1f * 0.25f);
            myEffect.Parameters["totalMult"].SetValue(1.05f); // 10f looks cool and void-y


            //We want the number of repititions to be relative to the number of points
            float repValue = 0.05f * l_positions.Count;

            myEffect.Parameters["tex1reps"].SetValue(1f * repValue); //2.5
            myEffect.Parameters["tex2reps"].SetValue(0.3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(1f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(0.25f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.015f); //-0.05

        }

        //Used by real rainbowgunfront to add points
        public void AddPoint(Vector2 pos, float rot)
        {
            l_positions.Add(pos);
            l_rotations.Add(rot);
        }
    }
}
