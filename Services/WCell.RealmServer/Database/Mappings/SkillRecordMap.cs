﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;
using WCell.RealmServer.Database.Entities;

namespace WCell.RealmServer.Database.Mappings
{
    public class SkillRecordMap : ClassMap<SkillRecord>
    {
        public SkillRecordMap()
        {
			Not.LazyLoad();
			//EntityLowId
			Id(x => x.Guid);
            Map(x => x.OwnerId).Not.Nullable();
            Map(x => x.SkillId).Not.Nullable();
            Map(x => x.CurrentValue).Not.Nullable();
            //MaxVale
            Map(x => x.MaxValue).Column("MaxVal").Not.Nullable();
        }
    }
}
