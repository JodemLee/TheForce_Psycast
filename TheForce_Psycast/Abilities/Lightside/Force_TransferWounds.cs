using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Lightside
{
    internal class Force_TransferWounds : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            // Check if the caster is a valid pawn and there is at least one target
            if (targets.Length < 1 || !(targets[0].Thing is Pawn target) || !(this.pawn is Pawn caster))
            {
                Log.Error("Invalid targets for Force_TransferWounds.");
                return;
            }

            TransferWounds(target, caster); // Transfer from target to caster
        }

        private void TransferWounds(Pawn source, Pawn dest)
        {
            // Get the wounds (injuries) from the source (target) pawn
            List<Hediff_Injury> sourceInjuries = GetWoundsToTransfer(source);

            // Remove wounds from source
            foreach (var injury in sourceInjuries)
            {
                source.health.RemoveHediff(injury);
            }

            // Transfer wounds to the destination (caster)
            AddWounds(dest, sourceInjuries);
        }

        private List<Hediff_Injury> GetWoundsToTransfer(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(hediff => hediff.CanHealNaturally() && hediff.Visible && !hediff.IsPermanent())
                .ToList();
        }

        private void AddWounds(Pawn pawn, List<Hediff_Injury> injuries)
        {
            foreach (var injury in injuries)
            {
                try
                {
                    pawn.health.AddHediff(injury);
                }
                catch (Exception e)
                {
                    Log.Error($"Error while adding hediff to {pawn.Label}: {e}");
                }
            }
        }
    }
}