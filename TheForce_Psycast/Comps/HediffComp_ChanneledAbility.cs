using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheForce_Psycast.Abilities.Mechu_deru;
using Verse;

namespace TheForce_Psycast.Comps
{
    public class HediffComp_ChanneledAbility : HediffComp
    {
        public virtual float EntropyGainPerTick => 1f;
        public virtual float SeverityIncreasePerTick => 0.0001f;

        public HediffCompProperties_ChanneledAbility Props => (HediffCompProperties_ChanneledAbility)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            AddEntropy();
            IncreaseSeverity();
        }

        private void AddEntropy()
        {
            var statMultiplier = Pawn.GetStatValue(Props.entropyGainMultiplier);
            if (Find.TickManager.TicksGame % 10 == 0)
                Pawn.psychicEntropy.TryAddEntropy(Props.entropyGainPerTick * statMultiplier, overLimit: true);
            if (Pawn.psychicEntropy.EntropyValue >= Pawn.psychicEntropy.MaxEntropy)
                Pawn.health.RemoveHediff(parent);
        }

        private void IncreaseSeverity()
        {
            parent.Severity += Props.severityIncreasePerTick;
        }

        public override string CompTipStringExtra
        {
            get
            {
                string tip = base.CompTipStringExtra;
                if (Props.entropyGainPerTick > 0)
                {
                    var statMulti = Props.entropyGainPerTick * Pawn.GetStatValue(Props.entropyGainMultiplier);
                    if (!tip.NullOrEmpty())
                        tip += "\n"; // Add a newline if there's already text in the tooltip
                    tip += "EntropyGainPerTick".Translate() + ": " + statMulti.ToString("F2");
                }
                return tip;
            }
        }
    }

    public class HediffCompProperties_ChanneledAbility : HediffCompProperties
    {
        public float entropyGainPerTick = 1f;
        public StatDef entropyGainMultiplier;
        public float severityIncreasePerTick = 0f;

        public HediffCompProperties_ChanneledAbility()
        {
            compClass = typeof(HediffComp_ChanneledAbility);
        }
    }
}