// MapComponent_PowerBroker.cs
// Copyright Karel Kroeze, -2020

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BackupPower
{
    public class MapComponent_PowerBroker : MapComponent
    {
        public HashSet<Building_BackupPowerAttachment> brokers = new HashSet<Building_BackupPowerAttachment>();
        public MapComponent_PowerBroker( Map map ) : base( map )
        {
        }

        public static MapComponent_PowerBroker For( Map map )
        {
            return map.GetComponent<MapComponent_PowerBroker>();
        }

        public static void RegisterBroker( Building_BackupPowerAttachment broker, bool update = false, Map map = null )
        {
            For( map ?? broker.Map ).brokers.AddSafe( broker );
        }

        public static void DeregisterBroker( Building_BackupPowerAttachment broker, Map map = null )
        {
            For( map ?? broker.Map ).brokers.RemoveSafe( broker );
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if ( Find.TickManager.TicksGame % BackupPower.Settings.UpdateInterval != 0 )
                return;

            foreach ( var group in brokers.Where( b => b.PowerNet != null )
                                          .GroupBy( b => b.PowerNet ) )
                PowerNetUpdate( group.Key, new HashSet<Building_BackupPowerAttachment>( group ) );
        }

        public void PowerNetUpdate( PowerNet net, HashSet<Building_BackupPowerAttachment> brokers )
        {
            // get desired power
            var users = net.powerComps
                           .Select( p => ( comp: p,
                                           broker: ( p.parent is Building building )
                                               ? brokers.FirstOrDefault( b => b.Parent == building )
                                               : null,
                                           consumption: Consumption( p ),
                                           currentProduction: CurrentProduction( p ),
                                           potentialProduction: PotentialProduction( p ) ) );

            var need = users.Sum( u => u.consumption );
            var production = users.Sum( u => u.currentProduction );
            var staticProduction = users.Where( u => u.broker == null ).Sum( u => u.currentProduction );

//            Log.Debug( $"need: {need}, production: {production}, static: {staticProduction}" );
            
            if ( production > need )
            {
                // try to shut backups off
                var overProduction = production - need;
                var backups = users.Where( u => u.currentProduction > 0
                                             && u.currentProduction < overProduction
                                             && u.broker            != null
                                             && u.broker.CanTurnOff() )
                                   .ToList();

                while ( backups.Any() )
                {
                    // TODO: implement knapsack packing to optimise assignment, or keep it random?
                    var backup = backups.RandomElementByWeight( c => 1 / c.currentProduction ); // weight smaller producers to turn off first.

                    // turn it off
                    backup.broker.TurnOff();

                    // remove invalid backups from the list
                    overProduction -= backup.currentProduction;
                    backups = backups.Where( b => b != backup && b.currentProduction < overProduction ).ToList();
                }
            }

            if ( production < need )
            {
                // try to turn backups on
                var underProduction = need - production;
                var backups = users.Where( u => Math.Abs( u.currentProduction ) < Mathf.Epsilon
                                             && u.potentialProduction           > 0
                                             && u.broker                        != null )
                                   .ToList();
                
                while ( underProduction > 0 && backups.Any() )
                {
                    // TODO: implement knapsack packing to optimise assignment, or keep it random?
                    var backup = backups.RandomElementByWeight( c => c.potentialProduction ); // weight larger producers to turn on first.

                    // turn it on
                    backup.broker.TurnOn();

                    // remove invalid backups from the list
                    underProduction -= backup.currentProduction;
                    backups.Remove( backup );
                }
            }
        }
        
        public float PotentialProduction( CompPowerTrader comp )
        {
            if ( !( comp is CompPowerPlant plant ) )
                return 0;

            var refuelable = plant.parent.RefuelableComp();
            if ( refuelable != null && !refuelable.HasFuel )
                return 0;

            var breakdownable = plant.parent.BreakdownableComp();
            if ( breakdownable != null && breakdownable.BrokenDown )
                return 0;

            // TODO: check how this interacts with variable power output buildings, e.g. solar, wind.
            return Mathf.Max( plant.DesiredOutput(), plant.PowerOutput, 0 );
        }
        
        public float CurrentProduction( CompPowerTrader comp )
        {
            if ( !( comp is CompPowerPlant plant ) )
                return 0;

            if ( !plant.PowerOn )
                return 0;

            return Mathf.Max( plant.PowerOutput, 0 );
        }

        public float Consumption( CompPowerTrader comp )
        {
            if ( !comp.PowerOn && !FlickUtility.WantsToBeOn( comp.parent ) )
                return 0;

            return Mathf.Max( -comp.PowerOutput, 0f );
        }
    }
}