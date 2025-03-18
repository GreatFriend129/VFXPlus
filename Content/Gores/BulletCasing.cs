using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using static Terraria.NPC;

namespace AerovelenceMod.Content.Items.Weapons.AreaPistols.ErinGun
{
    public class BulletCasing : ModGore
    {
        public override string Texture => "VFXPlus/Content/Gores/BulletCasing";
        public override bool Update(Gore gore)
        {

            if (gore.frameCounter == 0)
                gore.alpha = 255;

            if (gore.frameCounter <= 30)
                gore.alpha -= 24;
            if (gore.frameCounter > 30)
            {
                gore.alpha += 10;

                if (gore.alpha >= 250)
                    gore.active = false;
            }
            
            gore.frameCounter++;


            if (gore.velocity.Y == 0 && gore.light == 0)
            {
                SoundStyle style = new SoundStyle("Terraria/Sounds/Coin_3") with { Volume = 0.018f, Pitch = -1f, PitchVariance = 0.1f, MaxInstances = -1 };
                SoundEngine.PlaySound(style, gore.position);

                gore.light = -0.01f;
            }

            return base.Update(gore);
        }

    }

    public class PurpleCasing : BulletCasing
    {
        public override string Texture => "VFXPlus/Content/Gores/PurpleCasing";
    }

    public class AquaCasing : BulletCasing
    {
        public override string Texture => "VFXPlus/Content/Gores/AquaCasing";
    }
    public class GreenCasing : BulletCasing
    {
        public override string Texture => "VFXPlus/Content/Gores/GreenCasing";
    }
}

