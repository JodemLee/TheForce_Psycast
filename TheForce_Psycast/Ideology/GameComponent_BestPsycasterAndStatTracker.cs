using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Ideology
{
    internal class GameComponent_BestPsycasterAndStatTracker : GameComponent
    {
        public int tickCounter = 0;
        public int tickInterval = 4000;

        public GameComponent_BestPsycasterAndStatTracker(Game game) : base()
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void GameComponentTick()
        {
            if (ModsConfig.IdeologyActive)
            {
                if (Find.IdeoManager.classicMode) return;

                tickCounter++;
                if (tickCounter > tickInterval)
                {

                    {
                        Ideo ideo = Current.Game.World.factionManager.OfPlayer.ideos.PrimaryIdeo;

                        // Check for highest psycast level
                        if (ideo?.HasPrecept(DefDatabase<PreceptDef>.GetNamedSilentFail("Force_Leader_HighestPsycaster")) == true)
                        {
                            Pawn bestPsycaster = null;
                            int highestPsycastLevel = 0;

                            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
                            {
                                int psycastLevel = PawnUtility.GetPsylinkLevel(pawn);
                                if (psycastLevel > highestPsycastLevel && pawn.ideo?.Ideo == ideo && !pawn.IsSlave)
                                {
                                    highestPsycastLevel = psycastLevel;
                                    bestPsycaster = pawn;
                                }
                            }

                            Precept_Role precept_role = bestPsycaster?.Ideo?.GetPrecept(PreceptDefOf.IdeoRole_Leader) as Precept_Role;

                            if (precept_role?.ChosenPawnSingle() != bestPsycaster)
                            {
                                if (precept_role.RequirementsMet(bestPsycaster))
                                {
                                    precept_role.Unassign(precept_role.ChosenPawnSingle(), false);
                                    precept_role.Assign(bestPsycaster, true);
                                }
                            }
                        }

                        // Check for highest stat value
                        if (ideo?.HasPrecept(DefDatabase<PreceptDef>.GetNamedSilentFail("Force_Leader_HighestDarkside")) == true)
                        {
                            Pawn bestStatPawn = null;
                            float highestStatValue = 0f;

                            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
                            {
                                if (pawn.ideo?.Ideo == ideo && !pawn.IsSlave)
                                {
                                    // Replace "StatDefOf.ShootingAccuracyPawn" with your desired stat
                                    float statValue = pawn.GetStatValue(ForceDefOf.Force_Darkside_Attunement, true);
                                    if (statValue > highestStatValue)
                                    {
                                        highestStatValue = statValue;
                                        bestStatPawn = pawn;
                                    }
                                }
                            }

                            Precept_Role precept_role = bestStatPawn?.Ideo?.GetPrecept(PreceptDefOf.IdeoRole_Leader) as Precept_Role;

                            if (precept_role?.ChosenPawnSingle() != bestStatPawn)
                            {
                                if (precept_role.RequirementsMet(bestStatPawn))
                                {
                                    precept_role.Unassign(precept_role.ChosenPawnSingle(), false);
                                    precept_role.Assign(bestStatPawn, true);
                                }
                            }
                        }

                        if (ideo?.HasPrecept(DefDatabase<PreceptDef>.GetNamedSilentFail("Force_Leader_HighestLightside")) == true)
                        {
                            Pawn bestStatPawn = null;
                            float highestStatValue = 0f;

                            foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
                            {
                                if (pawn.ideo?.Ideo == ideo && !pawn.IsSlave)
                                {
                                    // Replace "StatDefOf.ShootingAccuracyPawn" with your desired stat
                                    float statValue = pawn.GetStatValue(ForceDefOf.Force_Lightside_Attunement, true);
                                    if (statValue > highestStatValue)
                                    {
                                        highestStatValue = statValue;
                                        bestStatPawn = pawn;
                                    }
                                }
                            }

                            Precept_Role precept_role = bestStatPawn?.Ideo?.GetPrecept(PreceptDefOf.IdeoRole_Leader) as Precept_Role;

                            if (precept_role?.ChosenPawnSingle() != bestStatPawn)
                            {
                                if (precept_role.RequirementsMet(bestStatPawn))
                                {
                                    precept_role.Unassign(precept_role.ChosenPawnSingle(), false);
                                    precept_role.Assign(bestStatPawn, true);
                                }
                            }
                        }
                    }

                    tickCounter = 0;
                }
            }
        }
    }
}