using System;
using UnityEngine.Profiling;

namespace NavMeshMaps.UnityExtensions
{
    public class ProfilerSampleScope : IDisposable
    {
        public ProfilerSampleScope(string name)
        {
            Profiler.BeginSample(name);
        }

        public void Dispose()
        {
            Profiler.EndSample();
        }
    }
}
