using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities
{
    internal class Command_PsycastAbility_WithFloat : Command_Ability
    {
        public Func<IEnumerable<FloatMenuOption>> floatMenuGetter;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions => this.floatMenuGetter?.Invoke();

        private readonly AbilityExtension_Psycast psycastExtension;

        public Command_PsycastAbility_WithFloat(Pawn pawn, Ability ability) : base(pawn, ability)
        {
            this.psycastExtension = this.ability.def.GetModExtension<AbilityExtension_Psycast>();
            this.shrinkable = PsycastsMod.Settings.shrink;
        }

        public override string TopRightLabel
        {
            get
            {
                if (this.ability.AutoCast) return null;
                string topRightLabel = string.Empty;
                float entropy = this.psycastExtension.GetEntropyUsedByPawn(this.ability.pawn);
                if (entropy > float.Epsilon)
                    topRightLabel += "NeuralHeatLetter".Translate() + ": " + entropy.ToString(CultureInfo.CurrentCulture) + "\n";

                float psyfocusCost = this.psycastExtension.GetPsyfocusUsedByPawn(this.ability.pawn);
                if (psyfocusCost > float.Epsilon)
                    topRightLabel += "PsyfocusLetter".Translate() + ": " + psyfocusCost.ToStringPercent();
                return topRightLabel.TrimEndNewlines();
            }
        }
    }
}
