using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;

namespace TheForce_Psycast
{
    public class Hediff_ForceDefense : Hediff_DefensiveStance
    {
        private Sustainer sustainer;
        public override Color OverlayColor => new ColorInt(255, 255, 255).ToColor;
        protected override void DeflectProjectile(Projectile projectile)
        {
            base.DeflectProjectile(projectile);
            this.AddEntropy();
            
        }

        public override void PostTick()
        {
            base.PostTick();
            this.AddEntropy();
            if (this.sustainer == null || this.sustainer.Ended)
                this.sustainer = VPE_DefOf.VPE_GuardianSkipbarrier_Sustainer.TrySpawnSustainer(SoundInfo.InMap(this.pawn, MaintenanceType.PerTick));
            this.sustainer.Maintain();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!this.sustainer.Ended) this.sustainer?.End();
        }

        private void AddEntropy()
        {
            if (Find.TickManager.TicksGame % 15  == 0) this.pawn.psychicEntropy.TryAddEntropy(1f, overLimit: true);
            if (this.pawn.psychicEntropy.EntropyValue >= this.pawn.psychicEntropy.MaxEntropy) this.pawn.health.RemoveHediff(this);
        }
    }
}
