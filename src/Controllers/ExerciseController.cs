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


namespace Workforce.Controllers {
    public class ExerciseController : Controller {
        private readonly IConfiguration _config;

        public ExerciseController (IConfiguration config) {
            _config = config;
        }

        public IDbConnection Connection {
            get {
                return new SqliteConnection (_config.GetConnectionString ("DefaultConnection"));
            }
        }

        public async Task<IActionResult> Index () {
            using (IDbConnection conn = Connection) {
                IEnumerable<Exercise> exercises = await conn.QueryAsync<Exercise> (
                    "select id, name, language from exercise;"
                );
                return View (exercises);
            }
        }

        public async Task<IActionResult> Edit (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $"SELECT Id, Name, Language FROM Exercise WHERE Id = {id}";

            using (IDbConnection conn = Connection) {
                Exercise exercise = (await conn.QueryAsync<Exercise> (sql)).Single ();
                return View (exercise);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit ([FromRoute] int id, [Bind ("Id,Name,Language")] Exercise exercise) {
            if (id != exercise.Id) {
                return NotFound ();
            }

            if (ModelState.IsValid) {
                string sql = $@"
                    UPDATE Exercise
                    SET Name = '{exercise.Name}',
                        Language = '{exercise.Language}'
                    WHERE Id = {id}";

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql);
                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                    throw new Exception ("No rows affected");
                }

            } else {
                return new StatusCodeResult(StatusCodes.Status406NotAcceptable);
            }

        }
    }
}
