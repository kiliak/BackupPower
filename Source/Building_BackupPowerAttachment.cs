// Building_BackupPowerAttachment.cs
// Copyright Karel Kroeze, 2020-2020

using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BackupPower
{
    public enum BackupPowerStatus
    {
        Standby,
        Running,
        Error
    }

    public class Building_BackupPowerAttachment : Building
    {
        public  FloatRange           batteryRange = FloatRange.One;
        private Command_BatteryRange _command;
        private int                  _lastOnTick;

        private         Color         _prevColor;
        public override Color         DrawColor => Resources.StatusColor( Status );
        public          CompFlickable Flickable => Parent?.FlickableComp();

        public Building Parent { get; private set; }

        public PowerNet       PowerNet   => Parent?.PowerComp?.PowerNet;
        public CompPowerPlant PowerPlant => Parent?.PowerPlantComp();


        public BackupPowerStatus Status
        {
            get
            {
                if ( ( Parent?.BreakdownableComp()?.BrokenDown ?? false ) ||
                     ( !Parent?.RefuelableComp()?.HasFuel      ?? false ) )
                    return BackupPowerStatus.Error;
                if ( PowerPlant?.PowerOn ?? false )
                    return BackupPowerStatus.Running;
                return BackupPowerStatus.Standby;
            }
        }

        public bool CanTurnOff()
        {
            return _lastOnTick + BackupPower.Settings.MinimumOnTime < Find.TickManager.TicksGame;
        }

        public void CopySettingsTo( Building_BackupPowerAttachment other )
        {
            other.batteryRange = batteryRange;
        }

        public override void Destroy( DestroyMode mode = DestroyMode.Vanish )
        {
            try
            {
                MapComponent_PowerBroker.DeregisterBroker( this );
            }
            catch ( Exception err )
            {
                Verse.Log.Error( $"Error deregistering broker: {err}" );
            }

            base.Destroy( mode );
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look( ref batteryRange, "batteryRange", FloatRange.One );
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return _command;
            foreach ( var _gizmo in base.GetGizmos() )
                yield return _gizmo;
        }

        public override string GetInspectString()
        {
            var desc = base.GetInspectString();
            return I18n.StatusString( Status, batteryRange.min, batteryRange.max, PowerNet.StorageLevel() ) +
                   ( desc.NullOrEmpty() ? "" : $"\n{desc}" );
        }

        public override void Notify_ColorChanged()
        {
            base.Notify_ColorChanged();
            // again, for good measure.
            Map.mapDrawer.MapMeshDirty( Position, MapMeshFlag.Things );
            _prevColor = DrawColor;
        }

        public override void SpawnSetup( Map map, bool respawningAfterLoad )
        {
            base.SpawnSetup( map, respawningAfterLoad );
            _command = new Command_BatteryRange( this );

            if ( !respawningAfterLoad )
                TryAttach( Map );
        }

        public override void Tick()
        {
            if ( this.IsHashIntervalTick( 60 ) && _prevColor != DrawColor )
                Notify_ColorChanged();

            // TODO: think about refactoring this and hooking onto parents' Destroy() instead.
            base.Tick();
            if ( Parent.DestroyedOrNull() && !TryAttach( Map, true ) )
            {
                Messages.Message( I18n.AttachmentDestroyedBecauseParentGone( Parent?.Label ?? I18n.Generator ),
                                  MessageTypeDefOf.NegativeEvent,
                                  false );
                Destroy( DestroyMode.Refund );
            }
        }

        public void TurnOff()
        {
            Flickable.Force( false );
        }

        public void TurnOn()
        {
            _lastOnTick = Find.TickManager.TicksGame;
            Flickable.Force( true );
        }

        private bool TryAttach( Map map, bool reAttach = false )
        {
            Parent = Position.GetEdifice( map );
            var success = PowerPlant != null && Flickable != null;
            if ( success ) MapComponent_PowerBroker.RegisterBroker( this, reAttach );
            return success;
        }
    }
}