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
        private const string SLOT_NAME = "Document Import Configurator";
        private const string PANEL_TITLE = "Document Import";
        private const string LABEL_TEXT_IMPORT = "Import";
        private const string LABEL_TEXT_SKIP = "Skip conversion";
        private const float PANEL_HEIGHT = 0.25f;
        private static readonly float2 CANVAS_SIZE = new float2(200f, 108f);
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

            var slot = world.AddSlot(SLOT_NAME, false);
            slot.GlobalPosition = position;
            slot.GlobalRotation = rotation;
            slot.GlobalScale = scale;

            var panel = slot.AttachComponent<NeosCanvasPanel>();
            panel.Panel.Title = PANEL_TITLE;
            panel.Panel.AddCloseButton();
            panel.CanvasSize = CANVAS_SIZE;
            panel.CanvasScale = PANEL_HEIGHT / panel.CanvasSize.y;

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
                        var ifield = (IField)FieldBuilder(prop, conf)?.Invoke(null, new object[] { slot, converter, prop, onChange });

                        if (ifield != null)
                        {
                            SyncMemberEditorBuilder.Build(ifield, conf.name, prop, uiBuilder);
                            break;
                        }
                    }
                }
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