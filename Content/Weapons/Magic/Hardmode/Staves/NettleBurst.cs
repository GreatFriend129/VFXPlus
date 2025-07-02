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
using VFXPlus.Common.Drawing;
using Terraria.Graphics;
using VFXPlus.Content.Weapons.Magic.Hardmode.Staves;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using VFXPlus.Content.Weapons.Ranged.Ammo.Bullets;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    //Literally just so that the proj alternate color
    public class NettleBurstPlayer : ModPlayer
    {
        public bool makeRed = false;
    }

    public class NettleBurstBaseOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NettleBurstLeft || entity.type == ProjectileID.NettleBurstRight) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NettleBurstToggle;
        }

        bool isRed = false;

        public float scale = 0f;
        public float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && projectile.ai[1] == 0)
            {
                projectile.ai[2] = Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed ? 1 : -1;
                Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed = projectile.ai[2] != 1;
            }
            isRed = projectile.ai[2] == 1;

            if (timer == 0 && projectile.ai[1] % 2 == 0)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.07f); //14
                float pitch2 = -0.4f + (projectile.ai[1] * 0.07f); //14

                SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .025f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; 
                SoundEngine.PlaySound(style, projectile.Center);

                SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.07f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                SoundEngine.PlaySound(style2, projectile.Center);
            }


            float timeForPopInAnim = 25;
            float animProgress = Math.Clamp((timer + 7) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);

            if (timer >= 4 && timer <= 11 && timer % 2 == 0) //7
            {
                for (int i = 0; i < 1; i++) //5 + Main.rand.Next(1, 3)
                {
                    Color col = isRed ? Color.Red : Color.ForestGreen;

                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(7f, 7f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: col * 1f, Scale: Main.rand.NextFloat(0.2f, 0.25f) * projectile.scale * 1.5f); //3

                    p.velocity += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * -1f;

                    //int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<GlowPixelCross>(), newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f) * projectile.scale);
                }

            }

            if (timer == 7)
            {

                for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++)
                {
                    Color col = Color.Brown * 2f;

                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(5f, 5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, DustID.WoodFurniture, vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: Color.LightPink, Scale: Main.rand.NextFloat(1f, 1.3f)); //3

                    p.alpha = 200;

                    p.noGravity = true;
                    Dust dust23 = p;
                    Dust dust3 = dust23;
                    dust3.velocity *= 0.5f;
                }
            }

            timer++;

            #region vanillaAI
            if (Main.netMode != 2 && projectile.ai[1] == 0f && projectile.localAI[0] == 0f)
            {
                projectile.localAI[0] = 1f;
                SoundStyle legacySoundStyle = SoundID.Item8;
                if (projectile.type == 494)
                {
                    legacySoundStyle = SoundID.Item101;
                }
                SoundEngine.PlaySound(in legacySoundStyle, projectile.Center);
            }
            projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
            if (projectile.ai[0] == 0f)
            {
                if (projectile.type >= 150 && projectile.type <= 152 && projectile.ai[1] == 0f && projectile.alpha == 255 && Main.rand.Next(2) == 0)
                {
                    projectile.type++;
                    projectile.netUpdate = true;
                }
                projectile.alpha -= 50;
                if (projectile.type >= 150 && projectile.type <= 152)
                {
                    projectile.alpha -= 25;
                }
                else if (projectile.type == 493 || projectile.type == 494)
                {
                    projectile.alpha -= 50;
                }
                if (projectile.alpha > 0)
                {
                    return false;
                }
                projectile.alpha = 0;
                projectile.ai[0] = 1f;
                if (projectile.ai[1] == 0f)
                {
                    projectile.ai[1] += 1f;
                    projectile.position += projectile.velocity * 1f;
                }
                if (projectile.type == 7 && Main.myPlayer == projectile.owner)
                {
                    int num755 = projectile.type;
                    if (projectile.ai[1] >= 6f)
                    {
                        num755++;
                    }
                    int num766 = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + projectile.velocity.X + (float)(projectile.width / 2), projectile.position.Y + projectile.velocity.Y + (float)(projectile.height / 2), projectile.velocity.X, projectile.velocity.Y, num755, projectile.damage, projectile.knockBack, projectile.owner);
                    Main.projectile[num766].damage = projectile.damage;
                    Main.projectile[num766].ai[1] = projectile.ai[1] + 1f;
                    NetMessage.SendData(27, -1, -1, null, num766);
                }
                else if (projectile.type == 494 && Main.myPlayer == projectile.owner)
                {
                    int num777 = projectile.type;
                    if (projectile.ai[1] >= (float)(7 + Main.rand.Next(2)))
                    {
                        num777--;
                    }
                    int num788 = projectile.damage;
                    float num799 = projectile.knockBack;
                    if (num777 == 493)
                    {
                        num788 = (int)((double)projectile.damage * 1.25);
                        num799 = projectile.knockBack * 1.25f;
                    }
                    int number = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + projectile.velocity.X + (float)(projectile.width / 2),
                        projectile.position.Y + projectile.velocity.Y + (float)(projectile.height / 2), projectile.velocity.X, projectile.velocity.Y, num777, num788, num799, projectile.owner, 0f, projectile.ai[1] + 1f);
                    NetMessage.SendData(27, -1, -1, null, number);
                }
                else if ((projectile.type == 150 || projectile.type == 151) && Main.myPlayer == projectile.owner)
                {
                    int num810 = projectile.type;
                    if (projectile.type == 150)
                    {
                        num810 = 151;
                    }
                    else if (projectile.type == 151)
                    {
                        num810 = 150;
                    }
                    if (projectile.ai[1] >= 10f && projectile.type == 151)
                    {
                        num810 = 152;
                    }
                    int num821 = Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X + projectile.velocity.X + (float)(projectile.width / 2), 
                        projectile.position.Y + projectile.velocity.Y + (float)(projectile.height / 2), projectile.velocity.X, projectile.velocity.Y, num810, projectile.damage, projectile.knockBack, projectile.owner);
                    Main.projectile[num821].damage = projectile.damage;
                    Main.projectile[num821].ai[1] = projectile.ai[1] + 1f;

                    //Added
                    Main.projectile[num821].ai[2] = projectile.ai[2];

                    NetMessage.SendData(27, -1, -1, null, num821);
                }
                return false;
            }
            if (projectile.alpha < 170 && projectile.alpha + 5 >= 170)
            {
                if (projectile.type >= 150 && projectile.type <= 152)
                {
                    for (int num832 = 0; num832 < 8; num832++)
                    {
                        int num843 = Dust.NewDust(projectile.position, projectile.width, projectile.height, 7, projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 200, default(Color), 1.3f);
                        Main.dust[num843].noGravity = true;
                        Dust dust41 = Main.dust[num843];
                        Dust dust212 = dust41;
                        dust212.velocity *= 0.5f;
                    }
                }
                else if (projectile.type == 493 || projectile.type == 494)
                {
                    for (int num855 = 0; num855 < 8; num855++)
                    {
                        int num866 = Dust.NewDust(projectile.position, projectile.width, projectile.height, Main.rand.Next(68, 71), projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 200, default(Color), 1.3f);
                        Main.dust[num866].noGravity = true;
                        Dust dust42 = Main.dust[num866];
                        Dust dust212 = dust42;
                        dust212.velocity *= 0.5f;
                    }
                }
                else
                {
                    for (int num877 = 0; num877 < 3; num877++)
                    {
                        Dust.NewDust(projectile.position, projectile.width, projectile.height, 18, projectile.velocity.X * 0.025f, projectile.velocity.Y * 0.025f, 170, default(Color), 1.2f);
                    }
                    Dust.NewDust(projectile.position, projectile.width, projectile.height, 14, 0f, 0f, 170, default(Color), 1.1f);
                }
            }
            if (projectile.type >= 150 && projectile.type <= 152)
            {
                projectile.alpha += 3;
            }
            else if (projectile.type == 493 || projectile.type == 494)
            {
                projectile.alpha += 4;
            }
            else
            {
                projectile.alpha += 5;
            }
            if (projectile.alpha >= 255)
            {
                projectile.Kill();
            }
            #endregion

            return false;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;// + new Vector2(3f, 2f).RotatedBy(projectile.rotation);

            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                for (int num163 = 0; num163 < 4; num163++)
                {
                    Vector2 offset = projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)num163) * 4f;
                    offset += offset.RotatedBy(Main.timeForVisualEffects * 0.2f * projectile.direction) * 0.25f;

                    Color col = isRed ? Color.Red : Color.Green;

                    Main.EntitySpriteDraw(vanillaTex, drawPos + offset, null,
                        col with { A = 0 } * projectile.Opacity * alpha, projectile.rotation, vanillaTex.Size() / 2f, vec2Scale, 0f);
                }
            });

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2f, vec2Scale, SpriteEffects.None);

            return false;            
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

    }

    public class NettleBurstTipShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.NettleBurstEnd) && ModContent.GetInstance<VFXPlusToggles>().MagicToggle.NettleBurstToggle;
        }

        bool isRed = false;

        float scale = 0;
        float alpha = 1f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0 && projectile.ai[1] == 0)
            {
                projectile.ai[2] = Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed ? 1 : -1;
                Main.player[projectile.owner].GetModPlayer<NettleBurstPlayer>().makeRed = projectile.ai[2] != 1;
            }
            isRed = projectile.ai[2] == 1;


            if (timer == 4)
            {
                float pitch = 0.2f + (projectile.ai[1] * 0.12f);
                float pitch2 = -0.4f + (projectile.ai[1] * 0.12f);

                //SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Metallic/joker_stab2") with { Volume = .035f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, };
                //SoundEngine.PlaySound(style, projectile.Center);

                //SoundStyle style2 = new SoundStyle("Terraria/Sounds/Item_153") with { Volume = 0.1f, Pitch = pitch2, PitchVariance = .05f, MaxInstances = -1, }; //153\156
                //SoundEngine.PlaySound(style2, projectile.Center);
            }


            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f); //15 60

            scale = 0f + MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, in_tensity: 0f, out_tensity: 2f));

            if (scale == 1f)
                alpha = Math.Clamp(MathHelper.Lerp(alpha, -0.5f, 0.05f), 0f, 1f);

            if (timer >= 4 && timer <= 11 && timer % 2 == 0) //7
            {
                for (int i = 0; i < 1; i++) //5 + Main.rand.Next(1, 3)
                {
                    Color col = isRed ? Color.Red : Color.ForestGreen;

                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    Vector2 posOffset = Main.rand.NextVector2Circular(7f, 7f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + posOffset, ModContent.DustType<GlowPixelCross>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: col * 1f, Scale: Main.rand.NextFloat(0.2f, 0.25f) * projectile.scale * 1.5f); //3

                    p.velocity += (projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * -1f;

                    //int d = Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<GlowPixelCross>(), newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.45f) * projectile.scale);
                }
            }

            timer++;
            return base.PreAI(projectile);
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Vector2 vec2Scale = new Vector2(scale * projectile.scale, projectile.scale);


            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                for (int num163 = 0; num163 < 4; num163++)
                {
                    Vector2 offset = projectile.rotation.ToRotationVector2().RotatedBy((float)Math.PI / 2f * (float)num163) * 4f;
                    offset += offset.RotatedBy(Main.timeForVisualEffects * 0.2f * projectile.direction) * 0.25f;

                    Color col = isRed ? Color.Red : Color.Green;

                    Main.EntitySpriteDraw(vanillaTex, drawPos + offset, null,
                        col with { A = 0 } * projectile.Opacity * alpha, projectile.rotation, vanillaTex.Size() / 2f, vec2Scale, 0f);
                }
            });

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * projectile.Opacity, projectile.rotation, vanillaTex.Size() / 2, vec2Scale, SpriteEffects.None);
            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            //return false;
            return base.PreKill(projectile, timeLeft);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }
    }
}
