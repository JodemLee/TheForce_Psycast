using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            "Form I: Shii-Cho" + "\n\n" + "Shii-Cho, the Way of the Sarlacc, is the most basic form focusing on wide, sweeping strikes and disarming techniques, ideal for beginners seeking simplicity and versatility in combat."  + "\n\n" + " [CLICK HERE TO CHANGE] ",
            "Form II: Makashi" + "\n\n" + "Makashi, the Way of the Ysalamiri:, is characterized by its elegant and precise movements, emphasizing dueling and defensive maneuvers, making it favored by those seeking finesse and sophistication in combat."  + "\n\n" + " [CLICK HERE TO CHANGE] ",
            "Form III: Soresu" + "\n\n" + "Soresu, the Way of the Mynock, is a defensive form focused on tight, efficient movements and quick reflexes, designed to provide maximum protection against blaster fire and lightsaber attacks."  + "\n\n" + " [CLICK HERE TO CHANGE] " ,
            "Form IV: Ataru" + "\n\n" + "Ataru, the Way of the Hawk-Bat, is an acrobatic and aggressive style characterized by its fast-paced, energetic movements, incorporating flips and spins to overwhelm opponents with speed and unpredictability."  + "\n\n" + " [CLICK HERE TO CHANGE] ",
            "Form V: Shien/Djem So" + "\n\n" + "Shien and Djem So, also known as the Perseverance and Way of the Krayt Dragon respectively, are variants of the same form emphasizing powerful counterattacks and aggressive deflection, suited for warriors seeking to turn their opponent's strength against them." + "\n\n" + " [CLICK HERE TO CHANGE] ",
            "Form VI: Niman" + "\n\n" + "Niman, the Way of the Rancor, is a balanced form incorporating elements of all previous forms, providing practitioners with versatility and adaptability in combat situations, making it a preferred choice for diplomats and peacekeepers."  + "\n\n" + " [CLICK HERE TO CHANGE] ",
            "Form VII: Juyo/Vaapad" + "\n\n" + "Juyo and its variant Vaapad, the Ferocity and the Way of the Vornskr respectively, are aggressive and unpredictable styles that channel the user's inner darkness, offering immense offensive power at the risk of succumbing to the dark side of the Force."  + "\n\n" + " [CLICK HERE TO CHANGE] "
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