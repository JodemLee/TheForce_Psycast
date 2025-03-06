using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;

namespace TheForce_Psycast
{
    public class PsycasterPathDef_ForceVer : PsycasterPathDef
    {
        public HediffDef requiredHediff;
        public SkillDef requiredSkill; 
        public int requiredSkillLevel; 
        public TraitDef requiredTrait;
        public ResearchProjectDef requiredResearch;

        public override bool CanPawnUnlock(Pawn pawn) =>
        base.CanPawnUnlock(pawn) &&
        PawnHasCorrectHediff(pawn) &&
        PawnHasRequiredSkill(pawn) &&
        PawnHasRequiredTrait(pawn) &&
        ResearchIsCompleted(pawn);

        private bool PawnHasCorrectHediff(Pawn pawn) => requiredHediff == null || (pawn.health?.hediffSet.HasHediff(requiredHediff) ?? false);
        private bool PawnHasRequiredSkill(Pawn pawn)
        {
            if (requiredSkill == null) return true;
            var skill = pawn.skills?.GetSkill(requiredSkill);
            return skill != null && skill.Level >= requiredSkillLevel;
        }
        private bool PawnHasRequiredTrait(Pawn pawn) =>
        requiredTrait == null || (pawn.story?.traits?.HasTrait(requiredTrait) ?? false);
        private bool ResearchIsCompleted(Pawn pawn)
        {
            if (requiredResearch == null) return true;
            return requiredResearch.IsFinished;
        }

    }
}
