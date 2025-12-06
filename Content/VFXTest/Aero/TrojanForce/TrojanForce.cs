using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.GameContent.Creative;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPlus.Content.Particles;

namespace VFXPlus.Content.VFXTest.Aero.TrojanForce
{
	public class TrojanForce : ModItem
	{
        public override string Texture => "VFXPlus/Content/VFXTest/Aero/TrojanForce/TrojanForce";

        public override void SetDefaults()
		{ 
            Item.DefaultToWhip(ModContent.ProjectileType<TrojanForceWhipProjTryAgain>(), 65, 3f, 3.75f, 34);
        }
		public override bool MeleePrefix() => true;

        bool isBlue = false;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: 0, ai2: isBlue ? -1 : 1f);

            isBlue = !isBlue;

            //Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai1: 1, ai2: isBlue ? -1 : 1f);


            return false;
        }
    }

    public class TrojanForceWhipProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.
            ProjectileID.Sets.IsAWhip[Type] = true;
        }
        public Vector2 TipPosition => Projectile.WhipPointsForCollision[whipSegments - 1] + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        int whipSegments = 14;
        float rangeMultiplier = 1.4f;
        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();

            Projectile.WhipSettings.Segments = whipSegments; //14
            Projectile.WhipSettings.RangeMultiplier = rangeMultiplier;

            //Projectile.extraUpdates = 6;
        }

        bool isPink = Main.rand.NextBool();

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => false;

        protected float flyTime;
        public override bool PreAI()
        {

            Player player = Main.player[Projectile.owner];
            flyTime = player.itemAnimationMax * Projectile.MaxUpdates;

            Projectile.ai[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
            Projectile.spriteDirection = (!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : -1;

            if (Projectile.ai[0] >= flyTime || player.itemAnimation == 0)
            {
                Projectile.Kill();
                return false;
            }

            player.heldProj = Projectile.whoAmI;
            player.itemAnimation = player.itemAnimationMax - (int)(Projectile.ai[0] / Projectile.MaxUpdates);
            player.itemTime = player.itemAnimation;

            if (Projectile.ai[0] == (int)(flyTime / 2f))
            {
                Projectile.WhipPointsForCollision.Clear();
                SetPoints(Projectile.WhipPointsForCollision);
                Vector2 position = Projectile.WhipPointsForCollision[^1];
                SoundEngine.PlaySound(SoundID.Item153, position);
            }

            if (Utils.GetLerpValue(0.1f, 0.7f, Projectile.ai[0] / flyTime, true) * Utils.GetLerpValue(0.9f, 0.7f, Projectile.ai[0] / flyTime, true) > 0.5f)
            {
                Projectile.WhipPointsForCollision.Clear();
                SetPoints(Projectile.WhipPointsForCollision);
            }

            if (Projectile.ai[0] > flyTime * 0.45f)
            {
                var points = new List<Vector2>();
                points.Clear();
                SetPoints(points);

                Vector2 tipPos = points[whipSegments - 1];

                previousTipPositions.Add(tipPos);
                if (previousTipPositions.Count > 6)
                    previousTipPositions.RemoveAt(0);

                Vector2 difference = points[whipSegments - 1] - points[whipSegments - 2];
                float rotation = difference.ToRotation() - MathHelper.PiOver2;

                previousTipRotations.Add(rotation);
                if (previousTipRotations.Count > 6)
                    previousTipRotations.RemoveAt(0);
            }
            return false;
        }

        public virtual void SetPoints(List<Vector2> controlPoints)
        {
            float time = Projectile.ai[0] / flyTime;
            float timeModified = time * 1.5f;
            float segmentOffset = MathHelper.Pi * 10f * (1f - timeModified) * -Projectile.spriteDirection / whipSegments;
            float tLerp = 0f;

            if (timeModified > 1f)
            {
                tLerp = (timeModified - 1f) / 0.5f;
                timeModified = MathHelper.Lerp(1f, 0f, tLerp);
            }

            //vanilla code	 
            Player player = Main.player[Projectile.owner];
            Item heldItem = player.HeldItem;
            float realRange = ContentSamples.ItemsByType[heldItem.type].useAnimation * 2 * time * player.whipRangeMultiplier;
            float segmentLength = Projectile.velocity.Length() * realRange * timeModified * rangeMultiplier / whipSegments;
            Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);
            Vector2 firstPos = playerArmPosition;
            float negativeAngle = -MathHelper.PiOver2;
            Vector2 midPos = firstPos;
            float directedAngle = 0f + MathHelper.PiOver2 + MathHelper.PiOver2 * Projectile.spriteDirection;
            Vector2 lastPos = firstPos;
            float positiveAngle = MathHelper.PiOver2;
            controlPoints.Add(playerArmPosition);

            for (int i = 0; i < whipSegments; i++)
            {
                float thisOffset = segmentOffset * (i / (float)whipSegments);
                Vector2 nextFirst = firstPos + negativeAngle.ToRotationVector2() * segmentLength;
                Vector2 nextLast = lastPos + positiveAngle.ToRotationVector2() * (segmentLength * 2f);
                Vector2 nextMid = midPos + directedAngle.ToRotationVector2() * (segmentLength * 2f);
                float progressModifier = 1f - (float)Math.Pow(1f - timeModified, 2);
                var lerpPoint1 = Vector2.Lerp(nextLast, nextFirst, progressModifier * 0.9f + 0.1f);
                var lerpPoint2 = Vector2.Lerp(nextMid, lerpPoint1, progressModifier * 0.7f + 0.3f);
                Vector2 spinningpoint = playerArmPosition + (lerpPoint2 - playerArmPosition) * new Vector2(1f, 1.5f);
                Vector2 item = spinningpoint.RotatedBy(Projectile.rotation + 4.712389f * (float)Math.Pow(tLerp, 2) * Projectile.spriteDirection, playerArmPosition);
                controlPoints.Add(item);
                negativeAngle += thisOffset;
                positiveAngle += thisOffset;
                directedAngle += thisOffset;
                firstPos = nextFirst;
                lastPos = nextLast;
                midPos = nextMid;
            }
        }

        Vector2 oldTip = Vector2.Zero;

        public List<Vector2> previousTipPositions = new List<Vector2>();
        public List<float> previousTipRotations = new List<float>();

        public override bool PreDraw(ref Color lightColor)
        {
            float swingTime = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
            float swingProgress = Timer / swingTime;

            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawAfterImage(false);
            });
            DrawAfterImage(true);


            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glowmask = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            bool draw = false;
            for (int i = 0; i < list.Count - 1; i++)
            {
                // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
                // You can change them if they don't!
                Rectangle frame = new Rectangle(0, 0, 38, 18);
                Vector2 origin = new Vector2(19, 5);
                float scale = 1;

                // These statements determine what part of the spritesheet to draw for the current segment.
                // They can also be changed to suit your sprite.
                if (i == list.Count - 2)
                {
                    frame.Y = 66;
                    frame.Height = 18;

                    // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));

                    origin.Y = 9;

                    draw = true;

                }
                else if (i > 10) // All 3 whip segments are identical but im still doing this in case a resprite changes that
                {
                    frame.Y = 50;
                    frame.Height = 16;
                    origin.Y = 8;
                    draw = true;

                }
                else if (i > 5)
                {
                    frame.Y = 34;
                    frame.Height = 16;
                    origin.Y = 8;
                    draw = true;

                }
                else if (i > 0)
                {
                    frame.Y = 18;
                    frame.Height = 16;
                    origin.Y = 8;
                    draw = true;
                }
                else
                {
                    draw = true;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                if (draw)
                {

                    float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
                    Color color = Lighting.GetColor(element.ToTileCoordinates());

                    if (i == list.Count - 2)
                    {
                        //Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Main.rand.NextVector2Circular(2f, 2f), frame, Color.HotPink with { A = 0 } * 1f, rotation, origin, scale * 1.1f, flip, 0);
                    }

                    //Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f), frame, Color.HotPink with { A = 0 } * 1f, rotation, origin, scale * 1.05f, flip, 0);

                    Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);
                }
                pos += diff;




            }
            return false;
        }

        public void DrawAfterImage(bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D Core = (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Orbs/anotheranotherorb");

            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut1, out int _, out float _);
            float t1 = Timer / timeToFlyOut1;
            float coreScale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t1, true) * Utils.GetLerpValue(0.9f, 0.7f, t1, true)) * 0.5f;

            float swingTime = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
            float swingProgress = Timer / swingTime;

            //float alpha = ;

            bool flip = false;
            for (int i = 6; i > 0; i--)
            {
                float fade = (1 - (6f - i) / 6f) * 1f;

                fade = Easings.easeInQuad(fade);

                Color col = isPink ? Color.DeepPink : Color.DeepSkyBlue;// Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.5f);

                if (i > 0 && i < previousTipPositions.Count)
                {
                    float progress = (float)i / previousTipPositions.Count;
                    //Color col = Color.Lerp(Color.DeepPink, Color.HotPink, Easings.easeInCubic(progress)) with { A = 0 } * Easings.easeInCirc(progress) * 1f;

                    Main.spriteBatch.Draw(Core, previousTipPositions[i] - Main.screenPosition, null, col with { A = 0 } * fade * 0.25f, previousTipRotations[i], Core.Size() / 2f, coreScale, 0f, 0f);
                    Main.spriteBatch.Draw(Core, previousTipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * fade * 0.25f, previousTipRotations[i], Core.Size() / 2f, coreScale * 0.5f, 0f, 0f);

                    if (i < 6 && i + 1 < previousTipPositions.Count)
                    {
                        var newPosition = Vector2.Lerp(previousTipPositions[i], previousTipPositions[i + 1], 0.5f);
                        float newRotation = MathHelper.Lerp(previousTipRotations[i], previousTipRotations[i + 1], 0.5f);

                        Main.spriteBatch.Draw(Core, newPosition - Main.screenPosition, null, col with { A = 0 } * fade * 0.25f, newRotation, Core.Size() / 2f, coreScale, 0f, 0f);
                        Main.spriteBatch.Draw(Core, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * fade * 0.25f, newRotation, Core.Size() / 2f, coreScale * 0.5f, 0f, 0f);
                    }
                }
            }

        }

        // This method draws a line between all points of the whip, in case there's empty space between the sprites.
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);


            bool isBlue = true;
            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 2; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
                Vector2 scale = new Vector2(1f * 0.6f, (diff.Length() + 2) / frame.Height);
                Vector2 scale2 = new Vector2(1f, (diff.Length() + 2) / frame.Height);

                Color glowCol = isBlue ? Color.DeepSkyBlue : Color.DeepPink;
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Main.rand.NextVector2Circular(1.5f, 1.5f), frame, glowCol with { A = 0 }, rotation, origin, scale2, SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color * 0.75f, rotation, origin, scale, SpriteEffects.None, 0);



                pos += diff;



            }
        }
    }
    public class TrojanForceWhipProjTryAgain : ModProjectile
    {
        public override string Texture => "VFXPlus/Content/VFXTest/Aero/TrojanForce/TrojanForceWhipProj";

        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public int whipSegments => 12;
        public Vector2 TipPosition => Projectile.WhipPointsForCollision[whipSegments - 1] + new Vector2(Projectile.width * 0.5f, Projectile.height * 0.5f);

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetDefaults()
        {
            //Projectile.DefaultToWhip();
            Projectile.width = Projectile.height = 18;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.penetrate = -1;


            Projectile.WhipSettings.Segments = whipSegments;
            Projectile.WhipSettings.RangeMultiplier = 1.5f;

            Projectile.extraUpdates = 7;
        }

        float flyTime;
        public bool HitboxActive => Utils.GetLerpValue(0.1f, 0.7f, Projectile.ai[0] / flyTime, true) * Utils.GetLerpValue(0.9f, 0.7f, Projectile.ai[0] / flyTime, true) > 0.5f;

        bool doneEffect = false;
        bool isBlue = false;
        public override void AI()
        {
            if (Timer == 0)
            {
                isBlue = Projectile.ai[2] == 1;
            }

            //Projectile.velocity = Projectile.velocity.RotatedBy(-0.02f);

            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            
            if (Projectile.ai[1] == 0)
                Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;
            else
                Projectile.spriteDirection = Projectile.velocity.X >= 0f ? -1 : 1;

            Timer++;

            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation * 4 <= 0)
            {
                Projectile.Kill();
                return;
            }

            owner.heldProj = Projectile.whoAmI;
            if (Timer == swingTime / 2)
            {
                // Plays a whipcrack sound at the tip of the whip.
                List<Vector2> points2 = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points2);
                SoundEngine.PlaySound(SoundID.Item153, points2[points2.Count - 1]);

                //if (isBlue)
                //    Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, Projectile.velocity, ModContent.ProjectileType<TrojanForceWhipProjTryAgain>(), Projectile.damage, Projectile.knockBack, owner.whoAmI, ai2: isBlue ? -1 : 1f);
            }

            float swingProgress = Timer / swingTime;

            //Main.NewText(previousTipPostions.Count);
            List<Vector2> points_ = Projectile.WhipPointsForCollision;
            points_.Clear();
            Projectile.FillWhipControlPoints(Projectile, points_);
            List<Vector2> points = points_;

            if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.5f && Main.rand.NextBool(6))
            {
                int pointIndex = Main.rand.Next(points.Count - 4, points.Count);
                Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));
                //int dustType = DustID.Enchanted_Gold;
                //if (Main.rand.NextBool(3))
                //    dustType = DustID.TintableDustLighted;

                Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, ModContent.DustType<GlowPixelCross>(), 0f, 0f, 0, isBlue ? Color.DeepSkyBlue : Color.DeepPink);

                //dust.noLight = false;
                dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(postSlowPower: 0.9f, velToBeginShrink: 2.5f, fadePower: 0.9f, shouldFadeColor: false);

                dust.scale = Main.rand.NextFloat(0.35f, 0.45f);

                // After choosing a randomized dust and a whip segment to spawn from, dust is spawned.
                //Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, 100, Color.White);
                dust.position = points[pointIndex];
                //dust.fadeIn = 0.3f;
                Vector2 spinningPoint = points[pointIndex] - points[pointIndex - 1];
                dust.velocity *= 0.5f;
                // This math causes these dust to spawn with a velocity perpendicular to the direction of the whip segments, giving the impression of the dust flying off like sparks.
                dust.velocity += spinningPoint.RotatedBy(owner.direction * ((float)Math.PI / 2f)) * 0.1f;
                //dust.velocity *= 0.35f;
            }

            if (swingProgress > 0.5f && swingProgress < 0.85f && Timer % 5 == 0) //58-75
            {
                var d = Dust.NewDustPerfect(points[points.Count - 1], ModContent.DustType<GlowStarSharp>(), Vector2.Zero, 0, isBlue ? Color.DeepSkyBlue : Color.DeepPink, 0.3f);
                d.position += Main.rand.NextVector2Circular(8, 8).RotatedBy(Projectile.rotation);
                d.velocity += new Vector2(0f, -Main.rand.Next(1, 3)).RotatedBy(Projectile.rotation).RotatedByRandom(0.5f);
            }


            if (swingProgress > 0.46f && swingProgress < 1f)
            {

                Vector2 top = points[points.Count - 1] - owner.Center;
                float dir = top.ToRotation();

                float whipSpeed = Main.player[Projectile.owner].GetTotalAttackSpeed(DamageClass.SummonMeleeSpeed);

                if (previousTipPostions.Count > 0)
                {
                    Vector2 o = previousTipPostions[previousTipPostions.Count - 1];
                    Vector2 nv = top;
                    for (float i = 0.1f; i <= 0.1f; i += 0.1f)
                    {
                        previousTipPostions.Add(Vector2.Lerp(o, nv, i) + dir.ToRotationVector2() * (4f * whipSpeed));
                        if (previousTipPostions.Count > 60)
                        {
                            previousTipPostions.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    previousTipPostions.Add(top);
                }
            }

            trailWidth = 1f - Utils.GetLerpValue(0.85f, 0.95f, swingProgress, true);

        }


        public override bool PreDraw(ref Color lightColor)
        {
            float swingTime = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
            float swingProgress = Timer / swingTime;

            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                TrailDraw(false);
            });
            //DrawTrail(true);
            TrailDraw(true);


            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Texture2D glowmask = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/TrojanForceWhipProjGlow").Value; //


            Vector2 pos = list[0];

            float lastRot = 0f;
            bool draw = false;
            for (int i = 0; i < list.Count - 1; i++)
            {
                // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
                // You can change them if they don't!
                Rectangle frame = new Rectangle(0, 0, 38, 18);
                Vector2 origin = new Vector2(19, 5);
                float scale = 1;

                // These statements determine what part of the spritesheet to draw for the current segment.
                // They can also be changed to suit your sprite.
                if (false && i == list.Count - 2)
                {
                    frame.Y = 66;
                    frame.Height = 18;

                    // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOutA, out int _, out float _);
                    float t = Timer / timeToFlyOutA;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));

                    origin.Y = 9;

                    draw = true;

                }
                else if (i > 10) // All 3 whip segments are identical but im still doing this in case a resprite changes that
                {
                    frame.Y = 50;
                    frame.Height = 16;
                    origin.Y = 8;
                    draw = true;

                }
                else if (i > 5)
                {
                    frame.Y = 34;
                    frame.Height = 16;
                    origin.Y = 8;
                    draw = true;

                }
                else if (i > 0)
                {
                    frame.Y = 18;
                    frame.Height = 16;
                    origin.Y = 8;
                    draw = true;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                if (true)
                {

                    float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
                    Color color = Lighting.GetColor(element.ToTileCoordinates());

                    if (i == list.Count - 2)
                    {
                        //Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Main.rand.NextVector2Circular(2f, 2f), frame, Color.HotPink with { A = 0 } * 1f, rotation, origin, scale * 1.1f, flip, 0);
                    }

                    //Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f), frame, Color.HotPink with { A = 0 } * 1f, rotation, origin, scale * 1.05f, flip, 0);

                    Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                    lastRot = rotation;
                }
                pos += diff;




            }

            //Draw the handle last so it is drawn over the chain segments
            Color handleCol = Lighting.GetColor(list[0].ToTileCoordinates());

            float myRot = (list[1] - list[0]).ToRotation() - MathHelper.PiOver2;
            Main.EntitySpriteDraw(texture, list[0] - Main.screenPosition, new Rectangle(0, 0, 38, 18), handleCol, myRot,  new Vector2(19, 0), 1f, flip, 0);
            Main.EntitySpriteDraw(glowmask, list[0] - Main.screenPosition, new Rectangle(0, 0, 38, 18), Color.White, myRot, new Vector2(19, 0), 1f, flip, 0);

            //Who up playing wit they orb
            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
            float t2 = Timer / timeToFlyOut;
            float tipScale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t2, true) * Utils.GetLerpValue(0.9f, 0.7f, t2, true));


            Texture2D orb = CommonTextures.feather_circle128PMA.Value;
            Vector2 originPoint = list[list.Count - 1] - Main.screenPosition + (lastRot + MathHelper.PiOver2).ToRotationVector2() * 5f;

            Color[] cols = { Color.LightSkyBlue * 0.75f, Color.SkyBlue * 0.525f, Color.DeepSkyBlue * 0.375f };
            if (!isBlue)
            {
                cols[0] = Color.Pink;
                cols[1] = Color.HotPink;
                cols[2] = Color.DeepPink;
            }

            float[] scales = { 0.85f, 1.6f, 2.5f };

            float orbAlpha = 0.15f;
            Vector2 orbScale = new Vector2(0.9f, 0.9f) * 0.35f * tipScale;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.07f) * 0.15f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.13f) * 0.1f;

            Main.EntitySpriteDraw(orb, originPoint, null, cols[0] with { A = 0 } * orbAlpha, lastRot, orb.Size() / 2f, orbScale * scales[0], SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, cols[1] with { A = 0 } * orbAlpha, lastRot, orb.Size() / 2f, orbScale * scales[1] * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(orb, originPoint, null, cols[2] with { A = 0 } * orbAlpha, lastRot, orb.Size() / 2f, orbScale * scales[2] * sineScale2, SpriteEffects.None);



            //Draw the tip in the actual correct position 
            Color tipCol = Lighting.GetColor(list[list.Count - 1].ToTileCoordinates());



            for (int i = 0; i < 4; i++)
            {
                Main.EntitySpriteDraw(texture, list[list.Count - 1] - Main.screenPosition + Main.rand.NextVector2Circular(3f, 3f), new Rectangle(0, 66, 38, 18), Color.White with { A = 0 } * 0.4f, lastRot, new Vector2(19, 5), tipScale, flip, 0);
            }

            Main.EntitySpriteDraw(texture, list[list.Count - 1] - Main.screenPosition, new Rectangle(0, 66, 38, 18), Color.White, lastRot, new Vector2(19, 5), tipScale, flip, 0);
            //Main.EntitySpriteDraw(glowmask, list[list.Count - 1] - Main.screenPosition, new Rectangle(0, 66, 38, 18), Color.White, lastRot, new Vector2(19, 5), tipScale, flip, 0);

            //Main.EntitySpriteDraw(glowmask, list[list.Count - 1] - Main.screenPosition + Main.rand.NextVector2Circular(2f, 2f), new Rectangle(0, 66, 38, 18), Color.White with { A = 0 }, lastRot, new Vector2(19, 5), tipScale, flip, 0);


            return false;
        }

        List<float> previousTipRotations = new List<float>();
        List<Vector2> previousTipPostions = new List<Vector2>();

        List<Vector2> newPositions = new List<Vector2>();
        List<float> newRotations = new List<float>();

        float trailWidth = 1f;
        float trailAlpha = 1f;
        Effect trailEffect = null;
        public void TrailDraw(bool giveUp)
        {
            if (giveUp || previousTipPostions.Count < 3)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/spark_07_Black").Value; //

            if (trailEffect == null)
                trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            newPositions.Clear();
            newRotations.Clear();
            for (int i = 1; i < previousTipPostions.Count; i++)
            {
                Vector2 startPos = previousTipPostions[i - 1] + Main.player[Projectile.owner].Center;
                Vector2 endPos = previousTipPostions[i] + Main.player[Projectile.owner].Center;

                float rot = (endPos - startPos).ToRotation();

                newPositions.Add(startPos);
                newRotations.Add(rot);
            }


            Color colA = isBlue ? Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.15f) : Color.DeepPink;
            Color colB = isBlue ? Color.SkyBlue : Color.HotPink;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = newPositions.ToArray();
            float[] rot_arr = newRotations.ToArray();


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0f;


            Color StripColor(float progress) => Color.White * Easings.easeInQuad(progress);

            float StripWidth(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.5f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = Easings.easeOutSine(1f - LV);
                }

                return toReturn * sineWidthMult * 0.75f * 60f * trailWidth;
            }

            float StripWidth2(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.5f) //back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = Easings.easeOutSine(1f - LV);
                }

                return toReturn * sineWidthMult * 1f * 60f * trailWidth;
            }


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.015f); //0.02
            trailEffect.Parameters["reps"].SetValue(0.5f * (previousTipPostions.Count() / 60f));


            //Over layer
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["ColorOne"].SetValue(colA.ToVector3() * 2f * trailAlpha); //2f
            trailEffect.Parameters["glowThreshold"].SetValue(0.9f);
            trailEffect.Parameters["glowIntensity"].SetValue(1f);
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            //UnderLayer
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            trailEffect.Parameters["glowThreshold"].SetValue(1f);
            trailEffect.Parameters["glowIntensity"].SetValue(1f);
            trailEffect.Parameters["ColorOne"].SetValue(colB.ToVector3() * 5f * trailAlpha);
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        // This method draws a line between all points of the whip, in case there's empty space between the sprites.
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);


            bool isBlue = true;
            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.White);
                Vector2 scale = new Vector2(1f * 0.6f, (diff.Length() + 2) / frame.Height);
                Vector2 scale2 = new Vector2(1f, (diff.Length() + 2) / frame.Height);

                Color glowCol = isBlue ? Color.DeepSkyBlue : Color.DeepPink;
                Main.EntitySpriteDraw(texture, pos - Main.screenPosition + Main.rand.NextVector2Circular(1.5f, 1.5f), frame, glowCol with { A = 0 }, rotation, origin, scale2, SpriteEffects.None, 0);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color * 0.75f, rotation, origin, scale, SpriteEffects.None, 0);



                pos += diff;

                if (i % 2 == 0)
                    isBlue = !isBlue;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            Projectile.damage = (int)(hit.Damage * 0.8f); // Multihit penalty. 
        }
    }

    public class TrojanForceBotTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.ignoreWater = false;
            Projectile.hostile = false;
            Projectile.friendly = true;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 99370;
        }

        public override bool? CanDamage() => isDashing;

        public NPC target;
        public Player Owner => Main.player[Projectile.owner];

        private enum State
        {
            SpawnIn,
            TriangleLaser,
            Dash,
            Orb,
            Idle,
            Glitching
        }

        State currentState = State.TriangleLaser;

        //How blue the colors should be 0f = pink, 1f = blue
        float dashColorProgress = 0f;

        float orbChargePower = 0f;

        float justShotPower = 0f;

        int timer = 0;
        int substateTimer = 0;

        float overallScale = 1f;
        float overallAlpha = 1f;
        public override void AI()
        {
            FindTarget();

            //currentState = State.Orb;
            switch (currentState)
            {
                case State.TriangleLaser:
                    TriangleLaser();
                    break;
                case State.Dash:
                    Dash();
                    break;
                case State.Orb:
                    Orb();
                    break;
                case State.Idle:
                    Idle();
                    break;
                case State.Glitching:
                    Glitching();
                    break;
            }



            int trailCount = 9;
            previousPositions.Add(Projectile.Center);
            previousRotations.Add(Projectile.rotation);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (timer % 5 == 0)
                Projectile.frame = (Projectile.frame + 1) % 4;

            if (timer % 1 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Color thisCol = Color.Lerp(Color.LightSkyBlue, Color.DeepSkyBlue, 0.75f);

                    Vector2 myvel = Projectile.rotation.ToRotationVector2() * -(1f + i);

                    Vector2 dustSpawnPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * -20f;

                    float particleScale = isDashing ? 0.7f : 0.5f;

                    FireParticle fire = new FireParticle(dustSpawnPos + Main.rand.NextVector2Circular(2f, 2f), myvel, particleScale, thisCol, colorMult: 0.5f, bloomAlpha: 1f,
                        AlphaFade: 0.88f, RotPower: 0.01f);
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }

            justShotPower = Math.Clamp(MathHelper.Lerp(justShotPower, -0.5f, 0.07f), 0f, 1f);
            
            if (isDashing)
                dashColorProgress = Math.Clamp(MathHelper.Lerp(dashColorProgress, 0.5f, 0.1f), 0f, 1f);
            else
                dashColorProgress = Math.Clamp(MathHelper.Lerp(dashColorProgress, -0.5f, 0.1f), 0f, 1f);

            timer++;
        }

        #region States
        public void SpawnIn()
        {
            substateTimer++;
        }

        Vector2 orbitVector = Main.rand.NextVector2CircularEdge(200f, 200f);
        public void TriangleLaser()
        {
            if (!FindTarget())
                return;

            int timeBeforeShot = 25; //25 | 20 --> 20 | 15
            int timeAfterShot = 20;

            if (substateTimer < timeBeforeShot)
            {
                if (substateTimer == 0)
                    orbitVector = Main.rand.NextVector2CircularEdge(200f, 200f);

                //Move to target
                Vector2 goalPos = target.Center + orbitVector;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(goalPos) * (Projectile.Distance(goalPos) / 15), 0.2f); //15 0.2

                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
            }
            else if (substateTimer < timeBeforeShot + timeAfterShot)
            {
                Vector2 goalPos = target.Center + orbitVector;

                //Shoot
                if (substateTimer == timeBeforeShot)// || substateTimer % 6 == 0)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                    int laser = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget.RotateRandom(0.1f * 0f) * 6f, ProjectileID.PurpleLaser, 10, 1, Projectile.owner);
                    Main.projectile[laser].scale *= 1f;
                    //Main.projectile[laser].penetrate = 1;

                    //Recoil
                    Projectile.velocity += toTarget * -10f;

                    //Dust
                    Vector2 dustPos = Projectile.Center;
                    Color between = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);
                    for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++) //2 //0,3
                    {
                        Dust dp = Dust.NewDustPerfect(dustPos, ModContent.DustType<LineSpark>(),
                            toTarget.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(6f, 22f),
                            newColor: between * 1f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                        dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                            1f, 0.5f);
                    }

                    for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
                    {
                        Color col1 = Color.Lerp(Color.DeepPink, Color.HotPink, 0.65f);

                        Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                        Dust dust = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.25f, 0.3f) * 1.15f);
                        dust.noLight = false;
                        dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                            velToBeginShrink: 10f, fadePower: 0.93f, shouldFadeColor: false);

                        dust.velocity += toTarget * 6f;
                    }


                    Vector2 vel = toTarget * 1f; //2.5
                    Dust d = Dust.NewDustPerfect(dustPos - toTarget * 2f, ModContent.DustType<CirclePulse>(), vel, newColor: Color.HotPink * 1f);
                    d.scale = 0.05f;
                    CirclePulseBehavior b = new CirclePulseBehavior(0.2f, true, 2, 0.2f, 0.35f);
                    b.drawLayer = "OverPlayers";
                    d.customData = b;


                    SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = .12f, Pitch = .1f, PitchVariance = .15f, MaxInstances = 1 };
                    SoundEngine.PlaySound(style, Projectile.Center);

                    SoundStyle style2 = new SoundStyle("Terraria/Sounds/Research_1") with { Pitch = .85f, PitchVariance = .2f, Volume = 0.25f };
                    SoundEngine.PlaySound(style2, Projectile.Center);

                    justShotPower = 1f;
                }

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(goalPos) * (Projectile.Distance(goalPos) / 10), 0.15f); //15 0.2
                //Projectile.velocity *= 0.8f;
            }
            else if (substateTimer == timeBeforeShot + timeAfterShot)
            {
                currentState = State.Dash;
                substateTimer = -1;
            }

            substateTimer++;
        }

        bool isDashing = false;
        public void Dash()
        {
            int timeBeforeDash = 50; //50 | 15 | 15
            int timeToBackUp = 15;
            int timeAfterDash = 15;

            //Move closer to target
            if (substateTimer < timeBeforeDash)
            {
                if (substateTimer == 0)
                    orbitVector = orbitVector.SafeNormalize(Vector2.UnitX) * 100f;

                //Back up right before dash
                if (substateTimer == timeBeforeDash - timeToBackUp)
                    orbitVector *= 1.75f;

                Vector2 goalPos = target.Center + orbitVector;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(goalPos) * (Projectile.Distance(goalPos) / 15), 0.2f); //15 0.2

                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
            }
            //Dash
            else if (substateTimer < timeBeforeDash + timeAfterDash)
            {
                if (substateTimer == timeBeforeDash)
                {
                    Projectile.velocity = Projectile.rotation.ToRotationVector2() * 40f;

                    Dust d2 = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.75f, ModContent.DustType<CirclePulse>(), Projectile.velocity.SafeNormalize(Vector2.UnitX) * 2f, 
                        newColor: Color.DeepSkyBlue * 0.3f);
                    d2.scale = 0.1f;
                    CirclePulseBehavior b2 = new CirclePulseBehavior(0.5f, true, 2, 0.2f, 0.4f);
                    b2.drawLayer = "UnderProjectiles";
                    d2.customData = b2;
                }

                Projectile.velocity *= 0.93f;

                if (Projectile.velocity.Length() > 15f && substateTimer != timeBeforeDash)
                {
                    //Color dustCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);
                    Color dustCol = Color.Lerp(Color.DeepSkyBlue, Color.SkyBlue, 0.35f);

                    Dust p = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), ModContent.DustType<WindLine>(), 
                        Projectile.velocity.SafeNormalize(Vector2.UnitX) * -2f, newColor: dustCol, Scale: 3f);

                    WindLineBehavior wlb = new WindLineBehavior(VelFadePower: 0.95f, TimeToStartShrink: 0, ShrinkYScalePower: 0.75f, 0.6f, 0.55f, true);
                    wlb.drawWhiteCore = true;
                    p.customData = wlb;
                }

                Projectile.rotation = Projectile.velocity.ToRotation();

                isDashing = true;
            }
            else if (substateTimer == timeBeforeDash + timeAfterDash)
            {
                currentState = Main.rand.NextBool(2) ? State.Orb : State.TriangleLaser;
                orbitVector = Main.rand.NextVector2CircularEdge(200f, 200f);
                isDashing = false;

                substateTimer = -1;
            }
            substateTimer++;
        }

        public void Orb()
        {
            if (!FindTarget())
                return;

            int timeBeforeShot = 70;
            int timeAfterShot = 60;

            if (substateTimer < timeBeforeShot)
            {
                if (substateTimer == 0)
                    orbitVector = Main.rand.NextVector2CircularEdge(280f, 280f);

                //Move to target
                Vector2 goalPos = target.Center + orbitVector;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(goalPos) * (Projectile.Distance(goalPos) / 15), 0.2f); //15 0.2

                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

                orbChargePower = Utils.GetLerpValue(0, timeBeforeShot, substateTimer, true);

            }
            else if (substateTimer < timeBeforeShot + timeAfterShot)
            {
                Vector2 goalPos = target.Center + orbitVector;

                //Shoot
                if (substateTimer == timeBeforeShot)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                    float offRot = (MathHelper.TwoPi / 3);

                    int laser = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget.RotatedBy(offRot) * 6f, ModContent.ProjectileType<TrojanForceEnergyBall>(), 10, 1, Projectile.owner);
                    int laser2 = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget.RotatedBy(-offRot) * 6f, ModContent.ProjectileType<TrojanForceEnergyBall>(), 10, 1, Projectile.owner);
                    int laser3 = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 4f, ModContent.ProjectileType<TrojanForceEnergyBall>(), 10, 1, Projectile.owner);

                    Main.projectile[laser].scale *= 0.65f;
                    Main.projectile[laser2].scale *= 0.65f;
                    Main.projectile[laser3].scale *= 1f;

                    //Recoil
                    Projectile.velocity += toTarget * -14f;
                    Projectile.rotation = toTarget.ToRotation();

                    //Dust
                    Vector2 dustPos = Projectile.Center;
                    Color between = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f);
                    for (int i = 0; i < 2 + Main.rand.Next(1, 3); i++) //2 //0,3
                    {
                        Dust dp = Dust.NewDustPerfect(dustPos, ModContent.DustType<LineSpark>(),
                            toTarget.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(6f, 22f),
                            newColor: between * 1f, Scale: Main.rand.NextFloat(0.45f, 0.65f) * 0.45f);

                        dp.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.88f, preShrinkPower: 0.99f, postShrinkPower: 0.8f, timeToStartShrink: 10 + Main.rand.Next(-5, 5), killEarlyTime: 80,
                            1f, 0.5f);
                    }

                    for (int i = 0; i < 3 + Main.rand.Next(0, 3); i++)
                    {
                        Color col1 = Color.Lerp(Color.DeepPink, Color.HotPink, 0.65f);

                        Vector2 randomStart = Main.rand.NextVector2Circular(4f, 4f) * 1f;
                        Dust dust = Dust.NewDustPerfect(dustPos, ModContent.DustType<GlowPixelCross>(), randomStart, newColor: col1, Scale: Main.rand.NextFloat(0.25f, 0.3f) * 1.15f);
                        dust.noLight = false;
                        dust.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, preSlowPower: 0.99f, timeBeforeSlow: 0, postSlowPower: 0.89f,
                            velToBeginShrink: 10f, fadePower: 0.93f, shouldFadeColor: false);

                        dust.velocity += toTarget * 6f;
                    }

                    CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.35f, true, 1, 0.8f, 0.8f);

                    Dust d1 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.HotPink * 0.5f);
                    d1.scale = 0.1f;
                    d1.customData = cpb2;
                    d1.velocity = new Vector2(-0.01f, 0f);

                    Dust d2 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: Color.HotPink * 0.5f);
                    d2.scale = 0.1f;
                    d2.customData = cpb2;
                    d2.velocity = new Vector2(0.01f, 0f);

                    SoundStyle style = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_75") with { Volume = 0.35f, Pitch = -0.05f, PitchVariance = .05f, MaxInstances = -1 };
                    SoundEngine.PlaySound(style, Projectile.Center);

                    SoundStyle style32 = new SoundStyle("VFXPlus/Sounds/Effects/laser_fire") with { Volume = 0.15f, Pitch = 0.25f, MaxInstances = -1, PitchVariance = 0.1f };
                    SoundEngine.PlaySound(style32, Projectile.Center);

                    SoundStyle style23 = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Dd2_sky_dragons_fury_shot_0") with { Pitch = 0.15f, PitchVariance = 0.4f, Volume = 0.3f };
                    SoundEngine.PlaySound(style23, Projectile.Center);

                    SoundStyle stylec = new SoundStyle("VFXPlus/Sounds/Effects/Vanilla/Item_67") with { Pitch = 0f, Volume = 0.55f, PitchVariance = 0.1f, MaxInstances = -1 }; //1f
                    SoundEngine.PlaySound(stylec, Projectile.Center);

                    justShotPower = 1f;
                }

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(goalPos) * (Projectile.Distance(goalPos) / 10), 0.1f); //15 0.2

                orbChargePower = 0f;
            }
            else if (substateTimer == timeBeforeShot + timeAfterShot)
            {
                currentState = State.Dash;
                substateTimer = -1;
            }

            substateTimer++;
        }

        public void Idle()
        {
            //Every 90 frames, choose a random new location around player and try to move there
            if (substateTimer % 40 == 0)
                orbitVector = Main.rand.NextVector2CircularEdge(70f, 70f);

            Vector2 goalPos = Owner.Center + orbitVector + new Vector2(0f, -40f);

            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(goalPos) * (Projectile.Distance(goalPos) / 10), 0.01f); //15 0.2

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Projectile.velocity.Length() > 18)
                isDashing = true;
            else
                isDashing = false;


                Main.NewText(Projectile.velocity.Length());
            //Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 30;


            substateTimer++;
        }

        public void Glitching()
        {

            substateTimer++;
        }
        #endregion

        public bool FindTarget(float maxTargetDist = 700f)
        {
            if (Owner.HasMinionAttackTargetNPC) //take the minion target first
            {
                target = Main.npc[Owner.MinionAttackTargetNPC];
                return true;
            }

            foreach (NPC npc in Main.ActiveNPCs) //scan all targets for anything less than maxTargetDist units away
            {
                if (!npc.friendly && npc.CanBeChasedBy(Projectile) && Vector2.Distance(npc.Center, Projectile.Center) < maxTargetDist)
                {
                    target = npc;
                    return true;
                }
            }
            return false;
        }


        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D Bot = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/TrojanForceBot").Value;
            Texture2D Glowmask = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/TrojanForceBotGlowmask").Value;
            Texture2D GlowWhite = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/TrojanForceBotGlowWhite").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Bot.Frame(1, 4, frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = Projectile.velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            //Orb
            Texture2D Glow = CommonTextures.feather_circle128PMA.Value;
            Main.EntitySpriteDraw(Glow, drawPos, null, (isDashing ? Color.DeepSkyBlue : Color.DeepPink) with { A = 0 } * 0.35f, Projectile.rotation, Glow.Size() / 2f, Projectile.scale * 0.75f * overallScale, SpriteEffects.None);


            //Trail
            Color trailCol = isDashing ? Color.DeepSkyBlue : Color.HotPink;
            for (int i = 0; i < previousPositions.Count; i++)
            {
                float progress = (float)i / previousPositions.Count;

                float size = (Projectile.scale * overallScale) - (0.33f - (progress * 0.33f));
                Color col = trailCol with { A = 0 } * progress * 0.25f;


                Vector2 AfterImagePos = previousPositions[i] - Main.screenPosition;

                Main.EntitySpriteDraw(GlowWhite, AfterImagePos, sourceRectangle, col,
                        Projectile.rotation, TexOrigin, size, SE);
            }

            Color between = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);
            Color borderCol = isDashing ? Color.DeepSkyBlue : between;
            //Border
            for (int i = 0; i < 4; i++)
            {
                Main.spriteBatch.Draw(GlowWhite, drawPos + Main.rand.NextVector2Circular(1.5f, 1.5f), sourceRectangle, borderCol with { A = 0 } * overallAlpha * 0.3f, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3
            }

            //Main Tex
            Main.spriteBatch.Draw(Bot, drawPos, sourceRectangle, lightColor * overallAlpha, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            Main.spriteBatch.Draw(Glowmask, drawPos, sourceRectangle, Color.White * overallAlpha, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            Main.spriteBatch.Draw(GlowWhite, drawPos, sourceRectangle, (isDashing ? Color.SkyBlue : Color.LightPink) with { A = 0 } * justShotPower, Projectile.rotation, TexOrigin, Projectile.scale * overallScale, SE, 0f); //0.3

            //Star
            Texture2D Star = CommonTextures.CrispStarPMA.Value;

            Vector2 starPos = drawPos + Projectile.rotation.ToRotationVector2() * 14f;
            float starRot = (float)Main.timeForVisualEffects * 0.15f;
            float starScale = Projectile.scale * overallScale * Easings.easeOutQuad(justShotPower) * 1f;

            Main.spriteBatch.Draw(Star, starPos, null, Color.DeepPink with { A = 0 } * 1f, starRot, Star.Size() / 2f, starScale, SE, 0f); //0.3
            Main.spriteBatch.Draw(Star, starPos, null, Color.White with { A = 0 } * 1f, starRot, Star.Size() / 2f, starScale * 0.5f, SE, 0f); //0.3

            //GlitchFX
            Texture2D Line = CommonTextures.Flare.Value;
            for (int i = 0; i < 0; i++)
            {
                Vector2 linePos = drawPos + Main.rand.NextVector2Circular(12f, 12f);

                Main.spriteBatch.Draw(Line, linePos + new Vector2(2f, 0f), null, Color.HotPink with { A = 0 } * 1f, MathHelper.PiOver2, Line.Size() / 2f, new Vector2(1f, 1f) * 0.5f, SE, 0f); //0.3
                Main.spriteBatch.Draw(Line, linePos + new Vector2(-2f, 0f), null, Color.SkyBlue with { A = 0 } * 1f, MathHelper.PiOver2, Line.Size() / 2f, new Vector2(1f, 1f) * 0.5f, SE, 0f); //0.3
                Main.spriteBatch.Draw(Line, linePos, null, Color.Black * 1f, MathHelper.PiOver2, Line.Size() / 2f, new Vector2(1f, 1f) * 0.5f, SE, 0f); //0.3
            }


            //Orb on top
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.Dusts, () =>
            {
                DrawBasicBall(false);
            });

            return false;
        }

        //I should really just make this a function in utils atp
        public void DrawBasicBall(bool giveUp)
        {
            if (giveUp || orbChargePower == 0)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition + Projectile.rotation.ToRotationVector2() * 25f;

            //Draw Orb
            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;

            Color[] cols = { Color.White * 1f, Color.HotPink * 0.525f, Color.DeepPink * 0.375f };
            float[] scales = { 0.85f, 1.45f, 2.5f };

            float orbAlpha = 1f;
            float totalScale = Projectile.scale * 0.25f * Easings.easeInOutQuad(orbChargePower); //0.25

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.2f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.11f;

            //Main.EntitySpriteDraw(Orb, drawPos, null, Color.DodgerBlue * orbAlpha * 0.35f, 0f, Orb.Size() / 2f, scales[2] * totalScale, SpriteEffects.None);

            Main.EntitySpriteDraw(Orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[0] * totalScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[1] * totalScale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[2] * totalScale * sineScale2, SpriteEffects.None);
        }


    }

    public class TrojanForceEnergyBall : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        float justShotVal = 1f;

        int timer = 0;
        public override void AI()
        {
            int trailCount = 12;
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center + Projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (FindTarget() && timer > 5)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                Projectile.velocity += toTarget * 0.4f;

                if (Projectile.velocity.Length() > 16)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20f;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX), toTarget, 0.04f) * Projectile.velocity.Length();

            }


            Lighting.AddLight(Projectile.Center, Color.DeepPink.ToVector3());

            if (timer > 5)
                justShotVal = Math.Clamp(MathHelper.Lerp(justShotVal, -0.35f, 0.04f), 0f, 1f);

            if (timer % 4 == 0 && false)
            {
                Dust dp = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<ElectricSparkGlow>(), newColor: Color.DeepSkyBlue, Scale: Main.rand.NextFloat(0.85f, 1f));
                dp.velocity *= 0.4f;
                dp.velocity += Projectile.velocity * 0.25f;

                ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.8f, FadeScalePower: 0.91f, FadeVelPower: 0.92f, Pixelize: false, XScale: 1f, YScale: 1f); //0.91
                esb.killEarlyTime = 20;
                dp.customData = esb;
            }

            if (timer % 1 == 0 && false)
            {
                for (int i = 0; i < 2; i++)
                {
                    Color thisCol = Color.Lerp(Color.DeepPink, Color.HotPink, 0.75f);

                    Vector2 myvel = Projectile.rotation.ToRotationVector2() * -(1f + i);

                    Vector2 dustSpawnPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * 0f;

                    FireParticle fire = new FireParticle(dustSpawnPos + Main.rand.NextVector2Circular(2f, 2f), myvel, 0.8f, thisCol, colorMult: 0.5f, bloomAlpha: 1f,
                        AlphaFade: 0.88f, RotPower: 0.01f);
                    fire.renderLayer = RenderLayer.UnderProjectiles;
                    ShaderParticleHandler.SpawnParticle(fire);
                }
            }

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 0.75f));

            if (timer % 4 == 0)
                Projectile.frame = (Projectile.frame + 1) % 6;

            timer++;
        }

        public NPC target;
        public bool FindTarget(float maxTargetDist = 700f)
        {
            foreach (NPC npc in Main.ActiveNPCs) //scan all targets for anything less than maxTargetDist units away
            {
                if (!npc.friendly && npc.CanBeChasedBy(Projectile) && Vector2.Distance(npc.Center, Projectile.Center) < maxTargetDist)
                {
                    target = npc;
                    return true;
                }
            }
            return false;
        }


        public override bool PreKill(int timeLeft)
        {
            #region Dust
            Color between = Color.Lerp(Color.DeepPink, Color.HotPink, 0f);
            Dust d11 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<FeatheredGlowDust>(), Velocity: Vector2.Zero, newColor: between, Scale: 1f * Projectile.scale);

            FeatheredGlowBehavior fgb = new FeatheredGlowBehavior(AlphaChangeSpeed: 0.65f, timeToChangeAlpha: 6, ScaleChangeSpeed: 1.1f, timeToKill: 120, OverallAlpha: 0.5f);
            fgb.DrawWhiteCore = true;
            d11.customData = fgb;


            //Impact Dust
            Color betweenPink = Color.Lerp(Color.DeepPink, Color.HotPink, 0.6f);
            Color betweenPink2 = Color.Lerp(Color.DeepPink, Color.HotPink, 1f);

            CirclePulseBehavior cpb2 = new CirclePulseBehavior(0.55f * Projectile.scale, true, 3, 0.8f, 0.8f);

            Dust d1 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: betweenPink * 0.25f);
            d1.customData = cpb2;
            d1.velocity = new Vector2(-0.01f, 0f);
            d1.fadeIn = 0.5f;

            Dust d2 = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<CirclePulse>(), Velocity: Vector2.Zero, newColor: betweenPink2 * 0.25f);
            d2.customData = cpb2;
            d2.velocity = new Vector2(0.01f, 0f);
            d2.fadeIn = 0.5f;


            int crossCount = (int)(6 * Projectile.scale);
            for (int i = 0; i < crossCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)crossCount) * i;

                Vector2 dustVel = dir.ToRotationVector2() * Main.rand.NextFloat(3.5f, 7f);
                dustVel = dustVel.RotatedBy(Main.rand.NextFloat(-0.15f, 0.15f));

                Color middleBlue = Color.Lerp(Color.DeepPink, Color.HotPink, 0.25f + Main.rand.NextFloat(-0.15f, 0.15f));

                Dust gd = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<GlowPixelCross>(), dustVel, newColor: middleBlue, Scale: Main.rand.NextFloat(0.25f, 0.55f));
                gd.customData = DustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.2f, timeBeforeSlow: 5,
                    preSlowPower: 0.94f, postSlowPower: 0.9f, velToBeginShrink: 1.5f, fadePower: 0.92f, shouldFadeColor: false);
            }

            int pgoCount = (int)(7 * Projectile.scale); ;
            for (int i = 0; i < pgoCount; i++)
            {
                float dir = (MathHelper.TwoPi / (float)pgoCount) * i;

                Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelGlowOrb>(), newColor: Color.DeepPink, Scale: Main.rand.NextFloat(0.55f, 0.8f));
                d.velocity = dir.ToRotationVector2() * Main.rand.NextFloat(0.5f, 6f);
                d.velocity = d.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f));

                d.customData = DustBehaviorUtil.AssignBehavior_PGOBase(rotPower: 0.04f, timeBeforeSlow: 4, postSlowPower: 0.89f, velToBeginShrink: 1f, fadePower: 0.8f, colorFadePower: 1f, glowIntensity: 0.4f);
            }

            #endregion

            return base.PreKill(timeLeft);
        }

        float overallAlpha = 1f;
        float overallScale = 1f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/CyverEnergyBall").Value;
            Texture2D TexGlow = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/CyverEnergyBallGlow").Value;
            //Texture2D TexBorder = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/CyverEnergyBallOutline").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle sourceRectangle = Tex.Frame(1, 9, frameY: Projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;
            SpriteEffects SE = Projectile.velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            float rot = Projectile.velocity.ToRotation();

            Color pinkBetween = Color.Lerp(Color.DeepPink, Color.SkyBlue, 0.5f);
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(TexGlow, drawPos + rot.ToRotationVector2() * -15 + Main.rand.NextVector2Circular(3f, 3f), sourceRectangle, pinkBetween with { A = 0 }, rot, sourceRectangle.Size() / 2f, Projectile.scale * overallScale, SpriteEffects.None);
            }

            //Main.EntitySpriteDraw(TexBorder, drawPos + rot.ToRotationVector2() * -15, sourceRectangle, Color.White, rot, sourceRectangle.Size() / 2f, Projectile.scale * overallScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Tex, drawPos + rot.ToRotationVector2() * -15, sourceRectangle, Color.White, rot, sourceRectangle.Size() / 2f, Projectile.scale * overallScale, SpriteEffects.None);



            #region Gash
            Texture2D gash = CommonTextures.SoulSpike.Value;

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.1f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.06f;

            Color between = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0.5f);

            float gashRot = 0f;
            Vector2 gashScale = new Vector2(2f * Easings.easeOutCubic(1f - justShotVal) * sineScale2, 0.45f * sineScale1) * Projectile.scale;
            Main.EntitySpriteDraw(gash, drawPos, null, Color.DeepPink with { A = 0 } * Easings.easeInQuad(justShotVal) * 1f, gashRot, gash.Size() / 2f, gashScale * 2f, SpriteEffects.None);
            Main.EntitySpriteDraw(gash, drawPos, null, Color.White with { A = 0 } * Easings.easeInQuad(justShotVal) * 1f, gashRot, gash.Size() / 2f, gashScale * 1f, SpriteEffects.None);
            #endregion

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(false);
                DrawBasicBall(false);
            });

            return false;
        }

        public void DrawBasicBall(bool giveUp)
        {
            if (giveUp)
                return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            //Draw Orb
            Texture2D Orb = CommonTextures.feather_circle128PMA.Value;

            Color[] cols = { Color.White * 1f, Color.HotPink * 0.525f, Color.DeepPink * 0.375f };
            float[] scales = { 0.85f, 1.45f, 2.5f };

            float orbAlpha = 0.75f;
            float totalScale = Projectile.scale * 0.4f * Easings.easeInOutQuad(overallScale);

            float sineScale1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * 0.12f) * 0.1f;
            float sineScale2 = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.22f) * 0.06f;

            //Main.EntitySpriteDraw(Orb, drawPos, null, cols[0] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[0] * totalScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[1] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[1] * totalScale * sineScale1, SpriteEffects.None);
            Main.EntitySpriteDraw(Orb, drawPos, null, cols[2] with { A = 0 } * orbAlpha, 0f, Orb.Size() / 2f, scales[2] * totalScale * sineScale2, SpriteEffects.None);
        }

        public void DrawTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Texture2D line = CommonTextures.SoulSpike.Value;
            Color between2 = Color.Lerp(Color.DeepPink, Color.HotPink, 0.4f);
            for (int i = 0; i < previousPositions.Count - 1; i++)
            {
                float progress = (float)i / (float)previousPositions.Count;

                Vector2 offset1 = Vector2.Zero;

                offset1 += Main.rand.NextVector2Circular(11f * Projectile.scale * 0f, 6f * Projectile.scale).RotatedBy(previousRotations[i]) * overallScale * Projectile.scale * Easings.easeOutQuad(1f - progress);


                Vector2 flarePos = previousPositions[i] - Main.screenPosition;

                Color col = Color.Lerp(Color.DeepPink * 1.25f, between2, Easings.easeOutCubic(progress)) * overallAlpha;

                Vector2 lineScale = new Vector2(1f, 1f) * progress * overallScale * Projectile.scale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, col with { A = 0 } * 0.9f * progress,
                    previousRotations[i], line.Size() / 2f, lineScale, SpriteEffects.None);

                Vector2 innerScale = new Vector2(1f, 1f * 0.1f) * progress * overallScale * Projectile.scale;
                Main.EntitySpriteDraw(line, flarePos + offset1, null, Color.White with { A = 0 } * 1f * progress * overallAlpha,
                    previousRotations[i], line.Size() / 2f, innerScale, SpriteEffects.None);
            }
        }
    }

    public class TrojanForceDagger : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = true;

            Projectile.scale = 0.95f;
        }

        float justShotVal = 1f;

        int timer = 0;
        public override void AI()
        {
            int trailCount = 14; //12 | 26
            previousRotations.Add(Projectile.velocity.ToRotation());
            previousPositions.Add(Projectile.Center + Projectile.velocity);

            if (previousRotations.Count > trailCount)
                previousRotations.RemoveAt(0);

            if (previousPositions.Count > trailCount)
                previousPositions.RemoveAt(0);


            if (FindTarget() && timer > 5 && false)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            }

            if (timer < 70)
            {
                //Home a little stronger after half a second
                float turnPower = 25f;
                int turn2 = timer < 50f ? 30 : 35;

                Vector2 mousePos = Vector2.Zero;

                if (Main.myPlayer == Projectile.owner)
                    mousePos = Main.MouseWorld;

                Vector2 toMouse = (mousePos - Projectile.Center).SafeNormalize(Vector2.UnitX);
                toMouse *= turnPower;

                Projectile.velocity = (Projectile.velocity * (turn2 - 1) + toMouse) / turn2;
                if (Projectile.velocity.Length() < 10f)
                {
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= 10f;
                }

                Projectile.velocity *= 0.01f;
            }
            else if (timer < 90)
                Projectile.velocity *= 1.3f;

            Projectile.velocity = Projectile.velocity.RotatedBy(0.11f);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;


            Lighting.AddLight(Projectile.Center, Color.DeepPink.ToVector3());

            if (timer > 5)
                justShotVal = Math.Clamp(MathHelper.Lerp(justShotVal, -0.5f, 0.04f), 0f, 1f);

            float timeForPopInAnim = 20;
            float animProgress = Math.Clamp((timer + 6) / timeForPopInAnim, 0f, 1f);

            overallScale = MathHelper.Lerp(0f, 1f, Easings.easeInOutBack(animProgress, 0f, 1.15f));


            timer++;
        }

        public NPC target;
        public bool FindTarget(float maxTargetDist = 1000f)
        {
            foreach (NPC npc in Main.ActiveNPCs) //scan all targets for anything less than maxTargetDist units away
            {
                if (!npc.friendly && npc.CanBeChasedBy(Projectile) && Vector2.Distance(npc.Center, Projectile.Center) < maxTargetDist)
                {
                    target = npc;
                    return true;
                }
            }
            return false;
        }


        public override bool PreKill(int timeLeft)
        {
            return base.PreKill(timeLeft);
        }

        float overallAlpha = 1f;
        float overallScale = 1f;

        public List<float> previousRotations = new List<float>();
        public List<Vector2> previousPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(false);
            });
            //Interesting style
            //DrawTrail(false);

            Texture2D Tex = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/ShadowBlade").Value;
            Texture2D TexBorder = Mod.Assets.Request<Texture2D>("Content/VFXTest/Aero/TrojanForce/ShadowBladeOuter").Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 15;

            Color borderCol = Color.Lerp(Color.DeepPink * 0.9f, Color.White, justShotVal);
            Vector2 daggerScale = new Vector2(1f - (justShotVal * 0.5f), 1f) * Projectile.scale * overallScale;

            Main.EntitySpriteDraw(TexBorder, drawPos, null, borderCol, Projectile.rotation, TexBorder.Size() / 2f, daggerScale, SpriteEffects.None);

            Main.EntitySpriteDraw(Tex, drawPos, null, Color.White, Projectile.rotation, Tex.Size() / 2f, daggerScale, SpriteEffects.None);

            return false;
        }

        Effect trailEffect = null;
        public void DrawTrail(bool giveUp)
        {
            if (giveUp)
                return;

            // 12 trailPositions
            
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Pixel/SoulSpikeHalf").Value;

            trailEffect ??= ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.09f) * 0.15f;

            Color StripColor(float progress) => Color.White * (progress * progress);
            float StripWidthUnder(float progress) => 18f * Easings.easeOutCubic(progress) * overallScale * sineWidthMult * 1.15f;
            float StripWidthOver(float progress) => 12f * Easings.easeOutCubic(progress) * overallScale * sineWidthMult * 1.15f;

            VertexStrip vertexStripUnder = new VertexStrip();
            vertexStripUnder.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidthUnder, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStripOver = new VertexStrip();
            vertexStripOver.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidthOver, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue(timer * 0.05f * 0f);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            Color underCol = Color.Lerp(Color.DeepPink, Color.White, justShotVal);
            trailEffect.Parameters["ColorOne"].SetValue(underCol.ToVector3() * 1.5f);
            trailEffect.Parameters["glowThreshold"].SetValue(1f);
            trailEffect.Parameters["glowIntensity"].SetValue(1f);
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStripUnder.DrawTrail();

            //Over layer
            trailEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 0.75f);
            trailEffect.Parameters["glowThreshold"].SetValue(1f); //0.7
            trailEffect.Parameters["glowIntensity"].SetValue(1f); //2
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStripOver.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            

            // 26 trail positions
            /*
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/FireEdge").Value;

            trailEffect ??= ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;

            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousPositions.ToArray();
            float[] rot_arr = previousRotations.ToArray();

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.09f) * 0.15f;

            Color StripColor(float progress) => Color.White * (progress * progress);
            float StripWidthUnder(float progress) => 20f * Easings.easeOutCubic(progress) * overallScale * sineWidthMult * 0.5f;
            float StripWidthOver(float progress) => 12f * Easings.easeOutCubic(progress) * overallScale * sineWidthMult * 0.5f;

            VertexStrip vertexStripUnder = new VertexStrip();
            vertexStripUnder.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidthUnder, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStripOver = new VertexStrip();
            vertexStripOver.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidthOver, -Main.screenPosition, includeBacksides: true);

            trailEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            trailEffect.Parameters["progress"].SetValue(timer * 0.05f * 0f);
            trailEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            trailEffect.Parameters["reps"].SetValue(1f);

            //UnderLayer
            trailEffect.Parameters["ColorOne"].SetValue(Color.DeepPink.ToVector3() * 1f);
            trailEffect.Parameters["glowThreshold"].SetValue(1f);
            trailEffect.Parameters["glowIntensity"].SetValue(1f);
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStripUnder.DrawTrail();

            //Over layer
            trailEffect.Parameters["ColorOne"].SetValue(Color.Black.ToVector3() * 1f);
            trailEffect.Parameters["glowThreshold"].SetValue(1f); //0.7
            trailEffect.Parameters["glowIntensity"].SetValue(1f); //2
            trailEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStripOver.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            */
        }
    }
}