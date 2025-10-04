using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace VFXPlus.Content.QueenBee
{
    public partial class ReduxQueenBee : ModNPC
    {
        public const string AssetDirectory = "VFXPlus/Content/QueenBee/Assets/";
        public override string Texture => "Terraria/Images/Projectile_0";


        private enum QueenBeeState
        {
            Dummy = -1,
            SpawnAnim = 0,
            VanillaDashes = 1,
            WalledDashes = 2,
            AsgoreCircle = 3, 
            NamaahWall = 4, 
            Galaga = 5,
            Sweep = 6, 
            ForkedBurst = 7
        }

        private QueenBeeState CurrentAttack
        {
            get => (QueenBeeState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        public override void SetStaticDefaults() { }

        public override bool CheckActive() { return false; }

        public override void SetDefaults()
        {
            NPC.lifeMax = 4000;
            NPC.width = 80;
            NPC.height = 80;
            NPC.damage = 0;

            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;

            NPC.knockBackResist = 0;
            NPC.npcSlots = 10;
        }
        public Player player => Main.player[NPC.target];


        public int timer = 0;

        //Does not get reset upon changing attacks
        int universalTimer = 0;

        public int substate = 0;
        public int attackReps = 0;

        bool isDashing = false;
        bool firstFrame = true;
        public override void AI()
        {
            if (firstFrame)
            {
                firstFrame = false;
            }

            NPC.dontTakeDamage = false;
            NPC.hide = false;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            Main.NewText("ignore me");

            //Trispin, Dive, UmbrellaRain, CornerTravelShot, Martlet, Madison, Offscreen

            CurrentAttack = QueenBeeState.Dummy;

            switch (CurrentAttack)
            {
                case QueenBeeState.Dummy:
                    Dummy();
                    break;
            }

            //Basic After-Image
            int trailCount = 10; //10
            previousPositions.Add(NPC.Center);
            previousRotations.Add(NPC.rotation);

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            if (universalTimer % 6 == 0)
                NPC.frameCounter++;

            universalTimer++;

            timer++;
        }

    }
}
