namespace BossMod.Components;

// generic 'exaflare' component - these mechanics are a bunch of moving aoes, with different lines either staggered or moving with different speed
public class Exaflare(BossModule module, AOEShape shape, ActionID aid = default) : GenericAOEs(module, aid, "GTFO from exaflare!")
{
    public class Line
    {
        public WPos Next;
        public WDir Advance;
        public Angle Rotation;
        public DateTime NextExplosion;
        public float TimeToMove;
        public int ExplosionsLeft;
        public int MaxShownExplosions;
    }

    public readonly AOEShape Shape = shape;
    public uint ImminentColor = Colors.Danger;
    public uint FutureColor = Colors.AOE;
    public readonly List<Line> Lines = [];

    public bool Active => Lines.Count != 0;

    public Exaflare(BossModule module, float radius, ActionID aid = new()) : this(module, new AOEShapeCircle(radius), aid) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
            return [];
        var futureAOEs = FutureAOEs(linesCount);
        var imminentAOEs = ImminentAOEs(linesCount);
        var futureCount = futureAOEs.Count;
        var imminentCount = imminentAOEs.Length;

        var aoes = new AOEInstance[futureCount + imminentCount];
        for (var i = 0; i < futureCount; ++i)
        {
            var aoe = futureAOEs[i];
            aoes[i] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, FutureColor);
        }

        for (var i = 0; i < imminentCount; ++i)
        {
            var aoe = imminentAOEs[i];
            aoes[futureCount + i] = new(Shape, aoe.Item1, aoe.Item3, aoe.Item2, ImminentColor);
        }
        return aoes;
    }

    protected (WPos, DateTime, Angle)[] ImminentAOEs(int count)
    {
        var exas = new (WPos, DateTime, Angle)[count];
        for (var i = 0; i < count; ++i)
        {
            var l = Lines[i];
            if (l.ExplosionsLeft != 0)
                exas[i] = (WPos.ClampToGrid(l.Next), l.NextExplosion, l.Rotation);
        }
        return exas;
    }

    protected List<(WPos, DateTime, Angle)> FutureAOEs(int count)
    {
        var exas = new List<(WPos, DateTime, Angle)>(count);
        var currentTime = WorldState.CurrentTime;
        for (var i = 0; i < count; ++i)
        {
            var l = Lines[i];
            var num = Math.Min(l.ExplosionsLeft, l.MaxShownExplosions);
            var pos = l.Next;
            var time = l.NextExplosion > currentTime ? l.NextExplosion : currentTime;
            for (var j = 1; j < num; ++j)
            {
                pos += l.Advance;
                time = time.AddSeconds(l.TimeToMove);
                exas.Add((WPos.ClampToGrid(pos), time, l.Rotation));
            }
        }
        return exas;
    }

    protected void AdvanceLine(Line l, WPos pos)
    {
        l.Next = pos + l.Advance;
        l.NextExplosion = WorldState.FutureTime(l.TimeToMove);
        --l.ExplosionsLeft;
    }
}
