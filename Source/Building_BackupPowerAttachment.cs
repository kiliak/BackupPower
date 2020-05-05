// Building_BackupPowerAttachment.cs
// Copyright Karel Kroeze, -2020

using RimWorld;
using UnityEngine;
using Verse;
using System;

namespace BackupPower
{
    public class Building_BackupPowerAttachment : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!respawningAfterLoad)
                TryAttach(Map);
        }

        private static readonly Color StandByColor = new Color(5 / 255f, 150 / 255f, 251 / 255f, 1f);
        private static readonly Color ActiveColor = new Color(86 / 255f, 255 / 255f, 100 / 255f, 1f);
        private static readonly Color ErrorColor = new Color(237 / 255f, 67 / 255f, 55 / 255f, 1f);

        public override Color DrawColor
        {
            get
            {
                if ((Parent?.BreakdownableComp()?.BrokenDown ?? false) ||
                     (!Parent?.RefuelableComp()?.HasFuel ?? false))
                    return ErrorColor;
                if (PowerPlant?.PowerOn ?? false)
                    return ActiveColor;
                return StandByColor;
            }
        }

        private bool TryAttach(Map map, bool reAttach = false)
        {
            Parent = Position.GetEdifice(map);
            var success = PowerPlant != null && Flickable != null;
            if (success) MapComponent_PowerBroker.RegisterBroker(this, reAttach);
            return success;
        }

        private Color _prevColor;

        public override void Notify_ColorChanged()
        {
            base.Notify_ColorChanged();
            // again, for good measure.
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);

            Log.Debug($"changing color to: {DrawColor} (prev: {_prevColor}");
            _prevColor = DrawColor;
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(60) && _prevColor != DrawColor)
                Notify_ColorChanged();

            // TODO: think about refactoring this and hooking onto parents' Destroy() instead.
            base.Tick();
            if (Parent.DestroyedOrNull() && !TryAttach(Map, true))
            {
                Messages.Message(I18n.AttachmentDestroyedBecauseParentGone(Parent?.Label ?? I18n.Generator), MessageTypeDefOf.NegativeEvent,
                                  false);
                Destroy(DestroyMode.Refund);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            try
            {
                MapComponent_PowerBroker.DeregisterBroker(this);
            }
            catch (Exception err)
            {
                Verse.Log.Error($"Error deregistering broker: {err}");
            }
            base.Destroy(mode);
        }

        public void TurnOn()
        {
            _lastOnTick = Find.TickManager.TicksGame;
            Flickable.Force(true);
        }

        public bool CanTurnOff()
        {
            return _lastOnTick + BackupPower.Settings.MinimumOnTime < Find.TickManager.TicksGame;
        }

        public void TurnOff()
        {
            Flickable.Force(false);
        }

        private Building _parent;
        private int _lastOnTick;

        public Building Parent
        {
            get => _parent;
            private set => _parent = value;
        }

        public PowerNet PowerNet => Parent?.PowerComp?.PowerNet;
        public CompFlickable Flickable => Parent?.FlickableComp();
        public CompPowerPlant PowerPlant => Parent?.PowerPlantComp();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref _parent, "parent");
        }
    }
}