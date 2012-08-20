using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace MetaWebLogSite.Controllers
{
	public class XmlRpcController : Controller
	{
		public ActionResult Rsd()
		{
            var ms = new MemoryStream();
            var xmlWriter = XmlWriter.Create(ms);

            var root = (HttpContext.Request.IsSecureConnection ? "https://" : "http://") + HttpContext.Request.Url.Authority;
            var api = root + VirtualPathUtility.ToAbsolute("~/blogapi");

            xmlWriter.WriteStartDocument();
            {
                xmlWriter.WriteStartElement("rsd", "http://archipelago.phrasewise.com/rsd");
                xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                xmlWriter.WriteAttributeString("version", "1.0");
                {
                    xmlWriter.WriteStartElement("service");
                    {
                        xmlWriter.WriteStartElement("engineName");
                        xmlWriter.WriteString("Markpad Example MetaWebLog Engine");
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("homePageLink");
                        xmlWriter.WriteString(Url.Action("Index", "Home"));
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("apis");
                        {
                            xmlWriter.WriteStartElement("api");
                            xmlWriter.WriteAttributeString("name", "MetaWeblog");
                            xmlWriter.WriteAttributeString("preferred", "true");
                            xmlWriter.WriteAttributeString("apiLink", api);
                            xmlWriter.WriteAttributeString("blogID", "1");
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();

                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndDocument();

            xmlWriter.Flush();
            ms.Position = 0;

            return new FileStreamResult(ms, "text/xml");
		}
	}
}
