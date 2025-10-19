namespace BossMod;

[SkipLocalsInit]
public static class SortHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortAOEByActorID(List<Components.GenericAOEs.AOEInstance> list)
    {
        RefSort.Sort(CollectionsMarshal.AsSpan(list), new AOEActorIDComparer());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortAOEByActivation(List<Components.GenericAOEs.AOEInstance> list)
    {
        RefSort.Sort(CollectionsMarshal.AsSpan(list), new AOEActivationComparer());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortForbiddenZonesByActivation(List<(ShapeDistance, DateTime, ulong)> list)
    {
        RefSort.Sort(CollectionsMarshal.AsSpan(list), new ForbiddenZonesActivationComparer());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SortForbiddenDirectionsByActivation(List<(Angle, Angle, DateTime)> list)
    {
        RefSort.Sort(CollectionsMarshal.AsSpan(list), new ForbiddenDirectionActivationComparer());
    }

    private readonly struct AOEActorIDComparer : IRefComparer<Components.GenericAOEs.AOEInstance>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Components.GenericAOEs.AOEInstance a, ref Components.GenericAOEs.AOEInstance b) => a.ActorID.CompareTo(b.ActorID);
    }

    private readonly struct AOEActivationComparer : IRefComparer<Components.GenericAOEs.AOEInstance>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Components.GenericAOEs.AOEInstance a, ref Components.GenericAOEs.AOEInstance b) => a.Activation.CompareTo(b.Activation);
    }

    private readonly struct ForbiddenZonesActivationComparer : IRefComparer<(ShapeDistance, DateTime, ulong)>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref (ShapeDistance, DateTime, ulong) a, ref (ShapeDistance, DateTime, ulong) b) => a.Item2.CompareTo(b.Item2);
    }

    private readonly struct ForbiddenDirectionActivationComparer : IRefComparer<(Angle, Angle, DateTime)>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref (Angle, Angle, DateTime) a, ref (Angle, Angle, DateTime) b) => a.Item3.CompareTo(b.Item3);
    }
}

[SkipLocalsInit]
public interface IRefComparer<T>
{
    int Compare(ref T a, ref T b);
}

[SkipLocalsInit]
public static class RefSort
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T, TComparer>(Span<T> span, TComparer comparer) where TComparer : struct, IRefComparer<T>
    {
        QuickSort(span, 0, span.Length - 1, comparer);
    }

    private static void QuickSort<T, TComparer>(Span<T> span, int left, int right, TComparer comparer) where TComparer : struct, IRefComparer<T>
    {
        while (left < right)
        {
            var pivot = Partition(span, left, right, comparer);
            if (pivot - left < right - pivot)
            {
                QuickSort(span, left, pivot - 1, comparer);
                left = pivot + 1;
            }
            else
            {
                QuickSort(span, pivot + 1, right, comparer);
                right = pivot - 1;
            }
        }
    }

    private static int Partition<T, TComparer>(Span<T> span, int left, int right, TComparer comparer) where TComparer : struct, IRefComparer<T>
    {
        ref var pivot = ref span[right];
        var storeIndex = left;

        for (var i = left; i < right; ++i)
        {
            if (comparer.Compare(ref span[i], ref pivot) < 0)
            {
                if (storeIndex != i)
                {
                    Swap(ref span[storeIndex], ref span[i]);
                }
                ++storeIndex;
            }
        }

        Swap(ref span[storeIndex], ref span[right]);
        return storeIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap<T>(ref T a, ref T b)
    {
        (b, a) = (a, b);
    }
}
