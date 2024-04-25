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
            "Form_III".Translate() ,
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
                Find.WindowStack.Add(new Dialog_Slider(hediff.LabelCap, 1, 7, newValue =>
                {
                    Hediff diff = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def);
                    if (diff != null)
                        diff.Severity = newValue;
                    else
                        pawn.health.AddHediff(hediff.def);
                }, (int)(GetCurrentSeverity())));
            }

            return new GizmoResult(GizmoState.Clear);
        }

        private float GetCurrentSeverity()
        {
            // Logic to determine the current severity of the hediff
            return pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def)?.Severity ?? 0f;
        }
    }
}