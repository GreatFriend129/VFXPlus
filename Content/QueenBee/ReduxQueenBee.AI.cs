using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils.Cil;
using rail;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using VFXPlus.Common;
using VFXPlus.Common.Utilities;
using VFXPlus.Content.Dusts;
using VFXPLus.Common;

namespace VFXPlus.Content.QueenBee
{
    public partial class ReduxQueenBee : ModNPC
    {
        Vector2 dummyGoalPos = Vector2.Zero;
        public void Dummy() 
        {
            if (timer == 0)
            {
                NPC.velocity = Vector2.Zero;

                dummyGoalPos = Main.rand.NextVector2CircularEdge(300f, 300f);

            }

            BasicMovement(player.Center + dummyGoalPos, 5f, 1240f);

            if (timer == 200)
            {
                timer = -1;
            }

        }




        #region MovementCode
        void BasicMovement(Vector2 goalPos, float moveSpeed = 6f, float clampDistance = 240f)
        {
            NPC.velocity *= 0.875f;

            Vector2 trueGoal = goalPos - NPC.Center;
            Vector2 lerpGoal = Vector2.Lerp(NPC.Center, goalPos, 0.8f);

            Vector2 goTo = trueGoal;
            float speed = moveSpeed * MathHelper.Clamp(goTo.Length() / clampDistance, 0, 1);
            if (goTo.Length() < speed)
            {
                speed = goTo.Length();
            }
            NPC.velocity += goTo.SafeNormalize(Vector2.Zero) * speed;

        }
    }
    #endregion
}
