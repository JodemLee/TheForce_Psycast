using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.Sound;
using Verse;
using Verse.Noise;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    [StaticConstructorOnStartup]
    internal class SithLightningEvent : WeatherEvent_LightningFlash
    {
        private IntVec3 strikeLoc = IntVec3.Invalid;
        private Mesh boltMesh;
        private Color lightningColor = Color.red;


        private static readonly Material SithMat;

        static SithLightningEvent()
        {
            Texture2D sithLightningTexture = ContentFinder<Texture2D>.Get("Weather/SithLightning", true);
            if (sithLightningTexture != null)
            {
                // Create a new material manually using the "Mote" shader
                SithMat = new Material(ShaderDatabase.MoteGlowDistorted)
                {
                    mainTexture = sithLightningTexture,
                    color = Color.red  // Set default color (can be changed later)
                };
            }
            else
            {
                Log.Error("Failed to load SithLightning texture!");
            }
        }


        public SithLightningEvent(Map map, Color color)
            : base(map)
        {
            this.lightningColor = color;
        }

        public SithLightningEvent(Map map, IntVec3 forcedStrikeLoc, Color color)
            : base(map)
        {
            this.strikeLoc = forcedStrikeLoc;
            this.lightningColor = color;
        }

        public override void FireEvent()
        {
            DoStrike(strikeLoc, map, ref boltMesh, lightningColor);
        }

        public static void DoStrike(IntVec3 strikeLoc, Map map, ref Mesh boltMesh, Color lightningColor)
        {
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
            if (!strikeLoc.IsValid)
            {
                strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
            }
            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (!strikeLoc.Fogged(map))
            {
                GenExplosion.DoExplosion(strikeLoc, map, 1.9f, ForceDefOf.Force_Lightning, null);
                Vector3 loc = strikeLoc.ToVector3Shifted();
                for (int i = 0; i < 4; i++)
                {
                    FleckMaker.ThrowSmoke(loc, map, 1.5f);
                    FleckMaker.ThrowMicroSparks(loc, map);
                    FleckMaker.ThrowLightningGlow(loc, map, 1.5f);
                }
            }
            SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map));
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
        }

        public override void WeatherEventDraw()
        {
            // Use MaterialPropertyBlock to apply a color dynamically
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetColor("_Color", lightningColor);  // Set the color of the lightning

            Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity,
                              FadedMaterialPool.FadedVersionOf(SithMat, LightningBrightness), 0, null, 0, mpb);  // Apply the color block

        }
    }
}
   
