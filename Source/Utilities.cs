// Utilities.cs
// Copyright Karel Kroeze, -2020

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace BackupPower
{
    public static class Utilities
    {
        public static void AddSafe<T>( this HashSet<T> set, T item )
        {
            if ( item == null )
                throw new ArgumentNullException( nameof( item ) );
            if ( set.Contains( item ) )
                throw new ArgumentException( "hashset already contains this item", nameof( item ) );
            set.Add( item );
        }

        public static void RemoveSafe<T>( this HashSet<T> set, T item )
        {
            if ( item == null )
                throw new ArgumentNullException( nameof( item ) );
            if ( !set.Contains( item ) )
                throw new ArgumentException("hashset does not contain this item", nameof( item ) );
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
}