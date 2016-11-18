using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FineRoadAnarchy
{
    public class OptionsKeymapping : UICustomControl
    {
        private static readonly string kKeyBindingTemplate = "KeyBindingTemplate";

        private SavedInputKey m_EditingBinding;

        private string m_EditingBindingCategory;

        public static readonly SavedInputKey toggleAnarchy = new SavedInputKey("toggleAnarchy", FineRoadAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.A, true, false, false), true);
        public static readonly SavedInputKey toggleBending = new SavedInputKey("toggleBending", FineRoadAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.B, true, false, false), true);
        public static readonly SavedInputKey toggleSnapping = new SavedInputKey("toggleSnapping", FineRoadAnarchy.settingsFileName, SavedInputKey.Encode(KeyCode.S, true, false, false), true);

        private int count = 0;

        private void Awake()
        {
            AddKeymapping("Toggle Anarchy", toggleAnarchy);
            AddKeymapping("Toggle Bending", toggleBending);
            AddKeymapping("Toggle Snapping", toggleSnapping);
        }

        private void AddKeymapping(string label, SavedInputKey savedInputKey)
        {
            UIPanel uIPanel = component.AttachUIComponent(UITemplateManager.GetAsGameObject(kKeyBindingTemplate)) as UIPanel;
            if (count++ % 2 == 1) uIPanel.backgroundSprite = null;

            UILabel uILabel = uIPanel.Find<UILabel>("Name");
            UIButton uIButton = uIPanel.Find<UIButton>("Binding");
            uIButton.eventKeyDown += new KeyPressHandler(this.OnBindingKeyDown);
            uIButton.eventMouseDown += new MouseEventHandler(this.OnBindingMouseDown);

            uILabel.text = label;
            uIButton.text = savedInputKey.ToLocalizedString("KEYNAME");
            uIButton.objectUserData = savedInputKey;
        }

        private void OnEnable()
        {
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        private void OnDisable()
        {
            LocaleManager.eventLocaleChanged -= new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        private void OnLocaleChanged()
        {
            this.RefreshBindableInputs();
        }

        private bool IsModifierKey(KeyCode code)
        {
            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift || code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
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
            if (button == UIMouseButton.Left)
            {
                return KeyCode.Mouse0;
            }
            if (button == UIMouseButton.Right)
            {
                return KeyCode.Mouse1;
            }
            if (button == UIMouseButton.Middle)
            {
                return KeyCode.Mouse2;
            }
            if (button == UIMouseButton.Special0)
            {
                return KeyCode.Mouse3;
            }
            if (button == UIMouseButton.Special1)
            {
                return KeyCode.Mouse4;
            }
            if (button == UIMouseButton.Special2)
            {
                return KeyCode.Mouse5;
            }
            if (button == UIMouseButton.Special3)
            {
                return KeyCode.Mouse6;
            }
            return KeyCode.None;
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (this.m_EditingBinding != null && !this.IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();
                KeyCode keycode = p.keycode;
                InputKey inputKey = (p.keycode == KeyCode.Escape) ? this.m_EditingBinding.value : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if (p.keycode == KeyCode.Backspace)
                {
                    inputKey = SavedInputKey.Empty;
                }
                this.m_EditingBinding.value = inputKey;
                UITextComponent uITextComponent = p.source as UITextComponent;
                uITextComponent.text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                this.m_EditingBinding = null;
                this.m_EditingBindingCategory = string.Empty;
            }
        }

        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (this.m_EditingBinding == null)
            {
                p.Use();
                this.m_EditingBinding = (SavedInputKey)p.source.objectUserData;
                this.m_EditingBindingCategory = p.source.stringUserData;
                UIButton uIButton = p.source as UIButton;
                uIButton.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                uIButton.text = "Press any key";
                p.source.Focus();
                UIView.PushModal(p.source);
            }
            else if (!this.IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();
                InputKey inputKey = SavedInputKey.Encode(this.ButtonToKeycode(p.buttons), this.IsControlDown(), this.IsShiftDown(), this.IsAltDown());

                this.m_EditingBinding.value = inputKey;
                UIButton uIButton2 = p.source as UIButton;
                uIButton2.text = this.m_EditingBinding.ToLocalizedString("KEYNAME");
                uIButton2.buttonsMask = UIMouseButton.Left;
                this.m_EditingBinding = null;
                this.m_EditingBindingCategory = string.Empty;
            }
        }

        private void RefreshBindableInputs()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                if (uITextComponent != null)
                {
                    SavedInputKey savedInputKey = uITextComponent.objectUserData as SavedInputKey;
                    if (savedInputKey != null)
                    {
                        uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
                    }
                }
                UILabel uILabel = current.Find<UILabel>("Name");
                if (uILabel != null)
                {
                    uILabel.text = Locale.Get("KEYMAPPING", uILabel.stringUserData);
                }
            }
        }

        internal InputKey GetDefaultEntry(string entryName)
        {
            FieldInfo field = typeof(DefaultSettings).GetField(entryName, BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                return 0;
            }
            object value = field.GetValue(null);
            if (value is InputKey)
            {
                return (InputKey)value;
            }
            return 0;
        }

        private void RefreshKeyMapping()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                SavedInputKey savedInputKey = (SavedInputKey)uITextComponent.objectUserData;
                if (this.m_EditingBinding != savedInputKey)
                {
                    uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
                }
            }
        }
    }
}