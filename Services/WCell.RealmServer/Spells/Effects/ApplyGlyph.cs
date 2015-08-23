﻿using WCell.RealmServer.Entities;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Effects
{
	public class ApplyGlyphEffectHandler : SpellEffectHandler
	{ 
		public ApplyGlyphEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}
		public override void Initialize(ref SpellFailedReason fail)
		{
			if (m_cast.m_glyphSlot != 0)
			{
				var glyph = (uint)m_cast.Spell.Effects[0].MiscValue;
				var properties = GlyphInfoHolder.GetPropertiesEntryForGlyph(glyph);
				var slot = GlyphInfoHolder.GetGlyphSlotEntryForGlyphSlotId(m_cast.CasterChar.GetGlyphSlot((byte)m_cast.m_glyphSlot));
				if (properties.TypeFlags != slot.TypeFlags)
				{
					fail =  SpellFailedReason.InvalidGlyph;
				}
			}
		}
		public override void Apply()
		{
			var chr = m_cast.CasterChar;
			chr.ApplyGlyph((byte)m_cast.m_glyphSlot, GlyphInfoHolder.GetPropertiesEntryForGlyph((uint)m_cast.Spell.Effects[0].MiscValue));
		}
	}
}
