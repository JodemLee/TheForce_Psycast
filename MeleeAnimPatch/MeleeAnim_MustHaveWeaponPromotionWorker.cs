using AM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeleeAnimPatch
{
    internal class MeleeAnim_MustHaveWeaponPromotionWorker : AnimDef.IPromotionWorker
    {
        public float GetPromotionRelativeChanceFor(in AnimDef.PromotionInput input)
        {
            bool hasWeaponEquipped = input.Victim?.equipment?.Primary?.def?.IsWeapon ?? false;
            return hasWeaponEquipped ? 1 : 0;
        }
    }
}
