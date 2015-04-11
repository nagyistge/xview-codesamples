﻿using System.Collections.Generic;
using System.Globalization;

using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;

using XView;

namespace XViewCodeSamples.Views.ComponentViews
{
    /// <summary>
    /// This View illustrates the usage of XTemplate for parsing an HTML template.
    /// </summary>
    public class ArticleView : ComponentView
    {
        protected override string Render()
        {
            // Instantiates a new dynamic XTemplate object through the NewXTemplate() method
            // which is defined in the base View<TModel>.

            dynamic xt = this.NewXTemplate(ComponentLayout.ArticleView);

            // Assigns values to {Title} and {Introduction} variables.

            xt.Title = this.Fields.GetText("title");
            xt.Introduction = this.Fields.GetText("introduction");

            IList<ItemFields> paragraphs = this.Fields.GetEmbeddedFields("paragraphs");

            // Assigns value to {NumberOfAchorColumns} variable.

            xt.NumberOfAchorColumns = paragraphs.Count > 4 ? "2" : "1";

            bool showAnchors = this.Fields.GetText("show_anchors") == "Yes";
            int paragraphIndex = 0;

            foreach (var paragraph in paragraphs)
            {
                paragraphIndex++;
                Component image = paragraph.GetComponent("image");

                // image is a optional field, therefore the "image" block will only be parsed
                // if there is an image available in a paragraph.

                if (image != null)
                {
                    ItemFields imageMetadata = image.GetMetadataFields();

                    // Assigns values to {ImageUrl}, {ImageAlt} and {ImageTitle} variables.

                    xt.ImageUrl = this.Context.AddBinary(image).Url;
                    xt.ImageAlt = imageMetadata.GetText("alt");
                    xt.ImageTitle = imageMetadata.GetText("title") ?? xt.ImageAlt;

                    // Parses nested block "image". The "image" block will only be included
                    // to the "paragraph" block output if that paragraph has an image.

                    xt.Parse("root.paragraph.image");
                }

                // Assigns values to {ParagraphTitle}, {ParagraphText} and {ParagraphIndex} variables.

                // IMPORTANT INFORMATION:
                // Usually the "text" field in a paragraph is an XhtmlField which contains xhtml content.
                // Xhtml content needs to be resolved. The .GetText("text") method call below automatically
                // resolves the xhtml content in the "text" field using the XhtmlResolver handler method
                // TemplateUtilities.ResolveRichTextFieldXhtml(string inputXhtml). (See the SampleController
                // class for how this is setup).
                // This method is assigned to the TridionExtensions.XhtmlResolver delegate to resolve xhtml 
                // content whenever the .GetText() extension method encounters an XhtmlField.

                xt.ParagraphTitle = paragraph.GetText("title");
                xt.ParagraphText = paragraph.GetText("text");
                xt.ParagraphIndex = paragraphIndex.ToString(CultureInfo.InvariantCulture);

                // Parses the "paragraph" block for each paragraph found in the Component.

                xt.Parse("root.paragraph");

                // Only parses "anchor" block if the option to show anchors is checked.

                if (showAnchors)
                {
                    // Parses the inner "anchor" block .
                    // This parses each individual <li></li> tags.

                    xt.Parse("root.anchors.anchor");
                }
            }

            // Only parses "anchors" block if the option to show anchors is checked.

            if (showAnchors)
            {
                // TODO: translate label "On this page".

                // Parses the outer "anchors" block. Anny previously parsed nested block "anchor"
                // will be included.
                // This parses the container <div></did> tags containing the <ul></ul> tags.

                xt.Parse("root.anchors");
            }

            // Returns the output of the parsed template ComponentLayout.ArticleView. 

            return xt.ToString();
        }
    }
}