using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;
using VPEPuppeteer;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities
{
    internal class Master_Apprentice : Ability
    {
        private Hediff_Master masterHediff;

        public override void Init()
        {
            base.Init();
            masterHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
            if (masterHediff == null)
            {
                CreateMasterHediff();
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Pawn == null || !this.CasterPawn.HasPsylink)
            {
                return false;
            }

            // Check if the target is a psycaster
            if (!target.Pawn.HasPsylink)
            {
                if (showMessages)
                {
                    Messages.Message("Force.TargetIsNotPsycaster".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }

            // Check if the psycaster is already an apprentice
            if (target.Pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Apprentice) != null)
            {
                if (showMessages)
                {
                    Messages.Message("Force.TargetIsApprenticeAlready".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }

            // Check if the psycaster's psylink level is lower than the caster's level
            if (target.Pawn.GetPsylinkLevel() >= this.CasterPawn.GetPsylinkLevel())
            {
                if (showMessages)
                {
                    Messages.Message("Force.TargetPsylinkLevelTooHigh".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }
        public override bool IsEnabledForPawn(out string reason)
        {
            if (masterHediff != null && masterHediff.apprentices.Count >= masterHediff.apprenticeCapacity)
            {
                reason = "Force.CannotHaveMoreApprentice".Translate();
                return false;
            }
            return base.IsEnabledForPawn(out reason);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets == null || targets.Length == 0 || targets[0].Thing == null)
            {
                return;
            }

            var target = targets[0].Thing as Pawn;

            if (masterHediff == null)
            {
                CreateMasterHediff();
            }

            var puppetHediff = HediffMaker.MakeHediff(ForceDefOf.Force_Apprentice, target, target.health.hediffSet.GetBrain()) as Hediff_Apprentice;
            puppetHediff.master = pawn;
            masterHediff.apprentices.Add(target);
            target.health.AddHediff(puppetHediff);

            target.Notify_DisabledWorkTypesChanged();
            PawnComponentsUtility.AddAndRemoveDynamicComponents(target);
        }

        private void CreateMasterHediff()
        {
            masterHediff = HediffMaker.MakeHediff(ForceDefOf.Force_Master, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_Master;
            pawn.health.AddHediff(masterHediff);
        }

    }

    public class Hediff_Master : Hediff_PuppetBase
    {
        public HashSet<Pawn> apprentices = new HashSet<Pawn>();
        public int apprenticeCapacity = 1;
        private ApprenticeBandwidthGizmo bandwidthGizmo;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref apprentices, "apprentices", LookMode.Reference);
            Scribe_Values.Look(ref apprenticeCapacity, "apprenticeCapacity", 1);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!preventRemoveEffects && !pawn.IsAliveOrTransferingMind())
            {
                Clearapprentices();
            }
        }

        private void Clearapprentices()
        {
            pawn.SpawnMoteAttached(VPEP_DefOf.VPEP_PsycastAreaEffect, 9999);
            foreach (var puppet in apprentices)
            {
                var hediff = puppet.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Apprentice);
                if (hediff != null)
                {
                    puppet.health.RemoveHediff(hediff);
                }
            }
            VPEP_DefOf.VPEP_Puppet_Master_Death.PlayOneShot(pawn);
            apprentices.Clear();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.pawn.IsColonistPlayerControlled)
            {
                yield return bandwidthGizmo ?? (bandwidthGizmo = new ApprenticeBandwidthGizmo(this));
            }
        }
    }

    public class Hediff_Apprentice : Hediff_PuppetBase
    {
        public Pawn master;
        private int ticksSinceLastXPGain = 0;
        public int xpGainInterval;

        public override string Label => base.Label + ": " + master.LabelShort;

        public Hediff_Apprentice()
        {
            xpGainInterval = Rand.Range(120000, 300000);
        }

        public override void Tick()
        {
            base.Tick();
            ticksSinceLastXPGain++;
            if (ticksSinceLastXPGain >= xpGainInterval)
            {
                GainExperienceAndCheckLevels();
                ticksSinceLastXPGain = 0;
                ApplyRandomHediff();
            }
        }

        private void GainExperienceAndCheckLevels()
        {
            var target = pawn;
            var totalXP = master.GetPsylinkLevel() - target.GetPsylinkLevel();
            pawn.Psycasts().GainExperience(totalXP * 10f);
            if (target.GetPsylinkLevel() > master.GetPsylinkLevel())
                EndApprenticeship();
        }

        private void EndApprenticeship()
        {
            Log.Message("Apprenticeship ended: Apprentice surpassed master's psycast level.");
            pawn.health.RemoveHediff(this);
        }

        private void ApplyRandomHediff()
        {
            if (Rand.Chance(0.1f)) // Change chance as needed
                ApplySpecificHediff(master, pawn);
        }

        private void ApplySpecificHediff(Pawn target, Pawn master)
        {
            HediffDef hediffDef = ForceDefOf.ForceBond_MasterApprentice;
            Hediff hediff = HediffMaker.MakeHediff(hediffDef, target);
            target.health.AddHediff(hediff);
            master.health.AddHediff(hediff);
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            pawn.health.RemoveHediff(this);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!preventRemoveEffects)
            {

                if (master != null)
                {
                    var hediff = master.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
                    hediff?.apprentices.Remove(pawn);
                }
            }
        }

        private void RemoveHediffs()
        {
            // Logic to remove specific hediffs if needed
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref master, "master");
        }
    }

    public abstract class Hediff_PuppetBase : HediffWithComps
    {
        public static bool preventRemoveEffects;
    }

    public class ApprenticeBandwidthGizmo : Gizmo
    {
        public const int InRectPadding = 6;

        private static readonly Color EmptyBlockColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        private static readonly Color FilledBlockColor = Color.yellow;

        private static readonly Color ExcessBlockColor = ColorLibrary.Red;

        private Hediff_Master hediff;

        public override bool Visible => Find.Selector.SelectedPawns.Count == 1;

        public ApprenticeBandwidthGizmo(Hediff_Master hediff)
        {
            this.hediff = hediff;
            Order = -90f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect rect2 = rect.ContractedBy(InRectPadding);
            Widgets.DrawWindowBackground(rect);

            int totalBandwidth = hediff.apprenticeCapacity;
            int usedBandwidth = hediff.apprentices.Count;
            string text = $"{usedBandwidth} / {totalBandwidth}";

            // Tooltip
            string tooltipText = $"Force.Apprentices".Translate().Colorize(ColoredText.TipSectionTitleColor) + $": {text}\n\n";
            if (usedBandwidth > 0)
            {
                IEnumerable<string> entries = hediff.apprentices.Select(p => p.LabelCap);
                tooltipText += $"Force.ApprenticeUsage".Translate() + "\n" + entries.ToLineList(" - ");
            }
            TooltipHandler.TipRegion(rect, tooltipText);

            // Labels
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Rect labelRect = new Rect(rect2.x, rect2.y, rect2.width, 30f);
            Widgets.Label(labelRect, "Force.Apprentices".Translate());
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(labelRect, text);

            // Draw bandwidth bars
            Rect bandwidthBarRect = new Rect(rect2.x, rect2.y + 30f, rect2.width, rect2.height - 30f);
            float usedBandwidthRatio = (float)usedBandwidth / (float)totalBandwidth;
            float usedBandwidthWidth = bandwidthBarRect.width * usedBandwidthRatio;

            Rect filledBandwidthRect = new Rect(bandwidthBarRect.x, bandwidthBarRect.y, usedBandwidthWidth, bandwidthBarRect.height);
            Widgets.DrawRectFast(filledBandwidthRect, FilledBlockColor);

            Rect emptyBandwidthRect = new Rect(bandwidthBarRect.x + usedBandwidthWidth, bandwidthBarRect.y, bandwidthBarRect.width - usedBandwidthWidth, bandwidthBarRect.height);
            Widgets.DrawRectFast(emptyBandwidthRect, EmptyBlockColor);

            return new GizmoResult(GizmoState.Clear);
        }

        public override float GetWidth(float maxWidth)
        {
            return 136f;
        }
    }
}


