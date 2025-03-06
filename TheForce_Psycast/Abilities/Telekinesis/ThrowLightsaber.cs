using RimWorld;
using RimWorld.Planet;
using System;
using TheForce_Psycast.Abilities;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class ThrowLightsaber : Ability_ShootProjectile 
   {
        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 1) * 4);

        public override void PreCast(GlobalTargetInfo[] target, ref bool startAbilityJobImmediately, Action startJobAction)
        {
            base.PreCast(target, ref startAbilityJobImmediately, startJobAction);

            Find.BattleLog.Add(new BattleLogEntry_VFEAbilityUsed(this.pawn, target[0].Thing, this.def, RulePackDefOf.Event_AbilityUsed));
        }
    }
}
    
