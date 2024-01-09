using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;
using Ability = VanillaPsycastsExpanded;

namespace TheForce_Psycast
{
     public class LightsaberPatch  : ThingWithComps
    {
            private AbilityDef ability;
            private bool alreadyHad;
            private int lastCooldown;

            public AbilityDef Ability => ability;
            
            
            public bool Added => !alreadyHad;
            
            public PsycasterPathDef Path => ability.Psycast().path;

            public override void ExposeData()
            {
                base.ExposeData();
                Scribe_Defs.Look(ref ability, nameof(ability));
                Scribe_Values.Look(ref alreadyHad, nameof(alreadyHad));
            }

        
        public override void Notify_Equipped(Pawn pawn)
        {
            var ability = ForceDefOf.Force_ThrowWeapon;
            base.Notify_Equipped(pawn);
            if (pawn.Psycasts() is null or { level: <= 0 })
            {
                return;
            }
            if (ability == null)
            {
                Log.Warning("[VPE] Psyring present with no ability, destroying.");
                Destroy();
                return;
            }

            { 
                var comp = pawn.GetComp<CompAbilities>();
                if (comp == null) return;
                alreadyHad = comp.HasAbility(ability);
                if (!alreadyHad) comp.GiveAbility(ability);
            }
        }

            public override void Notify_Unequipped(Pawn pawn)
            {
                 var ability = ForceDefOf.Force_ThrowWeapon;
                base.Notify_Unequipped(pawn);
                if (ability == null) return;
                if (!alreadyHad) pawn.GetComp<CompAbilities>().LearnedAbilities.RemoveAll(ab => ab.def == ability);
                alreadyHad = false;
            }
        }
 }
   

