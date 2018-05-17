using ColossalFramework.UI;

namespace NetworkSkins
{
    public abstract class UIOption : UIPanel
    {
        protected bool Populating { get; private set; }


        protected float ParentWidth => this.transform.parent.gameObject.GetComponent<UIComponent>().width;

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
            this.Initialize();

            this.width = ParentWidth;
            this.FitChildrenVertically();
        }

        protected abstract void Initialize();
    }
}
