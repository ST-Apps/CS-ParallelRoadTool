using ColossalFramework.Math;

namespace ParallelRoadTool.Redirection
{
    public class Tuple<T1, T2>
    {
        public T1 First { get; }
        public T2 Second { get; }
        internal Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }

    public class Tuple<T1, T2, T3>
    {
        public T1 First { get; }
        public T2 Second { get; }
        public T3 Third { get; }

        internal Tuple(T1 first, T2 second, T3 third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }

    public static class Tuple
    {
        public static Tuple<T1, T2> New<T1, T2>(T1 first, T2 second)
        {
            var tuple = new Tuple<T1, T2>(first, second);
            return tuple;
        }

        public static Tuple<T1, T2, T3> New<T1, T2, T3>(T1 first, T2 second, T3 third)
        {
            var tuple = new Tuple<T1, T2, T3>(first, second, third);
            return tuple;
        }
    }
}