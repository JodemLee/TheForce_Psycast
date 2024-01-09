using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class CompProperties_UseEffect_Psytrainer : CompProperties_UseEffectGiveAbility
    {
        public CompProperties_UseEffect_Psytrainer() => this.compClass = typeof(CompPsytrainer);
    }

    public class CompPsytrainer : CompUseEffect_GiveAbility
    {
        public override void DoEffect(Pawn usedBy)
        {
            if (this.Props.ability?.Psycast()?.path is { } path && usedBy.Psycasts() is { } psycasts
                && !psycasts.unlockedPaths.Contains(path))
                psycasts.UnlockPath(path);
            base.DoEffect(usedBy);
        }

        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            if (p.Psycasts() is null or { level: <= 0 })
            {
                failReason = "VPE.MustBePsycaster".Translate();
                return false;
            }
            var path = this.Props.ability?.Psycast()?.path;
            if (path != null)
            {
                if (path.CanPawnUnlock(p) is false)
                {
                    if (path.ignoreLockRestrictionsForNeurotrainers is false)
                    {
                        failReason = this.Props.ability.Psycast().path.lockedReason;
                        return false;
                    }
                }
            }
            if (p.GetComp<CompAbilities>().HasAbility(this.Props.ability))
            {
                failReason = "VPE.AlreadyHasPsycast".Translate(this.Props.ability.LabelCap);
                return false;
            }
            failReason = null;
            return true;
        }
    }
}