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
using System.Runtime.InteropServices;
using Terraria.GameContent;
using VFXPlus.Common.Drawing;


namespace VFXPlus.Content.Projectiles
{
    public class ConstellationTrail : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private class Constellation
        {
            public Vector2 position;
            public Vector2 velocity;
            public float alpha = 1f;
            public float scale = 1f;

            public List<int> connectingIndices = new List<int>();

            public int lifeTime = 0;
            public int timer = 0;
            public void Update()
            {
                float fadeInTime = Math.Clamp((timer + 3f) / 25f, 0f, 1f);
                scale = Easings.easeInOutBack(fadeInTime, 0f, 1f);

                if (timer > 9)
                    alpha -= 0.06f;

                velocity *= 0.95f; //95
                position += velocity;

                timer++;
            }

            public Constellation(Vector2 position, Vector2 velocity)
            {
                this.position = position;
                this.velocity = velocity;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.hide = true;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 250; //180
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public int parentType;
        public Color starCol = Color.DodgerBlue;
        public Color lineCol = Color.DodgerBlue;
        public int timeBetweenStars = 4;

        bool deactivated = false;
        int timer = 0;

        List<Constellation> constellations = new List<Constellation>();
        public override void AI()
        {
            Projectile parent = Main.projectile[(int)Projectile.ai[0]];

            if (parent.active && parent.type == parentType && parent.ai[2] == timer + 1)
            {
                Projectile.Center = parent.Center;
                Projectile.velocity = parent.velocity;
            }
            else
            {
                deactivated = true;
            }

            if (timer == 0)
            {
                //Which side should the constellation start on
                Projectile.ai[1] = Main.rand.NextBool() ? 1f : -1f;

                //Delay before first constellation
                Projectile.ai[2] = Main.rand.Next(0, 4);
            }

            if ((timer + (int)Projectile.ai[2]) % timeBetweenStars == 0 && !deactivated)
            {
                Vector2 dir = Projectile.velocity.RotatedBy(MathHelper.PiOver2).RotatedByRandom(1f).SafeNormalize(Vector2.UnitX);

                Vector2 randomDir = dir * Main.rand.NextFloat(4f, 8f) * Projectile.ai[1];
                constellations.Add(new Constellation(Projectile.Center + randomDir * Main.rand.NextFloat(1f, 3f), (Projectile.velocity * 0.1f) + (randomDir * 0.1f))); //* Main.rand.NextFloat(1f, 3f)

                if (constellations.Count > 10)
                    constellations.RemoveAt(0);

                Projectile.ai[1] *= -1;
            }

            for (int i = 0; i < constellations.Count; i++)
            {
                constellations[i].Update();
            }

            //Delete projectile if deactivated and all constellations are under alpha
            if (deactivated)
            {
                bool shouldKill = true;
                for (int i = 0; i < constellations.Count; i++)
                {
                    if (constellations[i].alpha > 0.1f)
                        shouldKill = false;
                }

                if (shouldKill)
                    Projectile.Kill();
            }

            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= Projectile.ai[2])
                return false;

            Texture2D Pixel = CommonTextures.CrispStarPMA.Value;
            Texture2D Line = CommonTextures.SoulSpikePMA.Value;

            Color betweenBlue = Color.Lerp(Color.DodgerBlue, Color.DeepSkyBlue, 0f);
            Color betweenPink = Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f);

            if (parentType == ProjectileID.StarCannonStar)
            {
                Color deepBlue = Color.Lerp(Color.DodgerBlue, Color.Blue, 0.85f);
                Color golden = Color.Lerp(Color.Gold, Color.Orange, 0.75f);
                float yellowValue = GeneralUtilities.OscSine(0f, 0.85f, 2f, 0f);
                lineCol = Color.Lerp(deepBlue with { A = 175 }, golden with { A = 175 }, yellowValue);
                starCol = Color.Lerp(deepBlue with { A = 160 }, golden with { A = 160 }, yellowValue);
            }

            //Draw connections
            for (int i = 1; i < constellations.Count; i++)
            {
                Constellation currentConst = constellations[i];
                Constellation previousConst = constellations[i - 1];

                if (previousConst.alpha > 0.1f)
                {
                    Vector2 between = (currentConst.position - previousConst.position);
                    Vector2 scale = new Vector2(previousConst.scale * (between.Length() / Line.Width), 0.65f * currentConst.scale);

                    Main.spriteBatch.Draw(Line, previousConst.position - Main.screenPosition, null, lineCol * previousConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(Line, previousConst.position - Main.screenPosition, null, Color.White with { A = 175 } * previousConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), new Vector2(scale.X, scale.Y * 0.25f), SpriteEffects.None, 0f);
                }
            }

            //Draw connect from last point to parent
            if (!deactivated && constellations.Count > 0)
            {
                Constellation finalConst = constellations.Last();
                if (finalConst.alpha > 0.1f)
                {
                    Vector2 between = (Projectile.Center - finalConst.position);
                    Vector2 scale = new Vector2(finalConst.scale * (between.Length() / Line.Width), 0.65f * finalConst.scale);

                    Main.spriteBatch.Draw(Line, finalConst.position - Main.screenPosition, null, lineCol * finalConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), scale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(Line, finalConst.position - Main.screenPosition, null, Color.White with { A = 175 } * finalConst.alpha, between.ToRotation(), new Vector2(0f, Line.Height / 2f), new Vector2(scale.X, scale.Y * 0.25f), SpriteEffects.None, 0f);
                }
            }


            //Draw stars
            for (int i = 0; i < constellations.Count; i++)
            {
                Constellation currentConst = constellations[i];

                float starScale = GeneralUtilities.OscSine(0.85f, 1.15f, 10f, 0f) * currentConst.scale;
                float starAlpha = Easings.easeOutQuad(currentConst.alpha);

                Main.spriteBatch.Draw(Pixel, currentConst.position - Main.screenPosition, null, starCol * starAlpha, 0f, Pixel.Size() / 2f, 0.5f * starScale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(Pixel, currentConst.position - Main.screenPosition, null, Color.White with { A = 160 } * starAlpha, 0f, Pixel.Size() / 2f, 0.25f * starScale, SpriteEffects.None, 0f);
            }

            return false;
        }

    }
}
