using GomokuWebAPI.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GomokuWebAPI.Controllers
{
    [Route("admin/db-migrate")]
    public class DataMigratorController : Controller
    {
        private readonly AppDbContext _db;

        public DataMigratorController(AppDbContext db)
        {
            _db = db;
        }

        [AllowAnonymous]
        [HttpGet("migrate")]
        public IActionResult Migrate()
        {
            try
            {
                _db.Database.Migrate();
            }
            catch (Exception e)
            {
                return Json(e.Message);
            }
            return Json("migration-ok");
        }
    }
}
