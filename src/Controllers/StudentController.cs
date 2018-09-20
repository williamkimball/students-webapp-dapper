using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Workforce.Models;
using Workforce.Models.ViewModels;

namespace Workforce.Controllers {
    public class StudentController : Controller {
        private readonly IConfiguration _config;

        public StudentController (IConfiguration config) {
            _config = config;
        }

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        public async Task<IActionResult> Index () {

            string sql = @"
                select
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.SlackHandle,
                    c.CohortId,
                    c.CohortName
                from Student s
                join Cohort c on s.CohortId = c.CohortId
            ";

            using (IDbConnection conn = Connection) {
                Dictionary<int, Student> students = new Dictionary<int, Student> ();

                var studentQuerySet = await conn.QueryAsync<Student, Cohort, Student> (
                        sql,
                        (student, cohort) => {
                            if (!students.ContainsKey (student.Id)) {
                                students[student.Id] = student;
                            }
                            students[student.Id].Cohort = cohort;
                            return student;
                        },
                        splitOn:"CohortId"
                    );
                return View(students.Values);

            }
        }

        public async Task<IActionResult> Edit (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
                select
                    s.Id,
                    s.FirstName,
                    s.LastName,
                    s.SlackHandle,
                    c.CohortId,
                    c.CohortName
                from Student s
                join Cohort c on s.CohortId = c.CohortId
                WHERE s.Id = {id}";


            using (IDbConnection conn = Connection) {
                StudentEditViewModel model = new StudentEditViewModel(conn);

                model.Student = (await conn.QueryAsync<Student, Cohort, Student> (
                    sql,
                    (s, c) => {
                        s.Cohort = c;
                        return s;
                    },
                    splitOn: "CohortId"
                )).Single();

                return View(model);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, StudentEditViewModel model) {
            if (id != model.Student.Id) {
                return NotFound ();
            }

            if (ModelState.IsValid) {
                string sql = $@"
                    UPDATE Student
                    SET FirstName = '{model.Student.FirstName}',
                        LastName = '{model.Student.LastName}',
                        SlackHandle = '{model.Student.SlackHandle}',
                        CohortId = {model.Student.CohortId}
                    WHERE Id = {id}";

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql);
                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                    throw new Exception ("No rows affected");
                }
            } else {
                return new StatusCodeResult (StatusCodes.Status406NotAcceptable);
            }

        }

        public IActionResult Contact () {
            ViewData["Message"] = "Your contact page.";

            return View ();
        }

        public IActionResult Privacy () {
            return View ();
        }

        [ResponseCache (Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error () {
            return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}