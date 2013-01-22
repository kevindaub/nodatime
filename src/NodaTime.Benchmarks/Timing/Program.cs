// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NodaTime.Benchmarks.Timing
{
    /// <summary>
    /// Entry point for benchmarking.
    /// </summary>
    internal class Program
    {
        private const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static void Main(string[] args)
        {
            BenchmarkOptions options = BenchmarkOptions.FromCommandLine(args);

            var types =
                typeof(Program).Assembly.GetTypes().OrderBy<Type, string>(GetTypeDisplayName).Where(type => type.GetMethods(AllInstance).Any(IsBenchmark));

            var results = new List<BenchmarkResult>();
            foreach (Type type in types)
            {
                if (options.TypeFilter != null && type.Name != options.TypeFilter)
                {
                    continue;
                }

                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor == null)
                {
                    Console.WriteLine("Ignoring {0}: no public parameterless constructor", type.Name);
                    continue;
                }
                Console.WriteLine("Environment: CLR {0} on {1}", Environment.Version, Environment.OSVersion);
                Console.WriteLine("Running benchmarks in {0}", GetTypeDisplayName(type));
                object instance = ctor.Invoke(null);
                foreach (var method in type.GetMethods(AllInstance).Where(IsBenchmark))
                {
                    if (options.MethodFilter != null && method.Name != options.MethodFilter)
                    {
                        continue;
                    }

                    if (method.GetParameters().Length != 0)
                    {
                        Console.WriteLine("Ignoring {0}: method has parameters", method.Name);
                        continue;
                    }
                    BenchmarkResult result = RunBenchmark(method, instance, options);
                    Console.WriteLine("  " + result.ToString(options));
                    results.Add(result);
                }
            }
        }

        private static string GetTypeDisplayName(Type type)
        {
            return type.FullName.Replace("NodaTime.Benchmarks.", "");
        }

        private static BenchmarkResult RunBenchmark(MethodInfo method, object instance, BenchmarkOptions options)
        {
            var action = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
            // Start small, double until we've hit our warm-up time
            int iterations = 100;
            while (true)
            {
                Duration duration = RunTest(action, iterations, options.Timer);
                if (duration >= options.WarmUpTime)
                {
                    // Scale up the iterations to work out the full test time
                    double scale = ((double)options.TestTime.Ticks) / duration.Ticks;
                    double scaledIterations = scale * iterations;
                    // Make sure we never end up overflowing...
                    iterations = (int)Math.Min(scaledIterations, int.MaxValue - 1);
                    break;
                }
                // Make sure we don't end up overflowing due to doubling...
                if (iterations >= int.MaxValue / 2)
                {
                    break;
                }
                iterations *= 2;
            }
            Duration testDuration = RunTest(action, iterations, options.Timer);
            return new BenchmarkResult(method, iterations, testDuration);
        }

        private static Duration RunTest(Action action, int iterations, IBenchTimer timer)
        {
            PrepareForTest();
            timer.Reset();
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            return timer.ElapsedTime;
        }

        private static void PrepareForTest()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private static bool IsBenchmark(MethodInfo method)
        {
            return method.IsDefined(typeof(BenchmarkAttribute), false);
        }
    }
}
