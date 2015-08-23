﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Database.Mappings
{
	public class SummonedPetRecordMap : ClassMap<SummonedPetRecord>
	{
		public SummonedPetRecordMap()
		{
			Not.LazyLoad();
			#region Base Fields
			Id(x => x.EntryId).GeneratedBy.Assigned();
			Map(x => x.NameTimeStamp);
			Map(x => x.PetState).Not.Nullable();
			Map(x => x.AttackMode).Not.Nullable();
			Map(x => x.Flags).Not.Nullable();
			Map(x => x.OwnerId).Not.Nullable();
			Map(x => x.IsActivePet);
			Map(x => x.Name);
			Map(x => x.ActionButtons).Not.Nullable();
			#endregion

			Map(x => x.PetNumber).Not.Nullable();
		}
	}
}
