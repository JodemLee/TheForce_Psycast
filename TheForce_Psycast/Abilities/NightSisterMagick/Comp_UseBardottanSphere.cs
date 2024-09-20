using RimWorld;
using Verse;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Comp_UseBardottanSphere : CompTargetEffect
    {
        public override void DoEffectOn(Pawn user, Thing target)
        {
            Pawn pawn = (Pawn)target;
            if (!pawn.Dead)
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).TryRandomElement(out var result);
                BattleLogEntry_ItemUsed battleLogEntry_ItemUsed = new BattleLogEntry_ItemUsed(user, target, parent.def, RulePackDefOf.Event_ItemUsed);
                hediff.combatLogText = battleLogEntry_ItemUsed.ToGameStringFromPOV(null);
                pawn.health.AddHediff(hediff, result);
                Find.BattleLog.Add(battleLogEntry_ItemUsed);
            }
        }
    }


    public class CompApparelStoredValue : ThingComp
    {
        public int storedValue = 0;

        public override void Notify_Equipped(Pawn pawn)
        {
            // Check if the apparel has the required mod
            if (!ModLister.CheckRoyalty("Persona weapon"))
            {
                return;
            }
            base.Notify_Equipped(pawn);
    
            if (pawn != null)
            {
                foreach (StatModifier modifier in parent.def.equippedStatOffsets)
                {
                    StatDef stat = modifier.stat;
                    StatWorker.StatOffsetFromGear(parent, stat);
                }
            }
        }
    }
}
    

