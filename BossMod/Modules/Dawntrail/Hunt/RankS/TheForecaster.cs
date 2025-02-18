namespace BossMod.Dawntrail.Hunt.RankS.TheForecaster;

public enum OID : uint
{
    Boss = 0x4397 // R6.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    GaleForceWinds = 38534, // Boss->self, 4.0s cast, range 40 width 40 rect
    BlizzardConditions = 38535, // Boss->self, 4.0s cast, range 40 width 5 cross
    Hyperelectricity = 38533, // Boss->self, 4.0s cast, range 10 circle
    WildfireConditions = 38532, // Boss->self, 4.0s cast, range 5-40 donut

    Forecast1 = 38521, // Boss->self, 7.0s cast, single-target (Hyperelectricity, Gale-force winds, Wildfires)
    Forecast2 = 38523, // Boss->self, 7.0s cast, single-target (Gale-force winds, Wildfires, Hyperelectricity)
    Forecast3 = 38520, // Boss->self, 7.0s cast, single-target (Wildfires, Hyperelectricity, Blizzards)
    Forecast4 = 38522, // Boss->self, 7.0s cast, single-target (Blizzard, Hyperelectricity, Gale-force winds)

    WeatherChannelFirstCircle = 38526, // Boss->self, 5.0s cast, range 10 circle
    WeatherChannelFirstRect = 38528, // Boss->self, 5.0s cast, range 40 width 40 rect
    WeatherChannelFirstDonut = 38524, // Boss->self, 5.0s cast, range 5-40 donut
    WeatherChannelFirstCross = 38530, // Boss->self, 5.0s cast, range 40 width 5 cross
    WeatherChannelRestCircle = 38527, // Boss->self, no cast, range 10 circle
    WeatherChannelRestRect = 38529, // Boss->self, no cast, range 40 width 40 rect
    WeatherChannelRestDonut = 38525, // Boss->self, no cast, range 5-40 donut
    WeatherChannelRestCross = 38531, // Boss->self, no cast, range 40 width 5 cross
    FloodConditions = 38536, // Boss->location, 3.0s cast, range 6 circle
    ClimateChange1 = 39128, // Boss->self, 3.0s cast, single-target (Blizzard replaces Gale-force winds)
    ClimateChange2 = 39125, // Boss->self, 3.0s cast, single-target (Wildfire replaces Blizzard)
    ClimateChange3 = 39127, // Boss->self, 3.0s cast, single-target (Gale-force winds replace Hyperelectricity)
    ClimateChange4 = 39126, // Boss->self, 3.0s cast, single-target (Hyperelectricity replaces Wildfire)

    ClimateStatusEffects = 38537, // Boss->self, no cast, single-target, boss resets its status effects
    ClimateChangeStatusEffect = 39133 // Boss->self, no cast, single-target
}

class FloodConditions(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FloodConditions), 6f);
class GaleForceWinds(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GaleForceWinds), new AOEShapeRect(40f, 20f));
class Hyperelectricity(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Hyperelectricity), 10f);
class WildfireConditions(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WildfireConditions), new AOEShapeDonut(5f, 40f));
class BlizzardConditions(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BlizzardConditions), new AOEShapeCross(40f, 2.5f));

class ForecastClimateChange(BossModule module) : Components.GenericAOEs(module)
{
    private enum Forecast { None, HGW, GWH, WHB, BHG }
    private Forecast currentForecast;
    private enum ClimateChange { None, G2B, B2W, H2G, W2H }
    private ClimateChange currentClimateChange;
    private static readonly AOEShapeRect rect = new(40f, 20f);
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeCross cross = new(40f, 2.5f);
    private readonly List<AOEInstance> _aoes = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Forecast1:
                currentForecast = Forecast.HGW;
                break;
            case (uint)AID.Forecast2:
                currentForecast = Forecast.GWH;
                break;
            case (uint)AID.Forecast3:
                currentForecast = Forecast.WHB;
                break;
            case (uint)AID.Forecast4:
                currentForecast = Forecast.BHG;
                break;
            case (uint)AID.ClimateChange1:
                currentClimateChange = ClimateChange.G2B;
                break;
            case (uint)AID.ClimateChange2:
                currentClimateChange = ClimateChange.B2W;
                break;
            case (uint)AID.ClimateChange3:
                currentClimateChange = ClimateChange.H2G;
                break;
            case (uint)AID.ClimateChange4:
                currentClimateChange = ClimateChange.W2H;
                break;
            case (uint)AID.WeatherChannelFirstCircle:
            case (uint)AID.WeatherChannelFirstCross:
            case (uint)AID.WeatherChannelFirstDonut:
            case (uint)AID.WeatherChannelFirstRect:
                AddAOEs(spell);
                break;
        }
    }

    private void AddAOEs(ActorCastInfo spell)
    {
        var position = Module.PrimaryActor.Position;
        AOEShape[] shapes =
        [
            AdjustShape(GetShapeForForecast(currentForecast, 0)),
            AdjustShape(GetShapeForForecast(currentForecast, 1)),
            AdjustShape(GetShapeForForecast(currentForecast, 2)),
        ];

        _aoes.Add(new(shapes[0], position, spell.Rotation, Module.CastFinishAt(spell)));
        _aoes.Add(new(shapes[1], position, spell.Rotation, Module.CastFinishAt(spell, 3.1f)));
        _aoes.Add(new(shapes[2], position, spell.Rotation, Module.CastFinishAt(spell, 6.1f)));
        currentClimateChange = ClimateChange.None;
    }

    private static AOEShape GetShapeForForecast(Forecast forecast, int index)
    {
        AOEShape[] hgwShapes = [circle, rect, donut];
        AOEShape[] gwhShapes = [rect, donut, circle];
        AOEShape[] whbShapes = [donut, circle, cross];
        AOEShape[] bhgShapes = [cross, circle, rect];

        return forecast switch
        {
            Forecast.HGW => hgwShapes[index],
            Forecast.GWH => gwhShapes[index],
            Forecast.WHB => whbShapes[index],
            Forecast.BHG => bhgShapes[index],
            _ => cross
        };
    }

    private AOEShape AdjustShape(AOEShape shape)
    {
        return shape switch
        {
            AOEShapeRect when currentClimateChange == ClimateChange.G2B => cross,
            AOEShapeCross when currentClimateChange == ClimateChange.B2W => donut,
            AOEShapeCircle when currentClimateChange == ClimateChange.H2G => rect,
            AOEShapeDonut when currentClimateChange == ClimateChange.W2H => circle,
            _ => shape
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.WeatherChannelFirstCircle:
                case (uint)AID.WeatherChannelFirstCross:
                case (uint)AID.WeatherChannelFirstDonut:
                case (uint)AID.WeatherChannelFirstRect:
                case (uint)AID.WeatherChannelRestCircle:
                case (uint)AID.WeatherChannelRestDonut:
                case (uint)AID.WeatherChannelRestRect:
                case (uint)AID.WeatherChannelRestCross:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class TheForecasterStates : StateMachineBuilder
{
    public TheForecasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ForecastClimateChange>()
            .ActivateOnEnter<WildfireConditions>()
            .ActivateOnEnter<BlizzardConditions>()
            .ActivateOnEnter<Hyperelectricity>()
            .ActivateOnEnter<FloodConditions>()
            .ActivateOnEnter<GaleForceWinds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13437)]
public class TheForecaster(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
