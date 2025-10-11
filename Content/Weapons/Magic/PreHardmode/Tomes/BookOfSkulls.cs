using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;


namespace VFXPlus.Content.Weapons.Magic.PreHardmode.Tomes
{
    
    public class BookOfSkulls : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.BookofSkulls) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BookOfSkullsToggle;
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundStyle stylees = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_117") with { Pitch = .45f, PitchVariance = .25f, Volume = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(stylees, player.Center);

            SoundStyle style2 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Dd2_book_staff_cast_0") with { Volume = 0.2f, Pitch = -0.5f, PitchVariance = 0.2f, MaxInstances = -1 };
            SoundEngine.PlaySound(style2, player.Center);

            SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_8") with { Volume = 0.85f, PitchVariance = 0.1f, Pitch = .05f, MaxInstances = -1, }; 
            SoundEngine.PlaySound(style, player.Center);

            return true;
        }

    }
    public class BookOfSkullsShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.BookOfSkullsSkull) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.BookOfSkullsToggle;
        }


        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 50;
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            //Add a second position to the trail
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            //FadeIn
            float progress = Math.Clamp((timer + 5) / 40f, 0f, 1f); //timer / 50
            fadeInAlpha = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(progress, 0f, 0f));
            scaleFadeIn = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(progress, 0f, 2f));


            //Fire Particles
            if (timer % 2 == 0 && timer > 10)
            {
                Vector2 VelDir = projectile.velocity.SafeNormalize(Vector2.UnitX);

                Vector2 dustVel = (VelDir * Main.rand.NextFloat(1f, 4.1f)).RotateRandom(0.3f);

                FireParticle fire = new FireParticle(projectile.Center, -dustVel * 1.75f, 1f, Color.Lerp(Color.OrangeRed, Color.Orange, 0f), colorMult: 1f, bloomAlpha: 1f, AlphaFade: 0.92f); //colMult3 || alphafade .92
                fire.scaleFadePower = 1.04f; //1.06 look sweet at higher proj speed
                fire.renderLayer = RenderLayer.UnderProjectiles;
                ShaderParticleHandler.SpawnParticle(fire);
            }

            #region vanillaAI dear god
            projectile.frame++;
            if (projectile.frame > 2)
            {
                projectile.frame = 0;
            }

            float num94 = (float)Math.Sqrt(projectile.velocity.X * projectile.velocity.X + projectile.velocity.Y * projectile.velocity.Y);
            float num95 = projectile.localAI[0];
            if (num95 == 0f)
            {
                projectile.localAI[0] = num94;
                num95 = num94;
            }
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 25;
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            float num96 = projectile.position.X;
            float num97 = projectile.position.Y;
            float num98 = 300f;
            bool flag5 = false;
            int num99 = 0;
            if (projectile.ai[1] == 0f)
            {
                for (int num101 = 0; num101 < 200; num101++)
                {
                    if (Main.npc[num101].CanBeChasedBy(this) && (projectile.ai[1] == 0f || projectile.ai[1] == (float)(num101 + 1)))
                    {
                        float num102 = Main.npc[num101].position.X + (float)(Main.npc[num101].width / 2);
                        float num103 = Main.npc[num101].position.Y + (float)(Main.npc[num101].height / 2);
                        float num104 = Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num102) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num103);
                        if (num104 < num98 && Collision.CanHit(new Vector2(projectile.position.X + (float)(projectile.width / 2), projectile.position.Y + (float)(projectile.height / 2)), 1, 1, Main.npc[num101].position, Main.npc[num101].width, Main.npc[num101].height))
                        {
                            num98 = num104;
                            num96 = num102;
                            num97 = num103;
                            flag5 = true;
                            num99 = num101;
                        }
                    }
                }
                if (flag5)
                {
                    projectile.ai[1] = num99 + 1;
                }
                flag5 = false;
            }
            if (projectile.ai[1] > 0f)
            {
                int num105 = (int)(projectile.ai[1] - 1f);
                if (Main.npc[num105].active && Main.npc[num105].CanBeChasedBy(this, ignoreDontTakeDamage: true) && !Main.npc[num105].dontTakeDamage)
                {
                    float num106 = Main.npc[num105].position.X + (float)(Main.npc[num105].width / 2);
                    float num107 = Main.npc[num105].position.Y + (float)(Main.npc[num105].height / 2);
                    if (Math.Abs(projectile.position.X + (float)(projectile.width / 2) - num106) + Math.Abs(projectile.position.Y + (float)(projectile.height / 2) - num107) < 1000f)
                    {
                        flag5 = true;
                        num96 = Main.npc[num105].position.X + (float)(Main.npc[num105].width / 2);
                        num97 = Main.npc[num105].position.Y + (float)(Main.npc[num105].height / 2);
                    }
                }
                else
                {
                    projectile.ai[1] = 0f;
                }
            }
            if (!projectile.friendly)
            {
                flag5 = false;
            }
            if (flag5)
            {
                float num244 = num95;
                Vector2 vector19 = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
                float num108 = num96 - vector19.X;
                float num109 = num97 - vector19.Y;
                float num112 = (float)Math.Sqrt(num108 * num108 + num109 * num109);
                num112 = num244 / num112;
                num108 *= num112;
                num109 *= num112;
                int num113 = 8;
                if (projectile.type == 837)
                {
                    num113 = 32;
                }
                projectile.velocity.X = (projectile.velocity.X * (float)(num113 - 1) + num108) / (float)num113;
                projectile.velocity.Y = (projectile.velocity.Y * (float)(num113 - 1) + num109) / (float)num113;
            }

            #endregion

            Color lightingCol = Color.Lerp(Color.Orange, Color.OrangeRed, 0.4f);
            Lighting.AddLight(projectile.Center, lightingCol.ToVector3() * 0.65f);

            timer++;
            return false;
        }

        float fadeInAlpha = 0f;
        float scaleFadeIn = 0f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawVertexTrail(false);
            });

            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects se = projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            //Orb
            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;

            Color orbCol1 = Color.Yellow * 0.75f;
            Color orbCol2 = Color.Orange * 0.525f;
            Color orbCol3 = Color.OrangeRed * 0.375f;

            float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.5f;
            float scale1 = 0.75f;
            float scale2 = 1.6f;
            float scale3 = 2.5f + sineScale;

            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol1 with { A = 0 } * fadeInAlpha * 0.15f, 0f, Glow.Size() / 2f, projectile.scale * scale1 * 0.7f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol2 with { A = 0 } * fadeInAlpha * 0.15f, 0f, Glow.Size() / 2f, projectile.scale * scale2 * 0.7f, SpriteEffects.None);
            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol3 with { A = 0 } * fadeInAlpha * 0.075f, 0f, Glow.Size() / 2f, projectile.scale * scale3 * 0.7f, SpriteEffects.None);

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 4f + ((1f - fadeInAlpha) * 20f);

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.15f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.White with { A = 0 } * fadeInAlpha * 0.75f, projectile.velocity.ToRotation(), TexOrigin, projectile.scale * 1.05f, se);
            }


            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * 1f, projectile.velocity.ToRotation(), TexOrigin, projectile.scale * scaleFadeIn, se);

            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = CommonTextures.Extra_196_Black.Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPostions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            Color StripColor(float progress) => Color.White * (progress * progress * progress);
            float StripWidth(float progress) => 60f * Easings.easeOutQuad(progress) * fadeInAlpha;// * MathF.Sqrt(Utils.GetLerpValue(0f, 0.1f, progress, true));


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue(timer * 0.08f);
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);

            myEffect.Parameters["glowThreshold"].SetValue(0.4f);
            myEffect.Parameters["glowIntensity"].SetValue(2.5f);
            myEffect.Parameters["reps"].SetValue(1f);

            //Black UnderLayer
            myEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.15f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();
            vertexStrip.DrawTrail();

            //Over layer
            myEffect.Parameters["ColorOne"].SetValue(Color.OrangeRed.ToVector3() * 10f);
            myEffect.Parameters["glowThreshold"].SetValue(0.5f);
            myEffect.Parameters["glowIntensity"].SetValue(2.5f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }


        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            int soundID = Main.rand.Next(0, 3);

            SoundStyle style = new SoundStyle("Terraria/Sounds/Custom/dd2_skeleton_death_" + soundID) with { Volume = .5f, Pitch = -.25f, PitchVariance = .35f, MaxInstances = -1 }; 
            SoundEngine.PlaySound(style, projectile.Center);

            return base.PreKill(projectile, timeLeft);
        }


    }

}
