using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common.Drawing;
using static Terraria.GameContent.TextureAssets;

namespace VFXPlus.Content.VFXTest
{
    public class GalaxyTest : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1500;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = false;

            Projectile.tileCollide = false;
            Projectile.timeLeft = 12250; //180
        }



        int timer = 0;
        public float overallAlpha = 1f;
        public float overallScale = 1f;

        public override void AI()
        {
            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            ModContent.GetInstance<PixelationSystem>().QueueRenderAction(RenderLayer.OverPlayers, () =>
            {
                DrawVertexTrail(false);
            });
            DrawVertexTrail(true);

            return false;
        }

        Effect myEffect = null;
        public void DrawVertexTrail(bool giveUp)
        {
            if (giveUp)
                return;

            Effect trailEffect = ModContent.Request<Effect>("VFXPlus/Effects/Air/Galaxy3", AssetRequestMode.ImmediateLoad).Value;

            Vector3[] cols2 =
            {
                Color.DeepPink.ToVector3() * 1f,
                Color.Lerp(Color.DeepPink, Color.HotPink, 0.5f).ToVector3(),
                Color.HotPink.ToVector3() * 1f,
                Color.Lerp(Color.HotPink, Color.Pink, 0.5f).ToVector3(),
                Color.Pink.ToVector3() * 1f,
                Color.LightPink.ToVector3() * 1f,
            };

            Texture2D pixel = ModContent.Request<Texture2D>("VFXPlus/Assets/Pixel").Value;

            trailEffect.Parameters["progress"].SetValue((float)Main.timeForVisualEffects * 0.02f);
            trailEffect.Parameters["posterizationSteps"].SetValue(6.0f);
            trailEffect.Parameters["zoom"].SetValue(5.0f);

            trailEffect.Parameters["gradColors"].SetValue(cols2);
            trailEffect.Parameters["numberOfColors"].SetValue(cols2.Length);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, trailEffect, Main.GameViewMatrix.EffectMatrix);

            Main.spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, null, Color.White, 0f, pixel.Size() / 2f, 1000f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        }

    }

}