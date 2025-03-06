using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace TheForce_Psycast
{
    public static class PositionUtils
    {
        // Checks if a position is valid (walkable and in bounds)
        public static bool CheckValidPosition(IntVec3 position, Map map)
        {
            return position.InBounds(map) && position.Standable(map);
        }

        // Finds a valid position close to the original position using the offset
        public static IntVec3 FindValidPosition(IntVec3 originalPosition, IntVec3 offset, Map map)
        {
            IntVec3 closestValidPosition = originalPosition;
            float closestDistanceSquared = float.MaxValue;

            for (int i = 1; i <= 3; i++)
            {
                IntVec3 newPosition = originalPosition + (offset * i);
                if (newPosition.InBounds(map) && newPosition.Standable(map))
                {
                    // Calculate distance to original position
                    float distanceSquared = newPosition.DistanceToSquared(originalPosition);
                    if (distanceSquared < closestDistanceSquared)
                    {
                        closestValidPosition = newPosition;
                        closestDistanceSquared = distanceSquared;
                    }
                }
            }
            return closestValidPosition;
        }

        public static bool IsCarryingWeaponOpenly(this Pawn pawn)
        {
            if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
            {
                return false;
            }
            if (pawn.Drafted)
            {
                return true;
            }
            if ((pawn.CurJob?.def?.alwaysShowWeapon).GetValueOrDefault())
            {
                return true;
            }
            if ((pawn.mindState?.duty?.def?.alwaysShowWeapon).GetValueOrDefault())
            {
                return true;
            }
            Lord lord = pawn.GetLord();
            if (lord != null && lord.LordJob != null && lord.LordJob.AlwaysShowWeapon)
            {
                return true;
            }
            return false;
        }

        public static string AsCompassDirection(this Rot4 rotation)
        {
            return rotation == Rot4.South ? "south" :
                   rotation == Rot4.North ? "north" :
                   rotation == Rot4.East ? "east" : "west";
        }
    }

    public static class ProjectileUtility
    {
        // Method to get all projectile ThingDefs from the DefDatabase
        public static List<ThingDef> GetAllProjectiles()
        {
            List<ThingDef> allProjectiles = new List<ThingDef>();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                // Check if this ThingDef has a projectile component
                if (def.projectile != null)
                {
                    allProjectiles.Add(def);
                }
            }

            return allProjectiles;
        }
    }

    public static class StuffColorUtility
    {
        private static System.Random random = new System.Random();
        public static List<ThingDef> GetAllStuffed()
        {
            List<ThingDef> allStuffedDefs = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.stuffProps != null)
                {
                    allStuffedDefs.Add(def);
                }
            }
            
            return allStuffedDefs;
        }
        public static Color GetStuffColor(ThingDef thingDef)
        {
            if (thingDef.stuffProps != null && thingDef.stuffProps.color != null)
            {
                return thingDef.stuffProps.color;
            }
            if (thingDef.graphicData != null && thingDef.graphicData.color != null)
            {
                return thingDef.graphicData.color;
            }
            return Color.white;
        }

        public static Color GetRandomColorFromStuffCategories(List<StuffCategoryDef> categories)
        {
            List<ThingDef> allStuffedDefs = GetAllStuffed();
            Dictionary<StuffCategoryDef, List<ThingDef>> categorizedStuffDefs = new Dictionary<StuffCategoryDef, List<ThingDef>>();

            foreach (ThingDef def in allStuffedDefs)
            {
                if (def.stuffProps != null && def.stuffProps.categories != null)
                {
                    foreach (StuffCategoryDef category in def.stuffProps.categories)
                    {
                        if (category.label != "Metallic")
                        {
                            if (!categorizedStuffDefs.ContainsKey(category))
                            {
                                categorizedStuffDefs[category] = new List<ThingDef>();
                            }
                            categorizedStuffDefs[category].Add(def);
                        }
                    }
                }
            }

            List<ThingDef> matchingDefs = categorizedStuffDefs
                .Where(pair => categories.Contains(pair.Key))
                .SelectMany(pair => pair.Value)
                .ToList();

            List<Color> colors = matchingDefs
                .Select(def => StuffColorUtility.GetStuffColor(def))
                .Where(color => color != Color.white)
                .ToList();

            if (colors.Count > 0)
            {
                return colors[random.Next(colors.Count)];
            }
            else
            {
                return Color.white;
            }
        }
    }

    public static class PawnCloningUtility
    {
        public static Pawn Duplicate(Pawn pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException(nameof(pawn), "Pawn cannot be null.");

            // Create a new pawn with the same biological and chronological age
            float biologicalAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            float chronologicalAge = Math.Min(pawn.ageTracker.AgeChronologicalYearsFloat, biologicalAge);

            PawnGenerationRequest request = new PawnGenerationRequest(
                pawn.kindDef, pawn.Faction, PawnGenerationContext.NonPlayer, -1,
                forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
                canGeneratePawnRelations: false, mustBeCapableOfViolence: false,
                0f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true,
                allowPregnant: false, allowFood: true, allowAddictions: true,
                inhabitant: false, certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false,
                0f, 0f, null, 0f, null, null, null, null, null,
                fixedGender: pawn.gender, fixedIdeo: pawn.Ideo,
                fixedBiologicalAge: biologicalAge, fixedChronologicalAge: chronologicalAge,
                fixedLastName: null, fixedBirthName: null, fixedTitle: null,
                forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: true,
                forceDead: false, forcedXenogenes: null, forcedEndogenes: null,
                forcedXenotype: pawn.genes.Xenotype, forcedCustomXenotype: pawn.genes.CustomXenotype,
                allowedXenotypes: null, forceBaselinerChance: 0f,
                developmentalStages: DevelopmentalStage.Adult, pawnKindDefGetter: null,
                excludeBiologicalAgeRange: null, biologicalAgeRange: null,
                forceRecruitable: false, dontGiveWeapon: false, onlyUseForcedBackstories: false,
                maximumAgeTraits: -1, minimumAgeTraits: 0, forceNoGear: true);

            Pawn newPawn = PawnGenerator.GeneratePawn(request);

            // Copy essential properties
            CopyBasicProperties(pawn, newPawn);
            CopyGenesAndTraits(pawn, newPawn);
            CopyAppearanceAndStyle(pawn, newPawn);
            CopySkillsAndAbilities(pawn, newPawn);
            CopyHealthAndNeeds(pawn, newPawn);

            // Notify the new pawn of duplication
            newPawn.Notify_DuplicatedFrom(pawn);
            newPawn.Drawer.renderer.SetAllGraphicsDirty();
            newPawn.Notify_DisabledWorkTypesChanged();

            return newPawn;
        }

        private static void CopyBasicProperties(Pawn source, Pawn target)
        {
            target.Name = NameTriple.FromString(source.Name.ToString());
            target.gender = source.gender;

            if (ModsConfig.BiotechActive)
            {
                target.ageTracker.growthPoints = source.ageTracker.growthPoints;
                target.ageTracker.vatGrowTicks = source.ageTracker.vatGrowTicks;
                target.genes.xenotypeName = source.genes.xenotypeName;
                target.genes.iconDef = source.genes.iconDef;
            }
        }

        private static void CopyGenesAndTraits(Pawn source, Pawn target)
        {
            // Copy genes
            if (ModsConfig.BiotechActive)
            {
                target.genes.Xenogenes.Clear();
                foreach (var gene in source.genes.Xenogenes)
                {
                    target.genes.AddGene(gene.def, xenogene: true);
                }

                target.genes.Endogenes.Clear();
                foreach (var gene in source.genes.Endogenes)
                {
                    target.genes.AddGene(gene.def, xenogene: false);
                }
            }

            // Copy traits
            target.story.traits.allTraits.Clear();
            foreach (var trait in source.story.traits.allTraits)
            {
                if (!ModsConfig.BiotechActive || trait.sourceGene == null)
                {
                    target.story.traits.GainTrait(new Trait(trait.def, trait.Degree, trait.ScenForced));
                }
            }
        }

        private static void CopyAppearanceAndStyle(Pawn source, Pawn target)
        {
            target.story.headType = source.story.headType;
            target.story.bodyType = source.story.bodyType;
            target.story.hairDef = source.story.hairDef;
            target.story.HairColor = source.story.HairColor;
            target.story.SkinColorBase = source.story.SkinColorBase;
            target.story.skinColorOverride = source.story.skinColorOverride;
            target.story.furDef = source.story.furDef;

            target.style.beardDef = source.style.beardDef;
            if (ModsConfig.IdeologyActive)
            {
                target.style.BodyTattoo = source.style.BodyTattoo;
                target.style.FaceTattoo = source.style.FaceTattoo;
            }
        }

        private static void CopySkillsAndAbilities(Pawn source, Pawn target)
        {
            // Copy skills
            target.skills.skills.Clear();
            foreach (var skill in source.skills.skills)
            {
                var newSkill = new SkillRecord(target, skill.def)
                {
                    levelInt = skill.levelInt,
                    passion = skill.passion,
                    xpSinceLastLevel = skill.xpSinceLastLevel,
                    xpSinceMidnight = skill.xpSinceMidnight
                };
                target.skills.skills.Add(newSkill);
            }

            // Copy abilities
            target.abilities.abilities.Clear();
            foreach (var ability in source.abilities.abilities)
            {
                target.abilities.GainAbility(ability.def);
            }
        }

        private static void CopyHealthAndNeeds(Pawn source, Pawn target)
        {
            // Copy health (hediffs)
            target.health.hediffSet.hediffs.Clear();
            foreach (var hediff in source.health.hediffSet.hediffs)
            {
                if (hediff.def.duplicationAllowed && (hediff.Part == null || target.health.hediffSet.HasBodyPart(hediff.Part)))
                {
                    var newHediff = HediffMaker.MakeHediff(hediff.def, target, hediff.Part);
                    newHediff.CopyFrom(hediff);
                    target.health.hediffSet.AddDirect(newHediff);
                }
            }

            // Copy needs
            target.needs.AllNeeds.Clear();
            foreach (var need in source.needs.AllNeeds)
            {
                var newNeed = (Need)Activator.CreateInstance(need.def.needClass, target);
                newNeed.def = need.def;
                newNeed.SetInitialLevel();
                newNeed.CurLevel = need.CurLevel;
                target.needs.AllNeeds.Add(newNeed);
            }
        }
    }
}