using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static VFXPlus.Content.FeatheredFoe.FeatheredFoeBoss;

namespace VFXPlus.Content.QueenBee
{
    public partial class ReduxQueenBee : ModNPC
    {
        public const string AssetDirectory = "VFXPlus/Content/QueenBee/Assets/";
        public override string Texture => "Terraria/Images/Projectile_0";


        public enum QueenBeeState
        {
            Dummy = -1,
            SpawnAnim = 0,
            VanillaDashes = 1,
            WalledDashes = 2,
            AsgoreCircle = 3, 
            NamaahWall = 4, 
            Galaga = 5,
            Sweep = 6, 
            ForkedBurst = 7,
            RadialBurst = 8,
            OphanaimBees = 9,
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

            NPC.HitSound = SoundID.NPCHit1;
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
            //43aistyle

            if (firstFrame)
            {
                firstFrame = false;
                CurrentAttack = QueenBeeState.NamaahWall;
            }

            NPC.dontTakeDamage = false;
            NPC.hide = false;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(false);
            }

            CurrentAttack = QueenBeeState.NamaahWall;

            switch (CurrentAttack)
            {
                case QueenBeeState.Dummy:
                    Dummy();
                    break;
                case QueenBeeState.Sweep:
                    StingerSweep();
                    break;
                case QueenBeeState.NamaahWall:
                    NamaahWall();
                    break;
                case QueenBeeState.RadialBurst:
                    RadialBurst();
                    break;
                case QueenBeeState.VanillaDashes:
                    VanillaDashes();
                    break;
                case QueenBeeState.OphanaimBees:
                    OphanaimBees(); 
                    break;
                case QueenBeeState.WalledDashes:
                    WalledDashes();
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

        public void FacePlayer()
        {
            if (player.Center.X > NPC.Center.X)
                NPC.direction = 1;
            else
                NPC.direction = -1;
        }

        public List<QueenBeeState> previousAttacks = new List<QueenBeeState>();
        public void ChooseNextAttack()
        {
            //CurrentAttack = QueenBeeState.VanillaDashes;

            QueenBeeState[] options = {
                QueenBeeState.VanillaDashes,
                QueenBeeState.Sweep,
                QueenBeeState.OphanaimBees,
                QueenBeeState.NamaahWall,
                QueenBeeState.WalledDashes, 
                QueenBeeState.RadialBurst };

            int nextAttackIndex = Main.rand.Next(0, options.Length);
            CurrentAttack = options[nextAttackIndex];

            timer = -1;
            attackReps = 0;
            substate = 0;
            drawOphaLines = false;
            isDashing = false;

            //Attack Options
            /*
            FeatheredFoeState[] options = {
                FeatheredFoeState.TriSpin,
                FeatheredFoeState.Dive,
                FeatheredFoeState.CornerTravelShot,
                FeatheredFoeState.OffscreenDash,
                FeatheredFoeState.UmbrellaRain,
                FeatheredFoeState.WindDirShot,
                FeatheredFoeState.Madison  };


            previousAttacks.Add(CurrentAttack);

            //Only store the previous 3 attacks
            if (previousAttacks.Count >= 4)
                previousAttacks.RemoveAt(0);


            //Loop until we select an attack that isn't in the list
            bool hasFoundAttack = false;
            while (!hasFoundAttack)
            {
                int nextAttackIndex = Main.rand.Next(0, options.Length);

                if (!previousAttacks.Contains(options[nextAttackIndex]))
                {
                    CurrentAttack = options[nextAttackIndex];
                    hasFoundAttack = true;
                }
            }

            timer = -1;
            attackReps = 0;
            substate = 0;
            drawTriSpinStar = false;
            drawDrill = false;
            */
        }

    }
}
