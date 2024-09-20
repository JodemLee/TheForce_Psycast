using System.Collections.Generic;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    internal class LightsaberGlowManager : GameComponent
    {
        public List<ThingComp> compsToTickNormal = new List<ThingComp>();

        public List<Comp_LightsaberBlade> compGlowerToTick = new List<Comp_LightsaberBlade>();

        public static LightsaberGlowManager Instance;

        public LightsaberGlowManager(Game game)
        {
            Init();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Init();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Init();
        }

        private void Init()
        {
            Instance = this;
            if (compsToTickNormal == null)
            {
                compsToTickNormal = new List<ThingComp>();
            }
        }

        public override void GameComponentTick()
        {
            for (int num = compGlowerToTick.Count - 1; num >= 0; num--)
            {
                compGlowerToTick[num].Tick();
            }
            for (int num2 = compsToTickNormal.Count - 1; num2 >= 0; num2--)
            {
                compsToTickNormal[num2].CompTick();
            }
        }
    }
}