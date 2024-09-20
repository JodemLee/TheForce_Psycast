﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;

namespace ModularWeapon_Patch
{
    [StaticConstructorOnStartup]
    public class Gizmo_WeaponStance : Gizmo
    {
        private readonly Pawn pawn;
        private readonly Hediff hediff;
        private readonly ThingDef thingDef;
        private readonly List<StanceData> stanceDataList;
        private string[] severityTips;
        private const float ButtonWidth = 75f;
        private const float ButtonHeight = 75f;
        private DefStanceAngles extension;

        public Gizmo_WeaponStance(Pawn pawn, Hediff hediff, ThingDef thingDef)
        {
            this.pawn = pawn;
            this.hediff = hediff;
            this.thingDef = thingDef;

            // Cache the extension
            extension = thingDef.GetModExtension<DefStanceAngles>() ?? hediff.def.GetModExtension<DefStanceAngles>();
            stanceDataList = extension?.stanceData ?? new List<StanceData>();

            // Initialize severityTips
            severityTips = stanceDataList.Count > 0
                ? stanceDataList.Select(s => s.StanceString ?? "Unknown").ToArray()
                : new string[] { "Default Tooltip" };
        }

        public override float GetWidth(float maxWidth) => ButtonWidth;

        private string GetTipForSeverity(float severity)
        {
            var stanceData = extension?.GetStanceDataForSeverity(severity);
            return stanceData?.StanceString ?? "Default Tip";
        }

        private Texture2D GetStanceIconForSeverity(float severity)
        {
            var stanceData = extension?.GetStanceDataForSeverity(severity);
            if (stanceData != null)
            {
                if (string.IsNullOrEmpty(stanceData.StanceIconPath))
                {
                    Log.Warning("Stance icon path is null or empty.");
                    return null;
                }
                return ContentFinder<Texture2D>.Get(stanceData.StanceIconPath, true);
            }
            return null;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), ButtonHeight);
            Widgets.DrawMenuSection(rect);

            float currentSeverity = GetCurrentSeverity();
            Texture2D texture = GetStanceIconForSeverity(currentSeverity);

            if (texture != null)
            {
                Rect buttonRect = new Rect(rect.x, rect.y, texture.width, texture.height);
                Widgets.DrawTextureFitted(buttonRect, texture, 1f);
                Widgets.DrawHighlightIfMouseover(buttonRect);
                TooltipHandler.TipRegion(buttonRect, GetTipForSeverity(currentSeverity));
            }
            else
            {
                Log.Warning("Texture for severity " + currentSeverity + " is null.");
            }

            if (Widgets.ButtonInvisible(new Rect(rect.x, rect.y, ButtonWidth, ButtonHeight)))
            {
                Find.WindowStack.Add(new Dialog_WeaponStance(pawn, hediff, thingDef, stanceDataList, severityTips));
            }

            return new GizmoResult(GizmoState.Clear);
        }

        public float GetCurrentSeverity()
        {
            float severity = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def)?.Severity ?? 0f;
            return severity;
        }
    }

    public class Dialog_WeaponStance : Window
    {
        private readonly Pawn pawn;
        private readonly Hediff hediff;
        private readonly ThingDef thingDef;
        private readonly string[] severityTips;
        private readonly List<StanceData> stanceDataList;

        private Texture2D[] stanceTextures;
        private List<float> stanceAngles;
        private List<Vector3> drawOffsets;
        private int selectedStance;

        private readonly List<float> defaultStanceAngles;
        private readonly List<Vector3> defaultDrawOffsets;
        private Vector2 scrollPosition = Vector2.zero;

        // Store the last saved stance angles and offsets
        private List<float> lastSavedStanceAngles;
        private List<Vector3> lastSavedDrawOffsets;

        public Dialog_WeaponStance(Pawn pawn, Hediff hediff, ThingDef thingDef, List<StanceData> stanceDataList, string[] severityTips)
        {
            this.pawn = pawn;
            this.hediff = hediff;
            this.thingDef = thingDef;
            this.stanceDataList = stanceDataList ?? throw new ArgumentNullException(nameof(stanceDataList));
            this.severityTips = severityTips ?? throw new ArgumentNullException(nameof(severityTips));

            stanceTextures = stanceDataList.Select(s => ContentFinder<Texture2D>.Get(s.StanceIconPath, true)).ToArray();

            // Initialize default angles and offsets
            defaultStanceAngles = stanceDataList.Select(s => s.Angle).ToList();
            defaultDrawOffsets = stanceDataList.Select(s => s.Offset).ToList();

            // Initialize saved angles and offsets
            var lightsaberStanceComp = pawn.equipment?.Primary?.GetComp<Comp_WeaponStance>();
            stanceAngles = lightsaberStanceComp?.savedStanceAngles ?? new List<float>(defaultStanceAngles);
            drawOffsets = lightsaberStanceComp?.savedDrawOffsets ?? new List<Vector3>(defaultDrawOffsets);

            // Initialize last saved angles and offsets
            lastSavedStanceAngles = new List<float>(stanceAngles);
            lastSavedDrawOffsets = new List<Vector3>(drawOffsets);

            // Initialize lists to ensure correct count
            InitializeLists();

            // Set the selected stance based on the current severity
            float currentSeverity = GetCurrentSeverity();
            selectedStance = GetClosestStanceIndex(currentSeverity);
        }

        private int GetClosestStanceIndex(float severity)
        {
            int closestIndex = 0;
            float closestDifference = float.MaxValue;

            for (int i = 0; i < stanceDataList.Count; i++)
            {
                float stanceSeverity = stanceDataList[i].MinSeverity; // Assuming Angle represents severity here
                float difference = Mathf.Abs(severity - stanceSeverity);

                if (difference < closestDifference)
                {
                    closestDifference = difference;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private float GetCurrentSeverity()
        {
            // Get the current severity of the pawn's hediff
            return pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def)?.Severity ?? 0f;
        }

        private void InitializeLists()
        {
            // Ensure stanceAngles and drawOffsets match the stanceDataList count
            while (stanceAngles.Count < stanceDataList.Count)
            {
                // Use last saved angles or default angles if out of bounds
                stanceAngles.Add(lastSavedStanceAngles.Count > stanceAngles.Count ? lastSavedStanceAngles[stanceAngles.Count] : defaultStanceAngles[stanceAngles.Count]);
            }

            while (drawOffsets.Count < stanceDataList.Count)
            {
                // Use last saved offsets or default offsets if out of bounds
                drawOffsets.Add(lastSavedDrawOffsets.Count > drawOffsets.Count ? lastSavedDrawOffsets[drawOffsets.Count] : defaultDrawOffsets[drawOffsets.Count]);
            }
        }


        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            this.forcePause = false;


            if (stanceTextures.Length == 0 || stanceDataList.Count == 0)
            {
                Log.Warning("No stance textures or stance data available.");
            }

            selectedStance = Mathf.Clamp(selectedStance, 0, stanceTextures.Length - 1);

            Rect textureRect = new Rect(0f, 0f, 100f, 100f);
            Texture2D selectedTexture = stanceTextures[selectedStance];

            if (selectedTexture != null)
            {
                Widgets.DrawTextureFitted(textureRect, selectedTexture, 1f);
            }
            else
            {
                Widgets.Label(textureRect, "No Texture");
            }

            string stanceTip = severityTips[selectedStance];
            Rect scrollRect = new Rect(textureRect.xMax + 10f, 0f, inRect.width - textureRect.width - 20f, 100f); // Fixed size for the container
            Rect contentRect = new Rect(0f, 0f, scrollRect.width - 16f, Text.CalcHeight(stanceTip, scrollRect.width)); // Content rect to hold the full text

            // Add a scroll view for the text
            Widgets.BeginScrollView(scrollRect, ref scrollPosition, contentRect);
            Widgets.Label(contentRect, stanceTip);
            Widgets.EndScrollView();

            // Move the slider to a fixed position below the scroll area
            Rect sliderRect = new Rect(0f, scrollRect.yMax + 10f, inRect.width, 30f);
            selectedStance = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, selectedStance, 0, stanceTextures.Length - 1, true, null, null, null, 1f));
            Rect offsetSliderRect = new Rect(0f, sliderRect.yMax + 30f, inRect.width, 150f);

            if (selectedStance >= 0 && selectedStance < drawOffsets.Count && selectedStance < stanceAngles.Count)
            {
                Vector3 currentOffset = drawOffsets[selectedStance];
                float angleRotation = stanceAngles[selectedStance];


                if (Force_ModSettings.customStance)
                {
                    // Adjust sliders and modify offsetSliderRect
                    currentOffset.x = Widgets.HorizontalSlider(new Rect(offsetSliderRect.x, offsetSliderRect.y, offsetSliderRect.width, 30f), currentOffset.x, -10f, 10f, true, "X Offset: " + currentOffset.x.ToString("F1"));
                    currentOffset.y = Widgets.HorizontalSlider(new Rect(offsetSliderRect.x, offsetSliderRect.y + 35f, offsetSliderRect.width, 30f), currentOffset.y, -10f, 10f, true, "Y Offset: " + currentOffset.y.ToString("F1"));
                    currentOffset.z = Widgets.HorizontalSlider(new Rect(offsetSliderRect.x, offsetSliderRect.y + 70f, offsetSliderRect.width, 30f), currentOffset.z, -10f, 10f, true, "Z Offset: " + currentOffset.z.ToString("F1"));
                    angleRotation = Widgets.HorizontalSlider(new Rect(offsetSliderRect.x, offsetSliderRect.y + 105f, offsetSliderRect.width, 30f), angleRotation, -180f, 180f, true, "Angle Rotation: " + angleRotation.ToString("F1"));

                    drawOffsets[selectedStance] = currentOffset;
                    stanceAngles[selectedStance] = angleRotation;
                    ApplyStanceRotationAndOffset();
                    DrawApplyAndResetButtons(inRect, offsetSliderRect);
                }
                else
                {
                    // Apply and reset options for non-custom stances
                    ApplyStanceRotationAndOffset();
                    ResetToDefault();

                    // Dynamically position the Apply button after the last rendered element (offsetSliderRect)
                    Rect applyButtonRect = new Rect((inRect.width - 120f) / 2f, offsetSliderRect.yMax + 20f, 120f, 40f);

                    if (Widgets.ButtonText(applyButtonRect, "Apply"))
                    {
                        Hediff diff = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def);
                        if (diff != null)
                        {
                            diff.Severity = selectedStance + 1;
                        }
                        else
                        {
                            pawn.health.AddHediff(hediff.def).Severity = selectedStance + 1;
                        }
                        ApplyStanceRotationAndOffset();
                        Close();
                    }
                }
            }
            else { Log.Warning("Selected stance is out of bounds."); }
        }

        private void DrawApplyAndResetButtons(Rect inRect, Rect offsetSliderRect)
        {
            float buttonWidth = 120f;
            float buttonHeight = 40f;
            float spacing = 10f;

            Rect applyButtonRect = new Rect((inRect.width - buttonWidth * 2 - spacing) / 2f, offsetSliderRect.yMax + 20f, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(applyButtonRect, "Apply"))
            {
                Hediff diff = pawn.health.hediffSet.GetFirstHediffOfDef(hediff.def);
                if (diff != null)
                {
                    diff.Severity = selectedStance + 1;
                }
                else
                {
                    pawn.health.AddHediff(hediff.def).Severity = selectedStance + 1;
                }
                ApplyStanceRotationAndOffset();
                Close();
            }

            Rect resetButtonRect = new Rect(applyButtonRect.xMax + spacing, offsetSliderRect.yMax + 20f, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(resetButtonRect, "Reset"))
            {
                ResetToDefault();
            }
        }

        private void ResetToDefault()
        {
            // Reset stance angles and offsets to default values
            stanceAngles = new List<float>(defaultStanceAngles);
            drawOffsets = new List<Vector3>(defaultDrawOffsets);

            ApplyStanceRotationAndOffset();
        }

        private void ApplyStanceRotationAndOffset()
        {
            Comp_WeaponStance lightsaberStanceComp = pawn.equipment?.Primary?.GetComp<Comp_WeaponStance>();
            if (lightsaberStanceComp != null)
            {
                InitializeLists();

                // Ensure indices are within bounds
                if (selectedStance >= stanceAngles.Count || selectedStance >= drawOffsets.Count)
                {
                    // If out of bounds, restore to the last saved values
                    stanceAngles = new List<float>(lastSavedStanceAngles);
                    drawOffsets = new List<Vector3>(lastSavedDrawOffsets);
                }
                else
                {
                    // Save the current valid stance values
                    lastSavedStanceAngles = new List<float>(stanceAngles);
                    lastSavedDrawOffsets = new List<Vector3>(drawOffsets);
                }

                // Update the saved stance angles and offsets
                lightsaberStanceComp.savedStanceAngles[selectedStance] = stanceAngles[selectedStance];
                lightsaberStanceComp.savedDrawOffsets[selectedStance] = drawOffsets[selectedStance];

                // Update the lightsaber's visuals
                Comp_WeaponStance lightsaberComp = pawn.equipment?.Primary?.GetComp<Comp_WeaponStance>();
                if (lightsaberComp != null)
                {
                    lightsaberComp.UpdateRotationForStance(lightsaberStanceComp.savedStanceAngles[selectedStance]);
                    lightsaberComp.UpdateDrawOffsetForStance(lightsaberStanceComp.savedDrawOffsets[selectedStance]);
                }
            }
        }
    }
}