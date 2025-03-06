using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Hediffs
{
    public class HediffComp_Ghost : HediffComp
    {
        private const int TICK_THRESHOLD = 10;

        private Thing _linkedObject;
        private int _tickCounter = 0;
        private bool _showNestedGizmos = false;

        public HediffCompProperties_Ghost Props => (HediffCompProperties_Ghost)props;

        public Thing LinkedObject
        {
            get => _linkedObject;
            set => _linkedObject = value;
        }

        private int TickCounter
        {
            get => _tickCounter;
            set => _tickCounter = value;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref _linkedObject, "linkedObject");
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent == null || parent.pawn == null) return;
            base.CompPostTick(ref severityAdjustment);
            TickCounter++;

            if (TickCounter > TICK_THRESHOLD)
            {
                if (LinkedObject != null && LinkedObject.Destroyed)
                {
                    var ghostHediff = parent.pawn.health.GetOrAddHediff(ForceDefOf.Force_SithGhost);
                    parent.pawn.health.RemoveHediff(ghostHediff);
                    parent.pawn.Kill(null);
                    LinkedObject = null;
                    return;
                }
                TickCounter = 0;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (parent == null || parent.pawn == null)
                yield break;

            Pawn pawn = parent.pawn;
            HediffCompProperties_Ghost props = Props;
            if (props == null)
                yield break;

            // Light Side Ghost Logic
            if (props.isLightSide)
            {
                if (pawn.Dead && parent.Severity >= props.severityThreshold && !ForceGhostUtility.IsForceGhost(pawn))
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Force_ReturnAsGhost".Translate(),
                        defaultDesc = "Force_ReturnAsGhost".Translate(),
                        icon = ContentFinder<Texture2D>.Get("Abilities/Lightside/ForceGhost", true),
                        action = () =>
                        {
                            ForceGhostUtility.TryReturnAsGhost(pawn, ForceDefOf.Force_Ghost, 1f);
                            pawn.apparel?.LockAll();
                        }
                    };
                }

                if (ForceGhostUtility.IsForceGhost(pawn) && pawn.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Force_Sithzombie) == null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Force_ReturnToFlesh".Translate(),
                        defaultDesc = "Force_ReturnToFlesh".Translate(),
                        icon = ContentFinder<Texture2D>.Get("Abilities/Lightside/ForceGhost", true),
                        action = () =>
                        {
                            var ghost = pawn.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Force_Ghost);
                            if (ghost != null)
                            {
                                pawn.health.RemoveHediff(ghost);
                                pawn.Kill(null);
                            }
                        }
                    };
                }
            }
            else // Dark Side Logic
            {
                if (pawn.Dead && parent.Severity >= props.severityThreshold && !ForceGhostUtility.IsForceGhost(pawn) && LinkedObject != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Force_ReturnAsGhost".Translate(),
                        defaultDesc = "Force_ReturnAsGhost".Translate(),
                        icon = ContentFinder<Texture2D>.Get("Abilities/Darkside/SithGhost", true),
                        action = () =>
                        {
                            ForceGhostUtility.TryReturnAsGhost(pawn, ForceDefOf.Force_SithGhost, 1f);
                            pawn.apparel?.LockAll();
                        }
                    };
                }

                if (ForceGhostUtility.IsForceGhost(pawn) && pawn.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Force_Sithzombie) == null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Force_ReturnToFlesh".Translate(),
                        defaultDesc = "Force_ReturnToFlesh".Translate(),
                        icon = ContentFinder<Texture2D>.Get("Abilities/Darkside/SithGhost", true),
                        action = () =>
                        {
                            var ghost = pawn.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Force_SithGhost);
                            if (ghost != null)
                            {
                                pawn.health.RemoveHediff(ghost);
                                pawn.Kill(null);
                                Messages.Message($"{pawn.LabelCap} has been killed and the linked object has been cleared.", MessageTypeDefOf.NegativeEvent);
                            }
                        }
                    };
                }

                yield return new Command_Action
                {
                    defaultLabel = "Linked Object Actions".Translate(),
                    defaultDesc = "Expand to see actions related to the Sith's Spirit.".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Abilities/Darkside/SithGhost", true),
                    action = () => _showNestedGizmos = !_showNestedGizmos
                };

                if (_showNestedGizmos)
                {
                    if (parent.Severity >= props.severityThreshold && LinkedObject == null && !pawn.Dead)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "Link to Object".Translate(),
                            defaultDesc = "Link the pawn's spirit to an object. If the object is destroyed, the pawn will die.".Translate(),
                            icon = ContentFinder<Texture2D>.Get("Abilities/Darkside/SithGhost", true),
                            action = () => Find.Targeter?.BeginTargeting(ForLinkToObject(), target => LinkedObject = target.Thing)
                        };
                    }

                    foreach (var gizmo in GetLinkedObjectGizmos())
                    {
                        yield return gizmo;
                    }
                }
            }

            foreach (var gizmo in base.CompGetGizmos() ?? Enumerable.Empty<Gizmo>())
            {
                yield return gizmo;
            }
        }

        private IEnumerable<Gizmo> GetLinkedObjectGizmos()
        {
            if (LinkedObject == null)
                yield break;

            yield return new Command_Action
            {
                defaultLabel = "Force_SithGhost".Translate() + LinkedObject.LabelCap,
                defaultDesc = "Force_SithGhostDesc".Translate(),
                icon = GetIconFor(LinkedObject.def, LinkedObject.Stuff),
                action = () => CameraJumper.TryJumpAndSelect(LinkedObject)
            };

            if (!ForceGhostUtility.IsForceGhost(parent?.pawn))
            {
                yield return new Command_Action
                {
                    defaultLabel = "Force_Unlink".Translate(),
                    defaultDesc = "Force_UnlinkDesc".Translate(),
                    icon = GetIconFor(LinkedObject.def, LinkedObject.Stuff),
                    action = () =>
                    {
                        LinkedObject = null;
                        Messages.Message("Force_UnlinkMessage".Translate(), MessageTypeDefOf.PositiveEvent);
                    }
                };
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
                validator = target => target.HasThing && IsValidLinkTarget(target.Thing)
            };
        }

        private static bool IsValidLinkTarget(Thing thing)
        {
            return thing.stackCount == 1 &&
                   (thing.def.category == ThingCategory.Building ||
                    thing.def.IsApparel ||
                    thing.def.IsWeapon);
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

    public class HediffCompProperties_Ghost : HediffCompProperties
    {
        public bool isLightSide = true;
        public bool canLinkToObjects = false;
        public float severityThreshold = 1f;

        public HediffCompProperties_Ghost()
        {
            this.compClass = typeof(HediffComp_Ghost);
        }
    }
}