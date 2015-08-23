using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells
{
	/// <summary>
	/// All non-Instance or battleground bound NPC spells are to be fixed here
	/// </summary>
	public static class NPCSpells
	{

		/// <summary>
		/// Most NPC spells miss basic spell information
		/// </summary>
		[Initialization(InitializationPass.Second)]
		public static void AdjustNPCSpells()
		{
			FixTotems();
			FixOthers();
		}

		private static void FixTotems()
		{
			
		}

		private static void FixOthers()
		{
			SpellHandler.Apply(spell =>
			{
				spell.PowerCost = 100;
				spell.CooldownTime = 2000;
				spell.Range = new SimpleRange(0, 10);
			}, SpellId.ConeOfFire);

			SpellHandler.Apply(spell =>
			{
				spell.PowerCost = 150;
				spell.CastDelay = 3000;
				spell.CooldownTime = 6000;
			}, SpellId.Chilled);

			SpellHandler.Apply(spell =>
			{
				spell.RequiredTargetType = RequiredSpellTargetType.NPCAlive;
				spell.Range = new SimpleRange(1,20);
				spell.RequiredTargetId = 9096;
			}, SpellId.MendDragon);
		}
	}
}