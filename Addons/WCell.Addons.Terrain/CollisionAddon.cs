using System.Globalization;
using WCell.Core.Addons;
using WCell.Core.Initialization;
using WCell.Terrain;

namespace WCell.Addons.Terrain
{
	public class CollisionAddon : WCellAddonBase<CollisionAddon>
	{
		/// <summary>
		/// The culture-invariant name of this Addon
		/// </summary>
		public override string Name
		{
			get { return "Terrain Addon"; }
		}

		/// <summary>
		/// A shorthand name of the Addon that does not contain any spaces.
		///  Used as unique ID for this Addon.
		/// </summary>
		public override string ShortName
		{
			get { return "Terrain"; }
		}

		/// <summary>
		/// The name of the Author
		/// </summary>
		public override string Author
		{
			get { return "The WCell Team"; }
		}

		/// <summary>
		/// Website (where this Addon can be found)
		/// </summary>
		public override string Website
		{
			get { return "http://www.wcell.org"; }
		}

		/// <summary>
		/// The localized name, in the given culture
		/// </summary>
		public override string GetLocalizedName(CultureInfo culture)
		{
			return "Terrain Provider";
		}

		public override void TearDown()
		{

		}

		public override bool UseConfig
		{
			get { return true; }
		}

		public override Util.Variables.IConfiguration CreateConfig()
		{
			var cfg = TerrainAddonConfiguration.Instance;
			WCellTerrainSettings.Config = cfg;
			return cfg;
		}

		[Initialization(InitializationPass.First, "Initialize Terrain Addon")]
		public static void Init()
		{
		}
	}
}