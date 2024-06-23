using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse.Sound;
using Verse;
using VanillaPsycastsExpanded.Graphics;
using RimWorld.Planet;
using VFECore.Abilities;
using RimWorld;
using VFECore;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Darkside
{

    public class Ability_ForceLightning : Ability_ShootProjectile
    {
        protected override Projectile ShootProjectile(GlobalTargetInfo target)
        {
            var projectile = base.ShootProjectile(target) as ForceLightningProjectile;
            projectile.ability = this;
            return projectile;
        }
    }

    public class ForceLightningProjectile : ExpandableProjectile
    {
        public Ability ability;
        public override void DoDamage(IntVec3 pos)
        {
            base.DoDamage(pos);
            try
            {
                if (pos != this.launcher.Position && this.launcher.Map != null && GenGrid.InBounds(pos, this.launcher.Map))
                {
                    var list = this.launcher.Map.thingGrid.ThingsListAt(pos);
                    for (int num = list.Count - 1; num >= 0; num--)
                    {
                        if (IsDamagable(list[num]))
                        {
                            this.customImpact = true;
                            base.Impact(list[num]);
                            this.customImpact = false;
                            if (list[num] is Pawn pawn)
                            {
                                var severityImpact = (0.5f / pawn.Position.DistanceTo(launcher.Position));
                                if (ability.def.goodwillImpact != 0)
                                {
                                    ability.ApplyGoodwillImpact(pawn);
                                }
                            }
                        }
                    }
                }
            }
            catch { };
        }

        public override bool IsDamagable(Thing t)
        {
            return t is Pawn && base.IsDamagable(t) || t.def == ThingDefOf.Fire;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ability, "ability");
        }
    }
}


