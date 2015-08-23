using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.Hunter
{
	public static class HunterFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixHunter()
		{
			// taming has an invalid target
			SpellHandler.Apply(spell =>
			    {
				    spell.GetEffect(AuraType.PeriodicTriggerSpell).ImplicitTargetA = ImplicitSpellTargetType.SingleEnemy;
			    }, SpellId.ClassSkillTameBeast);

			// Only one Aspect can be active at a time
			AuraHandler.AddAuraGroup(SpellLineId.HunterAspectOfTheBeast, SpellLineId.HunterAspectOfTheCheetah,
									 SpellLineId.HunterAspectOfTheDragonhawk, SpellLineId.HunterAspectOfTheHawk,
									 SpellLineId.HunterAspectOfTheMonkey,
									 SpellLineId.HunterAspectOfThePack, SpellLineId.HunterAspectOfTheViper,
									 SpellLineId.HunterAspectOfTheWild);

			// Only one Sting per Hunter can be active on any one target
			AuraHandler.AddAuraGroup(SpellLineId.HunterSurvivalWyvernSting, SpellLineId.HunterSerpentSting,
									 SpellLineId.HunterScorpidSting, SpellLineId.HunterViperSting, SpellLineId.HunterSerpentSting);

            // Sealed cooldowns
            SpellLineId.HunterScorpidSting.Apply(spell =>
                {
                    spell.CooldownTime = 0;
                });

            SpellLineId.HunterConcussiveShot.Apply(spell =>
                {
                    spell.CooldownTime = 12000;
                });

            SpellLineId.HunterSerpentSting.Apply(spell =>
                {
                    spell.CooldownTime = 0;
                });

            SpellLineId.HunterViperSting.Apply(spell =>
                {
                    spell.CooldownTime = 15000;
                });


			// Expose Weakness aura applied on the target  - Seems the spell has changed
			//SpellHandler.Apply(spell => spell.Effects[0].ImplicitTargetA = ImplicitTargetType.SingleEnemy,
			//                   SpellId.ExposeWeakness_2);
		}
	}
}