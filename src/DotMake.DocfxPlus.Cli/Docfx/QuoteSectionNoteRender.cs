using System.Collections.Generic;
using System.Linq;
using System.Web;
using Docfx.MarkdigEngine.Extensions;
using HarmonyLib;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

// ReSharper disable InconsistentNaming

namespace DotMake.DocfxPlus.Cli.Docfx
{
    internal static class QuoteSectionNoteRender
    {
        internal static void WriteNote(object __instance, HtmlRenderer renderer, QuoteSectionNoteBlock obj)
        {
            var type = __instance.GetType();
            var _context = AccessTools.Field(type, "_context")
                .GetValue(__instance);
            var _notes = AccessTools.Field(type, "_notes")
                .GetValue(__instance) as Dictionary<string, string>;

            var title = GetFirstBoldLineAndRemove(obj);

            var getToken = AccessTools.Method(_context!.GetType(),"GetToken");
            var noteHeadingText = title
                                  ?? getToken.Invoke(_context, [obj.NoteTypeString.ToLower()]) as string
                                  ?? obj.NoteTypeString.ToUpper();

            // Trim <h5></h5> for backward compatibility
            if (noteHeadingText.StartsWith("<h5>") && noteHeadingText.EndsWith("</h5>"))
            {
                noteHeadingText = noteHeadingText[4..^5];
            }

            var noteHeading = $"<h5>{HttpUtility.HtmlEncode(noteHeadingText)}</h5>";
            var classNames = _notes!.TryGetValue(obj.NoteTypeString, out var value) ? value : obj.NoteTypeString.ToUpper();
            renderer.Write("<div").Write($" class=\"{classNames}\"").WriteAttributes(obj).WriteLine(">");
            var savedImplicitParagraph = renderer.ImplicitParagraph;
            renderer.ImplicitParagraph = false;
            renderer.WriteLine(noteHeading);
            renderer.WriteChildren(obj);
            renderer.ImplicitParagraph = savedImplicitParagraph;
            renderer.WriteLine("</div>");
        }

        private static string GetFirstBoldLineAndRemove(ContainerBlock obj)
        {
            // Get the first paragraph
            var firstParagraph = obj.OfType<ParagraphBlock>().FirstOrDefault();
            if (firstParagraph == null)
                return null;

            // Get the first inline element in that paragraph
            var firstInline = firstParagraph.Inline?.FirstOrDefault();
            if (firstInline is EmphasisInline emphasis
                && emphasis.DelimiterChar == '*'
                && emphasis.DelimiterCount == 2)
            {
                // Extract text from the bold inline
                var literal = emphasis.FirstOrDefault() as LiteralInline;
                if (literal != null)
                {
                    firstInline.Remove();

                    return literal.Content.Text.Substring(
                        literal.Content.Start,
                        literal.Content.Length
                    );
                }
            }

            return null; // Not bold or no text
        }
    }
}
