using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded.Technomancer;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities
{
    internal static class AutocastingTweaks
    {

    }

    public interface IPawnCheck
    {
        bool Check(Pawn pawn);
    }

    public class PawnHasHediff : IPawnCheck
    {
        private readonly string hediffDefName;

        public PawnHasHediff(string hediffDefName)
        {
            this.hediffDefName = hediffDefName;
        }

        public bool Check(Pawn pawn)
        {
            return pawn?.health?.hediffSet?.hediffs.Any(hediff => hediff.def.defName == hediffDefName) ?? false;
        }
    }

    public class PawnIsDown : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn?.CurJobDef == JobDefOf.LayDown || pawn?.jobs?.curDriver?.asleep == true || pawn?.Downed == true;
        }
    }

    public class PawnIsColonyAnimal : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.Faction?.IsPlayer == true && pawn.RaceProps.Animal;
        }
    }

    // Check for Wild Animals
    public class PawnIsWildAnimal : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.Faction == null && pawn.RaceProps.Animal;
        }
    }

    // Check for Visitors
    public class PawnIsVisitor : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.Faction != null && !(pawn.Faction.IsPlayer || pawn.Faction.HostileTo(Faction.OfPlayer));
        }
    }

    // Check for Colonists
    public class PawnIsColonist : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.IsColonist;
        }
    }

    // Check for Immunizable Pawns
    public class PawnIsImmunizable : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.health.hediffSet.HasImmunizableNotImmuneHediff();
        }
    }

    // Check for Slaves
    public class PawnIsSlave : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.IsSlaveOfColony;
        }
    }

    // Check for Prisoners
    public class PawnIsPrisoner : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.IsPrisoner;
        }
    }

    // Check if Pawn is Not Down
    public class PawnIsNotDown : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return !(pawn.CurJobDef == JobDefOf.LayDown || pawn.jobs?.curDriver?.asleep == true || pawn.Downed);
        }
    }

    // Check if Pawn Has Mental Break
    public class PawnHasMentalBreak : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.MentalState != null;
        }
    }

    // Check for Highest Sensitivity (return type modified for compatibility)
    public class PawnHasHighestSensitivity : IPawnCheck
    {
        private readonly List<Pawn> allPawns;

        public PawnHasHighestSensitivity(List<Pawn> allPawns)
        {
            this.allPawns = allPawns;
        }

        public bool Check(Pawn pawn)
        {
            return pawn == allPawns.OrderByDescending(p => p.psychicEntropy.PsychicSensitivity).FirstOrDefault();
        }
    }

    public class PawnHasLowestSensitivity : IPawnCheck
    {
        private readonly List<Pawn> allPawns;

        public PawnHasLowestSensitivity(List<Pawn> allPawns)
        {
            this.allPawns = allPawns;
        }

        public bool Check(Pawn pawn)
        {
            return pawn == allPawns.OrderBy(p => p.psychicEntropy.PsychicSensitivity).FirstOrDefault();
        }
    }

    // Check for Psychically Sensitive Pawns
    public class PawnIsPsychicallySensitive : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn.psychicEntropy?.IsPsychicallySensitive == true;
        }
    }

    // Check if Pawn Can Psycast
    public class PawnCanPsycast : IPawnCheck
    {
        public bool Check(Pawn pawn)
        {
            return pawn?.Downed == false
                && pawn.HasPsylink
                && pawn.Awake();
        }
    }

    public class PawnCheckExtension : DefModExtension
    {
        public List<IPawnCheck> PawnChecks;
    }

    internal static class PsycastingHandler
    {
        internal static bool CastAbilityOnTarget(Ability ability, GlobalTargetInfo target)
        {
            if (ability is null)
                throw new ArgumentNullException(nameof(ability));
            if (ability.def.targetCount > 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ability),
                    "Can't autocast with multiple targets"
                );
            }

            // Check for the PawnCheckExtension mod extension
            var pawnCheckExtension = ability.def.GetModExtension<PawnCheckExtension>();
            if (pawnCheckExtension != null)
            {
                // Validate each IPawnCheck in the PawnChecks list
                foreach (IPawnCheck check in pawnCheckExtension.PawnChecks)
                {
                    if (!check.Check(ability.pawn)) // If any check fails, return false
                    {
                        return false;
                    }
                }
            }

            AbilityTargetingMode targetMode = ability.def.targetModes[0];
            const bool showMessages = true;
            bool validated;

            if (targetMode == AbilityTargetingMode.Self)
            {
                validated = true;
            }
            else
            {
                validated = ability.ValidateTarget((LocalTargetInfo)target, showMessages);
            }

            if (!validated)
            {
                return false;
            }

            ability.CreateCastJob(target);
            return true;
        }
    }
}