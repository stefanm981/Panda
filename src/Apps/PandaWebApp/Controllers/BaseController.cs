using PandaWebApp.Data;

namespace PandaWebApp.Controllers
{
    using SIS.MvcFramework;

    public class BaseController : Controller
    {
        public BaseController()
        {
            this.Db = new PandaDbContext();
        }

        protected PandaDbContext Db { get; }
    }
}
