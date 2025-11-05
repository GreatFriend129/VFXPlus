using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;


namespace VFXPlus.Content.QueenBee
{
    public class HittableBee : ModNPC
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.ProjectileNPC[Type] = true;
            
            //NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.NeverDropsResourcePickups[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 10;
            NPC.width = NPC.height = 25; //25
            NPC.damage = 1;

            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.aiStyle = -1;
            NPC.knockBackResist = 0;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }

        public bool isHittable = true;

        public int timer = 0;

        public float overallScale = 0f;
        public float overallAlpha = 1f;

        public bool fadeOut = false;

        public override void AI()
        {
            BaseBeeAI();

            if (timer >= 500)
                fadeOut = true;
        }

        //Things that all
        public void BaseBeeAI()
        {
            if (isHittable)
                NPC.dontTakeDamage = false;
            else
                NPC.dontTakeDamage = true;


            int trailCount = 9;
            previousPositions.Add(NPC.Center);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (timer % 5 == 0)
                NPC.frameCounter = (NPC.frameCounter + 1) % 4;

            float timeForPopInAnim = 30;
            float animProgress = Math.Clamp((timer + 10) / timeForPopInAnim, 0f, 1f);
            overallScale = 0.25f + MathHelper.Lerp(0f, 0.75f, Easings.easeInOutBack(animProgress, 0f, 2f));

            if (fadeOut)
            {
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, -0.5f, 0.08f), 0f, 1f);
                overallScale = overallAlpha;
                if (overallAlpha <= 0)
                    NPC.active = false;
            }
            else
                overallAlpha = Math.Clamp(MathHelper.Lerp(overallAlpha, 1.25f, 0.07f), 0f, 1f);

            NPC.rotation = NPC.velocity.X * 0.05f;

            timer++;
        }

        public override void OnKill()
        {
            for (int num487 = 0; num487 < 6; num487++)
            {
                int num488 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bee, NPC.velocity.X, NPC.velocity.Y, 50);
                Main.dust[num488].noGravity = true;
                Main.dust[num488].scale = 1f;
            }
        }
        

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D Bee = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/BeeSheet").Value;
            //Texture2D BeeBorder = Mod.Assets.Request<Texture2D>("Content/QueenBee/Assets/BeeSheetBorder").Value;

            Vector2 drawPos = NPC.Center - screenPos;
            Rectangle sourceRectangle = Bee.Frame(1, 4, frameY: (int)NPC.frameCounter);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = NPC.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Color betweenRed = Color.Lerp(Color.OrangeRed, Color.Red, 0.85f);

            //Orb
            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;
            Color orbCol1 = isHittable ? Color.DarkGoldenrod : betweenRed;

            Main.EntitySpriteDraw(Glow, drawPos, null, orbCol1 with { A = 0 } * 0.5f * overallAlpha, NPC.rotation, Glow.Size() / 2f, NPC.scale * 0.3f * overallScale, SpriteEffects.None);


            //Trail
            Color afterImageCol = isHittable ? Color.Gold : betweenRed;
            float afterImageAlpha = isHittable ? 0.125f : 0.25f;
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                float size = (NPC.scale * overallScale) - (0.33f - (progress * 0.33f));
                Color col = afterImageCol with { A = 0 } * progress * overallAlpha;


                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(Bee, AfterImagePos, sourceRectangle, col * afterImageAlpha,
                        NPC.rotation, TexOrigin, size, SE);
            }

            //Border
            Color borderCol = isHittable ? Color.White : betweenRed;
            for (int i = 0; i < 4; i++)
            {
                Main.spriteBatch.Draw(Bee, drawPos + Main.rand.NextVector2Circular(2f, 2f), sourceRectangle, borderCol with { A = 0 } * overallAlpha * 1.5f, NPC.rotation, TexOrigin, NPC.scale * overallScale, SE, 0f); //0.3
            }

            //Red Border on non-hittable bees
            //if (!isHittable)
            //    Main.spriteBatch.Draw(BeeBorder, drawPos, sourceRectangle, betweenRed with { A = 0 } * overallAlpha, NPC.rotation, TexOrigin, NPC.scale * overallScale * 0.9f, SE, 0f); //0.3

            //Main Tex
            Main.spriteBatch.Draw(Bee, drawPos, sourceRectangle, drawColor * overallAlpha, NPC.rotation, TexOrigin, NPC.scale * overallScale, SE, 0f); //0.3

            return false;
        }
    }

    public class AsgoreBee : HittableBee
    {
        public Vector2 centerOffset = Vector2.Zero;

        public Vector2 centerPos = Vector2.Zero;

        //How fast the bees more inward
        public float inwardSpeed = -0.5f;

        public float rotSpeed = 0f;

        float rot = 0f;
        float distance = 500;
        public override void AI()
        {
            if (timer == 0)
            {
                distance = centerOffset.Length();
                rot = centerOffset.ToRotation();

                NPC.Center = centerPos + centerOffset;
            }

            BaseBeeAI();

            NPC.Center = centerPos + (rot.ToRotationVector2() * distance);
            rot = rot + rotSpeed;

            distance -= inwardSpeed;

            //Doing this for rotation, velocity doesn't actually matter here
            NPC.velocity = new Vector2(3f * (rot.ToRotationVector2().X > 0 ? -1f : 1f), 0f);

            if (distance <= 15f)
            {
                fadeOut = true;
            }
        }
    }

    public class OphanaimBee : HittableBee
    {
        //What direction the bees move after stoping
        public Vector2 secondDirection = Vector2.Zero;

        public float secondSpeed = 10f;

        public float velFadeAmount = 0.98f;

        public int timeBeforeSecondDirection = 60;

        public override void AI()
        {
            if (timer < timeBeforeSecondDirection)
            {
                NPC.velocity *= velFadeAmount;
            }
            else if (timer >= timeBeforeSecondDirection)
            {
                if (timer == timeBeforeSecondDirection)
                    NPC.velocity = secondDirection;

                if (NPC.velocity.Length() < secondSpeed)
                    NPC.velocity *= 1.02f;
            }
            BaseBeeAI();


            //Auto Kill after like 4 seconds
            if (timer > 220)
                fadeOut = true;
        }
    }


    public class WalledDashBee : HittableBee
    {
        public int ownerIndex = 0;

        public Vector2 goalPos = Vector2.Zero;

        public int timeToReachDest = 60;

        public float circleRadius = 100f;

        Vector2 startPos = Vector2.Zero;

        float angleToTravel = MathHelper.Pi;
        float angleOffset = 0f;

        float distToTravel = 0f;

        Vector2 previousPos = Vector2.Zero;
        public override void AI()
        {
            if (timer == 0)
            {
                startPos = NPC.Center;
                angleOffset = Main.rand.NextFloat(6.28f);

                if (ownerIndex == -1)
                {
                    NPC.active = false;
                    return;
                }
            }
            previousPos = NPC.Center;


            NPC owner = Main.npc[ownerIndex];


            float movementProgress = Math.Clamp((float)timer / timeToReachDest, 0f, 1f);

            Vector2 trueEndPos = owner.Center + goalPos;

            Vector2 circleOffset = new Vector2(circleRadius * Easings.easeInQuad(1f - movementProgress), 0f).RotatedBy(MathHelper.Pi * Easings.easeOutQuart(movementProgress));
            circleOffset = circleOffset.RotatedBy(angleOffset);

            NPC.Center = Vector2.Lerp(owner.Center + (startPos - owner.Center), trueEndPos + circleOffset, Easings.easeOutQuad(movementProgress));


            //For rotation, doesnt actually affect movement
            //NPC.velocity = NPC.Center - previousPos;


            BaseBeeAI();

            //Auto Kill after like 2 seconds
            if (timer > 120)
                fadeOut = true;

        }
    }

}