//using System.Reflection;
//using ColossalFramework;
//using ColossalFramework.Globalization;
//using ColossalFramework.UI;
//using UnityEngine;

//namespace ParallelRoadTool.UI.Utils
//{
//    // ReSharper disable once ClassNeverInstantiated.Global
//    public class OptionsKeymapping : UICustomControl
//    {
//        private const string KKeyBindingTemplate = "KeyBindingTemplate";

//        public static readonly SavedInputKey ToggleParallelRoads = new("toggleParallelRoads",
//                                                                       Configuration.SettingsFileName,
//                                                                       SavedInputKey.Encode(KeyCode.P, true, false, false), true);

//        public static readonly SavedInputKey IncreaseHorizontalOffset = new("increaseHorizontalOffset",
//                                                                            Configuration.SettingsFileName,
//                                                                            SavedInputKey.Encode(KeyCode.Equals, true, false, false),
//                                                                            true);

//        public static readonly SavedInputKey DecreaseHorizontalOffset = new("decreaseHorizontalOffset",
//                                                                            Configuration.SettingsFileName,
//                                                                            SavedInputKey.Encode(KeyCode.Minus, true, false, false),
//                                                                            true);

//        public static readonly SavedInputKey IncreaseVerticalOffset = new("increaseVerticalOffset",
//                                                                          Configuration.SettingsFileName,
//                                                                          SavedInputKey.Encode(KeyCode.Equals, true, true, false),
//                                                                          true);

//        public static readonly SavedInputKey DecreaseVerticalOffset = new("decreaseVerticalOffset",
//                                                                          Configuration.SettingsFileName,
//                                                                          SavedInputKey.Encode(KeyCode.Minus, true, true, false), true);

//        private int _count;

//        private SavedInputKey _mEditingBinding;

//        private void Awake()
//        {
//            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ToolToggleButton"),
//                          ToggleParallelRoads);
//            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DecreaseHorizontalOffsetOption"),
//                          DecreaseHorizontalOffset);
//            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "IncreaseHorizontalOffsetOption"),
//                          IncreaseHorizontalOffset);
//            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "DecreaseVerticalOffsetOption"),
//                          DecreaseVerticalOffset);
//            AddKeymapping(Locale.Get($"{Configuration.ResourcePrefix}TEXTS", "IncreaseVerticalOffsetOption"),
//                          IncreaseVerticalOffset);
//        }

//        private void AddKeymapping(string label, SavedInputKey savedInputKey)
//        {
//            var uIPanel =
//                component.AttachUIComponent(UITemplateManager.GetAsGameObject(KKeyBindingTemplate)) as UIPanel;
//            if (_count++ % 2 == 1) uIPanel.backgroundSprite = null;

//            var uILabel = uIPanel.Find<UILabel>("Name");
//            var uIButton = uIPanel.Find<UIButton>("Binding");
//            uIButton.eventKeyDown += OnBindingKeyDown;
//            uIButton.eventMouseDown += OnBindingMouseDown;

//            uILabel.text = label;
//            uIButton.text = savedInputKey.ToLocalizedString("KEYNAME");
//            uIButton.objectUserData = savedInputKey;
//        }

//        private void OnEnable()
//        {
//            LocaleManager.eventLocaleChanged += OnLocaleChanged;
//        }

//        private void OnDisable()
//        {
//            LocaleManager.eventLocaleChanged -= OnLocaleChanged;
//        }

//        private void OnLocaleChanged()
//        {
//            RefreshBindableInputs();
//        }

//        private bool IsModifierKey(KeyCode code)
//        {
//            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift ||
//                   code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
//        }

//        private bool IsControlDown()
//        {
//            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
//        }

//        private bool IsShiftDown()
//        {
//            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
//        }

//        private bool IsAltDown()
//        {
//            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
//        }

//        private bool IsUnbindableMouseButton(UIMouseButton code)
//        {
//            return code == UIMouseButton.Left || code == UIMouseButton.Right;
//        }

//        private KeyCode ButtonToKeycode(UIMouseButton button)
//        {
//            if (button == UIMouseButton.Left) return KeyCode.Mouse0;
//            if (button == UIMouseButton.Right) return KeyCode.Mouse1;
//            if (button == UIMouseButton.Middle) return KeyCode.Mouse2;
//            if (button == UIMouseButton.Special0) return KeyCode.Mouse3;
//            if (button == UIMouseButton.Special1) return KeyCode.Mouse4;
//            if (button == UIMouseButton.Special2) return KeyCode.Mouse5;
//            if (button == UIMouseButton.Special3) return KeyCode.Mouse6;
//            return KeyCode.None;
//        }

//        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
//        {
//            if (_mEditingBinding != null && !IsModifierKey(p.keycode))
//            {
//                p.Use();
//                UIView.PopModal();
//                var keycode = p.keycode;
//                var inputKey = p.keycode == KeyCode.Escape
//                                   ? _mEditingBinding.value
//                                   : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
//                if (p.keycode == KeyCode.Backspace) inputKey = SavedInputKey.Empty;
//                _mEditingBinding.value = inputKey;
//                var uITextComponent = p.source as UITextComponent;
//                uITextComponent.text = _mEditingBinding.ToLocalizedString("KEYNAME");
//                _mEditingBinding = null;
//            }
//        }

//        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
//        {
//            if (_mEditingBinding == null)
//            {
//                p.Use();
//                _mEditingBinding = (SavedInputKey)p.source.objectUserData;
//                var uIButton = p.source as UIButton;
//                uIButton.buttonsMask = UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle |
//                                       UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 |
//                                       UIMouseButton.Special3;
//                uIButton.text = "Press any key";
//                p.source.Focus();
//                UIView.PushModal(p.source);
//            }
//            else if (!IsUnbindableMouseButton(p.buttons))
//            {
//                p.Use();
//                UIView.PopModal();
//                var inputKey = SavedInputKey.Encode(ButtonToKeycode(p.buttons), IsControlDown(), IsShiftDown(),
//                                                    IsAltDown());

//                _mEditingBinding.value = inputKey;
//                var uIButton2 = p.source as UIButton;
//                uIButton2.text = _mEditingBinding.ToLocalizedString("KEYNAME");
//                uIButton2.buttonsMask = UIMouseButton.Left;
//                _mEditingBinding = null;
//            }
//        }

//        private void RefreshBindableInputs()
//        {
//            foreach (var current in component.GetComponentsInChildren<UIComponent>())
//            {
//                var uITextComponent = current.Find<UITextComponent>("Binding");
//                if (uITextComponent != null)
//                {
//                    var savedInputKey = uITextComponent.objectUserData as SavedInputKey;
//                    if (savedInputKey != null) uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
//                }

//                var uILabel = current.Find<UILabel>("Name");
//                if (uILabel != null) uILabel.text = Locale.Get("KEYMAPPING", uILabel.stringUserData);
//            }
//        }

//        internal InputKey GetDefaultEntry(string entryName)
//        {
//            var field = typeof(DefaultSettings).GetField(entryName, BindingFlags.Static | BindingFlags.Public);
//            if (field == null) return 0;
//            var value = field.GetValue(null);
//            if (value is InputKey key) return key;
//            return 0;
//        }

//        private void RefreshKeyMapping()
//        {
//            foreach (var current in component.GetComponentsInChildren<UIComponent>())
//            {
//                var uITextComponent = current.Find<UITextComponent>("Binding");
//                var savedInputKey = (SavedInputKey)uITextComponent.objectUserData;
//                if (_mEditingBinding != savedInputKey)
//                    uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
//            }
//        }
//    }
//}
