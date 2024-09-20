using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Darkside
{
    public class Ability_ForceInsanity : Ability
    {
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Thing is not Pawn victim || victim.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)
            {
                return false;
            }

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

            float successChance = Mathf.Clamp01((casterSensitivity - targetSensitivity + 1) / 2);
            return successChance;
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
                        if (pawn.Spawned && pawn.Map != null)
                        {
                            Messages.Message("Force_HigherConnection".Translate(target.Pawn.Name), pawn, MessageTypeDefOf.NegativeEvent, true);
                        }
                        continue;
                    }

                    if (Rand.Chance(successChance))
                    {
                        var mentalBreakDef = DefDatabase<MentalStateDef>.AllDefs.RandomElement();
                        pawn.mindState.mentalStateHandler.TryStartMentalState(mentalBreakDef, "Force_Insanity".Translate(), true);
                    }
                    else
                    {
                        if (pawn.Spawned && pawn.Map != null)
                        {
                            Messages.Message("Force_InsanityFailed".Translate(target.Pawn.Name), pawn, MessageTypeDefOf.NeutralEvent, true);
                        }
                    }
                }
            }
        }
    }
}
