using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;

namespace TheForce_Psycast.Abilities.Lightside
{
    public class Hediff_ProtectionBubble : Hediff_Overshield
    {
        public override Color OverlayColor => GetColorForPawn();

        private Color GetColorForPawn()
        {
            if (pawn?.story?.favoriteColor.HasValue ?? false)
            {
                return (Color)pawn.story.favoriteColor;
            }
            else
            {
                // Default color when pawn doesn't have a favorite color (or any other condition)
                return new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.5f); // Example: White with 50% opacity
            }
        }
    }
}
