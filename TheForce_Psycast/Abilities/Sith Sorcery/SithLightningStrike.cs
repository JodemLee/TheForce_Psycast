using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Ability = VFECore.Abilities.Ability;
using UnityEngine;

namespace TheForce_Psycast
{
    internal class SithLightningStrike : Ability
    {
        public float DarksideConnection => pawn.GetStatValue(ForceDefOf.Force_Darkside_Attunement);

        public override bool IsEnabledForPawn(out string reason)
        {
            if (!base.IsEnabledForPawn(out reason)) return false;
            if (DarksideConnection >= 2.5f) return true;
            reason = "Force.NotEnoughAttunement".Translate();
            return false;
        }


        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            this.pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside).Severity -= 1f;
            foreach (GlobalTargetInfo target in targets)
            {
                foreach (Thing thing in target.Cell.GetThingList(target.Map).ListFullCopy())
                    thing.TakeDamage(new DamageInfo(DamageDefOf.Flame, 25, -1f, this.pawn.DrawPos.AngleToFlat(thing.DrawPos), this.pawn));
                GenExplosion.DoExplosion(target.Cell, target.Map, this.GetRadiusForPawn(), DamageDefOf.EMP, this.pawn);
                this.pawn.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(this.pawn.Map, target.Cell));
            }
        }

        public override string GetDescriptionForPawn() => base.GetDescriptionForPawn() + "\n" + "Force.MustHaveDarkAttuneAmount".Translate(250).Colorize(Color.red);
    }
}