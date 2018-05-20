using System;
using System.Text;
using System.Text.RegularExpressions;
using ColossalFramework.UI;
using ICities;
using JetBrains.Annotations;
using UnityEngine;

namespace ParallelRoadTool.UI.Base
{
    public static class UIUtil
    {
        private const float COLUMN_PADDING = 5f;
        private const float TEXT_FIELD_WIDTH = 35f;
        private static readonly string kSliderTemplate = "OptionsSliderTemplate";

        public static UIDropDown CreateDropDownTextFieldWithLabel(out UIButton deleteButton, out UITextField textField,
            UIComponent parent, string labelText, float width)
        {
            
            var dropDownWidth = width - TEXT_FIELD_WIDTH - 5 * COLUMN_PADDING;

            textField = CreateTextField(parent);
            textField.relativePosition = new Vector3(COLUMN_PADDING, 8f);
            textField.width = TEXT_FIELD_WIDTH;

            var dropDown = CreateDropDown(parent);
            dropDown.relativePosition = new Vector3(TEXT_FIELD_WIDTH + 2 * COLUMN_PADDING, 8f);
            dropDown.width = dropDownWidth;

            textField.height = dropDown.height;

            // Buttons
            deleteButton = parent.AddUIComponent<UIButton>();
            deleteButton.text = "";
            deleteButton.tooltip = "Delete network";
            deleteButton.size = new Vector2(36, 36);
            deleteButton.relativePosition = new Vector3(0f, 0f);
            deleteButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            deleteButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            deleteButton.normalFgSprite = "buttonclose";
            deleteButton.hoveredFgSprite = "buttonclosehover";
            deleteButton.pressedFgSprite = "buttonclosepressed";
            deleteButton.focusedFgSprite = "buttonclosehover";
            deleteButton.disabledFgSprite = "buttonclose";
            deleteButton.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            deleteButton.horizontalAlignment = UIHorizontalAlignment.Right;
            deleteButton.verticalAlignment = UIVerticalAlignment.Middle;
            deleteButton.zOrder = 0;
            deleteButton.textScale = 0.8f;
            deleteButton.relativePosition = new Vector3(TEXT_FIELD_WIDTH + dropDown.width + 3 * COLUMN_PADDING, 4f);

            return dropDown;
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
                button.size = t;
                dropDown.listWidth = (int)t.x;
            };

            return dropDown;
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

                // replace spaces at start and end
                itemName = itemName.Trim();

                // replace multiple spaces with one
                itemName = Regex.Replace(itemName, " {2,}", " ");

                //itemName = AddSpacesToSentence(itemName);
            }

            return itemName;
        }

    }
}