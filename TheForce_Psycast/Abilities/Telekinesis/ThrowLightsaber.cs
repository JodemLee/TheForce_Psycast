using RimWorld;
using UnityEngine;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class ThrowLightsaber : Ability_ShootProjectile 
   {
        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 1) * 4);

    }
}
    
