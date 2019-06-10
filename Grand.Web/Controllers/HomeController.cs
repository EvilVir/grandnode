using Grand.Framework.UI;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        private readonly IPageHeadBuilder _pageHeadBuilder;

        public HomeController(IPageHeadBuilder pageHeadBuilder)
        {
            this._pageHeadBuilder = pageHeadBuilder;
        }

        public virtual IActionResult Index()
        {
            _pageHeadBuilder.AddPageCssClassParts($"page-home");
            return View();
        }
    }
}
