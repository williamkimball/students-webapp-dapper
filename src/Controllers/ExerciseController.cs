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

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string sql = $@"
            SELECT
                e.Id,
                e.Name,
                e.Language
            FROM Exercise e
            WHERE e.Id = {id}";

            using (IDbConnection conn = Connection)
            {
                Exercise exercise = await conn.QuerySingleAsync<Exercise>(sql);

                if (exercise == null) {
                    return NotFound();
                }

                return View(exercise);
            }
        }


        public IActionResult Create () {
            return View ();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ([Bind ("ExerciseId, Name, Language")] Exercise exercise) {
            if (ModelState.IsValid) {
                string sql = $@"
                    INSERT INTO Exercise
                        ( Id, Name, Language )
                        VALUES
                        ( null, '{exercise.Name}', '{exercise.Language}' )
                    ";

                using (IDbConnection conn = Connection) {
                    int rowsAffected = await conn.ExecuteAsync (sql);

                    if (rowsAffected > 0) {
                        return RedirectToAction (nameof (Index));
                    }
                }
            }

            return View (exercise);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (
            [FromRoute] int id,
            [Bind ("Id,Name,Language")] Exercise exercise
        ) {
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
                return new StatusCodeResult (StatusCodes.Status406NotAcceptable);
            }
        }

        public async Task<IActionResult> DeleteConfirm (int? id) {
            if (id == null) {
                return NotFound ();
            }

            string sql = $@"
                SELECT
                    e.Id,
                    e.Name,
                    e.Language
                FROM Exercise e
                WHERE e.Id = {id}";

            using (IDbConnection conn = Connection) {
                Exercise exercise = await conn.QueryFirstAsync<Exercise> (sql);

                if (exercise == null) return NotFound ();

                return View (exercise);
            }
        }

        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id) {

            string sql = $@"DELETE FROM Exercise WHERE Id = {id}";

            using (IDbConnection conn = Connection) {
                int rowsAffected = await conn.ExecuteAsync (sql);
                if (rowsAffected > 0) {
                    return RedirectToAction (nameof (Index));
                }
                throw new Exception ("No rows affected");
            }
        }
    }
}