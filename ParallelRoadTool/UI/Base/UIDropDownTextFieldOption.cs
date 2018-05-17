using System;
using ColossalFramework.UI;
using UnityEngine;

namespace NetworkSkins.UI
{
    public abstract class UIDropDownTextFieldOption : UIOption
    {
        private UILabel label;
        private string _description = String.Empty;
        protected UIDropDown DropDown { get; private set; }
        protected UITextField TextField { get; private set; }

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
            UITextField field;
            DropDown = UIUtil.CreateDropDownTextFieldWithLabel(out label, out field, this, Description, ParentWidth);
            DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;
            TextField = field;
            TextField.eventTextSubmitted += TextField_eventTextSubmitted;
        }

        protected override bool PopulateImpl()
        {
            var result = PopulateDropDown();
            isVisible = result;
            return result;
        }

        private void DropDown_eventSelectedIndexChanged(UIComponent component, int index)
        {
            if (Populating) return;

#if DEBUG
            Debug.LogFormat("Changing " + Description);
#endif
            OnSelectionChanged(index);
        }

        private void TextField_eventTextSubmitted(UIComponent component, string value)
        {
            if (Populating) return;

#if DEBUG
            Debug.LogFormat("Changing " + Description + " TextField");
#endif
            OnTextChanged(value);
        }

        protected abstract bool PopulateDropDown();

        protected abstract void OnSelectionChanged(int index);

        protected abstract void OnTextChanged(string value);
    }
}