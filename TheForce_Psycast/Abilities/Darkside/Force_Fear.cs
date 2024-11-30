using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Darkside
{
    public class Ability_ForceFear : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Thing is not Pawn victim || victim.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)
                return false;

            return base.ValidateTarget(target, showMessages);
        }

        private float CalculateSuccessChance(Pawn target)
        {
            float casterSensitivity = this.pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            float targetSensitivity = target.GetStatValue(StatDefOf.PsychicSensitivity);

            if (targetSensitivity > casterSensitivity)
            {
                return 0f; // Always fail if target's sensitivity is higher
            }

            return Mathf.Clamp01((casterSensitivity - targetSensitivity + 1) / 2); // Ensures a value between 0 and 1
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (var target in targets)
            {
                var pawn = target.Thing as Pawn;
                if (pawn != null)
                {
                    float successChance = CalculateSuccessChance(pawn);

                    if (successChance == 0f)
                    {
                        // Ensure the ability fails if the target has higher psychic sensitivity
                        if (pawn.Spawned && pawn.Map != null)
                        {
                            Messages.Message("Force_HigherConnection".Translate(target.Pawn.Name), pawn, MessageTypeDefOf.NegativeEvent, true);
                        }
                        continue;
                    }

                    if (pawn.Faction == Faction.OfPlayer && pawn.Map?.IsPlayerHome != true)
                    {
                        // Apply hediff if target is of the player's faction and on a temporary map
                        var hediff = ForceDefOf.Force_Fear;
                        if (hediff != null)
                        {
                            HealthUtility.AdjustSeverity(pawn, hediff, 1.0f);
                            Messages.Message("Force_Fear_TemporaryMap".Translate(), pawn, MessageTypeDefOf.NeutralEvent, true);
                        }
                    }
                    else if (Rand.Chance(successChance))
                    {
                        // Apply a fear-related mental break
                        if (pawn.mindState != null && pawn.mindState.mentalStateHandler != null)
                        {
                            // Use a specific mental state for fear, e.g., PanicFlee
                            var fearMentalStateDef = ForceDefOf.Force_ForceFear;
                            pawn.mindState.mentalStateHandler.TryStartMentalState(fearMentalStateDef, "Force_Fear".Translate(), true);
                        }
                    }
                    else
                    {
                        // Target overcomes fear
                        if (pawn.Spawned && pawn.Map != null)
                        {
                            Messages.Message("Force_FearFailed".Translate(target.Pawn.Name), pawn, MessageTypeDefOf.NeutralEvent, true);
                        }
                    }
                }
            }
        }
    }
}
