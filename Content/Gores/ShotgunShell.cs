using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using VFXPlus.Common;
using static Terraria.NPC;

namespace VFXPlus.Content.Gores
{
    public class ShotgunShell : ModGore
    {
        public override string Texture => "VFXPlus/Content/Gores/ShotgunShell";
        public override bool Update(Gore gore)
        {

            if (gore.frameCounter == 0)
            {
                gore.rotation = Main.rand.NextFloat(6.28f);
                gore.alpha = 255;
            }

            float sizeProg = Utils.GetLerpValue(0, 255, gore.alpha, true);


            if (gore.frameCounter <= 60)
            {
                gore.alpha -= 20;

                gore.scale = 0.625f * Easings.easeInOutBack(1f - sizeProg, 0f, 2f);

            }
            else if (gore.frameCounter > 60)
            {
                gore.alpha += 10;

                gore.scale = 0.625f * Easings.easeInOutQuad(1f - sizeProg);

                if (gore.alpha >= 250)
                    gore.active = false;
            }

            gore.alpha = Math.Clamp(gore.alpha, 0, 255);


            gore.velocity.X *= 0.99f;

            //Play sound if we hit a tile
            if (gore.velocity.Y == 0 && gore.light == 0 && gore.scale > 0.15f)
            {
                SoundStyle style = new SoundStyle("Terraria/Sounds/Coin_3") with { Volume = 0.018f, Pitch = -1f, PitchVariance = 0.1f, MaxInstances = -1 }; 
                SoundEngine.PlaySound(style, gore.position);

                gore.light = -0.01f;

                gore.velocity.X *= 0.5f;
            }

            gore.frameCounter++;
            return base.Update(gore);
        }

    }

    public class ShotgunShellOnyx : ShotgunShell
    {
        public override string Texture => "VFXPlus/Content/Gores/ShotgunShellOnyx";
    }
}

