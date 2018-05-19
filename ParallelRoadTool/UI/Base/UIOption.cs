using ColossalFramework.UI;

namespace ParallelRoadTool.UI.Base
{
    public abstract class UIOption : UIPanel
    {
        protected bool Populating { get; private set; }


        protected float ParentWidth => transform.parent.gameObject.GetComponent<UIComponent>().width;

        public bool Populate()
        {
            Populating = true;
            var result = PopulateImpl();
            Populating = false;
            return result;
        }

        protected abstract bool PopulateImpl();

        public override void Awake()
        {
            base.Awake();
            Initialize();

            width = ParentWidth;
            FitChildrenVertically();
        }

        protected abstract void Initialize();
    }
}