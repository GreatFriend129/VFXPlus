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

namespace VFXPlus.Content.FeatheredFoe
{
    public partial class FeatheredFoeBoss : ModNPC
    {
        public const string AssetDirectory = "VFXPlus/Content/FeatheredFoe/Assets/";
        public override string Texture => "Terraria/Images/Projectile_0";


        private enum FeatheredFoeState
        {
            BasicAttack = 0,
            SwoopFeatherBehind = 1,
            TriSpin = 2, 
            MartletOrbitFeather = 3,
            CircleBurstFeather = 4,
            SwirlFeather = 5,
            CornerTravelShot = 6,
            MeleeTalon = 7,
            CircleDash = 8,
            Dive = 9,
            UmbrellaRain = 10,
            OffscreenDash = 11,
            Madison = 12,
            Maelstrom = 13,
            WindOrb = 14,
            WindDirShot = 15
        }

        private FeatheredFoeState CurrentAttack
        {
            get => (FeatheredFoeState)NPC.ai[0];
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

            NPC.aiStyle = -1;
            NPC.knockBackResist = 0;
            NPC.npcSlots = 10;
        }
        public Player player => Main.player[NPC.target];


        public int timer = 0;
        public int substate = 0;
        public int attackReps = 0;

        bool firstFrame = true;
        public override void AI()
        {
            if (firstFrame)
            {
                SkyManager.Instance.Activate("VFXPlus:FeatheredFoe");
                firstFrame = false;

                CurrentAttack = FeatheredFoeState.TriSpin;
            }

            NPC.dontTakeDamage = false;
            NPC.hide = false;

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest();
            }

            //Main.NewText(CurrentAttack + "|" + attackReps);

            //Trispin, Dive, UmbrellaRain, CornerTravelShot, Martlet, Madison, Offscreen, Maelstrom

            CurrentAttack = FeatheredFoeState.Madison;

            switch (CurrentAttack)
            {
                case FeatheredFoeState.BasicAttack:
                    BasicAttack();
                    break;
                case FeatheredFoeState.SwoopFeatherBehind: //Nothing
                    SwoopFeatherBehind();
                    break;
                case FeatheredFoeState.TriSpin: //GOOD | Easyish to hit
                    TriSpin();
                    break;
                case FeatheredFoeState.MartletOrbitFeather: //WIP
                    MartletOrbitFeather();
                    break;
                case FeatheredFoeState.CircleBurstFeather: //Forked burst
                    CircleBurstFeather();
                    break;
                case FeatheredFoeState.SwirlFeather: //Nothing
                    SwirlFeather();
                    break;
                case FeatheredFoeState.CornerTravelShot: //hardish to hit
                    CornerTravelShot();
                    break;
                case FeatheredFoeState.Dive: //hardish to hit
                    Dive();
                    break;
                case FeatheredFoeState.UmbrellaRain: //very easy to hit
                    UmbrellaRain();
                    break;
                case FeatheredFoeState.OffscreenDash: //impossible to hit
                    OffscreenDash();
                    break;
                case FeatheredFoeState.Madison: //very easy to hit
                    MadisonTornado();
                    break;
                case FeatheredFoeState.Maelstrom:
                    Maelstrom();
                    break;
                case FeatheredFoeState.WindOrb:
                    WindOrb();
                    break;
                case FeatheredFoeState.WindDirShot: 
                    WindDirShot();
                    break;
            }

            windOverlayOpacity = MathHelper.Lerp(windOverlayOpacity, windOverlayOpacityGoal, 0.05f);
            bgPulsePower = Math.Clamp(MathHelper.Lerp(bgPulsePower, -0.25f, 0.04f), 0f, 100f);
            randomShakePower = Math.Clamp(MathHelper.Lerp(randomShakePower, -0.35f, 0.05f), 0f, 100f);

            if (doPassiveWindParticles)
                PassiveWindParticles();

            if (drawDrill)
                drillAlpha = Math.Clamp(MathHelper.Lerp(drillAlpha, 1.35f, 0.04f), 0f, 1f);
            else
                drillAlpha = Math.Clamp(MathHelper.Lerp(drillAlpha, -1f, 0.09f), 0f, 1f);

            //Basic After-Image
            int trailCount = 10;
            previousPositions.Add(NPC.Center);
            previousRotations.Add(NPC.rotation);

            if (previousPositions.Count > trailCount)
            {
                previousPositions.RemoveAt(0);
                previousRotations.RemoveAt(0);
            }

            timer++;
        }


        public float bgPulsePower = 0f;

        //How many particles are spawned per frame (defaults to 1)
        int passiveWindParticlesCount = 1;

        float passiveWindParticleDirection = 0f;
        bool doPassiveWindParticles = false;

        bool centerPassiveWindParticlesOnPlayer = false;
        public void PassiveWindParticles()
        {
            float rot = passiveWindParticleDirection;// (player.Center - NPC.Center).ToRotation();

            int Ydir = rot.ToRotationVector2().X > 0 ? 1 : -1;

            for (int i = 0; i < passiveWindParticlesCount; i++)
            {
                Vector2 anchor = centerPassiveWindParticlesOnPlayer ? player.Center : NPC.Center;

                Vector2 windDustSpawnPosition = anchor + (new Vector2(1f * -550f + Main.rand.NextFloat(-400f, 0f), Main.rand.NextFloatDirection() * 900f) * 1f).RotatedBy(rot);
                Vector2 windDustVelocity = new Vector2(1f, Ydir * 0.15f).RotatedBy(rot) * Main.rand.NextFloat(0.1f, 1.8f) * 45f;

                float dustScale = Main.rand.NextFloat(1f, 2f);

                Dust wind = Dust.NewDustPerfect(windDustSpawnPosition, 176, windDustVelocity * 1f, newColor: Color.LightSkyBlue with { A = 0 } * 1f, Scale: dustScale); //dust176
                wind.noGravity = true;
            }

        }
    }
}
