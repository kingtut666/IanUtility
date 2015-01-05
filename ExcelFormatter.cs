using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IanUtility
{
    public class ExcelFormatter : IFormatter
    {
        Dictionary<System.IO.Stream, ExcelParser> parsers = new Dictionary<System.IO.Stream, ExcelParser>();
        Dictionary<System.IO.Stream, ExcelParser.Sheet> curSheet = new Dictionary<System.IO.Stream, ExcelParser.Sheet>();
        Dictionary<System.IO.Stream, ExcelParser.Row> curRow = new Dictionary<System.IO.Stream, ExcelParser.Row>();


        public SerializationBinder Binder
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public StreamingContext Context
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public object Deserialize(System.IO.Stream serializationStream)
        {
            throw new NotImplementedException();
        }

        public void Serialize(System.IO.Stream serializationStream, object graph)
        {
            Type t = TypeDescriptor.GetReflectionType(graph);
            if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type i = t.GetGenericArguments().Single();
                    ExcelParser p = new ExcelParser();
                    ExcelParser.Sheet s = p.NewSheet("Sheet1");
                    bool firstRow = true;
                    foreach (object o in (System.Collections.IEnumerable)graph)
                    {
                        if (firstRow)
                        {
                            //just create columns
                            //TODO: This should be dynamic - what about dictionaries and lists....
                            _Serialize(s, null, o, true);
                            firstRow = false;
                        }
                        ExcelParser.Row r = s.CreateNewRow();
                        _Serialize(s, r, o, false);
                    }
                    p.SaveAs(serializationStream);
                    return;
                }
                
            }
            throw new NotImplementedException("Only type List<> are supported");
        }
        void _Serialize(ExcelParser.Sheet sht, ExcelParser.Row r, object graph, bool firstRow){
            Type t = TypeDescriptor.GetReflectionType(graph);
            if (t.IsGenericType)
            {
                return; //TODO: until implemented
                if (t.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type i = t.GetGenericArguments().Single();
                    foreach (object o in (System.Collections.IEnumerable)graph)
                        _Serialize(sht, r, o, firstRow);
                }
                else throw new NotImplementedException();
            }
            else
            {
                foreach (FieldInfo f in t.GetFields())
                {
                    if (f.IsNotSerialized) continue;
                    if (f.FieldType == typeof(string)) Write(sht, r, (string)f.GetValue(graph), f.Name, firstRow);
                    else if (f.FieldType == typeof(int)) Write(sht, r, (int)f.GetValue(graph), f.Name, firstRow);
                    else if (f.FieldType.IsEnum) WriteEnum(sht, r, f.GetValue(graph), f.Name, firstRow);
                    else if (f.FieldType.IsGenericType)
                    {
                        if (f.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                            _Serialize(sht, r, f.GetValue(graph), firstRow);
                        else if (f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                            _Serialize(sht, r, f.GetValue(graph), firstRow);
                        else
                            throw new NotImplementedException("Field " + f.Name + " cannot be serialized. Type " + f.FieldType.Name + " not supported.");

                    }
                    else
                        throw new NotImplementedException("Field " + f.Name + " cannot be serialized. Type " + f.FieldType.Name + " not supported.");
                } 

            }

        }

        void Write(ExcelParser.Sheet sht, ExcelParser.Row r, string s, string name, bool firstRow)
        {
            if (firstRow)
            {
                sht.CreateNewColumn(name);
            }
            else r[name].Text = s;
        }
        void Write(ExcelParser.Sheet sht, ExcelParser.Row r, int i, string name, bool firstRow)
        {
            if (firstRow)
            {
                sht.CreateNewColumn(name);
            }
            else r[name].Int = i;
        }
        void WriteEnum(ExcelParser.Sheet sht, ExcelParser.Row r, object en, string name, bool firstRow)
        {
            //TODO: Currently serialize as string, could want numeric
            if (firstRow)
            {
                sht.CreateNewColumn(name);
                return;
            }
            Enum.Format(en.GetType(), en, "g");
        }



        public ISurrogateSelector SurrogateSelector
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
