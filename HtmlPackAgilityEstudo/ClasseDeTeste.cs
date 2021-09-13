using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HtmlPackAgilityEstudo
{
    // corrija o erro com o instalar  TestTools.UnitTesting;
    [TestClass]
    public class ClasseDeTeste
    {
        [TestMethod]
        public void TestMethod1()
        {
            Robo teste = new Robo();
            teste.GetImgCaptcha();
        }
    }
}
