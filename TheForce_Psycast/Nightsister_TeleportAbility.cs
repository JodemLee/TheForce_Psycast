using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded.Skipmaster;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Nightsister_TeleportAbility : Ability_Teleport
    {
        public new virtual FleckDef[] EffectSet => new[]
        {
            FleckDefOf.PsycastSkipFlashEntry,
            ForceDefOf.MagickSkipInnerExit,
            ForceDefOf.MagickSkipOuterRingExit
        };

        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            toil.AddPreTickAction(delegate
            {
                if (this.pawn.jobs.curDriver.ticksLeftThisToil != 5) return;
                FleckDef[] set = this.EffectSet;
                for (int i = 0; i < this.Comp.currentlyCastingTargets.Length; i += 2)
                    if (this.Comp.currentlyCastingTargets[i].Thing is { } t)
                    {
                        if (t is Pawn p)
                        {
                            FleckCreationData dataAttachedOverlay = FleckMaker.GetDataAttachedOverlay(p, set[0], Vector3.zero);
                            dataAttachedOverlay.link.detachAfterTicks = 5;
                            p.Map.flecks.CreateFleck(dataAttachedOverlay);
                        }
                        else
                            FleckMaker.Static(t.TrueCenter(), t.Map, FleckDefOf.PsycastSkipFlashEntry);

                        GlobalTargetInfo dest = this.Comp.currentlyCastingTargets[i + 1];
                        FleckMaker.Static(dest.Cell, dest.Map, set[1]);
                        FleckMaker.Static(dest.Cell, dest.Map, set[2]);
                        SoundDefOf.Psycast_Skip_Entry.PlayOneShot(t);
                        SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(dest.Cell, dest.Map));
                        this.AddEffecterToMaintain(ForceDefOf.Magick_Entry.Spawn(t, t.Map), t.Position, 60);
                        this.AddEffecterToMaintain(ForceDefOf.Magick_Exit.Spawn(dest.Cell, dest.Map), dest.Cell, 60);
                    }
            });
        }
    }
}
