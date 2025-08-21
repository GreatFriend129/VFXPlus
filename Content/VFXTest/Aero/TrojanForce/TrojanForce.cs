using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using VFXPlus.Content.Dusts;
using VFXPlus.Common.Utilities;
using VFXPlus.Common;
using VFXPlus.Common.Drawing;
using ReLogic.Content;
using Terraria.Graphics;
using Terraria.GameContent.Animations;
using Terraria.DataStructures;
using System.Linq;
using static tModPorter.ProgressUpdate;
using Terraria.Modules;
using SteelSeries.GameSense;

namespace VFXPlus.Content.VFXTest.Aero.TrojanForce
{
	public class TrojanForce : ModItem
	{
        public override string Texture => "VFXPlus/Content/VFXTest/Aero/TrojanForce/TrojanForce";

        public override void SetDefaults()
		{ 
            Item.DefaultToWhip(ModContent.ProjectileType<TrojanForceWhipProj>(), 65, 3f, 2.75f, 36);
        }
		public override bool MeleePrefix() => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return !Main.projectile.Any(n => n.active && n.type == type && n.owner == player.whoAmI);
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
            
            for (int i = 220; i < previousTipPositions.Count; i++)
            {
                /*
                float progress = (float)i / previousTipPositions.Count;

                Vector2 tipPos = previousTipPositions[i];
                float tipRot = 0f;/// previousTipRotations[i] + ((float)Main.timeForVisualEffects * 0.06f);

                float colOpacity = Easings.easeInCirc(progress) * 1f;

                Color col = Color.Lerp(Color.DeepPink, Color.HotPink, Easings.easeInCubic(progress)) with { A = 0 } * Easings.easeInCirc(progress) * 1f;

                float finalTrailScale = coreScale * 0.7f;// * progress * 0.2f;


                Main.EntitySpriteDraw(Core, tipPos - Main.screenPosition, null, col, tipRot, Core.Size() / 2f, finalTrailScale, 0);
                Main.EntitySpriteDraw(Core, tipPos - Main.screenPosition, null, Color.White with { A = 0 } * colOpacity, tipRot, Core.Size() / 2f, finalTrailScale * 0.35f, 0);

                //Draw two after images in between each tip
                if (i != 0 && false)
                {
                    Vector2 previousTipPos = previousTipPositions[i - 1];

                    Vector2 inBetweenPos = Vector2.Lerp(tipPos, previousTipPos, 0.33f);

                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, col, 0f, Core.Size() / 2f, finalTrailScale, 0);
                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, Color.White with { A = 0 } * colOpacity, 0f, Core.Size() / 2f, finalTrailScale * 0.35f, 0);

                    inBetweenPos = Vector2.Lerp(tipPos, previousTipPos, 0.66f);

                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, col, 0f, Core.Size() / 2f, finalTrailScale, 0);
                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, Color.White with { A = 0 } * colOpacity, 0f, Core.Size() / 2f, finalTrailScale * 0.35f, 0);

                }
                */
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
            Projectile.DefaultToWhip();

            Projectile.WhipSettings.Segments = whipSegments; //14
            Projectile.WhipSettings.RangeMultiplier = 1f;

            //Projectile.extraUpdates = 6;
        }

        float flyTime;
        public bool HitboxActive => Utils.GetLerpValue(0.1f, 0.7f, Projectile.ai[0] / flyTime, true) * Utils.GetLerpValue(0.9f, 0.7f, Projectile.ai[0] / flyTime, true) > 0.5f;

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            flyTime = player.itemAnimationMax * Projectile.MaxUpdates;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.ai[0]++;
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
                Projectile.FillWhipControlPoints(Projectile, Projectile.WhipPointsForCollision);
                Vector2 position = Projectile.WhipPointsForCollision[^1];
                SoundEngine.PlaySound(SoundID.Item153, position);
            }

            if (HitboxActive)
            {
                Projectile.WhipPointsForCollision.Clear();
                Projectile.FillWhipControlPoints(Projectile, Projectile.WhipPointsForCollision);
            }

            if (Projectile.ai[0] - 1 == 1)
            {
                trailPosList.Add(oldTip);
                trailPosList.Add(TipPosition);

                float rot = (TipPosition - oldTip).ToRotation();
                trailRotList.Add(rot);
                trailRotList.Add(rot);
            }
            else if (Projectile.ai[0] - 1 > 1)
            {
                int thisTrailCount = 30;

                Vector2 currentTip = TipPosition;

                float newRot = (TipPosition - oldTip).ToRotation();
                float oldRot = trailRotList[trailRotList.Count - 1];

                trailPosList.Add(currentTip);
                if (trailPosList.Count > thisTrailCount)
                    trailPosList.RemoveAt(0);

                trailRotList.Add(newRot);
                if (trailPosList.Count > thisTrailCount)
                    trailPosList.RemoveAt(0);
            }

            oldTip = TipPosition;

            return false;
        }

        Vector2 oldTip = Vector2.Zero;

        public override bool PreDraw(ref Color lightColor)
        {
            float swingTime = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
            float swingProgress = Timer / swingTime;

            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(false);
            });
            DrawTrail(true);


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



        public List<Vector2> trailPosList = new List<Vector2>();
        public List<float> trailRotList = new List<float>();

        bool firstTrailFrame = true;
        float overallScale = 1f;
        float overallAlpha = 1f;
        Effect myEffect = null;
        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp)
                return;

            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/Extra_196_Black").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = trailPosList.ToArray();
            float[] rot_arr = trailRotList.ToArray();


            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0f;


            Color StripColor(float progress) => Color.White * Easings.easeOutCubic(progress) * overallAlpha;

            float StripWidth(float progress)
            {
                float toReturn = 0f;
                if (progress < 0.5f) //Back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = 1f;// Easings.easeOutSine(1f - LV);
                }

                return toReturn * sineWidthMult * overallScale * 120f * 0.5f; //50
            }

            float StripWidth2(float progress)
            {
                return 40f;

                float toReturn = 0f;
                if (progress < 0.5f) //Back half
                {
                    float LV = Utils.GetLerpValue(0f, 0.5f, progress, true);
                    toReturn = Easings.easeOutSine(LV);
                }
                else //Front half
                {
                    float LV = Utils.GetLerpValue(0.5f, 1f, progress, true);
                    toReturn = 1f;// Easings.easeOutSine(1f - LV);
                }

                return toReturn * overallScale * 40f * 0.5f; //50
            }


            VertexStrip vertexStrip = new VertexStrip();
            vertexStrip.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);

            VertexStrip vertexStrip2 = new VertexStrip();
            vertexStrip2.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth2, -Main.screenPosition, includeBacksides: true);


            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
            myEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.035f); //0.02
            myEffect.Parameters["reps"].SetValue(1f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.DeepPink.ToVector3() * 2f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1.2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            //vertexStrip.DrawTrail();

            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(Color.HotPink.ToVector3() * 2.5f); //Hotpink4.5
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
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

}