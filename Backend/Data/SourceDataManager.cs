using Microsoft.VisualBasic.FileIO;
namespace reader
{
    class ReadCsv
    {
        string location;
        public ReadCsv(string location)
        {
            this.location = location;
            using (TextFieldParser parser = new TextFieldParser(location))
            {
                parser.SetDelimiters(";");
                parser.HasFieldsEnclosedInQuotes = true;
                while (!parser.EndOfData)
                {

                    string[] fields = parser.ReadFields();
                    if(fields!=null)
                    {
                        Console.WriteLine(string.Join(" | ", fields));
                    }
                    else
                    {
                        Console.WriteLine("error | read failed >> no fields found");
                    }
                }
            }
        }
    }
}
