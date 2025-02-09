using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using VFXPlus.Common.Interfaces;

namespace VFXPlus.Common
{
    public class ArrowILEdits : ModSystem
    {
        public override void OnModLoad()
        {
            IL_Projectile.Update += RemoveFlamingArrowDust;
        }

        // So for some reason a lot of arrows have their dust run in the Update function instead of AI
        // This IL Edit is designed to remove that dust
        private void RemoveFlamingArrowDust(ILContext il)
        {

            ILCursor c = new ILCursor(il);

            
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }
            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);


            //WORKS FOR HOSTILE FIRE ARROW

            /*
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(82)))
            {
                VFXPlus.Instance.Logger.Warn("RemoveFlamingArrowDust Edit failed.");
                return;
            }

            //Effectively appends '&& false' to the if statement, making it never run 
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.And);
            */
        }
    }
}
