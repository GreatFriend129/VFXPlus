using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;
using VFXPlus.Content.Projectiles;



namespace VFXPlus.Content.Weapons.Melee.Hardmode.Spears
{
    
    public class TitantiumSpearItemOverride : GlobalItem 
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item item, bool lateInstatiation)
        {
            return lateInstatiation && (item.type == ItemID.TitaniumTrident);
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void SetDefaults(Item entity)
        {
            entity.UseSound = SoundID.Item1 with { Volume = 0f };
            base.SetDefaults(entity);
        }

    }

    public class TitaniumSpearProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.TitaniumTrident);
        }

        int timer = 0;

        float fakeRot = 0f;
        Vector2 fakePosition = Vector2.Zero;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 10;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(fakePosition);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Player owner = Main.player[projectile.owner];

            float armRotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;


            int totalDuration = owner.itemAnimationMax;
            float totalProgress = (float)timer / totalDuration;


            float tipDistance = 0f;


            Vector2 offset = new Vector2(0f, 4f) * 0f;

            //Stab Out
            if (totalProgress < 0.5f)
            {
                float stabProg = totalProgress * 2f;

                float easedStabProg = Easings.easeInPow(stabProg, 2f);
                //float easedStabProg = Easings.easeInOutBack(stabProg, 2f, 0f);


                if (easedStabProg < 0.5f)
                {
                    int side = projectile.direction;

                    armRotation += (MathHelper.Pi + -0.5f * projectile.direction) - MathHelper.PiOver2 * Easings.easeInQuad(Utils.GetLerpValue(0f, 0.5f, easedStabProg, true)) * projectile.direction;
                    //armRotation

                    if (easedStabProg < 0.165f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                    else if (easedStabProg < 0.33f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                    else
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);

                    /*
                    if (easedStabProg < 0.125f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
                    else if (easedStabProg < 0.25f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                    else if (easedStabProg < 0.375f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                    else
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
                    */

                    //offset = new Vector2(0f, -4f);
                }
                else
                {

                    if (easedStabProg < 0.625f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
                    else if (easedStabProg < 0.75f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                    else if (easedStabProg < 0.875f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                    else
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);

                }


                /*
                if (easedStabProg < 0.25f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
                else if (easedStabProg < 0.5f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                else if (easedStabProg < 0.75f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                else
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
                */

                tipDistance = MathHelper.Lerp(10f, 140f, easedStabProg);

            }
            //Retract
            else
            {
                float retractProg = (totalProgress - 0.5f) * 2f;
                float easedRetractProg = Easings.easeInSine(retractProg);


                tipDistance = MathHelper.Lerp(140f, 70f, easedRetractProg);


                if (easedRetractProg < 0.25f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
                else if (easedRetractProg < 0.5f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                else if (easedRetractProg < 0.75f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                else
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
            }

            fakePosition = owner.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * tipDistance + offset;

            float fadeInTime = Math.Clamp((timer + 2f) / 17f, 0f, 1f);
            overallScale = 1f;// 0.35f + Easings.easeInOutBack(fadeInTime, 0f, 1f) * 0.65f;

            timer++;
            return true;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        float overallScale = 1f;
        float overallAlpha = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D VanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = fakePosition - Main.screenPosition;

            Vector2 origin = new Vector2(20f, 20f);

            float drawRot = projectile.direction == 1 ? MathHelper.PiOver2 : 0f;
            SpriteEffects SE = SpriteEffects.None;// projectile.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            for (int i = 220; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float colorProg = (progress * 4f) % 1f;
                Color col = Color.DeepPink;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(VanillaTex, AfterImagePos, null, Color.Silver with { A = 50 } * 1f * progress,
                       previousRotations[i] + drawRot, origin, projectile.scale * overallScale * Easings.easeOutQuad(progress), SpriteEffects.None);
            }

            Main.spriteBatch.Draw(VanillaTex, drawPos, null, lightColor * overallAlpha, projectile.rotation + drawRot, origin, projectile.scale * overallScale, SE, 0f);

            return false;
        }



        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

    }


    public class CobaltSpearProjOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.CobaltNaginata);
        }

        int timer = 0;

        float fakeRot = 0f;
        Vector2 fakePosition = Vector2.Zero;
        public override bool PreAI(Projectile projectile)
        {
            int trailCount = 10;
            previousRotations.Add(projectile.rotation);
            previousPositions.Add(fakePosition);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Player owner = Main.player[projectile.owner];

            float armRotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;


            int totalDuration = owner.itemAnimationMax;
            float totalProgress = (float)timer / totalDuration;


            float tipDistance = 0f;


            Vector2 offset = new Vector2(0f, 4f) * 0f;

            //Stab Out
            if (totalProgress < 0.5f)
            {
                float stabProg = totalProgress * 2f;

                float easedStabProg = Easings.easeInPow(stabProg, 3f);
                //float easedStabProg = Easings.easeInOutBack(stabProg, 2f, 0f);

                
                if (easedStabProg < 0.5f)
                {
                    int side = projectile.direction;

                    armRotation += (MathHelper.Pi + -0.6f /*0.5 0.3*/ * projectile.direction) - MathHelper.PiOver2 * Easings.easeInQuad(Utils.GetLerpValue(0f, 0.5f, easedStabProg, true)) * projectile.direction;
                    //armRotation

                    /*
                    if (easedStabProg < 0.165f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                    else if (easedStabProg < 0.33f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                    else
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
                    */

                    
                    if (easedStabProg < 0.125f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
                    else if (easedStabProg < 0.25f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                    else if (easedStabProg < 0.375f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                    else
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);


                    
                    if (easedStabProg < 0.165f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                    else if (easedStabProg < 0.33f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                    else
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
                    

                    //offset = new Vector2(0f, -4f);
                }
                else
                {
                    
                    if (easedStabProg < 0.625f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
                    else if (easedStabProg < 0.75f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                    else if (easedStabProg < 0.875f)
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                    else
                        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
                    
                }
                

                /*
                if (easedStabProg < 0.25f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
                else if (easedStabProg < 0.5f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                else if (easedStabProg < 0.75f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                else
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
                */

                tipDistance = MathHelper.Lerp(30f, 160f, easedStabProg);

            }
            //Retract
            else
            {
                float retractProg = (totalProgress - 0.5f) * 2f;
                float easedRetractProg = Easings.easeInQuad(retractProg);


                tipDistance = MathHelper.Lerp(160f, 65f, easedRetractProg);


                if (easedRetractProg < 0.25f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
                else if (easedRetractProg < 0.5f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRotation);
                else if (easedRetractProg < 0.75f)
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, armRotation);
                else
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, armRotation);
            }

            fakePosition = owner.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * tipDistance + offset;

            float fadeInTime = Math.Clamp((timer + 2f) / 17f, 0f, 1f);
            overallScale = 1f;// 0.35f + Easings.easeInOutBack(fadeInTime, 0f, 1f) * 0.65f;

            timer++;
            return true;
        }

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();

        float overallScale = 1f;
        float overallAlpha = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Texture2D VanillaTex = TextureAssets.Projectile[projectile.type].Value;
            Vector2 drawPos = fakePosition - Main.screenPosition;

            Vector2 origin = projectile.direction == 1 ? new Vector2(VanillaTex.Width, 0f) : new Vector2(0f, 0f);

            float drawRot = projectile.direction == 1 ? 0f : 0f;
            SpriteEffects SE = projectile.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            for (int i = 220; i < previousRotations.Count; i++)
            {
                float progress = (float)i / previousRotations.Count;

                float colorProg = (progress * 4f) % 1f;

                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(VanillaTex, AfterImagePos, null, Color.DodgerBlue with { A = 50 } * 1f * progress,
                       previousRotations[i] + drawRot, origin, projectile.scale * overallScale * Easings.easeOutQuad(progress), SpriteEffects.None);
            }

            Main.spriteBatch.Draw(VanillaTex, drawPos, null, lightColor * overallAlpha, projectile.rotation + drawRot, origin, projectile.scale * overallScale, SE, 0f);

            return false;
        }



        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

    }
}
