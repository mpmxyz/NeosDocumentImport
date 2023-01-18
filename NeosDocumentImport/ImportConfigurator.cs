using BaseX;
using FrooxEngine;
using System.Collections.Generic;
using FrooxEngine.UIX;
using System.Reflection;

namespace NeosDocumentImport
{
    delegate void ImportAction(IEnumerable<string> files, IConverter converter, World world, float3 position, floatQ rotation);

    internal static class ImportConfigurator
    {
        private static IField BuildValue<T>(Slot slot, object obj, FieldInfo prop)
        {
            var value = slot.AttachComponent<ValueField<T>>().Value;
            value.Value = (T) prop.GetValue(obj);
            value.OnValueChange += (x) => { prop.SetValue(obj, x.Value); };
            return value;
        }
        private static IField BuildReference<T>(Slot slot, object obj, FieldInfo prop) where T : class, IWorldElement
        {
            var value = slot.AttachComponent<ReferenceField<T>>().Reference;
            value.Target = (T) prop.GetValue(obj);
            value.OnReferenceChange += (x) => { prop.SetValue(obj, x.Target); };
            return value;
        }

        internal static void Spawn(
            IEnumerable<string> files,
            World world,
            float3 position,
            floatQ rotation,
            float3 scale,
            IConverter converter)
        {

            var slot = world.AddSlot("Document Import Configurator", false);
            slot.GlobalPosition = position;
            slot.GlobalRotation = rotation;
            slot.GlobalScale = scale;

            var panel = slot.AttachComponent<NeosCanvasPanel>();
            panel.Panel.Title = "Document Import";
            panel.Panel.AddCloseButton();
            panel.CanvasSize = new float2(200, 108);
            panel.CanvasScale = 0.25f / panel.CanvasSize.y;

            var uiBuilder = new UIBuilder(panel.Canvas);
            uiBuilder.ScrollArea();
            uiBuilder.VerticalLayout(4f);

            var converterType = converter.GetType();

            foreach (var prop in converterType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var propType = prop.FieldType;
                var propAttrs = prop.GetCustomAttributes(true);
                foreach (var attr in propAttrs)
                {
                    if (attr is ConfigAttribute conf)
                    {
                        MethodInfo buildSpecificField = null;
                        switch (conf.type)
                        {
                            case ConfigType.Value:
                                buildSpecificField =
                            typeof(ImportConfigurator)
                            .GetGenericMethod(nameof(ImportConfigurator.BuildValue), BindingFlags.NonPublic | BindingFlags.Static, propType);
                                break;
                            case ConfigType.Reference:
                                buildSpecificField =
                            typeof(ImportConfigurator)
                            .GetGenericMethod(nameof(ImportConfigurator.BuildReference), BindingFlags.NonPublic | BindingFlags.Static, propType);
                                break;
                        }
                        var ifield = (IField) buildSpecificField.Invoke(null, new object[] { slot, converter, prop });

                        SyncMemberEditorBuilder.Build(ifield, conf.name, prop, uiBuilder);
                        break;
                    }
                }
            }

            panel.CanvasSize = new float2(200, 108);

            uiBuilder.Style.FlexibleHeight = -1;
            uiBuilder.Style.MinHeight = 24;

            var trigger = uiBuilder.Button((LocaleString) "Import!");
            trigger.LocalPressed += (button, data) =>
            {
                if (converter.ValidateConfig())
                {
                    DocumentImporter.Spawn(files, converter, world, slot.GlobalPosition, slot.GlobalRotation);
                    slot.Destroy();
                }
            };
        }

        private static void PrependHeader(this UIBuilder uiBuilder, string text)
        {
            uiBuilder.Next(text);
            uiBuilder.Nest();
            RectTransform header, content;
            uiBuilder.SplitHorizontally(0.25f, out header, out content, 0.02f);
            uiBuilder.NestInto(header);
            uiBuilder.Text((LocaleString)$"{text}:");
            uiBuilder.NestOut();
            uiBuilder.NestInto(content);
        }
    }
}