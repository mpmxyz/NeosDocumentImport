using BaseX;
using FrooxEngine;
using System.Collections.Generic;
using FrooxEngine.UIX;

namespace NeosDocumentImport
{
    delegate void ImportAction(IEnumerable<string> files, IConverter converter, ImportConfig config, World world, float3 position, floatQ rotation);

    internal static class ImportConfigurator
    {
        internal static void Spawn(
            IEnumerable<string> files,
            World world,
            float3 position,
            floatQ rotation,
            float3 scale,
            ImportAction import,
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
            uiBuilder.VerticalLayout(4f);
            uiBuilder.Style.FlexibleHeight = -1;
            uiBuilder.Style.MinHeight = 24;

            var dpi = slot.AttachComponent<ValueField<int>>().Value;
            dpi.Value = 150;
            {
                uiBuilder.PrependHeader("DPI");
                var editor = uiBuilder.SliderMemberEditor(50, 300, dpi);
                uiBuilder.NestOut();
                uiBuilder.NestOut();
                uiBuilder.NestOut();
            }

            var transparent = slot.AttachComponent<ValueField<bool>>().Value;
            {
                uiBuilder.PrependHeader("Transparent");
                uiBuilder.BooleanMemberEditor(transparent);
                uiBuilder.NestOut();
                uiBuilder.NestOut();
            }

            var pageString = slot.AttachComponent<ValueField<string>>().Value;
            {
                uiBuilder.PrependHeader("Pages");
                uiBuilder.HorizontalLayout(4f);
                uiBuilder.Style.FlexibleWidth = 10f;
                uiBuilder.PrimitiveMemberEditor(pageString);
                uiBuilder.NestOut();
                uiBuilder.NestOut();
                uiBuilder.NestOut();
            }

            var trigger = uiBuilder.Button((LocaleString) "Import!");
            trigger.LocalPressed += (button, data) =>
            {
                var pages = ParsePageList(pageString.Value);
                var config = new ImportConfig(dpi.Value, !transparent.Value, pages);

                if (config.IsValid)
                {
                    import(files, converter, config, world, slot.GlobalPosition, slot.GlobalRotation);
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

        private static List<PageRange> ParsePageList(string pageString)
        {
            var pages = new List<PageRange>();
            if (pageString == null) {
                //null => no selection
                return pages;
            }

            pageString = pageString.RemoveWhitespace();

            if (pageString.Length == 0)
            {
                //empty string => no selection
                return pages;
            }

            foreach (var segment in pageString.Split(','))
            {
                var numbers = segment.Split('-');

                switch (numbers.Length)
                {
                    case 1:
                        if (int.TryParse(numbers[0], out int x))
                        {
                            pages.Add(new PageRange(x));
                        }
                        else
                        {
                            //parsing failed
                            return null;
                        }

                        break;
                    case 2:
                        if (int.TryParse(numbers[0], out int a)
                            && int.TryParse(numbers[1], out int b))
                        {
                            var range = new PageRange(a, b);

                            if (range.Count > 9999)
                            {
                                //hard limit # of generated files to prevent endless lockup of Neos
                                return null;
                            }

                            pages.Add(range);
                        } 
                        else 
                        {
                            //parsing failed
                            return null;
                        }

                        break;
                    default:
                        //too many minus signs
                        return null;
                }
            }
            return pages;
        }
    }
}