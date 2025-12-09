using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using VFXPlus.Common;
using VFXPlus.Content.Dusts;
using ReLogic.Content;
using VFXPlus.Common.Utilities;
using Terraria.Graphics;
using VFXPlus.Common.Drawing;
using Terraria.GameContent.Drawing;
using VFXPlus.Content.Projectiles;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.MagicGuns
{
    
    public class RainbowGun : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.RainbowGun) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.RainbowGunToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            entity.noUseGraphic = true;
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/laser_line") with { Pitch = .55f, Volume = 0.35f, PitchVariance = 0.1f }; 
            //SoundEngine.PlaySound(style, player.Center);

            //SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_176") with { Volume = 1f, Pitch = 1f, PitchVariance = 0.1f }; 
            //SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Tech/MagicImpactLong2") with { Volume = 0.65f };
            SoundEngine.PlaySound(style, player.Center);


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

            int gun = Projectile.NewProjectile(null, position, Vector2.Zero, ModContent.ProjectileType<BasicRecoilProj>(), 0, 0, player.whoAmI);
            if (Main.projectile[gun].ModProjectile is BasicRecoilProj held)
            {
                held.SetProjInfo(
                    GunID: ItemID.RainbowGun,
                    AnimTime: 20,
                    NormalXOffset: 18f,
                    DestXOffset: -3f,
                    YRecoilAmount: 0.1f,
                    HoldOffset: new Vector2(0f, 2f)
                    );
            }

            return true;
        }

    }
    public class RainbowGunFrontShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.RainbowFront) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.RainbowGunToggle;
        }

        int timer = 0;
        int vfxIndex = -1;

        Vector2 vfxSpawnPos = Vector2.Zero;

        //Whether this projectile has spawned a RainbowBack Projectile yet
        //Used so we can anchor the VFX proj to the first one that spawns
        bool hasSpawnedFirstProj = false;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 2)
            {


                ParticleOrchestraSettings particleSettings = new()
                {
                    PositionInWorld = projectile.Center,
                    MovementVector = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * 20f //30

                };
                ParticleOrchestrator.RequestParticleSpawn(true, ParticleOrchestraType.RainbowRodHit, particleSettings);

                //Circle Pulses
                float randomStart = Main.rand.NextFloat(0f, 1f);
                for (int j = 0; j < 10; j++)
                {
                    Color rainbow = Main.hslToRgb((randomStart + (j * 0.1f)) % 1f, 1f, 0.75f, 0) * 0.75f;


                    float progress = (float)j / 9;

                    float widthStretch = Easings.easeOutSine(progress) * 0.5f; //j * 0.1f

                    Vector2 lessVel = projectile.velocity * (0.1f + (j * 0.035f));

                    Dust d2 = Dust.NewDustPerfect(projectile.Center - projectile.velocity, ModContent.DustType<CirclePulse>(), 0.75f * lessVel, newColor: rainbow);
                    d2.scale = 0.05f;
                    CirclePulseBehavior b2 = new CirclePulseBehavior(0.17f + (j * 0.00f), true, 2, 0.25f, 0.35f + widthStretch); 
                    b2.drawLayer = "Dusts";
                    d2.customData = b2;
                }

                for (int i = 0; i < 19 + Main.rand.Next(0, 2); i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f) * 2f;

                    Color rainbow = Main.hslToRgb((i * 0.05f + 0.5f) % 1f, 1f, 0.7f, 0) * 1f;

                    Dust pa = Dust.NewDustPerfect(projectile.Center - projectile.velocity * 0.25f, ModContent.DustType<PixelatedLineSpark>(), vel,
                        newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.65f);

                    pa.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.83f, preShrinkPower: 0.99f, postShrinkPower: 0.82f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                        0.75f, 0.45f, shouldFadeColor: false);
                }

                vfxSpawnPos = projectile.Center;
            }

            if (vfxIndex != -1)
            {
                //Add point to VFX Proj
                (Main.projectile[vfxIndex].ModProjectile as RainbowGunVFX).AddPoint(projectile.Center, projectile.velocity.ToRotation());
            }

            timer++;

            #region vanillaAI
            int num366 = (int)(projectile.Center.X / 16f);
            int num367 = (int)(projectile.Center.Y / 16f);


            //if (WorldGen.InWorld(num366, num367) && Main.tile[num366, num367] != null && Main.tile[num366, num367].liquid > 0 && Main.tile[num366, num367].shimmer())
            if (WorldGen.InWorld(num366, num367) && Main.tile[num366, num367] != null && Main.tile[num366, num367].LiquidAmount > 0 && Main.tile[num366, num367].LiquidType == LiquidID.Shimmer)
            {
                projectile.Kill();
            }
            int num368 = 2400;
            if (projectile.type == 250)
            {
                Point point2 = projectile.Center.ToTileCoordinates();
                if (!WorldGen.InWorld(point2.X, point2.Y, 2) || Main.tile[point2.X, point2.Y] == null)
                {
                    projectile.Kill();
                    return false;
                }
                if (projectile.owner == Main.myPlayer)
                {
                    projectile.localAI[0] += 1f;
                    if (projectile.localAI[0] > 4f)
                    {
                        projectile.localAI[0] = 3f;

                        int rainbowBack = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center.X, projectile.Center.Y, projectile.velocity.X * 0.001f, projectile.velocity.Y * 0.001f, 251, projectile.damage, projectile.knockBack, projectile.owner);
                        ///
                        //Spawn and anchor the VFX proj to the first rainbowBack proj

                        if (!hasSpawnedFirstProj)
                        {
                            hasSpawnedFirstProj = true;

                            int p = Projectile.NewProjectile(null, vfxSpawnPos, Vector2.Zero, ModContent.ProjectileType<RainbowGunVFX>(), 0, 0, projectile.owner);
                            vfxIndex = p;
                            Main.projectile[p].rotation = projectile.velocity.ToRotation();

                            //Start the VFX proj at random color
                            float randRainbowOffset = Main.rand.NextFloat(0f, 1f);
                            (Main.projectile[p].ModProjectile as RainbowGunVFX).rainbowOffset = randRainbowOffset;

                            //Anchor
                            (Main.projectile[p].ModProjectile as RainbowGunVFX).anchorProj = rainbowBack;

                            (Main.projectile[vfxIndex].ModProjectile as RainbowGunVFX).AddPoint(vfxSpawnPos, projectile.velocity.ToRotation());
                        }

                        ///
                    }
                    if (projectile.timeLeft > num368)
                    {
                        projectile.timeLeft = num368;
                    }
                }
                float num369 = 1f;
                if (projectile.velocity.Y < 0f)
                {
                    num369 -= projectile.velocity.Y / 3f;
                }
                projectile.ai[0] += num369;
                if (projectile.ai[0] > 30f)
                {
                    projectile.velocity.Y += 0.5f;
                    if (projectile.velocity.Y > 0f)
                    {
                        projectile.velocity.X *= 0.95f;
                    }
                    else
                    {
                        projectile.velocity.X *= 1.05f;
                    }
                }
                float x3 = projectile.velocity.X;
                float y3 = projectile.velocity.Y;
                float num370 = (float)Math.Sqrt(x3 * x3 + y3 * y3);
                num370 = 15.95f * projectile.scale / num370;
                x3 *= num370;
                y3 *= num370;
                projectile.velocity.X = x3;
                projectile.velocity.Y = y3;
                projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - 1.57f;
                return false;
            }
            if (projectile.localAI[0] == 0f)
            {
                if (projectile.velocity.X > 0f)
                {
                    projectile.spriteDirection = -1;
                    projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - 1.57f;
                }
                else
                {
                    projectile.spriteDirection = 1;
                    projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - 1.57f;
                }
                projectile.localAI[0] = 1f;
                projectile.timeLeft = num368;
            }
            projectile.velocity.X *= 0.98f;
            projectile.velocity.Y *= 0.98f;
            if (projectile.rotation == 0f)
            {
                projectile.alpha = 255;
            }
            else if (projectile.timeLeft < 10)
            {
                projectile.alpha = 255 - (int)(255f * (float)projectile.timeLeft / 10f);
            }
            else if (projectile.timeLeft > num368 - 10)
            {
                int num371 = num368 - projectile.timeLeft;
                projectile.alpha = 255 - (int)(255f * (float)num371 / 10f);
            }
            else
            {
                projectile.alpha = 0;
            }
            #endregion

            return false;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return true;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (vfxIndex != -1)
            {
                (Main.projectile[vfxIndex].ModProjectile as RainbowGunVFX).isAttached = false;
                (Main.projectile[vfxIndex].ModProjectile as RainbowGunVFX).headCollidePower = 2f;

                Main.projectile[vfxIndex].timeLeft = timeLeft + timer;
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
            return lateInstantiation && (entity.type == ProjectileID.RainbowBack) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.RainbowGunToggle;
        }

        public override bool PreAI(Projectile projectile)
        {
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return false;
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

        public bool longFade = false;

        public bool shouldFade = false;
        public bool isAttached = true;

        //Random color offset, used when spawning in the projectile in order to sync color with dustFX
        public float rainbowOffset = 0;

        public float headCollidePower = 1f;

        
        public int anchorProj = -1;
        public override void AI()
        {
            if (anchorProj == -1 || Main.projectile[anchorProj].active == false)
                Projectile.active = false;

            if (Projectile.timeLeft < 30)
                shouldFade = true;

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
            for (int i = 0; i < l_positions.Count; i += 12)
            {
                Lighting.AddLight(l_positions[i], Main.hslToRgb((timer * 0.015f + rainbowOffset) % 1f, 1f, 0.75f, 0).ToVector3() * 0.75f);
            }


            //Line dust all throughout trail
            if (timer > 5 && timer % 4 == 0)
            {
                for (int i = 0; i < l_positions.Count * 0.9f; i += 6)
                {
                    Vector2 pos = l_positions[i];
                    float rot = l_rotations[i];

                    Color rainbow = Main.hslToRgb(Main.rand.NextFloat(0f, 1f), 1f, 0.6f, 0) * 0.75f;

                    if (Main.rand.NextBool() && (!shouldFade || Main.rand.NextBool()))
                    {
                        Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15, 15);
                        Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.05f) * Main.rand.NextFloat(2, 7);

                        Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowPixelCross>(), vel * 1f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1f) * 0.35f);
                        d.customData = DustBehaviorUtil.AssignBehavior_GPCBase(velToBeginShrink: 3f, fadePower: 0.93f, shouldFadeColor: false);
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

            Color rainbow = Main.hslToRgb((timer * 0.03f + rainbowOffset) % 1f, 1f, 0.75f, 0) * 0.75f * ease1;

            #region Portal
            Texture2D portal = CommonTextures.RainbowRod.Value;
            Texture2D bloom = CommonTextures.feather_circle128PMA.Value;

            float sinScale = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.03f) * 0.2f;

            Vector2 v2Scale = new Vector2(1f * ease1, 0.6f * ease2) * true_width * sinScale;


            Vector2 drawPosStart = l_positions[0] - Main.screenPosition;
            float starRotStart = l_rotations[0] + MathHelper.PiOver2;

            //Start Portal
            Main.EntitySpriteDraw(portal, drawPosStart, null, Color.Black * 0.3f * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);

            Main.EntitySpriteDraw(bloom, drawPosStart, null, rainbow with { A = 0 } * true_alpha * 0.4f, starRotStart, bloom.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(1f, 1f), null, rainbow with { A = 0 } * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(2f, 2f), null, rainbow with { A = 0 } * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 1.75f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosStart + Main.rand.NextVector2Circular(4f, 4f), null, Color.White with { A = 0 } * (ease3 * true_alpha), starRotStart, portal.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            //End Portal
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

            Main.EntitySpriteDraw(portal, drawPosEnd + Main.rand.NextVector2Circular(1f, 1f), null, rainbow with { A = 0 } * true_alpha, starRotEnd, portal.Size() / 2, v2Scale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosEnd + Main.rand.NextVector2Circular(2f, 2f), null, rainbow with { A = 0 } * true_alpha, starRotEnd, portal.Size() / 2, v2Scale * 1.75f, SpriteEffects.None);
            Main.EntitySpriteDraw(portal, drawPosEnd + Main.rand.NextVector2Circular(4f, 4f), null, Color.White with { A = 0 } * true_alpha, starRotEnd, portal.Size() / 2, v2Scale * 1f, SpriteEffects.None);

            #endregion

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);

            return false;
        }

        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp || true_alpha < 0.25f)
                return;

            //Create arrays
            Vector2[] pos_arr = l_positions.ToArray();
            float[] rot_arr = l_rotations.ToArray();

            float widthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.07f) * 0.15f;

            Color StripColor(float progress) => Color.White * true_alpha;
            float StripWidth(float progress) => 45f * Easings.easeInSine(true_alpha) * widthMult;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));

            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
            ShaderParams();

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }
        public void ShaderParams()
        {
            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Trails/Clear/GlowTrailClear").Value);
            myEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>("VFXPlus/Assets/Gradients/RainbowGrad1").Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["satPower"].SetValue(0.8f); //higher power -> less affected by background 

            myEffect.Parameters["sampleTexture1"].SetValue(CommonTextures.ThinGlowLine.Value);
            myEffect.Parameters["sampleTexture2"].SetValue(CommonTextures.spark_06.Value);
            myEffect.Parameters["sampleTexture3"].SetValue(CommonTextures.Extra_196_Black.Value);
            myEffect.Parameters["sampleTexture4"].SetValue(CommonTextures.Trail5Loop.Value);

            myEffect.Parameters["grad1Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad2Speed"].SetValue(2f / 3f);
            myEffect.Parameters["grad3Speed"].SetValue(3.1f / 3f);
            myEffect.Parameters["grad4Speed"].SetValue(2.3f / 3f);

            myEffect.Parameters["tex1Mult"].SetValue(1.25f);
            myEffect.Parameters["tex2Mult"].SetValue(1.5f);
            myEffect.Parameters["tex3Mult"].SetValue(1.15f);
            myEffect.Parameters["tex4Mult"].SetValue(2.5f);
            myEffect.Parameters["totalMult"].SetValue(1f);

            //We want the number of repititions to be relative to the number of points
            float repValue = 0.05f * l_positions.Count;
            myEffect.Parameters["gradientReps"].SetValue(0.25f * repValue); //1f
            myEffect.Parameters["tex1reps"].SetValue(1f * repValue); //2.5
            myEffect.Parameters["tex2reps"].SetValue(0.3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(1f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(0.25f * repValue);

            myEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.015f);

        }

        //Used by real rainbowgunfront to add points
        public void AddPoint(Vector2 pos, float rot)
        {
            l_positions.Add(pos);
            l_rotations.Add(rot);
        }
    }
}
