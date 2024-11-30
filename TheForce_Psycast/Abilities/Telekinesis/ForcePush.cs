using RimWorld;
using RimWorld.Planet;
using Verse;

namespace TheForce_Psycast
{
    public class Ability_Forcepush : VFECore.Abilities.Ability
    {
        // Add a variable to store the base damage amount
        public float baseDamage = 1f;
        Force_ModSettings modSettings = new Force_ModSettings();
        public bool usePsycastStat = false;
        public int offsetMultiplier { get; set; }

        public Ability_Forcepush()
        {
            modSettings = new Force_ModSettings(); // Instantiate Force_ModSettings
        }

        public int GetOffsetMultiplier()
        {

            if (Force_ModSettings.usePsycastStat == true)
            {
                offsetMultiplier = (int)(offsetMultiplier * pawn.GetStatValue(StatDefOf.PsychicSensitivity));
                return offsetMultiplier;
            }
            else
            {
                offsetMultiplier = (int)Force_ModSettings.offSetMultiplier;

            }
            return offsetMultiplier;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            int offsetMultiplier = GetOffsetMultiplier();

            foreach (GlobalTargetInfo target in targets)
            {
                // Calculate the offset between caster and target
                IntVec3 offset = target.Cell - this.pawn.Position;
                if (offset.LengthHorizontal < 3)
                {
                    offset = offset * 3;
                }
                offset *= offsetMultiplier;
                IntVec3 initialPushBackPosition = target.Cell + offset;
                IntVec3 pushBackPosition = initialPushBackPosition;
                Map map = Caster.Map;
                IntVec3 lastValidCell = this.pawn.Position;
                foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(this.pawn.Position, initialPushBackPosition))
                {
                    if (!cell.InBounds(map) || cell.GetRoofHolderOrImpassable(map) is Building)
                    {
                        pushBackPosition = lastValidCell;
                        break;
                    }
                    lastValidCell = cell;
                }
                if (!PositionUtils.CheckValidPosition(pushBackPosition, map))
                {
                    pushBackPosition = PositionUtils.FindValidPosition(target.Cell, offset, map);
                }
                float distancePushed = (pushBackPosition - target.Cell).LengthHorizontal;
                float scaledDamage = baseDamage + (distancePushed * 2f);
                DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, (int)scaledDamage, 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                target.Thing.TakeDamage(damageInfo);
                if (target.Thing is Pawn pawn)
                {
                    var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawnPush, pawn, pushBackPosition, null, null);
                    GenSpawn.Spawn(flyer, pushBackPosition, map);
                }
                else
                {
                    target.Thing.Position = pushBackPosition;
                }
            }

            base.Cast(targets);
        }
    }
}



