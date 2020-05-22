// Utilities.cs
// Copyright Karel Kroeze, -2020

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace BackupPower
{
    public static class Utilities
    {
        public static void AddSafe<T>( this HashSet<T> set, T item )
        {
            if ( item == null )
                Verse.Log.ErrorOnce( "tried adding null element to hashset", 123411 );
            if ( set.Contains( item ) )
                Verse.Log.ErrorOnce( "tried adding duplicate item to hashset", 123412 );
            set.Add( item );
        }

        public static Rect MiddlePart( this Rect rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f )
        {
            return new Rect( rect.xMin + rect.width  * left,
                             rect.yMin + rect.height * top,
                             rect.width  * ( 1 - left - right ),
                             rect.height * ( 1 - top  - bottom ) );
        }

        public static bool HasStorage( this PowerNet net )
        {
            return !net.batteryComps.NullOrEmpty();
        }

        public static string Bold( this string msg)
        {
            return $"<b>{msg}</b>";
        }

        public static float StorageLevel( this PowerNet net )
        {
            if ( net.batteryComps.NullOrEmpty() ) return 0;

            var (current, max) = net.batteryComps
                                       .Select( b => ( b.StoredEnergy, b.Props.storedEnergyMax ) )
                                       .Aggregate( ( a, b ) => (
                                                       a.StoredEnergy    + b.StoredEnergy,
                                                       a.storedEnergyMax + b.storedEnergyMax ) );
            return current / max;
        }

        public static void DrawLineDashed( Vector2 start, Vector2 end, Color? color = null, float size = 1, float stroke = 5,
                                           float dash = 3)
        {
            var partLength = dash + stroke;
            var totalLength = ( end - start ).magnitude;
            var direction = ( end - start ).normalized;
            var done = 0f;
            while ( done < totalLength )
            {
                var _start = start + done * direction;
                var _end = start + Mathf.Min( done + stroke, totalLength ) * direction;
                Widgets.DrawLine( _start, _end, color.GetValueOrDefault( Color.white ), size );
                done += partLength;
            }
        }

        public static Vector2 BottomLeft( this Rect rect )
        {
            return new Vector2( rect.xMin, rect.yMax );
        }

        public static void RemoveSafe<T>( this HashSet<T> set, T item )
        {
            if ( item == null)
                Verse.Log.ErrorOnce( "tried removing null element from hashset", 123413 );
            if ( !set.Contains( item ))
                Verse.Log.ErrorOnce( "tried removing item from hashset that it does not have", 123414 );
            set.Remove( item );
        }

        private static readonly ConditionalWeakTable<ThingWithComps, CompRefuelable> _refuelables =
            new ConditionalWeakTable<ThingWithComps, CompRefuelable>();

        public static CompRefuelable RefuelableComp( this ThingWithComps parent )
        {
            if ( _refuelables.TryGetValue( parent, out var refuelable ) )
                return refuelable;
            refuelable = parent.GetComp<CompRefuelable>();
            _refuelables.Add( parent, refuelable );
            return refuelable;
        }

        private static readonly ConditionalWeakTable<ThingWithComps, CompFlickable> _flickables =
            new ConditionalWeakTable<ThingWithComps, CompFlickable>();

        public static CompFlickable FlickableComp( this ThingWithComps parent )
        {
            if ( _flickables.TryGetValue( parent, out var flickable ) )
                return flickable;
            flickable = parent.GetComp<CompFlickable>();
            _flickables.Add( parent, flickable );
            return flickable;
        }

        private static readonly FieldInfo _flickable_wantSwitchOn_FI = typeof( CompFlickable ).GetField( "wantSwitchOn", BindingFlags.Instance | BindingFlags.NonPublic );

        public static void Force( this CompFlickable flickable, bool mode )
        {
            if ( mode != flickable.SwitchIsOn )
                flickable.SwitchIsOn = mode;
            if ( flickable.WantsFlick() )
                _flickable_wantSwitchOn_FI.SetValue( flickable, mode );
        }

        private static readonly ConditionalWeakTable<ThingWithComps, CompBreakdownable> _breakdownables =
            new ConditionalWeakTable<ThingWithComps, CompBreakdownable>();

        public static CompBreakdownable BreakdownableComp( this ThingWithComps parent )
        {
            if ( _breakdownables.TryGetValue( parent, out var breakdownable ) )
                return breakdownable;
            breakdownable = parent.GetComp<CompBreakdownable>();
            _breakdownables.Add( parent, breakdownable );
            return breakdownable;
        }

        private static readonly ConditionalWeakTable<ThingWithComps, CompPowerPlant> _powerplants =
            new ConditionalWeakTable<ThingWithComps, CompPowerPlant>();

        public static CompPowerPlant PowerPlantComp( this ThingWithComps parent )
        {
            if ( _powerplants.TryGetValue( parent, out var powerplant ) )
                return powerplant;
            powerplant = parent.GetComp<CompPowerPlant>();
            _powerplants.Add( parent, powerplant );
            return powerplant;
        }

        private static readonly MethodInfo _desiredOutputGetter_MI = typeof( CompPowerPlant )
                                                                    .GetProperty(
                                                                         "DesiredPowerOutput",
                                                                         BindingFlags.Instance |
                                                                         BindingFlags.NonPublic )
                                                                    .GetMethod;

        public static float DesiredOutput( this CompPowerPlant plant )
        {
            return (float) _desiredOutputGetter_MI.Invoke( plant, null );
        }
    }

    [RimWorld.DefOf]
    public static class DefOf
    {
        public static ThingDef BackupPower_Attachment;
    }
}