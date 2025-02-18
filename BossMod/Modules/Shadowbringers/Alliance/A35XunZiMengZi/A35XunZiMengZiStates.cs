namespace BossMod.Shadowbringers.Alliance.A35XunZiMengZi;

class A35XunZiMengZiStates : StateMachineBuilder
{
    public A35XunZiMengZiStates(BossModule module) : base(module)
    {
        TrivialPhase()

            .ActivateOnEnter<DeployArmaments1>()
            .ActivateOnEnter<DeployArmaments2>()
            .ActivateOnEnter<DeployArmaments3>()
            .ActivateOnEnter<DeployArmaments4>()
            .ActivateOnEnter<UniversalAssault>();
    }
}
