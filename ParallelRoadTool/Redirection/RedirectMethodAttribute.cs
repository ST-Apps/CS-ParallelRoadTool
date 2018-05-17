using System;

namespace ParallelRoadTool.Redirection
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    internal class RedirectMethodAttribute : Attribute
    {
        public RedirectMethodAttribute()
        {
            this.OnCreated = false;
        }

        public RedirectMethodAttribute(bool onCreated)
        {
            this.OnCreated = onCreated;
        }

        public bool OnCreated { get; private set; }
    }
}
