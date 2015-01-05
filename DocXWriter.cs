using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;

namespace IanUtility
{
    public class DocXWriter :IDisposable
    {

        public static bool ApplyStyle(Paragraph p, string styleName)
        {
            // If the paragraph has no ParagraphProperties object, create one.
            if (p.Elements<ParagraphProperties>().Count() == 0)
            {
                p.PrependChild<ParagraphProperties>(new ParagraphProperties());
            }

            // Get a reference to the ParagraphProperties object.
            ParagraphProperties pPr = p.ParagraphProperties;

            // If a ParagraphStyleId object does not exist, create one.
            if (pPr.ParagraphStyleId == null)
                pPr.ParagraphStyleId = new ParagraphStyleId();

            // Set the style of the paragraph.
            pPr.ParagraphStyleId.Val = styleName;
            return true;
        }
        public static bool ApplyStyle(Run r, string styleName)
        {
            // If the Run has no RunProperties object, create one.
            if (r.Elements<RunProperties>().Count() == 0)
            {
                r.PrependChild<RunProperties>(new RunProperties());
            }

            // Get a reference to the RunProperties.
            RunProperties rPr = r.RunProperties;

            // Set the character style of the run.
            if (rPr.RunStyle == null)
                rPr.RunStyle = new RunStyle();
            rPr.RunStyle.Val = styleName;
            return true;
        }
        public static bool ApplyCharFormat(Run r, bool bold, bool italic)
        {
            if (r.RunProperties == null) r.RunProperties = new RunProperties();
            if (bold)
            {
                Bold b = new Bold();
                b.Val = DocumentFormat.OpenXml.OnOffValue.FromBoolean(bold);
                r.RunProperties.Append(b);
            }
            if (italic)
            {
                Italic b = new Italic();
                b.Val = DocumentFormat.OpenXml.OnOffValue.FromBoolean(italic);
                r.RunProperties.Append(b);
            }
            return true;
        }
        public static void CreatePara(string text, string style)
        {
            curXWriter.PushStyle(style);
            curXWriter.NewParagraph();
            curXWriter.AppendText(text);
            curXWriter.PopStyle();
            //return curXWriter.curPara;
            //Paragraph ret = new Paragraph(new Run(new Text() { Text = text, Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve }));
            //ApplyStyle(ret, style);
            //return ret;
        }
        public static void CreatePara2Run(string paraStyle, string text, string style, string text2, string style2)
        {
            curXWriter.PushStyle(paraStyle);
            curXWriter.NewParagraph();
            curXWriter.PushStyle(style);
            curXWriter.AppendText(text);
            curXWriter.PopStyle();
            curXWriter.PushStyle(style2);
            curXWriter.AppendText(text2);
            curXWriter.PopStyle();
            curXWriter.PopStyle();
            //return curXWriter.curPara;
            //Paragraph ret = new Paragraph();
            //ApplyStyle(ret, paraStyle);
            //Run r = new Run(new Text() { Text = text, Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve });
            //ApplyStyle(r, style);
            //ret.Append(r);
            //Run r2 = new Run(new Text() { Text = text2, Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve });
            //ApplyStyle(r2, style2);
            //ret.Append(r2);
            //return ret;
        }
    
        public static DocXWriter curXWriter = null;

        public WordprocessingDocument package;
        Body b;
        Paragraph curPara;
        string curParaStyle;
        Stack<string> curStyleStack = new Stack<string>();
        Stack<Tuple<bool, bool>> curCharFmt = new Stack<Tuple<bool, bool>>(); //bold,italic
        public DocXWriter(string fname)
        {
            package = WordprocessingDocument.Create(fname, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);

            // Add a new main document part. 
            package.AddMainDocumentPart();

            package.MainDocumentPart.AddNewPart<StyleDefinitionsPart>();
            package.MainDocumentPart.StyleDefinitionsPart.Styles = new Styles();
            //CreateDefaultStyles();


            // Create the Document DOM. 
            package.MainDocumentPart.Document = new Document();
            b = package.MainDocumentPart.Document.Body = new Body();

            curXWriter = this;
        }
        public bool AddParas(List<Paragraph> paras){
            b.Append(paras);
            return true;
        }
        public bool Save(){
            EndParagraph();
            b = null;
            // Save changes to the main document part. 
            package.MainDocumentPart.Document.Save();
            package.Close();
            package.Dispose();
            package = null;
            return true;
        }
        void IDisposable.Dispose()
        {
            if (package == null) return;
            package.Close();
            package.Dispose();
            package = null;
        }

        public void CreateStyle(string name, string id, bool isDefault, StyleValues type, string font, int fontSize,
            bool bold = false, bool italic = false, string linkedTo = "", int beforeSpace = 2, int firstLineIndent = 360)
        {
            Style ret = new Style() { Type = type, StyleId = id, CustomStyle = true, Default = isDefault };
            ret.Append(new StyleName() { Val = name });
            if (linkedTo != "") ret.Append(new LinkedStyle() { Val = linkedTo });

            StyleRunProperties srp = new StyleRunProperties();
            if (font != null) srp.Append(new RunFonts() { Ascii = font });
            if (fontSize > 0) srp.Append(new FontSize() { Val = fontSize.ToString() });
            if (bold) srp.Append(new Bold());
            if (italic) srp.Append(new Italic());
            if (srp.ChildElements.Count > 0) ret.Append(srp);

            if (type == StyleValues.Paragraph)
            {
                StyleParagraphProperties spp = new StyleParagraphProperties();
                spp.SpacingBetweenLines = new SpacingBetweenLines() { After = "0", Before = (beforeSpace * 20).ToString() };
                if (firstLineIndent > 0) spp.Indentation = new Indentation() { FirstLine = firstLineIndent.ToString() };
                ret.Append(spp);
            }

            package.MainDocumentPart.StyleDefinitionsPart.Styles.Append(ret);
        }
        public void CreateDefaultStyles()
        {
            //name, text, descr, smalltext      Char styles: bold, bolditalic - both based on text
            CreateStyle("Name", "Name", false, StyleValues.Paragraph, "Arial", 48, beforeSpace: 18, firstLineIndent: 0);
            CreateStyle("text", "text", true, StyleValues.Paragraph, "Arial", 22, firstLineIndent: 0);
            CreateStyle("descr", "descr", false, StyleValues.Paragraph, "Arial", 20);
            CreateStyle("descFixed", "descFixed", false, StyleValues.Paragraph, "Courier New", 20, firstLineIndent: 0);
            CreateStyle("smalltext", "smalltext", false, StyleValues.Paragraph, "Arial", 16, firstLineIndent: 0);
            CreateStyle("School", "School", false, StyleValues.Paragraph, "Arial", 22, false, true, firstLineIndent: 0);
            CreateStyle("bold", "bold", false, StyleValues.Character, null, -1, true, false, "text");
            CreateStyle("bolditalic", "bolditalic", false, StyleValues.Character, null, -1, true, true, "text");
            
        }
        

        public void PushStyle(string s) {
            curStyleStack.Push(s);
        }
        public void PopStyle() {
            curStyleStack.Pop();
        }
        public void PushCharFormat(bool bold, bool italic) {
            curCharFmt.Push(new Tuple<bool, bool>(bold, italic));
        }
        public void PopCharFormat() {
            curCharFmt.Pop();
        }
        public void NewParagraph() {
            EndParagraph();
            curPara = new Paragraph();
            if (curStyleStack.Count > 0)
            {
                curParaStyle = curStyleStack.Peek();
                ApplyStyle(curPara, curParaStyle);
            }
        }
        public void EndParagraph()
        {
            if (curPara != null)
            {
                if (curCell != null) curCell.Append(curPara);
                else b.Append(curPara);
                curPara = null;
            }
        }
        public void AppendText(string text) {
            if (curPara == null)
                NewParagraph();
            Run r = new Run();
            if (curStyleStack.Count>0 && curParaStyle != curStyleStack.Peek()) ApplyStyle(r, curStyleStack.Peek());
            if (curCharFmt.Count>0 && (curCharFmt.Peek().Item1 || curCharFmt.Peek().Item2))
                ApplyCharFormat(r, curCharFmt.Peek().Item1, curCharFmt.Peek().Item2);
            r.Append(new Text() { Text = text, Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve});
            curPara.Append(r);
        }
        public void LineBreak() {
            if (curPara == null)
                NewParagraph();
            curPara.Append(new Run(new Break()));
        }
        public void HorizRule()
        {
            NewParagraph();
            ParagraphProperties paraProperties = new ParagraphProperties();
            ParagraphBorders paraBorders = new ParagraphBorders();
            BottomBorder bottom = new BottomBorder() { Val = BorderValues.Single, Color = "auto", Size = (UInt32Value)12U, Space = (UInt32Value)1U };
            paraBorders.Append(bottom);
            paraProperties.Append(paraBorders);
            curPara.Append(paraProperties);

            NewParagraph();
        }



        Table curTbl;
        TableRow curRow;
        TableCell curCell;
        public void TableStart() {
            EndParagraph();
            curTbl = new Table();
            // Set the style and width for the table.
            TableProperties tableProp = new TableProperties();
            //tableProp.TableCellSpacing = new TableCellSpacing() { Width = "144" };
            //TableStyle tableStyle = new TableStyle() { Val = "TableGrid" };

            // Make the table width 100% of the page width.
            //TableWidth tableWidth = new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct };

            // Apply
            //tableProp.Append(tableStyle, tableWidth);
            curTbl.AppendChild(tableProp);

            // Add 3 columns to the table.
            //TableGrid tg = new TableGrid(new GridColumn(), new GridColumn(), new GridColumn());
            //tbl.AppendChild(tg);

        }
        public void TableEnd() {
            TableEndRow();
            if (curTbl != null)
            {
                b.Append(curTbl);
                curTbl = null;
            }
        }
        public void TableNewRow() {
            TableEndRow();
            curRow = new TableRow();
        }
        public void TableEndRow()
        {
            TableEndCell();
            if (curRow != null)
            {
                curTbl.Append(curRow);
                curRow = null;
            }
        }
        public void TableNewCell() {
            TableEndCell();
            curCell = new TableCell();
            NewParagraph();
        }
        public void TableEndCell()
        {
            if (curCell != null)
            {
                EndParagraph();
                curRow.Append(curCell);
                curCell = null;
            }
        }
    
    }
}
