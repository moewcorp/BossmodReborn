namespace BossMod.Shadowbringers.Alliance.A22SuperiorFlightUnits;

sealed class A22SuperiorFlightUnitsStates : StateMachineBuilder
{
    private readonly A22SuperiorFlightUnits _module;

    public A22SuperiorFlightUnitsStates(A22SuperiorFlightUnits module) : base(module)
    {
        _module = module;
        TrivialPhase()
            .ActivateOnEnter<ShieldProtocol>()
            .ActivateOnEnter<IncendiaryBombing>()
            .ActivateOnEnter<IncendiaryBombingBait>()
            .ActivateOnEnter<StandardSurfaceMissile>()
            .ActivateOnEnter<LethalRevolution>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<SurfaceMissileHighOrderExplosiveBlastCircle>()
            .ActivateOnEnter<HighOrderExplosiveBlastCross>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<PrecisionGuidedMissile>()
            .ActivateOnEnter<ManeuverHighPoweredLaser>()
            .ActivateOnEnter<ManeuverMissileCommand>()
            .ActivateOnEnter<SharpTurn>()
            .ActivateOnEnter<SlidingSwipe>()
            .ActivateOnEnter<IncendiaryBarrage>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.FlightUnitBEta()?.IsDeadOrDestroyed ?? true) && (_module.FlightUnitCHi()?.IsDeadOrDestroyed ?? true);
    }
}
