using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Grammar;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities
{
    public class Ability_WriteCombatLog : VFECore.Abilities.Ability
    {
        public override void PreCast(GlobalTargetInfo[] target, ref bool startAbilityJobImmediately, Action startJobAction)
        {
            base.PreCast(target,ref startAbilityJobImmediately, startJobAction);

            Find.BattleLog.Add(new BattleLogEntry_VFEAbilityUsed(this.pawn, target[0].Thing, this.def, RulePackDefOf.Event_AbilityUsed));
        }
    }

    public class BattleLogEntry_VFEAbilityUsed : BattleLogEntry_Event
    {
        public VFECore.Abilities.AbilityDef abilityUsed;

        public BattleLogEntry_VFEAbilityUsed()
        {
        }

        public BattleLogEntry_VFEAbilityUsed(Pawn caster, Thing target, VFECore.Abilities.AbilityDef ability, RulePackDef eventDef)
            : base(target, eventDef, caster)
        {
            abilityUsed = ability;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref abilityUsed, "abilityUsed");
        }

        protected override GrammarRequest GenerateGrammarRequest()
        {
            GrammarRequest result = base.GenerateGrammarRequest();
            result.Rules.AddRange(GrammarUtility.RulesForDef("ABILITY", abilityUsed));
            if (subjectPawn == null && subjectThing == null)
            {
                result.Rules.Add(new Rule_String("SUBJECT_definite", "AreaLower".Translate()));
            }
            return result;
        }
    }
}
