using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Verse.AI;
using Verse.Sound;
using Verse.Noise;

namespace TheForce_Psycast
{
    public class Ability_ForceThrow : VFECore.Abilities.Ability
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
                        GlobalTargetInfo dest = this.Comp.currentlyCastingTargets[i + 1];
                    }
            });
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            for (int i = 0; i < targets.Length; i += 2)
            {
                if (targets[i].Thing is Pawn)
                {
                    GlobalTargetInfo dest = targets[i + 1];
                    AbilityPawnFlyer flyer = (AbilityPawnFlyer)PawnFlyer.MakeFlyer(VFE_DefOf_Abilities.VFEA_AbilityFlyer, targets[i].Thing as Pawn, dest.Cell, null, null);
                    flyer.ability = this;
                    flyer.target = dest.Cell.ToVector3();
                    GenSpawn.Spawn(flyer, dest.Cell, dest.Map);
                }
                else
                {
                    targets[i].Thing.Position = targets[i + 1].Cell;
                }
            }

            base.Cast(targets);
        }
    }
}
