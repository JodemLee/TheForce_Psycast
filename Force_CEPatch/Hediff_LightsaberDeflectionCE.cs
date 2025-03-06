using CombatExtended;
using RimWorld;
using TheForce_Psycast;
using TheForce_Psycast.Lightsabers;
using Verse;

namespace Force_CEPatch
{
    [StaticConstructorOnStartup]
    public class Hediff_LightsaberDeflectionCE : Hediff_LightsaberDeflection
    {

        private int lastInterceptTicks = -999999;
        private float lastInterceptAngle;
        private bool drawInterceptCone;
        public float entropyGain { get; set; }
        public float deflectionMultiplier { get; set; }

        public void AddEntropy(ProjectileCE projectile)
        {
            // Calculate entropy gain based on projectile speed
            float EntropyGain = CalculateEntropyGain(projectile);

            // Add entropy to the pawn's psychicEntropy hediff
            this.pawn.psychicEntropy.TryAddEntropy(EntropyGain, overLimit: false);
        }

        public float CalculateEntropyGain(ProjectileCE projectile)
        {
            entropyGain = Force_ModSettings.entropyGain;
            float EntropyGain = projectile.def.projectile.speed / entropyGain;
            return EntropyGain;
        }


        public virtual bool ShouldDeflectProjectile(ProjectileCE projectile)
        {
            float deflectionMultiplier = Force_ModSettings.deflectionMultiplier;

            if (projectile.launcher == null || projectile.launcher.Faction == null || projectile.launcher.Faction == pawn.Faction)
            {
                return false;
            }

            if (this.pawn.psychicEntropy.EntropyValue >= this.pawn.psychicEntropy.MaxEntropy)
            {
                return false;
            }

            // Calculate the potential entropy gain
            float potentialEntropyGain = CalculateEntropyGain(projectile);
            float newEntropyValue = this.pawn.psychicEntropy.EntropyValue + potentialEntropyGain;

            // Check if the new entropy value exceeds 90% of the maximum entropy
            if (newEntropyValue >= (this.pawn.psychicEntropy.MaxEntropy * 0.99f))
            {
                ;
                return false;
            }

            float deflectionSkillChance = pawn.GetStatValue(ForceDefOf.Force_Lightsaber_Deflection);
            float deflection = deflectionSkillChance * deflectionMultiplier;
            float randomValue = Rand.Range(0f, 1.0f);

            if (deflection >= randomValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastInterceptTicks, "lastInterceptTicks");
            Scribe_Values.Look(ref lastInterceptAngle, "lastInterceptTicks");
            Scribe_Values.Look(ref drawInterceptCone, "drawInterceptCone");
        }
    }
}