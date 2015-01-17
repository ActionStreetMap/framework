// defined from .NET Framework 4.5 and NETFX_CORE

namespace ActionStreetMap.Infrastructure.Reactive
{
    /// <summary />
    public interface IProgress<T>
    {
        /// <summary />
        void Report(T value);
    }
}