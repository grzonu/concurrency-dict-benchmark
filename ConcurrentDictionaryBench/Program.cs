using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ConcurrentDictionaryBench
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<ConcurrentDictionaryIterationBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class ConcurrentDictionaryIterationBenchmarks
    {
        private ConcurrentDictionary<int, int> _dict = null!;

        [GlobalSetup]
        public void Setup()
        {
            _dict = new ConcurrentDictionary<int, int>();

            var random = new Random(42);

            for (int i = 0; i < 10_000; i++)
            {
                _dict[random.Next()] = random.Next();
            }
        }

        [Benchmark]
        public long IterateViaValues()
        {
            long sum = 0;

            foreach (var value in _dict.Values.Where(x => x % 2 == 0))
            {
                sum += value;
            }

            return sum;
        }
        
        [Benchmark]
        public long IterateWithWhereViaValues()
        {
            long sum = 0;

            foreach (var value in _dict.Where(x => x.Value % 2 == 0).Select(x => x.Value))
            {
                sum += value;
            }

            return sum;
        }

        [Benchmark]
        public long IterateViaEnumerator()
        {
            long sum = 0;
            var enumerator = _dict.GetEnumerator();

            while(enumerator.MoveNext())
            {
                if (enumerator.Current.Value % 2 == 0)
                {
                    sum += enumerator.Current.Value;
                }
            }

            return sum;
        }
        
        [Benchmark]
        public async Task<long> IterateParallelViaValues()
        {
            var tasks = Enumerable.Range(0, 2)
                .Select(x => Task.Run(() =>
                {
                    long sum = 0;

                    foreach (var value in _dict.Values.Where(x => x % 2 == 0))
                    {
                        sum += value;
                    }

                    return sum;
                }));
            var total = (await Task.WhenAll(tasks)).Sum();
            return total;
        }
        
        [Benchmark]
        public async Task<long> IterateParallelWithWhereViaValues()
        {
            var tasks = Enumerable.Range(0, 2)
                .Select(x => Task.Run(() =>
                {
                    long sum = 0;

                    foreach (var value in _dict.Where(x => x.Value % 2 == 0).Select(x => x.Value))
                    {
                        sum += value;
                    }

                    return sum;
                }));
            var total = (await Task.WhenAll(tasks)).Sum();
            return total;
        }
        
        [Benchmark]
        public async Task<long> IterateParallelViaEnumerator()
        {
            var tasks = Enumerable.Range(0, 2)
                .Select(x => Task.Run(() =>
                {
                    long sum = 0;
                    var enumerator = _dict.GetEnumerator();

                    while(enumerator.MoveNext())
                    {
                        if (enumerator.Current.Value % 2 == 0)
                        {
                            sum += enumerator.Current.Value;
                        }
                    }

                    return sum;
                }));
            var total = (await Task.WhenAll(tasks)).Sum();
            return total;
        }
    }
}