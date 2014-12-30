using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace IanUtility
{
    public class ExcelParser
    {
        public class Sheet {
            public Dictionary<int, Column> ColumnByIndex = new Dictionary<int,Column>();
            public Dictionary<string, Column> ColumnByName = new Dictionary<string,Column>();
            public List<Row> Rows = new List<Row>();
            public string Name = "";

            public Column CreateNewColumn(string name, int idx = -1)
            {
                if (ColumnByName.ContainsKey(name)) return null;
                Column c = new Column();
                c.Name = name;
                if (idx == -1)
                {
                    //find last idx
                    int upper = 0;
                    foreach (int i in ColumnByIndex.Keys)
                    {
                        if (i > upper) upper = i;
                    }
                    c.Idx = upper+1;
                }
                else
                {
                    //insert in - by shifting all up
                    List<int> cur = ColumnByIndex.Keys.ToList();
                    cur.Sort();
                    for (int i = cur.Count - 1; i >= 0; i--)
                    {
                        Column cc = ColumnByIndex[i];
                        cc.Idx += 1;
                        ColumnByIndex.Remove(i);
                        ColumnByIndex.Add(cc.Idx, cc);
                    }
                }
                ColumnByIndex.Add(c.Idx, c);
                ColumnByName.Add(c.Name, c);
                return c;
            }
            public Row CreateNewRow()
            {
                Row r = new Row();
                r.Parent = this;
                Rows.Add(r);
                r.RowIdx = Rows.Count+1; //add 1 due to header row
                foreach (Column c in ColumnByIndex.Values)
                {
                    Cell cell = new Cell(){ Type = DataType.Empty, row = r, column = c };
                    r.Cells.Add(c,cell);
                }

                return r;
            }
        }
        public class Column
        {
            public int Idx;
            public string Name;
            public string ExcelIdx
            {
                get {
                    string s = "";
                    if(Idx/26 > 0) s += (char)((int)'A' + (Idx / 26));
                    s += (char)((int)'A' + (Idx % 26)-1);
                    return s;
                }
            }
        }
        public class Row 
        {
            public Sheet Parent;
            public int RowIdx;
            public Dictionary<Column, Cell> Cells = new Dictionary<Column,Cell>();

            public Cell this[int idx]
            {
                get
                {
                    if (!Parent.ColumnByIndex.ContainsKey(idx)) throw new ArgumentOutOfRangeException();
                    if (!Cells.ContainsKey(Parent.ColumnByIndex[idx])) throw new ArgumentOutOfRangeException();
                    return Cells[Parent.ColumnByIndex[idx]];
                }
            }
            public Cell this[string name]
            {
                get {
                    if (!Parent.ColumnByName.ContainsKey(name)) throw new ArgumentOutOfRangeException();
                    if (!Cells.ContainsKey(Parent.ColumnByName[name])) throw new ArgumentOutOfRangeException();
                    return Cells[Parent.ColumnByName[name]];
                }
            }
        }
        public class Cell
        {
            public Row row;
            public Column column;
            public DataType Type = DataType.Text;
            string sVal;
            int iVal;
            bool bVal;
            public string Text
            {
                get {
                    switch (Type)
                    {
                        case DataType.Text: return sVal;
                        case DataType.Int: return iVal.ToString();
                        case DataType.Empty: return "";
                        case DataType.Bool: return bVal.ToString();
                        default:
                            return null;
                    }
                }
                set
                {
                    Type = DataType.Text;
                    sVal = value;
                }
            }
            public int Int
            {
                get {
                    switch (Type)
                    {
                        case DataType.Text:
                            int i;
                            if (!Int32.TryParse(sVal, out i)) return 0;
                            return i;
                        case DataType.Int: return iVal;
                        case DataType.Empty: return 0;
                        case DataType.Bool: if (bVal) return 1; else return 0;
                        default:
                            return 0;
                    }
                }
                set
                {
                    Type = DataType.Int;
                    iVal = value;
                }
            }
            public bool Bool
            {
                get {
                    switch (Type)
                    {
                        case DataType.Text:
                            if (sVal == null || sVal == "") return false;
                            else return true;
                        case DataType.Int: if (iVal == 0) return false; else return true;
                        case DataType.Empty: return false;
                        case DataType.Bool: return bVal;
                        default:
                            return false;
                    }
                
                }
                set {
                    bVal = value;
                    Type = DataType.Bool;
                }
            }
            internal static Cell CellFromSheet(DocumentFormat.OpenXml.Spreadsheet.Cell c, DocumentFormat.OpenXml.Spreadsheet.SharedStringTable sst)
            {
                Cell ret = new Cell();

                //TODO: Populate row, column

                if (c == null || (c.DataType == null && c.CellValue==null))
                    return null;
                if (c.DataType == null)
                {
                    //treat as text
                    ret.sVal = c.CellValue.Text;
                    ret.Type = DataType.Text;
                }
                else
                {
                    switch (c.DataType.Value)
                    {
                        case CellValues.Boolean:
                        case CellValues.Date:
                        case CellValues.Error:
                        case CellValues.InlineString:
                        case CellValues.Number:
                        default:
                            throw new NotImplementedException();
                        case CellValues.String:
                            ret.sVal = c.CellValue.Text;
                            ret.Type = DataType.Text;
                            break;
                        case CellValues.SharedString:
                            int idx = 0;
                            if (sst == null) return null;
                            if (!Int32.TryParse(c.CellValue.Text, out idx))
                                throw new Exception();
                            SharedStringItem i = (SharedStringItem)sst.ElementAt(idx);
                            ret.sVal = i.Text.Text;
                            ret.Type = DataType.Text;
                            break;
                    }
                }
                return ret;
            }

            internal static DocumentFormat.OpenXml.Spreadsheet.Cell CreateCell(WorkbookPart wp, Row r, Column c, string text)
            {
                //Get ssp
                SharedStringTablePart shareStringPart;
                SharedStringTable sst;
                if (wp.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = wp.GetPartsOfType<SharedStringTablePart>().First();
                    if (shareStringPart.SharedStringTable == null)
                    {
                        sst = new SharedStringTable();
                        shareStringPart.SharedStringTable = sst;
                    }
                    else sst = shareStringPart.SharedStringTable;
                }
                else
                {
                    shareStringPart = wp.AddNewPart<SharedStringTablePart>();
                    sst = new SharedStringTable();
                    shareStringPart.SharedStringTable = sst;
                }
                //look for or insert text
                int ssiIdx = 0;
                bool found = false;
                foreach (SharedStringItem ssi in sst.Elements<SharedStringItem>())
                {
                    if (ssi.InnerText == text)
                    {
                        found = true;
                        break;
                    }
                    ssiIdx++;
                }
                if (!found)
                {
                    sst.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
                    
                }
                //Create cell
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cell.CellValue = new CellValue(ssiIdx.ToString());
                cell.DataType = CellValues.SharedString;

                cell.CellReference = c.ExcelIdx + (r==null?"1":r.RowIdx.ToString());

                return cell;
            }
            internal static DocumentFormat.OpenXml.Spreadsheet.Cell CreateCell(WorkbookPart wp, Row r, Column c, bool b)
            {
                //Create cell
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cell.CellValue = new CellValue(DocumentFormat.OpenXml.BooleanValue.FromBoolean(b));
                cell.DataType = CellValues.Boolean;

                cell.CellReference = c.ExcelIdx + (r == null ? "1" : r.RowIdx.ToString());

                return cell;
            }
            internal static DocumentFormat.OpenXml.Spreadsheet.Cell CreateCell(WorkbookPart wp, Row r, Column c, int i)
            {
                //Create cell
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cell.CellValue = new CellValue(DocumentFormat.OpenXml.IntegerValue.FromInt64(i));
                cell.DataType = CellValues.Number;

                cell.CellReference = c.ExcelIdx + (r == null ? "1" : r.RowIdx.ToString());

                return cell;
            
            }
            internal static DocumentFormat.OpenXml.Spreadsheet.Cell CreateEmptyCell(WorkbookPart wp, Row r, Column c)
            {
                //Create cell
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cell.InlineString = new InlineString("");
                cell.DataType = CellValues.InlineString;

                cell.CellReference = c.ExcelIdx + (r == null ? "1" : r.RowIdx.ToString());

                return cell;

            }
            internal DocumentFormat.OpenXml.Spreadsheet.Cell CreateCell(WorkbookPart wp)
            {
                switch (Type)
                {
                    case DataType.Int: return Cell.CreateCell(wp,row,column, iVal);
                    case DataType.Text: return Cell.CreateCell(wp, row, column, sVal);
                    case DataType.Empty: return Cell.CreateEmptyCell(wp, row, column);
                    case DataType.Bool: return Cell.CreateCell(wp, row, column, bVal);
                    default:
                        return null;
                }
            }

        }
        public enum DataType { Text, Int, Empty, Bool }

        public List<Sheet> Sheets = new List<Sheet>();


        public bool SaveAs(string fname)
        {
            try
            {
                using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Create(fname, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
                {
                    // create the workbook
                    spreadSheet.AddWorkbookPart();
                    spreadSheet.WorkbookPart.Workbook = new Workbook();     // create the worksheet
                    spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();

                    foreach (Sheet s in Sheets)
                    {
                        CreateSheet(s, spreadSheet);
                    }
                    spreadSheet.Close();
                }
            }
            catch (Exception e) { 
                return false; 
            }
            return true;
        }
        bool CreateSheet(Sheet s, SpreadsheetDocument spreadSheet){
            Worksheet wks = new Worksheet();
            spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet = wks;

            // create sheet data
            SheetData sd = new SheetData();
            wks.AppendChild(sd);

            /////// Heading
            DocumentFormat.OpenXml.Spreadsheet.Row r = new DocumentFormat.OpenXml.Spreadsheet.Row();
            sd.AppendChild(r);
            List<int> colIdxs = s.ColumnByIndex.Keys.ToList();
            colIdxs.Sort();
            foreach (int idx in colIdxs)
            {
                Column c = s.ColumnByIndex[idx];
                DocumentFormat.OpenXml.Spreadsheet.Cell cell = Cell.CreateCell(spreadSheet.WorkbookPart, null, c, (c.Name==""||c.Name==null?c.Idx.ToString():c.Name));
                r.AppendChild(cell);   
            }

            //Data
            foreach (Row dataRow in s.Rows)
            {
                DocumentFormat.OpenXml.Spreadsheet.Row r2 = new DocumentFormat.OpenXml.Spreadsheet.Row();
                sd.AppendChild(r2);
                
                List<int> colIdxs2 = s.ColumnByIndex.Keys.ToList();
                colIdxs2.Sort();
                foreach (int idx in colIdxs)
                {
                    Column c = s.ColumnByIndex[idx];
                    Cell myCell = dataRow.Cells[c];
                    DocumentFormat.OpenXml.Spreadsheet.Cell cell = myCell.CreateCell(spreadSheet.WorkbookPart);
                    r2.AppendChild(cell);
                }
            }

            
            // create the worksheet to workbook relation
            if (spreadSheet.WorkbookPart.Workbook.Sheets == null)
                spreadSheet.WorkbookPart.Workbook.Sheets = new Sheets();
            int shtIdx = spreadSheet.WorkbookPart.Workbook.Sheets.Count() + 1;
            DocumentFormat.OpenXml.Spreadsheet.Sheet ss = new DocumentFormat.OpenXml.Spreadsheet.Sheet()
            {
                Id = spreadSheet.WorkbookPart.GetIdOfPart(spreadSheet.WorkbookPart.WorksheetParts.First()),
                SheetId = DocumentFormat.OpenXml.UInt32Value.FromUInt32((UInt32)shtIdx),
                Name = (s.Name==null || s.Name=="")?"Sheet"+shtIdx.ToString():s.Name
            };
            spreadSheet.WorkbookPart.Workbook.Sheets.AppendChild(ss);

            // save worksheet
            if (spreadSheet.WorkbookPart != null && 
                spreadSheet.WorkbookPart.SharedStringTablePart != null && spreadSheet.WorkbookPart.SharedStringTablePart.SharedStringTable != null) 
                    spreadSheet.WorkbookPart.SharedStringTablePart.SharedStringTable.Save();
            wks.Save();
            spreadSheet.WorkbookPart.Workbook.Save();
            

            return true;
        }





        public static ExcelParser FromFile(string fname, string sheetName="", bool firstLineIsTitle=false, bool IgnoreEmptyRows=false)
        {
            ExcelParser ret = new ExcelParser();


            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fname, false))
            {
                WorkbookPart workbookPart = doc.WorkbookPart;
                SharedStringTable sst = workbookPart.SharedStringTablePart.SharedStringTable;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                //TODO: Handle other sheets

                Sheet s = new Sheet();

                bool firstRow = firstLineIsTitle;
                foreach (DocumentFormat.OpenXml.Spreadsheet.Row r in sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>())
                {
                    Row row = new Row();
                    int cellIdx = 0;
                    foreach(DocumentFormat.OpenXml.Spreadsheet.Cell c in r.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>() ){
                        Cell cc = Cell.CellFromSheet(c, sst);
                        if (cc == null) { cellIdx++; continue; } 
                        
                        if (firstRow)
                        {
                            Column col = new Column();
                            col.Idx = cellIdx;
                            col.Name = cc.Text;
                            s.ColumnByIndex.Add(cellIdx, col);
                            s.ColumnByName.Add(col.Name, col);
                        }
                        else
                        {
                            // !firstRow
                            if (!s.ColumnByIndex.ContainsKey(cellIdx))
                            {
                                Column col = new Column();
                                col.Idx = cellIdx;
                                s.ColumnByIndex.Add(cellIdx, col);
                            }
                            row.Cells.Add(s.ColumnByIndex[cellIdx], cc);
                        }
                        cellIdx++;
                    }
                    if (!firstRow)
                    {
                        if (!IgnoreEmptyRows || row.Cells.Count > 0)
                        {
                            s.Rows.Add(row);
                            row.Parent = s;
                        }
                    }
                    else firstRow = false;
                }
                ret.Sheets.Add(s);

            }

            return ret;

        }





        public Sheet NewSheet(string name)
        {
            Sheet s = new Sheet() { Name = name };
            Sheets.Add(s);
            return s;
        }



    }
}
