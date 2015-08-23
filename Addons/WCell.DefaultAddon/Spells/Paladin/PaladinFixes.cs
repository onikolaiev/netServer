using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class PaladinFixes
	{
		/// <summary>
		/// All paladin auras
		/// </summary>
		public static readonly SpellLineId[] PaladinAuras = new[]
		{
			SpellLineId.PaladinDevotionAura, SpellLineId.PaladinCrusaderAura,
		                                           		SpellLineId.PaladinConcentrationAura,
		                                           		SpellLineId.PaladinFireResistanceAura,
		                                           		SpellLineId.PaladinFrostResistanceAura,
		                                           		SpellLineId.PaladinRetributionAura,
		                                           		SpellLineId.PaladinShadowResistanceAura
		                                           	};

		[Initialization(InitializationPass.Second)]
		public static void FixPaladin()
		{
			FixBlessings();
			FixStrongBuffs();
			FixHolyShock();

			// Players may only have one Hand on them per Paladin at any one time
			AuraHandler.AddAuraCasterGroup(
				SpellLineId.PaladinHandOfFreedom,
				SpellLineId.PaladinHandOfProtection,
				SpellLineId.PaladinHandOfReckoning,
				SpellLineId.PaladinHandOfSacrifice,
				SpellLineId.PaladinHandOfSalvation);
		}

		#region Blessings
		private static void FixBlessings()
		{
			// Normal and Greater blessings are mutually exclusive
			AuraHandler.AddAuraGroup(SpellLineId.PaladinBlessingOfKings, SpellLineId.PaladinGreaterBlessingOfKings);
			AuraHandler.AddAuraGroup(SpellLineId.PaladinBlessingOfMight, SpellLineId.PaladinGreaterBlessingOfMight);
			AuraHandler.AddAuraGroup(SpellLineId.PaladinBlessingOfWisdom, SpellLineId.PaladinGreaterBlessingOfWisdom);

			// only one blessing per pala
			AuraHandler.AddAuraCasterGroup(
				SpellLineId.PaladinBlessingOfKings,
				SpellLineId.PaladinBlessingOfMight,
				SpellLineId.PaladinBlessingOfWisdom,
				SpellLineId.PaladinGreaterBlessingOfKings,
				SpellLineId.PaladinGreaterBlessingOfMight,
				SpellLineId.PaladinGreaterBlessingOfWisdom,
				SpellLineId.PaladinGreaterBlessingOfSanctuary);

			// Sanctuary is a bit more complicated
			SpellHandler.Apply(spell =>
			{
				// first effect should mod damage taken
				var firstEffect = spell.Effects[0];
				if (firstEffect.EffectType == SpellEffectType.Dummy)
				{
					firstEffect.EffectType = SpellEffectType.ApplyAura;
					firstEffect.AuraType = AuraType.ModDamageTakenPercent;
				}

				// Custom proc (target = the one who is blessed): 
				// "When the target blocks, parries, or dodges a melee attack the target will gain $57319s1% of maximum displayed mana."
				spell.AddProcHandler(new TriggerSpellProcHandlerTemplate(
					SpellHandler.Get(SpellId.BlessingOfSanctuary),
					ProcTriggerFlags.MeleeHitOther | ProcTriggerFlags.RangedHitOther,
					ProcHandler.DodgeBlockOrParryValidator
					));

				// add str & stam
				var strEff = spell.AddAuraEffect(AuraType.ModStatPercent, ImplicitSpellTargetType.SingleFriend);
				strEff.MiscValue = (int)StatType.Strength;
				strEff.BasePoints = 10;

				var stamEff = spell.AddAuraEffect(AuraType.ModStatPercent, ImplicitSpellTargetType.SingleFriend);
				stamEff.MiscValue = (int)StatType.Stamina;
				stamEff.BasePoints = 10;
			},
			SpellLineId.PaladinProtectionBlessingOfSanctuary,
			SpellLineId.PaladinGreaterBlessingOfSanctuary);
		}
		#endregion

		private static void FixStrongBuffs()
		{
			// used by client only to prevent casting of Invul buffs
			var avengingWrathMarker2 = SpellHandler.AddCustomSpell(61988, "Invul Prevention");
			avengingWrathMarker2.IsPreventionDebuff = true;				// is prevention debuff
			avengingWrathMarker2.AddAuraEffect(AuraType.Dummy);			// make Aura
			avengingWrathMarker2.Attributes |= SpellAttributes.InvisibleAura | SpellAttributes.UnaffectedByInvulnerability;
			avengingWrathMarker2.AttributesExC |= SpellAttributesExC.PersistsThroughDeath;
			//avengingWrathMarker2.Durations = new Spell.DurationEntry { Min = 180000, Max = 180000 };
			avengingWrathMarker2.Durations = new Spell.DurationEntry { Min = 120000, Max = 120000 };

			SpellHandler.Apply(spell => spell.AddTargetTriggerSpells(SpellId.Forbearance),
								SpellLineId.PaladinHandOfProtection,
				//SpellLineId.PaladinLayOnHands,
								SpellId.ClassSkillDivineShield,
								SpellId.ClassSkillDivineProtection);

			SpellHandler.Apply(spell => spell.AddTargetTriggerSpells(SpellId.AvengingWrathMarker, avengingWrathMarker2.SpellId),
								SpellLineId.PaladinHandOfProtection,
								SpellId.ClassSkillAvengingWrath,
								SpellId.ClassSkillDivineShield,
								SpellId.ClassSkillDivineProtection);

			SpellHandler.Apply(spell => { spell.IsPreventionDebuff = true; },
							   SpellId.AvengingWrathMarker, SpellId.Forbearance);
		}

		#region Holy Shock
		private static void FixHolyShock()
		{
			AddHolyShockSpell(SpellId.PaladinHolyHolyShockRank1, SpellId.ClassSkillHolyShockRank1_2, SpellId.ClassSkillHolyShockRank1);
			AddHolyShockSpell(SpellId.ClassSkillHolyShockRank2, SpellId.ClassSkillHolyShockRank2_3, SpellId.ClassSkillHolyShockRank2_2);
			AddHolyShockSpell(SpellId.ClassSkillHolyShockRank3, SpellId.ClassSkillHolyShockRank3_3, SpellId.ClassSkillHolyShockRank3_2);
			AddHolyShockSpell(SpellId.ClassSkillHolyShockRank4, SpellId.ClassSkillHolyShockRank4_3, SpellId.ClassSkillHolyShockRank4_2);
			AddHolyShockSpell(SpellId.ClassSkillHolyShockRank5, SpellId.ClassSkillHolyShockRank5_3, SpellId.ClassSkillHolyShockRank5_2);
			AddHolyShockSpell(SpellId.ClassSkillHolyShockRank6_3, SpellId.ClassSkillHolyShockRank6_2, SpellId.ClassSkillHolyShockRank6);
			AddHolyShockSpell(SpellId.ClassSkillHolyShockRank7_3, SpellId.ClassSkillHolyShockRank7, SpellId.ClassSkillHolyShockRank7_2);
		}

		static void AddHolyShockSpell(SpellId spellid, SpellId heal, SpellId dmg)
		{
			SpellHandler.Apply(spell => { spell.GetEffect(SpellEffectType.Dummy).SpellEffectHandlerCreator = (cast, effect) => new FriendFoeTriggerSpellHandler(cast, effect, heal, dmg); }, spellid);
		}

		/// <summary>
		/// Triggers a different spell, depending on whether the target is friendly or hostile
		/// </summary>
		public class FriendFoeTriggerSpellHandler : SpellEffectHandler
		{
			readonly SpellId beneficialSpell;
			readonly SpellId harmfulSpell;
			public FriendFoeTriggerSpellHandler(SpellCast cast, SpellEffect eff, SpellId beneficialSpell, SpellId harmfulSpell)
				: base(cast, eff)
			{
				this.beneficialSpell = beneficialSpell;
				this.harmfulSpell = harmfulSpell;
			}

			protected override void Apply(WorldObject target)
			{
				var chr = m_cast.CasterUnit as Character;
				if (chr != null)
				{
					if (chr.MayAttack(target))
					{
						chr.SpellCast.Trigger(harmfulSpell, target);
					}
					else
					{
						chr.SpellCast.Trigger(beneficialSpell, target);
					}
				}
			}
		}
		#endregion
	}
}