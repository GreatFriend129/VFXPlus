using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VFXPLus.Common
{
    public class ScreenShakePlayer : ModPlayer
    {
        public float ScreenShakePower;

        public override void ModifyScreenPosition()
        {
            if (ScreenShakePower > 0.1f) //Kackbrise#5454 <3 <3 Thank
            {
                //This runs less often at lower frame rates (and vice versa) so this normalizes that 
                float adjustedValue = ScreenShakePower * (Main.frameRate / 144f);

                float totalIntensity = adjustedValue * 1f;// ModContent.GetInstance<AeroClientConfig>().ScreenshakeIntensity;

                if (totalIntensity > 0)
                    Main.screenPosition += new Vector2(Main.rand.NextFloat(totalIntensity), Main.rand.NextFloat(totalIntensity));
                ScreenShakePower *= 0.9f;
            }
        }

    }
}
