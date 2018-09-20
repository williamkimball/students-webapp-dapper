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
                c.Id,
                c.Name
            from Student s
            join Cohort c on s.CohortId = c.Id
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
                        }
                    );
                return View (students.Values);

            }
        }

        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
            select
                s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle
            from Student s
            WHERE s.Id = {id}";

            using (IDbConnection conn = Connection) {

                Student student = (await conn.QueryAsync<Student> (sql)).ToList ().Single ();

                if (student == null) {
                    return NotFound ();
                }

                return View (student);
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
                s.CohortId,
                c.Id,
                c.Name
            from Student s
            join Cohort c on s.CohortId = c.Id
            WHERE s.Id = {id}";

            using (IDbConnection conn = Connection) {
                StudentEditViewModel model = new StudentEditViewModel (conn);

                model.Student = (await conn.QueryAsync<Student, Cohort, Student> (
                    sql,
                    (student, cohort) => {
                        student.Cohort = cohort;
                        return student;
                    }
                )).Single ();

                return View (model);
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

        public async Task<IActionResult> DeleteConfirm (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
            select
                s.Id,
                s.FirstName,
                s.LastName,
                s.SlackHandle
            from Student s
            WHERE s.Id = {id}";

            using (IDbConnection conn = Connection) {

                Student student = (await conn.QueryAsync<Student> (sql)).ToList ().Single ();

                if (student == null) {
                    return NotFound ();
                }

                return View (student);
            }
        }

        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id) {

            string sql = $@"DELETE FROM Student WHERE Id = {id}";

            using (IDbConnection conn = Connection) {
                int rowsAffected = await conn.ExecuteAsync (sql);
                if (rowsAffected > 0) {
                    return RedirectToAction (nameof (Index));
                }
                throw new Exception ("No rows affected");
            }
        }

        [ResponseCache (Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error () {
            return View (new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}