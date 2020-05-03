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

        public static string Settings_UpdateInterval( float value, float @default ) => Translate(
            "Settings.UpdateInterval",
            value.ToString( "##.#'s'" ),
            @default.ToString( "##.#'s'" ) );
        public static string Settings_UpdateInterval_Tooltip = Translate( "Settings.UpdateInterval.Tooltip" );
        public static string Generator = Translate( "Generator" );

        public static string AttachmentDestroyedBecauseParentGone( string label ) =>
            Translate( "AttachmentDestroyedBecauseParentGone",  label );
    }
}