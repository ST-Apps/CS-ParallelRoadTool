using System;
using System.Text.RegularExpressions;
using ColossalFramework.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ParallelRoadTool.UI.Base
{
    public static class UIUtil
    {
        [Flags]
        public enum FindOptions
        {
            None = 0,
            NameContains = 1 << 0
        }

        private const float COLUMN_PADDING = 5f;
        private const float TEXT_FIELD_WIDTH = 35f;
        public static readonly UITextureAtlas TextureAtlas = LoadResources();
        public static readonly UITextureAtlas DefaultAtlas = ResourceLoader.GetAtlas("Ingame");
        public static readonly UITextureAtlas AdvisorAtlas = ResourceLoader.GetAtlas("AdvisorSprites");

        public static UIView uiRoot;

        // some helper functions from https://github.com/bernardd/Crossings/blob/master/Crossings/UIUtils.cs
        private static void FindUIRoot()
        {
            uiRoot = null;

            foreach (var view in Object.FindObjectsOfType<UIView>())
                if (view.transform.parent == null && view.name == "UIView")
                {
                    uiRoot = view;
                    break;
                }
        }

        public static string GetTransformPath(Transform transform)
        {
            var path = transform.name;
            var t = transform.parent;
            while (t != null)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }

            return path;
        }

        public static T FindComponent<T>(string name, UIComponent parent = null, FindOptions options = FindOptions.None)
            where T : UIComponent
        {
            if (uiRoot == null)
            {
                FindUIRoot();
                if (uiRoot == null) return null;
            }

            foreach (var component in Object.FindObjectsOfType<T>())
            {
                bool nameMatches;
                if ((options & FindOptions.NameContains) != 0) nameMatches = component.name.Contains(name);
                else nameMatches = component.name == name;

                if (!nameMatches) continue;

                Transform parentTransform;
                if (parent != null) parentTransform = parent.transform;
                else parentTransform = uiRoot.transform;

                var t = component.transform.parent;
                while (t != null && t != parentTransform) t = t.parent;

                if (t == null) continue;

                return component;
            }


            return null;
        }

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

        public static UITextField CreateTextField(UIComponent parent)
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
            dropDown.size = new Vector2(250f, 40f);
            dropDown.listBackground = "GenericPanelLight";
            dropDown.itemHeight = 25;
            dropDown.itemHover = "ListItemHover";
            dropDown.itemHighlight = "ListItemHighlight";
            dropDown.normalBgSprite = "ButtonMenu";
            dropDown.disabledBgSprite = "ButtonMenuDisabled";
            dropDown.hoveredBgSprite = "ButtonMenuHovered";
            dropDown.focusedBgSprite = "ButtonMenu";
            dropDown.listWidth = 250;
            dropDown.listHeight = 300;
            dropDown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dropDown.popupColor = new Color32(45, 52, 61, 255);
            dropDown.popupTextColor = new Color32(170, 170, 170, 255);
            dropDown.zOrder = 1;
            dropDown.textScale = 0.8f;
            dropDown.verticalAlignment = UIVerticalAlignment.Middle;
            dropDown.horizontalAlignment = UIHorizontalAlignment.Left;
            dropDown.selectedIndex = 0;
            dropDown.textFieldPadding = new RectOffset(14, 0, 14, 0);
            dropDown.itemPadding = new RectOffset(14, 0, 8, 0);
            dropDown.listPosition = UIDropDown.PopupListPosition.Below;
            dropDown.listOffset = new Vector2(0, 0);

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
                dropDown.listWidth = (int) t.x;
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

        public static UIButton CreateUiButton(UIComponent parent, string text, string tooltip, Vector2 size,
            string sprite)
        {
            var uiButton = parent.AddUIComponent<UIButton>();
            uiButton.atlas = TextureAtlas;
            uiButton.text = text;
            uiButton.tooltip = tooltip;
            uiButton.size = new Vector2(36, 36);
            uiButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            uiButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            uiButton.normalFgSprite = sprite;
            uiButton.hoveredFgSprite = $"{sprite}Hovered";
            uiButton.pressedFgSprite = $"{sprite}Pressed";
            uiButton.focusedFgSprite = $"{sprite}Focussed";
            uiButton.disabledFgSprite = $"{sprite}Disabled";
            uiButton.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            uiButton.horizontalAlignment = UIHorizontalAlignment.Right;
            uiButton.verticalAlignment = UIVerticalAlignment.Middle;

            return uiButton;
        }

        //public static UICheckBox CreateCheckBox(UIComponent parent, string spriteName, string toolTip, bool value)
        //{
        //    return CreateCheckBox(parent, spriteName, toolTip, value, new Vector2(36, 36));
        // }

        public static UICheckBox CreateCheckBox(UIComponent parent, string spriteName, string toolTip, bool value, bool isStatic = false)
        {
            var checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = new Vector2(36, 36);

            var button = checkBox.AddUIComponent<UIButton>();
            button.name = "PRT_" + spriteName;
            button.atlas = !isStatic ? TextureAtlas : DefaultAtlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(0, 0);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            if (!isStatic)
            {
                button.hoveredFgSprite = spriteName + "Hovered";
                button.pressedFgSprite = spriteName + "Pressed";
                button.disabledFgSprite = spriteName + "Disabled";
            }

            checkBox.isChecked = value;
            if (value)
            {
                button.normalBgSprite = "OptionBaseFocused";
                if (!isStatic)
                    button.normalFgSprite = spriteName + "Focused";
            }

            checkBox.eventCheckChanged += (c, s) =>
            {
                if (s)
                {
                    button.normalBgSprite = "OptionBaseFocused";
                    if (!isStatic)
                        button.normalFgSprite = spriteName + "Focused";
                }
                else
                {
                    button.normalBgSprite = "OptionBase";
                    if (!isStatic)
                        button.normalFgSprite = spriteName;
                }
            };

            return checkBox;
        }

        private static UITextureAtlas LoadResources()
        {
            string[] spriteNames =
            {
                "Add",
                "AddDisabled",
                "AddFocused",
                "AddHovered",
                "AddPressed",
                "Remove",
                "RemoveDisabled",
                "RemoveFocused",
                "RemoveHovered",
                "RemovePressed",
                "Parallel",
                "ParallelDisabled",
                "ParallelFocused",
                "ParallelHovered",
                "ParallelPressed",
                "Reverse",
                "ReverseDisabled",
                "ReverseFocused",
                "ReverseHovered",
                "ReversePressed",
                "Tutorial"
            };

            var textureAtlas =
                ResourceLoader.CreateTextureAtlas("ParallelRoadTool", spriteNames, "ParallelRoadTool.Icons.");

            var defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures =
            {
                defaultAtlas["OptionBase"].texture,
                defaultAtlas["OptionBaseFocused"].texture,
                defaultAtlas["OptionBaseHovered"].texture,
                defaultAtlas["OptionBasePressed"].texture,
                defaultAtlas["OptionBaseDisabled"].texture,
                defaultAtlas["Snapping"].texture,
                defaultAtlas["SnappingFocused"].texture,
                defaultAtlas["SnappingHovered"].texture,
                defaultAtlas["SnappingPressed"].texture,
                defaultAtlas["SnappingDisabled"].texture
            };

            ResourceLoader.AddTexturesInAtlas(textureAtlas, textures);

            return textureAtlas;
        }
    }
}