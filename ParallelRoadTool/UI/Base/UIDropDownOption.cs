using System;
using ColossalFramework.UI;
using UnityEngine;

namespace NetworkSkins.UI
{
    public abstract class UIDropDownOption : UIOption
    {
        private UILabel label;
        private string _description = String.Empty;
        protected UIDropDown DropDown { get; private set; }

        protected string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                if (label != null) label.text = value;
            }
        }

        protected override void Initialize()
        {
            DropDown = UIUtil.CreateDropDownWithLabel(out label, this, Description, ParentWidth);
            DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;
        }

        protected override bool PopulateImpl()
        {
            var result = PopulateDropDown();
            isVisible = result;
            return result;
        }

        public void DropDown_eventSelectedIndexChanged(UIComponent component, int index)
        {
            if (Populating) return;

#if DEBUG
            Debug.LogFormat("Changing " + Description);
#endif
            OnSelectionChanged(index);
        }

        protected abstract bool PopulateDropDown();

        protected abstract void OnSelectionChanged(int index);
    }
}
