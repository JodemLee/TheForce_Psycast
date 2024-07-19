using AM;
using AM.Sweep;
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
            lightsaberComp = Item?.GetComp<Comp_LightsaberBlade>();

            if (lightsaberComp == null)
                return true;
            Graphics.DrawMesh(Mesh, TRS, Material, (int)-.001f);

            // Draw the main blade
            DrawLightsaberBlade(lightsaberComp.Graphic);

            // Draw the cores if they exist
            if (lightsaberComp.LightsaberCore1Graphic != null)
            {
                DrawLightsaberCore(lightsaberComp.LightsaberCore1Graphic);
            }
            if (lightsaberComp.LightsaberBlade2Graphic != null)
            {
                DrawLightsaberBlade(lightsaberComp.LightsaberBlade2Graphic);
            }
            if (lightsaberComp.LightsaberCore2Graphic != null)
            {
                DrawLightsaberCore(lightsaberComp.LightsaberCore2Graphic);
            }

            return true;
        }

        private void DrawLightsaberBlade(Graphic graphic)
        {
            if (graphic == null)
                return;

            Material material = graphic.MatSingle;

            float bladeYOffset = 0.002f; // Adjust this value as needed

            // Create the TRSBlade matrix with the y offset
            Matrix4x4 TRSBlade = TRS;
            TRSBlade.m23 -= bladeYOffset; // Adjusting the translation component in the y-axis

            float lenFactor = Rand.Range(0.99f, 1.01f);
            Vector3 scale = new Vector3(lenFactor, 1, 1);
            Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

            Graphics.DrawMesh(Mesh, TRSBlade * scaleMatrix, material, 0);
        }

        private void DrawLightsaberCore(Graphic graphic)
        {
            if (graphic == null)
                return;

            Material material = graphic.MatSingle;

            float CoreYOffset = 0.001f; // Adjust this value as needed

            // Create the TRSBlade matrix with the y offset
            Matrix4x4 TRSCore = TRS;
            TRSCore.m23 -= CoreYOffset; // Adjusting the translation component in the y-axis

            float lenFactor = Rand.Range(0.99f, 1.01f);
            Vector3 scale = new Vector3(lenFactor, 1, 1);
            Matrix4x4 scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

            Graphics.DrawMesh(Mesh, TRSCore * scaleMatrix, material, 0);
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
