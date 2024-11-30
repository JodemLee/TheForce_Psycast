using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Darkside
{
    public class Ability_ForceDestruction : Ability
    {
        public override float GetRadiusForPawn() =>
            Mathf.Min(this.pawn.psychicEntropy.EntropyValue / 10f * base.GetRadiusForPawn(),
                GenRadial.MaxRadialPatternRadius);

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var explosionRadius = this.GetRadiusForPawn();
            float sensitivityFactor = this.pawn.GetStatValue(ForceDefOf.Force_Darkside_Attunement);
            int baseDamage = 25;
            int damageAmount = Mathf.RoundToInt(baseDamage * sensitivityFactor);
            damageAmount = Mathf.Max(1, damageAmount);
            this.pawn.psychicEntropy.RemoveAllEntropy();
            MakeStaticFleck(targets[0].Cell, targets[0].Thing.Map, ForceDefOf.Force_Burst_Bubble, explosionRadius/2, 1);
            GenExplosion.DoExplosion(
                center: targets[0].Cell,
                map: pawn.Map,
                radius: explosionRadius,
                damType: DamageDefOf.Blunt,
                explosionSound: SoundDefOf.Psycast_Skip_Entry,
                instigator: pawn,
                damAmount: damageAmount,
                ignoredThings: new List<Thing> { pawn }
            );;
        }
    }
}
