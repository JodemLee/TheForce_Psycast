using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded.Technomancer;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using UnityEngine;

namespace TheForce_Psycast
{
    internal class Craft_AncientSithScrollResurrection :  Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var thing = targets[0].Thing;
            if (thing is null) return;
            thing.Destroy();
            IntVec3 cell = thing.Position;
            Thing scroll = ThingMaker.MakeThing(ForceDefOf.Force_AncientResurrectorScroll);
            GenSpawn.Spawn(scroll, cell, this.pawn.Map);
        }
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            if (target.Thing.def != ForceDefOf.Force_AncientScroll)
            {
                if (showMessages) Messages.Message("Force.MustbeAncientScrollSith".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }
    }

    internal class BoltofHatred : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var thing = targets[0].Thing;
            if (thing is null) return;
            thing.Destroy();
            IntVec3 cell = thing.Position;
            Thing scroll = ThingMaker.MakeThing(ForceDefOf.Force_BoltofHatred);
            GenSpawn.Spawn(scroll, cell, this.pawn.Map);
        }
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            if (target.Thing.def != ForceDefOf.Force_AncientScroll)
            {
                if (showMessages) Messages.Message("Force.MustbeAncientScrollSith".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }
    }

    internal class SuppressedThoughts : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var thing = targets[0].Thing;
            if (thing is null) return;
            thing.Destroy();
            IntVec3 cell = thing.Position;
            Thing scroll = ThingMaker.MakeThing(ForceDefOf.Force_ScrollSuppressThought);
            GenSpawn.Spawn(scroll, cell, this.pawn.Map);
        }
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            if (target.Thing.def != ForceDefOf.Force_AncientScroll)
            {
                if (showMessages) Messages.Message("Force.MustbeAncientScrollSith".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }
    }

    internal class DarksideWeb : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var thing = targets[0].Thing;
            if (thing is null) return;
            thing.Destroy();
            IntVec3 cell = thing.Position;
            Thing scroll = ThingMaker.MakeThing(ForceDefOf.Force_ScrollDarksideWeb);
            GenSpawn.Spawn(scroll, cell, this.pawn.Map);
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            if (target.Thing.def != ForceDefOf.Force_AncientScroll)
            {
                if (showMessages) Messages.Message("Force.MustbeAncientScrollSith".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }
    }

}

