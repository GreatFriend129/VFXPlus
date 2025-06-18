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
using static tModPorter.ProgressUpdate;
using VFXPlus.Common.Drawing;
using ReLogic.Utilities;
using Terraria.Graphics;


namespace VFXPlus.Content.Weapons.Ranged.Hardmode.Bows
{
    
    public class PhantomPhoenixOverride : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.DD2PhoenixBow);
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

    public class PhantomPhoenixBowOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.DD2PhoenixBow);
        }
        public override bool InstancePerEntity => true;


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            #region vanillaAI but I fixed the sound
            /*
            Player player = Main.player[projectile.owner];
            float num = (float)Math.PI / 2f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            int num12 = 2;
            float num23 = 0f;

            if (projectile.type == 705)
            {
                num = 0f;
                if (projectile.spriteDirection == -1)
                {
                    num = (float)Math.PI;
                }
                projectile.ai[0] += 1f;
                int itemAnimationMax = player.itemAnimationMax;
                projectile.ai[1] -= 1f;
                bool flag6 = false;
                if (projectile.ai[1] <= 0f)
                {
                    projectile.ai[1] = itemAnimationMax;
                    flag6 = true;
                }
                bool canShoot4 = player.channel && player.HasAmmo(player.inventory[player.selectedItem]//, canUse: true//) && !player.noItems && !player.CCed;
                if (projectile.localAI[0] > 0f)
                {
                    projectile.localAI[0] -= 1f;
                }
                if (projectile.soundDelay <= 0 && canShoot4)
                {
                    projectile.soundDelay = itemAnimationMax;
                    if (projectile.ai[0] != 1f)
                    {
                        SoundEngine.PlaySound(in SoundID.Item5, projectile.position);
                    }
                    projectile.localAI[0] = 12f;
                }
                if (flag6 && Main.myPlayer == projectile.owner)
                {
                    int projToShoot4 = 14;
                    float speed4 = 12f;
                    int Damage4 = player.GetWeaponDamage(player.inventory[player.selectedItem]);
                    float KnockBack4 = player.inventory[player.selectedItem].knockBack;
                    int num70 = 2;
                    float num71 = 1.5f;
                    if (canShoot4)
                    {
                        player.PickAmmo(player.inventory[player.selectedItem], ref projToShoot4, ref speed4, ref canShoot4, ref Damage4, ref KnockBack4, out var usedAmmoItemId4);
                        IEntitySource projectileSource_Item_WithPotentialAmmo4 = player.GetProjectileSource_Item_WithPotentialAmmo(player.HeldItem, usedAmmoItemId4);
                        KnockBack4 = player.GetWeaponKnockback(player.inventory[player.selectedItem], KnockBack4);
                        if (projToShoot4 == 1)
                        {
                            projToShoot4 = 2;
                        }
                        if (++player.phantomPhoneixCounter >= 3)
                        {
                            player.phantomPhoneixCounter = 0;
                            num70 = 1;
                            Damage4 *= 2;
                            num71 = 0f;
                            this.ai[1] *= 1.5f;
                            projToShoot4 = 706;
                            speed4 = 16f;
                        }
                        float num72 = player.inventory[player.selectedItem].shootSpeed * this.scale;
                        Vector2 vector28 = vector;
                        Vector2 value4 = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector28;
                        if (player.gravDir == -1f)
                        {
                            value4.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector28.Y;
                        }
                        Vector2 vector29 = Vector2.Normalize(value4);
                        if (float.IsNaN(vector29.X) || float.IsNaN(vector29.Y))
                        {
                            vector29 = -Vector2.UnitY;
                        }
                        vector29 *= num72;
                        if (vector29.X != base.velocity.X || vector29.Y != base.velocity.Y)
                        {
                            this.netUpdate = true;
                        }
                        base.velocity = vector29 * 0.55f;
                        for (int num73 = 0; num73 < num70; num73++)
                        {
                            Vector2 vector30 = Vector2.Normalize(base.velocity) * speed4;
                            vector30 += Main.rand.NextVector2Square(0f - num71, num71);
                            if (float.IsNaN(vector30.X) || float.IsNaN(vector30.Y))
                            {
                                vector30 = -Vector2.UnitY;
                            }
                            Vector2 vector31 = vector28;
                            int num74 = Projectile.NewProjectile(projectileSource_Item_WithPotentialAmmo4, vector31.X, vector31.Y, vector30.X, vector30.Y, projToShoot4, Damage4, KnockBack4, this.owner);
                            Main.projectile[num74].noDropItem = true;
                        }
                    }
                    else
                    {
                        this.Kill();
                    }
                }
            }
            projectile.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            projectile.timeLeft = 2;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.SetDummyItemTime(num12);
            player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(projectile.velocity.Y * (float)projectile.direction, projectile.velocity.X * (float)projectile.direction) + num23);
            */
            #endregion

            return true;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            return true;
        }
    }


    public class PhantomPhoenixBirdOverride : GlobalProjectile
    {
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.DD2PhoenixBowShot);
        }
        public override bool InstancePerEntity => true;


        //Sound slot shite
        private SlotId phoenixSlot = SlotId.Invalid;


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 28; //18

            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.rotation);

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            previousPositions.Add(projectile.Center + projectile.velocity * 0.5f);
            previousRotations.Add(projectile.rotation);

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            //Sound FUCK THIS SHIT
            //if (!SoundEngine.TryGetActiveSound(phoenixSlot, out ActiveSound sound))
            //    phoenixSlot = SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot, projectile.Center);
            //else
            //    sound.Position = projectile.Center;
            if (timer == 0)
                SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot with { MaxInstances = -1 }, projectile.Center);

            float fadeInTime = Math.Clamp((timer + 9f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;

            #region vanillaAI
            if (projectile.wet)
            {
                projectile.Kill();
                return false;
            }
            SlotId val;
            if (projectile.localAI[1] == 0f)
            {
                float[] array = projectile.localAI;
                //val = SoundEngine.PlayTrackedSound(in SoundID.DD2_PhantomPhoenixShot, base.Center);
                //array[0] = ((SlotId)(ref val)).ToFloat();
                projectile.localAI[1] += 1f;
                for (int num26 = 0; num26 < 15; num26++)
                {
                    if (Main.rand.Next(4) != 0)
                    {
                        //Dust dust9 = Dust.NewDustDirect(projectile.Center - projectile.Size / 4f, projectile.width / 2, projectile.height / 2, Utils.SelectRandom<int>(Main.rand, 6, 31, 31));
                        //dust9.noGravity = true;
                        //dust9.velocity *= 2.3f;
                        //dust9.fadeIn = 1.5f;
                        //dust9.noLight = true;
                    }
                }
            }
            //ActiveSound activeSound = SoundEngine.TryGetActiveSound(SlotId.FromFloat(projectile.localAI[0]));
            //if (activeSound != null)
            //{
            //    activeSound.Position = projectile.Center;
            //}
            //else
            //{
            //    float[] array2 = projectile.localAI;
            //    val = SlotId.Invalid;
            //    array2[0] = ((SlotId)(ref val)).ToFloat();
            //}
            if (projectile.alpha <= 0)
            {
                for (int num27 = 220; num27 < 2; num27++)
                {
                    if (Main.rand.Next(4) != 0)
                    {
                        Dust dust10 = Dust.NewDustDirect(projectile.Center - projectile.Size / 4f, projectile.width / 2, projectile.height / 2, Utils.SelectRandom<int>(Main.rand, 6, 31, 31));
                        dust10.noGravity = true;
                        dust10.velocity *= 2.3f;
                        dust10.fadeIn = 1.5f;
                        dust10.noLight = true;
                    }
                }
                Vector2 spinningpoint8 = new Vector2(0f, (float)Math.Cos((float)projectile.frameCounter * ((float)Math.PI * 2f) / 40f - (float)Math.PI / 2f)) * 16f;
                spinningpoint8 = spinningpoint8.RotatedBy(projectile.rotation);
                Vector2 vector16 = projectile.velocity.SafeNormalize(Vector2.Zero);
                for (int num28 = 220; num28 < 1; num28++)
                {
                    Dust dust11 = Dust.NewDustDirect(projectile.Center - projectile.Size / 4f, projectile.width / 2, projectile.height / 2, 6);
                    dust11.noGravity = true;
                    dust11.position = projectile.Center + spinningpoint8;
                    dust11.velocity *= 0f;
                    dust11.fadeIn = 1.4f;
                    dust11.scale = 1.15f;
                    dust11.noLight = true;
                    dust11.position += projectile.velocity * 1.2f;
                    dust11.velocity += vector16 * 2f;
                    Dust dust12 = Dust.NewDustDirect(projectile.Center - projectile.Size / 4f, projectile.width / 2, projectile.height / 2, 6);
                    dust12.noGravity = true;
                    dust12.position = projectile.Center + spinningpoint8;
                    dust12.velocity *= 0f;
                    dust12.fadeIn = 1.4f;
                    dust12.scale = 1.15f;
                    dust12.noLight = true;
                    dust12.position += projectile.velocity * 0.5f;
                    dust12.position += projectile.velocity * 1.2f;
                    dust12.velocity += vector16 * 2f;
                }
            }
            if (++projectile.frameCounter >= 40)
            {
                projectile.frameCounter = 0;
            }
            projectile.frame = projectile.frameCounter / 5;
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 55;
                if (projectile.alpha < 0)
                {
                    projectile.alpha = 0;
                    float num29 = 16f;
                    for (int num30 = 220; (float)num30 < num29; num30++)
                    {
                        Vector2 spinningpoint9 = Vector2.UnitX * 0f;
                        spinningpoint9 += -Vector2.UnitY.RotatedBy((float)num30 * ((float)Math.PI * 2f / num29)) * new Vector2(1f, 4f);
                        spinningpoint9 = spinningpoint9.RotatedBy(projectile.velocity.ToRotation());
                        int num31 = Dust.NewDust(projectile.Center, 0, 0, 6);
                        Main.dust[num31].scale = 1.5f;
                        Main.dust[num31].noLight = true;
                        Main.dust[num31].noGravity = true;
                        Main.dust[num31].position = projectile.Center + spinningpoint9;
                        Main.dust[num31].velocity = Main.dust[num31].velocity * 4f + projectile.velocity * 0.3f;
                    }
                }
            }
            DelegateMethods.v3_1 = new Vector3(1f, 0.6f, 0.2f);
            Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * 4f, 40f, DelegateMethods.CastLightOpen);

            projectile.rotation = projectile.velocity.ToRotation();
            projectile.direction = projectile.velocity.X > 0 ? 1 : -1;
            #endregion

            return false;
        }

        float overallScale = 0f;
        float overallAlpha = 1f;
        public List<Vector2> previousPositions = new List<Vector2>();
        public List<float> previousRotations = new List<float>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            float rot = projectile.velocity.ToRotation();
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(projectile, false);
                DrawVertexTrail(true);
            });
            DrawTrail(projectile, true);


            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Vector2 orbPos = drawPos + new Vector2(0f, 0f);
            Vector2 orbScale = new Vector2(1f, 0.75f) * projectile.scale * overallScale * 1.5f;
            Main.EntitySpriteDraw(orb, orbPos, null, Color.OrangeRed with { A = 0 } * overallAlpha * 0.15f, rot, orb.Size() / 2f, orbScale, SE);
            Main.EntitySpriteDraw(orb, orbPos, null, Color.Orange with { A = 0 } * overallAlpha * 0.15f, rot, orb.Size() / 2f, orbScale * 0.75f, SE);

            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                if (i > previousPositions.Count * 0.75f && (i + 1) % 2 == 0)
                {
                    float progress = (float)(i) / previousPositions.Count;

                    Vector2 trailPos = previousPositions[i] - Main.screenPosition;
                    float trailAlpha = Easings.easeOutQuad(progress);
                    Vector2 trailScale = new Vector2(1f, 1f) * overallScale;

                    //Main.EntitySpriteDraw(vanillaTex, trailPos, sourceRectangle, Color.OrangeRed with { A = 0 } * trailAlpha * 0.25f, previousRotations[i], TexOrigin, trailScale, SE);
                }

                if (i > previousPositions.Count * 0.25f)
                {
                    float progress = (float)(i) / previousPositions.Count;

                    Vector2 trailPos = previousPositions[i] - Main.screenPosition;
                    float trailAlpha = Easings.easeInQuad(progress);
                    Vector2 trailScale = new Vector2(1f, 1f) * overallScale * (1f - ((1f - progress) * 0.6f));

                    Main.EntitySpriteDraw(vanillaTex, trailPos, sourceRectangle, Color.Orange with { A = 0 } * trailAlpha * 0.25f, previousRotations[i], TexOrigin, trailScale, SE);
                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
                Main.EntitySpriteDraw(vanillaTex, offset + drawPos + projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)i) * 2f, sourceRectangle, Color.White with { A = 0 } * 2f, rot, TexOrigin,
                    projectile.scale * overallScale, SE);

            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;

        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value; //smoketrailsmudge
            // CommonTextures.Extra_196_Black.Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress) * overallAlpha;
            float StripWidth(float progress) => 100f * Easings.easeOutQuad(progress) * overallAlpha;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.05f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["reps"].SetValue(1f);

            //Black UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //vertexStrip.DrawTrail();
            //vertexStrip.DrawTrail();

            //Over layer
            Color col = Color.Lerp(Color.Orange, Color.OrangeRed, 0.65f);

            myEffect.Parameters["ColorOne"].SetValue(col.ToVector3() * 3f);
            myEffect.Parameters["glowThreshold"].SetValue(0.4f);
            myEffect.Parameters["glowIntensity"].SetValue(1.4f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            #region the beach that makes you old

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color orangeRedder = Color.Lerp(Color.Orange, Color.OrangeRed, 0.35f);
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpike").Value;
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / (float)previousPositions.Count;

                Vector2 offset1 = Vector2.Zero;

                //Vector2 offset1 = new Vector2(0f, -35f * progress * overallScale).RotatedBy(dashTrailRotations[i]);
                //Vector2 offset2 = new Vector2(0f, 35f * progress * overallScale).RotatedBy(dashTrailRotations[i]);

                offset1 += Main.rand.NextVector2Circular(10f, 10f);
                //offset2 += Main.rand.NextVector2Circular(15f, 15f);


                Vector2 flarePos = previousPositions[i] - Main.screenPosition;

                Color col = Color.Lerp(Color.OrangeRed * 1.5f, orangeRedder, Easings.easeOutCubic(progress)) * overallAlpha;

                Vector2 lineScale = new Vector2(1f, 1f) * progress * overallScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, col with { A = 0 } * 0.45f * progress,
                    previousRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                //Main.EntitySpriteDraw(line, flarePos + offset2, null, col with { A = 0 } * 0.45f * progress,
                //    dashTrailRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Vector2 innerScale = new Vector2(1f, 1f * 0.1f) * progress * overallScale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, Color.White with { A = 0 } * 0.6f * progress * overallAlpha,
                    previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);

                //Main.EntitySpriteDraw(line, flarePos + offset2, null, Color.White with { A = 0 } * 0.6f * progress * overallAlpha,
                //    dashTrailRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
            }
            #endregion
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return true;

            #region Explosion
            for (int i = 0; i < 2 + Main.rand.Next(2); i++)
            {
                Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * 1.25f;
                Color col = Main.rand.NextBool() ? Color.OrangeRed : Color.Orange;
                Dust sa = Dust.NewDustPerfect(projectile.Center, DustID.PortalBoltTrail, v * Main.rand.NextFloat(2f, 5f), 0,
                    col, Main.rand.NextFloat(0.4f, 0.7f) * 1.35f);

                if (sa.velocity.Y > 0)
                    sa.velocity.Y *= -1;
            }

            for (int i = 0; i < 14; i++) //16
            {
                Color col1 = Color.Lerp(Color.OrangeRed, Color.Orange, 0.9f);

                float progress = (float)i / 13;
                Color col = Color.Lerp(Color.Brown * 0.5f, col1 with { A = 0 }, progress);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(1.5f, 4f) * 1f,
                    newColor: col, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 0.85f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(7, 19), 0.95f, 0.01f, 1f); //7 21

                d.rotation = Main.rand.NextFloat(6.28f);
            }

            //Light Dust
            Dust softGlow = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<SoftGlowDust>(), Vector2.Zero, newColor: Color.Orange, Scale: 0.2f);

            softGlow.customData = DustBehaviorUtil.AssignBehavior_SGDBase(timeToStartFade: 3, timeToChangeScale: 0, fadeSpeed: 0.9f, sizeChangeSpeed: 0.95f, timeToKill: 10,
                overallAlpha: 0.2f, DrawWhiteCore: true, 1f, 1f);

            for (int i = 0; i < 7 + Main.rand.Next(0, 3); i++)
            {
                Vector2 randomStart = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.1f);
                Dust dust = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: Color.Orange, Scale: Main.rand.NextFloat(0.25f, 0.65f) * 1.75f);
                dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.15f, preSlowPower: 0.99f, timeBeforeSlow: 13, postSlowPower: 0.92f,
                    velToBeginShrink: 4f, fadePower: 0.91f, shouldFadeColor: false);
            }

            #endregion


            #region vanilla Kill
            int num976 = 4;
            int num987 = 20;
            int num998 = 10;
            int num1009 = 20;
            int num1019 = 20;
            int num3 = 4;
            float num14 = 1.5f;
            int num25 = 6;
            int num35 = 6;
            if (Main.player[projectile.owner].setApprenticeT3)
            {
                num976 += 4;
                num1019 += 10;
                num987 += 20;
                num1009 += 30;
                num998 /= 2;
                num3 += 4;
                num14 += 0.5f;
                num25 += 7;
                num35 = 270;
            }
            projectile.position = projectile.Center;
            projectile.width = (projectile.height = 16 * num25);
            projectile.Center = projectile.position;
            projectile.Damage();
            SoundEngine.PlaySound(in SoundID.Item100, projectile.position);
            for (int num36 = 0; num36 < num976; num36++)
            {
                //int num37 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                //Main.dust[num37].position = base.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * base.width / 2f;
            }
            for (int num38 = 0; num38 < num987; num38++)
            {
                //Dust dust281 = Dust.NewDustDirect(new Vector2(base.position.X, base.position.Y), base.width, base.height, 6, 0f, 0f, 200, default(Color), 2.5f);
                //dust281.position = base.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * base.width / 10f;
                //Dust dust13 = dust281;
                //Dust dust334 = dust13;
                //dust334.velocity *= 16f;
                //if (dust281.velocity.Y > -2f)
                //{
                //    dust281.velocity.Y *= -0.4f;
                //}
                //dust281.noLight = true;
                //dust281.noGravity = true;
            }
            for (int num39 = 0; num39 < num1009; num39++)
            {
                //Dust dust282 = Dust.NewDustDirect(new Vector2(base.position.X, base.position.Y), base.width, base.height, num35, 0f, 0f, 100, default(Color), 1.5f);
                //dust282.position = base.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * base.width / 2f;
                //Dust dust14 = dust282;
                //Dust dust334 = dust14;
                //dust334.velocity *= 2f;
                //dust282.noGravity = true;
                //dust282.fadeIn = num14;
            }
            for (int num40 = 0; num40 < num998; num40++)
            {
                //int num41 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 6, 0f, 0f, 0, default(Color), 2.7f);
                //Main.dust[num41].position = base.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(base.velocity.ToRotation()) * base.width / 2f;
                //Main.dust[num41].noGravity = true;
                //Dust dust15 = Main.dust[num41];
                //Dust dust334 = dust15;
                //dust334.velocity *= 3f;
            }
            for (int num43 = 0; num43 < num1019; num43++)
            {
                //int num44 = Dust.NewDust(new Vector2(base.position.X, base.position.Y), base.width, base.height, 31, 0f, 0f, 0, default(Color), 1.5f);
                //Main.dust[num44].position = base.Center + Vector2.UnitX.RotatedByRandom(3.1415927410125732).RotatedBy(base.velocity.ToRotation()) * base.width / 2f;
                //Main.dust[num44].noGravity = true;
                //Dust dust16 = Main.dust[num44];
                //Dust dust334 = dust16;
                //dust334.velocity *= 3f;
            }
            for (int num45 = 0; num45 < num3; num45++)
            {
                //int num46 = Gore.NewGore(base.position + new Vector2((float)(base.width * Main.rand.Next(100)) / 100f, (float)(base.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(61, 64));
                //Main.gore[num46].position = base.Center + Vector2.UnitY.RotatedByRandom(3.1415927410125732) * (float)Main.rand.NextDouble() * base.width / 2f;
                //Gore gore8 = Main.gore[num46];
                //Gore gore64 = gore8;
                //gore64.position -= Vector2.One * 16f;
                //if (Main.rand.Next(2) == 0)
                //{
                 //   Main.gore[num46].position.Y -= 30f;
                //}
                //gore8 = Main.gore[num46];
                //gore64 = gore8;
                //gore64.velocity *= 0.3f;
                //Main.gore[num46].velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                //Main.gore[num46].velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
            }
            #endregion

            return false;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
