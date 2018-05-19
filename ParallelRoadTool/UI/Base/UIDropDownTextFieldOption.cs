using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.UI.Base
{
    public abstract class UIDropDownTextFieldOption : UIOption
    {
        private string _description = string.Empty;
        private UILabel _label;
        protected UIDropDown DropDown { get; private set; }
        protected UITextField TextField { get; private set; }

        protected string Description
        {
            get => _description;
            set
            {
                _description = value;
                if (_label != null) _label.text = value;
            }
        }

        protected override void Initialize()
        {
            UITextField field;
            DropDown = UIUtil.CreateDropDownTextFieldWithLabel(out _label, out field, this, Description, ParentWidth);
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