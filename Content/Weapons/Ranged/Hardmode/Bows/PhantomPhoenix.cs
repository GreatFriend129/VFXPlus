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

            //player.GetModPlayer<HeldBowPlayer>().arrowType = ProjectileID.BoneArrow;
            //player.GetModPlayer<HeldBowPlayer>().bowType = ItemID.Marrow;
            //player.GetModPlayer<HeldBowPlayer>().holdOffset = new Vector2(-2f, 0f);
            //player.GetModPlayer<HeldBowPlayer>().arrowOffset = -10f;
            //player.GetModPlayer<HeldBowPlayer>().arrowPullAmount = 15f;
            //player.GetModPlayer<HeldBowPlayer>().underGlowPower = 0f;
            //player.GetModPlayer<HeldBowPlayer>().underGlowColor = new Color(42, 2, 82);
            return true;
        }

        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame) => UseStyleHelper.BasicBowUseStyle(player);


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
            int trailCount = 40; //18

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

            float fadeInTime = Math.Clamp((timer + 18f) / 45f, 0f, 1f);
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

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                DrawTrail(projectile, false);
            });
            DrawTrail(projectile, true);


            //Trail
            for (int i = 0; i < previousPositions.Count; i++)
            {
                if (i > previousPositions.Count * 0.75f && (i + 1) % 2 == 0)
                {
                    float progress = (float)(i) / previousPositions.Count;

                    Vector2 trailPos = previousPositions[i] - Main.screenPosition;
                    float trailAlpha = Easings.easeOutQuad(progress);
                    Vector2 trailScale = new Vector2(1f, 1f) * overallScale;

                    Main.EntitySpriteDraw(vanillaTex, trailPos, sourceRectangle, Color.OrangeRed with { A = 0 } * trailAlpha * 0.25f, previousRotations[i], TexOrigin, trailScale, SE);
                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                Vector2 offsetPos = drawPos + Main.rand.NextVector2Circular(2f, 2f);
                Main.EntitySpriteDraw(vanillaTex, offsetPos, sourceRectangle, Color.Orange with { A = 0 } * 0.5f, projectile.rotation, TexOrigin, projectile.scale * 1.05f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;

        }

        public void DrawTrail(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Texture2D flare = CommonTextures.Flare.Value;

            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

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

                Color col = Color.Lerp(Color.OrangeRed, Color.Orange, progress) * overallAlpha;

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
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            

            return true;
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
