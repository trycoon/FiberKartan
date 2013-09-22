[assembly: WebActivator.PreApplicationStartMethod(typeof(FiberKartan.App_Start.Combres), "PreStart")]
namespace FiberKartan.App_Start {
	using System.Web.Routing;
	using global::Combres;
	
    public static class Combres {
        public static void PreStart() {
            RouteTable.Routes.AddCombresRoute("Combres");
        }
    }
}