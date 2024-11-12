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


namespace VFXPlus.Content.Weapons.Magic.Hardmode.Staves
{
    
    public class CrystalSerpent : GlobalItem 
    {
        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.CrystalSerpent);
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
    public class CrystalSerpentShotOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CrystalPulse);
        }

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 30; ///12
            previousRotations.Add(projectile.velocity.ToRotation());
            previousPostions.Add(projectile.Center);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPostions.Count > trailCount)
                previousPostions.RemoveAt(0);

            bool addInBetween = true;
            if (addInBetween)
            {
                previousRotations.Add(projectile.velocity.ToRotation());
                previousPostions.Add(projectile.Center + projectile.velocity * 0.5f);

                if (previousRotations.Count > trailCount)
                    previousRotations.RemoveAt(0);

                if (previousPostions.Count > trailCount)
                    previousPostions.RemoveAt(0);
            }

            //projectile.velocity.Y += 0.62f;

            timer++;

            return false;
            return base.PreAI(projectile);
        }

        float fadeInAlpha = 0f;
        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPostions = new List<Vector2>();
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
            {
                Draw(projectile);
            });

            //Draw(projectile);

            //Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.SkyBlue with { A = 0 } * overallAlpha, projectile.rotation, TexOrigin, projectile.scale * overallAlpha, SpriteEffects.None);

            return false;
        }

        public void Draw(Projectile projectile)
        {
            Texture2D line = Mod.Assets.Request<Texture2D>("Assets/Pixel/Flare").Value;

            //After-Image
            if (previousRotations != null && previousPostions != null)
            {
                for (int i = 0; i < previousRotations.Count; i++)
                {
                    float progress = (float)i / previousRotations.Count;

                    float sineScale = MathF.Sin((float)Main.timeForVisualEffects * 0.25f) * 0.1f;

                    Vector2 AfterImagePos = previousPostions[i] - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f); //3f

                    float startScale = projectile.scale + sineScale;

                    Color col = Color.Lerp(Color.HotPink, Color.DeepPink, 0.5f);

                    float easedFadeValue = progress * progress;


                    Vector2 lineScale = new Vector2(1.25f, 0.3f + 0.4f * progress); //
                    Vector2 lineScale2 = new Vector2(1.25f, 0.05f + 0.05f * progress); //0.1f 0.2f

                    //Black
                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.Black * 0.4f * easedFadeValue,
                        projectile.velocity.ToRotation(), line.Size() / 2f, lineScale2 * projectile.scale, SpriteEffects.None);

                    Main.EntitySpriteDraw(line, AfterImagePos, null, col with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale * startScale, SpriteEffects.None);


                    Main.EntitySpriteDraw(line, AfterImagePos, null, Color.White with { A = 0 } * 0.5f * easedFadeValue,
                        previousRotations[i], line.Size() / 2f, lineScale2 * startScale, SpriteEffects.None);

                }

            }

        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
