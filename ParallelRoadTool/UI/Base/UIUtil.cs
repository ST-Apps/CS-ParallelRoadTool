using System;
using System.Text;
using System.Text.RegularExpressions;
using ColossalFramework.UI;
using ICities;
using JetBrains.Annotations;
using UnityEngine;

namespace NetworkSkins.UI
{
    public static class UIUtil
    {
        private static readonly string kSliderTemplate = "OptionsSliderTemplate";

        private const float LABEL_RELATIVE_WIDTH = .25f;
        private const float COLUMN_PADDING = 5f;

        public static UIDropDown CreateDropDownWithLabel(out UILabel label, UIComponent parent, string labelText, float width)
        {
            var labelWidth = Mathf.Round(width * LABEL_RELATIVE_WIDTH);

            var dropDown = UIUtil.CreateDropDown(parent);
            dropDown.relativePosition = new Vector3(labelWidth + COLUMN_PADDING, 0);
            dropDown.width = width - labelWidth - COLUMN_PADDING;

            label = AddLabel(parent, labelText, labelWidth, dropDown.height);

            return dropDown;
        }


        public static UIDropDown CreateDropDownTextFieldWithLabel(out UILabel label, out UITextField textField, UIComponent parent, string labelText, float width)
        {
            var labelWidth = Mathf.Round(width * LABEL_RELATIVE_WIDTH);
            var textFieldWidth = 35f;
            var dropDownWidth = width - labelWidth - textFieldWidth - 2 * COLUMN_PADDING;


            var dropDown = UIUtil.CreateDropDown(parent);
            dropDown.relativePosition = new Vector3(labelWidth + COLUMN_PADDING, 0);
            dropDown.width = dropDownWidth;

            textField = UIUtil.CreateTextField(parent);
            textField.relativePosition = new Vector3(labelWidth + dropDownWidth + 2 * COLUMN_PADDING, 0);
            textField.width = textFieldWidth;
            textField.height = dropDown.height;

            label = AddLabel(parent, labelText, labelWidth, dropDown.height);

            return dropDown;
        }

        private static UILabel AddLabel(UIComponent parent, string text, float width, float dropDownHeight)
        {
            var label = parent.AddUIComponent<UILabel>();
            label.text = text;
            label.textScale = .85f;
            label.textColor = new Color32(200, 200, 200, 255);
            label.autoSize = false;
            label.autoHeight = true;
            label.width = width;
            label.textAlignment = UIHorizontalAlignment.Right;
            label.relativePosition = new Vector3(0, Mathf.Round((dropDownHeight - label.height) / 2));

            return label;
        }

        private static UITextField CreateTextField(UIComponent parent)
        {
            var textField = parent.AddUIComponent<UITextField>();

            textField.size = new Vector2(90f, 20f);
            textField.padding = new RectOffset(0, 0, 7, 0);
            textField.builtinKeyNavigation = true;
            textField.isInteractive = true;
            textField.readOnly = false;
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.selectionSprite = "EmptySprite";
            textField.selectionBackgroundColor = new Color32(0, 172, 234, 255);
            textField.normalBgSprite = "TextFieldPanel";
            textField.hoveredBgSprite = "TextFieldPanelHovered";
            textField.focusedBgSprite = "TextFieldPanelHovered";
            textField.textColor = new Color32(0, 0, 0, 255);
            textField.disabledTextColor = new Color32(0, 0, 0, 128);
            textField.color = new Color32(255, 255, 255, 255);
            textField.eventGotFocus += (component, param) => component.color = new Color32(253, 227, 144, 255);
            textField.eventLostFocus += (component, param) => component.color = new Color32(255, 255, 255, 255);
            return textField;
        }

        public static UIDropDown CreateDropDown(UIComponent parent)
        {
            var dropDown = parent.AddUIComponent<UIDropDown>();
            dropDown.size = new Vector2(90f, 30f);
            dropDown.listBackground = "GenericPanelLight";
            dropDown.itemHeight = 25;
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.normalBgSprite = "ButtonMenu";
            dropDown.disabledBgSprite = "ButtonMenuDisabled";
            dropDown.hoveredBgSprite = "ButtonMenuHovered";
            dropDown.focusedBgSprite = "ButtonMenu";
            dropDown.listWidth = 90;
            dropDown.listHeight = 300;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.popupColor = new Color32(45, 52, 61, 255);
            dropDown.popupTextColor = new Color32(170, 170, 170, 255);
            dropDown.zOrder = 1;
            dropDown.textScale = 0.8f;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.selectedIndex = 0;
            dropDown.textFieldPadding = new RectOffset(8, 0, 8, 0);
            dropDown.itemPadding = new RectOffset(14, 0, 8, 0);
            dropDown.listPosition = UIDropDown.PopupListPosition.Above;

            var button = dropDown.AddUIComponent<UIButton>();
            dropDown.triggerButton = button;
            button.text = "";
            button.size = dropDown.size;
            button.relativePosition = new Vector3(0f, 0f);
            button.textVerticalAlignment = UIVerticalAlignment.Middle;
            button.textHorizontalAlignment = UIHorizontalAlignment.Left;
            button.normalFgSprite = "IconDownArrow";
            button.hoveredFgSprite = "IconDownArrowHovered";
            button.pressedFgSprite = "IconDownArrowPressed";
            button.focusedFgSprite = "IconDownArrowFocused";
            button.disabledFgSprite = "IconDownArrowDisabled";
            button.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            button.horizontalAlignment = UIHorizontalAlignment.Right;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.zOrder = 0;
            button.textScale = 0.8f;

            dropDown.eventSizeChanged += (c, t) =>
            {
                button.size = t; dropDown.listWidth = (int)t.x;
            };

            return dropDown;
        }

        /*
        public static UISlider CreatSliderWithLabel(out UILabel label, UIComponent parent, string labelText, float width)
        {
            var labelWidth = Mathf.Round(width * LABEL_RELATIVE_WIDTH);

            var slider = UIUtil.CreateSlider(parent);
            slider.relativePosition = new Vector3(labelWidth + COLUMN_PADDING, 0);
            slider.width = width - labelWidth - COLUMN_PADDING;

            label = AddLabel(parent, labelText, labelWidth, dropDown.height);

            return slider;
        }*/

        public static UIPanel CreateSlider(UIComponent parent, string text, float min, float max, float step, float defaultValue, [NotNull] OnValueChanged eventCallback)
        {
            if (eventCallback == null) throw new ArgumentNullException(nameof(eventCallback));

            UIPanel uIPanel = parent.AttachUIComponent(UITemplateManager.GetAsGameObject(kSliderTemplate)) as UIPanel;
            uIPanel.position = Vector3.zero;

            uIPanel.Find<UILabel>("Label").text = text;

            UISlider uISlider = uIPanel.Find<UISlider>("Slider");
            uISlider.minValue = min;
            uISlider.maxValue = max;
            uISlider.stepSize = step;
            uISlider.value = defaultValue;
            uISlider.eventValueChanged += delegate (UIComponent c, float val)
            {
                eventCallback(val);
            };
            return uIPanel;
        }


        public static string GenerateBeautifiedNetName(this NetInfo prefab)
        {
            string itemName;

            if (prefab == null)
            {
                itemName = "Same as selected road";
            }
            else
            {
                itemName = prefab.GetUncheckedLocalizedTitle();

                /*
                var index1 = itemName.IndexOf('.');
                if (index1 > -1) itemName = itemName.Substring(index1 + 1);

                var index2 = itemName.IndexOf("_Data", StringComparison.Ordinal);
                if (index2 > -1) itemName = itemName.Substring(0, index2);
                */

                // replace spaces at start and end
                itemName = itemName.Trim();

                // replace multiple spaces with one
                itemName = Regex.Replace(itemName, " {2,}", " ");

                //itemName = AddSpacesToSentence(itemName);
            }

            return itemName;
        }

        private static string AddSpacesToSentence(string text)
        {
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if (text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
