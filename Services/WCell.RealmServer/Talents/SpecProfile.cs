using System;
using System.Collections.Generic;
using WCell.RealmServer.Database.Entities;
using WCell.Util.Logging;
using WCell.RealmServer.Entities;
using WCell.Util;
using WCell.RealmServer.Database;
using System.Linq;

namespace WCell.RealmServer.Talents
{
	//[ActiveRecord(Access = PropertyAccess.Property)]
	public class SpecProfile // : WCellRecord<SpecProfile>
	{
		//static readonly Order[] order = { Order.Asc("SpecIndex") };

		public static int MAX_TALENT_GROUPS = 2;
		public static int MAX_GLYPHS_PER_GROUP = 6;

		internal static SpecProfile[] LoadAllOfCharacter(Character chr)
		{
			//ICriterion[] query = { Expression.Eq("_characterGuid", (int)chr.EntityId.Low) };

			var specs = RealmWorldDBMgr.DatabaseProvider.Query<SpecProfile>().Where(specProfile => specProfile.CharacterGuid == chr.EntityId.Low).OrderBy(specProfile => specProfile.SpecIndex).ToArray(); //FindAll(order, query);
			var i = 0;

			foreach (var spec in specs)
			{
				// ensure correct index
				if (spec.SpecIndex != i)
				{
					LogManager.GetCurrentClassLogger().Warn("Found SpecProfile for \"{0}\" with invalid SpecIndex {1} (should be {2})", spec.SpecIndex, i);
					spec.SpecIndex = i;
					RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(spec);
				}

				// ensure correct ActionButtons
				if (spec.ActionButtons == null)
				{
					// make sure to create the array, if loading or the last save failed
					spec.ActionButtons = (byte[])chr.Archetype.ActionButtons.Clone();
				}
				else if (spec.ActionButtons.Length != chr.Archetype.ActionButtons.Length)
				{
					var buts = spec.ActionButtons;
					Array.Resize(ref buts, chr.Archetype.ActionButtons.Length);
					spec.ActionButtons = buts;
				}
				i++;
			}
			return specs;
		}

		/// <summary>
		/// Creates a new SpecProfile and saves it to the db.
		/// </summary>
		/// <param name="record">The character or pet that will own the spec profile.</param>
		/// <returns>The newly created SpecProfile.</returns>
		public static SpecProfile NewSpecProfile(Character owner, int specIndex)
		{
			return NewSpecProfile(owner, specIndex, (byte[])owner.Archetype.ActionButtons.Clone());
		}

		/// <summary>
		/// Creates a new SpecProfile and saves it to the db.
		/// </summary>
		/// <param name="record">The character or pet that will own the spec profile.</param>
		/// <returns>The newly created SpecProfile.</returns>
		public static SpecProfile NewSpecProfile(Character owner, int specIndex, byte[] actionbar)
		{
			// TODO: Glyphs
			var newProfile = new SpecProfile(owner.EntityId.Low, specIndex)
			{
				ActionButtons = actionbar,
				GlyphIds = new uint[6]
			};

			return newProfile;
		}

		//[Field("CharacterId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _characterGuid;

		private SpecProfile()
		{
			TalentSpells = new List<SpellRecord>();
		}

		private SpecProfile(uint lowId, int specIndex)
			: this()
		{
			_characterGuid = (int)lowId;
			SpecIndex = specIndex;
			RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(this); // TODO: Will this create the same outcome as setting State to new?
		}

		/// <summary>
		/// Primary key. A combination of Character id and TalentGroup.
		/// </summary>
		//[PrimaryKey(PrimaryKeyType.Assigned)]
		public long SpecRecordId
		{
			get
			{
				return Utility.MakeLong(_characterGuid, SpecIndex);
			}
			set
			{
				_characterGuid = (int)value;
				SpecIndex = (int)(value >> 32);
			}
		}

		public uint CharacterGuid
		{
			get { return (uint)_characterGuid; }
			set { _characterGuid = (int)value; }
		}

		/// <summary>
		/// The Id of the Talent Group currently in use.
		/// </summary>
		//[Property]
		public int SpecIndex
		{
			get;
			set;
		}

		/// <summary>
		/// TODO: Move to own table
		/// </summary>
		//[Property]
		public uint[] GlyphIds
		{
			get;
			set;
		}

		/// <summary>
		/// 
		/// </summary>
		//[Property]
		public byte[] ActionButtons
		{
			get;
			set;
		}

		public List<SpellRecord> TalentSpells
		{
			get;
			internal set;
		}
	}
}