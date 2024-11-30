using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Hediffs
{
    public class HediffWithComps_DarksideGhost : HediffWithComps
    {
        public Thing linkedObject;
        private int tickcounter = 0;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref linkedObject, "linkedObject");
        }

        public override void Tick()
        {
            base.Tick();
            tickcounter++;
            if (linkedObject != null && linkedObject.Destroyed && tickcounter > 10 && def != null)
            {
                pawn.health.RemoveHediff(pawn.health.GetOrAddHediff(ForceDefOf.Force_SithGhost));
                pawn.Kill(null);
                return;
            }
            if (tickcounter > 10)
            {
                tickcounter = 0;
            }
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

            // Only show "Link to Object" when no object is linked and the pawn is not dead
            if (Severity >= 1f && linkedObject == null && !pawn.Dead)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Link to Object",
                    defaultDesc = "Link the pawn's spirit to an object. If the object is destroyed, the pawn will die.",
                    icon = ContentFinder<Texture2D>.Get("Abilities/Darkside/SithGhost", true),
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

            // Show "Linked to Object" and "Unlink Object" when an object is linked
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

                // Show "Unlink Object" if the pawn is not already a Force Ghost
                if (!ForceGhostUtility.IsForceGhost(pawn))
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Unlink Object",
                        defaultDesc = "Safely unlink the pawn's spirit from the object.",
                        icon = Widgets.GetIconFor(linkedObject.def, linkedObject.Stuff),
                        action = () =>
                        {
                            linkedObject = null;
                            Messages.Message("The object has been safely unlinked from the pawn.", MessageTypeDefOf.PositiveEvent);
                        }
                    };
                }

                // New command: Clear linked object and kill the pawn
                if(ForceGhostUtility.IsForceGhost(pawn))
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Clear Linked Object and Kill",
                        defaultDesc = "Clear the linked object and kill the pawn immediately.",
                        icon = ContentFinder<Texture2D>.Get("Abilities/Darkside/SithGhost", true), // You can use a different icon if needed
                        action = () =>
                        {
                            linkedObject = null; // Clear the linked object
                            var ghost = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_SithGhost); 
                            pawn.health.RemoveHediff(ghost); // Remove the ghost hediff
                            pawn.Kill(null); // Kill the pawn
                            Messages.Message($"{pawn.LabelCap} has been killed and the linked object has been cleared.", MessageTypeDefOf.NegativeEvent);
                        }
                    };
                } 
            }

            // Show "Return as Ghost" if the pawn is dead and linked to an object, but not yet a Force Ghost
            if (pawn.Dead && linkedObject != null && !ForceGhostUtility.IsForceGhost(pawn))
            {
                yield return new Command_Action
                {
                    defaultLabel = "Force_ReturnAsGhost".Translate(),
                    defaultDesc = "Force_ReturnAsGhost".Translate(), // Ensure descriptions are properly localized
                    icon = ContentFinder<Texture2D>.Get("Abilities/Darkside/SithGhost", true),
                    action = () =>
                    {
                        if (pawn.Dead)
                        {
                            // Attempt to resurrect the pawn
                            GhostResurrectionUtility.TryReturnAsGhost(pawn, ForceDefOf.Force_SithGhost, 1f);
                            pawn.apparel.LockAll();
                        }
                    }
                };
            }

            // Yield any other gizmos from the base class
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
