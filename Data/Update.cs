using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemachApp.Data
{
    [Table("Updates")] // This tells EF to use the existing 'Updates' table
    public class UpdateLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecordId { get; set; }         

        public string TableName { get; set; }        // e.g., "Clients" or "Agents"
        
        public string ObjectId { get; set; }      // PrimaryKey of this object
        public string ColumName { get; set; }       // e.g., "Phone", "FullName"

        public string PrevVersion { get; set; }      // previous field value
        public string UpdatedVersion { get; set; }      // updated field value
        public string Agent {  get; set; }            // who made the change
        public DateTime Timestamp { get; set; } = DateTime.Now;       // when the change was made

   
    }
}
