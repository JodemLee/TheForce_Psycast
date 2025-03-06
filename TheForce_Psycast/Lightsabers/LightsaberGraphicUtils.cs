using GraphicCustomization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;
using Verse.Noise;

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
        private static Vector3 cachedGlowPosition;
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
        // Cached matrices
        private static Matrix4x4 cachedGlowMatrix;
        private static float cachedGlowRadius;
        public static void DrawLightsaberGraphics(Thing eq, Vector3 drawLoc, float angle, bool flip, Comp_LightsaberBlade compLightsaberBlade)
        {
            var graphic1 = compLightsaberBlade.Graphic;
            var core1Graphic = compLightsaberBlade.LightsaberCore1Graphic;
            var blade2Graphic = compLightsaberBlade.LightsaberBlade2Graphic;
            var core2Graphic = compLightsaberBlade.LightsaberCore2Graphic;
            var glowGraphic = compLightsaberBlade.LightsaberGlowGraphic;
            if (graphic1 == null && core1Graphic == null && blade2Graphic == null && core2Graphic == null)
            {
                return;
            }
            compLightsaberBlade.UpdateScalingAndOffset();
            Vector3 coreDrawLoc = drawLoc + coreOffset;
            Vector3 bladeDrawLoc = drawLoc + bladeOffset;
            Vector3 hiltDrawLoc = drawLoc + hiltOffset;
            Vector3 glowDrawLoc = drawLoc + glowOffset;
            if (!previousAngle.HasValue || Math.Abs(angle - previousAngle.Value) > 0.01f)
            {
                rotationCache = Quaternion.AngleAxis(angle, Vector3.up);
                previousAngle = angle;
            }
            meshCache = meshCache ?? MeshPool.plane10;
            meshFlipCache = meshFlipCache ?? MeshPool.plane10Flip;
            Mesh currentMesh = flip ? meshFlipCache : meshCache;
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
            var bladeMatrix1 = Matrix4x4.TRS(bladeDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore1AndBlade1);
            var bladeMatrix2 = Matrix4x4.TRS(bladeDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore2AndBlade2);
            var coreMatrix1 = Matrix4x4.TRS(coreDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore1AndBlade1);
            var coreMatrix2 = Matrix4x4.TRS(coreDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore2AndBlade2);
            var finalBladeMatrix1 = bladeMatrix1 * scaleMatrix;
            var finalBladeMatrix2 = bladeMatrix2 * scaleMatrix2;
            MaterialPropertyBlock mpb = compLightsaberBlade.materialPropertyBlock;
            if (graphic1 != null)
            {
                Graphics.DrawMesh(currentMesh, finalBladeMatrix1, graphic1.MatSingle, 0, null, 0, mpb);
            }
            if (core1Graphic != null)
            {
                Graphics.DrawMesh(currentMesh, coreMatrix1, core1Graphic.MatSingle, 0, null, 0, mpb);
            }
            if (blade2Graphic != null)
            {
                Graphics.DrawMesh(currentMesh, finalBladeMatrix2, blade2Graphic.MatSingle, 0, null, 0, mpb);
            }
            if (core2Graphic != null)
            {
                Graphics.DrawMesh(currentMesh, coreMatrix2, core2Graphic.MatSingle, 0, null, 0, mpb);
            }
            if (glowGraphic != null && Force_ModSettings.LightsaberFakeGlow)
            {
                if (cachedGlowRadius != Force_ModSettings.glowRadius || glowDrawLoc != cachedGlowPosition)
                {
                    var glowScale = new Vector3(Force_ModSettings.glowRadius, 1f, Force_ModSettings.glowRadius) * 1.3f;
                    cachedGlowMatrix = Matrix4x4.TRS(glowDrawLoc, rotationCache, glowScale);
                    cachedGlowRadius = Force_ModSettings.glowRadius;
                    cachedGlowPosition = glowDrawLoc; // Cache the glow position to avoid recalculating unnecessarily
                }

                // Draw the glow
                Graphics.DrawMesh(currentMesh, cachedGlowMatrix, glowGraphic.MatSingle, 0, null, 0, mpb);
            }

            // Handling hilt graphics
            if (compLightsaberBlade.selectedhiltgraphic.graphicData != null)
            {
                compLightsaberBlade.UpdateHiltGraphics();
                var hiltMaterial = compLightsaberBlade.selectedhiltgraphic.graphicData.Graphic.MatSingle;
                var hiltMatrix = Matrix4x4.TRS(hiltDrawLoc, rotationCache, new Vector3(compLightsaberBlade.HiltGraphic.drawSize.x, 1f, compLightsaberBlade.HiltGraphic.drawSize.x));

                Graphics.DrawMesh(currentMesh, hiltMatrix, hiltMaterial, 0, null, 0, compLightsaberBlade.hiltMaterialPropertyBlock);
            }
        }

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


    [StaticConstructorOnStartup]
    public static class LightsaberGlowShaderLoader
    {
        static LightsaberGlowShaderLoader()
        {
            LoadShader();
        }

        private static void LoadShader()
        {
            // Find the current mod folder dynamically
            ModContentPack modPack = LoadedModManager.RunningModsListForReading
                .FirstOrDefault(mod => mod.PackageIdPlayerFacing.Contains("lee.theforce.psycast"));

            if (modPack == null)
            {
                Log.Error("LightsaberGlowShaderLoader: Could not find the mod folder.");
                return;
            }

            // Corrected path: Ensure it includes the full filename with .assetbundle extension
            string bundlePath = Path.Combine(modPack.RootDir, "AssetBundle", "lightsabershaderglow.assetbundle");
            if (!File.Exists(bundlePath))
            {
                Log.Error($"LightsaberGlowShaderLoader: Asset bundle not found at {bundlePath}");
                return;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                Log.Error("LightsaberGlowShaderLoader: Failed to load asset bundle.");
                return;
            }

            Shader customShader = bundle.LoadAsset<Shader>("LightsaberGlowShader");
            if (customShader == null)
            {
                Log.Error("LightsaberGlowShaderLoader: Shader not found in bundle.");
                return;
            }

            // Assign the shader to ShaderTypeDef manually
            ShaderTypeDef shaderDef = DefDatabase<ShaderTypeDef>.GetNamedSilentFail("Force_LightsaberGlow");
            if (shaderDef != null)
            {
                typeof(ShaderTypeDef).GetField("shaderInt", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(shaderDef, customShader);
            }
            else
            {
                Log.Error("LightsaberGlowShaderLoader: ShaderTypeDef 'LightsaberGlow' not found.");
            }
        }
    }


    public class CompCache
    {
        private readonly Dictionary<Thing, Comp_LightsaberBlade> cachedComps = new Dictionary<Thing, Comp_LightsaberBlade>();
        public Comp_LightsaberBlade GetCachedComp(Thing thing)
        {
            if (thing == null)
                return null;
            if (cachedComps.TryGetValue(thing, out var cachedComp))
            {
                return cachedComp;
            }
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


