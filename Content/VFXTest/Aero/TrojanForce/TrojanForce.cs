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

namespace VFXPlus.Content.VFXTest.Aero.TrojanForce
{
	public class TrojanForce : ModItem
	{
        public override string Texture => "VFXPlus/Content/VFXTest/Aero/TrojanForce/TrojanForce";

        public override void SetDefaults()
		{
            Item.DefaultToWhip(ModContent.ProjectileType<TrojanForceWhipProj>(), 65, 2f, 4f, 30);
        }
		public override bool MeleePrefix() => true;
	}

    public class TrojanForceWhipProj : ModProjectile
    {
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
            Projectile.width = Projectile.height = 18;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.penetrate = -1;

            Projectile.WhipSettings.Segments = whipSegments; //14
            Projectile.WhipSettings.RangeMultiplier = 2f;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

            Timer++;

            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation * 2 <= 0)
            {
                Projectile.Kill();
                return;
            }

            owner.heldProj = Projectile.whoAmI;
            if (Timer == swingTime / 2)
            {
                // Plays a whipcrack sound at the tip of the whip.
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                SoundEngine.PlaySound(SoundID.Item153, points[points.Count - 1]);
            }

            float swingProgress = Timer / swingTime;
            if (Utils.GetLerpValue(0.1f, 0.7f, swingProgress, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, clamped: true) > 0.5f)
            {
                List<Vector2> points = Projectile.WhipPointsForCollision;
                points.Clear();
                Projectile.FillWhipControlPoints(Projectile, points);
                int pointIndex = Main.rand.Next(points.Count - 6, points.Count);
                Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(15f, 15f));
                int dustType = ModContent.DustType<PixelatedLineSpark>();

                // After choosing a randomized dust and a whip segment to spawn from, dust is spawned.
                Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, dustType, 0f, 0f, 50, Color.DeepSkyBlue, Main.rand.NextFloat(0.5f, 0.65f) * 0.5f);
                dust.position = points[pointIndex];
                Vector2 spinningpoint = points[pointIndex] - points[pointIndex - 1];
                dust.velocity *= 0.5f;
                dust.noGravity = true;
                // This math causes these dust to spawn with a velocity perpendicular to the direction of the whip segments, giving the impression of the dust flying off like sparks.
                dust.velocity += spinningpoint.RotatedBy(owner.direction * ((float)Math.PI / 2f));
                dust.velocity *= 0.2f;
                dust.velocity += Projectile.velocity;

                dust.position -= dust.velocity * 5f;

                dust.customData = DustBehaviorUtil.AssignBehavior_LSBase(velFadePower: 0.93f, preShrinkPower: 0.99f, postShrinkPower: 0.89f, timeToStartShrink: 8 + Main.rand.Next(-5, 5), killEarlyTime: 40,
                        0.75f, 0.5f, shouldFadeColor: false);

                //ElectricSparkBehavior esb = new ElectricSparkBehavior(FadeAlphaPower: 0.89f, FadeScalePower: 0.98f, FadeVelPower: 0.92f, Pixelize: true, XScale: 1f, YScale: 0.75f); //0.91
                //dust.customData = esb;
            }

            if (Timer > 10 && false)
            {

                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
                if (Main.player[Projectile.owner].Distance(points[points.Count - 1]) > 50)
                {
                    Vector2 vel = (points[points.Count - 1] - Projectile.Center).SafeNormalize(Vector2.UnitX) * 5;

                    Dust dust = Dust.NewDustPerfect(points[points.Count - 1], ModContent.DustType<GlowPixelCross>(), newColor: Color.DeepSkyBlue, Scale: 0.35f);
                    dust.position -= vel * 8;

                    dust.velocity = vel;


                    //int dust = Dust.NewDust(points[points.Count - 1], Projectile.width / 2, Projectile.height / 2, ModContent.DustType<GlowPixelAlts>(), newColor: Color.DeepSkyBlue);
                    //Main.dust[dust].velocity = (points[points.Count - 1] - Projectile.Center).SafeNormalize(Vector2.UnitX) * 2;
                    //Main.dust[dust].noGravity = true;
                    //Main.dust[dust].alpha = 2;
                }

            }

            #region store TipperInfo

            #region store TipperInfo
            if (swingProgress >= 0.4f && swingProgress <= 0.85f) // 0.7f | 0.3
            {
                int trailCount = 6; //12
                List<Vector2> w_points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, w_points);

                Vector2 tip = w_points[w_points.Count - 2];
                Vector2 diff = w_points[w_points.Count - 1] - tip;
                float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.

                previousTipPositions.Add(tip);

                if (previousTipPositions.Count > trailCount)
                    previousTipPositions.RemoveAt(0);
            }
            else
            {
                previousTipPositions.Clear();
            }
            #endregion
            #endregion

        }


        public List<Vector2> previousTipPositions = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            float swingTime = Main.player[Projectile.owner].itemAnimationMax * Projectile.MaxUpdates;
            float swingProgress = Timer / swingTime;
           
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list);

            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.UnderProjectiles, () =>
            {
                DrawTrail(true);
                DrawAfterImage(false);
            });
            DrawTrail(true);
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

            Texture2D Core = (Texture2D)ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel/PartiGlow");

            Projectile.GetWhipSettings(Projectile, out float timeToFlyOut1, out int _, out float _);
            float t1 = Timer / timeToFlyOut1;
            float coreScale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t1, true) * Utils.GetLerpValue(0.9f, 0.7f, t1, true)) * 0.75f;

            for (int i = 0; i < previousTipPositions.Count; i++)
            {
                float progress = (float)i / previousTipPositions.Count;

                Vector2 tipPos = previousTipPositions[i];
                float tipRot = 0f;/// previousTipRotations[i] + ((float)Main.timeForVisualEffects * 0.06f);

                float colOpacity = Easings.easeInCirc(progress) * 1f;

                Color col = Color.Lerp(Color.DeepPink, Color.HotPink, Easings.easeInCubic(progress)) with { A = 0 } * Easings.easeInCirc(progress) * 1f;

                float finalTrailScale = coreScale * 0.7f;// * progress * 0.2f;


                Main.EntitySpriteDraw(Core, tipPos - Main.screenPosition, null, col, tipRot, Core.Size() / 2f, finalTrailScale, 0);
                Main.EntitySpriteDraw(Core, tipPos - Main.screenPosition, null, Color.White with { A = 0 } * colOpacity, tipRot, Core.Size() / 2f, finalTrailScale * 0.35f, 0);

                //Draw two after images in between each tip
                if (i != 0)
                {
                    Vector2 previousTipPos = previousTipPositions[i - 1];

                    Vector2 inBetweenPos = Vector2.Lerp(tipPos, previousTipPos, 0.33f);

                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, col, 0f, Core.Size() / 2f, finalTrailScale, 0);
                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, Color.White with { A = 0 } * colOpacity, 0f, Core.Size() / 2f, finalTrailScale * 0.35f, 0);

                    inBetweenPos = Vector2.Lerp(tipPos, previousTipPos, 0.66f);

                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, col, 0f, Core.Size() / 2f, finalTrailScale, 0);
                    Main.EntitySpriteDraw(Core, inBetweenPos - Main.screenPosition, null, Color.White with { A = 0 } * colOpacity, 0f, Core.Size() / 2f, finalTrailScale * 0.35f, 0);

                }
            }

        }


        float overallScale = 1f;
        float overallAlpha = 1f;
        Effect myEffect = null;
        public void DrawTrail(bool giveUp = false)
        {
            if (giveUp)
                return;

            /*
            Texture2D trailTexture = Mod.Assets.Request<Texture2D>("Assets/Trails/ThinGlowLine").Value; //
            Texture2D trailTexture2 = Mod.Assets.Request<Texture2D>("Assets/Trails/s06sBloom").Value; //

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("VFXPlus/Effects/TrailShaders/TendrilShader", AssetRequestMode.ImmediateLoad).Value;


            //Smooth out all the rots and points
            for (int i = 1; i < previousTipRotations.Count; i++)
            {
                //0 = earliest made point

                Vector2 currentPoint = previousTipRotations[i].ToRotationVector2();
                Vector2 prevPoint = previousTipRotations[i - 1].ToRotationVector2();

                previousTipRotations[i] = Vector2.Lerp(currentPoint, prevPoint, 0f).ToRotation();
            }


            //Convert lists to arrays for use in vertex strip
            Vector2[] pos_arr = previousTipPositions.ToArray();
            float[] rot_arr = previousTipRotations.ToArray();


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
            myEffect.Parameters["reps"].SetValue(2f);


            //Over layer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture);
            myEffect.Parameters["ColorOne"].SetValue(Color.HotPink.ToVector3() * 2f);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1.2f);
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip.DrawTrail();

            //UnderLayer
            myEffect.Parameters["TrailTexture"].SetValue(trailTexture2);
            myEffect.Parameters["glowThreshold"].SetValue(1f);
            myEffect.Parameters["glowIntensity"].SetValue(1f);
            myEffect.Parameters["ColorOne"].SetValue(Color.HotPink.ToVector3() * 4.5f); //Hotpink4.5
            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip2.DrawTrail();

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            */

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