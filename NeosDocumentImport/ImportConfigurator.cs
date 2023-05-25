using BaseX;
using FrooxEngine;
using System.Collections.Generic;
using FrooxEngine.UIX;
using System.Reflection;
using CodeX;
using System;

namespace NeosDocumentImport
{
    /// <summary>
    /// Helper class to create an import dialog
    /// </summary>
    internal static class ImportConfigurator
    {
        private const string SLOT_NAME_IMPORT = "Document Import Configurator";
        private const string SLOT_NAME_USERSPACE_DIALOG = "Userspace Dialog";
        private const string TITLE_IMPORT_PANEL = "Document Import";
        private const string TITLE_SECRET_EDIT_PANEL = "Edit...";
        private const string LABEL_TEXT_IMPORT = "Import";
        private const string LABEL_TEXT_SKIP = "Skip conversion";
        private const string LABEL_SECRET_EDIT = "Edit";
        private const string LABEL_USERSPACE_DIALOG_CLOSE = "OK";
        private const float CONFIG_PANEL_HEIGHT = 0.25f;
        private const float USERSPACE_PANEL_HEIGHT = 0.15f;
        private static readonly float2 CONFIG_CANVAS_SIZE = new float2(200f, 108f);
        private static readonly float2 USERSPACE_CANVAS_SIZE = new float2(200f, 52f);
        private const float SPACING = 4f;
        private const float BUTTON_HEIGHT = 24f;

        internal static void Spawn(
            AssetClass assetClass,
            IEnumerable<string> files,
            World world,
            float3 position,
            floatQ rotation,
            float3 scale,
            IConverter converter)
        {

            var slot = world.AddSlot(SLOT_NAME_IMPORT, false);
            slot.GlobalPosition = position;
            slot.GlobalRotation = rotation;
            slot.GlobalScale = scale;

            var panel = slot.AttachComponent<NeosCanvasPanel>();
            panel.Panel.Title = TITLE_IMPORT_PANEL;
            panel.Panel.AddCloseButton();
            panel.CanvasSize = CONFIG_CANVAS_SIZE;
            panel.CanvasScale = CONFIG_PANEL_HEIGHT / panel.CanvasSize.y;

            var uiBuilder = new UIBuilder(panel.Canvas);
            uiBuilder.ScrollArea();
            uiBuilder.VerticalLayout(SPACING);

            Button trigger = null;
            BuildConfigFields(converter, slot, uiBuilder, () => UpdateButton(converter, trigger));

            uiBuilder.Style.FlexibleHeight = -1;
            uiBuilder.Style.MinHeight = BUTTON_HEIGHT;
            uiBuilder.Style.PreferredHeight = BUTTON_HEIGHT;
            uiBuilder.Style.ForceExpandWidth = true;

            uiBuilder.HorizontalLayout(SPACING);
            uiBuilder.Style.FlexibleWidth = 1;

            trigger = uiBuilder.Button();
            trigger.LocalPressed += (button, data) =>
            {
                if (converter.ValidateConfig(out var ignored))
                {
                    Conversion.Start(files, converter, world, slot.GlobalPosition, slot.GlobalRotation);
                    slot.Destroy();
                }
            };

            var rawImportTrigger = uiBuilder.Button((LocaleString)LABEL_TEXT_SKIP);
            rawImportTrigger.LocalPressed += (button, data) =>
            {
                NeosDocumentImportMod.skipNext = true;
                UniversalImporter.Import(assetClass, files, world, position, rotation);
                slot.Destroy();
            };

            UpdateButton(converter, trigger);
        }

        private static void UpdateButton(IConverter converter, Button trigger)
        {
            if (trigger != null)
            {
                if (!converter.ValidateConfig(out var msg))
                {
                    trigger.Enabled = false;
                    trigger.Label.Color.Value = color.Red;
                    trigger.LabelText = $"<b>{msg}</b>";
                }
                else
                {
                    trigger.Enabled = true;
                    trigger.Label.Color.Value = color.Black;
                    trigger.LabelText = LABEL_TEXT_IMPORT;
                }
            }
        }

        private static void BuildConfigFields(IConverter converter, Slot slot, UIBuilder uiBuilder, Action onChange)
        {
            var converterType = converter.GetType();

            foreach (var prop in converterType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                foreach (var attr in prop.GetCustomAttributes(true))
                {
                    if (attr is ConfigAttribute conf)
                    {
                        if (conf.secret)
                        {
                            BuildSecretButton(prop.Name, uiBuilder, (uiBuilder2) =>
                            {
                                var root = uiBuilder2.Root;
                                BuildEditor(converter, root, prop, onChange, uiBuilder2, conf);
                                var editor = root.GetComponentInChildren<TextEditor>();
                                editor?.Focus();
                            });
                        }
                        else
                        {
                            BuildEditor(converter, slot, prop, onChange, uiBuilder, conf);
                        }
                        break;
                    }
                }
            }
        }

        private static void BuildSecretButton(string name, UIBuilder uiBuilder, Action<UIBuilder> createFields)
        {
            uiBuilder.PushStyle();
            uiBuilder.Style.MinHeight = 24f;

            uiBuilder.Panel();

            Text text2 = uiBuilder.Text(name + ":", bestFit: true, Alignment.MiddleLeft, parseRTF: false);
            text2.Color.Value = color.Black;
            uiBuilder.CurrentRect.AnchorMax.Value = new float2(0.25f, 1f);

            Button button = uiBuilder.Button(LABEL_SECRET_EDIT);
            uiBuilder.CurrentRect.AnchorMin.Value = new float2(0.25f);
            button.LocalPressed += (b, d) =>
            {
                CreateUserSpacePopup(TITLE_SECRET_EDIT_PANEL, createFields);
            };

            uiBuilder.NestOut();
            uiBuilder.PopStyle();
        }

        private static void CreateUserSpacePopup(string title, Action<UIBuilder> createFields)
        {
            Userspace.UserspaceWorld.RunSynchronously(() =>
            {
                Slot slot = Userspace.UserspaceWorld.AddSlot(SLOT_NAME_USERSPACE_DIALOG, persistent: false);
                var panel = slot.AttachComponent<NeosCanvasPanel>();
                panel.Panel.Title = title;
                panel.CanvasSize = USERSPACE_CANVAS_SIZE;
                panel.CanvasScale = USERSPACE_PANEL_HEIGHT / panel.CanvasSize.y;

                var uiBuilder = new UIBuilder(panel.Canvas);
                uiBuilder.ScrollArea();
                uiBuilder.VerticalLayout(SPACING);

                createFields(uiBuilder);

                uiBuilder.Style.FlexibleHeight = -1;
                uiBuilder.Style.MinHeight = BUTTON_HEIGHT;
                uiBuilder.Style.PreferredHeight = BUTTON_HEIGHT;

                Button trigger = uiBuilder.Button(LABEL_USERSPACE_DIALOG_CLOSE);
                trigger.LocalPressed += (button, data) =>
                {
                    slot.Destroy();
                };

                slot.PositionInFrontOfUser(float3.Backward);
            });
        }

        private static void BuildEditor(object valueObj, Slot ifieldSlot, FieldInfo prop, Action onChange, UIBuilder uiBuilder, ConfigAttribute conf)
        {
            var ifield = (IField)FieldBuilder(prop, conf)?.Invoke(null, new object[] { ifieldSlot, valueObj, prop, onChange });
            if (ifield != null)
            {
                SyncMemberEditorBuilder.Build(ifield, conf.name, prop, uiBuilder);
            }
        }

        private static MethodInfo FieldBuilder(FieldInfo prop, ConfigAttribute conf)
        {
            const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Static;
            switch (conf.type)
            {
                case ConfigType.Value:
                    return typeof(ImportConfigurator)
                        .GetGenericMethod(nameof(ImportConfigurator.BuildValue), FLAGS, prop.FieldType);
                case ConfigType.Reference:
                    return typeof(ImportConfigurator)
                        .GetGenericMethod(nameof(ImportConfigurator.BuildReference), FLAGS, prop.FieldType);
                default: return null;
            }
        }

        private static IField BuildValue<T>(Slot slot, object obj, FieldInfo prop, Action onChange)
        {
            var value = slot.AttachComponent<ValueField<T>>().Value;
            value.Value = (T)prop.GetValue(obj);
            value.OnValueChange += (x) =>
            {
                prop.SetValue(obj, x.Value);
                onChange();
            };
            return value;
        }

        private static IField BuildReference<T>(Slot slot, object obj, FieldInfo prop, Action onChange) where T : class, IWorldElement
        {
            var value = slot.AttachComponent<ReferenceField<T>>().Reference;
            value.Target = (T)prop.GetValue(obj);
            value.OnReferenceChange += (x) =>
            {
                prop.SetValue(obj, x.Target);
                onChange();
            };
            return value;
        }
    }
}