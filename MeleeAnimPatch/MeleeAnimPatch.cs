using AM;
using AM.Reqs;
using AM.Sweep;
using AM.Tweaks;
using LudeonTK;
using RimWorld;
using System;
using TheForce_Psycast;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;

namespace MeleeAnimPatch_ForcePsycast
{
    internal class MeleeAnimPatch : PartRenderer
    {
        private Comp_LightsaberBlade compLightsaberBlade;

        private const float HiltYOffset = 0f;
        private const float GlowYOffset = -1f;
        private const float CoreYOffset = -0.001f;
        private const float BladeYOffset = -0.003f;

        private static readonly Vector3 coreOffset = new Vector3(0f, CoreYOffset, 0f);
        private static readonly Vector3 bladeOffset = new Vector3(0f, BladeYOffset, 0f);
        private static readonly Vector3 hiltOffset = new Vector3(0f, HiltYOffset, 0f);
        private static readonly Vector3 glowOffset = new Vector3(0f, GlowYOffset, 0f);

        private static Quaternion rotationCache;
        private static float? previousAngle;

        public override bool Draw()
        {
            // Draw lightsaber base
            var weapon = Item;
            if (weapon == null)
                return true;

            // Fetch the lightsaber component once
            compLightsaberBlade = weapon.GetComp<Comp_LightsaberBlade>();
            if (compLightsaberBlade == null)
                return true;

            compLightsaberBlade.UpdateScalingAndOffset();
            Vector3 coreDrawLoc = weapon.DrawPos + coreOffset;
            Vector3 bladeDrawLoc = weapon.DrawPos + bladeOffset;
            Vector3 hiltDrawLoc = weapon.DrawPos + hiltOffset;
            Vector3 glowDrawLoc = weapon.DrawPos + glowOffset;

            // Retrieve material references for the lightsaber parts
            var bladeMat = compLightsaberBlade.Graphic?.MatSingle;
            var coreMat = compLightsaberBlade.LightsaberCore1Graphic?.MatSingle;
            var blade2Mat = compLightsaberBlade.LightsaberBlade2Graphic?.MatSingle;
            var core2Mat = compLightsaberBlade.LightsaberCore2Graphic?.MatSingle;
            var hiltMat = compLightsaberBlade.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle;
            var glowMat = compLightsaberBlade.LightsaberGlowGraphic?.MatSingle;

            if (bladeMat == null)
                return true;

            // Use the lenFactor from LightsaberGraphicsUtil
            float lenFactor = Rand.Range(compLightsaberBlade.minVibrate, compLightsaberBlade.minVibrate + compLightsaberBlade.vibrationrate);
            Vector3 scale = new Vector3(lenFactor, 1, 1);
            var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);


            float lenFactor2 = Rand.Range(compLightsaberBlade.minVibrate, compLightsaberBlade.minVibrate + compLightsaberBlade.vibrationrate2);
            Vector3 scale2 = new Vector3(lenFactor2, 1, 1);
            var scaleMatrix2 = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale2);


            var glowMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Force_ModSettings.glowRadius * scale);

            // Slightly offset the blade and core matrices
            var bladeMatrix1 = Matrix4x4.TRS(bladeDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore1AndBlade1);
            var bladeMatrix2 = Matrix4x4.TRS(bladeDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore2AndBlade2);
            var coreMatrix1 = Matrix4x4.TRS(coreDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore1AndBlade1);
            var coreMatrix2 = Matrix4x4.TRS(coreDrawLoc, rotationCache, compLightsaberBlade.currentScaleForCore2AndBlade2);


            Matrix4x4 TRSBlade = TRS;
            TRSBlade.m13 -= 0.002f;  // Adjust this as needed to avoid z-fighting
            Matrix4x4 TRSCore = TRS;
            TRSCore.m13 -= 0.001f;
            Matrix4x4 TRSHilt = TRS;
            TRSHilt.m13 -= 0.00f;

            // Draw each part conditionally
            if (bladeMat != null)
                Graphics.DrawMesh(Mesh, TRSBlade * scaleMatrix, bladeMat, 0);

            if (coreMat != null)
                Graphics.DrawMesh(Mesh, TRSCore * scaleMatrix, coreMat, 0);

            if (blade2Mat != null)
                Graphics.DrawMesh(Mesh, TRSBlade * scaleMatrix2, blade2Mat, 0);

            if (core2Mat != null)
                Graphics.DrawMesh(Mesh, TRSCore * scaleMatrix2, core2Mat, 0);

            if (hiltMat != null)
                Graphics.DrawMesh(Mesh, TRS, hiltMat, 0, null, 0, compLightsaberBlade.hiltMaterialPropertyBlock);

            // Draw glow if enabled in settings
            if (glowMat != null && Force_ModSettings.LightsaberFakeGlow)
            {
                Graphics.DrawMesh(Mesh, TRSCore * glowMatrix, glowMat, 0);
            }

            return true;
        }
    }

        public static class Extensions
        {
            public static Color? TryGetLightsaberColor(this Thing lightsaber)
            {
                var comp = lightsaber.TryGetComp<Comp_LightsaberBlade>();
                if (comp == null)
                    return null;

                // Assuming you want the color of lightsaber blade 1
                Color color = comp.lightsaberBlade1OverrideColor;

                // Check if the color is default (usually Color.white), return null if so
                

                return color;
            }
        }

        public class LightsaberSweepProvider : ISweepProvider
        {
            public const float length = 0.15f;
            public const float minVel = 1f;
            public const float maxVel = 2f;

            public LightsaberSweepProvider() { }

            public (Color low, Color high) GetTrailColors(in SweepProviderArgs args)
            {
                var saber = args.Renderer.GetOverride(args.Part)?.Weapon;
                if (saber == null)
                {
                    return (Color.green, default);
                }

                var color = saber.TryGetLightsaberColor() ?? default;
                if (color == default)
                {
                    return (default, default);
                }

                float timeSinceHere = args.LastTime - args.Time;
                if (timeSinceHere > length)
                    return (default, default);

                float sa = Mathf.InverseLerp(minVel, maxVel, args.DownVel);
                float sb = Mathf.InverseLerp(minVel, maxVel, args.UpVel);

                float a = Mathf.Clamp01(1f - timeSinceHere / length);
                var low = color;
                var high = color;
                low.a = a * sa;
                high.a = a * sb;

                return (low, high);
            }
        }

        public class AnyLightsaberType : Req
        {
            public MeleeWeaponTypeLightsaber types;

            public override bool Evaluate(ReqInput input)
            {
                uint result = (uint)input.TypeFlags & (uint)types;
                return result != 0;
            }
        }

        [Flags]
        public enum MeleeWeaponTypeLightsaber : uint
        {
            Long_Blunt = 1,
            Long_Sharp = 2,
            Long_Stab = 4,
            Short_Blunt = 8,
            Short_Sharp = 16,
            Short_Stab = 32,
            Lightsaber = 64
        }
}

