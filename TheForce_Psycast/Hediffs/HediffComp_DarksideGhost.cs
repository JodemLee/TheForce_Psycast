using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheForce_Psycast.Abilities.Lightside;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Hediffs
{
    public class HediffWithComps_DarksideGhost : HediffWithComps
    {
        public Thing linkedObject;
        private bool isDead;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            isDead = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref linkedObject, "linkedObject");
            Scribe_Values.Look(ref isDead, "isDead", false);
        }

        public static TargetingParameters ForLinkToObject()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                canTargetItems = true,
                canTargetLocations = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo target)
                {
                    if (target.HasThing)
                    {
                        var thing = target.Thing;

                        // Check if the thing is a single item and not a stack
                        bool isSingleItem = thing.stackCount == 1;

                        return isSingleItem &&
                               (thing.def.category == ThingCategory.Building ||
                                thing.def.IsApparel || // Check if the thing is apparel
                                thing.def.IsWeapon); // Check if the thing is a weapon
                    }
                    return false;
                }
            };
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (pawn == null)
            {
                yield break;
            }

            if (Severity >= 1f && linkedObject == null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Link to Object",
                    defaultDesc = "Link the pawn's spirit to an object. If the object is destroyed, the pawn will die.",
                    icon = ContentFinder<Texture2D>.Get("Abilities/Lightside/ForceGhost", true),
                    action = () =>
                    {
                        Find.Targeter.BeginTargeting(ForLinkToObject(), (target) =>
                        {
                            if (target.HasThing)
                            {
                                linkedObject = target.Thing;
                            }
                        });
                    }
                };
            }

            if (linkedObject != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Linked to: " + linkedObject.LabelCap,
                    defaultDesc = "The pawn's spirit is linked to this object. If it is destroyed, the pawn will die.",
                    icon = GetIconFor(linkedObject.def, linkedObject.Stuff),
                    action = () =>
                    {
                        CameraJumper.TryJumpAndSelect(linkedObject);
                    }
                };

                if (!ForceGhostUtility.IsForceGhost(pawn))
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Unlink Object",
                        defaultDesc = "Safely unlink the pawn's spirit from the object.",
                        icon = GetIconFor(linkedObject.def, linkedObject.Stuff),
                        action = () =>
                        {
                            linkedObject = null;
                            Messages.Message("The object has been safely unlinked from the pawn.", MessageTypeDefOf.PositiveEvent);
                        }
                    };
                }
            }

            if (isDead && linkedObject != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Return as Ghost",
                    defaultDesc = "Return as a Force Ghost.",
                    icon = ContentFinder<Texture2D>.Get("Abilities/Lightside/ForceGhost", true),
                    action = () =>
                    {
                        ResurrectionUtility.TryResurrect(pawn);
                        isDead = false;
                        Severity = 1;
                        pawn.health.AddHediff(ForceDefOf.Force_SithGhost);
                    }
                };
            }

            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            linkedObject = null;
        }

        public static Texture2D GetIconFor(ThingDef thingDef, ThingDef stuffDef = null, ThingStyleDef thingStyleDef = null, int? graphicIndexOverride = null)
        {
            if (thingDef.IsCorpse && thingDef.ingestible?.sourceDef != null)
            {
                thingDef = thingDef.ingestible.sourceDef;
            }
            Texture2D result = thingDef.GetUIIconForStuff(stuffDef);
            if (thingStyleDef != null && thingStyleDef.UIIcon != null)
            {
                result = (!graphicIndexOverride.HasValue) ? thingStyleDef.UIIcon : thingStyleDef.IconForIndex(graphicIndexOverride.Value);
            }
            else if (thingDef.graphic is Graphic_Appearances graphic_Appearances)
            {
                result = (Texture2D)graphic_Appearances.SubGraphicFor(stuffDef ?? GenStuff.DefaultStuffFor(thingDef)).MatAt(thingDef.defaultPlacingRot).mainTexture;
            }
            return result;
        }
    }
}
