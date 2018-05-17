using System.Collections.Generic;
using System.Reflection;

namespace ParallelRoadTool.Redirection
{
    public class Redirector<T>
    {
        private static Dictionary<MethodInfo, RedirectCallsState> _redirects;

        public static void Deploy()
        {
            if (IsDeployed())
            {
                return;
            }
            _redirects = RedirectionUtil.RedirectType(typeof (T));
        }

        public static void Revert()
        {
            if (!IsDeployed())
            {
                return;
            }
            RedirectionUtil.RevertRedirects(_redirects);
            _redirects.Clear();
        }

        public static bool IsDeployed()
        {
            return _redirects != null && _redirects.Count != 0;
        }
    }
}