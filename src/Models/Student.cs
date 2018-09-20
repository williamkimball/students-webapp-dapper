using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Workforce.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SlackHandle { get; set; }
        public int CohortId { get; set; }
        public Cohort Cohort { get; set; }

        [Display(Name="Student Name")]
        public string FullName {
            get {
                return $"{FirstName} {LastName}";
            }
        }
        public List<Exercise> AssignedExercises { get; set; } = new List<Exercise>();
    }

}