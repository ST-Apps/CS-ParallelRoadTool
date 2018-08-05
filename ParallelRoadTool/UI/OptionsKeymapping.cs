using System.Reflection;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class OptionsKeymapping : UICustomControl
    {
        private static readonly string kKeyBindingTemplate = "KeyBindingTemplate";

        public static readonly SavedInputKey toggleParallelRoads = new SavedInputKey("toggleParallelRoads",
            Configuration.SettingsFileName, SavedInputKey.Encode(KeyCode.P, true, false, false), true);

        public static readonly SavedInputKey increaseHorizontalOffset = new SavedInputKey("increaseHorizontalOffset",
            Configuration.SettingsFileName, SavedInputKey.Encode(KeyCode.Equals, true, false, false), true);

        public static readonly SavedInputKey decreaseHorizontalOffset = new SavedInputKey("decreaseHorizontalOffset",
            Configuration.SettingsFileName, SavedInputKey.Encode(KeyCode.Minus, true, false, false), true);

        public static readonly SavedInputKey increaseVerticalOffset = new SavedInputKey("increaseVerticalOffset",
            Configuration.SettingsFileName, SavedInputKey.Encode(KeyCode.Equals, true, true, false), true);

        public static readonly SavedInputKey decreaseVerticalOffset = new SavedInputKey("decreaseVerticalOffset",
            Configuration.SettingsFileName, SavedInputKey.Encode(KeyCode.Minus, true, true, false), true);

        private int count;

        private SavedInputKey m_EditingBinding;

        private string m_EditingBindingCategory;

        private void Awake()
        {
            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ToolToggleButton"), toggleParallelRoads);
            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DecreaseHorizontalOffsetOption"), decreaseHorizontalOffset);
            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "IncreaseHorizontalOffsetOption"), increaseHorizontalOffset);
            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DecreaseVerticalOffsetOption"), decreaseVerticalOffset);
            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "IncreaseVerticalOffsetOption"), increaseVerticalOffset);
        }

        private void AddKeymapping(string label, SavedInputKey savedInputKey)
        {
            var uIPanel =
                component.AttachUIComponent(UITemplateManager.GetAsGameObject(kKeyBindingTemplate)) as UIPanel;
            if (count++ % 2 == 1) uIPanel.backgroundSprite = null;

            var uILabel = uIPanel.Find<UILabel>("Name");
            var uIButton = uIPanel.Find<UIButton>("Binding");
            uIButton.eventKeyDown += OnBindingKeyDown;
            uIButton.eventMouseDown += OnBindingMouseDown;

            uILabel.text = label;
            uIButton.text = savedInputKey.ToLocalizedString("KEYNAME");
            uIButton.objectUserData = savedInputKey;
        }

        private void OnEnable()
        {
            LocaleManager.eventLocaleChanged += OnLocaleChanged;
        }

        private void OnDisable()
        {
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;
        }

        private void OnLocaleChanged()
        {
            RefreshBindableInputs();
        }

        private bool IsModifierKey(KeyCode code)
        {
            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift ||
                   code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        }

        private bool IsControlDown()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private bool IsAltDown()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        private bool IsUnbindableMouseButton(UIMouseButton code)
        {
            return code == UIMouseButton.Left || code == UIMouseButton.Right;
        }

        private KeyCode ButtonToKeycode(UIMouseButton button)
        {
            if (button == UIMouseButton.Left) return KeyCode.Mouse0;
            if (button == UIMouseButton.Right) return KeyCode.Mouse1;
            if (button == UIMouseButton.Middle) return KeyCode.Mouse2;
            if (button == UIMouseButton.Special0) return KeyCode.Mouse3;
            if (button == UIMouseButton.Special1) return KeyCode.Mouse4;
            if (button == UIMouseButton.Special2) return KeyCode.Mouse5;
            if (button == UIMouseButton.Special3) return KeyCode.Mouse6;
            return KeyCode.None;
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (m_EditingBinding != null && !IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();
                var keycode = p.keycode;
                var inputKey = p.keycode == KeyCode.Escape
                    ? m_EditingBinding.value
                    : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if (p.keycode == KeyCode.Backspace) inputKey = SavedInputKey.Empty;
                m_EditingBinding.value = inputKey;
                var uITextComponent = p.source as UITextComponent;
                uITextComponent.text = m_EditingBinding.ToLocalizedString("KEYNAME");
                m_EditingBinding = null;
                m_EditingBindingCategory = string.Empty;
            }
        }

        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (m_EditingBinding == null)
            {
                p.Use();
                m_EditingBinding = (SavedInputKey)p.source.objectUserData;
                m_EditingBindingCategory = p.source.stringUserData;
                var uIButton = p.source as UIButton;
                uIButton.buttonsMask = UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle |
                                       UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 |
                                       UIMouseButton.Special3;
                uIButton.text = "Press any key";
                p.source.Focus();
                UIView.PushModal(p.source);
            }
            else if (!IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();
                var inputKey = SavedInputKey.Encode(ButtonToKeycode(p.buttons), IsControlDown(), IsShiftDown(),
                    IsAltDown());

                m_EditingBinding.value = inputKey;
                var uIButton2 = p.source as UIButton;
                uIButton2.text = m_EditingBinding.ToLocalizedString("KEYNAME");
                uIButton2.buttonsMask = UIMouseButton.Left;
                m_EditingBinding = null;
                m_EditingBindingCategory = string.Empty;
            }
        }

        private void RefreshBindableInputs()
        {
            foreach (var current in component.GetComponentsInChildren<UIComponent>())
            {
                var uITextComponent = current.Find<UITextComponent>("Binding");
                if (uITextComponent != null)
                {
                    var savedInputKey = uITextComponent.objectUserData as SavedInputKey;
                    if (savedInputKey != null) uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
                }

                var uILabel = current.Find<UILabel>("Name");
                if (uILabel != null) uILabel.text = Locale.Get("KEYMAPPING", uILabel.stringUserData);
            }
        }

        internal InputKey GetDefaultEntry(string entryName)
        {
            var field = typeof(DefaultSettings).GetField(entryName, BindingFlags.Static | BindingFlags.Public);
            if (field == null) return 0;
            var value = field.GetValue(null);
            if (value is InputKey) return (InputKey)value;
            return 0;
        }

        private void RefreshKeyMapping()
        {
            foreach (var current in component.GetComponentsInChildren<UIComponent>())
            {
                var uITextComponent = current.Find<UITextComponent>("Binding");
                var savedInputKey = (SavedInputKey)uITextComponent.objectUserData;
                if (m_EditingBinding != savedInputKey)
                    uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
            }
        }
    }
}