using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheForce_ItsSorcery.Abilities
{
    public class CompAbility_MindTrick : CompAbilityEffect
    {
        public new CompProperties_MindTrick Props => (CompProperties_MindTrick)props;

        public void AssignJob(JobDef jobDef, Pawn pawn)
        {
            if (pawn == null || jobDef == null)
                return;

            Job job = JobMaker.MakeJob(jobDef, new LocalTargetInfo(pawn.Position));
            float num = 1f;
            if (Props.durationMultiplier != null)
            {
                num = pawn.GetStatValue(Props.durationMultiplier);
            }
            job.expiryInterval = (parent.def.GetStatValueAbstract(StatDefOf.Ability_Duration, parent.pawn) * num).SecondsToTicks();
            job.mote = MoteMaker.MakeThoughtBubble(pawn, parent.def.iconPath, maintain: true);
            RestUtility.WakeUp(pawn);
            pawn.jobs.StopAll();
            pawn.jobs.StartJob(job, JobCondition.InterruptForced);
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            // This method can be kept as is or updated if needed to use jobDef
            base.Apply(target, dest);
            if (target.Thing is Pawn pawn)
            {
                // Trigger the gizmo/window to select job from available jobs
                Find.WindowStack.Add(new Window_JobSelector(pawn, this));
            }
        }
    }

    public class CompProperties_MindTrick : CompProperties_AbilityEffect
    {
        public List<JobDef> availableJobs = new List<JobDef>();
        public StatDef durationMultiplier;

        public CompProperties_MindTrick()
        {
            this.compClass = typeof(CompAbility_MindTrick);
        }
    }

    public class Window_JobSelector : Window
    {
        private readonly Pawn pawn;
        private readonly CompAbility_MindTrick compAbilityEffect;

        public Window_JobSelector(Pawn pawn, CompAbility_MindTrick compAbilityEffect)
        {
            this.pawn = pawn;
            this.compAbilityEffect = compAbilityEffect;
            this.closeOnAccept = true;
            this.closeOnCancel = true;
            this.doCloseButton = true;
            this.doWindowBackground = true;
            this.absorbInputAroundWindow = true;

            // Manual centering
            float windowWidth = 400f;
            float windowHeight = 300f;
            float screenWidth = UI.screenWidth;
            float screenHeight = UI.screenHeight;

            this.windowRect = new Rect(
                (screenWidth - windowWidth) / 2f,
                (screenHeight - windowHeight) / 2f,
                windowWidth,
                windowHeight
            );
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            foreach (JobDef jobDef in compAbilityEffect.Props.availableJobs)
            {
                if (Widgets.ButtonText(listing.GetRect(30), jobDef.defName))
                {
                    compAbilityEffect.AssignJob(jobDef, pawn);
                    Close(true);
                }
            }

            listing.End();
        }
    }
}
