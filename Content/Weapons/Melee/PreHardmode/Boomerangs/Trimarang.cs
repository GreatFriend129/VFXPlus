using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Weapons.Melee.PreHardmode.Boomerangs
{
    
    public class TrimarangItemOverride : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.Trimarang) && false;
        }

        public override void SetDefaults(Item entity)
        {
            //entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity); 
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //return false;
            return true;
        }

    }
    public class TrimarangProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public List<float> previousRotations = new List<float>();

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.Trimarang);
        }

        public List<Vector2> previousPositions = new List<Vector2>();

        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 14; //18
            previousPositions.Add(projectile.Center);
            previousRotations.Add(projectile.velocity.ToRotation());

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);
            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);


            float fadeInTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            overallScale = Easings.easeInOutHarsh(fadeInTime);

            //float fadeInAlphaTime = Math.Clamp((timer + 4f) / 12f, 0f, 1f); //4 |12
            //overallAlpha = Easings.easeInOutHarsh(fadeInTime);

            //visualRotation = projectile.rotation;
            visualRotation += 0.45f * projectile.direction * overallScale;

            timer++;
            return true;
        }

        float visualRotation = 0f;
        float overallAlpha = 1f;
        float overallScale = 0f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawSpinningUnder(projectile, false); 
            });
            DrawSpinningUnder(projectile, true);


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(vanillaTex, drawPos, null, lightColor * overallAlpha, visualRotation, vanillaTex.Size() / 2f, projectile.scale * overallScale, SpriteEffects.None);

            return false;

        }

        public void DrawSpinningUnder(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = projectile.Center - Main.screenPosition;

            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;
            Main.EntitySpriteDraw(Orb, drawPos, null, Color.Blue with { A = 0 } * 0.2f, 0f, Orb.Size() / 2f, 1f * projectile.scale * overallScale, SpriteEffects.None);

            //Trail
            Texture2D trailTex = CommonTextures.Flare.Value;
            for (int i = 0; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                Vector2 spikeScale = new Vector2(1f, 0.75f * progress) * progress;
                Color spikeCol = Color.Red * 0.15f * overallAlpha * Easings.easeInQuad(progress);

                Main.EntitySpriteDraw(trailTex, previousPositions[i] - Main.screenPosition, null, spikeCol with { A = 0 } * overallAlpha * progress,
                        previousRotations[i], trailTex.Size() / 2f, projectile.scale * overallScale * spikeScale, SpriteEffects.None);
            }


            //
            //Texture2D TrailTex = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpikeHalf").Value;

            //Vector2 trailScale1 = new Vector2(0.25f * projectile.velocity.Length(), 0.5f);
            //Vector2 trailScale2 = new Vector2(0.3f * projectile.velocity.Length(), 0.25f);
            //float trailRot = projectile.velocity.ToRotation();
            //Vector2 origin = new Vector2(TrailTex.Width, TrailTex.Height / 2f);

            //Main.EntitySpriteDraw(TrailTex, drawPos, null, Color.Red with { A = 0 } * overallAlpha * 0.5f, trailRot, origin, trailScale1, SpriteEffects.None);
            //Main.EntitySpriteDraw(TrailTex, drawPos, null, Color.White with { A = 0 } * overallAlpha, trailRot, origin, trailScale2, SpriteEffects.None);


            //Ring
            Texture2D RingTex = Mod.Assets.Request<Texture2D>("Assets/Slash/FadeRingB").Value; //A |0.75f time|

            float ringRot = (float)Main.timeForVisualEffects * 0.6f * projectile.direction;

            float ringScale = 0.135f * projectile.scale * overallScale; //125
            float ringAlpha = overallAlpha * 1f;

            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.DodgerBlue with { A = 0 } * ringAlpha, ringRot, RingTex.Size() / 2f, ringScale, SpriteEffects.None);
            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Blue with { A = 0 } * ringAlpha, ringRot + MathHelper.PiOver4, RingTex.Size() / 2f, ringScale * 1.1f, SpriteEffects.None);
            Main.EntitySpriteDraw(RingTex, drawPos, null, Color.Red with { A = 0 } * ringAlpha, ringRot + MathHelper.PiOver2, RingTex.Size() / 2f, ringScale * 0.5f, SpriteEffects.None);
        }

        public override bool PreKill(Projectile projectile, int timeLeft)
        {

            return base.PreKill(projectile, timeLeft);
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            //Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);

            return base.OnTileCollide(projectile, oldVelocity);
        }


    }

}
