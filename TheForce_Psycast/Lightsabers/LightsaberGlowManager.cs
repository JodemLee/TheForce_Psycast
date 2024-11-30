using System.Collections.Generic;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    internal class LightsaberGlowManager : GameComponent
    {
        public List<ThingComp> compsToTickNormal { get; private set; } = new List<ThingComp>();
        public List<Comp_LightsaberBlade> compGlowerToTick { get; private set; } = new List<Comp_LightsaberBlade>();
        public static LightsaberGlowManager Instance { get; private set; }

        public LightsaberGlowManager(Game game)
        {
            Instance = this;
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
            if (compsToTickNormal == null)
            {
                compsToTickNormal = new List<ThingComp>();
            }
        }

        public override void GameComponentTick()
        {
            if (!Force_ModSettings.LightsaberRealGlow)
            {
                return;
            }
            for (int i = compGlowerToTick.Count - 1; i >= 0; i--)
            {
                if (compGlowerToTick[i].ShouldGlow())
                {
                    compGlowerToTick[i].CompTick();
                }
            }

            // Update normal ticking components
            for (int i = compsToTickNormal.Count - 1; i >= 0; i--)
            {
                compsToTickNormal[i].CompTick();
            }
        }
    }
}


