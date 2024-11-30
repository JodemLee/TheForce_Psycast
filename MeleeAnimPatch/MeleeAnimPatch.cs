using AM;
using AM.Reqs;
using AM.Sweep;
using AM.Tweaks;
using LudeonTK;
using System;
using TheForce_Psycast;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;

namespace MeleeAnimPatch_ForcePsycast
{

        internal class MeleeAnimPatch : PartRenderer
        {
            private Comp_LightsaberBlade lightsaberComp;

            [TweakValue("aaa_LightsaberBladeMinJitter", 0, 2f)]
            private static float minVibrate = 0.999f;

            [TweakValue("aaa_LightsaberBladeMaxJitter", 0, 2f)]
            private static float maxVibrate = 1.005f;
            public override bool Draw()
            {
                // Draw lightsaber base


                var weapon = Item;
                if (weapon == null)
                    return true;

                // Fetch the lightsaber component once
                lightsaberComp = weapon.GetComp<Comp_LightsaberBlade>();
                if (lightsaberComp == null)
                    return true;

                // Retrieve material references for the lightsaber parts
                var bladeMat = lightsaberComp.Graphic?.MatSingle;
                var coreMat = lightsaberComp.LightsaberCore1Graphic?.MatSingle;
                var blade2Mat = lightsaberComp.LightsaberBlade2Graphic?.MatSingle;
                var core2Mat = lightsaberComp.LightsaberCore2Graphic?.MatSingle;
                var hiltMat = lightsaberComp.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle;
                var glowMat = lightsaberComp.LightsaberGlowGraphic?.MatSingle;

                if (bladeMat == null)
                    return true;

                float lenFactor = Rand.Range(0.999f, 1.005f);
                Vector3 scale = new Vector3(lenFactor, 1, 1);
                var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
                var glowMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Force_ModSettings.glowRadius * scale);

                // Slightly offset the blade and core matrices
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
                    Graphics.DrawMesh(Mesh, TRSBlade * scaleMatrix, blade2Mat, 0);

                if (core2Mat != null)
                    Graphics.DrawMesh(Mesh, TRSCore * scaleMatrix, core2Mat, 0);
                if (hiltMat != null)
                    Graphics.DrawMesh(Mesh, TRS, lightsaberComp.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle, 0, null, 0, lightsaberComp.hiltMaterialPropertyBlock);

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
                if (color == Color.white)
                    return null;

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

