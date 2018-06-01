using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.UI.Base
{
    public abstract class UIDropDownTextFieldOption : UIOption
    {
        protected UITextField TextField { get; private set; }
        protected UIDropDown DropDown { get; private set; }
        protected UIButton DeleteButton { get; private set; }

        protected string Description { get; set; } = string.Empty;

        protected override void Initialize()
        {
            DropDown = UIUtil.CreateDropDownTextFieldWithLabel(out var deleteButton, out var textField, this,
                Description, ParentWidth);
            DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;

            TextField = textField;
            TextField.eventTextSubmitted += TextField_eventTextSubmitted;

            DeleteButton = deleteButton;
            DeleteButton.eventClicked += DeleteButton_eventClicked;
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

        private void DeleteButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (Populating) return;

#if DEBUG
            Debug.LogFormat("Changing " + Description);
#endif
            OnDeleteButtonClicked();
        }

        protected abstract bool PopulateDropDown();

        protected abstract void OnSelectionChanged(int index);

        protected abstract void OnTextChanged(string value);

        protected abstract void OnDeleteButtonClicked();
    }
}