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
using System.Runtime.CompilerServices;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Misc
{
    
    public class MedusaHead : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.MedusaHead);
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
    public class MedusaHeadHeldProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.MedusaHead);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 12;
            previousRotations.Add(projectile.rotation);
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);


            float timeForPopInAnim = 25;
            float animProgress = Math.Clamp((timer + 7) / timeForPopInAnim, 0f, 1f);

            projectile.scale = 0.5f + MathHelper.Lerp(0f, 0.5f, Easings.easeInOutBack(animProgress, 1f, 3f));
            drawAlpha = 1f;// Math.Clamp(MathHelper.Lerp(drawAlpha, 1.25f, 0.08f), 0f, 1f);

            timer++;

            return base.PreAI(projectile);
        }

        float drawAlpha = 0f;
        float drawScale = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {            
            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects se = Main.player[projectile.owner].direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;
                    float size = projectile.scale * progress;

                    Color col = Color.Gold * progress * projectile.Opacity * drawAlpha;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition;

                    Main.EntitySpriteDraw(vanillaTex, AfterImagePos, sourceRectangle, col with { A = 0 } * 0.15f,
                            previousRotations[i], TexOrigin, size, se);

                }

            }

            //Border
            for (int i = 0; i < 4; i++)
            {
                float dist = 1.25f;

                Vector2 offset = new Vector2(dist, 0f).RotatedBy(MathHelper.PiOver2 * i);
                Vector2 offsetDrawPos = drawPos + offset.RotatedBy(Main.timeForVisualEffects * 0.05f * projectile.direction);

                Main.EntitySpriteDraw(vanillaTex, offsetDrawPos, sourceRectangle,
                    Color.Gold with { A = 0 } * drawAlpha * 1.25f, projectile.rotation, TexOrigin, projectile.scale * 1f, se);
            }

            //We MUST return true here because otherwise the rays dont ever run PreDraw for some reason
            return true;

            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, lightColor * projectile.Opacity * drawAlpha, projectile.rotation, TexOrigin, projectile.scale * drawScale, se);

            //if (projectile.ai[0] > 0)
                //Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2Circular(5f, 5f), sourceRectangle, Color.Gold with { A = 0 } * 0.5f * projectile.Opacity * drawAlpha, projectile.rotation, TexOrigin, projectile.scale * drawScale, se);

            return false;
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            return base.PreKill(projectile, timeLeft);
        }


    }

    public class MedusaRayShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.type == ProjectileID.MedusaHeadRay;
        }

        float overallAlpha = 1f;
        float overallWidth = 0.15f;

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
                storedVelocity = projectile.velocity;

            projectile.velocity = Vector2.Zero;
            

            overallWidth = Math.Clamp(MathHelper.Lerp(overallWidth, 1.5f, 0.05f), 0f, 1f);

            timer++;
            return base.PreAI(projectile);
        }

        Vector2 storedVelocity = Vector2.Zero;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (overallWidth == 0)
                return false;
            
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
            {
                DrawRay(projectile, false);
            });
            DrawRay(projectile, true);

            return false;
        }

        public void DrawRay(Projectile projectile, bool giveUp = false)
        {
            if (giveUp)
                return;

            float easedWidth = Easings.easeInOutBack(overallWidth, 0f, 2f);

            Texture2D beam = Mod.Assets.Request<Texture2D>("Assets/Pixel/FlareLineHalf").Value;

            Vector2 startPosition = projectile.Center;
            Vector2 goalPosition = startPosition + storedVelocity;
            Vector2 drawPosition = startPosition - Main.screenPosition;

            float rot = storedVelocity.ToRotation();
            Vector2 origin = new Vector2(0f, beam.Height / 2f);

            float distance = storedVelocity.Length();
            float XScale = (distance / (float)beam.Width) * projectile.scale * 1.5f;
            float YScale = easedWidth * projectile.scale;

            Vector2 scale = new Vector2(XScale, YScale);
            Vector2 scale2 = new Vector2(XScale, YScale * 0.65f);
            Vector2 scale3 = new Vector2(XScale, YScale * 0.25f);

            Vector2 scale4 = new Vector2(XScale, YScale * 1.25f);

            Main.EntitySpriteDraw(beam, drawPosition, null, Color.Gold with { A = 0 } * 0.15f, rot, origin, scale4, SpriteEffects.None);

            Main.EntitySpriteDraw(beam, drawPosition + Main.rand.NextVector2Circular(5f, 5f), null, Color.Orange with { A = 0 } * 0.5f, rot, origin, scale, SpriteEffects.None);
            Main.EntitySpriteDraw(beam, drawPosition + Main.rand.NextVector2Circular(3f, 3f), null, Color.LightGoldenrodYellow with { A = 0 } * 0.75f, rot, origin, scale2, SpriteEffects.None);
            Main.EntitySpriteDraw(beam, drawPosition, null, Color.White with { A = 0 }, rot, origin, scale3, SpriteEffects.None);

        }
    }

}
