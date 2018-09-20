using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Workforce.Models
{
    public class Exercise
    {
        public int Id { get; set; }

        [Display(Name="Exercise Name")]

        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        public string Language { get; set; }
        public List<Student> AssignedStudents { get; set; } = new List<Student>();
    }
}