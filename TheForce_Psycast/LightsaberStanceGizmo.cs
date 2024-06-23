using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    internal class Gizmo_LightsaberStance : Gizmo
    {
        private readonly Pawn pawn;
        private readonly Hediff hediff;
        private readonly Texture2D[] stanceTextures;
        private const float ButtonWidth = 75f;
        private const float ButtonHeight = 75f;

        public Gizmo_LightsaberStance(Pawn pawn, Hediff hediff)
        {
            this.pawn = pawn;
            this.hediff = hediff;

            // Initialize the stanceTextures array
            stanceTextures = new Texture2D[] {
                ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/Stance_Form1", true),
                ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/Stance_Form2", true),
                ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/Stance_Form3", true),
                ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/Stance_Form4", true),
                ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/Stance_Form5", true),
                ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/Stance_Form6", true),
                ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/Stance_Form7", true)
            };
        }

        public override float GetWidth(float maxWidth)
        {
            return ButtonWidth;
        }

        private string[] severityTips = new string[]
        {
            "Form_I".Translate(),
            "Form_II".Translate(),
            "Form_III".Translate(),
            "Form_IV".Translate(),
            "Form_V".Translate(),
            "Form_VI".Translate(),
            "Form_VII".Translate()
        };

        private string GetTipForSeverity(float severity)
        {
            int index = Mathf.Clamp(Mathf.FloorToInt(severity) - 1, 0, severityTips.Length - 1);
            return severityTips[index];
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), ButtonHeight);
            Widgets.DrawMenuSection(rect);

            Texture2D texture = stanceTextures[Mathf.Clamp((int)(GetCurrentSeverity()) - 1, 0, stanceTextures.Length - 1)];
            Rect buttonRect = new Rect(rect.x, rect.y, texture.width, texture.height);
            Widgets.DrawTextureFitted(buttonRect, texture, 1f);
            Widgets.DrawHighlightIfMouseover(buttonRect);
            TooltipHandler.TipRegion(buttonRect, GetTipForSeverity(GetCurrentSeverity()));

            if (Widgets.ButtonInvisible(buttonRect))
            {
                Find.WindowStack.Add(new Dialog_LightsaberStance(pawn, hediff, stanceTextures, severityTips));
            }

            return new GizmoResult(GizmoState.Clear);
        }

        private float GetCurrentSeverity()
        {
            // Logic to determine the current severity of the hediff
            return pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def)?.Severity ?? 0f;
        }
    }


    public class Dialog_LightsaberStance : Window
    {
        private readonly Pawn pawn;
        private readonly Hediff hediff;
        private readonly Texture2D[] stanceTextures;
        private readonly string[] severityTips;
        private int selectedStance;

        public Dialog_LightsaberStance(Pawn pawn, Hediff hediff, Texture2D[] stanceTextures, string[] severityTips)
        {
            this.pawn = pawn;
            this.hediff = hediff;
            this.stanceTextures = stanceTextures;
            this.severityTips = severityTips;
            this.forcePause = true;
            this.doCloseX = true;
            this.selectedStance = Mathf.Clamp((int)(hediff.Severity) - 1, 0, stanceTextures.Length - 1);
        }

        public override Vector2 InitialSize => new Vector2(600f, 250f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            // Draw the texture
            Rect textureRect = new Rect(0f, 0f, 100f, 100f);
            Widgets.DrawTextureFitted(textureRect, stanceTextures[selectedStance], 1f);

            // Draw the description
            Rect descriptionRect = new Rect(textureRect.xMax + 10f, 0f, inRect.width - textureRect.width - 20f, 105f);
            Widgets.Label(descriptionRect, severityTips[selectedStance]);

            // Draw the slider
            Rect sliderRect = new Rect(0f, descriptionRect.yMax + 30f, inRect.width, 30f);
            selectedStance = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, selectedStance, 0, stanceTextures.Length - 1, true, null, null, null, 1f));

            // Apply button
            Rect applyButtonRect = new Rect((inRect.width - 120f) / 2f, inRect.height - 40f, 120f, 40f);
            if (Widgets.ButtonText(applyButtonRect, "Apply"))
            {
                Hediff diff = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def);
                if (diff != null)
                    diff.Severity = selectedStance + 1;
                else
                    pawn.health.AddHediff(hediff.def).Severity = selectedStance + 1;
                Close();
            }
        }

    }
}

