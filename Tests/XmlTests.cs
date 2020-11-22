using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateChartsDotNet.CoreEngine;
using StateChartsDotNet.CoreEngine.ModelProvider.Xml.States;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tests
{
    [TestClass]
    public class XmlTests
    {
        private static ILogger _logger;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var loggerFactory = LoggerFactory.Create(
                                    builder => builder.AddFilter("XmlTests", level => true).AddDebug());

            _logger = loggerFactory.CreateLogger("XmlTests");
        }

        [TestMethod]
        public async Task Foreach()
        {
            var xmldoc = @"<?xml version='1.0'?>
                           <scxml xmlns='http://www.w3.org/2005/07/scxml'
                                  version='1.0'
                                  datamodel='csharp'
                                  initial='loop'>
                               <datamodel>
                                   <data id='items' expr='new [] { 1, 2, 3, 4, 5 }' />
                                   <data id='sum' expr='0' />
                               </datamodel>
                               <state id='loop'>
                                   <onentry>
                                       <foreach array='items' item='arrayItem'>
                                           <assign location='sum' expr='sum + arrayItem' />
                                           <log expr='&quot;item = &quot; + arrayItem' />
                                       </foreach>
                                   </onentry>
                                   <transition cond='sum &gt;= 15' target='done' />
                               </state>
                               <final id='done'>
                                   <onentry>
                                       <log expr='&quot;sum = &quot; + sum' />
                                   </onentry>
                               </final>
                           </scxml>";

            var machine = new StateChart(XDocument.Parse(xmldoc));

            var context = new ExecutionContext(machine, _logger);

            var interpreter = new Interpreter();

            await interpreter.Run(context);

            Assert.AreEqual(15, context["sum"]);
        }

        [TestMethod]
        public async Task Microwave()
        {
            var xmldoc = @"<?xml version='1.0'?>
                           <scxml xmlns='http://www.w3.org/2005/07/scxml'
                                  version='1.0'
                                  datamodel='csharp'
                                  initial='off'>

                               <!--  trivial 5 second microwave oven example -->
                               <datamodel>
                                   <data id='cook_time' expr='5'/>
                                   <data id='door_closed' expr='true'/>
                                   <data id='timer' expr='0'/>
                               </datamodel>

                               <state id='off'>
                                   <transition event='turn.on' target='on'/>
                               </state>

                               <state id='on'>
                                   <initial>
                                       <transition target='idle'/>
                                   </initial>

                                   <transition event='turn.off' target='off'/>
                                   <transition cond='timer &gt;= cook_time' target='off'/>

                                   <state id='idle'>
                                       <transition cond='door_closed' target='cooking'/>
                                       <transition event='door.close' target='cooking'>
                                           <assign location='door_closed' expr='true'/>
                                       </transition>
                                   </state>

                                   <state id='cooking'>
                                       <transition event='door.open' target='idle'>
                                           <assign location='door_closed' expr='false'/>
                                       </transition>
                                       <transition event='time'>
                                           <assign location='timer' expr='timer + 1'/>
                                       </transition>
                                   </state>

                               </state>
                           </scxml>";

            var machine = new StateChart(XDocument.Parse(xmldoc));

            var context = new ExecutionContext(machine, _logger);

            var interpreter = new Interpreter();

            var task = interpreter.Run(context);

            await Task.Delay(1000);

            context.Send("turn.on");

            for (var i = 0; i < 5; i++)
            {
                context.Send("time");
                await Task.Delay(200);
            }

            context.Send("cancel");

            await task;

            Assert.AreEqual(5, context["timer"]);
        }
    }
}
