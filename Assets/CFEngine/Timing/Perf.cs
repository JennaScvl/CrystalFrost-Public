using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CrystalFrost.Timing
{

    /// <summary>
    /// Facilitates capturing code performance data
    /// </summary>
    public static class Perf
    {
        /// <summary>
        /// Set to False to enable collection of performance data.
        /// </summary>
        public static bool Disabled = true;

        // Two dictionaries instead of one that holds a class or a tuple is 
        // a deliberate choice. the update behavior of ConcurrentDictionary
        // behaves differently when replacing a value type, than when replacing a reference type.
        private static readonly ConcurrentDictionary<string, long> TotalTicks = new();
        private static readonly ConcurrentDictionary<string, long> Invokations = new();

        private static void Record(string category, long elapsedTicks)
        {
            TotalTicks.AddOrUpdate(category, elapsedTicks, (cat, current) => current +=elapsedTicks);
            Invokations.AddOrUpdate(category, 1, (cat, current) => current += 1);
        }

        /// <summary>
        /// Clears recorded performance data.
        /// </summary>
        public static void Reset()
        {
            TotalTicks.Clear();
            Invokations.Clear();
        }

        /// <summary>
        /// Outputs to a file in CSV Format.
        /// </summary>
        /// <param name="filename"></param>
        public static void DumpStats(string filename)
        {
            if (Disabled) return;
            using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            DumpStats(stream);
        }

        /// <summary>
        /// Outputs to a stream in CSV Format
        /// </summary>
        /// <param name="stream"></param>
        public static void DumpStats(Stream stream)
        {
            if (Disabled) return;
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.WriteLine("Category,Invokations,TotalTicks");
            foreach(var category in Invokations.Keys)
            {
                var invokations = Invokations[category];
                var totalTicks = TotalTicks[category];
                writer.Write(category);
                writer.Write(',');
                writer.Write(invokations.ToString());
                writer.Write(',');
                writer.WriteLine(totalTicks.ToString());
            }
        }

        /// <summary>
        /// Measures code that returns no value.
        /// </summary>
        public static void Measure(string category, Action action)
        {
            if (Disabled)
            {
                action();
                return;
            }
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            Record(category, sw.ElapsedTicks);
        }

        /// <summary>
        /// Measure code that returns a <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T Measure<T>(string category, Func<T> func)
        {
            if (Disabled) return func();
            var sw = Stopwatch.StartNew();
            var result = func();
            sw.Stop();
            Record(category, sw.ElapsedTicks);
            return result;
        }

        /// <summary>
        /// Measures a Task
        /// </summary>
        public static async Task Measure(string category, Func<Task> action)
        {
            if (Disabled)
            {
                await action();
                return;
            }
            var sw = Stopwatch.StartNew();
            await action();
            sw.Stop();
            Record(category, sw.ElapsedTicks);
        }

        /// <summary>
        /// Measures a Task<typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static async Task<T> Measure<T>(string category, Func<Task<T>> action)
        {
            if (Disabled) return await action();
            var sw = Stopwatch.StartNew();
            var result = await action();
            sw.Stop();
            Record(category, sw.ElapsedTicks);
            return result;
        }
    }
}
