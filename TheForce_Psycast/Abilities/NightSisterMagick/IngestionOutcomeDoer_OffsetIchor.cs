﻿using RimWorld;
using System.Collections.Generic;
using TheForce_Psycast.NightSisterMagick;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    internal class IngestionOutcomeDoer_OffsetIchor : IngestionOutcomeDoer
    {
        public float offset;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingest)
        {
            Force_GeneMagick.OffsetIchor(pawn, offset * (float)ingested.stackCount);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (ModsConfig.BiotechActive)
            {
                string text = ((offset >= 0f) ? "+" : string.Empty);
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsNonPawnImportant, "Ichor".Translate().CapitalizeFirst(), text + Mathf.RoundToInt(offset * 100f), "IchorDesc".Translate(), 1000);
            }
        }
    }
}