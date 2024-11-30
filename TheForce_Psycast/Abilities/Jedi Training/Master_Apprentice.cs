using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast.Abilities.Jedi_Training;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities
{
    internal class Master_Apprentice : Ability
    {
        private Hediff_Master masterHediff;

        public override void Init()
        {
            base.Init();
            if (masterHediff == null)
            {
                masterHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master ?? CreateMasterHediff();
                masterHediff.ChangeApprenticeCapacitySetting(Force_ModSettings.apprenticeCapacity);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Pawn == null || CasterPawn == null || !CasterPawn.HasPsylink) return false;

            var targetPawn = target.Pawn;
            if (IsInvalidApprentice(targetPawn, showMessages)) return false;

            return base.ValidateTarget(target, showMessages);
        }

        private bool IsInvalidApprentice(Pawn targetPawn, bool showMessages)
        {
            var apprenticeHediff = targetPawn.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Force_Apprentice) as Hediff_Apprentice;
            if (apprenticeHediff != null && apprenticeHediff.master != CasterPawn)
            {
                if (showMessages) Messages.Message("Force.TargetIsNotYourApprentice".Translate(), MessageTypeDefOf.RejectInput);
                return true;
            }

            var masterHediff = CasterPawn.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
            if (masterHediff == null || (masterHediff.apprentices.Count >= masterHediff.apprenticeCapacity && !masterHediff.apprentices.Contains(targetPawn)))
            {
                if (showMessages) Messages.Message("Force.ApprenticeLimitReached".Translate(), MessageTypeDefOf.RejectInput);
                return true;
            }

            if (targetPawn.GetPsylinkLevel() >= CasterPawn.GetPsylinkLevel())
            {
                if (showMessages) Messages.Message("Force.TargetPsylinkLevelTooHigh".Translate(targetPawn.Label), MessageTypeDefOf.CautionInput);
                return true;
            }
            if(!targetPawn.HasPsylink)
            {
                if (showMessages) Messages.Message("Force.NotPsycaster".Translate(targetPawn.Label), MessageTypeDefOf.CautionInput);
                return true;
            }
            return false;
        }


        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (masterHediff == null) Init();
            masterHediff.ChangeApprenticeCapacitySetting(Force_ModSettings.apprenticeCapacity);

            if (targets == null || targets.Length == 0 || !(targets[0].Thing is Pawn targetPawn))
            {
                Log.Warning("Invalid cast target; either null or not a Pawn.");
                return;
            }

            if (targetPawn.health?.hediffSet == null)
            {
                Log.Warning("Target pawn health is invalid.");
                return;
            }
            Hediff_Apprentice apprenticeHediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Apprentice) as Hediff_Apprentice;
            if (apprenticeHediff != null && apprenticeHediff.master == CasterPawn)
            {
                if (CasterPawn.health.hediffSet.HasHediff(ForceDefOf.Force_TeachingCooldown))
                {
                    Messages.Message("Force.MasterIsInCooldown".Translate(), MessageTypeDefOf.RejectInput);
                    return;
                }
                ShowTeachAbilityDialog(targetPawn);
            }
            else
            {
                AssignApprenticeHediff(targetPawn);
                if (Force_ModSettings.rankUpApprentice) ChangeBackstoryBasedOnAlignment(targetPawn);
                if (!CasterPawn.relations.DirectRelationExists(ForceDefOf.Force_ApprenticeRelation, targetPawn))
                {
                    CasterPawn.relations.AddDirectRelation(ForceDefOf.Force_ApprenticeRelation, targetPawn);
                }
                if (!targetPawn.relations.DirectRelationExists(ForceDefOf.Force_MasterRelation, CasterPawn))
                {
                    targetPawn.relations.AddDirectRelation(ForceDefOf.Force_MasterRelation, CasterPawn);
                }
            }
        }

        private void ChangeBackstoryBasedOnAlignment(Pawn targetPawn)
        {
            HashSet<string> sithSpawnCategories = new HashSet<string> { "SithChildhood" };
            HashSet<string> jediSpawnCategories = new HashSet<string> { "JediChildhood" };
            float darksideAlignment = masterHediff.GetDarksideAlignment();
            float lightsideAlignment = masterHediff.GetLightsideAlignment();

            if (darksideAlignment > lightsideAlignment)
            {
                if (targetPawn.story.Childhood.spawnCategories.Any(category => sithSpawnCategories.Contains(category)))
                {
                    Messages.Message("Force.AlreadyHaveCorrectBackstory".Translate(targetPawn.Label, targetPawn.story.Childhood.title), MessageTypeDefOf.RejectInput);
                    return;
                }
                targetPawn.story.Childhood = ForceDefOf.Force_SithApprenticeChosen;
            }
            else if (lightsideAlignment > darksideAlignment)
            {
                if (targetPawn.story.Childhood.spawnCategories.Any(category => jediSpawnCategories.Contains(category)))
                {
                    Messages.Message("Force.AlreadyHaveCorrectBackstory".Translate(targetPawn.Label, targetPawn.story.Childhood.title), MessageTypeDefOf.RejectInput);
                    return;
                }
                targetPawn.story.Childhood = ForceDefOf.Force_JediPadawanChosen;
            }
            else
            {

                bool chooseSith = UnityEngine.Random.value < 0.5f;
                if (chooseSith)
                {
                    if (targetPawn.story.Childhood.spawnCategories.Any(category => sithSpawnCategories.Contains(category)))
                    {
                        Messages.Message("Force.AlreadyHaveCorrectBackstory".Translate(targetPawn.Label, targetPawn.story.Childhood.title), MessageTypeDefOf.RejectInput);
                        return;
                    }
                    targetPawn.story.Childhood = ForceDefOf.Force_SithApprenticeChosen;
                }
                else
                {
                    if (targetPawn.story.Childhood.spawnCategories.Any(category => jediSpawnCategories.Contains(category)))
                    {
                        Messages.Message("Force.AlreadyHaveCorrectBackstory".Translate(targetPawn.Label, targetPawn.story.Childhood.title), MessageTypeDefOf.RejectInput);
                        return;
                    }
                    targetPawn.story.Childhood = ForceDefOf.Force_JediPadawanChosen;
                }
            }
        }


        private void AssignApprenticeHediff(Pawn targetPawn)
        {
            var apprenticeHediff = HediffMaker.MakeHediff(ForceDefOf.Force_Apprentice, targetPawn, targetPawn.health.hediffSet.GetBrain()) as Hediff_Apprentice;
            if (apprenticeHediff != null)
            {
                apprenticeHediff.master = pawn;
                masterHediff.apprentices.Add(targetPawn);
                targetPawn.health.AddHediff(apprenticeHediff);

                targetPawn.Notify_DisabledWorkTypesChanged();
                PawnComponentsUtility.AddAndRemoveDynamicComponents(targetPawn);
            }
        }

        private void ShowTeachAbilityDialog(Pawn apprenticePawn)
        {
            var masterComp = pawn.GetComp<CompAbilities>();
            if (masterComp == null)
            {
                Messages.Message("Force.MasterHasNoAbilities".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.RejectInput);
                return;
            }
            var apprenticeComp = apprenticePawn.GetComp<CompAbilities>();
            if (apprenticeComp == null)
            {
                Messages.Message("Force.ApprenticeCannotLearnAbility".Translate(apprenticePawn.Name.ToStringShort), MessageTypeDefOf.RejectInput);
                return;
            }
            List<Ability> teachableAbilities = masterComp.LearnedAbilities
                .FindAll(ability => !apprenticeComp.HasAbility(ability.def));
            Find.WindowStack.Add(new Dialog_TeachAbility(pawn, apprenticePawn, teachableAbilities));
        }

        private Hediff_Master CreateMasterHediff()
        {
            var hediff = HediffMaker.MakeHediff(ForceDefOf.Force_Master, pawn) as Hediff_Master;
            pawn.health.AddHediff(hediff);
            return hediff;
        }
    }



    public class Dialog_TeachAbility : Window
    {
        private Pawn masterPawn;
        private Pawn apprenticePawn;
        private List<Ability> teachableAbilities;

        private Vector2 scrollPosition = Vector2.zero; // Keeps track of scroll position
        private const float ButtonHeight = 40f; // Height of the buttons
        private const float RowHeight = 80f;

        public override Vector2 InitialSize => new Vector2(600f, 600f); // Window size

        public Dialog_TeachAbility(Pawn master, Pawn apprentice, List<Ability> abilities)
        {
            this.masterPawn = master;
            this.apprenticePawn = apprentice;
            this.teachableAbilities = abilities;

            doCloseButton = true;
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), "Teach Ability or Cancel Apprenticeship");
            if (Widgets.ButtonText(new Rect(0f, 50f, inRect.width, ButtonHeight), "Remove Apprentice"))
            {
                RemoveApprentice();
            }
            if (Widgets.ButtonText(new Rect(0f, 100f, inRect.width, ButtonHeight), "Graduate Apprentice"))
            {
                GraduateApprentice();
            }
            Rect outRect = new Rect(0f, 150f, inRect.width, inRect.height - 150f);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, teachableAbilities.Count * RowHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float y = 0f;
            foreach (Ability ability in teachableAbilities)
            {
                DrawAbilityRow(new Rect(0f, y, viewRect.width, RowHeight), ability);
                y += RowHeight;
            }
            Widgets.EndScrollView();
        }

        private void DrawAbilityRow(Rect rect, Ability ability)
        {
            Rect iconRect = new Rect(rect.x, rect.y, 60f, 60f);
            if (ability.def.icon != null)
            {
                Widgets.DrawTextureFitted(iconRect, ability.def.icon, 1f);
            }
            Rect textRect = new Rect(rect.x + 70f, rect.y, rect.width - 200f, rect.height);
            Text.Font = GameFont.Small;
            Widgets.Label(textRect, $"{ability.def.label.CapitalizeFirst()}\n{ability.def.description}");
            if (Widgets.ButtonText(new Rect(rect.width - 120f, rect.y + 40f, 100f, 30f), "Teach"))
            {
                TeachAbility(ability);
            }
        }

        private void TeachAbility(Ability ability)
        {
            var apprenticeComp = apprenticePawn.GetComp<CompAbilities>();
            if (apprenticeComp == null)
            {
                Messages.Message("Force.ApprenticeCannotLearnAbility".Translate(apprenticePawn.Name.ToStringShort), MessageTypeDefOf.RejectInput);
                return;
            }

            apprenticeComp.GiveAbility(ability.def);
            ApplyCooldown(masterPawn);
            Messages.Message("Force.TaughtNewAbility".Translate(apprenticePawn.Name.ToStringShort, ability.def.label), MessageTypeDefOf.PositiveEvent);
            Close();
        }

        private void ApplyCooldown(Pawn pawn)
        {
            Hediff cooldownHediff = HediffMaker.MakeHediff(ForceDefOf.Force_TeachingCooldown, pawn);
            pawn.health.AddHediff(cooldownHediff);
        }

        private void RemoveApprentice()
        {
            var masterHediff = masterPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
            if (masterHediff != null)
            {
                RemoveApprenticeReference();
                Messages.Message("Force.ApprenticeshipRemoved".Translate(apprenticePawn.Name.ToStringShort), MessageTypeDefOf.NeutralEvent);
            }
            Close();
        }

        public void GraduateApprentice()
        {
            var apprenticeStory = apprenticePawn.story;
            if (Force_ModSettings.rankUpApprentice)
            {
                if (apprenticeStory != null)
                {
                    var masterHediff = masterPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
                    if (masterHediff != null)
                    {
                        masterHediff.apprentices.Remove(apprenticePawn);
                        RemoveApprenticeReference();
                        if (Force_ModSettings.rankUpMaster)
                        {
                            masterHediff.graduatedApprenticesCount++;
                            masterHediff.CheckAndPromoteMasterBackstory();
                        }
                    }
                    Find.WindowStack.Add(new Dialog_SelectBackstory(apprenticePawn));
                }
            }
            Close();
        }

        public void RemoveApprenticeReference()
        {
            var masterHediff = masterPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
            masterHediff.apprentices.Remove(apprenticePawn);
            apprenticePawn.health.RemoveHediff(apprenticePawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Apprentice));
            apprenticePawn.relations.RemoveDirectRelation(ForceDefOf.Force_MasterRelation, masterPawn);
            masterPawn.relations.RemoveDirectRelation(ForceDefOf.Force_ApprenticeRelation, apprenticePawn);
        }
    }

    public class Dialog_SelectBackstory : Window
    {
        private Pawn targetPawn;
        private List<BackstoryDef> availableBackstories;

        public Dialog_SelectBackstory(Pawn targetPawn)
        {
            this.targetPawn = targetPawn ?? throw new ArgumentNullException(nameof(targetPawn));
            BackstoryModExtension modExtension = targetPawn.story.Childhood.GetModExtension<BackstoryModExtension>();
            if (modExtension != null)
            {
                availableBackstories = modExtension.availableBackstories ?? new List<BackstoryDef>(); // Ensure availableBackstories is not null
            }

            this.closeOnClickedOutside = false;
            this.doCloseX = true;
            doCloseButton = true;
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize => new Vector2(300f, 400f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f), "Select an Adult Backstory");
            Text.Font = GameFont.Small;
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            if (availableBackstories != null && availableBackstories.Any())
            {
                foreach (var backstory in availableBackstories)
                {
                    if (listing.ButtonText(backstory.title))
                    {
                        targetPawn.story.Adulthood = backstory;
                        Messages.Message("Force.ApprenticeshipGraduated".Translate(targetPawn.Name.ToStringShort, targetPawn.story.Adulthood.title), MessageTypeDefOf.PositiveEvent);
                        Close();
                    }
                }
            }
            else
            {
                Widgets.Label(new Rect(inRect.x, inRect.y + 40f, inRect.width, 30f), "No available backstories to select.");
            }

            listing.End();
        }
    }

    public class BackstoryModExtension : DefModExtension
    {
        public List<BackstoryDef> availableBackstories;
    }
}

    

    