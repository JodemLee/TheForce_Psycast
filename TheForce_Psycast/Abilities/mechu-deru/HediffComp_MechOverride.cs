using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    public class HediffComp_MechOverride : HediffComp
    {
        private Faction originalFaction;
        public HediffCompProperties_MechOverride Props => (HediffCompProperties_MechOverride)props;

        public override void CompPostMake()
        {
            base.CompPostMake();
            Pawn pawn = this.Pawn;
            if (pawn != null && pawn.RaceProps.IsMechanoid)
            {
                originalFaction = pawn.Faction;
                pawn.SetFaction(Faction.OfPlayer);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            RevertFaction();
        }

        private void RevertFaction()
        {
            if (Pawn != null && !Pawn.Dead && Pawn.Faction == Faction.OfPlayer)
            {
                Pawn.SetFaction(originalFaction);
            }
        }
    }

    public class HediffCompProperties_MechOverride : HediffCompProperties
    {
        public HediffCompProperties_MechOverride()
        {
            this.compClass = typeof(HediffComp_MechOverride);
        }
    }
}
