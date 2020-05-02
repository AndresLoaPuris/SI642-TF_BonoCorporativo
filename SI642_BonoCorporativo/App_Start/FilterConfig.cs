using System.Web;
using System.Web.Mvc;

namespace SI642_BonoCorporativo
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
