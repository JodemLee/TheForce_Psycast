using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    internal class Ability_Mechu_TekRok : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            Thing targetThing = targets[0].Thing;

            // Check if the target is valid and not already studied using the matching surgery technique
            if (ValidateTarget(new LocalTargetInfo(targetThing)))
            {
                if (targetThing != null)
                {
                    if (targetThing is Pawn targetPawn)
                    {
                        Find.WindowStack.Add(new Dialog_ChooseImplant(targetPawn, CasterPawn, ReplaceImplant));
                    }
                    else if (targetThing.def.isTechHediff)
                    {
                        Pawn casterPawn = CasterPawn as Pawn;
                        if (casterPawn != null)
                        {
                            var hediffComp = casterPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_MechuLinkImplant)?.TryGetComp<HediffComp_MechuDeru>();
                            if (hediffComp != null)
                            {
                                var matchingSurgeries = DefDatabase<RecipeDef>.AllDefs
                                    .Where(recipe => recipe.ingredients.Any(ingredient => ingredient.IsFixedIngredient && ingredient.FixedIngredient == targetThing.def));

                                foreach (var surgery in matchingSurgeries)
                                {
                                    if (surgery.addsHediff != null)
                                    {
                                        hediffComp.StudyImplant(surgery.addsHediff);
                                    }
                                }

                                targetThing.Destroy(DestroyMode.Vanish);
                                Messages.Message($"Successfully studied {targetThing.LabelCap}.", MessageTypeDefOf.PositiveEvent);
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = false)
        {
            base.ValidateTarget(target, showMessages);
            Thing targetThing = target.Thing;
            Pawn casterPawn = CasterPawn as Pawn;
            if (casterPawn != null && targetThing != null)
            {
                var hediffComp = casterPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_MechuLinkImplant)?.TryGetComp<HediffComp_MechuDeru>();
                if (hediffComp != null)
                {
                    var matchingSurgeries = DefDatabase<RecipeDef>.AllDefs
                        .Where(recipe => recipe.ingredients.Any(ingredient => ingredient.IsFixedIngredient && ingredient.FixedIngredient == targetThing.def));

                    foreach (var surgery in matchingSurgeries)
                    {
                        if (surgery.addsHediff != null)
                        {
                            if (hediffComp.HasStudied(surgery.addsHediff))
                            {
                                if (showMessages)
                                {
                                    Messages.Message($"The implant {targetThing.LabelCap} has already been studied.", MessageTypeDefOf.RejectInput);
                                }
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        private void ReplaceImplant(Pawn pawn, Hediff_Implant oldImplant, HediffDef newImplantDef)
        {
            pawn.health.RemoveHediff(oldImplant);
            pawn.health.AddHediff(newImplantDef, oldImplant.Part);
            Messages.Message($"Replaced {oldImplant.LabelCap} with {newImplantDef.LabelCap} on {pawn.LabelCap}.", MessageTypeDefOf.PositiveEvent);
        }
    }

    public class Dialog_ChooseImplant : Window
    {
        private Pawn targetPawn;
        private Pawn casterPawn;  // Store the caster pawn
        private Action<Pawn, Hediff_Implant, HediffDef> onImplantSelected;

        private Vector2 scrollPosition;
        private bool highlight = true;

        public Dialog_ChooseImplant(Pawn targetPawn, Pawn casterPawn, Action<Pawn, Hediff_Implant, HediffDef> onImplantSelected)
        {
            this.targetPawn = targetPawn;
            this.casterPawn = casterPawn;  // Pass casterPawn here
            this.onImplantSelected = onImplantSelected;
            this.forcePause = true;
            this.closeOnAccept = false;
            this.closeOnCancel = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 500f); // Smaller window size

        public override void DoWindowContents(Rect inRect)
        {
            // Draw the header
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 30f), $"Select an implant to replace for {targetPawn.LabelCap}:");
            Text.Font = GameFont.Small;

            // Divide the window into two sections: left for the pawn portrait, right for the hediff list
            Rect leftRect = new Rect(0, 40f, inRect.width * 0.3f, inRect.height - 40f); // Smaller width for the portrait
            Rect rightRect = new Rect(leftRect.width, 40f, inRect.width - leftRect.width, inRect.height - 40f);

            // Draw the pawn's portrait on the left (scaled down)
            DrawPawnPortrait(leftRect, targetPawn);

            // Draw the hediff list on the right
            DrawHediffList(rightRect, targetPawn);
        }

        private void DrawPawnPortrait(Rect rect, Pawn pawn)
        {
            // Scale down the pawn's portrait
            Rect portraitRect = new Rect(rect.center.x - 40f, rect.center.y - 60f, 80f, 120f); // Smaller and centered
            GUI.DrawTexture(portraitRect, PortraitsCache.Get(pawn, portraitRect.size, Rot4.South));
        }

        private void DrawHediffList(Rect rect, Pawn pawn)
        {
            // Begin a scrollable list for the hediffs
            Rect scrollRect = new Rect(rect.x, rect.y, rect.width, rect.height - 16f);

            // Get and sort hediffs the same way vanilla does
            var hediffGroups = GetVisibleHediffGroups(pawn);
            float viewRectHeight = hediffGroups.Sum(group => group.Count() * 25f); // Smaller row height (25f)
            Rect viewRect = new Rect(0, 0, scrollRect.width - 16f, viewRectHeight);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0;

            // Iterate through sorted hediff groups and draw their rows
            foreach (var group in hediffGroups)
            {
                foreach (var hediff in group)
                {
                    Rect rowRect = new Rect(0, curY, viewRect.width, 25f); // Smaller row height
                    DrawHediffRow(rowRect, hediff);
                    curY += 25f; // Smaller row spacing
                }
            }

            Widgets.EndScrollView();
        }

        private IEnumerable<IGrouping<BodyPartRecord, Hediff_Implant>> GetVisibleHediffGroups(Pawn pawn)
        {
            // Get all implants and group them by body part
            var implants = pawn.health.hediffSet.hediffs.OfType<Hediff_Implant>();

            // Sort groups by body part priority (same as vanilla)
            return implants
                .GroupBy(hediff => hediff.Part)
                .OrderByDescending(group => GetListPriority(group.Key));
        }

        private float GetListPriority(BodyPartRecord part)
        {
            // Vanilla sorting logic for body parts
            if (part == null)
                return 9999999f; // Null parts go to the bottom
            return (float)((int)part.height * 10000) + part.coverageAbsWithChildren;
        }

        private void DrawHediffRow(Rect rect, Hediff_Implant hediff)
        {
            // Highlight the row on hover
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
            }

            // Draw the hediff label and body part
            float infoButtonSize = 20f; // Smaller info button
            float padding = 4f; // Smaller padding
            float colWidth = (rect.width - infoButtonSize - padding * 3) / 3; // Adjusted for three columns

            // Info card button
            if (Widgets.InfoCardButton(new Rect(rect.x + padding, rect.y + (rect.height - infoButtonSize) / 2, infoButtonSize, infoButtonSize), hediff))
            {
                Find.WindowStack.Add(new Dialog_InfoCard(hediff));
            }

            // Hediff label
            Widgets.Label(new Rect(rect.x + padding + infoButtonSize + padding, rect.y, colWidth, rect.height), hediff.LabelCap);

            // Body part label
            Widgets.Label(new Rect(rect.x + padding + infoButtonSize + padding + colWidth, rect.y, colWidth, rect.height), hediff.Part?.LabelCap ?? "WholeBody".Translate());

            // Select button (ensure it fits within the row)
            if (Widgets.ButtonText(new Rect(rect.x + padding + infoButtonSize + padding + colWidth * 2, rect.y, colWidth, rect.height), "Select"))
            {
                // Pass casterPawn along with the implant to the replacement dialog
                Find.WindowStack.Add(new Dialog_ChooseReplacement(targetPawn, hediff, casterPawn, onImplantSelected));
                Close();
            }

            // Tooltip for the row
            TooltipHandler.TipRegion(rect, () => GetHediffTooltip(hediff), hediff.GetHashCode());
        }

        private string GetHediffTooltip(Hediff_Implant hediff)
        {
            // Tooltip with hediff description and body part info
            StringBuilder tooltip = new StringBuilder();
            tooltip.AppendLine(hediff.LabelCap);
            tooltip.AppendLine(hediff.def.description);
            if (hediff.Part != null)
            {
                tooltip.AppendLine();
                tooltip.AppendLine("Body Part: " + hediff.Part.LabelCap);
            }
            return tooltip.ToString();
        }
    }

    public class Dialog_ChooseReplacement : Window
    {
        private Pawn targetPawn;
        private Hediff_Implant selectedImplant;
        private Pawn casterPawn;  // Store casterPawn
        private Action<Pawn, Hediff_Implant, HediffDef> onReplacementChosen;

        public Dialog_ChooseReplacement(Pawn targetPawn, Hediff_Implant selectedImplant, Pawn casterPawn, Action<Pawn, Hediff_Implant, HediffDef> onReplacementChosen)
        {
            this.targetPawn = targetPawn;
            this.selectedImplant = selectedImplant;
            this.casterPawn = casterPawn;  // Pass casterPawn here
            this.onReplacementChosen = onReplacementChosen;
            this.forcePause = true;
            this.closeOnAccept = false;
            this.closeOnCancel = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 30f), $"Select a replacement for {selectedImplant.LabelCap}:");
            Text.Font = GameFont.Small;

            Rect scrollRect = new Rect(0, 40f, inRect.width - 16f, inRect.height - 50f);
            Rect viewRect = new Rect(0, 0, scrollRect.width - 16f, GetReplacementOptions().Count() * 30f);

            Widgets.BeginScrollView(scrollRect, ref scrollPosition, viewRect);
            float curY = 0;

            foreach (var implantDef in GetReplacementOptions())
            {
                Rect rowRect = new Rect(0, curY, viewRect.width, 30f);
                float infoButtonSize = 24f;
                float padding = 5f;
                float colWidth = (rowRect.width - infoButtonSize - padding * 2) / 3;

                // Info card button
                if (Widgets.InfoCardButton(new Rect(rowRect.x + padding, rowRect.y + (rowRect.height - infoButtonSize) / 2, infoButtonSize, infoButtonSize), implantDef))
                {
                    Find.WindowStack.Add(new Dialog_InfoCard(implantDef));
                }

                // Implant label
                Widgets.Label(new Rect(rowRect.x + padding + infoButtonSize + padding, rowRect.y, colWidth, rowRect.height), implantDef.LabelCap);

                // Body part label
                Widgets.Label(new Rect(rowRect.x + padding + infoButtonSize + padding + colWidth, rowRect.y, colWidth, rowRect.height), selectedImplant.Part?.LabelCap ?? "WholeBody".Translate());

                // Select button
                if (Widgets.ButtonText(new Rect(rowRect.x + padding + infoButtonSize + padding + colWidth * 2, rowRect.y, colWidth, rowRect.height), "Select"))
                {
                    // Pass casterPawn along with the implant to the replacement dialog
                    onReplacementChosen(targetPawn, selectedImplant, implantDef);
                    Close();
                }

                curY += 30f;
            }

            Widgets.EndScrollView();
        }

        private Vector2 scrollPosition;

        private IEnumerable<HediffDef> GetReplacementOptions()
        {
            var bodyPartDef = selectedImplant.Part.def;
            var applicableHediffs = DefDatabase<RecipeDef>.AllDefs
                .Where(recipe => recipe.appliedOnFixedBodyParts != null &&
                                 recipe.appliedOnFixedBodyParts.Contains(bodyPartDef))
                .Select(recipe => recipe.addsHediff)
                .OfType<HediffDef>()
                .Where(implantDef => PawnHasStudiedImplant(casterPawn, implantDef)); // Filter by studied implants

            return applicableHediffs.Distinct();
        }

        private bool PawnHasStudiedImplant(Pawn pawn, HediffDef implantDef)
        {
            var studiedComp = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_MechuLinkImplant)?.TryGetComp<HediffComp_MechuDeru>();
            return studiedComp != null && studiedComp.HasStudied(implantDef);
        }
    }
}
