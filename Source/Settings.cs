using UnityEngine;
using Verse;

namespace BackupPower
{
	public class Settings : ModSettings
    {
        public int UpdateInterval = GenTicks.TicksPerRealSecond;

		public void DoWindowContents(Rect canvas)
		{
			var options = new Listing_Standard();
			options.Begin(canvas);
            options.Label( I18n.Settings_UpdateInterval( (float)UpdateInterval / GenTicks.TicksPerRealSecond, 1f ), tooltip: I18n.Settings_UpdateInterval_Tooltip );
            UpdateInterval = (int) options.Slider( (float)UpdateInterval / GenTicks.TicksPerRealSecond, 1 / 2f, 60 ) *
                             GenTicks.TicksPerRealSecond;
			options.End();
		}
		
		public override void ExposeData()
        {
            Scribe_Values.Look( ref UpdateInterval, "updateInterval", GenTicks.TicksPerRealSecond );
        }
	}
}