using System;
using System.Linq;
using System.Reflection;
using ActionStreetMap.Infrastructure.Diagnostic;

namespace ActionStreetMap.Infrastructure.Dependencies.Interception.Behaviors
{
    /// <summary>
    ///     This behavior logs methods signature and result call to output.
    /// </summary>
    public class TraceBehavior: ExecuteBehavior
    {
        private readonly ITrace _trace;

        /// <summary>
        ///     Creates TraceBehavior
        /// </summary>
        /// <param name="trace">Output trace.</param>
        public TraceBehavior(ITrace trace)
        {
            _trace = trace;
            Name = "trace";
        }

        /// <inheritdoc />
        public override IMethodReturn Invoke(MethodInvocation methodInvocation)
        {
            var methodName = String.Format("{0}.{1}({2})", 
                methodInvocation.Target.GetType(),
                methodInvocation.MethodBase.Name,
                methodInvocation.Parameters.Aggregate("", 
                    (ag, p) => ag + (ag != "" ? ", ": "") +
                    (p.Value != null? p.ToString(): "<null>")));

            _trace.Debug("interception." + Name,"invoke {0}", methodName);
            
            var result = methodInvocation.IsInvoked? methodInvocation.Return :  
                base.Invoke(methodInvocation);

            var resultString = "";
            var methodInfo = methodInvocation.MethodBase as MethodInfo;
            if (methodInfo != null && methodInfo.ReturnType != typeof(void))
                resultString += ": " + result.GetReturnValue();

            _trace.Debug("interception." + Name, "end {0}{1}", methodName, resultString);

            return result;
        }
    }
}
