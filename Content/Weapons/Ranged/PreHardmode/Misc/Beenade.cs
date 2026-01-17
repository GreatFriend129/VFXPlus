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
using rail;


namespace VFXPlus.Content.Weapons.Ranged.PreHardmode.Misc
{
    public class BeenadeOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Beenade);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {            
            int trailCount = 20; // 14
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            float fadeInTime = Math.Clamp((timer + 9f) / 25f, 0f, 1f);
            overallScale = Easings.easeInOutBack(fadeInTime, 0f, 1.5f);

            timer++;
            return base.PreAI(projectile);
        }

        float overallAlpha = 1f;
        float overallScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = vanillaTex.Size() / 2f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    //Start End
                    Color col = Color.Gold * progress * progress * progress;

                    float size1 = (0.5f + (progress * 0.5f)) * projectile.scale;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * progress * 0.35f, 
                        previousRotations[i], TexOrigin, size1 * overallScale, SE);
                }
            }

            //Border
            for (int i = 0; i < 3; i++)
            {
                Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle,
                    Color.Yellow with { A = 0 } * 1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f * overallScale, SE);
            }

            //MainTex
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor, projectile.rotation, TexOrigin, projectile.scale * overallScale, SE);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            for (int i = 0; i < 19; i++)
            {
                float prog = (float)i / 19f;

                float proggg = Main.rand.NextFloat();
                Color col = Color.Lerp(Color.Orange with { A = 0 } * 1f, Color.Black * 0.5f, proggg);

                Dust d = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<MediumSmoke>(), Velocity: Main.rand.NextVector2Unit() * Main.rand.NextFloat(0.9f, 3f) * 2f,
                    newColor: col * prog, Scale: Main.rand.NextFloat(0.9f, 1.5f) * 1f);
                d.customData = new MediumSmokeBehavior(Main.rand.Next(15, 23), 0.93f, 0.01f, 0.95f); //12 28

            }

            #region vanillaKill

            SoundEngine.PlaySound(in SoundID.Item14, projectile.position);
            for (int num755 = 0; num755 < 5; num755++) //20
            {
                int num756 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust136 = Main.dust[num756];
                Dust dust334 = dust136;
                dust334.velocity *= 1f;
            }
            /*
            int num757 = Gore.NewGore(null,new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
            Main.gore[num757].velocity.X += 1f;
            Main.gore[num757].velocity.Y += 1f;
            Gore gore34 = Main.gore[num757];
            Gore gore64 = gore34;
            gore64.velocity *= 0.3f;
            num757 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
            Main.gore[num757].velocity.X -= 1f;
            Main.gore[num757].velocity.Y += 1f;
            gore34 = Main.gore[num757];
            gore64 = gore34;
            gore64.velocity *= 0.3f;
            num757 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
            Main.gore[num757].velocity.X += 1f;
            Main.gore[num757].velocity.Y -= 1f;
            gore34 = Main.gore[num757];
            gore64 = gore34;
            gore64.velocity *= 0.3f;
            num757 = Gore.NewGore(null, new Vector2(projectile.position.X, projectile.position.Y), default(Vector2), Main.rand.Next(61, 64));
            Main.gore[num757].velocity.X -= 1f;
            Main.gore[num757].velocity.Y -= 1f;
            gore34 = Main.gore[num757];
            gore64 = gore34;
            gore64.velocity *= 0.3f;
            */
            if (projectile.owner == Main.myPlayer)
            {
                int num758 = Main.rand.Next(15, 25);
                for (int num759 = 0; num759 < num758; num759++)
                {
                    float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
                    float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;
                    Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.position.X, projectile.position.Y, speedX, speedY, Main.player[projectile.owner].beeType(), Main.player[projectile.owner].beeDamage(projectile.damage), Main.player[projectile.owner].beeKB(0f), Main.myPlayer);
                }
            }
            #endregion
            return false;
        }

    }

}
