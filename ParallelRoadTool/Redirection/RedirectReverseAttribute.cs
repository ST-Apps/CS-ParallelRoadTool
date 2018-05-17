using System;

namespace ParallelRoadTool.Redirection
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class RedirectReverseAttribute : Attribute
    {
        public RedirectReverseAttribute()
        {
            this.OnCreated = false;
        }

        public RedirectReverseAttribute(bool onCreated)
        {
            this.OnCreated = onCreated;
        }

        public bool OnCreated { get; private set; }
    }
}