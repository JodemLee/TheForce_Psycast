using GraphicCustomization;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    [StaticConstructorOnStartup]
    public static class LightsaberGraphicsUtil
    {
        // Constants for offsets
        private const float HiltYOffset = 0f;
        private const float GlowYOffset = -1f;
        private const float CoreYOffset = -0.001f;
        private const float BladeYOffset = -0.002f;

        // Duration for scaling
        private const float scaleDuration = 0.5f; // 0.5 seconds for scaling

        // Cached values for performance
        private static Quaternion rotationCache;
        private static Mesh meshCache;
        private static Mesh meshFlipCache;

        // Cached scales
        private static Vector3 glowScaleCache;

        // Timer for scaling
        private static float scaleTimer;

        // Previous values to track changes and avoid unnecessary recalculations
        private static float? previousAngle;
        private static float? previousGlowRadius;

        // Cached offsets
        private static readonly Vector3 coreOffset = new Vector3(0f, CoreYOffset, 0f);
        private static readonly Vector3 bladeOffset = new Vector3(0f, BladeYOffset, 0f);
        private static readonly Vector3 hiltOffset = new Vector3(0f, HiltYOffset, 0f);
        private static readonly Vector3 glowOffset = new Vector3(0f, GlowYOffset, 0f);
        public static void DrawLightsaberGraphics(Thing eq, Vector3 drawLoc, float angle, bool flip, Comp_LightsaberBlade compLightsaberBlade)
        {
            var graphic1 = compLightsaberBlade.Graphic;
            var core1Graphic = compLightsaberBlade.LightsaberCore1Graphic;
            var blade2Graphic = compLightsaberBlade.LightsaberBlade2Graphic;
            var core2Graphic = compLightsaberBlade.LightsaberCore2Graphic;
            var glowGraphic = compLightsaberBlade.LightsaberGlowGraphic;
                
            // Skip if no graphics exist
            if (compLightsaberBlade.Graphic == null && compLightsaberBlade.LightsaberCore1Graphic == null &&
                compLightsaberBlade.LightsaberBlade2Graphic == null && compLightsaberBlade.LightsaberCore2Graphic == null)
            {
                return;
            }
            // Update the scaling for this lightsaber instance
            compLightsaberBlade.UpdateScalingAndOffset();

            // Calculate draw locations using cached offsets
            Vector3 coreDrawLoc = drawLoc + coreOffset;
            Vector3 bladeDrawLoc = drawLoc + bladeOffset;
            Vector3 hiltDrawLoc = drawLoc + hiltOffset;
            Vector3 glowDrawLoc = drawLoc + glowOffset;

            // Cache the rotation if the angle has changed
            if (!previousAngle.HasValue || Math.Abs(angle - previousAngle.Value) > 0.01f)
            {
                rotationCache = Quaternion.AngleAxis(angle, Vector3.up);
                previousAngle = angle;
            }

            // Cache the meshes based on the flip state
            meshCache = meshCache ?? MeshPool.plane10;
            meshFlipCache = meshFlipCache ?? MeshPool.plane10Flip;
            Mesh currentMesh = flip ? meshFlipCache : meshCache;

            // Cache glow scale
            if (glowScaleCache == default)
            {
                glowScaleCache = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.x) * Force_ModSettings.glowRadius * 1.3f;
            }

            float lenFactor = Rand.Range(compLightsaberBlade.minVibrate, compLightsaberBlade.minVibrate + compLightsaberBlade.vibrationrate);
            Vector3 scale = new Vector3(lenFactor, 1, 1);
            var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

            float lenFactor2 = Rand.Range(compLightsaberBlade.minVibrate, compLightsaberBlade.minVibrate + compLightsaberBlade.vibrationrate2);
            Vector3 scale2 = new Vector3(lenFactor2, 1, 1);
            var scaleMatrix2 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale2);

            // Cache rotation matrix if it changes
            if (!previousAngle.HasValue || Math.Abs(angle - previousAngle.Value) > 0.01f)
            {
                rotationCache = Quaternion.AngleAxis(angle, Vector3.up);
                previousAngle = angle;
            }

            // Precompute the base transformation matrices (scale, rotation, and position) for blade and core
            Matrix4x4 bladeMatrix1 = Matrix4x4.TRS(bladeDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore1AndBlade1);
            Matrix4x4 bladeMatrix2 = Matrix4x4.TRS(bladeDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore2AndBlade2);
            Matrix4x4 coreMatrix1 = Matrix4x4.TRS(coreDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore1AndBlade1);
            Matrix4x4 coreMatrix2 = Matrix4x4.TRS(coreDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore2AndBlade2);

            // Only apply additional scaling when needed (to avoid redundant multiplication)
            Matrix4x4 finalBladeMatrix1 = bladeMatrix1 * scaleMatrix;
            Matrix4x4 finalBladeMatrix2 = bladeMatrix2 * scaleMatrix2;

            // Use a single check for whether we should render graphics to avoid redundant operations
            if (graphic1 != null)
            {
                Graphics.DrawMesh(currentMesh, finalBladeMatrix1, graphic1.MatSingle, 0, null, 0, compLightsaberBlade.materialPropertyBlock);
            }
            if (core1Graphic != null)
            {
                Graphics.DrawMesh(currentMesh, coreMatrix1, core1Graphic.MatSingle, 0, null, 0, compLightsaberBlade.materialPropertyBlock);
            }
            if (blade2Graphic != null)
            {
                Graphics.DrawMesh(currentMesh, finalBladeMatrix2, blade2Graphic.MatSingle, 0, null, 0, compLightsaberBlade.materialPropertyBlock);
            }
            if (core2Graphic != null)
            {
                Graphics.DrawMesh(currentMesh, coreMatrix2, core2Graphic.MatSingle, 0, null, 0, compLightsaberBlade.materialPropertyBlock);
            }
            if (glowGraphic != null && Force_ModSettings.LightsaberFakeGlow)
            {
                Graphics.DrawMesh(currentMesh, Matrix4x4.TRS(glowDrawLoc, rotationCache, glowScaleCache), glowGraphic.MatSingle, 0, null, 0, compLightsaberBlade.materialPropertyBlock);
            }


            // Draw Glow if applicable
            if (Force_ModSettings.LightsaberFakeGlow && compLightsaberBlade.LightsaberGlowGraphic != null)
            {
                if (!previousGlowRadius.HasValue || Force_ModSettings.glowRadius != previousGlowRadius.Value)
                {
                    glowScaleCache = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.x) * Force_ModSettings.glowRadius * 1.3f;
                    previousGlowRadius = Force_ModSettings.glowRadius;
                }

                Graphics.DrawMesh(currentMesh, Matrix4x4.TRS(glowDrawLoc, rotationCache, glowScaleCache), compLightsaberBlade.LightsaberGlowGraphic.MatSingle, 0, null, 0, compLightsaberBlade.materialPropertyBlock);
            }

            // Draw Hilt (similar logic can be applied if needed)
            if (compLightsaberBlade.selectedhiltgraphic.hiltgraphic != null)
            {
                Material hiltMaterial = compLightsaberBlade.HiltGraphic.MatSingle;
                var graphicComp = compLightsaberBlade.parent.TryGetComp<CompGraphicCustomization>();

                if (graphicComp != null)
                {
                    var matSingle = eq.Graphic.MatSingle;
                    var s = new Vector3(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
                    var matrix = Matrix4x4.TRS(hiltDrawLoc, Quaternion.AngleAxis(angle, Vector3.up), s);
                    Graphics.DrawMesh(flip ? MeshPool.plane10Flip : MeshPool.plane10, matrix, matSingle, 0);

                }
                else
                {
                    Vector3 currentScaleForHilt = new Vector3(compLightsaberBlade.HiltGraphic.drawSize.x, 1f, compLightsaberBlade.HiltGraphic.drawSize.x);
                    Graphics.DrawMesh(currentMesh, Matrix4x4.TRS(hiltDrawLoc, rotationCache, currentScaleForHilt), hiltMaterial, 0, null, 0, compLightsaberBlade.hiltMaterialPropertyBlock);
                }
            }
        }

        private static Matrix4x4 cachedGlowMatrix;
        private static float cachedGlowRadius;

        public static void UpdateGlowMatrix(Vector3 glowDrawLoc, float glowRadius, Quaternion rotationCache)
        {
            if (cachedGlowRadius != glowRadius)
            {
                var glowScale = new Vector3(glowRadius, 1f, glowRadius) * 1.3f;
                cachedGlowMatrix = Matrix4x4.TRS(glowDrawLoc, rotationCache, glowScale);
                cachedGlowRadius = glowRadius;
            }
        }
    }

    public class CompCache
    {
        // Dictionary to store the cached component for each unique Thing (lightsaber)
        private readonly Dictionary<Thing, Comp_LightsaberBlade> cachedComps = new Dictionary<Thing, Comp_LightsaberBlade>();

        public Comp_LightsaberBlade GetCachedComp(Thing thing)
        {
            if (thing == null)
                return null;

            // Check if the component for this specific Thing is already cached
            if (cachedComps.TryGetValue(thing, out var cachedComp))
            {
                return cachedComp;
            }

            // Cache the component for this Thing if it hasn't been cached already
            var thingWithComps = thing as ThingWithComps;
            var comp = thingWithComps?.GetComp<Comp_LightsaberBlade>();

            if (comp != null)
            {
                cachedComps[thing] = comp;
            }

            return comp;
        }

        public void ClearCache(Thing thing)
        {
            // Remove the cache entry for this specific Thing
            if (thing != null)
            {
                cachedComps.Remove(thing);
            }
        }

        public void ClearAllCaches()
        {
            cachedComps.Clear();
        }
    }

    public class CompCacheGraphicCustomization
    {
        // Dictionary to store the cached component for each unique Thing (lightsaber)
        private readonly Dictionary<Thing, CompGraphicCustomization> cachedComps = new Dictionary<Thing, CompGraphicCustomization>();

        public CompGraphicCustomization GetCachedComp(Thing thing)
        {
            if (thing == null)
                return null;

            // Check if the component for this specific Thing is already cached
            if (cachedComps.TryGetValue(thing, out var cachedComp))
            {
                return cachedComp;
            }

            // Cache the component for this Thing if it hasn't been cached already
            var thingWithComps = thing as ThingWithComps;
            var comp = thingWithComps?.GetComp<CompGraphicCustomization>();

            if (comp != null)
            {
                cachedComps[thing] = comp;
            }

            return comp;
        }

        public void ClearCache(Thing thing)
        {
            // Remove the cache entry for this specific Thing
            if (thing != null)
            {
                cachedComps.Remove(thing);
            }
        }

        public void ClearAllCaches()
        {
            cachedComps.Clear();
        }
    }

    public static class lightsaberRetrievalUtility
    {
        public static Comp_LightsaberBlade GetCompLightsaber(this Thing thing)
        {
            if (thing is ThingWithComps thingWithComps)
            {
                var comps = thingWithComps.AllComps;
                for (int i = 0, count = comps.Count; i < count; i++)
                {
                    if (comps[i] is Comp_LightsaberBlade comp)
                        return comp;
                }
            }
            return null;
        }
    }

    public static class DarksideUtility
        {
            public static void AdjustHediffSeverity(Pawn pawn, HediffDef hediffDef, float severityIncrease)
            {
                var hediff = pawn?.health?.hediffSet?.GetFirstHediffOfDef(hediffDef);
                if (hediff != null)
                {
                    hediff.Severity += severityIncrease;
                }
            }
        }
    }


