using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workforce.Models;

namespace Workforce.Models.ViewModels
{
    public class StudentEditViewModel
    {
        public Student Student { get; set; }

        [Display(Name="Current Cohort")]
        public List<SelectListItem> Cohorts { get; private set; }

        public StudentEditViewModel() {}
        public StudentEditViewModel(IDbConnection conn)
        {
            string sql = $@"SELECT CohortId, CohortName FROM Cohort";

            using (conn) {
                List<Cohort> cohorts = (conn.Query<Cohort> (sql)).ToList();

                this.Cohorts = cohorts
                    .Select(li => new SelectListItem {
                        Text = li.CohortName,
                        Value = li.CohortId.ToString()
                    }).ToList();
            }


            // Add a prompt so that the <select> element isn't blank
            this.Cohorts.Insert(0, new SelectListItem {
                Text = "Choose cohort...",
                Value = "0"
            });
        }
    }
}
