using RimWorld;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    public class ITab_Pawn_Alignment : ITab
    {
        private Vector2 thoughtScrollPosition;  
        public static readonly Vector3 PawnTextureCameraOffset = default(Vector3);
        private bool showHat = false;
        private Rot4 rot = new Rot4(2);
        public const float pawnPanelSize = 256f;

        private Pawn PawnToShowInfoAbout
        {
            get
            {
                Pawn pawn = null;
                if (base.SelPawn != null)
                {
                    pawn = base.SelPawn;
                }
                else if (base.SelThing is Corpse corpse)
                {
                    pawn = corpse.InnerPawn;
                }
                if (pawn == null)
                {
                    Log.Error("Character tab found no selected pawn to display.");
                    return null;
                }
                return pawn;
            }
        }


        public override bool IsVisible
        {
            get
            {
                if (base.SelPawn == null)
                {
                    return false; // Return false if SelPawn is null
                }

                if (base.SelPawn.RaceProps.Animal && base.SelPawn.Faction == null)
                {
                    return false;
                }
                if (base.SelPawn.RaceProps.Insect && base.SelPawn.Faction != Faction.OfPlayer)
                {
                    return false;
                }
                if (base.SelPawn.needs != null)
                {
                    return base.SelPawn.needs.AllNeeds.Count > 0;
                }
                return false;
            }
        }

        public ITab_Pawn_Alignment()
        {
            labelKey = "Force.Alignment";
            tutorTag = "Force.Alignment";
        }

        public override void OnOpen()
        {
            thoughtScrollPosition = default(Vector2);
        }

        protected override void FillTab()
        {
            Rect tabRect = new Rect(0f, 0f, size.x, size.y);

            if (base.SelPawn != null && base.SelPawn.story != null)
            {
                StatDef statA = ForceDefOf.Force_Darkside_Attunement;
                StatDef statB = ForceDefOf.Force_Lightside_Attunement;

                Rect statRect = new Rect(tabRect.x, tabRect.y, tabRect.width, 30f);
                ForceAlignmentUtility.DoStats(statRect, base.SelPawn);
                Rect PawnRect = new Rect(32, 64, pawnPanelSize, pawnPanelSize);
                DrawColonist(PawnRect, base.SelPawn);
                UpdateSize();
                ForceAlignmentUtility.DrawCharacterCard(tabRect, PawnToShowInfoAbout);
            }
        }

        private void DrawColonist(Rect rect, Pawn pawn)
        {
            Vector2 pos = new Vector2(rect.width, rect.height);
            GUI.DrawTexture(rect, PortraitsCache.Get(pawn, pos, rot, PawnTextureCameraOffset, 1f, true, true, showHat, true, null, null, true));
            var pawnRefer = new Rect((rect.x + rect.width), 40f, 48, 48);
            pawnRefer.xMin += (pawnRefer.width - 24) / 2;
            pawnRefer.width = 24;
        }

        protected void UpdateSize()
        {
            size = ForceAlignmentUtility.GetSize(base.SelPawn);
        }
    }
    [StaticConstructorOnStartup]
    public static class ForceAlignmentUtility
    {
        public static Texture2D AlignmentBar = ContentFinder<Texture2D>.Get("UI/Force_AlignmentBar");
        public static Texture2D customText = ContentFinder<Texture2D>.Get("UI/Force_AlignmentText");
        public static Regex ValidNameRegex = new Regex("^[\\p{L}0-9 '\\-.]*$");
        public static void DoStats(Rect rect, Pawn pawn)
        {
            float statA = pawn.GetStatValue(ForceDefOf.Force_Darkside_Attunement, true);
            float statB = pawn.GetStatValue(ForceDefOf.Force_Lightside_Attunement, true);
            float statDifference = (statA - statB) * 10;
            DrawBackgroundAndTexture(rect);
            DrawHorizontalLine(rect, statDifference);
        }

        private static void DrawBackgroundAndTexture(Rect rect)
        {
            Color originalColor = GUI.color;
            float AlignmentXPost = 32f;
            float AlignmentYPost = 32f;
            float AlignmentWidth = 32f;
            float AlignmentHeight = 350f;


            // Draw the background bar
            GUI.color = originalColor;
            GUI.DrawTexture(new Rect(AlignmentXPost, AlignmentYPost, AlignmentWidth, AlignmentHeight), AlignmentBar);

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static void DrawHorizontalLine(Rect rect, float statDifference)
        {
            float HorizontalXPost = 32f;
            float HorizontalWidth = 32f;
            float HorizontalHeight = 3f;
            float centerAlignmentY = (rect.y + rect.yMax) * 6.9f;

            // Adjust these values to set the upper and lower limits
            float upperLimit = centerAlignmentY + 170f;  // Example upper limit
            float lowerLimit = centerAlignmentY - 170f;  // Example lower limit

            // Calculate the new vertical position, clamped within the limits
            float HorizontalYPost = Mathf.Clamp(centerAlignmentY + (statDifference * 15f), lowerLimit, upperLimit);

            GUI.color = Color.white;

            GUI.DrawTexture(new Rect(HorizontalXPost, HorizontalYPost, HorizontalWidth, HorizontalHeight), BaseContent.WhiteTex);

            Text.Anchor = TextAnchor.UpperLeft;
        }
        public static Vector2 GetSize(Pawn pawn)
        {
            return new Vector2(300f, 400f);
        }

        public static void DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback = null, Rect creationRect = default(Rect), bool showName = true)
        {
            float XaxisPost = 80f;
            float YaxisPost = 360f;
            float LabelWidth = 180f;
            float LabelHeight = 64f;
            bool flag = randomizeCallback != null;

            if (showName)
            {
                // Draw pawn's name
                Rect NameRect = new Rect(75f, 40, LabelWidth, LabelHeight * 2);
                NameTriple nameTriple = pawn.Name as NameTriple;
                if (flag && nameTriple != null)
                {
                    Rect rect4 = new Rect(NameRect) { width = NameRect.width * 0.333f };
                    Rect rect5 = new Rect(NameRect) { width = NameRect.width * 0.333f, x = rect4.x + rect4.width };
                    Rect rect6 = new Rect(NameRect) { width = NameRect.width * 0.333f, x = rect5.x + rect5.width };

                    string name = nameTriple.First;
                    string name2 = nameTriple.Nick;
                    string name3 = nameTriple.Last;

                    DoNameInputRect(rect4, ref name, 12);
                    GUI.color = (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last) ?
                        new Color(1f, 1f, 1f, 0.5f) : Color.white;
                    DoNameInputRect(rect5, ref name2, 16);
                    GUI.color = Color.white;
                    DoNameInputRect(rect6, ref name3, 12);

                    if (nameTriple.First != name || nameTriple.Nick != name2 || nameTriple.Last != name3)
                    {
                        pawn.Name = new NameTriple(name, string.IsNullOrEmpty(name2) ? name : name2, name3);
                    }

                    TooltipHandler.TipRegionByKey(rect4, "FirstNameDesc");
                    TooltipHandler.TipRegionByKey(rect5, "ShortIdentifierDesc");
                    TooltipHandler.TipRegionByKey(rect6, "LastNameDesc");
                }
                else
                {
                    NameRect.width = 999f;
                    Text.Font = GameFont.Medium;
                    Widgets.Label(NameRect, pawn.Name.ToStringFull.CapitalizeFirst());

                    if (pawn.guilt != null && pawn.guilt.IsGuilty)
                    {
                        float x = Text.CalcSize(pawn.Name.ToStringFull).x;
                        Rect guiltyRect = new Rect(x + 10f, 0f, 32f, 32f);
                        GUI.DrawTexture(guiltyRect, TexUI.GuiltyTex);
                        TooltipHandler.TipRegion(guiltyRect, () => pawn.guilt.Tip, 6321623);
                    }

                    Text.Font = GameFont.Small;
                }

                // Lightside Alignment
                Rect lightRect = new Rect(XaxisPost, YaxisPost - 70f, LabelWidth, LabelHeight);
                string LightSideLabel = "Lightside Alignment";
                float LightsideStatValue = pawn.GetStatValue(ForceDefOf.Force_Lightside_Attunement);
                Widgets.Label(lightRect, $"{LightSideLabel}: {LightsideStatValue:P0}");

                // Darkside Alignment
                Rect darkRect = new Rect(XaxisPost, YaxisPost - 50f, LabelWidth, LabelHeight);
                string DarkSideLabel = "Darkside Alignment";
                float DarksideStatValue = pawn.GetStatValue(ForceDefOf.Force_Darkside_Attunement);
                Widgets.Label(darkRect, $"{DarkSideLabel}: {DarksideStatValue:P0}");
            }
        }

        public static void DoNameInputRect(Rect rect, ref string name, int maxLength)
        {
            string text = Widgets.TextField(rect, name);
            if (text.Length <= maxLength && ValidNameRegex.IsMatch(text))
            {
                name = text;
            }
        }
    }
}