using System;
using System.Diagnostics;
using System.Linq;
using ActionStreetMap.Infrastructure.Diagnostic;

namespace ActionStreetMap.Infrastructure.Dependencies.Interception.Behaviors
{
    /// <summary> Defines behavior for profiling. </summary>
    public class ProfileBehavior: IBehavior
    {
        private readonly ITrace _trace;

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <summary> Creates <see cref="ProfileBehavior"/>. </summary>
        /// <param name="trace">Output trace.</param>
        public ProfileBehavior(ITrace trace)
        {
            _trace = trace;
            Name = "profile";
        }

        /// <inheritdoc />
        public IMethodReturn Invoke(MethodInvocation methodInvocation)
        {
            var methodName = String.Format("{0}.{1}({2})",
                methodInvocation.Target.GetType(),
                methodInvocation.MethodBase.Name,
                methodInvocation.Parameters.Aggregate("",
                    (ag, p) => ag + (ag != "" ? ", " : "") +
                    (p.Value != null ? p.ToString() : "<null>")));

            if (methodInvocation.IsInvoked)
            {
                _trace.Warn("Interception." + Name, 
                    String.Format("Unable to profile {0} as method is already executed! " +
                                  "Please check your interception behavior configuration", methodName));
                return methodInvocation.Return;
            }

            var methodBase = TypeHelper.GetMethodBySign(methodInvocation.Target.GetType(),
                   methodInvocation.MethodBase, methodInvocation.GenericTypes.ToArray());

            var sw = new Stopwatch();
            sw.Start();
            var result = methodBase.Invoke(methodInvocation.Target, methodInvocation.Parameters.Values.ToArray());
            sw.Stop();
            _trace.Debug("interception." + Name, "{0}: {1} ms", methodName, sw.ElapsedMilliseconds.ToString());
            
            methodInvocation.IsInvoked = true;
            return methodInvocation.Return = new MethodReturn(result);
        }
    }
}
