using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    public class HiltDef : Def
    {
        public GraphicData hiltgraphic;
    }

    public class HiltPartDef : Def
    {
        public HiltPartCategory category; // "PowerCell", "Casing", "Lens", "Emitter", "Crystal"
        public List<StatModifier> equippedStatOffsets;
        public Color color;
        public ThingDef requiredComponent;
        public ThingDef droppedComponent => requiredComponent;
    }

    public enum HiltPartCategory
    {
        PowerCell,
        Crystal,
        Lens,
        Emitter,
        Casing
    }

    public class StatPart_EquippedStatOffsetIncrease : StatPart
    {
        // Optional filter for a specific category (nullable to allow all categories when not defined)
        public HiltPartCategory? category;
        private static CompCache compCache = new CompCache();

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is not Pawn pawn || pawn.equipment?.Primary == null) return;
            var lightsaberComp = compCache.GetCachedComp(pawn.equipment.Primary);
            if (lightsaberComp == null || lightsaberComp.parent.ParentHolder?.ParentHolder is not Pawn weaponWearer || weaponWearer != pawn)
                return;
            var hiltParts = lightsaberComp.GetCurrentHiltParts();
            if (category.HasValue)
            {
                hiltParts = hiltParts.Where(part => part.category == category.Value).ToList();
            }
            float totalOffset = 0f;
            foreach (var part in hiltParts)
            {
                totalOffset += part.equippedStatOffsets?.Where(mod => mod.stat == parentStat).Sum(mod => mod.value) ?? 0f;
            }

            val += totalOffset;
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (req.Thing is not Pawn pawn || pawn.equipment?.Primary == null) return null;

            var lightsaberComp = compCache.GetCachedComp(pawn.equipment.Primary);
            if (lightsaberComp == null || lightsaberComp.parent.ParentHolder?.ParentHolder is not Pawn weaponWearer || weaponWearer != pawn)
                return null;
            var hiltParts = lightsaberComp.GetCurrentHiltParts();
            if (category.HasValue)
            {
                hiltParts = hiltParts.Where(part => part.category == category.Value).ToList();
            }
            StringBuilder explanation = new StringBuilder();
            float totalOffset = 0f;
            foreach (var part in hiltParts)
            {
                float partOffset = part.equippedStatOffsets?
                    .Where(mod => mod.stat == parentStat)
                    .Sum(mod => mod.value) ?? 0f;

                if (partOffset != 0f)
                {
                    explanation.AppendLine($"{part.label}: {partOffset:+0.##;-0.##} to {parentStat.label}");
                    totalOffset += partOffset;
                }
            }
            if (totalOffset != 0f)
            {
                explanation.AppendLine($"Total: {totalOffset:+0.##;-0.##} to {parentStat.label}");
                return explanation.ToString();
            }
            return null;
        }
    }
}
