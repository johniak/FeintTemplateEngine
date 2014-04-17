using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FeintTemplateEngine;

namespace TemplateEngineUnitTest
{
    [TestClass]
    public class TagUnitTest
    {
        [TestMethod]
        public void StandardTag()
        {
            String source = "tag1\n\ttag2";
            String expectation = "<tag1>\n\t<tag2/>\n</tag1>\n";
            TemplateEngine engine = new TemplateEngine(source, new { });
            String output = engine.Render();
            Assert.AreEqual(expectation, output);
        }
        [TestMethod]
        public void StandardTagWithTextInSameLine()
        {
            String source = "tag1 sample text";
            String expectation = "<tag1>\n\tsample text\n</tag1>\n";
            TemplateEngine engine = new TemplateEngine(source, new { });
            String output = engine.Render();
            Assert.AreEqual(expectation, output);
        }
        [TestMethod]
        public void StandardTagWithTextBlock()
        {
            String source = "tag1.\n\tsample text";
            String expectation = "<tag1>\n\tsample text\n</tag1>\n";
            TemplateEngine engine = new TemplateEngine(source, new { });
            String output = engine.Render();
            Assert.AreEqual(expectation, output);
        }
        [TestMethod]
        public void StandardTagWithAttribute()
        {
            String source = "tag1(foo=\"bar\", bar=\"foo\")";
            String expectation = "<tag1 foo=\"bar\" bar=\"foo\"/>\n";
            TemplateEngine engine = new TemplateEngine(source, new { });
            String output = engine.Render();
            Assert.AreEqual(expectation, output);
        }
        [TestMethod]
        public void DivTagWithId()
        {
            String source = "#car1";
            String expectation = "<div id=\"car1\"/>\n";
            TemplateEngine engine = new TemplateEngine(source, new { });
            String output = engine.Render();
            Assert.AreEqual(expectation, output);
        }
        [TestMethod]
        public void DivTagWithClass()
        {
            String source = ".car";
            String expectation = "<div class=\"car\"/>\n";
            TemplateEngine engine = new TemplateEngine(source, new { });
            String output = engine.Render();
            Assert.AreEqual(expectation, output);
        }
        [TestMethod]
        public void DivTagWithClassIdAndOtherAttributes()
        {
            String source = ".car#car1(color=\"red\")";
            String expectation = "<div id=\"car1\" class=\"car\" color=\"red\"/>\n";
            TemplateEngine engine = new TemplateEngine(source, new { });
            String output = engine.Render();
            Assert.AreEqual(expectation, output);
        }

    }
}
