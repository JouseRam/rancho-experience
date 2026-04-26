using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using Microsoft.Owin;

[assembly: AssemblyTitle("RanchoMvc")]
[assembly: AssemblyDescription("Rancho El Pato - Experiencias para familias")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: ComVisible(false)]
[assembly: OwinStartup(typeof(RanchoMvc.Startup))]
