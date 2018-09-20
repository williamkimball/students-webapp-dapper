using System.Collections.Generic;

namespace Workforce.Models
{
    public class Cohort
    {
        public int CohortId { get; set; }
        public string CohortName { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Instructor> Instructors { get; set; } = new List<Instructor>();
    }

}