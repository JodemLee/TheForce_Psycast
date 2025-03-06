using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    internal class SithRitual_Resurrection : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets != null)
            {
                GlobalTargetInfo target = targets[0];
                if (target.Thing is Corpse corpse)
                {
                    Pawn pawn = corpse.InnerPawn;
                    if (pawn != null && pawn.Dead)
                    {
                        ResurrectionUtility.TryResurrect(pawn); // Resurrect the corpse
                        ApplyHediffAndEnslave(pawn); // Apply the hediff and enslave
                    }
                }
            }
        }

        private void ApplyHediffAndEnslave(Pawn pawn)
        {
            Hediff hediff = HediffMaker.MakeHediff(ForceDefOf.Force_Sithzombie, pawn);
            pawn.health.AddHediff(hediff);
            pawn.SetFaction(this.pawn.Faction);
            if (pawn.IsColonist || pawn.IsPrisoner)
            {
                pawn.guest.SetGuestStatus(this.pawn.Faction, GuestStatus.Slave);
            }
        }
    }
}

  