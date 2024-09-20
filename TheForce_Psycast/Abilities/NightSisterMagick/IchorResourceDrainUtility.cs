using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using TheForce_Psycast.NightSisterMagick;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.NightSisterMagick
{
    public class IchorResourceDrainUtility
    {
        public static void TickResourceDrain(IGeneResourceDrain drain)
        {
            if (drain.CanOffset && drain.Resource != null)
            {
                OffsetResource(drain, (0f - drain.ResourceLossPerDay) / 60000f);
            }
        }

        public static void PostResourceOffset(IGeneResourceDrain drain, float oldValue)
        {
            if (oldValue > 0f && drain.Resource.Value <= 0f)
            {
                Pawn pawn = drain.Pawn;
            }
        }

        public static void OffsetResource(IGeneResourceDrain drain, float amnt)
        {
            float value = drain.Resource.Value;
            drain.Resource.Value += amnt;
            PostResourceOffset(drain, value);
        }

        public static IEnumerable<Gizmo> GetResourceDrainGizmos(IGeneResourceDrain drain)
        {
            if (DebugSettings.ShowDevGizmos && drain.Resource != null)
            {
                Gene_Resource resource = drain.Resource;
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: " + resource.ResourceLabel + " -10";
                command_Action.action = delegate
                {
                    OffsetResource(drain, -0.1f);
                };
                yield return command_Action;
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "DEV: " + resource.ResourceLabel + " +10";
                command_Action2.action = delegate
                {
                    OffsetResource(drain, 0.1f);
                };
                yield return command_Action2;
            }
        }
    }

 
    internal class AbilityExtensionHemogenDrain : AbilityExtension_AbilityMod
    {
        public float HemogenDrainAmount = 0f;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);

            // Check if the ability pawn exists and has the necessary components
            if (ability.pawn != null && ability.pawn.genes != null)
            {
                // Check if the pawn has the required hemogen gene
                Gene_Hemogen geneHemogen = ability.pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>(); ;
                if (geneHemogen != null)
                {
                    // Calculate the drain amount based on the HemogenDrainAmount parameter
                    float drainAmount = HemogenDrainAmount;

                    // Apply the hemogen drain
                    Force_GeneMagick.OffsetIchor(ability.pawn, drainAmount);
                }
            }
        }
    }

}

