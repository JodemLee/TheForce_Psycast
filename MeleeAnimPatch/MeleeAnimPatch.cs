using AM;
using AM.Sweep;
using TheForce_Psycast;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;

namespace MeleeAnimPatch_ForcePsycast
{
    internal class MeleeAnimPatch : PartRenderer
    {
        private Comp_LightsaberBlade lightsaberComp;
        public override bool Draw()
        {
            // Draw lightsaber base.
            Graphics.DrawMesh(Mesh, TRS, Material, 0);

            var weapon = Item;

            if (weapon == null)
                return true;

            var lightsaberComp = weapon.GetComp<Comp_LightsaberBlade>();
            if (lightsaberComp == null)
                return true;

            var bladeMat = weapon.GetComp<Comp_LightsaberBlade>()?.Graphic?.MatSingle;
            var coreMat = weapon.GetComp<Comp_LightsaberBlade>()?.LightsaberCore1Graphic?.MatSingle;
            var blade2Mat = weapon.GetComp<Comp_LightsaberBlade>()?.LightsaberBlade2Graphic?.MatSingle;
            var core2Mat = weapon.GetComp<Comp_LightsaberBlade>()?.LightsaberCore2Graphic?.MatSingle;
            var glowMat = weapon.GetComp<Comp_LightsaberBlade>()?.LightsaberGlowGraphic?.MatSingle;

            if (bladeMat == null)
                return true;

            float lenFactor = Rand.Range(0.99f, 1.01f);
            Vector3 scale = new Vector3(lenFactor, 1, 1);
            var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
            var glowMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Force_ModSettings.glowRadius*scale);

            Matrix4x4 TRSBlade = TRS;
            TRSBlade.m13 -= 0.002f;
            Matrix4x4 TRSCore = TRS;
            TRSCore.m13 -= 0.001f;

            if (lightsaberComp.Graphic != null)
            {
                Graphics.DrawMesh(Mesh, TRSBlade, bladeMat, 0);
            }

            if (lightsaberComp.LightsaberCore1Graphic != null)
            {
                Graphics.DrawMesh(Mesh, TRSCore, coreMat, 0);
            }

            if (lightsaberComp.LightsaberBlade2Graphic != null)
            {
                Graphics.DrawMesh(Mesh, TRSBlade, blade2Mat, 0);
            }

            if (lightsaberComp.LightsaberCore2Graphic != null)
            {
             Graphics.DrawMesh(Mesh, TRSCore, core2Mat, 0);
            }

            if (lightsaberComp.LightsaberGlowGraphic != null && Force_ModSettings.LightsaberFakeGlow)
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
                Core.Error($"Tried to use {nameof(LightsaberSweepProvider)} with a Thing that does not have a {nameof(Comp_LightsaberBlade)}! ({saber})");
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
}
