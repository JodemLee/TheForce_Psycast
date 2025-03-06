using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded.Skipmaster;
using Verse;
using Verse.AI;
using Verse.Sound;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Nightsister_TeleportAbility : Ability
    {

        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            toil.AddPreTickAction(delegate
            {
                if (this.pawn.jobs.curDriver.ticksLeftThisToil != 5) return;
                for (int i = 0; i < this.Comp.currentlyCastingTargets.Length; i += 2)
                    if (this.Comp.currentlyCastingTargets[i].Thing is { } t)
                    {
                        if (t is Pawn p)
                        {

                        }
                        else
                        {
                            GlobalTargetInfo dest = this.Comp.currentlyCastingTargets[i + 1];
                        }
                    }
            });
        }
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            AbilityExtension_Clamor clamor = this.def.GetModExtension<AbilityExtension_Clamor>();
            for (int i = 0; i < targets.Length; i += 2)
                if (targets[i].Thing is { } t)
                {
                    t.TryGetComp<CompCanBeDormant>()?.WakeUp();
                    GlobalTargetInfo dest = targets[i + 1];
                    if (t.Map != dest.Map)
                    {
                        if (t is not Pawn p) continue;
                        p.teleporting = true;
                        p.ExitMap(true, Rot4.Invalid);
                        p.teleporting = false;
                        GenSpawn.Spawn(p, dest.Cell, dest.Map);
                    }

                    t.Position = dest.Cell;
                    AbilityUtility.DoClamor(t.Position, clamor.clamorRadius, this.pawn, clamor.clamorType);
                    AbilityUtility.DoClamor(dest.Cell, clamor.clamorRadius, this.pawn, clamor.clamorType);
                    (t as Pawn)?.Notify_Teleported();
                }

            base.Cast(targets);
        }
    }
}
