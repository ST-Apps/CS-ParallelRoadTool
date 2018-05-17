using System;
using ColossalFramework.UI;
using UnityEngine;

namespace NetworkSkins.UI
{
    public abstract class UISliderOption : UIOption
    {
        private string _description = String.Empty;

        private UIPanel panel;
        private UILabel label;
        protected UISlider Slider { get; private set; }

        protected string Description
        {
            get { return _description; }
            set {
                _description = value;
                if(label != null) label.text = value;
            }
        }

        protected override void Initialize()
        {
            panel = UIUtil.CreateSlider(this, _description, 0, 100, 1, 50, Slider_onValueChanged);
            label = panel.Find<UILabel>("Label");
            Slider = panel.Find<UISlider>("Slider");
        }

        protected override bool PopulateImpl()
        {
            var result = PopulateSlider();
            isVisible = result;
            return result;
        }

        private void Slider_onValueChanged(float val)
        {
            if (Populating) return;

#if DEBUG
            Debug.LogFormat("Changing " + Description);
#endif
            OnValueChanged(val);
        }

        protected abstract bool PopulateSlider();

        protected abstract void OnValueChanged(float val);
    }
}