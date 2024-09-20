using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaPsycastsExpanded.Skipmaster;
using Verse;
using Verse.AI;
using Verse.Sound;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class Nightsister_TeleportAbility : Ability
    {

        public override void Init()
        {
            base.Init();
            var nightsisterGene = pawn.genes.GetGene(ForceDefOf.Force_NightSisterMagick);
            if (nightsisterGene is null)
            {
                nightsisterGene = GeneMaker.MakeGene(ForceDefOf.Force_NightSisterMagick, this.pawn);
                pawn.genes.AddGene(nightsisterGene.def, true);
            }
        }

        public virtual FleckDef[] EffectSet => new[]
        {
            ForceDefOf.MagickSkipFlashEntry,
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
                            FleckMaker.Static(t.TrueCenter(), t.Map, ForceDefOf.MagickSkipFlashEntry);

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
         public override void Cast(params GlobalTargetInfo[] targets)
        {
            AbilityExtension_Clamor clamor = this.def.GetModExtension<AbilityExtension_Clamor>();
            for (int i = 0; i < targets.Length; i += 2)
                if (targets[i].Thing is { } t)
                {
                    t.TryGetComp<CompCanBeDormant>()?.WakeUp();
                    GlobalTargetInfo dest = targets[i + 1];
                    if (t.Map != dest.Map)
                    {
                        if (t is not Pawn p) continue;
                        p.teleporting = true;
                        p.ExitMap(true, Rot4.Invalid);
                        p.teleporting = false;
                        GenSpawn.Spawn(p, dest.Cell, dest.Map);
                    }

                    t.Position = dest.Cell;
                    AbilityUtility.DoClamor(t.Position, clamor.clamorRadius, this.pawn, clamor.clamorType);
                    AbilityUtility.DoClamor(dest.Cell,  clamor.clamorRadius, this.pawn, clamor.clamorType);
                    (t as Pawn)?.Notify_Teleported();
                }

            base.Cast(targets);
        }
    }
}
