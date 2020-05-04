using Verse;

namespace BackupPower
{
    public static class I18n
    {
        private static string Translate( string key, params NamedArgument[] args )
        {
            return Key( key ).Translate( args ).Resolve();
        }

        private static string Key( string key )
        {
            return $"Fluffy.BackupPower.{key}";
        }

        public static string PlaceWorker_PlaceOnPowerPlant = Translate( "PlaceWorker.PlaceOnPowerPlant" );
        public static string PlaceWorker_PlaceOnFlickable = Translate( "PlaceWorker.PlaceOnFlickable" );
        public static string PlaceWorker_OnlyOneAttachmentAllowed = Translate( "PlaceWorker.OnlyOneAttachmentAllowed" );

        public static string FormatSeconds( this float seconds ) => seconds.ToString( "##.#'s'" );

        public static string Settings_UpdateInterval( float value, float @default ) => 
            Translate( "Settings.UpdateInterval", value.FormatSeconds(), @default.FormatSeconds() );
        public static string Settings_UpdateInterval_Tooltip = Translate( "Settings.UpdateInterval.Tooltip" );

        public static string Settings_MinimumOnTime( float value, float @default ) =>
            Translate( "Settings.MinimumOnTime", value.FormatSeconds(), @default.FormatSeconds() );
        public static string Settings_MinimumOnTime_Tooltip = Translate( "Settings.MinimumOnTime.Tooltip" );


        public static string Generator = Translate( "Generator" );

        public static string AttachmentDestroyedBecauseParentGone( string label ) =>
            Translate( "AttachmentDestroyedBecauseParentGone",  label );
    }
}