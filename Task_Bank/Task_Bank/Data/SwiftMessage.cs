//using static System.Net.Mime.MediaTypeNames;

namespace Task_Bank.Data
{
    public class SwiftMessage
    {
        public int Id { get; set; }
        public string Field_1 { get; set; }
        public string Field_2 { get; set; }
        public string Field_20 {get; set;}
        public string Field_21 {  get; set; } 
        public string Field_79 { get; set;} 
        public string Mac {  get; set;} 
        public string Chk {  get; set;} 
    }
}
